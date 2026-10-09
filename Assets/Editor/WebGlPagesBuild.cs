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
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.initialMemorySize = 128;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
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
