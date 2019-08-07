using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    [CustomEditor(typeof(CustomPassVolume))]
    sealed class CustomPassVolumeEditor : Editor
    {
        ReorderableList         m_CustomPassList;
        string                  m_ListName;
        CustomPassVolume        m_Volume;

        const string            k_DefaultListName = "Custom Passes";

        static class Styles
        {
            public static readonly GUIContent isGlobal = new GUIContent("Is Global", "Is the volume for the entire scene.");
            public static readonly GUIContent injectionPoint = new GUIContent("Injection Point", "Where the pass is going to be executed in the pipeline.");
        }

        class SerializedPassVolume
        {
            public SerializedProperty   isGlobal;
            public SerializedProperty   customPasses;
            public SerializedProperty   injectionPoint;
        }

        SerializedObject        m_serializedPassVolumeObject;
        SerializedPassVolume    m_SerializedPassVolume;

        void OnEnable()
        {
            m_Volume = target as CustomPassVolume;

            m_serializedPassVolumeObject = new SerializedObject(targets);

            using (var o = new PropertyFetcher<CustomPassVolume>(m_serializedPassVolumeObject))
            {
                m_SerializedPassVolume = new SerializedPassVolume
                {
                    isGlobal = o.Find(x => x.isGlobal),
                    injectionPoint = o.Find(x => x.injectionPoint),
                    customPasses = o.Find(x => x.customPasses),
                };
            }
            
            CreateReorderableList(m_SerializedPassVolume.customPasses);
        }

        public override void OnInspectorGUI()
        {
            DrawSettingsGUI();
            DrawCustomPassReorderableList();

            if (GUI.changed)
            {
                m_serializedPassVolumeObject.ApplyModifiedProperties();
            }
        }

        void DrawSettingsGUI()
        {
            EditorGUILayout.PropertyField(m_SerializedPassVolume.isGlobal, Styles.isGlobal);
            EditorGUILayout.PropertyField(m_SerializedPassVolume.injectionPoint, Styles.injectionPoint);
        }

        void DrawCustomPassReorderableList()
        {
            EditorGUILayout.BeginVertical();
            m_CustomPassList.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        void CreateReorderableList(SerializedProperty passList)
        {
            m_CustomPassList = new ReorderableList(passList.serializedObject, passList);

            m_CustomPassList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, k_DefaultListName, EditorStyles.largeLabel);
            };

            m_CustomPassList.drawElementCallback = (rect, index, active, focused) => {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, passList.GetArrayElementAtIndex(index), true);
                if (EditorGUI.EndChangeCheck())
                    m_serializedPassVolumeObject.ApplyModifiedProperties();
            };

            m_CustomPassList.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(passList.GetArrayElementAtIndex(index));
        }

        float GetCustomPassEditorHeight(SerializedProperty pass)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
    }
}