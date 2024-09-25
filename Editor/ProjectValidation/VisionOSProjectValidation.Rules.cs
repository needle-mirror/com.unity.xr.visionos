using System;
using System.Linq;
using System.Reflection;
using Unity.XR.CoreUtils.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityObject = UnityEngine.Object;

#if UNITY_HAS_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEditor.XR.VisionOS
{
    // Just the rule definitions and supporting fields and types
    static partial class VisionOSProjectValidation
    {
        /// <summary>
        /// Test container to include a name for each rule and Setup method so the test can validate that `CheckPredicate` returns false when the rule should be
        /// triggered. Also provides a TearDown method to undo whatever was done in Setup. TearDown should reset any project state that was modified in SetUp,
        /// and SetUp should set things up so that the rule is enabled (IsRuleEnabled returns true) and the check fails (CheckPredicate returns false).
        /// </summary>
        internal struct RuleTestContainer
        {
            /// <summary>
            /// The name of this rule, which will be included in the name of the test
            /// </summary>
            public string Name;

            /// <summary>
            /// The validation rule
            /// </summary>
            public BuildValidationRule Rule;

            /// <summary>
            /// SetUp method used by tests to ensure the rule is enabled and temporarily fails the check. The test will then ensure that `IsRuleEnabled` returns
            /// true and `CheckPredicate` returns false. Then it will run `FixIt`, ensure there are no unexpected logs, check that `CheckPredicate` returns true,
            /// and run TearDown to reset project/editor state.
            /// Return a message if any errors were encountered during setup (this is to avoid a dependency on nunit in this assembly), null otherwise.
            /// </summary>
            public Func<string> SetUp;

            /// <summary>
            /// TearDown method used by tests to reset project/editor state to what it was before SetUp and FixIt were run.
            /// Return a message if any errors were encountered during teardown (this is to avoid a dependency on nunit in this assembly), null otherwise.
            /// </summary>
            public Func<string> TearDown;

            /// <summary>
            /// Return a message if this test should be skipped for some reason. For example, some URP validations can only be tested if a URP asset is set,
            /// null otherwise.
            /// </summary>
            public Func<string> SkipTest;

            /// <summary>
            /// When FixItAutomatic is false, we expect the user to take some action. Define this method to fill in what the user might do. If there really is
            /// no way to fix the issue in code, return true for SkipTest above.
            /// Return a message if any errors were encountered during this method (this is to avoid a dependency on nunit in this assembly), null otherwise.
            /// </summary>
            public Func<string> TestFixIt;
        }

        const string k_CategoryFormat = "VisionOS - {0}";
        const string k_ARSessionMessageMetal = "An ARSession component is required to be active in the scene.";
        const string k_ARSessionMessageRealityKit = "An ARSession component is required to be active in the scene to provide access to ARKit features.";

#if UNITY_HAS_URP
        const string k_RendererDataListPropertyName = "m_RendererDataList";
        const string k_CopyDepthModePropertyName = "m_CopyDepthMode";
#endif

        const BuildTargetGroup k_VisionOSBuildTarget = BuildTargetGroup.VisionOS;

        // TODO: LXR-4045 Expose color gamuts player setting API
        static readonly MethodInfo k_GetColorGamuts = typeof(PlayerSettings).GetMethod("GetColorGamuts", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo k_SetColorGamuts = typeof(PlayerSettings).GetMethod("SetColorGamuts", BindingFlags.Static | BindingFlags.NonPublic);

        static readonly BuildValidationRule[] k_Rules;

        // This field needs to be made internal for testing
        // ReSharper disable once MemberCanBePrivate.Global
        internal static readonly RuleTestContainer[] k_ValidationRules =
        {
            new()
            {
                Name = "Color Space",
                Rule = new()
                {
                    Message = "The Color Space inside Player Settings must be set to Linear.",
                    Category = string.Format(k_CategoryFormat, "Color Space"),
                    Error = true,
                    CheckPredicate = () => PlayerSettings.colorSpace == ColorSpace.Linear,
                    FixIt = () => PlayerSettings.colorSpace = ColorSpace.Linear
                },
                SetUp = () =>
                {
                    s_PreviousColorSpace = PlayerSettings.colorSpace;
                    PlayerSettings.colorSpace = ColorSpace.Gamma;
                    return null;
                },
                TearDown = () =>
                {
                    PlayerSettings.colorSpace = s_PreviousColorSpace;
                    return null;
                }
            },
            new()
            {
                Name = "ARSession",
                Rule = new()
                {
                    Message = k_ARSessionMessageMetal,
                    Category = string.Format(k_CategoryFormat, "ARSession"),
                    Error = true,
                    CheckPredicate = () =>
                    {
                        // TODO: Is this check really needed? If so let's update this comment to explain why
                        var thisRule = k_Rules?[1];
                        if (thisRule != null)
                        {
                            var isMetal = CheckAppMode(VisionOSSettings.AppMode.Metal);
                            thisRule.Error = isMetal;
                            thisRule.Message = isMetal ? k_ARSessionMessageMetal : k_ARSessionMessageRealityKit;
                        }

                        return UnityObject.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include) != null;
                    },
                    FixIt = CreateARSession,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && !CheckAppMode(VisionOSSettings.AppMode.Windowed)
                },
                SetUp = () =>
                {
                    var message = CheckForLeakedARSession();
                    if (message != null)
                        return message;

                    message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    settings.appMode = VisionOSSettings.AppMode.Metal;

                    return null;
                },
                TearDown = () =>
                {
                    DestroyARSessionIfNewSessionExists();
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settingsMessage == null)
                        settings.appMode = s_PreviousAppMode;

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                }
            },
            new()
            {
                Name = "ARSession and XR Plug-in",
                Rule = new()
                {
                    Message = "The ARSession component requires the Apple visionOS plug-in to be enabled in the XR Plug-in Management.",
                    FixItMessage = "Enable the Apple visionOS plug-in",
                    Category = string.Format(k_CategoryFormat, "ARSession and XR Plug-in"),
                    CheckPredicate = VisionOSEditorUtils.IsLoaderEnabled,
                    FixIt = () => SetVisionOSLoaderEnabled(true),
                    IsRuleEnabled = () => UnityObject.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include) != null
                },
                SetUp = () =>
                {
                    var message = CheckForLeakedARSession();
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(false);
                    if (message != null)
                        return message;

                    // Create an AR Session to trigger the check
                    CreateARSession();

                    return null;
                },
                TearDown = () =>
                {
                    DestroyARSessionIfNewSessionExists();
                    return SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                }
            },
            new()
            {
                Name = "Metal app mode and XR Plug-in",
                Rule = new()
                {
                    Message = "Metal rendering requires the Apple visionOS plug-in to be enabled in the XR Plug-in Management.",
                    FixItMessage = "Enable the Apple visionOS plug-in",
                    Category = string.Format(k_CategoryFormat, "Metal app mode and XR Plug-in"),
                    Error = true,
                    CheckPredicate = VisionOSEditorUtils.IsLoaderEnabled,
                    FixIt = () => SetVisionOSLoaderEnabled(true),
                    IsRuleEnabled = () =>
                    {
                        var settings = VisionOSSettings.currentSettings;
                        if (settings == null)
                            return false;

                        return settings.appMode == VisionOSSettings.AppMode.Metal && !VisionOSEditorUtils.IsLoaderEnabled();
                    }
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(false);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    settings.appMode = VisionOSSettings.AppMode.Metal;

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                }
            },
            new()
            {
                Name = "World Sensing Usage Description",
                Rule = new()
                {
                    Message =
                        "World Sensing Usage Description (in Apple visionOS settings) is required for world sensing features (images, planes or meshes). " +
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
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_PreviousUsageDescription = settings.worldSensingUsageDescription;
                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    settings.worldSensingUsageDescription = null;

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                    {
                        settings.appMode = s_PreviousAppMode;
                        settings.worldSensingUsageDescription = s_PreviousUsageDescription;
                    }

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                TestFixIt = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    settings.worldSensingUsageDescription = k_TestUsageDescription;
                    return null;
                }
            },
            new()
            {
                Name = "Splash Screen",
                Rule = new()
                {
                    Message = "Splash screen is not yet supported for visionOS. If the splash screen is enabled, you may have errors when building or when " +
                        "running your application in the simulator or in the device.",
                    FixItMessage = "Disable the splash screen",
                    Category = string.Format(k_CategoryFormat, "Splash Screen"),
                    Error = true,
                    CheckPredicate = () => !PlayerSettings.SplashScreen.show,
                    FixIt = () => PlayerSettings.SplashScreen.show = false
                },
                SetUp = () =>
                {
                    s_SplashScreenWasEnabled = PlayerSettings.SplashScreen.show;
                    PlayerSettings.SplashScreen.show = true;
                    return null;
                },
                TearDown = () =>
                {
                    PlayerSettings.SplashScreen.show = s_SplashScreenWasEnabled;
                    return null;
                }
            },

#if UNITY_HAS_URP
            new()
            {
                Name = "Camera depth texture",
                Rule = new()
                {
                    Message = "Each camera must generate a depth texture.",
                    Category = string.Format(k_CategoryFormat, "Camera depth texture"),
                    Error = true,
                    CheckPredicate = IsCamerasDepthTextureDisabled,
                    FixIt = SetCamerasDepthTextureToEnabled,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    settings.appMode = VisionOSSettings.AppMode.Metal;

                    return CreateTestCameraWithNoDepthTexture();
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    var destroyMessage = DestroyTestCamera();
                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, destroyMessage, loaderMessage);
                },
                SkipTest = () => UniversalRenderPipeline.asset == null ? "Skipping Camera depth texture validation test because there is no URP asset set." : null
            },
            new()
            {
                Name = "DepthTextureMode",
                Rule = new()
                {
                    Message = "After Opaques is the only supported Depth Texture Mode for visionOS Metal applications.",
                    Category = string.Format(k_CategoryFormat, "DepthTextureMode"),
                    Error = true,
                    CheckPredicate = IsDepthTextureModeNotAfterOpaques,
                    FixIt = () => SetCopyDepthMode(CopyDepthMode.AfterOpaques),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    settings.appMode = VisionOSSettings.AppMode.Metal;

                    StoreCopyDepthModeSettings();
                    SetCopyDepthMode(CopyDepthMode.AfterTransparents);

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    RestoreCopyDepthModes();
                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                SkipTest = () => UniversalRenderPipeline.asset == null ? "Skipping DepthTextureMode validation test because there is no URP asset set." : null
            },
#endif

#if INCLUDE_UNITY_XR_HANDS
            new()
            {
                Name = "Hand Tracking Usage Description Error",
                Rule = new()
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
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var editorSettings);
                    if (message != null)
                        return message;

                    message = GetRuntimeSettingsIfExists(out var runtimeSettings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = editorSettings.appMode;
                    s_PreviousUsageDescription = editorSettings.handsTrackingUsageDescription;
                    s_InitializeHandTrackingWasEnabled = runtimeSettings.initializeHandTrackingOnStartup;
                    editorSettings.appMode = VisionOSSettings.AppMode.Metal;
                    editorSettings.handsTrackingUsageDescription = null;
                    runtimeSettings.initializeHandTrackingOnStartup = true;

                    return null;
                },
                TearDown = () =>
                {
                    var editorSettingsMessage = GetEditorSettingsIfExists(out var editorSettings);
                    if (editorSettings != null)
                    {
                        editorSettings.appMode = s_PreviousAppMode;
                        editorSettings.handsTrackingUsageDescription = s_PreviousUsageDescription;
                    }

                    var runtimeSettingsMessage = GetRuntimeSettingsIfExists(out var runtimeSettings);
                    if (runtimeSettings != null)
                        runtimeSettings.initializeHandTrackingOnStartup = s_InitializeHandTrackingWasEnabled;

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(editorSettingsMessage, runtimeSettingsMessage, loaderMessage);
                },
                TestFixIt = () =>
                {
                    var message = GetEditorSettingsIfExists(out var editorSettings);
                    if (message != null)
                        return message;

                    editorSettings.handsTrackingUsageDescription = k_TestUsageDescription;
                    return null;
                }
            },
            new()
            {
                Name = "Hand Tracking Usage Description Warning",
                Rule = new()
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
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var editorSettings);
                    if (message != null)
                        return message;

                    message = GetRuntimeSettingsIfExists(out var runtimeSettings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = editorSettings.appMode;
                    s_PreviousUsageDescription = editorSettings.handsTrackingUsageDescription;
                    s_InitializeHandTrackingWasEnabled = runtimeSettings.initializeHandTrackingOnStartup;
                    editorSettings.appMode = VisionOSSettings.AppMode.Metal;
                    editorSettings.handsTrackingUsageDescription = null;
                    runtimeSettings.initializeHandTrackingOnStartup = false;

                    return null;
                },
                TearDown = () =>
                {
                    var editorSettingsMessage = GetEditorSettingsIfExists(out var editorSettings);
                    if (editorSettings != null)
                    {
                        editorSettings.appMode = s_PreviousAppMode;
                        editorSettings.handsTrackingUsageDescription = s_PreviousUsageDescription;
                    }

                    var runtimeSettingsMessage = GetRuntimeSettingsIfExists(out var runtimeSettings);
                    if (runtimeSettings != null)
                        runtimeSettings.initializeHandTrackingOnStartup = s_InitializeHandTrackingWasEnabled;

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(runtimeSettingsMessage, editorSettingsMessage, loaderMessage);
                },
                TestFixIt = () =>
                {
                    var message = GetEditorSettingsIfExists(out var editorSettings);
                    if (message != null)
                        return message;

                    editorSettings.handsTrackingUsageDescription = k_TestUsageDescription;
                    return null;
                }
            },
