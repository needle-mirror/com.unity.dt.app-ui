using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A numeric input field for editing 2D vector values with X and Y components.
    /// </summary>
    /// <remarks>
    /// Vector2Field is a specialized input component that allows users to edit 2D vector values by providing
    /// separate numeric fields for X and Y coordinates. It's particularly useful for position, scale, or any
    /// other 2D vector properties in Unity applications.
    ///
    /// The component consists of two numeric input fields arranged horizontally - one for the X coordinate and
    /// one for the Y coordinate. Each field can be independently edited and supports numerical input with
    /// floating-point precision.
    ///
    /// TIP: Use Vector2Field when you need to edit 2D coordinates, sizes, or any other values that naturally
    /// come in pairs of X/Y components.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML. Creating a medium-sized Vector2Field with default value in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <Vector2Field name="position-field" size="M" value="0,0" />
    /// ]]></code>
    /// <para>Code usage with validation. Creating a Vector2Field with formatting, validation, and change handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector2Field = new Vector2Field();
    /// vector2Field.formatString = "F1";
    /// vector2Field.validateValue = (v) => v.magnitude <= 100f;
    /// vector2Field.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New value: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>Using with data binding. Binding the Vector2Field to a Transform's position.</para>
    /// <code lang="csharp"><![CDATA[
    /// vector2Field.value = transform.position;
    /// vector2Field.RegisterValueChangedCallback(evt => {
    ///     transform.position = evt.newValue;
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Vector2Field
        : BaseVisualElement, IInputElement<Vector2>, ISizeableElement, INotifyValueChanging<Vector2>, IFormattable<float>
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId invalidProperty = new BindingId(nameof(invalid));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId validateValueProperty = new BindingId(nameof(validateValue));

        internal static readonly BindingId formatStringProperty = new BindingId(nameof(formatString));

        internal static readonly BindingId formatFunctionProperty = new BindingId(nameof(formatFunction));


        /// <summary>
        /// The Vector2Field main styling class.
        /// </summary>
        public const string ussClassName = "appui-vector2field";

        /// <summary>
        /// The Vector2Field size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Vector2Field container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Vector2Field X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The Vector2Field Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        Size m_Size;

        Vector2 m_Value;

        readonly FloatField m_XField;

        readonly FloatField m_YField;

        Vector2 m_LastValue;

        Func<Vector2, bool> m_ValidateValue;

        string m_FormatString;

        FormatFunction<float> m_FormatFunction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector2Field()
        {
            AddToClassList(ussClassName);

            var container = new VisualElement { name = containerUssClassName };
            container.AddToClassList(containerUssClassName);

            var xFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_XField = new FloatField { name = xFieldUssClassName, unit = "X" };
            xFieldContainer.AddToClassList(xFieldUssClassName);
            xFieldContainer.Add(m_XField);

            var yFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_YField = new FloatField { name = yFieldUssClassName, unit = "Y" };
            yFieldContainer.AddToClassList(yFieldUssClassName);
            yFieldContainer.Add(m_YField);

            container.Add(xFieldContainer);
            container.Add(yFieldContainer);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector2.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector2Field.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector2Field.
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
            }
        }

        /// <summary>
        /// Set the value of the Vector2Field without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the Vector2Field. </param>
        public void SetValueWithoutNotify(Vector2 newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector2Field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2 value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Vector2>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector2Field.
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
            }
        }

        /// <summary>
        /// The validation function to use to validate the value.
        /// </summary>
        [CreateProperty]
        public Func<Vector2, bool> validateValue
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
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        void OnXFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector2(evt.newValue, m_Value.y));
        }

        void OnYFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector2(m_Value.x, evt.newValue));
        }

        void TrySendChangingEvent(Vector2 newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Vector2>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector2(value.x, evt.newValue);
        }

        void OnXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector2(evt.newValue, value.y);
        }

    }
}
