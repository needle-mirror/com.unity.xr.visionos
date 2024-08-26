using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.VisionOS;

#if UNITY_HAS_POLYSPATIAL
using Unity.PolySpatial;
using UnityEditor.PolySpatial;
using UnityEditor.PolySpatial.Internals;
#endif

namespace UnityEditor.XR.VisionOS
{
    /// <summary>
    /// Holds settings that are used to configure the Apple visionOS XR Plug-in.
    /// </summary>
    [Serializable]
    [XRConfigurationData("Apple visionOS", Constants.k_SettingsKey)]
    public class VisionOSSettings : ScriptableObject, IPackageSettings
    {
        const string k_HandTrackingUsageTooltip = "Provide a brief description of what hand tracking will be used for." +
            "This will be shown to users in a dialog asking them to allow authorization when hand tracking is requested." +
            "This description will be added to the Info.plist file of the generated visionOS Player Xcode project. If your " +
            "application does not use hand tracking, you can safely leave this field blank.";

        const string k_WorldSensingUsageTooltip = "Provide a brief description of what world sensing (planes, meshes, images)" +
            "will be used for. This will be shown to users in a dialog asking them to allow authorization when world sensing" +
            " is requested. This description will be added to the Info.plist file of the generated visionOS Player Xcode project. If your " +
            "application does not use world sensing, you can safely leave this field blank.";

        // TODO: Update this tooltip when we support volumes and immersive spaces simultaneously. In this case, tis setting will affect volume content
        // as long as the immersive space is open. In other words, when limb visibility is disabled, and the immersive space is open, the user's hands
        // will be occluded by content in a volume even though they can see pass-through.
        // TODO: Expose UpperLimbVisibility for VR and MR separately (should probably be in volume camera config, along with immersion style)
        const string k_UpperLimbVisibilityTooltip = "Controls how your app displays passthrough video of the user's hands and forearms. In the " +
            "Virtual Reality App Mode, hands are only shown when you enable this setting and always render on top of virtual content. In the " +
            "Mixed Reality App Mode, this setting controls how the user's hands and arms are blended with virtual objects. When enabled, hands " +
            "are blended with virtual objects based on depth. When disabled, hands are always displayed behind virtual objects. This setting is " +
            "ignored when the Volume Camera is in Bounded mode, or when the App Mode is set to Windowed.";

        const string k_FoveatedRenderingTooltip = "Controls if foveated rendering is enabled or disabled. This setting only applies to Virtual Reality apps. " +
            "Foveated rendering requires the Universal Render Pipeline.";

        const string k_VRImmersionStyleTooltip = "The ImmersionStyle to be used for the VR ImmersiveSpace.";
        const string k_MRImmersionStyleTooltip = "The ImmersionStyle to be used for the MR ImmersiveSpace.";

        const string k_IL2CPPLargeExeWorkaroundTooltip = "Patches the `Unity-VisionOS` project to work around linker errors that can occur in some " +
            "large projects. Check this box if you encounter the \"ARM64 branch out of range\" error when building the project in Xcode.";

        const string k_SkipPresentToMainScreenTooltip = "Enables the SkipPresentToMainScreen XR setting. This setting was previously disabled to work around" +
            "an issue that caused Unity versions prior to 2022.3.42f1 to leak GPU resources. In Unity 2022.3.42f1 and above, enabling this setting will fix" +
            "a known issue with frame pacing on visionOS. Do not modify this setting from its default value unless you have a specific reason. It should " +
            "be enabled on Unity 2022.3.42f1 and above, and disabled otherwise.";

        const string k_RuntimeSettingsFileName = "VisionOS Runtime Settings.asset";

        /// <summary>
        /// Which mode the app will use: MR or VR.
        /// </summary>
        public enum AppMode
        {
            /// <summary>
            /// Virtual Reality - Full Immersive Space
            /// </summary>
            [InspectorName("Virtual Reality - Fully Immersive Space")]
            VR,

            /// <summary>
            /// Mixed Reality - Volume or Immersive Space
            /// </summary>
            [InspectorName("Mixed Reality - Volume or Immersive Space")]
            MR,

            /// <summary>
            /// Windowed - 2D Window
            /// </summary>
            [InspectorName("Windowed - 2D Window")]
            Windowed
        }

