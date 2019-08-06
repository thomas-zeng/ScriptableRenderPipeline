using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Experimental.Rendering.Universal.Path2D;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace UnityEditor.Experimental.Rendering.Universal
{
    [CustomEditor(typeof(LightReactor2D))]
    [CanEditMultipleObjects]
    internal class LightReactor2DEditor : PathComponentEditor<ScriptablePath>
    {
        [EditorTool("Edit Shadow Caster Shape", typeof(LightReactor2D))]
        class LightReactor2DShadowCasterShapeTool : ShadowCaster2DShapeTool { };

        private static class Styles
        {
            public static GUIContent shadowMode = EditorGUIUtility.TrTextContent("Shadow Mode", "Specifies if what will be cast by this light reactor");
            public static GUIContent shadowCasterGroup = EditorGUIUtility.TrTextContent("Shadow Caster Group", "Shadow casters in the same group will not shadow each other");
            public static GUIContent selfShadows = EditorGUIUtility.TrTextContent("Self Shadows", "Specifies if this renderer will cast shadows on itself");
            public static GUIContent castsShadows = EditorGUIUtility.TrTextContent("Casts Shadows", "Specifies if this renderer will cast shadows");
        }

        SerializedProperty m_ShadowMode;
        SerializedProperty m_ShadowCasterGroup;
        SerializedProperty m_SelfShadows;
        SerializedProperty m_CastsShadows;
        SerializedProperty m_ReceivesShadows;
        string[] m_PopupContent;


        public void OnEnable()
        {


            m_ShadowMode = serializedObject.FindProperty("m_ShadowMode");
            m_ShadowCasterGroup = serializedObject.FindProperty("m_ShadowGroup");
            m_SelfShadows = serializedObject.FindProperty("m_SelfShadows");
            m_CastsShadows = serializedObject.FindProperty("m_CastsShadows");

            const int popupElements = 256;
            m_PopupContent = new string[popupElements];
            m_PopupContent[0] = "Auto Assign";
            for(int i=1;i<popupElements;i++)
                m_PopupContent[i] = i.ToString();
        }

        public void ShadowCaster2DSceneGUI()
        {
            LightReactor2D shadowCaster = target as LightReactor2D;

            Transform t = shadowCaster.transform;
            Vector3[] shape = shadowCaster.shapePath;
            Handles.color = Color.white;

            for (int i = 0; i < shape.Length - 1; ++i)
            {
                //Handles.DrawLine(t.TransformPoint(shape[i]), t.TransformPoint(shape[i + 1]));
                Handles.DrawAAPolyLine(4, new Vector3[] { t.TransformPoint(shape[i]), t.TransformPoint(shape[i + 1]) });
            }

            if (shape.Length > 1)
                Handles.DrawAAPolyLine(4, new Vector3[] { t.TransformPoint(shape[shape.Length - 1]), t.TransformPoint(shape[0]) });
        }

        public void ShadowCaster2DInspectorGUI<T>() where T : ShadowCaster2DShapeTool
        {
            DoEditButton<T>(PathEditorToolContents.icon, "Edit Shape");
            DoPathInspector<T>();
            DoSnappingInspector<T>();
        }


        public void OnSceneGUI()
        {
            ShadowCaster2DSceneGUI();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ShadowMode, Styles.shadowMode); // This needs to be changed to a popup later...

            //m_ShadowCasterGroup.intValue = EditorGUILayout.Popup(Styles.shadowCasterGroup, m_ShadowCasterGroup.intValue, m_PopupContent, GUILayout.Height(40));
            EditorGUILayout.PropertyField(m_SelfShadows, Styles.selfShadows);
            if (m_ShadowMode.intValue != (int)LightReactor2D.ShadowModes.RendererOnly)
                EditorGUILayout.PropertyField(m_CastsShadows, Styles.castsShadows);
            serializedObject.ApplyModifiedProperties();

            ShadowCaster2DInspectorGUI<LightReactor2DShadowCasterShapeTool>();
        }
    }
}
