using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonSurvivor.EditorTools
{
    public static class WebGlPagesBuild
    {
        const string RequestName = "webgl-build.request";
        const string RestoreName = "webgl-restore.request";
        const string ResultName = "webgl-build.result";

        // Starts a Pages build when Temp/webgl-build.request is present.
        [InitializeOnLoadMethod]
        static void WatchRequest()
        {
            EditorApplication.delayCall += TryBuildFromRequest;
        }

        [MenuItem("Neon Survivor/Build WebGL Pages")]
        public static void Build()
        {
            TryBuildFromRequest();
        }

        static void TryBuildFromRequest()
        {
            string requestPath = TempFile(RequestName);
            string restorePath = TempFile(RestoreName);
            if (!File.Exists(requestPath))
            {
                RestoreEditorTarget(restorePath);
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryBuildFromRequest;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += TryBuildFromRequest;
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                File.WriteAllText(restorePath, EditorUserBuildSettings.activeBuildTarget.ToString());
                Debug.Log("Neon Survivor WebGL: trocando o alvo de build para WebGL.");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                EditorApplication.delayCall += TryBuildFromRequest;
                return;
            }

            File.Delete(requestPath);
            RunBuild(restorePath);
        }

        static void RunBuild(string restorePath)
        {
            string output = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs"));
            try
            {
                ApplyWebGlSettings();
                IncludeNeonShaders();
                if (Directory.Exists(output))
                    Directory.Delete(output, true);

                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[]
                    {
                        "Assets/Scenes/MainMenu.unity",
                        "Assets/Scenes/SampleScene.unity"
                    },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });

                if (report.summary.result != BuildResult.Succeeded)
                {
                    File.WriteAllText(TempFile(ResultName), "FAIL " + report.summary.result + " errors=" + report.summary.totalErrors);
                    Debug.LogError("Neon Survivor WebGL: build falhou.");
                    return;
                }

                File.WriteAllText(Path.Combine(output, ".nojekyll"), string.Empty);
                File.WriteAllText(TempFile(ResultName), "OK bytes=" + report.summary.totalSize);
                Debug.Log("Neon Survivor WebGL: build pronta em docs.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(TempFile(ResultName), "FAIL " + exception.GetType().Name + " " + exception.Message);
                Debug.LogException(exception);
            }
            finally
            {
                RestoreEditorTarget(restorePath);
            }
        }

        // Ponto de entrada do GameCI. Grava o WebGL fora de docs/
        // para o site publicado na raiz continuar intacto.
        public static void BuildPreview()
        {
            try
            {
                string output = ReadArg("-buildOutput");
                if (string.IsNullOrEmpty(output))
                    output = Path.Combine("build", "WebGL");
                output = Path.GetFullPath(output);
                if (WouldReplaceLiveSite(output))
                    throw new InvalidOperationException("O preview não pode substituir o site publicado em docs.");

                EnsureWebGlTarget();
                ApplyWebGlSettings();
                IncludeNeonShaders();
                if (Directory.Exists(output))
                    Directory.Delete(output, true);

                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[]
                    {
                        "Assets/Scenes/MainMenu.unity",
                        "Assets/Scenes/SampleScene.unity"
                    },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("WebGL preview falhou: " + report.summary.result + " errors=" + report.summary.totalErrors);

                File.WriteAllText(Path.Combine(output, ".nojekyll"), string.Empty);
                Debug.Log("Neon Survivor WebGL preview pronta em " + output + " bytes=" + report.summary.totalSize);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
                throw;
            }
        }

        static void ApplyWebGlSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
        }

        static void EnsureWebGlTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
                return;

            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            if (!switched && EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new InvalidOperationException("Não foi possível trocar o alvo para WebGL.");
        }

        static void IncludeNeonShaders()
        {
            Shader[] required =
            {
                Shader.Find("Neon/SpriteUnlit"),
                Shader.Find("Neon/SpriteAdditive"),
                Shader.Find("Sprites/Default"),
                Shader.Find("Particles/Additive"),
                Shader.Find("UI/Default")
            };

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0)
                return;

            SerializedObject serialized = new SerializedObject(assets[0]);
            SerializedProperty array = serialized.FindProperty("m_AlwaysIncludedShaders");
            if (array == null || !array.isArray)
                return;

            bool changed = false;
            for (int i = 0; i < required.Length; i++)
            {
                Shader shader = required[i];
                if (shader == null)
                {
                    Debug.LogWarning("Neon Survivor WebGL: shader obrigatório ausente no índice " + i + ".");
                    continue;
                }

                bool found = false;
                for (int slot = 0; slot < array.arraySize; slot++)
                {
                    if (array.GetArrayElementAtIndex(slot).objectReferenceValue == shader)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    continue;

                int index = array.arraySize;
                array.InsertArrayElementAtIndex(index);
                array.GetArrayElementAtIndex(index).objectReferenceValue = shader;
                changed = true;
                Debug.Log("Neon Survivor WebGL: shader mantido no build " + shader.name);
            }

            if (changed)
                serialized.ApplyModifiedProperties();
        }

        static string ReadArg(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }

            return null;
        }

        static bool WouldReplaceLiveSite(string fullPath)
        {
            string docs = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs"));
            string preview = Path.GetFullPath(Path.Combine(docs, "preview"));
            if (string.Equals(fullPath, docs, System.StringComparison.OrdinalIgnoreCase))
                return true;

            string docsPrefix = docs + Path.DirectorySeparatorChar;
            string previewPrefix = preview + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(docsPrefix, System.StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(fullPath, preview, System.StringComparison.OrdinalIgnoreCase))
                return false;
            return !fullPath.StartsWith(previewPrefix, System.StringComparison.OrdinalIgnoreCase);
        }

        static void RestoreEditorTarget(string restorePath)
        {
            if (!File.Exists(restorePath))
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += () => RestoreEditorTarget(restorePath);
                return;
            }

            string previous = File.ReadAllText(restorePath).Trim();
            File.Delete(restorePath);
            BuildTarget target;
            if (!Enum.TryParse(previous, out target) || target == BuildTarget.WebGL || target == EditorUserBuildSettings.activeBuildTarget)
                return;

            Debug.Log("Neon Survivor WebGL: devolvendo o editor para " + previous + ".");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
        }

        static string TempFile(string name)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, name);
        }
    }
}
