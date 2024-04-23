#if UNITY_VISIONOS

// The only version supported by the package which does not support foveation is 2022.3.15f1
#if !(UNITY_2022_3_15)
#define UNITY_SUPPORT_FOVEATION
#endif

using System;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

#if UNITY_HAS_URP
using UnityEngine.Rendering.Universal;
#endif

#if INCLUDE_UNITY_XR_HANDS
using UnityEngine.XR.VisionOS;
#endif

namespace UnityEditor.XR.VisionOS
{
    static partial class VisionOSBuildProcessor
    {
        const string k_ARMWorkaroundOriginal = "--additional-defines=IL2CPP_DEBUG=";
        const string k_ARMWorkaroundReplacement = "--additional-defines=IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND=1,IL2CPP_DEBUG=";

        const string k_ARMWorkaroundOriginalAlt = "--compile-cpp";
        const string k_ARMWorkaroundReplacementAlt = "--compile-cpp --additional-defines=IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND=1";

        const string k_SceneManifestKey = "UIApplicationSceneManifest";
        const string k_SupportsMultipleScenesKey = "UIApplicationSupportsMultipleScenes";
        const string k_SessionRoleKey = "UIApplicationPreferredDefaultSceneSessionRole";
        const string k_SessionRoleValue = "CPSceneSessionRoleImmersiveSpaceApplication";
        const string k_HandsTrackingUsageDescriptionKey = "NSHandsTrackingUsageDescription";
        const string k_WorldSensingUsageDescriptionKey = "NSWorldSensingUsageDescription";

        const string k_PluginPath = "Libraries/com.unity.xr.visionos/Runtime/Plugins/visionos";
        const string k_MainFile = "MainApp/main.mm";

        class PostProcessor : IPostprocessBuildWithReport
        {
            // Run last
            public int callbackOrder => 9999;

#if INCLUDE_UNITY_XR_HANDS
            const string k_HandTrackingUsageError = "Hand tracking usage description is required when Initialize Hand Tracking On Startup is enabled. Refer to Project Settings > XR Plug-in Management > Apple visionOS or Project Validation for more information.";
#endif

            public void OnPostprocessBuild(BuildReport report)
            {
                if (report.summary.platform != BuildTarget.VisionOS)
                    return;

                PlayerSettings.SplashScreen.show = s_SplashScreenWasEnabled;
                s_SplashScreenWasEnabled = false;

                if (s_LoaderWasEnabled)
                    VisionOSEditorUtils.EnableLoader();

                s_LoaderWasEnabled = false;

                var outputPath = report.summary.outputPath;
                var settings = VisionOSSettings.currentSettings;
                var appMode = settings.appMode;

                if (settings.il2CPPLargeExeWorkaround)
                    ApplyArmWorkaround(outputPath);

                if (PlayerSettings.VisionOS.sdkVersion == VisionOSSdkVersion.Device)
                    RemoveSimulatorDylib(report.summary.outputPath);

                // Do not do any build post-processing for windowed apps
                if (appMode == VisionOSSettings.AppMode.Windowed)
                    return;

                FilterXcodeProj(outputPath);
                FilterPlist(outputPath, settings);
            }

            static void RemoveSimulatorDylib(string pathToXcodeProject)
            {
                // If the simulator dylib is present in the build for device, the device will crash on launch with an error like this:
                // dyld[435]: Library not loaded: @loader_path/libVisionOS-lib.dylib
                // This happens when doing an incremental build, to the same folder, that doesn't clean after doing a simulator build first.
                try
                {
                    File.Delete(Path.Combine(pathToXcodeProject, "Libraries/libVisionOS-lib.dylib"));
                }
                catch (Exception exception)
                {
                    throw new BuildFailedException("Failed to delete libVisionOS-lib.dylib: " + exception);
                }
            }

            static void FilterXcodeProj(string outputPath)
            {
                var xcodeProjectPath = GetXcodeProjectPath(outputPath);
                if (!File.Exists(xcodeProjectPath))
                {
                    Debug.LogError($"Failed to find Xcode project at path {xcodeProjectPath}");
                    return;
                }

                var pbxProject = new PBXProject();
                pbxProject.ReadFromFile(xcodeProjectPath);

                var unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();
                var unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();

                // We have lots of exported-symbol goop in the UnityFramework target for the simulator.
                // It's not necessary, and actually causes problems because it requires all exported
                // symbols to be specified that way. Remove these here until we get rid of them in the
                // template.
                // Remove this once this is removed from the template!
                foreach (var configName in pbxProject.BuildConfigNames())
                {
                    var configGuid = pbxProject.BuildConfigByName(unityFrameworkTargetGuid, configName);
                    if (configGuid == null)
                        continue;

                    var existing = pbxProject.GetBuildPropertyForConfig(configGuid, "OTHER_LDFLAGS");
                    if (existing != null)
                    {
                        const string exportedSymbolArgument = "-exported_symbol";
                        if (!existing.Contains(exportedSymbolArgument))
                            continue;

                        // This split is not 100% correct, individual elements may be "" quoted. But
                        // we re-join with a " " at the end, and we don't handle backslash-escapes,
                        // so this should be fine in 99.99999999% of cases
                        var items = existing.Split(" ");
                        items = items.Where(s => !s.Contains(exportedSymbolArgument)).ToArray();
                        pbxProject.SetBuildPropertyForConfig(configGuid, "OTHER_LDFLAGS", string.Join(" ", items));
                    }
                }

                // Remove main.mm which is replaced by swift trampoline
                pbxProject.RemoveFile(pbxProject.FindFileGuidByProjectPath(k_MainFile));
                File.Delete(Path.Combine(outputPath, k_MainFile));

                // Move swift trampoline files from UnityFramework to UnityMain
                foreach (var file in k_SwiftTrampolineFiles)
                {
                    var path = Path.Combine(k_PluginPath, file);
                    if (!File.Exists(Path.Combine(outputPath, path)))
                        continue;

                    BuildFileWithUnityTarget(pbxProject, path, unityMainTargetGuid, unityFrameworkTargetGuid);
                }

                AddSettingsFile(pbxProject, outputPath, k_PluginPath, unityMainTargetGuid);

                var pbxProjectContents = pbxProject.WriteToString();
                File.WriteAllText(xcodeProjectPath, pbxProjectContents);
            }

