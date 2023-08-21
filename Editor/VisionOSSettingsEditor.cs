using UnityEngine;
using RenderMode = UnityEngine.XR.VisionOS.RenderMode;

namespace UnityEditor.XR.VisionOS
{
    [CustomEditor(typeof(VisionOSSettings))]
    class VisionOSSettingsEditor : Editor
    {
        const string k_WorldSensingUsageWarning = "World sensing usage description is required if world sensing features " +
            "(images, planes, or meshes) will be used. If this field is blank, the app will not be allowed to request " +
            "world sensing authorization, and these features will not work. If your app does not use world sensing, " +
            "you can safely ignore this warning.";
        
        SerializedProperty m_AppModeProperty;
        SerializedProperty m_DeviceTargetProperty;
        SerializedProperty m_RenderModeProperty;
        SerializedProperty m_VolumeModeProperty;
        SerializedProperty m_VolumeDimensionsProperty;
        SerializedProperty m_HandsTrackingUsageDescriptionProperty;
        SerializedProperty m_WorldSensingUsageDescriptionProperty;
        
        void OnEnable()
        {
            m_AppModeProperty = serializedObject.FindProperty("m_AppMode");
            m_DeviceTargetProperty = serializedObject.FindProperty("m_DeviceTarget");
            m_RenderModeProperty = serializedObject.FindProperty("m_RenderMode");
            m_VolumeModeProperty = serializedObject.FindProperty("m_VolumeMode");
            m_VolumeDimensionsProperty = serializedObject.FindProperty("m_VolumeDimensions");
            m_HandsTrackingUsageDescriptionProperty = serializedObject.FindProperty("m_HandsTrackingUsageDescription");
            m_WorldSensingUsageDescriptionProperty = serializedObject.FindProperty("m_WorldSensingUsageDescription");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = 200;
            
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

            var isLoaderEnabled = VisionOSBuildProcessor.IsLoaderEnabled();
            using (new EditorGUI.DisabledScope(!isLoaderEnabled))
            {
                EditorGUILayout.PropertyField(m_HandsTrackingUsageDescriptionProperty);

                // TODO: Enable/disable hand tracking
                if (isLoaderEnabled && string.IsNullOrEmpty(m_HandsTrackingUsageDescriptionProperty.stringValue))
                    EditorGUILayout.HelpBox(VisionOSBuildProcessor.HandTrackingUsageWarning, MessageType.Warning);

                EditorGUILayout.PropertyField(m_WorldSensingUsageDescriptionProperty);
                if (isLoaderEnabled && string.IsNullOrEmpty(m_WorldSensingUsageDescriptionProperty.stringValue))
                    EditorGUILayout.HelpBox(k_WorldSensingUsageWarning, MessageType.Warning);
            }
            
            // TODO: Force app mode to VR if polyspatial packages are missing
            EditorGUILayout.PropertyField(m_AppModeProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Virtual Reality Settings", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(m_AppModeProperty.enumValueIndex != (int)VisionOSSettings.AppMode.VR))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.PropertyField(m_DeviceTargetProperty);

                    if (check.changed)
                    {
                        var mode = (VisionOSSettings.DeviceTarget)m_DeviceTargetProperty.intValue;
                        switch (mode)
                        {
                            case VisionOSSettings.DeviceTarget.Device:
                                m_RenderModeProperty.intValue = (int)RenderMode.SinglePassInstanced;
                                break;
                            case VisionOSSettings.DeviceTarget.Simulator:
                                m_RenderModeProperty.intValue = (int)RenderMode.MultiPass;
                                break;
                            default:
                                Debug.LogError($"Unsupported device mode {mode}");
                                break;
                        }
                    }
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_RenderModeProperty);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mixed Reality Settings", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(m_AppModeProperty.enumValueIndex != (int) VisionOSSettings.AppMode.MR))
            {
                EditorGUILayout.PropertyField(m_VolumeModeProperty);

                using (new EditorGUI.DisabledScope(m_VolumeModeProperty.enumValueIndex == (int)VisionOSSettings.VolumeMode.Unbounded))
                {
                    EditorGUILayout.PropertyField(m_VolumeDimensionsProperty);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
