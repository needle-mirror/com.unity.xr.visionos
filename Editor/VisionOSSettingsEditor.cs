namespace UnityEditor.XR.VisionOS
{
    [CustomEditor(typeof(VisionOSSettings))]
    class VisionOSSettingsEditor : Editor
    {
        SerializedProperty m_AppModeProperty;
        SerializedProperty m_DeviceTargetProperty;
        SerializedProperty m_RenderModeProperty;
        SerializedProperty m_VolumeModeProperty;
        SerializedProperty m_VolumeDimensionsProperty;
        
        void OnEnable()
        {
            m_AppModeProperty = serializedObject.FindProperty("m_AppMode");
            m_DeviceTargetProperty = serializedObject.FindProperty("m_DeviceTarget");
            m_RenderModeProperty = serializedObject.FindProperty("m_RenderMode");
            m_VolumeModeProperty = serializedObject.FindProperty("m_VolumeMode");
            m_VolumeDimensionsProperty = serializedObject.FindProperty("m_VolumeDimensions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_DeviceTargetProperty);

                if (check.changed)
                {
                    ((VisionOSSettings) target).SetDeviceTargetAndUpdateRenderMode((VisionOSSettings.DeviceTarget) m_DeviceTargetProperty.enumValueIndex);
                    serializedObject.Update();
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_RenderModeProperty);
            }
            
            EditorGUILayout.PropertyField(m_AppModeProperty);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mixed Reality settings", EditorStyles.boldLabel);

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
