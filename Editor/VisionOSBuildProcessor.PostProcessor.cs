#if UNITY_VISIONOS
using UnityEngine;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using RenderMode = UnityEngine.XR.VisionOS.RenderMode;
#endif

namespace UnityEditor.XR.VisionOS
{
    static partial class VisionOSBuildProcessor
    {
        internal const string HandTrackingUsageWarning = "Hand tracking usage description is required when the visionOS" +
            "loader is enabled. The hand subsystem will be started automatically and requires authorization to run.";

#if UNITY_VISIONOS
        const string k_SceneManifestKey = "UIApplicationSceneManifest";
        const string k_SupportsMultipleScenesKey = "UIApplicationSupportsMultipleScenes";
        const string k_HandsTrackingUsageDescriptionKey = "NSHandsTrackingUsageDescription";
        const string k_WorldSensingUsageDescriptionKey = "NSWorldSensingUsageDescription";

        class PostProcessor : IPostprocessBuildWithReport
        {
            // Run last
            public int callbackOrder => 9999;

            public void OnPostprocessBuild(BuildReport report)
            {
                if (!IsLoaderEnabled())
                    return;

                var settings = VisionOSSettings.currentSettings;
                var appMode = settings.appMode;

                var outputPath = report.summary.outputPath;
                FilterPlist(outputPath, settings, appMode);
                if (appMode == VisionOSSettings.AppMode.MR)
                    return;

                FilterXcodeProj(outputPath);
            }

            static void FilterXcodeProj(string outputPath)
            {
                var xcodeProj = outputPath + "/Unity-iPhone.xcodeproj";
                if (!Directory.Exists(xcodeProj))
                    return;

                var xcodePbx = xcodeProj + "/project.pbxproj";
                var pbx = new PBXProject();
                pbx.ReadFromFile(xcodePbx);

                // Add visionos_config header which exposes settings to native code
                const string configHeader = "visionos_config.h";
                File.WriteAllText(outputPath + "/" + configHeader,
                    $"// Generated during Unity build by com.unity.xr.visionos {nameof(VisionOSBuildProcessor)}.{nameof(PostProcessor)}\n"
                    + $"#define VISIONOS_SINGLE_PASS {(VisionOSSettings.currentSettings.renderMode == RenderMode.SinglePassInstanced ? "1" : "0")}\n"
                    + $"#define VISIONOS_SIMULATOR {(VisionOSSettings.currentSettings.deviceTarget == VisionOSSettings.DeviceTarget.Simulator ? "1" : "0")}\n");

                pbx.AddFile(outputPath + "/" + configHeader, configHeader);

                var unityMainTargetGuid = pbx.GetUnityMainTargetGuid();
                var unityFrameworkTargetGuid = pbx.GetUnityFrameworkTargetGuid();

                // Swift version 5 is required for swift trampoline
                pbx.SetBuildProperty(unityMainTargetGuid, "SWIFT_VERSION", "5.0");

                // Use legacy ld64 linker to work around sdk platform mismatch errors
                const string ldFlagsSettingName = "OTHER_LDFLAGS";
                var ldFlagsAddValues = new[] { "-Wl", "-ld64" };
                pbx.UpdateBuildProperty(unityMainTargetGuid, ldFlagsSettingName, ldFlagsAddValues, null);
                pbx.UpdateBuildProperty(unityFrameworkTargetGuid, ldFlagsSettingName, ldFlagsAddValues, null);

                // Add legacy TARGET_OS_XR define which was renamed to TARGET_OS_VISION to fix builds on earlier Unity versions
                const string cFlagsSettingName = "OTHER_CFLAGS";
                var cFlagsAddValues = new[] { "-DTARGET_OS_XR" };
                pbx.UpdateBuildProperty(unityMainTargetGuid, cFlagsSettingName, cFlagsAddValues, null);
                pbx.UpdateBuildProperty(unityFrameworkTargetGuid, cFlagsSettingName, cFlagsAddValues, null);

                // Remove main.mm which is replaced by swift trampoline
                pbx.RemoveFile(pbx.FindFileGuidByProjectPath("MainApp/main.mm"));

                // Move swift trampoline files from UnityFramework to UnityMain
                const string pluginPath = "Libraries/ARM64/Packages/com.unity.xr.visionos/Runtime/visionos";
                BuildFileWithUnityTarget(pbx, $"{pluginPath}/UnityMain.swift", unityMainTargetGuid, unityFrameworkTargetGuid);
                BuildFileWithUnityTarget(pbx, $"{pluginPath}/UnityLibrary.swift", unityMainTargetGuid, unityFrameworkTargetGuid);

                pbx.WriteToFile(xcodePbx);
            }

            static void BuildFileWithUnityTarget(PBXProject pbx, string file, string unityMainTargetGuid, string unityFrameworkTargetGuid)
            {
                var fileGuid = pbx.FindFileGuidByProjectPath(file);
                pbx.RemoveFileFromBuild(unityFrameworkTargetGuid, fileGuid);
                pbx.AddFileToBuild(unityMainTargetGuid, fileGuid);
            }

            static void FilterPlist(string outputPath, VisionOSSettings settings, VisionOSSettings.AppMode appMode)
            {
                var plistPath = outputPath + "/Info.plist";
                var text = File.ReadAllText(plistPath);
                var plist = Plist.ReadFromString(text);

                if (appMode == VisionOSSettings.AppMode.VR)
                {
                    var sceneManifestDictionary = plist.CreateElement("dict");
                    var valueElement = plist.CreateElement("true");
                    sceneManifestDictionary[k_SupportsMultipleScenesKey] = valueElement;
                    plist.root[k_SceneManifestKey] = sceneManifestDictionary;
                }

                // TODO: Enable/disable hand tracking
                var handsUsage = settings.handsTrackingUsageDescription;
                if (string.IsNullOrEmpty(handsUsage))
                    Debug.LogWarning(HandTrackingUsageWarning);
                else
                    plist.root[k_HandsTrackingUsageDescriptionKey] = plist.CreateElement("string", handsUsage);

                // TODO: Scene analysis to detect any managers that will request world sensing
                var worldSensingUsage = settings.worldSensingUsageDescription;
                if (!string.IsNullOrEmpty(worldSensingUsage))
                    plist.root[k_WorldSensingUsageDescriptionKey] = plist.CreateElement("string", worldSensingUsage);

                plist.WriteToFile(plistPath);
            }
        }
#endif
    }
}
