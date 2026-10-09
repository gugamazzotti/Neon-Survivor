# Calls the DreamLayer Agent API (https://docs.dreamlayer.io/) and saves PNGs for Neon Survivor.
# The API key is read from secrets.env, which is gitignored. This script never prints the key.
param(
    [ValidateSet("balance", "generate")]
    [string]$Command = "balance",
    [string]$Name = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Api = "https://api.dreamlayer.io"
$OutDir = Join-Path (Resolve-Path (Join-Path $Root "..\..")).Path "Assets\Art\Generated"
$JobDir = Join-Path $Root "jobs"

function Read-ApiKey {
    $path = Join-Path $Root "secrets.env"
    if (-not (Test-Path $path)) {
        throw "Missing Tools/DreamLayer/secrets.env. Copy secrets.env.example and put the API key there."
    }
    foreach ($line in Get-Content $path) {
        if ($line -match '^\s*DREAMLAYER_API_KEY\s*=\s*(.+)\s*$') {
            return $Matches[1].Trim().Trim('"').Trim("'")
        }
    }
    throw "DREAMLAYER_API_KEY was not found in secrets.env."
}

function Invoke-DreamLayer {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Body,
        [string]$IdempotencyKey,
        [string]$Accept = "application/json"
    )
    $key = Read-ApiKey
    $headerFile = New-TemporaryFile
    $bodyFile = $null
    try {
        @(
            "Authorization: Bearer $key"
            "DreamLayer-Version: 1"
            "Accept: $Accept"
        ) | Set-Content -Path $headerFile -Encoding ascii
        $curlArgs = @("-sS", "-X", $Method, "$Api$Path", "-D", "-", "-H", "@$headerFile")
        if ($IdempotencyKey) {
            $curlArgs += @("-H", "Idempotency-Key: $IdempotencyKey")
        }
        if ($Body) {
            $bodyFile = New-TemporaryFile
            [System.IO.File]::WriteAllText($bodyFile, $Body)
            $curlArgs += @("-H", "Content-Type: application/json", "--data-binary", "@$bodyFile")
        }
        $rawLines = & curl.exe @curlArgs
        if ($LASTEXITCODE -ne 0) {
            throw "DreamLayer request failed before a response ($Method $Path)."
        }
        $raw = if ($rawLines -is [System.Array]) { $rawLines -join "`n" } else { [string]$rawLines }
        $split = $raw -split "`r?`n`r?`n", 2
        $headerText = $split[0]
        $payload = if ($split.Length -gt 1) { $split[1] } else { "" }
        $status = 0
        foreach ($line in ($headerText -split "`r?`n")) {
            if ($line -match '^HTTP/\S+\s+(\d+)') { $status = [int]$Matches[1] }
        }
        return @{ Status = $status; Body = $payload.Trim(); Headers = $headerText }
    }
    finally {
        Remove-Item $headerFile -Force -ErrorAction SilentlyContinue
        if ($bodyFile) { Remove-Item $bodyFile -Force -ErrorAction SilentlyContinue }
    }
}

function Convert-FromPayload {
    param([string]$Payload)
    if ([string]::IsNullOrWhiteSpace($Payload)) { return $null }
    return $Payload | ConvertFrom-Json
}

function Get-AvailableCredits {
    $response = Invoke-DreamLayer -Method GET -Path "/v1/balance"
    if ($response.Status -ge 400) {
        throw "Balance check returned HTTP $($response.Status): $($response.Body)"
    }
    $json = Convert-FromPayload $response.Body
    $value = $null
    foreach ($prop in @("available", "balance", "credits")) {
        if ($null -ne $json.$prop) { $value = [double]$json.$prop; break }
    }
    if ($null -eq $value) {
        throw "Could not read credits from balance: $($response.Body)"
    }
    return @{ Credits = $value; Raw = $response.Body }
}

function Get-ExecutionId {
    param($Json)
    foreach ($prop in @("execution_id", "id")) {
        if ($Json.$prop) { return [string]$Json.$prop }
    }
    if ($Json.execution.id) { return [string]$Json.execution.id }
    return $null
}

function Get-DownloadUrl {
    param($Json)
    if ($Json.asset.download_url) { return [string]$Json.asset.download_url }
    if ($Json.output.download_url) { return [string]$Json.output.download_url }
    if ($Json.download_url) { return [string]$Json.download_url }
    if ($Json.image_job.finished_assets) {
        $finished = @($Json.image_job.finished_assets)
        if ($finished.Count -gt 0 -and $finished[0].download_url) {
            return [string]$finished[0].download_url
        }
    }
    if ($Json.image_job.public_job_id) {
        return "$Api/v1/image-jobs/$($Json.image_job.public_job_id)/asset"
    }
    if ($Json.assets) {
        $assets = @($Json.assets)
        if ($assets.Count -gt 0 -and $assets[0].download_url) {
            return [string]$assets[0].download_url
        }
    }
    return $null
}

function Get-Sha256Hex {
    param([string]$Text)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return ([BitConverter]::ToString($hash) -replace "-", "").ToLowerInvariant().Substring(0, 16)
}

