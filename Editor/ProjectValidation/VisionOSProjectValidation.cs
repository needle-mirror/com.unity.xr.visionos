using Unity.XR.CoreUtils.Editor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.VisionOS;
using UnityObject = UnityEngine.Object;

#if UNITY_HAS_URP
using System;
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEditor.XR.VisionOS
{
    [InitializeOnLoad]
    static class VisionOSProjectValidation
    {
        const string k_CategoryFormat = "VisionOS - {0}";
        const string k_ARSessionMessageVR = "An ARSession component is required to be active in the scene.";
        const string k_ARSessionMessageMR = "An ARSession component is required to be active in the scene to provide access to ARKit features.";
        const string k_RendererDataListPropertyName = "m_RendererDataList";
        const string k_CopyDepthModePropertyName = "m_CopyDepthMode";

        const BuildTargetGroup k_VisionOSBuildTarget = BuildTargetGroup.VisionOS;

        static readonly BuildValidationRule[] k_Rules;

        static VisionOSProjectValidation()
        {
            k_Rules = new BuildValidationRule[]
            {
                new ()
                {
                    Message = "The Color Space inside Player Settings must be set to Linear.",
                    Category = string.Format(k_CategoryFormat, "Color Space"),
                    Error = true,
                    CheckPredicate = () => PlayerSettings.colorSpace == ColorSpace.Linear,
                    FixIt = () => PlayerSettings.colorSpace = ColorSpace.Linear
                },

                new ()
                {
                    Message = k_ARSessionMessageVR,
                    Category = string.Format(k_CategoryFormat, "ARSession"),
                    Error = true,
                    CheckPredicate = () =>
                    {
                        var thisRule = k_Rules?[1];
                        if (thisRule != null)
                        {
                            var isVR = CheckAppMode(VisionOSSettings.AppMode.VR);
                            thisRule.Error = isVR;
                            thisRule.Message = isVR ? k_ARSessionMessageVR : k_ARSessionMessageMR;
                        }

                        return UnityObject.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include) != null;
                    },
                    FixIt = () => CreateARSession(),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && !CheckAppMode(VisionOSSettings.AppMode.Windowed)
                },

                new ()
                {
                    Message = "The ARSession component requires the Apple visionOS plug-in to be enabled in the XR Plug-in Management.",
                    FixItMessage = "Enable the Apple visionOS plug-in",
                    Category = string.Format(k_CategoryFormat, "ARSession and XR Plug-in"),
                    CheckPredicate = VisionOSEditorUtils.IsLoaderEnabled,
                    FixIt = EnableVisionOSLoader,
                    IsRuleEnabled = () => UnityObject.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include) != null
                },

                new ()
                {
                    Message = "Virtual Reality apps require the Apple visionOS plug-in to be enabled in the XR Plug-in Management.",
                    FixItMessage = "Virtual Reality the Apple visionOS plug-in",
                    Category = string.Format(k_CategoryFormat, "ARSession and XR Plug-in"),
                    Error = true,
                    CheckPredicate = VisionOSEditorUtils.IsLoaderEnabled,
                    FixIt = EnableVisionOSLoader,
                    IsRuleEnabled = () =>
                    {
                        var settings = VisionOSSettings.currentSettings;
                        if (settings == null)
                            return false;

                        return settings.appMode == VisionOSSettings.AppMode.VR && !VisionOSEditorUtils.IsLoaderEnabled();
                    }
                },

                new ()
                {
                    Message = "An ARInputManager component is required to be active in the scene.",
                    Category = string.Format(k_CategoryFormat, "ARInputManager"),
                    Error = true,
                    CheckPredicate = () => UnityObject.FindAnyObjectByType<ARInputManager>(FindObjectsInactive.Include) != null,
                    FixIt = CreateARInputManager,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && CheckAppMode(VisionOSSettings.AppMode.VR)
                },
#if UNITY_HAS_URP
                new ()
                {
                    Message = "Each camera must generate a depth texture.",
                    Category = string.Format(k_CategoryFormat, "Camera depth texture"),
                    Error = true,
                    CheckPredicate = IsCamerasDepthTextureDisabled,
                    FixIt = SetCamerasDepthTextureToEnabled,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && CheckAppMode(VisionOSSettings.AppMode.VR)
                },
                new ()
                {
                    Message = "After Opaques is the only supported Depth Texture Mode for visionOS VR applications.",
                    Category = string.Format(k_CategoryFormat, "DepthTextureMode"),
                    Error = true,
                    CheckPredicate = IsDepthTextureModeNotAfterOpaques,
                    FixIt = SetDepthTextureModeToAfterOpaques,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && CheckAppMode(VisionOSSettings.AppMode.VR)
                },
#endif

#if INCLUDE_UNITY_XR_HANDS
                new ()
                {
                    Message = "Hand Tracking Usage Description (in Apple visionOS settings) is required to automatically initialize hand tracking. You must " +
                        "set a usage description to prevent your app from crashing when trying to start an AR Session.",
                    FixItMessage = "Update the Hand Tracking Usage Description or disable Initialize Hand Tracking On Startup",
                    Category = string.Format(k_CategoryFormat, "Hand Tracking Usage Description"),
                    Error = true,
                    CheckPredicate = () => !GetEditorSettings(out var editorSettings) || !string.IsNullOrEmpty(editorSettings.handsTrackingUsageDescription),
                    FixItAutomatic = false,
                    FixIt = () => SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Apple visionOS"),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && !CheckAppMode(VisionOSSettings.AppMode.Windowed)
                        // Show an error if initializeHandTrackingOnStartup is true
                        && GetRuntimeSettings(out var runtimeSettings) && runtimeSettings.initializeHandTrackingOnStartup
                },

                new ()
                {
                    Message = "Hand Tracking Usage Description (in Apple visionOS settings) is required for hand tracking features. If your app uses hand " +
                        "tracking, your app will crash when trying to start an AR Session. If your app does not use hand tracking features, you can safely " +
                        "ignore this warning.",
                    FixItMessage = "Update the Hand Tracking Usage Description",
                    Category = string.Format(k_CategoryFormat, "Hand Tracking Usage Description"),
                    CheckPredicate = () => !GetEditorSettings(out var editorSettings) || !string.IsNullOrEmpty(editorSettings.handsTrackingUsageDescription),
                    FixItAutomatic = false,
                    FixIt = () => SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Apple visionOS"),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && !CheckAppMode(VisionOSSettings.AppMode.Windowed)
                        // Show a warning if initializeHandTrackingOnStartup is false
                        && GetRuntimeSettings(out var runtimeSettings) && !runtimeSettings.initializeHandTrackingOnStartup
                },
#endif

                new ()
                {
                    Message = "World Sensing Usage Description (in Apple visionOS settings) is required for world sensing features (images, planes or meshes). " +
                        "If your app uses world sensing, you need to add a World Sensing Usage Description in the Apple visionOS settings. If your app does not " +
                        "use world sensing features, you can safely ignore this warning.",
                    FixItMessage = "Update the World Sensing Usage Description",
                    Category = string.Format(k_CategoryFormat, "World Sensing Usage Description"),
                    Error = false,
                    CheckPredicate = () => !GetEditorSettings(out var editorSettings) || !string.IsNullOrEmpty(editorSettings.worldSensingUsageDescription),
                    FixItAutomatic = false,
                    FixIt = () => SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Apple visionOS"),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && !CheckAppMode(VisionOSSettings.AppMode.Windowed)
                },
                new ()
                {
                    Message = "Splash screen is not yet supported for visionOS. If the splash screen is enabled, you may have errors when building or when " +
                              "running your application in the simulator or in the device.",
                    FixItMessage = "Disable the splash screen",
                    Category = string.Format(k_CategoryFormat, "Splash Screen"),
                    Error = true,
                    CheckPredicate = () => !PlayerSettings.SplashScreen.show,
                    FixIt = () => PlayerSettings.SplashScreen.show = false
                },
            };

            BuildValidator.AddRules(k_VisionOSBuildTarget, k_Rules);
        }

        static bool CheckAppMode(VisionOSSettings.AppMode mode)
        {
            return GetEditorSettings(out var editorSettings) && editorSettings.appMode == mode;
        }

        static bool GetEditorSettings(out VisionOSSettings editorSettings)
        {
            editorSettings = VisionOSSettings.currentSettings;
            return editorSettings != null;
        }

