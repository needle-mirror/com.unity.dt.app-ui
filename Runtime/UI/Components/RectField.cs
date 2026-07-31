using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A specialized input field for editing Unity Rect values with position and size components.
    /// </summary>
    /// <remarks>
    /// The RectField component provides a specialized input interface for editing Unity Rect values. It breaks
    /// down the rect into its four components - X, Y (position) and Width, Height (size) - making it easier to
    /// edit rect values in a structured way.
    ///
    /// The field is organized in two rows - one for position (X,Y) and one for size (Width,Height) - each with
    /// clear labels and numeric inputs. Each numeric component is implemented as a FloatField, providing
    /// precise control over the values.
    ///
    /// All numeric inputs support keyboard input, validation, and optional value constraints. The component
    /// maintains the integrity of the Rect structure while allowing independent editing of its components.
    /// </remarks>
    /// <example>
    /// <para>Basic RectField Usage</para>
    /// <code lang="csharp"><![CDATA[
    /// // Create a new RectField
    /// var rectField = new RectField();
    ///
    /// // Set initial value
    /// rectField.value = new Rect(0, 0, 100, 100);
    ///
    /// // Register for value changes
    /// rectField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New rect: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>RectField with Validation</para>
    /// <code lang="csharp"><![CDATA[
    /// var rectField = new RectField();
    ///
    /// // Set up validation for positive values within bounds
    /// rectField.validateValue = (rect) => {
    ///     return rect.width > 0 && rect.height > 0 &&
    ///            rect.x >= 0 && rect.y >= 0 &&
    ///            rect.x + rect.width <= Screen.width &&
    ///            rect.y + rect.height <= Screen.height;
    /// };
    ///
    /// // Handle invalid values
    /// rectField.RegisterCallback<ChangeEvent<Rect>>(evt => {
    ///     if (rectField.invalid)
    ///     {
    ///         Debug.LogWarning("Invalid rectangle dimensions");
    ///     }
    /// });
    /// ]]></code>
    /// <para>UXML Usage Example</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:RectField
    ///         size="M"
    ///         value="0, 0, 100, 100"
    ///         style="width: 300px;"/>
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class RectField : BaseVisualElement, IInputElement<Rect>, ISizeableElement, INotifyValueChanging<Rect>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId sizeProperty = nameof(size);


        /// <summary>
        /// The RectField main styling class.
        /// </summary>
        public const string ussClassName = "appui-rectfield";

        /// <summary>
        /// The RectField row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The RectField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The RectField X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The RectField Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The RectField H NumericalField styling class.
        /// </summary>
        public const string hFieldUssClassName = ussClassName + "__h-field";

        /// <summary>
        /// The RectField W NumericalField styling class.
        /// </summary>
        public const string wFieldUssClassName = ussClassName + "__w-field";

        /// <summary>
        /// The RectField Label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        Rect m_LastValue;

        Rect m_Value;

        readonly FloatField m_WField;

        readonly FloatField m_XField;

        readonly FloatField m_YField;

        readonly FloatField m_HField;

        Func<Rect, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RectField()
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

            var wFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_WField = new FloatField { name = wFieldUssClassName, unit = "W" };
            wFieldContainer.AddToClassList(wFieldUssClassName);
            wFieldContainer.Add(m_WField);

            var hFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_HField = new FloatField { name = hFieldUssClassName, unit = "H" };
            hFieldContainer.AddToClassList(hFieldUssClassName);
            hFieldContainer.Add(m_HField);

            var positionLabel = new Text("Position") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            positionLabel.AddToClassList(labelUssClassName);
            var sizeLabel = new Text("Size") { size = TextSize.S, pickingMode = PickingMode.Ignore };
            sizeLabel.AddToClassList(labelUssClassName);

            var positionRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            positionRow.AddToClassList(rowUssClassName);
            positionRow.Add(positionLabel);
            positionRow.Add(xFieldContainer);
            positionRow.Add(yFieldContainer);

            var sizeRow = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            sizeRow.AddToClassList(rowUssClassName);
            sizeRow.Add(sizeLabel);
            sizeRow.Add(wFieldContainer);
            sizeRow.Add(hFieldContainer);

            hierarchy.Add(positionRow);
            hierarchy.Add(sizeRow);

            size = Size.M;
            SetValueWithoutNotify(Rect.zero);

            m_XField.RegisterValueChangedCallback(OnXFieldChanged);
            m_YField.RegisterValueChangedCallback(OnYFieldChanged);
            m_HField.RegisterValueChangedCallback(OnHFieldChanged);
            m_WField.RegisterValueChangedCallback(OnWFieldChanged);

            m_XField.RegisterValueChangingCallback(OnXFieldChanging);
            m_YField.RegisterValueChangingCallback(OnYFieldChanging);
            m_HField.RegisterValueChangingCallback(OnHFieldChanging);
            m_WField.RegisterValueChangingCallback(OnWFieldChanging);
        }

        /// <summary>
        /// The content container of the RectField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the RectField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                m_XField.size = m_Size;
                m_YField.size = m_Size;
                m_HField.size = m_Size;
                m_WField.size = m_Size;

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Set the value of the RectField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the RectField. </param>
        public void SetValueWithoutNotify(Rect newValue)
        {
            m_Value = newValue;
            m_LastValue = m_Value;
            m_XField.SetValueWithoutNotify(m_Value.x);
            m_YField.SetValueWithoutNotify(m_Value.y);
            m_HField.SetValueWithoutNotify(m_Value.height);
            m_WField.SetValueWithoutNotify(m_Value.width);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The value of the RectField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Rect value
        {
            get => m_Value;
            set
            {
                if (m_LastValue == m_Value && m_Value == value)
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<Rect>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The invalid state of the RectField.
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

                m_XField.EnableInClassList(Styles.invalidUssClassName, value);
                m_YField.EnableInClassList(Styles.invalidUssClassName, value);
                m_HField.EnableInClassList(Styles.invalidUssClassName, value);
                m_WField.EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// The validation function of the RectField.
        /// </summary>
        [CreateProperty]
        public Func<Rect, bool> validateValue
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

        void OnHFieldChanged(ChangeEvent<float> evt)
        {
            value = new Rect(value.x, value.y, value.width, evt.newValue);
        }

        void OnWFieldChanged(ChangeEvent<float> evt)
        {
            value = new Rect(value.x, value.y, evt.newValue, value.height);
        }

        void OnYFieldChanged(ChangeEvent<float> evt)
        {
            value = new Rect(value.x, evt.newValue, value.width, value.height);
        }

        void OnXFieldChanged(ChangeEvent<float> evt)
        {
            value = new Rect(evt.newValue, value.y, value.width, value.height);
        }

        void OnHFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Rect(value.x, value.y, value.width, evt.newValue);
            TrySendChangingEvent(val);
        }

        void OnWFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Rect(value.x, value.y, evt.newValue, value.height);
            TrySendChangingEvent(val);
        }

        void OnYFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Rect(value.x, evt.newValue, value.width, value.height);
            TrySendChangingEvent(val);
        }

        void OnXFieldChanging(ChangingEvent<float> evt)
        {
            evt.StopPropagation();
            var val = new Rect(evt.newValue, value.y, value.width, value.height);
            TrySendChangingEvent(val);
        }

        void TrySendChangingEvent(Rect newVal)
        {
            var previousValue = m_Value;
            m_Value = newVal;

            if (m_Value != previousValue)
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<Rect>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

    }
}
