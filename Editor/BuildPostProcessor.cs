// TODO: merge with VisionOSBuildProcessor
#if UNITY_VISIONOS
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEditor.UnityLinker;
using RenderMode = UnityEngine.XR.VisionOS.RenderMode;

namespace UnityEditor.XR.VisionOS
{
    public class BuildPostProcessor : IPostprocessBuildWithReport, IUnityLinkerProcessor
    {
        public void OnPostprocessBuild(BuildReport report)
        {
            if (!VisionOSBuildProcessor.IsLoaderEnabled())
                return;
            
            var settings = VisionOSSettings.currentSettings;
            if (settings.appMode == VisionOSSettings.AppMode.MR)
                return;

            var outputPath = report.summary.outputPath;
            FilterXcodeProj(outputPath);
            FilterPlist(outputPath);
        }

        void FilterXcodeProj(string outputPath)
        {
            // TODO: Rename bee platform
            var configHeader = "visionos_config.h";
            File.WriteAllText(outputPath + "/" + configHeader,
                "// Generated during Unity build by com.unity.xr.visionos BuildPostProcessor\n"
                + $"#define VISIONOS_SINGLE_PASS {(VisionOSSettings.currentSettings.renderMode == RenderMode.SinglePassInstanced ? "1" : "0")}\n"
                + $"#define VISIONOS_SIMULATOR {(VisionOSSettings.currentSettings.deviceTarget == VisionOSSettings.DeviceTarget.Simulator ? "1" : "0")}\n");

            var xcodeProj = outputPath + "/Unity-iPhone.xcodeproj";
            if (!Directory.Exists(xcodeProj))
                return;

            var xcodePbx = xcodeProj + "/project.pbxproj";
            var xcodePbxContents = File.ReadAllText(xcodePbx);

            PBXProject pbx = new PBXProject();
            pbx.ReadFromFile(xcodePbx);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(), "CompositorServices.framework", false);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(), "ARKit.framework", false);
            pbx.SetBuildProperty(pbx.GetUnityMainTargetGuid(), "SWIFT_VERSION", "5.0");
            pbx.UpdateBuildProperty(pbx.GetUnityMainTargetGuid(), "OTHER_LDFLAGS", new []{"-Wl,-ld64"}, null);
            pbx.UpdateBuildProperty(pbx.GetUnityFrameworkTargetGuid(), "OTHER_LDFLAGS", new []{"-Wl,-ld64"}, null);
            pbx.AddFile(outputPath + "/" + configHeader, configHeader);
            pbx.RemoveFile(pbx.FindFileGuidByProjectPath("MainApp/main.mm"));
            
            BuildFileWithUnityTarget(pbx, "Libraries/ARM64/Packages/com.unity.xr.visionos/Runtime/visionos/UnityMain.swift");
            BuildFileWithUnityTarget(pbx, "Libraries/ARM64/Packages/com.unity.xr.visionos/Runtime/visionos/UnityLibrary.swift");
            
            pbx.WriteToFile(xcodePbx);
        }

        private void BuildFileWithUnityTarget(PBXProject pbx, string file)
        {
            pbx.RemoveFileFromBuild(pbx.GetUnityFrameworkTargetGuid(), pbx.FindFileGuidByProjectPath(file));
            pbx.AddFileToBuild(pbx.GetUnityMainTargetGuid(), pbx.FindFileGuidByProjectPath(file));
        }
        
        private void FilterPlist(string outputPath)
        {
            var plist = outputPath + "/Info.plist";
            var plistContents = File.ReadAllText(plist);
            if (plistContents.Contains("UIApplicationSupportsMultipleScenes"))
                return;
            
            List<string> nplist = new List<string>();
            int skip = 0;
            foreach (var line in File.ReadLines(plist))
            {
                if (line.Contains("UIRequiredDeviceCapabilities"))
                    skip = 5;
            
                if (skip > 0)
                {
                    --skip;
                    continue;
                }
            
                nplist.Add(line);
                if (nplist.Count == 4)
                {
                    nplist.Add(
                        "<key>UIApplicationSceneManifest</key><dict><key>UIApplicationSupportsMultipleScenes</key><true/></dict>");
                }
            }
            
            File.WriteAllText(plist, string.Join("\n", nplist));
        }

        public int callbackOrder
        {
            // TODO: Merge with ARKit postprocessor
            // Run last
            get => 9999;
        }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return Path.GetFullPath(AssetDatabase.GUIDToAssetPath("bdb2b35a4686f4d8ca0540be9862764d"));
        }
    }
}
#endif
