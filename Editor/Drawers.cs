using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.AppUI.Editor.Theming;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using IntegerField = UnityEngine.UIElements.IntegerField;
using LongField = UnityEngine.UIElements.LongField;
using FloatField = UnityEngine.UIElements.FloatField;
using DoubleField = UnityEngine.UIElements.DoubleField;
using EnumField = UnityEngine.UIElements.EnumField;
using ColorField = UnityEditor.UIElements.ColorField;
using RectField = UnityEngine.UIElements.RectField;

namespace Unity.AppUI.Editor
{
    // Generic Property Drawers

    /// <summary>
    /// Draws the Inspector GUI for an optional property
    /// </summary>
    /// <typeparam name="T"> The type of the optional property </typeparam>
    /// <typeparam name="TU"> The type of the field used to edit the value of the optional property </typeparam>
    public class OptionalPropertyDrawer<T, TU> : PropertyDrawer
        where TU : BindableElement, INotifyValueChanged<T>, new()
    {
        /// <summary>
        /// Whether the optional property has a value
        /// </summary>
        protected SerializedProperty m_HasValue;

        /// <summary>
        /// The value of the optional property
        /// </summary>
        protected SerializedProperty m_Value;

        /// <summary>
        /// The toggle component used to edit the hasValue property
        /// </summary>
        protected UnityEngine.UIElements.Toggle m_HasValueField;

        /// <summary>
        /// The field component used to edit the value property
        /// </summary>
        protected TU m_ValueField;

        /// <summary>
        ///   <para>Creates custom GUI with UI Toolkit for the property.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        ///   <para>The element containing the custom GUI.</para>
        /// </returns>
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

        /// <summary>
        /// Sets the value of the property in the serialized object
        /// </summary>
        /// <param name="newValue"> The new value of the property </param>
        protected virtual void SetValue(T newValue)
        {

        }

        internal void SetValueInternal(T newValue)
        {
            SetValue(newValue);
        }
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional value property
    /// </summary>
    /// <typeparam name="T"> The type of the optional property </typeparam>
    /// <typeparam name="TU"> The type of the field used to edit the value of the optional property </typeparam>
    public class OptionalValuePropertyDrawer<T, TU> : OptionalPropertyDrawer<T, TU>
        where T : struct, IComparable, IComparable<T>, IFormattable
        where TU : BindableElement, INotifyValueChanged<T>, new()
    {

    }

    /// <summary>
    /// Draws the Inspector GUI for an optional enum property
    /// </summary>
    /// <typeparam name="T"> The type of the optional property </typeparam>
    public class OptionalEnumPropertyDrawer<T> : PropertyDrawer
        where T : struct, Enum
    {
        SerializedProperty m_HasValue;

        SerializedProperty m_Value;

        UnityEngine.UIElements.Toggle m_HasValueField;

        EnumField m_ValueField;

        /// <summary>
        ///   <para>Creates custom GUI with UI Toolkit for the property.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        ///   <para>The element containing the custom GUI.</para>
        /// </returns>
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

    /// <summary>
    /// Draws the Inspector GUI for an optional preferred tooltip placement property
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionalEnum<PopoverPlacement>))]
    public class OptionalPreferredTooltipPlacementDrawer : OptionalEnumPropertyDrawer<PopoverPlacement> { }

    /// <summary>
    /// Draws the Inspector GUI for an optional layout direction property
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionalEnum<Dir>))]
    public class OptionalDirDrawer : OptionalEnumPropertyDrawer<Dir> { }

    /// <summary>
    /// Draws the Inspector GUI for an optional integer property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<int>))]
    public class OptionalIntDrawer : OptionalValuePropertyDrawer<int, IntegerField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(int newValue) => m_Value.intValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional long property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<long>))]
    public class OptionalLongDrawer : OptionalValuePropertyDrawer<long, LongField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(long newValue) => m_Value.longValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional float property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<float>))]
    public class OptionalFloatDrawer : OptionalValuePropertyDrawer<float, FloatField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(float newValue) => m_Value.floatValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional double property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<double>))]
    public class OptionalDoubleDrawer : OptionalValuePropertyDrawer<double, DoubleField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(double newValue) => m_Value.doubleValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional string property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<string>))]
    public class OptionalStringDrawer : OptionalPropertyDrawer<string, UnityEngine.UIElements.TextField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional color property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<Color>))]
    public class OptionalColorDrawer : OptionalPropertyDrawer<Color, ColorField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(Color newValue) => m_Value.colorValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional rect property
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<Rect>))]
    public class OptionalRectDrawer : OptionalPropertyDrawer<Rect, RectField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(Rect newValue) => m_Value.rectValue = newValue;
    }

    /// <summary>
    /// Custom Field for <see cref="Panel.density"/> and <see cref="BaseVisualElement.densityOverride"/> property.
    /// </summary>
    public class DensityField : DropdownField
    {
        static readonly List<string> k_DensityOptions = new List<string> { "normal", "compact", "comfort" };

        /// <inheritdoc cref="DropdownField" />
        public DensityField() : this(null) { }

        /// <inheritdoc cref="DropdownField" />
        public DensityField(string label)
            : base(label, k_DensityOptions, 0, Core.StringExtensions.Capitalize, Core.StringExtensions.Capitalize)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }

    /// <summary>
    /// Custom Field for <see cref="Panel.theme"/> and <see cref="BaseVisualElement.themeOverride"/> property.
    /// </summary>
    public class ThemeField : DropdownField
    {
        /// <inheritdoc cref="DropdownField" />
        public ThemeField() : this(null) { }

        /// <inheritdoc cref="DropdownField" />
        public ThemeField(string label)
            : base(label, ThemeRegistry.GetThemes(), 0, FormatTheme, FormatTheme)
        {
            AddToClassList(alignedFieldUssClassName);

            // Themes are discovered from the project, so the list cannot be captured once in the
            // constructor. Rebuilding on attach is not enough either: importing a theme while an
            // inspector is already open invalidates the registry but would leave this field showing
            // the list it built when it attached. So it follows project changes for as long as it
            // is on screen, and stops when it leaves.
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                RefreshChoices();
                EditorApplication.projectChanged += RefreshChoices;
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                EditorApplication.projectChanged -= RefreshChoices;
            });
        }

        void RefreshChoices()
        {
            var themes = ThemeRegistry.GetThemes();

            // Never drop the current value. A project can legitimately set a theme this dropdown
            // does not know about — one defined by a hand-written stylesheet, or an asset that has
            // not been imported yet — and silently rewriting Panel.theme to the first entry would
            // be an inspector quietly changing a scene.
            if (!string.IsNullOrEmpty(value) && !themes.Contains(value))
                themes.Add(value);

            choices = themes;
        }

        static string FormatTheme(string theme)
        {
            return string.IsNullOrEmpty(theme) ? theme : ThemeRegistry.GetDisplayName(theme);
        }
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional density property
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionalDensityDrawerAttribute))]
    public class OptionalDensityDrawer : OptionalPropertyDrawer<string, DensityField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for a density property
    /// </summary>
    [CustomPropertyDrawer(typeof(DensityDrawerAttribute))]
    public class DensityDrawer : PropertyDrawer
    {
        /// <summary>
        ///   <para>Creates custom GUI with UI Toolkit for the property.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        ///   <para>The element containing the custom GUI.</para>
        /// </returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new DensityField(property.displayName)
            {
                bindingPath = property.propertyPath
            };
            field.Bind(property.serializedObject);
            return field;
        }
    }

    /// <summary>
    /// Draws the Inspector GUI for an optional theme property
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionalThemeDrawerAttribute))]
    public class OptionalThemeDrawer : OptionalPropertyDrawer<string, ThemeField>
    {
        /// <inheritdoc cref="OptionalPropertyDrawer{T,TU}.SetValue" />
        protected override void SetValue(string newValue) => m_Value.stringValue = newValue;
    }

    /// <summary>
    /// Draws the Inspector GUI for a theme property
    /// </summary>
    [CustomPropertyDrawer(typeof(ThemeDrawerAttribute))]
    public class ThemeDrawer : PropertyDrawer
    {
        /// <summary>
        ///   <para>Creates custom GUI with UI Toolkit for the property.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        ///   <para>The element containing the custom GUI.</para>
        /// </returns>
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

    /// <summary>
    /// Draws the Inspector GUI for an optional dir property
    /// </summary>
    [CustomPropertyDrawer(typeof(DefaultPropertyDrawerAttribute))]
    public class DirDrawer : PropertyDrawer
    {
        /// <summary>
        ///   <para>Creates custom GUI with UI Toolkit for the property.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>
        ///   <para>The element containing the custom GUI.</para>
        /// </returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            field.Bind(property.serializedObject);
            return field;
        }
    }
}
