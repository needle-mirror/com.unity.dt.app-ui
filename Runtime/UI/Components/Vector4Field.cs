using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A field control that allows users to edit a Vector4 value by inputting its X, Y, Z and W components.
    /// </summary>
    /// <remarks>
    /// The Vector4Field is a specialized input control that allows users to edit a Vector4 value through four
    /// separate numerical fields representing its X, Y, Z and W components. It's particularly useful for handling
    /// 4D vectors or when you need to input quaternion values.
    ///
    /// The field is organized in a 2x2 grid layout with clearly labeled inputs for each component. Each component
    /// field supports numerical input with floating-point precision and can be individually edited.
    ///
    /// Key features:
    /// - Individual floating-point input fields for X, Y, Z, and W components
    /// - Support for value validation
    /// - Customizable number formatting
    /// - Different size variants
    /// - Support for UXML integration
    /// </remarks>
    /// <example>
    /// <para>Basic usage with UXML. Basic UXML declaration of a Vector4Field.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Vector4Field name="myVector4Field"
    ///                size="M"
    ///                value="1 2 3 4"
    ///                format-string="F2" />
    /// ]]></code>
    /// <para>Code example showing various features. Creating and configuring a Vector4Field in code.</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector4Field = new Vector4Field {
    ///     value = new Vector4(1, 2, 3, 4),
    ///     size = Size.M,
    ///     formatString = "F2",
    ///     validateValue = (v) => v.magnitude <= 1.0f
    /// };
    ///
    /// vector4Field.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New value: {evt.newValue}");
    /// });
    ///
    /// container.Add(vector4Field);
    /// ]]></code>
    /// <para>Using with validation and custom formatting. Advanced setup with custom formatting and validation.</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector4Field = new Vector4Field();
    ///
    /// // Custom format function
    /// vector4Field.formatFunction = (float value) => $"{value:0.##}u";
    ///
    /// // Validation function
    /// vector4Field.validateValue = (Vector4 v) => {
    ///     return v.x >= 0 && v.y >= 0 && v.z >= 0 && v.w >= 0;
    /// };
    ///
    /// // Register change callback
    /// vector4Field.RegisterValueChangedCallback(evt => {
    ///     if (!vector4Field.invalid) {
    ///         ApplyVector4Value(evt.newValue);
    ///     }
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Vector4Field : BaseVisualElement, IInputElement<Vector4>, ISizeableElement, INotifyValueChanging<Vector4>, IFormattable<float>
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId invalidProperty = new BindingId(nameof(invalid));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId validateValueProperty = new BindingId(nameof(validateValue));

        internal static readonly BindingId formatStringProperty = new BindingId(nameof(formatString));

        internal static readonly BindingId formatFunctionProperty = new BindingId(nameof(formatFunction));


        /// <summary>
        /// The Vector4Field main styling class.
        /// </summary>
        public const string ussClassName = "appui-vector4field";

        /// <summary>
        /// The Vector4Field row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The Vector4Field size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Vector4Field X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The Vector4Field Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The Vector4Field Z NumericalField styling class.
        /// </summary>
        public const string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The Vector4Field W NumericalField styling class.
        /// </summary>
        public const string wFieldUssClassName = ussClassName + "__w-field";

        Size m_Size;

        Vector4 m_Value;

        readonly FloatField m_WField;

        readonly FloatField m_XField;

        readonly FloatField m_YField;

        readonly FloatField m_ZField;

        Vector4 m_LastValue;

        Func<Vector4, bool> m_ValidateValue;

        string m_FormatString;

        FormatFunction<float> m_FormatFunction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector4Field()
        {
            AddToClassList(ussClassName);

            var xFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_XField = new FloatField { name = xFieldUssClassName, unit = "X" };
            xFieldContainer.AddToClassList(xFieldUssClassName);
            xFieldContainer.Add(m_XField);

            var yFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_YField = new FloatField { name = yFieldUssClassName, unit = "Y" };
            yFieldContainer.AddToClassList(yFieldUssClassName);
            yFieldContainer.Add(m_YField);

            var zFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_ZField = new FloatField { name = zFieldUssClassName, unit = "Z" };
            zFieldContainer.AddToClassList(zFieldUssClassName);
            zFieldContainer.Add(m_ZField);

            var wFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_WField = new FloatField { name = wFieldUssClassName, unit = "W" };
            wFieldContainer.AddToClassList(wFieldUssClassName);
            wFieldContainer.Add(m_WField);

            var xyRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            xyRow.AddToClassList(rowUssClassName);
            xyRow.Add(xFieldContainer);
            xyRow.Add(yFieldContainer);

            var zwRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            zwRow.AddToClassList(rowUssClassName);
            zwRow.Add(zFieldContainer);
            zwRow.Add(wFieldContainer);

            hierarchy.Add(xyRow);
            hierarchy.Add(zwRow);

            size = Size.M;
            SetValueWithoutNotify(Vector4.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);
            m_ZField.RegisterValueChangingCallback(OnZFieldChanging);
            m_WField.RegisterValueChangingCallback(OnWFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_ZField.RegisterValueChangedCallback(OnZFieldChanged);
            m_WField.RegisterValueChangedCallback(OnWFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector4Field.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector4Field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                m_XField.size = m_Size;
                m_YField.size = m_Size;
                m_ZField.size = m_Size;
                m_WField.size = m_Size;
            }
        }

        /// <summary>
        /// Set the value of the Vector4Field without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the Vector4Field. </param>
        public void SetValueWithoutNotify(Vector4 newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_ZField.SetValueWithoutNotify(m_Value.z);
            m_WField.SetValueWithoutNotify(m_Value.w);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector4Field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector4 value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Vector4>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector4Field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                EnableInClassList(Styles.invalidUssClassName, value);

                m_XField.EnableInClassList(Styles.invalidUssClassName, value);
                m_YField.EnableInClassList(Styles.invalidUssClassName, value);
                m_ZField.EnableInClassList(Styles.invalidUssClassName, value);
                m_WField.EnableInClassList(Styles.invalidUssClassName, value);
            }
        }

        /// <summary>
        /// The validation function of the Vector4Field.
        /// </summary>
        [CreateProperty]
        public Func<Vector4, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !m_ValidateValue?.Invoke(m_Value) ?? false;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// The format string of the element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string formatString
        {
            get => m_FormatString;
            set
            {
                var changed = m_FormatString != value;
                m_FormatString = value;
                m_XField.formatString = m_FormatString;
                m_YField.formatString = m_FormatString;
                m_ZField.formatString = m_FormatString;
                m_WField.formatString = m_FormatString;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        /// <summary>
        /// The format function of the element.
        /// </summary>
        [CreateProperty]
        public FormatFunction<float> formatFunction
        {
            get => m_FormatFunction;
            set
            {
                var changed = m_FormatFunction != value;
                m_FormatFunction = value;
                m_XField.formatFunction = m_FormatFunction;
                m_YField.formatFunction = m_FormatFunction;
                m_ZField.formatFunction = m_FormatFunction;
                m_WField.formatFunction = m_FormatFunction;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        void OnXFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector4(evt.newValue, m_Value.y, m_Value.z, m_Value.w));
        }

        void OnYFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector4(m_Value.x, evt.newValue, m_Value.z, m_Value.w));
        }

        void OnZFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector4(m_Value.x, m_Value.y, evt.newValue, m_Value.w));
        }

        void OnWFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector4(m_Value.x, m_Value.y, m_Value.z, evt.newValue));
        }

        void TrySendChangingEvent(Vector4 newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Vector4>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnWFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, value.y, value.z, evt.newValue);
        }

        void OnZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, value.y, evt.newValue, value.w);
        }

        void OnYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(value.x, evt.newValue, value.z, value.w);
        }

        void OnXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector4(evt.newValue, value.y, value.z, value.w);
        }


    }
}
