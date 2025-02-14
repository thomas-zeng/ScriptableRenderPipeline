using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.VFX;
using UnityEditor.VFX.UIElements;
using UnityObject = UnityEngine.Object;
using Type = System.Type;

#if true
using ObjectField = UnityEditor.VFX.UI.VFXLabeledField<UnityEditor.UIElements.ObjectField, UnityEngine.Object>;

namespace UnityEditor.VFX.UI
{
    class ObjectPropertyRM : PropertyRM<UnityObject>
    {
        public ObjectPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
            m_ObjectField = new ObjectField(m_Label);
            if (controller.portType == typeof(Texture2D) || controller.portType == typeof(Texture3D) || controller.portType == typeof(Cubemap))
                m_ObjectField.control.objectType = typeof(Texture);
            else
                m_ObjectField.control.objectType = controller.portType;

            m_ObjectField.RegisterCallback<ChangeEvent<UnityObject>>(OnValueChanged);
            m_ObjectField.control.allowSceneObjects = false;
            m_ObjectField.style.flexGrow = 1f;
            m_ObjectField.style.flexShrink = 0f;

            Add(m_ObjectField);
        }

        public override float GetPreferredControlWidth()
        {
            return 120;
        }

        public void OnValueChanged(ChangeEvent<UnityObject> onObjectChanged)
        {
            UnityObject newValue = m_ObjectField.value;
            if (typeof(Texture).IsAssignableFrom(m_Provider.portType))
            {
                Texture tex = newValue as Texture;

                if (tex != null)
                {
                    if (m_Provider.portType == typeof(Texture2D))
                    {
                        if (tex.dimension != TextureDimension.Tex2D)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                    else if (m_Provider.portType == typeof(Texture3D))
                    {
                        if (tex.dimension != TextureDimension.Tex3D)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                    else if (m_Provider.portType == typeof(Cubemap))
                    {
                        if (tex.dimension != TextureDimension.Cube)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                }
            }
            m_Value = newValue;
            NotifyValueChanged();
        }

        ObjectField m_ObjectField;

        protected override void UpdateEnabled()
        {
            m_ObjectField.SetEnabled(propertyEnabled);
        }

        protected override void UpdateIndeterminate()
        {
            m_ObjectField.visible = !indeterminate;
        }

        public override void UpdateGUI(bool force)
        {
            if( force )
                m_ObjectField.SetValueWithoutNotify(null);
            m_ObjectField.SetValueWithoutNotify(m_Value);
        }

        public override void SetValue(object obj) // object setvalue should accept null
        {
            try
            {
                m_Value = (UnityObject)obj;
            }
            catch (System.Exception)
            {
                Debug.Log("Error Trying to convert" + (obj != null ? obj.GetType().Name : "null") + " to " + typeof(UnityObject).Name);
            }

            UpdateGUI(false);
        }

        public override bool showsEverything { get { return true; } }
    }
}
#else
using ObjectField = UnityEditor.VFX.UIElements.VFXObjectField;

namespace UnityEditor.VFX.UI
{
    class ObjectPropertyRM : PropertyRM<Object>
    {
        public ObjectPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
            m_ObjectField = new ObjectField(m_Label);
            if (controller.portType == typeof(Texture2D) || controller.portType == typeof(Texture3D) || controller.portType == typeof(TextureCube))
                m_ObjectField.editedType = typeof(Texture);
            else
                m_ObjectField.editedType = controller.portType;
            m_ObjectField.OnValueChanged = OnValueChanged;

            m_ObjectField.style.flex = 1;

            Add(m_ObjectField);
        }

        public void OnValueChanged()
        {
            Object newValue = m_ObjectField.GetValue();

            if (typeof(Texture).IsAssignableFrom(controller.portType))
            {
                Texture tex = newValue as Texture;

                if (tex != null)
                {
                    if (controller.portType == typeof(Texture2D))
                    {
                        if (tex.dimension != TextureDimension.Tex2D)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                    else if (controller.portType == typeof(Texture3D))
                    {
                        if (tex.dimension != TextureDimension.Tex3D)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                    else if (controller.portType == typeof(Cubemap))
                    {
                        if (tex.dimension != TextureDimension.Cube)
                        {
                            Debug.LogError("Wrong Texture Dimension");

                            newValue = null;
                        }
                    }
                }
            }
            m_Value = newValue;
            NotifyValueChanged();
        }

        ObjectField m_ObjectField;

        protected override void UpdateEnabled()
        {
            m_ObjectField.SetEnabled(propertyEnabled);
        }

        protected override void UpdateIndeterminate()
        {
            m_ObjectField.visible = !indeterminate;
        }

        public override void UpdateGUI()
        {
            m_ObjectField.SetValue(m_Value);
        }

        public override bool showsEverything { get { return true; } }
    }
}

#endif