        /// <summary>
        /// The ImmersionStyle for a given ImmersiveSpace. These enums correspond to their equivalently named Apple APIs.
        /// Refer to Apple Developer documentation for more information.
        /// </summary>
        public enum ImmersionStyle
        {
            /// <summary>
            /// The default immersion style. It currently defaults to Mixed.
            /// </summary>
            Automatic,

            /// <summary>
            /// Displays unbounded content that completely replaces passthrough.
            /// </summary>
            Full,

            /// <summary>
            /// Displays unbounded content mixed with passthrough.
            /// </summary>
            Mixed,

            /// <summary>
            /// Displays unbounded content that partially replaces passthrough.
            /// </summary>
            Progressive
        }

        [SerializeField, Tooltip("Initial mode of the app.")]
        AppMode m_AppMode = AppMode.VR;

        [SerializeField, Tooltip(k_HandTrackingUsageTooltip)]
        string m_HandsTrackingUsageDescription;

        [SerializeField, Tooltip(k_WorldSensingUsageTooltip)]
        string m_WorldSensingUsageDescription;

        [SerializeField, HideInInspector]
        VisionOSRuntimeSettings m_RuntimeSettings;

        public VisionOSRuntimeSettings GetOrCreateRuntimeSettings()
        {
            if (m_RuntimeSettings != null)
                return m_RuntimeSettings;

            var assetGuids = AssetDatabase.FindAssets($"t:{nameof(VisionOSRuntimeSettings)}");
            foreach (var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var asset = AssetDatabase.LoadAssetAtPath<VisionOSRuntimeSettings>(assetPath);
                if (asset != null)
                {
                    m_RuntimeSettings = asset;
                    break;
                }
            }

            if (m_RuntimeSettings != null)
                return m_RuntimeSettings;

            var path = GetAssetPathForSubFolders(new[] { "XR", "Settings" });
            if (string.IsNullOrEmpty(path))
                return null;

            m_RuntimeSettings = CreateInstance<VisionOSRuntimeSettings>();
            var targetPath = Path.Combine(path, k_RuntimeSettingsFileName);
            AssetDatabase.CreateAsset(m_RuntimeSettings, targetPath);
            AssetDatabase.SaveAssets();
            return m_RuntimeSettings;
        }

        [SerializeField, Tooltip(k_UpperLimbVisibilityTooltip)]
        bool m_UpperLimbVisibility;

        [SerializeField, Tooltip(k_FoveatedRenderingTooltip)]
        bool m_FoveatedRendering = true;

        [SerializeField, Tooltip(k_VRImmersionStyleTooltip)]
        ImmersionStyle m_VRImmersionStyle;

        [SerializeField, Tooltip(k_MRImmersionStyleTooltip)]
        ImmersionStyle m_MRImmersionStyle;

        [SerializeField, Tooltip(k_IL2CPPLargeExeWorkaroundTooltip)]
        bool m_IL2CPPLargeExeWorkaround;

        [SerializeField, Tooltip(k_SkipPresentToMainScreenTooltip)]
#if UNITY_2022_3_42_OR_NEWER
        bool m_SkipPresentToMainScreen = true;
#else
        bool m_SkipPresentToMainScreen;
#endif

        /// <summary>
        /// App mode.
        /// </summary>
        public AppMode appMode
        {
            get => m_AppMode;
            set => m_AppMode = value;
        }

        /// <summary>
        /// Hands tracking usage description (added to Info.plist).
        /// </summary>
        public string handsTrackingUsageDescription
        {
            get => m_HandsTrackingUsageDescription;
            set => m_HandsTrackingUsageDescription = value;
        }

        /// <summary>
        /// World sensing usage description (added to Info.plist).
        /// </summary>
        public string worldSensingUsageDescription
        {
            get => m_WorldSensingUsageDescription;
            set => m_WorldSensingUsageDescription = value;
        }

        /// <summary>
        /// Upper limb visibility setting (currently only set at the beginning of an app)
        /// </summary>
        public bool upperLimbVisibility
        {
            get => m_UpperLimbVisibility;
            set => m_UpperLimbVisibility = value;
        }

        /// <summary>
        /// Controls if foveated rendering is enabled or disabled. This setting only applies to Virtual Reality apps. Foveated rendering requires
        /// the Universal Render Pipeline.
        /// </summary>
        public bool foveatedRendering
        {
            get => m_FoveatedRendering;
            set => m_FoveatedRendering = value;
        }

        /// <summary>
        /// The ImmersionStyle to be used for the VR ImmersiveSpace.
        /// </summary>
        public ImmersionStyle vrImmersionStyle
        {
            get => m_VRImmersionStyle;
            set => m_VRImmersionStyle = value;
        }

