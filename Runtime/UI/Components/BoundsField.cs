using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A UI component for editing 3D bounds with center and size values.
    /// </summary>
    /// <remarks>
    /// The BoundsField is a specialized input component that allows users to edit 3D bounds by specifying both the
    /// center point and size dimensions. It provides a structured interface with separate numerical fields for each
    /// coordinate (X, Y, Z) of both the center position and size values.
    ///
    /// This component is particularly useful when working with 3D spaces and requiring precise control over
    /// boundary definitions, such as in level editors, collision detection setup, or camera framing configurations.
    ///
    /// The field is composed of two main sections:
    /// 1. Center: Three numerical fields for setting the X, Y, and Z coordinates of the bounds center point
    /// 2. Size: Three numerical fields for defining the width (X), height (Y), and depth (Z) dimensions of the bounds
    /// </remarks>
    /// <example>
    /// <para>Basic usage of BoundsField in UXML: creating a medium-sized BoundsField in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:BoundsField name="myBounds" size="M" />
    /// ]]></code>
    /// <para>Creating and configuring BoundsField in C#: complete example showing creation, configuration, validation,
    /// and change handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var boundsField = new BoundsField();
    /// boundsField.size = Size.M;
    /// boundsField.value = new Bounds(Vector3.zero, new Vector3(1, 1, 1));
    /// boundsField.validateValue = (bounds) => bounds.size.x > 0 && bounds.size.y > 0 && bounds.size.z > 0;
    ///
    /// // Register for value changes
    /// boundsField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New bounds: Center={evt.newValue.center}, Size={evt.newValue.size}");
    /// });
    /// ]]></code>
    /// <para>Using BoundsField with validation for game object bounds: example showing how to use BoundsField to edit
    /// game object bounds with size validation.</para>
    /// <code lang="csharp"><![CDATA[
    /// var boundsField = new BoundsField();
    ///
    /// // Set up validation for minimum size
    /// boundsField.validateValue = (bounds) => {
    ///     var minSize = 0.1f;
    ///     return bounds.size.x >= minSize &&
    ///            bounds.size.y >= minSize &&
    ///            bounds.size.z >= minSize;
    /// };
    ///
    /// // Update game object bounds when value changes
    /// boundsField.RegisterValueChangedCallback(evt => {
    ///     if (!boundsField.invalid) {
    ///         gameObject.transform.position = evt.newValue.center;
    ///         // Assuming a cube mesh
    ///         gameObject.transform.localScale = evt.newValue.size;
    ///     }
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class BoundsField : BaseVisualElement, IInputElement<Bounds>, ISizeableElement, INotifyValueChanging<Bounds>
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);
        /// <summary>
        /// The BoundsField main styling class.
        /// </summary>
        public const string ussClassName = "appui-boundsfield";

        /// <summary>
        /// The BoundsField row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The BoundsField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The BoundsField X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The BoundsField Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The BoundsField Z NumericalField styling class.
        /// </summary>
        public const string zFieldUssClassName = ussClassName + "__z-field";

        /// <summary>
        /// The BoundsField X NumericalField styling class.
        /// </summary>
        public const string sxFieldUssClassName = ussClassName + "__sx-field";

        /// <summary>
        /// The BoundsField Y NumericalField styling class.
        /// </summary>
        public const string syFieldUssClassName = ussClassName + "__sy-field";

        /// <summary>
        /// The BoundsField Z NumericalField styling class.
        /// </summary>
        public const string szFieldUssClassName = ussClassName + "__sz-field";

        /// <summary>
        /// The BoundsField Label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        Bounds m_LastValue;

        Bounds m_Value;

        Func<Bounds, bool> m_ValidateValue;

        readonly FloatField m_CXField;

        readonly FloatField m_CYField;

        readonly FloatField m_CZField;

        readonly FloatField m_SXField;

        readonly FloatField m_SYField;

        readonly FloatField m_SZField;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BoundsField()
        {
            AddToClassList(ussClassName);

            var cXFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CXField = new FloatField { name = xFieldUssClassName, unit = "X" };
            cXFieldContainer.AddToClassList(xFieldUssClassName);
            cXFieldContainer.Add(m_CXField);

            var cYFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CYField = new FloatField { name = yFieldUssClassName, unit = "Y" };
            cYFieldContainer.AddToClassList(yFieldUssClassName);
            cYFieldContainer.Add(m_CYField);

            var cZFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_CZField = new FloatField { name = zFieldUssClassName, unit = "Z" };
            cZFieldContainer.AddToClassList(zFieldUssClassName);
            cZFieldContainer.Add(m_CZField);

            var sXFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SXField = new FloatField { name = sxFieldUssClassName, unit = "X" };
            sXFieldContainer.AddToClassList(sxFieldUssClassName);
            sXFieldContainer.Add(m_SXField);

            var sYFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SYField = new FloatField { name = syFieldUssClassName, unit = "Y" };
            sYFieldContainer.AddToClassList(syFieldUssClassName);
            sYFieldContainer.Add(m_SYField);

            var sZFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_SZField = new FloatField { name = szFieldUssClassName, unit = "Z" };
            sZFieldContainer.AddToClassList(szFieldUssClassName);
            sZFieldContainer.Add(m_SZField);

            var centerLabel = new Text("Center") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            centerLabel.AddToClassList(labelUssClassName);
            var sizeLabel = new Text("Size") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            sizeLabel.AddToClassList(labelUssClassName);

            var centerRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            centerRow.AddToClassList(rowUssClassName);
            centerRow.Add(centerLabel);
            centerRow.Add(cXFieldContainer);
            centerRow.Add(cYFieldContainer);
            centerRow.Add(cZFieldContainer);

            var sizeRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            sizeRow.AddToClassList(rowUssClassName);
            sizeRow.Add(sizeLabel);
            sizeRow.Add(sXFieldContainer);
            sizeRow.Add(sYFieldContainer);
            sizeRow.Add(sZFieldContainer);

            hierarchy.Add(centerRow);
            hierarchy.Add(sizeRow);

            size = Size.M;
            SetValueWithoutNotify(new Bounds());

            m_CXField.RegisterValueChangedCallback(OnCXFieldChanged);
            m_CYField.RegisterValueChangedCallback(OnCYFieldChanged);
            m_CZField.RegisterValueChangedCallback(OnCZFieldChanged);
            m_SXField.RegisterValueChangedCallback(OnSXFieldChanged);
            m_SYField.RegisterValueChangedCallback(OnSYFieldChanged);
            m_SZField.RegisterValueChangedCallback(OnSZFieldChanged);

            m_CXField.RegisterValueChangingCallback(OnCXFieldChanging);
            m_CYField.RegisterValueChangingCallback(OnCYFieldChanging);
            m_CZField.RegisterValueChangingCallback(OnCZFieldChanging);
            m_SXField.RegisterValueChangingCallback(OnSXFieldChanging);
            m_SYField.RegisterValueChangingCallback(OnSYFieldChanging);
            m_SZField.RegisterValueChangingCallback(OnSZFieldChanging);
        }

        /// <summary>
        /// The content container of the BoundsField. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The BoundsField size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Bounds Field")]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                m_CXField.size = m_Size;
                m_CYField.size = m_Size;
                m_CZField.size = m_Size;
                m_SXField.size = m_Size;
                m_SYField.size = m_Size;
                m_SZField.size = m_Size;

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Sets the value of the BoundsField without notifying the value changed callback.
        /// </summary>
        /// <param name="newValue"> The new value of the BoundsField. </param>
        public void SetValueWithoutNotify(Bounds newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_CXField.SetValueWithoutNotify(m_Value.center.x);
            m_CYField.SetValueWithoutNotify(m_Value.center.y);
            m_CZField.SetValueWithoutNotify(m_Value.center.z);
            m_SXField.SetValueWithoutNotify(m_Value.size.x);
            m_SYField.SetValueWithoutNotify(m_Value.size.y);
            m_SZField.SetValueWithoutNotify(m_Value.size.z);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the BoundsField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Bounds value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Bounds>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Set the validation state of the BoundsField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = invalid != value;
                EnableInClassList(Styles.invalidUssClassName, value);

                m_CXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_CZField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SXField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SYField.EnableInClassList(Styles.invalidUssClassName, value);
                m_SZField.EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// The validation function of the BoundsField.
        /// </summary>
        [CreateProperty]
        public Func<Bounds, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                if (validateValue != null)
                    invalid = !validateValue(m_Value);

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        void OnCZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(value.center.x, value.center.y, evt.newValue), value.size);
        }

        void OnCYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(value.center.x, evt.newValue, value.center.z), value.size);
        }

        void OnCXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(new Vector3(evt.newValue, value.center.y, value.center.z), value.size);
        }

        void OnSXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(evt.newValue, value.size.y, value.size.z));
        }

        void OnSYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(value.size.x, evt.newValue, value.size.z));
        }

        void OnSZFieldChanged(ChangeEvent<float> evt)
        {
            value = new Bounds(value.center, new Vector3(value.size.x, value.size.y, evt.newValue));
        }

        void OnCZFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(new Vector3(value.center.x, value.center.y, evt.newValue), value.size);
            TrySendChangingEvent(val);
        }

        void OnCYFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(new Vector3(value.center.x, evt.newValue, value.center.z), value.size);
            TrySendChangingEvent(val);
        }

        void OnCXFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(new Vector3(evt.newValue, value.center.y, value.center.z), value.size);
            TrySendChangingEvent(val);
        }

        void OnSXFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(value.center, new Vector3(evt.newValue, value.size.y, value.size.z));
            TrySendChangingEvent(val);
        }

        void OnSYFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(value.center, new Vector3(value.size.x, evt.newValue, value.size.z));
            TrySendChangingEvent(val);
        }

        void OnSZFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Bounds(value.center, new Vector3(value.size.x, value.size.y, evt.newValue));
            TrySendChangingEvent(val);
        }

        void TrySendChangingEvent(Bounds newVal)
        {
            var previousValue = m_Value;
            m_Value = newVal;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Bounds>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

    }
}