#if INCLUDE_UNITY_XR_HANDS
        static bool GetRuntimeSettings(out VisionOSRuntimeSettings runtimeSettings)
        {
            runtimeSettings = VisionOSRuntimeSettings.GetOrCreate();
            return runtimeSettings != null;
        }
#endif

        static GameObject CreateARSession()
        {
            var arSession = UnityObject.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include);
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

            var cameras = UnityObject.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

            var cameras = UnityObject.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                var cameraData = camera.GetUniversalAdditionalCameraData();
                if (cameraData != null && !cameraData.requiresDepthTexture)
                    return false;
            }

            return true;
        }

        static void ForEachRendererData(UniversalRenderPipelineAsset asset, Action<UnityObject> action)
        {
            // NB: Copy Depth Mode is not exposed on UniversalRenderer, so we need to snoop SerializedProperties to check for UniversalRendererData references.
            var serializedObject = new SerializedObject(asset);
            var rendererDataList = serializedObject.FindProperty(k_RendererDataListPropertyName);
            if (rendererDataList == null)
                return;

            var count = rendererDataList.arraySize;
            for (var i = 0; i < count; i++)
            {
                var rendererDataListElement = rendererDataList.GetArrayElementAtIndex(i);
                var rendererData = rendererDataListElement.objectReferenceValue;
                if (rendererData == null)
                    continue;

                action.Invoke(rendererData);
            }
        }

        static SerializedProperty GetCopyDepthModeProperty(UnityObject rendererData)
        {
            var serializedObject = new SerializedObject(rendererData);
            return serializedObject.FindProperty(k_CopyDepthModePropertyName);
        }

        static void SetDepthTextureModeToAfterOpaques()
        {
            var asset = UniversalRenderPipeline.asset;
            if (asset == null)
                return;

            ForEachRendererData(asset, rendererData =>
            {
                var copyDepthModeProperty = GetCopyDepthModeProperty(rendererData);
                if (copyDepthModeProperty == null)
                    return;

                copyDepthModeProperty.intValue = (int)CopyDepthMode.AfterOpaques;
                copyDepthModeProperty.serializedObject.ApplyModifiedProperties();
            });
        }


        static bool IsDepthTextureModeNotAfterOpaques()
        {
            // Passes validation if no asset is set.
            var asset = UniversalRenderPipeline.asset;
            if (asset == null)
                return true;

            var foundInvalidRenderer = false;
            ForEachRendererData(asset, rendererData =>
            {
                var copyDepthModeProperty = GetCopyDepthModeProperty(rendererData);
                if (copyDepthModeProperty == null)
                    return;

                if (copyDepthModeProperty.intValue != (int)CopyDepthMode.AfterOpaques)
                    foundInvalidRenderer = true; // If any renderer's copy depth mode is not set to AfterOpaques, rendering issues can occur on visionOS hardware.
            });

            return !foundInvalidRenderer;
        }
#endif

        static void EnableVisionOSLoader()
        {
            var visionOSLoaderGUIDs = AssetDatabase.FindAssets($"t:{nameof(VisionOSLoader)}");
            if (visionOSLoaderGUIDs.Length == 0)
                return;

            var visionOSLoader = AssetDatabase.LoadAssetAtPath<VisionOSLoader>(AssetDatabase.GUIDToAssetPath(visionOSLoaderGUIDs[0]));
            if (visionOSLoader == null)
                return;

            var visionOSXRSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                BuildPipeline.GetBuildTargetGroup(BuildTarget.VisionOS));

            if (visionOSXRSettings == null)
                return;

            var manager = visionOSXRSettings.Manager;
            if (manager == null)
                return;

            if (manager.TryAddLoader(visionOSLoader))
            {
                EditorUtility.SetDirty(manager);
                AssetDatabase.SaveAssetIfDirty(manager);
            }
        }
    }
}
