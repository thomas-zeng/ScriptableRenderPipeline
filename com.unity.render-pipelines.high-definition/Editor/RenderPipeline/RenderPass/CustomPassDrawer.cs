using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;

namespace UnityEditor.Rendering.HighDefinition
{
	[CustomPropertyDrawer(typeof(CustomPass), true)]
    class CustomPassDrawer : PropertyDrawer
    {
	    internal class Styles
	    {
		    public static float defaultLineSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            public static float defaultIndentWidth = 12;
		    public static GUIContent callback = new GUIContent("Event", "Chose the Callback position for this render pass object.");

		    //Headers
		    public static GUIContent filtersHeader = new GUIContent("Filters", "Filters.");
		    public static GUIContent renderHeader = new GUIContent("Overrides", "Different parts fo the rendering that you can choose to override.");
		    
		    //Filters
		    public static GUIContent renderQueueFilter = new GUIContent("Queue", "Filter the render queue range you want to render.");
		    public static GUIContent layerMask = new GUIContent("Layer Mask", "Chose the Callback position for this render pass object.");
		    public static GUIContent shaderPassFilter = new GUIContent("Shader Passes", "Chose the Callback position for this render pass object.");
		    
		    //Render Options
		    public static GUIContent overrideMaterial = new GUIContent("Material", "Chose an override material, every renderer will be rendered with this material.");
		    public static GUIContent overrideMaterialPass = new GUIContent("Pass Index", "The pass index for the override material to use.");
		    
		    //Depth Settings
		    public static GUIContent overrideDepth = new GUIContent("Depth", "Override depth rendering.");
		    public static GUIContent writeDepth = new GUIContent("Write Depth", "Chose to write depth to the screen.");
		    public static GUIContent depthState = new GUIContent("Depth Test", "Choose a new test setting for the depth.");

		    //Camera Settings
		    public static GUIContent overrideCamera = new GUIContent("Camera", "Override camera projections.");
		    public static GUIContent cameraFOV = new GUIContent("Field Of View", "Field Of View to render this pass in.");
		    public static GUIContent positionOffset = new GUIContent("Position Offset", "This Vector acts as a relative offset for the camera.");
		    public static GUIContent restoreCamera = new GUIContent("Restore", "Restore to the original camera projection before this pass.");
	    }

	    //Headers and layout
	    private int m_FilterLines = 3;
	    private int m_MaterialLines = 2;
	    private int m_DepthLines = 3;
	    
	    private bool firstTime = true;

	    // Serialized Properties
		SerializedProperty      m_Name;
		SerializedProperty      m_Type;

		// Foldouts
		SerializedProperty      m_FilterFoldout;
		SerializedProperty      m_RendererFoldout;
		SerializedProperty      m_PassFoldout;

		// Filter
		SerializedProperty      m_FilterSettings;
		SerializedProperty      m_RenderQueue;
		SerializedProperty      m_LayerMask;
		SerializedProperty      m_ShaderPasses;

		// Render
		SerializedProperty      m_OverrideMaterial;
		SerializedProperty      m_OverrideMaterialPass;

		// Depth
		SerializedProperty      m_OverrideDepth;
		SerializedProperty      m_WriteDepth;
		SerializedProperty      m_DepthCompareFunction;

		// Fullscreen pass
		SerializedProperty		m_FullScreenPassMaterial;

	    private ReorderableList m_ShaderPassesList;

		void FetchPorperties(SerializedProperty property)
		{
			m_Name = property.FindPropertyRelative("name");
			m_Type = property.FindPropertyRelative("type");

		    // Header bools
			m_FilterFoldout = property.FindPropertyRelative("filterFoldout");
			m_RendererFoldout = property.FindPropertyRelative("rendererFoldout");
			m_PassFoldout = property.FindPropertyRelative("passFoldout");

		    // Filter props
		    m_FilterSettings = property.FindPropertyRelative("filterSettings");
		    m_RenderQueue = m_FilterSettings.FindPropertyRelative("renderQueueType");
		    m_LayerMask = m_FilterSettings.FindPropertyRelative("layerMask");
		    m_ShaderPasses = m_FilterSettings.FindPropertyRelative("passNames");

			// Render options
		    m_OverrideMaterial = property.FindPropertyRelative("overrideMaterial");
		    m_OverrideMaterialPass = property.FindPropertyRelative("overrideMaterialPassIndex");
			
		    // Depth props
		    m_OverrideDepth = property.FindPropertyRelative("overrideDepth");
		    m_WriteDepth = property.FindPropertyRelative("writeDepth");
		    m_DepthCompareFunction = property.FindPropertyRelative("depthCompareFunction");

			// FullScreen pass
			m_FullScreenPassMaterial = property.FindPropertyRelative("fullscreenPassMaterial");
		}

