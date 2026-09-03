using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lbs.MiniGames.Bootstrap.Editor
{
    /// <summary>
    /// Headless Android build entry point for CI / CLI usage.
    ///
    /// Usage (from the terminal, project root):
    ///   "/Applications/Unity/Hub/Editor/6000.5.9f1/Unity.app/Contents/MacOS/Unity" \
    ///     -batchmode -nographics \
    ///     -projectPath <project> \
    ///     -executeMethod Lbs.MiniGames.Bootstrap.Editor.AndroidBuild.BuildAndroidApk \
    ///     -quit -logFile -
    /// </summary>
    public static class AndroidBuild
    {
        private const string ProductName = "lbs-minigames";
        private const string DefaultOutputPath = "build/outputs/lbs-minigames-android.apk";

        public static void BuildAndroidApk()
        {
            EnsureAndroidTarget();
            string outputPath = DefaultOutputPath;

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.CleanBuildCache
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            log("===== ANDROID BUILD SUMMARY =====");
            log($"result  : {summary.result}");
            log($"output  : {summary.outputPath}");
            log($"size    : {summary.totalSize} bytes");
            log($"errors  : {summary.totalErrors}");
            log($"warnings: {summary.totalWarnings}");
            log("================================");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Android build failed with result: {summary.result} " +
                                    $"(errors: {summary.totalErrors}, warnings: {summary.totalWarnings})");
            }

            if (!File.Exists(outputPath))
            {
                throw new Exception("Build reported success but no APK was produced at: " + outputPath);
            }

            log("APK ready at: " + Path.GetFullPath(outputPath));
        }

        private static void EnsureAndroidTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                if (!switched)
                {
                    throw new InvalidOperationException("Failed to switch active build target to Android.");
                }
            }
        }

        private static string[] GetEnabledScenes()
        {
            string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
            if (scenes == null || scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes are configured in Build Settings.");
            }
            return scenes;
        }

        private static void log(string message)
        {
            Debug.Log("[AndroidBuild] " + message);
        }
    }
}
