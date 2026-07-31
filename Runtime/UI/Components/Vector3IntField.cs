using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A field component that allows users to input and edit 3D integer vectors with X, Y, and Z coordinates.
    /// </summary>
    /// <remarks>
    /// The Vector3IntField is a specialized input component designed for editing 3D integer vectors. It provides
    /// three numeric fields for X, Y, and Z coordinates, allowing precise control over 3D integer positions or
    /// dimensions.
    ///
    /// Each coordinate field supports direct numeric input, validation, and formatting options. The component is
    /// particularly useful in scenarios involving grid-based positioning, voxel coordinates, or any 3D
    /// integer-based data entry.
    ///
    /// The field supports various features including:
    /// - Individual editing of X, Y, and Z coordinates
    /// - Value validation
    /// - Custom formatting
    /// - Different size variants
    /// - Invalid state indication
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML: Creating a basic vector field with default values</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Vector3IntField name="position-field" size="M" value="0,0,0" />
    /// ]]></code>
    /// <para>Setting up validation and handling value changes: Complex example showing validation and event handling</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector3IntField = new Vector3IntField();
    ///
    /// // Set up validation
    /// vector3IntField.validateValue = (v) =>
    ///     v.x >= 0 && v.y >= 0 && v.z >= 0;
    ///
    /// // Handle value changes
    /// vector3IntField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New value: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>Custom formatting with units: Adding pixel units to the coordinate values</para>
    /// <code lang="csharp"><![CDATA[
    /// var vector3IntField = new Vector3IntField();
    /// vector3IntField.formatFunction = (value) =>
    ///     $"{value}px";
    ///
    /// // Set initial value
    /// vector3IntField.value = new Vector3Int(100, 200, 300);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Vector3IntField : BaseVisualElement, IInputElement<Vector3Int>, ISizeableElement, INotifyValueChanging<Vector3Int>, IFormattable<int>
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId invalidProperty = new BindingId(nameof(invalid));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId validateValueProperty = new BindingId(nameof(validateValue));

        internal static readonly BindingId formatStringProperty = new BindingId(nameof(formatString));

        internal static readonly BindingId formatFunctionProperty = new BindingId(nameof(formatFunction));


        /// <summary>
        /// The Vector3Field main styling class.
        /// </summary>
        public const string ussClassName = "appui-vector3field";

        /// <summary>
        /// The Vector3Field size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Vector3Field container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Vector3Field X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The Vector3Field Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The Vector3Field Z NumericalField styling class.
        /// </summary>
        public const string zFieldUssClassName = ussClassName + "__z-field";

        Size m_Size;

        Vector3Int m_Value;

        readonly IntField m_XField;

        readonly IntField m_YField;

        readonly IntField m_ZField;

        Vector3Int m_LastValue;

        Func<Vector3Int, bool> m_ValidateValue;

        string m_FormatString;

        FormatFunction<int> m_FormatFunction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector3IntField()
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

            var zFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_ZField = new IntField { name = zFieldUssClassName, unit = "Z" };
            zFieldContainer.AddToClassList(zFieldUssClassName);
            zFieldContainer.Add(m_ZField);

            container.Add(xFieldContainer);
            container.Add(yFieldContainer);
            container.Add(zFieldContainer);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector3Int.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);
            m_ZField.RegisterValueChangingCallback(OnZFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_ZField.RegisterValueChangedCallback(OnZFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector3IntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector3IntField.
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
            }
        }

        /// <summary>
        /// Sets the value of the Vector3IntField without notifying any listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(Vector3Int newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_ZField.SetValueWithoutNotify(m_Value.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector3IntField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector3Int value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Vector3Int>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector3IntField.
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
            }
        }

        /// <summary>
        /// The validation function of the Vector3IntField.
        /// </summary>
        [CreateProperty]
        public Func<Vector3Int, bool> validateValue
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
                m_ZField.formatFunction = m_FormatFunction;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        void OnXFieldChanging(ChangingEvent<int> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(evt.newValue, m_Value.y, m_Value.z));
        }

        void OnYFieldChanging(ChangingEvent<int> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(m_Value.x, evt.newValue, m_Value.z));
        }

        void OnZFieldChanging(ChangingEvent<int> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3Int(m_Value.x, m_Value.y, evt.newValue));
        }

        void TrySendChangingEvent(Vector3Int newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Vector3Int>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnZFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(value.x, value.y, evt.newValue);
        }

        void OnYFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(value.x, evt.newValue, value.z);
        }

        void OnXFieldChanged(ChangeEvent<int> evt)
        {
            value = new Vector3Int(evt.newValue, value.y, value.z);
        }

    }
}