	    private void Init(SerializedProperty property)
	    {
		    m_ShaderPassesList = new ReorderableList(null, m_ShaderPasses, true, true, true, true);

		    m_ShaderPassesList.drawElementCallback =
		    (Rect rect, int index, bool isActive, bool isFocused) =>
		    {
			    var element = m_ShaderPassesList.serializedProperty.GetArrayElementAtIndex(index);
			    var propRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			    var labelWidth = EditorGUIUtility.labelWidth;
			    EditorGUIUtility.labelWidth = 50;
			    element.stringValue = EditorGUI.TextField(propRect, "Name", element.stringValue);
			    EditorGUIUtility.labelWidth = labelWidth;
		    };
		    
		    m_ShaderPassesList.drawHeaderCallback = (Rect testHeaderRect) => {
			    EditorGUI.LabelField(testHeaderRect, Styles.shaderPassFilter);
		    };
		    
		    firstTime = false;
	    }

	    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	    {
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.BeginChangeCheck();
			EditorGUI.BeginProperty(rect, label, property);

			FetchPorperties(property);

			if (firstTime)
			    Init(property);
			
			var headerRect = new Rect(rect.x + Styles.defaultIndentWidth,
							rect.y + EditorGUIUtility.standardVerticalSpacing,
							rect.width - Styles.defaultIndentWidth,
							EditorGUIUtility.singleLineHeight);
			rect.y += Styles.defaultLineSpace;

			m_PassFoldout.boolValue = EditorGUI.Foldout(headerRect, m_PassFoldout.boolValue, m_Name.stringValue, true, EditorStyles.boldLabel);

			if (m_PassFoldout.boolValue)
				return;

			EditorGUI.PropertyField(rect, m_Name);
			rect.y += Styles.defaultLineSpace;
			
			EditorGUI.PropertyField(rect, m_Type);
			rect.y += Styles.defaultLineSpace;
		
			CustomPassType	passType = (CustomPassType)m_Type.enumValueIndex;

			if (passType == CustomPassType.Renderers)
				DoRenderersGUI(property, rect);
			else
				DoFullScreenGUI(rect);
	    }

		void DoFullScreenGUI(Rect rect)
		{
			EditorGUI.PropertyField(rect, m_FullScreenPassMaterial);
		}

		void DoRenderersGUI(SerializedProperty property, Rect rect)
		{
			DoFilters(ref rect);

			m_RendererFoldout.boolValue = EditorGUI.Foldout(rect, m_RendererFoldout.boolValue, Styles.renderHeader, true);
			rect.y += Styles.defaultLineSpace;
			if (m_RendererFoldout.boolValue)
			{
				EditorGUI.indentLevel++;
				//Override material
				DoMaterialOverride(ref rect);
				rect.y += Styles.defaultLineSpace;
				//Override depth
				DoDepthOverride(ref rect);
				rect.y += Styles.defaultLineSpace;

				EditorGUI.indentLevel--;
			}
			
			EditorGUI.EndProperty();
			if (EditorGUI.EndChangeCheck())
				property.serializedObject.ApplyModifiedProperties();
		}

	    void DoFilters(ref Rect rect)
	    {
		    m_FilterFoldout.boolValue = EditorGUI.Foldout(rect, m_FilterFoldout.boolValue, Styles.filtersHeader, true);
		    rect.y += Styles.defaultLineSpace;
		    if (m_FilterFoldout.boolValue)
		    {
			    EditorGUI.indentLevel++;
			    //Render queue filter
			    EditorGUI.PropertyField(rect, m_RenderQueue, Styles.renderQueueFilter);
			    rect.y += Styles.defaultLineSpace;
			    //Layer mask
			    EditorGUI.PropertyField(rect, m_LayerMask, Styles.layerMask);
			    rect.y += Styles.defaultLineSpace;
			    //Shader pass list
			    EditorGUI.indentLevel--;
			    m_ShaderPassesList.DoList(rect);
			    rect.y += m_ShaderPassesList.GetHeight();
		    }
	    }

	    void DoMaterialOverride(ref Rect rect)
	    {
		    //Override material
		    EditorGUI.PropertyField(rect, m_OverrideMaterial, Styles.overrideMaterial);
		    if (m_OverrideMaterial.objectReferenceValue)
		    {
			    rect.y += Styles.defaultLineSpace;
			    EditorGUI.indentLevel++;
			    EditorGUI.BeginChangeCheck();
			    EditorGUI.PropertyField(rect, m_OverrideMaterialPass, Styles.overrideMaterialPass);
			    if (EditorGUI.EndChangeCheck())
				    m_OverrideMaterialPass.intValue = Mathf.Max(0, m_OverrideMaterialPass.intValue);
			    EditorGUI.indentLevel--;
		    }
	    }

	    void DoDepthOverride(ref Rect rect)
	    {
		    EditorGUI.PropertyField(rect, m_OverrideDepth, Styles.overrideDepth);
		    if (m_OverrideDepth.boolValue)
		    {
			    rect.y += Styles.defaultLineSpace;
			    EditorGUI.indentLevel++;
			    //Write depth
			    EditorGUI.PropertyField(rect, m_WriteDepth, Styles.writeDepth);
			    rect.y += Styles.defaultLineSpace;
			    //Depth testing options
			    EditorGUI.PropertyField(rect, m_DepthCompareFunction, Styles.depthState);
			    EditorGUI.indentLevel--;
		    }
	    }
	    
	    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	    {
		    float height = Styles.defaultLineSpace;

			FetchPorperties(property);

			if (m_PassFoldout.boolValue)
				return height;
			
		    if (!firstTime)
		    {
				height += Styles.defaultLineSpace + Styles.defaultLineSpace; // name + type
		        height += Styles.defaultLineSpace * (m_FilterFoldout.boolValue ? m_FilterLines : 1);
		        height += m_FilterFoldout.boolValue ? m_ShaderPassesList.GetHeight() : 0;

		        height += Styles.defaultLineSpace; // add line for overrides dropdown
			    if (m_RendererFoldout.boolValue)
			    {
				    height += Styles.defaultLineSpace * (m_OverrideMaterial.objectReferenceValue != null ? m_MaterialLines : 1);
				    height += Styles.defaultLineSpace * (m_OverrideDepth.boolValue ? m_DepthLines : 1);
			    }
		    }

		    return height;
	    }
    }
}