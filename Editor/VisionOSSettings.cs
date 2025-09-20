using System;
using UnityEngine;
using UnityEngine.XR.Management;
using RenderMode = UnityEngine.XR.VisionOS.RenderMode;

namespace UnityEditor.XR.VisionOS
{
    /// <summary>
    /// Holds settings that are used to configure the Apple visionOS XR Plug-in.
    /// </summary>
    [Serializable]
    [XRConfigurationData("Apple visionOS", "UnityEditor.XR.VisionOS.VisionOSSettings")]
    public class VisionOSSettings : ScriptableObject
    {
        const string k_SettingsKey = "UnityEditor.XR.VisionOS.VisionOSSettings";
        
        /// <summary>
        /// Type of device to target.
        /// </summary>
        public enum DeviceTarget
        {
            /// <summary>
            /// Device
            /// </summary>
            Device,
            /// <summary>
            /// Simulator
            /// </summary>
            Simulator,
        }

        /// <summary>
        /// Device to target.
        /// </summary>
        [SerializeField, Tooltip("Device to target.")]
        DeviceTarget m_DeviceTarget;

        /// <summary>
        /// Targeted device.
        /// </summary>
        public DeviceTarget deviceTarget => m_DeviceTarget;

        /// <summary>
        /// Stereo rendering mode.
        /// </summary>
        [SerializeField, Tooltip("Stereo rendering mode.")]
        RenderMode m_RenderMode = RenderMode.SinglePassInstanced;

        public enum AppMode
        {
            /// <summary>
            /// Mixed Reality / Volume & Immersive Space
            /// </summary>
            [InspectorName("Mixed Reality - Volume or Immersive Space")]
            MR,
            /// <summary>
            /// Virtual Reality / Full Immersive Space
            /// </summary>
            [InspectorName("Virtual Reality - Fully Immersive Space")]
            VR
        }

        [SerializeField, Tooltip("Initial mode of the app.")]
        AppMode m_AppMode = AppMode.MR;

        /// <summary>
        /// Volume Mode.
        /// This should match QuantumVolumeCameraMode
        /// </summary>
        public enum VolumeMode
        {
            Bounded,
            Unbounded
        }

        [SerializeField] 
        VolumeMode m_VolumeMode = VolumeMode.Bounded;

        [SerializeField] 
        Vector3 m_VolumeDimensions = Vector3.one;
        
        /// <summary>
        /// Stereo rendering mode.
        /// </summary>
        public RenderMode renderMode
        {
            get => m_RenderMode;
            set => m_RenderMode = value;
        }
        
        /// <summary>
        /// App mode.
        /// </summary>
        public AppMode appMode
        {
            get => m_AppMode;
            set => m_AppMode = value;
        }
        
        /// <summary>
        /// Volume mode.
        /// </summary>
        public VolumeMode volumeMode
        {
            get => m_VolumeMode;
            set => m_VolumeMode = value;
        }
        
        /// <summary>
        /// Volume mode.
        /// </summary>
        public Vector3 volumeDimensions
        {
            get => m_VolumeDimensions;
            set => m_VolumeDimensions = value;
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

            return CreateInstance<VisionOSSettings>();
        }

        /// <summary>
        /// Get or set the <see cref="VisionOSSettings"/> to use for the Player build.
        /// </summary>
        public static VisionOSSettings currentSettings
        {
            get => EditorBuildSettings.TryGetConfigObject(k_SettingsKey, out VisionOSSettings settings) ? settings : null;

            set
            {
                if (value == null)
                {
                    EditorBuildSettings.RemoveConfigObject(k_SettingsKey);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(k_SettingsKey, value, true);
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

        /// <summary>
        /// Set the target device, which will also update the <see cref="renderMode"/> accordingly.
        /// </summary>
        /// <param name="target">Device to target</param>
        public void SetDeviceTargetAndUpdateRenderMode(DeviceTarget target)
        {
            m_DeviceTarget = target;

            // SinglePass is required on device, while MultiPass is required in Simulator. 
            renderMode = m_DeviceTarget == DeviceTarget.Device 
                ? RenderMode.SinglePassInstanced : RenderMode.MultiPass;
        }
    }
}
