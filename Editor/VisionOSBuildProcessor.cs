using System;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.VisionOS;

namespace UnityEditor.XR.VisionOS
{
    static partial class VisionOSBuildProcessor
    {
        internal static bool IsLoaderEnabled()
        {
            var visionOSXRSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                BuildPipeline.GetBuildTargetGroup(BuildTarget.VisionOS));

            if (visionOSXRSettings == null)
                return false;

            var manager = visionOSXRSettings.Manager;
            if (manager == null)
                return false;

            var activeLoaders = manager.activeLoaders;
            return activeLoaders != null && activeLoaders.OfType<VisionOSLoader>().Any();
        }

        class LinkerProcessor : IUnityLinkerProcessor
        {
            public int callbackOrder => 0;

            public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
            {
                return Path.GetFullPath(AssetDatabase.GUIDToAssetPath("bdb2b35a4686f4d8ca0540be9862764d"));
            }
        }
    }
}