        /// <summary>
        /// The ImmersionStyle to be used for the MR ImmersiveSpace.
        /// </summary>
        public ImmersionStyle mrImmersionStyle
        {
            get => m_MRImmersionStyle;
            set => m_MRImmersionStyle = value;
        }

        /// <summary>
        /// Setting that determines if the IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND flag is used when building an Xcode project.
        /// </summary>
        public bool il2CPPLargeExeWorkaround
        {
            get => m_IL2CPPLargeExeWorkaround;
            set => m_IL2CPPLargeExeWorkaround = value;
        }

        /// <summary>
        /// XR setting to signal to Unity that it should not try to present frames to the main screen. This setting was previously disabled to work around
        /// an issue that caused Unity versions prior to 2022.3.42f1 to leak GPU resources. In Unity 2022.3.42f1 and above, enabling this setting will fix
        /// a known issue with frame pacing on visionOS. Do not modify this setting from its default value unless you have a specific reason. It should
        /// be enabled on Unity 2022.3.42f1 and above, and disabled otherwise.
        /// </summary>
        public bool skipPresentToMainScreen
        {
            get => m_SkipPresentToMainScreen;
            set => m_SkipPresentToMainScreen = value;
        }

        /// <summary>
        /// Gets the currently selected settings, or creates default settings if no <see cref="VisionOSSettings"/> have been set in Player Settings.
        /// </summary>
        /// <returns>The visionOS settings to use for the current Player build.</returns>
        public static VisionOSSettings GetOrCreateSettings()
        {
            var settings = currentSettings;
            if (settings != null)
                return settings;

            settings = CreateInstance<VisionOSSettings>();
            settings.m_RuntimeSettings = settings.GetOrCreateRuntimeSettings();
            return settings;
        }

        /// <summary>
        /// Get or set the <see cref="VisionOSSettings"/> to use for the Player build.
        /// </summary>
        public static VisionOSSettings currentSettings
        {
            get => EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out VisionOSSettings settings) ? settings : null;

            set
            {
                if (value == null)
                {
                    EditorBuildSettings.RemoveConfigObject(Constants.k_SettingsKey);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(Constants.k_SettingsKey, value, true);
                }
            }
        }

        internal static bool TrySelect()
        {
            var settings = currentSettings;
            if (settings == null)
                return false;

            Selection.activeObject = settings;
            return true;
        }

        internal static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());

        static string GetAssetPathForSubFolders(string[] subFolders)
        {
            if (subFolders.Length <= 0)
                return null;

            var parentFolder = "Assets";
            foreach (var pathComponent in subFolders)
            {
                var fullPath = Path.Combine(parentFolder, pathComponent);
                var shouldCreate = true;
                foreach (var folder in AssetDatabase.GetSubFolders(parentFolder))
                {
                    if (string.Compare(FileUtil.GetPhysicalPath(folder), FileUtil.GetPhysicalPath(fullPath), true) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(parentFolder, pathComponent);

                parentFolder = fullPath;
            }

            return parentFolder;
        }

        /// <summary>
        /// Convert the upperLimbVisibility setting to a string that can be written into Swift code generated during the build process.
        /// </summary>
        /// <param name="upperLimbVisibility">The state of the upper limb visibility setting.</param>
        /// <returns>Swift code that can be used for build postprocessors.</returns>
        public static string UpperLimbVisibilityToString(bool upperLimbVisibility)
        {
            return upperLimbVisibility ? ".visible" : ".hidden";
        }

        /// <summary>
        /// Convert the immersionStyle setting to a string that can be written into Swift code generated during the build process.
        /// </summary>
        /// <param name="immersionStyle">The state of the immersion style setting.</param>
        /// <returns>Swift code that can be used for build postprocessors.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value of immersionStyle isn't one of the known values (Automatic, Full, Mixed, or Progressive)</exception>
        public static string ImmersionStyleToString(ImmersionStyle immersionStyle)
        {
            switch (immersionStyle)
            {
                case ImmersionStyle.Automatic:
                    return ".automatic";
                case ImmersionStyle.Full:
                    return ".full";
                case ImmersionStyle.Mixed:
                    return ".mixed";
                case ImmersionStyle.Progressive:
                    return ".progressive";
                default:
                    throw new ArgumentOutOfRangeException(nameof(immersionStyle), immersionStyle, null);
            }
        }
    }
}
