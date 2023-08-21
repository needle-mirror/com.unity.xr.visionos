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
            static readonly string[] k_RuntimePluginNames =
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
                if (settings.appMode == VisionOSSettings.AppMode.MR && settings.volumeMode != VisionOSSettings.VolumeMode.Unbounded)
                    Debug.LogWarning("Unbounded mode is required for ARKit features when building for Mixed Reality");
            }

            static void SetRuntimePluginCopyDelegate()
            {
                var allPlugins = PluginImporter.GetAllImporters();
                foreach (var plugin in allPlugins)
                {
                    if (!plugin.isNativePlugin)
                        continue;

                    foreach (var pluginName in k_RuntimePluginNames)
                    {
                        if (plugin.assetPath.Contains(pluginName))
                        {
                            plugin.SetIncludeInBuildDelegate(ShouldIncludeRuntimePluginsInBuild);
                            break;
                        }
                    }
                }
            }

            static bool ShouldIncludeRuntimePluginsInBuild(string path)
            {
                if (!IsLoaderEnabled())
                    return false;

                var settings = VisionOSSettings.currentSettings;
                if (settings.appMode == VisionOSSettings.AppMode.MR && path.Contains(".swift"))
                        return false;

                return true;
            }
        }
    }
}
