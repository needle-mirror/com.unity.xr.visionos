using UnityEngine;

namespace UnityEditor.XR.VisionOS
{
    [CustomEditor(typeof(VisionOSSettings))]
    class VisionOSSettingsEditor : Editor
    {
        const string k_WorldSensingUsageWarning = "World sensing usage description is required if world sensing features " +
            "(images, planes, or meshes) will be used. If this field is blank, the app will not be allowed to request " +
            "world sensing authorization, and these features will not work. If your app does not use world sensing, " +
            "you can safely leave this field blank.";

        SerializedProperty m_AppModeProperty;
        SerializedProperty m_HandsTrackingUsageDescriptionProperty;
        SerializedProperty m_WorldSensingUsageDescriptionProperty;

        void OnEnable()
        {
            m_AppModeProperty = serializedObject.FindProperty("m_AppMode");
            m_HandsTrackingUsageDescriptionProperty = serializedObject.FindProperty("m_HandsTrackingUsageDescription");
            m_WorldSensingUsageDescriptionProperty = serializedObject.FindProperty("m_WorldSensingUsageDescription");
        }

        public override void OnInspectorGUI()
        {
            var isLoaderEnabled = VisionOSBuildProcessor.IsLoaderEnabled();

            serializedObject.Update();
            EditorGUIUtility.labelWidth = 200;

            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

            // TODO: Force app mode to VR if PolySpatial packages are missing
            EditorGUILayout.PropertyField(m_AppModeProperty);
            var appMode = (VisionOSSettings.AppMode)m_AppModeProperty.intValue;

            void ShowUsageDescriptionFields()
            {
                // TODO: Enable/disable hand tracking
                EditorGUILayout.PropertyField(m_HandsTrackingUsageDescriptionProperty);
                if (isLoaderEnabled && string.IsNullOrEmpty(m_HandsTrackingUsageDescriptionProperty.stringValue))
                    EditorGUILayout.HelpBox(VisionOSBuildProcessor.HandTrackingUsageWarning, MessageType.Error);

                EditorGUILayout.PropertyField(m_WorldSensingUsageDescriptionProperty);
                if (isLoaderEnabled && string.IsNullOrEmpty(m_WorldSensingUsageDescriptionProperty.stringValue))
                    EditorGUILayout.HelpBox(k_WorldSensingUsageWarning, MessageType.Warning);
            }

            if (appMode == VisionOSSettings.AppMode.VR)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Virtual Reality settings", EditorStyles.boldLabel);

                ShowUsageDescriptionFields();

                switch (PlayerSettings.VisionOS.sdkVersion)
                {
                    case VisionOSSdkVersion.Device:
                        EditorGUILayout.HelpBox("When building for visionOS Device SDK, Single-Pass Instanced rendering will be used.", MessageType.Info);
                        break;
                    case VisionOSSdkVersion.Simulator:
                        EditorGUILayout.HelpBox("When building for visionOS Simulator SDK, Multi-Pass rendering will be used.", MessageType.Info);
                        break;
                }
            }
            else if (appMode == VisionOSSettings.AppMode.MR)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mixed Reality settings", EditorStyles.boldLabel);

#if UNITY_HAS_POLYSPATIAL_VISIONOS
                ShowUsageDescriptionFields();

                EditorGUILayout.HelpBox(
                    "The initial window configuration at app launch is determined by the default volume settings, found in Project Settings > PolySpatial Settings.",
                    MessageType.Info);
#else
                EditorGUILayout.HelpBox("Mixed Reality on visionOS requires PolySpatial and the PolySpatial visionOS packages.", MessageType.Error);
#endif
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
