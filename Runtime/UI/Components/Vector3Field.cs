using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A UI component for editing 3D vector values with x, y, and z coordinates.
    /// </summary>
    /// <remarks>
    /// The Vector3Field is a specialized input component that allows users to edit three-dimensional vector values.
    /// It provides three numerical input fields for the x, y, and z coordinates of a vector.
    ///
    /// The component is built on top of three FloatField components, each handling one dimension of the vector.
    /// It supports various features like value validation, formatting, and size customization.
    ///
    /// Use Vector3Field when you need to let users input or modify 3D coordinates, positions, scales, or any other
    /// three-dimensional values in your application.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Vector3Field : BaseVisualElement, IInputElement<Vector3>, ISizeableElement, INotifyValueChanging<Vector3>, IFormattable<float>
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

        Vector3 m_Value;

        readonly FloatField m_XField;

        readonly FloatField m_YField;

        readonly FloatField m_ZField;

        Vector3 m_LastValue;

        Func<Vector3, bool> m_ValidateValue;

        string m_FormatString;

        FormatFunction<float> m_FormatFunction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Vector3Field()
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

            var zFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_ZField = new FloatField { name = zFieldUssClassName, unit = "Z" };
            zFieldContainer.AddToClassList(zFieldUssClassName);
            zFieldContainer.Add(m_ZField);

            container.Add(xFieldContainer);
            container.Add(yFieldContainer);
            container.Add(zFieldContainer);

            hierarchy.Add(container);

            size = Size.M;
            SetValueWithoutNotify(Vector3.zero);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);
            m_ZField.RegisterValueChangingCallback(OnZFieldChanging);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_ZField.RegisterValueChangedCallback(OnZFieldChanged);
        }

        /// <summary>
        /// The content container of the Vector3Field.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Vector3Field.
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
        /// Sets the value of the Vector3Field without notifying the change event.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(Vector3 newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_ZField.SetValueWithoutNotify(m_Value.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the Vector3Field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector3 value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Vector3>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        /// <summary>
        /// The invalid state of the Vector3Field.
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
        /// The validation function of the Vector3Field.
        /// </summary>
        [CreateProperty]
        public Func<Vector3, bool> validateValue
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
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        void OnXFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3(evt.newValue, m_Value.y, m_Value.z));
        }

        void OnYFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3(m_Value.x, evt.newValue, m_Value.z));
        }

        void OnZFieldChanging(ChangingEvent<float> evt)
        {

            evt.StopPropagation();
            TrySendChangingEvent(new Vector3(m_Value.x, m_Value.y, evt.newValue));
        }

        void TrySendChangingEvent(Vector3 newVector)
        {
            var previousValue = m_Value;
            m_Value = newVector;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Vector3>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

        void OnZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector3(value.x, value.y, evt.newValue);
        }

        void OnYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector3(value.x, evt.newValue, value.z);
        }

        void OnXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Vector3(evt.newValue, value.y, value.z);
        }

    }
}