#endif

            new()
            {
                Name = "DisplayP3 Color Gamut",
                Rule = new ()
                {
                    Message = "DisplayP3 color gamut is required for HDR rendering.",
                    FixItMessage = "Enable DisplayP3 color gamut",
                    Category = string.Format(k_CategoryFormat, "DisplayP3 Color Gamut"),
                    Error = true,
                    CheckPredicate = () =>
                    {
                        if (k_GetColorGamuts == null)
                            return true;

                        var gamuts = k_GetColorGamuts.Invoke(null, null) as ColorGamut[];
                        if (gamuts == null)
                            return true;

                        return gamuts.Contains(ColorGamut.DisplayP3);
                    },
                    FixIt = () =>
                    {
                        if (k_SetColorGamuts == null)
                            return;

                        try
                        {
                            k_SetColorGamuts.Invoke(null, new object[] { new[] { ColorGamut.DisplayP3, ColorGamut.sRGB } });
                        }
                        catch (Exception)
                        {
                            // Ignore potential null reference exception that can occur if the user clicks "Fix" before having ever loaded player settings
                        }
                    },
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal() && IsHDREnabled()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    if (k_GetColorGamuts == null)
                        return "Validation test failed: GetColorGamuts method could not be found via reflection.";

                    // Check for SetColorGamuts since FixMe will silently fail if it is nul
                    if (k_SetColorGamuts == null)
                        return "Validation test failed: SetColorGamuts method could not be found via reflection.";

                    s_PreviousColorGamuts = k_GetColorGamuts.Invoke(null, null);
                    if (s_PreviousColorGamuts == null)
                        return "Validation test failed: GetColorGamuts returned null.";

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_HdrWasEnabled = IsHDREnabled();
                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    SetHDREnabled(true);

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settingsMessage != null)
                        settings.appMode = s_PreviousAppMode;

                    string reflectionMessage = null;
                    if (k_SetColorGamuts == null)
                    {
                        reflectionMessage = "Validation test failed: SetColorGamuts method could not be found via reflection.";
                    }
                    else
                    {
                        try
                        {
                            k_SetColorGamuts.Invoke(null, new[] { s_PreviousColorGamuts });
                        }
                        catch (Exception)
                        {
                            // Ignore potential null reference exception that can occur if the tests are run before having ever loaded player settings
                        }
                    }

                    SetHDREnabled(s_HdrWasEnabled);

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, reflectionMessage, loaderMessage);
                }
            },
            new()
            {
                Name = "Allow HDR Display Output",
                Rule = new ()
                {
                    // TODO: LXR-4047 This is only necessary for URP because PLATFORM_REQUIRES_HDR doesn't change the behavior of shader stripping.
                    // URP strips the HDR Overlay shader if PlayerSettings.allowHDRDisplaySupport is false, even though we have things
                    // configured to assume this setting is always enabled in the player
                    Message = "Allow HDR Display Output is required for HDR rendering on visionOS with the Universal Render Pipeline.",
                    FixItMessage = "Enable Allow HDR Display Output",
                    Category = string.Format(k_CategoryFormat, "Allow HDR Display Output"),
                    Error = true,
                    CheckPredicate = () => PlayerSettings.allowHDRDisplaySupport,
                    FixIt = () => PlayerSettings.allowHDRDisplaySupport = true,
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal() && IsHDREnabled() && HasUrpAsset()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_HdrWasEnabled = IsHDREnabled();
                    s_AllowHDRDisplaySupportWasEnabled = PlayerSettings.allowHDRDisplaySupport;
                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    SetHDREnabled(true);
                    PlayerSettings.allowHDRDisplaySupport = false;

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    SetHDREnabled(s_HdrWasEnabled);
                    PlayerSettings.allowHDRDisplaySupport = s_AllowHDRDisplaySupportWasEnabled;

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                SkipTest = () => !HasUrpAsset() ? "Skipping Allow HDR Display Output validation test because there is no URP asset set." : null
            },
            new()
            {
                Name = "HDR Post Processing Tone Mapping",
                Rule = new ()
                {
                    Message = "HDR rendering will not look right when post processing is enabled but tone mapping is disabled.",
                    FixItMessage = "Enable tone mapping in global volume profile",
                    Category = string.Format(k_CategoryFormat, "HDR Post Processing Tone Mapping"),
                    CheckPredicate = IsToneMappingEnabled,
                    FixIt = () => SetToneMappingEnabled(true),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal() && IsHDREnabled() && HasUrpAsset()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_HdrWasEnabled = IsHDREnabled();
                    s_ToneMappingWasEnabled = IsToneMappingEnabled();
                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    SetHDREnabled(true);
                    SetToneMappingEnabled(false);
                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    SetHDREnabled(s_HdrWasEnabled);
                    SetToneMappingEnabled(s_ToneMappingWasEnabled);

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                SkipTest = () => !HasUrpAsset() ? "Skipping Allow HDR Display Output validation test because there is no URP asset set." : null
            },
            new()
            {
                Name = "FP16 Format",
                Rule = new ()
                {
                    // FP16 is the only supported HDR format for builtin that includes alpha
                    Message = "FP16 format is required for passthrough",
                    FixItMessage = "Enable FP16 format",
                    Category = string.Format(k_CategoryFormat, "FP16 Format"),
                    Error = true,
                    CheckPredicate = () => GetTierSettings().hdrMode == CameraHDRMode.FP16,
                    FixIt = () =>
                    {
                        var tierSettings = GetTierSettings();
                        tierSettings.hdrMode = CameraHDRMode.FP16;
                        SetTierSettings(tierSettings);
                    },
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal() && IsHDREnabled()
                        && !HasUrpAsset() && AppSupportsMixedImmersion()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_PreviousImmersionStyle = settings.metalImmersionStyle;
                    s_HdrWasEnabled = IsHDREnabled();
                    s_PreviousTierSettings = GetTierSettings();
                    var newTierSettings = s_PreviousTierSettings;
                    newTierSettings.hdrMode = CameraHDRMode.R11G11B10;
                    newTierSettings.hdr = true;
                    SetTierSettings(newTierSettings);
                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    settings.metalImmersionStyle = VisionOSSettings.ImmersionStyle.Automatic;

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                    {
                        settings.appMode = s_PreviousAppMode;
                        settings.metalImmersionStyle = s_PreviousImmersionStyle;
                    }

                    SetTierSettings(s_PreviousTierSettings);

                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                SkipTest = () => HasUrpAsset() ? "Skipping FP16 Format validation test because URP is in use." : null
            },
            new()
            {
                Name = "Alpha Output",
                Rule = new ()
                {
                    Message = "Alpha output is required for passthrough when using post processing on Universal Render Pipeline.",
                    FixItMessage = "Enable alpha output in project settings and URP asset. You can ignore the warning in the URP asset inspector about camera" +
                        " back-buffer format",
                    Category = string.Format(k_CategoryFormat, "Alpha Output"),
                    Error = true,
                    CheckPredicate = IsAlphaOutputEnabled,
                    FixIt = () => SetAlphaOutputEnabled(true, true),
                    IsRuleEnabled = () => VisionOSEditorUtils.IsLoaderEnabled() && AppModeSupportsMetal() && IsHDREnabled()
                        && AppSupportsMixedImmersion() && IsUrpPostProcessingEnabled()
                },
                SetUp = () =>
                {
                    var message = GetEditorSettingsIfExists(out var settings);
                    if (message != null)
                        return message;

                    s_LoaderWasEnabled = VisionOSEditorUtils.IsLoaderEnabled();
                    message = SetVisionOSLoaderEnabledForTests(true);
                    if (message != null)
                        return message;

                    s_PreviousAppMode = settings.appMode;
                    s_HdrWasEnabled = IsHDREnabled();
                    s_AlphaOutputWasEnabled = PlayerSettings.preserveFramebufferAlpha;

#if UNITY_HAS_URP
                    var asset = UniversalRenderPipeline.asset;
                    if (asset != null)
                        s_UrpPostProcessAlphaOutputWasEnabled = asset.allowPostProcessAlphaOutput;
#endif

                    settings.appMode = VisionOSSettings.AppMode.Metal;
                    SetHDREnabled(true);
                    SetAlphaOutputEnabled(false, false);

                    return null;
                },
                TearDown = () =>
                {
                    var settingsMessage = GetEditorSettingsIfExists(out var settings);
                    if (settings != null)
                        settings.appMode = s_PreviousAppMode;

                    SetHDREnabled(s_HdrWasEnabled);
                    SetAlphaOutputEnabled(s_AlphaOutputWasEnabled, s_UrpPostProcessAlphaOutputWasEnabled);
                    var loaderMessage = SetVisionOSLoaderEnabledForTests(s_LoaderWasEnabled);
                    return HandleMultipleMessagesInTearDown(settingsMessage, loaderMessage);
                },
                SkipTest = () => !HasUrpAsset() ? "Skipping Alpha Output validation test because there is no URP asset set." : null
            },
        };

        static string HandleMultipleMessagesInTearDown(params string[] messages)
        {
            string result = null;
            var length = messages.Length;
            for (var i = 0; i < length; i++)
            {
                var message = messages[i];
                if (message != null)
                {
                    // Return first message if first step failed, log second message in case both steps failed
                    if (result == null)
                        result = message;
                    else
                        Debug.LogError(message);
                }
            }

            return result;
        }
    }
}
