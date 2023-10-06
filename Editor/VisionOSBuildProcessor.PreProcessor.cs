using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityEditor.XR.VisionOS
{
    static partial class VisionOSBuildProcessor
    {
        class Preprocessor : IPreprocessBuildWithReport
        {
            const string k_PreCompiledLibraryName = "libUnityVisionOS.a";
            static readonly string[] k_SourcePluginNames =
            {
                "UnityVisionOS.m",
                "VisionOSAppController.mm",
                // PolySpatial.visionOS package keeps these in Lib~ folder, so no need to distinguish between theirs and ours
                "UnityMain.swift",
                "UnityLibrary.swift"
            };

            public int callbackOrder => 0;

            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
            {
                SetRuntimePluginCopyDelegate();

                if (!IsLoaderEnabled())
                    return;

                var settings = VisionOSSettings.currentSettings;
                if (settings.appMode == VisionOSSettings.AppMode.MR)
                {
#if !UNITY_HAS_POLYSPATIAL_VISIONOS
                    throw new BuildFailedException("Mixed Reality app mode requires the PolySpatial visionOS support package");
#else
                    // TODO: Figure out how to report this warning only if Unbounded is set as the default
                    //if (settings.initialVolumeCameraConfiguration?.Mode != VolumeCamera.PolySpatialVolumeCameraMode.Unbounded)
                    Debug.Log("Notice: an Unbounded volume configuration is required for ARKit features when building for Mixed Reality");
#endif
                }
            }

            static void SetRuntimePluginCopyDelegate()
            {
                var allPlugins = PluginImporter.GetAllImporters();
                foreach (var plugin in allPlugins)
                {
                    if (!plugin.isNativePlugin)
                        continue;

                    // Process pre-compiled library separately. Exactly one version should always be included in the build
                    // regardless of whether the loader is enabled. Otherwise, builds will fail in the linker stage
                    if (plugin.assetPath.Contains(k_PreCompiledLibraryName))
                    {
                        plugin.SetIncludeInBuildDelegate(ShouldIncludePreCompiledLibraryInBuild);
                        continue;
                    }

                    foreach (var pluginName in k_SourcePluginNames)
                    {
                        if (plugin.assetPath.Contains(pluginName))
                        {
                            plugin.SetIncludeInBuildDelegate(ShouldIncludeSourcePluginsInBuild);
                            break;
                        }
                    }
                }
            }

            static bool ShouldIncludeSourcePluginsInBuild(string path)
            {
                if (!IsLoaderEnabled())
                    return false;

                return (VisionOSSettings.currentSettings.appMode != VisionOSSettings.AppMode.MR || !path.Contains(".swift"));
            }

            static bool ShouldIncludePreCompiledLibraryInBuild(string path)
            {
#if UNITY_2022_3_9 || UNITY_2022_3_10
                // Exclude simulator library, which is not used in 2022.3.9f1 and 2022.3.10f1
                return !path.Contains("Simulator");
#else
                // Exclude libraries that don't match the target SDK
                if (PlayerSettings.VisionOS.sdkVersion == VisionOSSdkVersion.Device)
                {
                    if (path.Contains("Simulator"))
                        return false;
                }
                else
                {
                    if (path.Contains("Device"))
                        return false;
                }

                return true;
#endif
            }
        }
    }
}