function Save-Job {
    param($Job, [string]$Path)
    $Job | ConvertTo-Json -Depth 8 | Set-Content -Path $Path -Encoding utf8
}

function Wait-Execution {
    param([string]$ExecutionId)
    for ($i = 0; $i -lt 80; $i++) {
        Start-Sleep -Seconds 3
        $response = Invoke-DreamLayer -Method GET -Path "/v1/executions/$ExecutionId"
        if ($response.Status -ge 400) {
            throw "Execution $ExecutionId returned HTTP $($response.Status): $($response.Body)"
        }
        $json = Convert-FromPayload $response.Body
        $status = [string]$json.status
        Write-Host "  $status"
        if ($status -in @("completed", "failed", "cancelled", "needs_input")) {
            return $json
        }
    }
    throw "Execution $ExecutionId is still running. Resume later; do not submit it again."
}

function Save-Asset {
    param([string]$Url, [string]$Destination)
    $key = Read-ApiKey
    $dir = Split-Path -Parent $Destination
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
    $temp = "$Destination.download"
    & curl.exe -sS -L -f -o $temp $Url -H "Authorization: Bearer $key"
    if ($LASTEXITCODE -ne 0) {
        if (Test-Path $temp) { Remove-Item $temp -Force }
        throw "Download failed for $Destination"
    }
    Move-Item -Force $temp $Destination
}

function New-GameImage {
    param($Asset)
    $prompt = [string]$Asset.prompt
    $aspect = "1:1"
    if ($Asset.aspect_ratio) { $aspect = [string]$Asset.aspect_ratio }
    $body = '{"operation":"text_to_image","prompt":' + ($prompt | ConvertTo-Json) + ',"aspect_ratio":"' + $aspect + '","max_credits":1}'
    $idem = "neon-" + $Asset.name + "-" + (Get-Sha256Hex $body)
    $jobPath = Join-Path $JobDir ($Asset.name + ".json")
    $png = Join-Path $OutDir ($Asset.name + ".png")
    if ((Test-Path $png) -and (Test-Path $jobPath)) {
        $existing = Get-Content $jobPath -Raw | ConvertFrom-Json
        if ($existing.status -eq "completed") {
            Write-Host "$($Asset.name) already generated."
            return $false
        }
    }

    $executionId = $null
    if (Test-Path $jobPath) {
        $existing = Get-Content $jobPath -Raw | ConvertFrom-Json
        if ($existing.idempotency_key -eq $idem -and $existing.execution_id) {
            $executionId = [string]$existing.execution_id
        }
    }

    if (-not $executionId) {
        Write-Host "Submitting $($Asset.name)"
        $response = Invoke-DreamLayer -Method POST -Path "/v1/execute" -Body $body -IdempotencyKey $idem
        if ($response.Status -ge 400) {
            throw "Submit $($Asset.name) returned HTTP $($response.Status): $($response.Body)"
        }
        $accepted = Convert-FromPayload $response.Body
        $executionId = Get-ExecutionId $accepted
        if (-not $executionId) {
            throw "Submit $($Asset.name) did not return an execution id: $($response.Body)"
        }
        Save-Job @{
            name = $Asset.name
            idempotency_key = $idem
            execution_id = $executionId
            status = "submitted"
        } $jobPath
    }
    else {
        Write-Host "Resuming $($Asset.name) ($executionId)"
    }

    $done = Wait-Execution $executionId
    $url = Get-DownloadUrl $done
    $finalStatus = [string]$done.status
    Save-Job @{
        name = $Asset.name
        idempotency_key = $idem
        execution_id = $executionId
        status = $finalStatus
        download_url = $url
    } $jobPath

    if ($finalStatus -ne "completed" -or -not $url) {
        throw "$($Asset.name) finished as $finalStatus. The same request can be resumed; it was not submitted again."
    }

    Save-Asset -Url $url -Destination $png
    Write-Host "Saved $png"
    return $true
}

if (-not (Test-Path $JobDir)) { New-Item -ItemType Directory -Path $JobDir | Out-Null }

if ($Command -eq "balance") {
    $balance = Get-AvailableCredits
    Write-Output $balance.Raw
    exit 0
}

$manifestPath = Join-Path $Root "assets.json"
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
if ($Name) {
    $manifest = @($manifest | Where-Object { $_.name -eq $Name })
    if ($manifest.Count -eq 0) { throw "No asset named $Name in assets.json." }
}

$balance = Get-AvailableCredits
$credits = $balance.Credits
Write-Output "Available credits: $credits"
$spent = 0
foreach ($asset in $manifest) {
    $png = Join-Path $OutDir ($asset.name + ".png")
    $jobPath = Join-Path $JobDir ($asset.name + ".json")
    $already = (Test-Path $png) -and (Test-Path $jobPath)
    if (-not $already -and ($credits - $spent) -lt 1) {
        Write-Host "Stopping. Not enough credits for $($asset.name)."
        break
    }
    $created = New-GameImage $asset
    if ($created) { $spent += 1 }
}
Write-Output "New images submitted: $spent"
