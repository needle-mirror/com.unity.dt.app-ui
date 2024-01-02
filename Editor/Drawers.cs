using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    // Generic Property Drawers
    
    public class OptionalPropertyDrawer<T, TU> : PropertyDrawer
        where TU : BindableElement, INotifyValueChanged<T>, new()
    {
        protected SerializedProperty m_HasValue;

        protected SerializedProperty m_Value;
        
        protected UnityEngine.UIElements.Toggle m_HasValueField;
        
        protected TU m_ValueField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_HasValue = property.FindPropertyRelative("isSet");
            m_Value = property.FindPropertyRelative("value");

            m_HasValueField = new UnityEngine.UIElements.Toggle
            {
                style =
                {
                    flexGrow = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0
                },
                label = null,
                bindingPath = m_HasValue.propertyPath
            };
            
            m_ValueField = new TU
            {
                style =
                {
                    flexGrow = 1,
                    marginRight = 0
                },
                bindingPath = m_Value.propertyPath
            };
            m_ValueField.RegisterValueChangedCallback(e =>
            {
                SetValue(e.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });
            
            m_HasValueField.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                m_ValueField.SetEnabled(e.newValue);
                m_HasValue.boolValue = e.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            m_HasValueField.SetValueWithoutNotify(m_HasValue.boolValue);
            m_ValueField.SetEnabled(m_HasValue.boolValue);
            
            var input = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            input.Add(m_HasValueField);
            input.Add(m_ValueField);
            
            var field = new OptionalField<T>(property.displayName, input);
            field.AddToClassList(OptionalField<T>.alignedFieldUssClassName);
            field.Bind(property.serializedObject);
            
            return field;
        }

        protected virtual void SetValue(T newValue)
        {
            
        }
        
        internal void SetValueInternal(T newValue)
        {
            SetValue(newValue);
        }
    }

    public class OptionalValuePropertyDrawer<T, TU> : OptionalPropertyDrawer<T, TU>
        where T : struct, IComparable, IComparable<T>, IFormattable
        where TU : BindableElement, INotifyValueChanged<T>, new()
    {
        
    }
    
    public class OptionalEnumPropertyDrawer<T> : PropertyDrawer
        where T : struct, Enum
    {
        SerializedProperty m_HasValue;

        SerializedProperty m_Value;
        
        UnityEngine.UIElements.Toggle m_HasValueField;
        
        EnumField m_ValueField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_HasValue = property.FindPropertyRelative("isSet");
            m_Value = property.FindPropertyRelative("value");

            m_HasValueField = new UnityEngine.UIElements.Toggle
            {
                style =
                {
                    flexGrow = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0
                },
                label = null,
                bindingPath = m_HasValue.propertyPath
            };
            
            m_ValueField = new EnumField
            {
                style =
                {
                    flexGrow = 1,
                    marginRight = 0
                },
                label = null,
                bindingPath = m_Value.propertyPath
            };
            m_ValueField.RegisterValueChangedCallback(e =>
            {
                m_Value.enumValueIndex = Convert.ToInt32(e.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });
            
            m_HasValueField.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                m_ValueField.SetEnabled(e.newValue);
                m_HasValue.boolValue = e.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            m_HasValueField.SetValueWithoutNotify(m_HasValue.boolValue);
            m_ValueField.SetEnabled(m_HasValue.boolValue);
            
            var input = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            input.Add(m_HasValueField);
            input.Add(m_ValueField);
            
            var field = new OptionalEnumField<T>(property.displayName, input);
            field.AddToClassList(OptionalEnumField<T>.alignedFieldUssClassName);
            field.Bind(property.serializedObject);
            
            return field;
        }
    }
    
    // Specific Property Drawers
    
    [CustomPropertyDrawer(typeof(OptionalEnum<UI.PopoverPlacement>))]
    public class OptionalPreferredTooltipPlacementDrawer : OptionalEnumPropertyDrawer<UI.PopoverPlacement> { }
    
    [CustomPropertyDrawer(typeof(OptionalEnum<Dir>))]
    public class OptionalDirDrawer : OptionalEnumPropertyDrawer<Dir> { }

    [CustomPropertyDrawer(typeof(Optional<int>))]
    public class OptionalIntDrawer : OptionalValuePropertyDrawer<int, IntegerField>
    {
        protected override void SetValue(int newValue) => m_Value.intValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<long>))]
    public class OptionalLongDrawer : OptionalValuePropertyDrawer<long, LongField>
    {
        protected override void SetValue(long newValue) => m_Value.longValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<float>))]
    public class OptionalFloatDrawer : OptionalValuePropertyDrawer<float, FloatField>
    {
        protected override void SetValue(float newValue) => m_Value.floatValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<double>))]
    public class OptionalDoubleDrawer : OptionalValuePropertyDrawer<double, DoubleField>
    {
        protected override void SetValue(double newValue) => m_Value.doubleValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<string>))]
    public class OptionalStringDrawer : OptionalPropertyDrawer<string, TextField>
    {
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<Color>))]
    public class OptionalColorDrawer : OptionalPropertyDrawer<Color, ColorField>
    {
        protected override void SetValue(Color newValue) => m_Value.colorValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(Optional<Rect>))]
    public class OptionalRectDrawer : OptionalPropertyDrawer<Rect, RectField>
    {
        protected override void SetValue(Rect newValue) => m_Value.rectValue = newValue;
    }

    public class ScaleField : DropdownField
    {
        static readonly List<string> k_ScaleOptions = new List<string> { "small", "medium", "large" };
        
        public ScaleField() : this(null) { }
        
        public ScaleField(string label) 
            : base(label, k_ScaleOptions, 1, Core.StringExtensions.Capitalize, Core.StringExtensions.Capitalize)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }
    
    public class ThemeField : DropdownField
    {
        static readonly List<string> k_ScaleOptions = new List<string> { "dark", "light", "editor-dark", "editor-light" };
        
        public ThemeField() : this(null) { }
        
        public ThemeField(string label) 
            : base(label, k_ScaleOptions, 1, Core.StringExtensions.Capitalize, Core.StringExtensions.Capitalize)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }

    [CustomPropertyDrawer(typeof(UI.OptionalScaleDrawerAttribute))]
    public class OptionalScaleDrawer : OptionalPropertyDrawer<string, ScaleField>
    {
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(UI.ScaleDrawerAttribute))]
    public class ScaleDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new ScaleField(property.displayName)
            {
                bindingPath = property.propertyPath
            };
            field.Bind(property.serializedObject);
            return field;
        }
    }
    
    [CustomPropertyDrawer(typeof(UI.OptionalThemeDrawerAttribute))]
    public class OptionalThemeDrawer : OptionalPropertyDrawer<string, ThemeField>
    {
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }
    
    [CustomPropertyDrawer(typeof(UI.ThemeDrawerAttribute))]
    public class ThemeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new ThemeField(property.displayName)
            {
                bindingPath = property.propertyPath
            };
            field.Bind(property.serializedObject);
            return field;
        }
    }
    
    [CustomPropertyDrawer(typeof(UI.DefaultPropertyDrawerAttribute))]
    public class DirDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            field.Bind(property.serializedObject);
            return field;
        }
    }
}
