#if UNITY_VISIONOS
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace UnityEditor.XR.VisionOS
{
    static partial class VisionOSBuildProcessor
    {
        const string k_XcodeProjectFolder = "Unity-VisionOS.xcodeproj";
        const string k_XcodeProjectName = "project.pbxproj";

        static bool s_SplashScreenWasEnabled;
        static bool s_LoaderWasEnabled;

        static string GetXcodeProjectPath(string outputPath)
        {
            return Path.Combine(outputPath, k_XcodeProjectFolder, k_XcodeProjectName);
        }

        class LinkerProcessor : IUnityLinkerProcessor
        {
            public int callbackOrder => 0;

            public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
            {
                return FileUtil.GetPhysicalPath(AssetDatabase.GUIDToAssetPath("bdb2b35a4686f4d8ca0540be9862764d"));
            }
        }
    }
}
#endif
