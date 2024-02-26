#if UNITY_EDITOR
using UnityEditor;
using UnityObject = UnityEngine.Object;
#endif // UNITY_EDITOR

namespace UnityEngine.XR.VisionOS
{
#if UNITY_EDITOR
    public interface IPackageSettings
    {
        VisionOSRuntimeSettings GetOrCreateRuntimeSettings();
    }
#endif // UNITY_EDITOR

    /// <summary>
    /// Build time settings for visionOS. These are serialized and available at runtime.
    /// </summary>
    public class VisionOSRuntimeSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Controls if hand tracking should be initialized when the application begins. The XR Hands package (com.unity.xr.hands) is required for hand tracking.")]
        bool m_InitializeHandTrackingOnStartup = true;

        public bool initializeHandTrackingOnStartup
        {
            get => m_InitializeHandTrackingOnStartup;
            set
            {
                m_InitializeHandTrackingOnStartup = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

#if !UNITY_EDITOR
        static VisionOSRuntimeSettings s_Settings;
        void Awake() => s_Settings = this;
#endif // !UNITY_EDITOR

        internal static VisionOSRuntimeSettings GetOrCreate()
        {
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            return GetPackageSettings().GetOrCreateRuntimeSettings();
#else // !UNITY_EDITOR
            if (s_Settings == null)
                s_Settings = CreateInstance<VisionOSRuntimeSettings>();

            return s_Settings;
#endif // !UNITY_EDITOR
        }

#if UNITY_EDITOR
        static IPackageSettings GetPackageSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject<UnityObject>(Constants.k_SettingsKey, out var settings) && settings is IPackageSettings packageSettings)
                return packageSettings;

            return null;
        }
#endif // UNITY_EDITOR
    }
}
