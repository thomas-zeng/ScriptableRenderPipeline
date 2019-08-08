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

        Dictionary<Object, SerializedProperty> m_CachedSettingsProperties = new Dictionary<Object, SerializedProperty>();

        SerializedPassVolume    m_SerializedPassVolume;

        void OnEnable()
        {
            m_Volume = target as CustomPassVolume;

            using (var o = new PropertyFetcher<CustomPassVolume>(serializedObject))
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
        }

        SerializedProperty GetCustomPassProperty(SerializedProperty passList, int index)
        {
            var customPass = passList.GetArrayElementAtIndex(index).objectReferenceValue;
            SerializedProperty  property;

            if (!m_CachedSettingsProperties.TryGetValue(customPass, out property))
            {
                property = m_CachedSettingsProperties[customPass] = new SerializedObject(customPass).FindProperty("settings");
            }

            property.serializedObject.Update();
            return property;
        }

        void DrawSettingsGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(m_SerializedPassVolume.isGlobal, Styles.isGlobal);
                EditorGUILayout.PropertyField(m_SerializedPassVolume.injectionPoint, Styles.injectionPoint);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        void DrawCustomPassReorderableList()
        {
            // Sanitize list:
            for (int i = 0; i < m_SerializedPassVolume.customPasses.arraySize; i++)
            {
                if (m_SerializedPassVolume.customPasses.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    m_SerializedPassVolume.customPasses.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    i++;
                }
            }

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
                
                var customPass = passList.GetArrayElementAtIndex(index).objectReferenceValue;
                var serializedPass = GetCustomPassProperty(passList, index);
                EditorGUI.PropertyField(rect, serializedPass, true);
                if (EditorGUI.EndChangeCheck())
                    serializedPass.serializedObject.ApplyModifiedProperties();
            };

            m_CustomPassList.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(GetCustomPassProperty(passList, index));

            m_CustomPassList.onAddCallback += (list) => {
				var customPass = ScriptableObject.CreateInstance< CustomPass >();
                Undo.RegisterCreatedObjectUndo(customPass, "Create new Custom pass");
				passList.arraySize++;
				passList.GetArrayElementAtIndex(list.count - 1).objectReferenceValue = customPass;
				passList.serializedObject.ApplyModifiedProperties();
			};

            m_CustomPassList.onRemoveCallback = (list) => {
                var customPass = passList.GetArrayElementAtIndex(list.index).objectReferenceValue;
                Undo.DestroyObjectImmediate(customPass);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                passList.DeleteArrayElementAtIndex(list.index);
                passList.serializedObject.ApplyModifiedProperties();
            };
        }

        float GetCustomPassEditorHeight(SerializedProperty pass)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}