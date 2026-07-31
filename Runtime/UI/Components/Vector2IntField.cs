using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A field control that allows users to input or modify a 2D vector with integer components.
    /// </summary>
    /// <remarks>
    /// The Vector2IntField is a specialized input control that enables users to edit two-dimensional vectors with
    /// integer components (X and Y values). It provides a clean and intuitive interface for manipulating 2D
    /// coordinates, dimensions, or any other data that can be represented as a pair of integer values.
    ///
    /// The field consists of two numerical inputs arranged horizontally - one for the X component and one for the
    /// Y component. Each component is clearly labeled and can be modified independently.
    ///
    /// This component is particularly useful in scenarios involving:
    ///
    /// - 2D grid positions or coordinates
    /// - Pixel dimensions or offsets
    /// - Integer-based 2D transformations
    ///
    /// Both input fields support keyboard navigation, direct numerical input, and validation to ensure only valid
    /// integer values are accepted.
    /// </remarks>
    /// <example>
    /// <para>Basic Vector2IntField Usage:</para>
    ///
    /// <para>Creating a basic Vector2IntField in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:Vector2IntField
    ///         name="positionField"
    ///         size="M"
    ///         value="0,0"
    ///     />
    /// </ui:UXML>
    /// ]]></code>
    /// <para>Vector2IntField with Validation:</para>
    ///
    /// <para>Setting up a Vector2IntField with validation and change handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector2IntField = new Vector2IntField();
    ///
    /// // Set up validation for positive values only
    /// vector2IntField.validateValue = (v) => v.x >= 0 && v.y >= 0;
    ///
    /// // Add change handler
    /// vector2IntField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Vector changed to: ({evt.newValue.x}, {evt.newValue.y})");
    /// });
    /// ]]></code>
    /// <para>Customized Vector2IntField:</para>
    ///
    /// <para>Creating a customized Vector2IntField for screen resolution input.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:Vector2IntField
    ///         name="resolutionField"
    ///         size="L"
    ///         value="1920,1080"
    ///         format-string="D4"
    ///     />
    /// </ui:UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Vector2IntField
        : BaseVisualElement, IInputElement<Vector2Int>, ISizeableElement, INotifyValueChanging<Vector2Int>, IFormattable<int>
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

        Vector2Int m_Value;

        readonly IntField m_XField;

        readonly IntField m_YField;

        Vector2Int m_LastValue;

        Func<Vector2Int, bool> m_ValidateValue;

        string m_FormatString;

        FormatFunction<int> m_FormatFunction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector2IntField()
        {
            AddToClassList(ussClassName);

            var container = new VisualElement { name = containerUssClassName };
            container.AddToClassList(containerUssClassName);

            var xFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_XField = new IntField { name = xFieldUssClassName, unit = "X" };
            xFieldContainer.AddToClassList(xFieldUssClassName);
            xFieldContainer.Add(m_XField);

            var yFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_YField = new IntField { name = yFieldUssClassName, unit = "Y" };
            yFieldContainer.AddToClassList(yFieldUssClassName);
            yFieldContainer.Add(m_YField);

            container.Add(xFieldContainer);
            container.Add(yFieldContainer);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector2Int.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector2IntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector2IntField.
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
        /// Set the value of the Vector2IntField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the Vector2IntField. </param>
        public void SetValueWithoutNotify(Vector2Int newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector2IntField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2Int value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Vector2Int>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector2IntField.
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
        public Func<Vector2Int, bool> validateValue
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
        public FormatFunction<int> formatFunction
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

        void OnXFieldChanging(ChangingEvent<int> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector2Int(evt.newValue, m_Value.y));
        }

        void OnYFieldChanging(ChangingEvent<int> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector2Int(m_Value.x, evt.newValue));
        }

        void TrySendChangingEvent(Vector2Int newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Vector2Int>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnYFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector2Int(value.x, evt.newValue);
        }

        void OnXFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector2Int(evt.newValue, value.y);
        }

    }
}