            static void ApplyArmWorkaround(string outputPath)
            {
                var xcodeProjectPath = GetXcodeProjectPath(outputPath);
                if (!File.Exists(xcodeProjectPath))
                {
                    Debug.LogError($"Failed to find Xcode project at path {xcodeProjectPath}");
                    return;
                }

                var pbxProjectContents = File.ReadAllText(xcodeProjectPath);

                // Newer versions use a slightly different IL2CPP script without --additional-defines=IL2CPP_DEBUG=
                pbxProjectContents = pbxProjectContents.Contains(k_ARMWorkaroundOriginal)
                    ? pbxProjectContents.Replace(k_ARMWorkaroundOriginal, k_ARMWorkaroundReplacement)
                    : pbxProjectContents.Replace(k_ARMWorkaroundOriginalAlt, k_ARMWorkaroundReplacementAlt);

                File.WriteAllText(xcodeProjectPath, pbxProjectContents);
            }

            static void BuildFileWithUnityTarget(PBXProject pbx, string file, string unityMainTargetGuid, string unityFrameworkTargetGuid)
            {
                var fileGuid = pbx.FindFileGuidByProjectPath(file);
                pbx.RemoveFileFromBuild(unityFrameworkTargetGuid, fileGuid);
                pbx.AddFileToBuild(unityMainTargetGuid, fileGuid);
            }

            static void FilterPlist(string outputPath, VisionOSSettings settings)
            {
                var plistPath = outputPath + "/Info.plist";
                var text = File.ReadAllText(plistPath);
                var plist = Plist.ReadFromString(text);

                if (settings.appMode == VisionOSSettings.AppMode.VR)
                {
                    var sceneManifestDictionary = plist.CreateElement("dict");
                    var supportsMultipleScenesValue = plist.CreateElement("true");
                    sceneManifestDictionary[k_SupportsMultipleScenesKey] = supportsMultipleScenesValue;
                    var sessionRoleValue = plist.CreateElement("string", k_SessionRoleValue);
                    sceneManifestDictionary[k_SessionRoleKey] = sessionRoleValue;
                    plist.root[k_SceneManifestKey] = sceneManifestDictionary;
                }

                // TODO: Project analysis to detect any scripts/scenes that will request hand tracking
                var handsUsage = settings.handsTrackingUsageDescription;
#if INCLUDE_UNITY_XR_HANDS
                if (string.IsNullOrEmpty(handsUsage) && VisionOSEditorUtils.IsLoaderEnabled())
                {
                    if (VisionOSRuntimeSettings.GetOrCreate().initializeHandTrackingOnStartup)
                        Debug.LogError(k_HandTrackingUsageError);
                }
                else
#endif
                {
                    plist.root[k_HandsTrackingUsageDescriptionKey] = plist.CreateElement("string", handsUsage);
                }

                // TODO: Scene analysis to detect any managers that will request world sensing
                var worldSensingUsage = settings.worldSensingUsageDescription;
                if (!string.IsNullOrEmpty(worldSensingUsage))
                    plist.root[k_WorldSensingUsageDescriptionKey] = plist.CreateElement("string", worldSensingUsage);

                plist.WriteToFile(plistPath);
            }

            static void AddSettingsFile(PBXProject pbx, string outputPath, string pluginPath, string targetGuid)
            {
                const string fileName = "VisionOSSettings.swift";
                var projectPath = Path.Combine(pluginPath, fileName);
                var fullPath = Path.Combine(outputPath, projectPath);
                File.WriteAllText(fullPath, GetSettingsString());
                var guid = pbx.AddFile(projectPath, projectPath);
                pbx.AddFileToBuild(targetGuid, guid);
            }

            static string GetSettingsString()
            {
                const string format = "var VisionOSEnableFoveation = {0};\nvar VisionOSUpperLimbVisibility = {1}";

                var enableFoveation = "false";
#if UNITY_HAS_URP && UNITY_SUPPORT_FOVEATION
                var hasUrpAsset = UniversalRenderPipeline.asset != null;
                if (VisionOSSettings.currentSettings.foveatedRendering && PlayerSettings.VisionOS.sdkVersion == VisionOSSdkVersion.Device && hasUrpAsset)
                    enableFoveation = "true";
#endif

                var upperLimbVisibility = "false";
                if (VisionOSSettings.currentSettings.upperLimbVisibility)
                    upperLimbVisibility = "true";

                return string.Format(format, enableFoveation, upperLimbVisibility);
            }
        }
    }
}
#endif
