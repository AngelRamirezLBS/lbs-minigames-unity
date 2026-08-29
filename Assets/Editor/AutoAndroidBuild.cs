using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Temporary auto-build helper for Android APK (logic-game branch).
/// Builds APK at build/lbs-minigames-logic.apk including all enabled scenes.
/// </summary>
public static class AutoAndroidBuild
{
    public static void Build()
    {
        Debug.Log("[AutoAndroidBuild] Starting Android APK build...");

        // Force APK (not AAB)
        EditorUserBuildSettings.buildAppBundle = false;

        // Ensure Android target is active (optional, BuildPipeline will switch)
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log($"[AutoAndroidBuild] Switching active build target to Android (current: {EditorUserBuildSettings.activeBuildTarget})");
            // Don't hard-switch here to avoid domain reload issues in batchmode, BuildPipeline will handle it.
        }

        // Ensure minSdk >= 22 (project already 26, keep 26) - use 26 to avoid obsolete API error
        var minSdk = PlayerSettings.Android.minSdkVersion;
        Debug.Log($"[AutoAndroidBuild] Current minSdkVersion: {minSdk} ({(int)minSdk})");
        if ((int)minSdk < 26)
        {
#pragma warning disable 618
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
#pragma warning restore 618
            Debug.Log("[AutoAndroidBuild] Updated minSdkVersion to 26");
        }

        // Ensure ARM64
        var arch = PlayerSettings.Android.targetArchitectures;
        Debug.Log($"[AutoAndroidBuild] Current targetArchitectures: {arch}");
        if (arch != AndroidArchitecture.ARM64)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("[AutoAndroidBuild] Set targetArchitectures to ARM64");
        }

        // Collect enabled scenes
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        Debug.Log($"[AutoAndroidBuild] Scenes to build ({scenes.Length}): {string.Join(", ", scenes)}");
        if (scenes.Length == 0)
        {
            Debug.LogError("[AutoAndroidBuild] No enabled scenes found! Aborting.");
            EditorApplication.Exit(1);
            return;
        }

        // Verify expected 4 scenes present
        var expected = new[] { "Assets/App/Bootstrap/Bootstrap.unity", "Assets/App/Lobby/Lobby.unity", "Assets/App/Games/Classification/Classification.unity", "Assets/App/Games/CountAndSelect/CountAndSelect.unity" };
        foreach (var e in expected)
        {
            if (!scenes.Contains(e))
                Debug.LogWarning($"[AutoAndroidBuild] Expected scene missing: {e}");
        }

        // Ensure build folder exists
        var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var apkRelative = "build/lbs-minigames-logic.apk";
        var apkFull = Path.Combine(projectPath, apkRelative);
        var dir = Path.GetDirectoryName(apkFull);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            Debug.Log($"[AutoAndroidBuild] Created directory: {dir}");
        }

        // Also ensure alternative path build/lbs-minigames-unity.apk not conflicting
        Debug.Log($"[AutoAndroidBuild] Building APK to: {apkFull}");
#pragma warning disable 618
        var appId = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);
#pragma warning restore 618
        Debug.Log($"[AutoAndroidBuild] ApplicationIdentifier: {appId} company={PlayerSettings.companyName} product={PlayerSettings.productName} bundleVersion={PlayerSettings.bundleVersion} versionCode={PlayerSettings.Android.bundleVersionCode}");

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkFull,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AutoAndroidBuild] Build succeeded! Size: {summary.totalSize} bytes, APK: {apkFull}, result: {summary.result}");
            // Extra file exist check
            if (File.Exists(apkFull))
            {
                var fi = new FileInfo(apkFull);
                Debug.Log($"[AutoAndroidBuild] APK exists: {fi.FullName} Size: {fi.Length} bytes ({fi.Length / (1024f * 1024f):F2} MB) LastWrite: {fi.LastWriteTime}");
            }
            else
            {
                Debug.LogError($"[AutoAndroidBuild] Build reported success but APK not found at {apkFull}");
                EditorApplication.Exit(1);
                return;
            }
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[AutoAndroidBuild] Build failed! Result: {summary.result} Errors: {summary.totalErrors} Warnings: {summary.totalWarnings}");
            // Dump steps?
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"[AutoAndroidBuild][{step.name}] {msg.type}: {msg.content}");
                }
            }
            EditorApplication.Exit(1);
        }
    }
}
