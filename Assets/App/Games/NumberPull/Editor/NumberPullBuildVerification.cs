using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lbs.MiniGames.Games.NumberPull.Editor
{
    public static class NumberPullBuildVerification
    {
        public static void BuildAndroidDevelopment()
        {
            string outputPath = Environment.GetEnvironmentVariable("NUMBER_PULL_ANDROID_OUTPUT");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new InvalidOperationException("NUMBER_PULL_ANDROID_OUTPUT must point to the verification APK.");
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android verification build failed: {report.summary.result}");
            }

            Debug.Log($"Number Pull Android verification build succeeded: {report.summary.outputPath} ({report.summary.totalSize} bytes).");
        }
    }
}
