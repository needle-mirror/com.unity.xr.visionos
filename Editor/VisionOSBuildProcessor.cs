using System;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.VisionOS;

namespace UnityEditor.XR.VisionOS
{ 
    static class VisionOSBuildProcessor
    {
        internal static bool IsLoaderEnabled()
        {
            var visionOSXRSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                BuildPipeline.GetBuildTargetGroup(BuildTarget.VisionOS));

            return visionOSXRSettings != null && visionOSXRSettings.Manager.activeLoaders.OfType<VisionOSLoader>().Any();
        }

        class Preprocessor : IPreprocessBuildWithReport
        {
            readonly string[] m_RuntimePluginNames =
            {
                // TODO: dummy/stub for native methods to fix undefined symbol errors, or use a define like UNITY_ARKIT_LOADER_ENABLED
                //"UnityVisionOS.a",
                "UnityVisionOS.m",
                "VisionOSAppController.mm",
                //"VisionOSNativeBridge.mm",
                // Quantum.RK package keeps these in Lib~ folder, so no need to distinguish between theirs and ours
                "UnityMain.swift",
                "UnityLibrary.swift"
            };

            public int callbackOrder => 0;

            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
            {
                // TODO: Do we really need this?
                SetRuntimePluginCopyDelegate();
                
                if (!IsLoaderEnabled())
                    return;
                
                var settings = VisionOSSettings.currentSettings;
                if (settings.appMode == VisionOSSettings.AppMode.MR && settings.volumeMode != VisionOSSettings.VolumeMode.Unbounded)
                    Debug.LogWarning("Unbounded mode is required for ARKit features when building for Mixed Reality");
            }

            // ReSharper disable once UnusedMember.Local
            void SetRuntimePluginCopyDelegate()
            {
                var allPlugins = PluginImporter.GetAllImporters();
                foreach (var plugin in allPlugins)
                {
                    if (plugin.isNativePlugin)
                    {
                        foreach (var pluginName in m_RuntimePluginNames)
                        {
                            if (plugin.assetPath.Contains(pluginName))
                            {
                                plugin.SetIncludeInBuildDelegate(ShouldIncludeRuntimePluginsInBuild);
                                break;
                            }
                        }
                    }
                }
            }

            static bool ShouldIncludeRuntimePluginsInBuild(string path)
            {
                if (!IsLoaderEnabled())
                    return false;

                var settings = VisionOSSettings.currentSettings;
                if (settings.appMode == VisionOSSettings.AppMode.MR)
                {
                    if (path.Contains(".swift"))
                        return false;
                }

                return true;
            }
        }
    }
}
