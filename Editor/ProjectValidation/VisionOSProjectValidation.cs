using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.VisionOS;
#if UNITY_HAS_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEditor.XR.VisionOS
{
    static class VisionOSProjectValidation
    {
        const string k_VisionOS = "VisionOS";

        const BuildTargetGroup k_VisionOSBuildTarget = BuildTargetGroup.VisionOS; 

        [InitializeOnLoadMethod]
        static void AddValidationRules()
        {
            var buildTargetRules = new BuildValidationRule[]
            {
                new BuildValidationRule()
                {
                    Message = "The Color Space inside Player Settings must be set to Linear.",
                    Category = k_VisionOS,
                    Error = true,
                    CheckPredicate = () => PlayerSettings.colorSpace == ColorSpace.Linear,
                    FixIt = () => PlayerSettings.colorSpace = ColorSpace.Linear,
                    IsRuleEnabled = VisionOSBuildProcessor.IsLoaderEnabled
                },

                new BuildValidationRule()
                {
                    Message = "An ARSession component is required to be active in the scene.",
                    Category = k_VisionOS,
                    Error = true,
                    CheckPredicate = () => Object.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include) != null,
                    FixIt = () => CreateARSession(),
                    IsRuleEnabled = VisionOSBuildProcessor.IsLoaderEnabled
                },

                new BuildValidationRule()
                {
                    Message = "An ARInputManager component is required to be active in the scene.",
                    Category = k_VisionOS,
                    Error = true,
                    CheckPredicate = () => Object.FindAnyObjectByType<ARInputManager>(FindObjectsInactive.Include) != null,
                    FixIt = CreateARInputManager,
                    IsRuleEnabled = VisionOSBuildProcessor.IsLoaderEnabled
                },
#if UNITY_HAS_URP
                new BuildValidationRule()
                {
                    Message = "Each camera must generate a depth texture.",
                    Category = k_VisionOS,
                    Error = true,
                    CheckPredicate = IsCamerasDepthTextureDisabled,
                    FixIt = SetCamerasDepthTextureToEnabled,
                    IsRuleEnabled = VisionOSBuildProcessor.IsLoaderEnabled
                },
#endif
            };

            BuildValidator.AddRules(k_VisionOSBuildTarget, buildTargetRules);
        }

        static GameObject CreateARSession()
        {
            var arSession = Object.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include);
            if (arSession != null)
                return arSession.gameObject;

            var newARSession = new GameObject("AR Session");
            newARSession.AddComponent<ARSession>();
            Undo.RegisterCreatedObjectUndo(newARSession, "Create AR Session");
            return newARSession;
        }

        static void CreateARInputManager()
        {
            var arSession = CreateARSession();
            Undo.AddComponent(arSession, typeof(ARInputManager));
        }

#if UNITY_HAS_URP
        static void SetCamerasDepthTextureToEnabled()
        {
            if (UniversalRenderPipeline.asset == null)
                return;

            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                var cameraData = camera.GetUniversalAdditionalCameraData();
                if (cameraData != null && !cameraData.requiresDepthTexture)
                {
                    Undo.RegisterCompleteObjectUndo(cameraData, "Enable Depth Texture");
                    cameraData.requiresDepthTexture = true;
                }
            }
        }

        static bool IsCamerasDepthTextureDisabled()
        {
            // Passes validation if no asset is set.
            if (UniversalRenderPipeline.asset == null)
                return true;

            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                var cameraData = camera.GetUniversalAdditionalCameraData();
                if (cameraData != null && !cameraData.requiresDepthTexture)
                    return false;
            }

            return true;
        }
#endif
    }
}