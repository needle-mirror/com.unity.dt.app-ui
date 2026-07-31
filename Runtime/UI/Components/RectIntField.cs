using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A field component for editing integer rectangle values with position (x,y) and size (width,height).
    /// </summary>
    /// <remarks>
    /// The RectIntField is a specialized input component that allows users to edit rectangle properties using
    /// integer values. It provides a convenient way to manipulate both position (X,Y) and size (Width,Height) of
    /// a rectangle in a single component.
    ///
    /// The field is organized into two rows: Position and Size, each containing two numerical inputs. Position
    /// controls the X and Y coordinates, while Size controls the Width and Height values.
    ///
    /// **Tip:** This component is particularly useful in scenarios where you need to edit rectangular bounds,
    /// layouts, or any other rectangle-based properties in your application.
    ///
    /// The component supports validation through a callback function and can be styled using different size
    /// variants.
    /// </remarks>
    /// <example>
    /// <para>Basic usage with default values:
    /// Creating a basic RectIntField in UXML with initial values.</para>
    /// <code lang="xml"><![CDATA[
    /// <RectIntField name="boundingBox" value="0,0,100,100" />
    /// ]]></code>
    /// <para>Advanced usage with validation and value change handling:
    /// Creating a RectIntField in code with validation and change handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var rectField = new RectIntField();
    /// rectField.validateValue = (rect) => {
    ///     return rect.width >= 0 && rect.height >= 0 && rect.x >= 0 && rect.y >= 0;
    /// };
    /// rectField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Rectangle changed to: {evt.newValue}");
    /// });
    /// rectField.value = new RectInt(10, 10, 200, 100);
    /// ]]></code>
    /// <para>Using RectIntField in a layout editor:
    /// Using RectIntField as part of a UI layout editor with custom styling.</para>
    /// <code lang="xml"><![CDATA[
    /// <RectIntField name="elementBounds" size="M" value="50,50,300,200">
    ///     <Style src="ElementEditor.uss" />
    /// </RectIntField>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class RectIntField : BaseVisualElement, IInputElement<RectInt>, ISizeableElement, INotifyValueChanging<RectInt>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId sizeProperty = nameof(size);


        /// <summary>
        /// The RectIntField main styling class.
        /// </summary>
        public const string ussClassName = "appui-rectfield";

        /// <summary>
        /// The RectIntField row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The RectIntField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The RectIntField X NumericalField styling class.
        /// </summary>
        public const string xFieldUssClassName = ussClassName + "__x-field";

        /// <summary>
        /// The RectIntField Y NumericalField styling class.
        /// </summary>
        public const string yFieldUssClassName = ussClassName + "__y-field";

        /// <summary>
        /// The RectIntField H NumericalField styling class.
        /// </summary>
        public const string hFieldUssClassName = ussClassName + "__h-field";

        /// <summary>
        /// The RectIntField W NumericalField styling class.
        /// </summary>
        public const string wFieldUssClassName = ussClassName + "__w-field";

        /// <summary>
        /// The RectIntField Label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Size m_Size;

        RectInt m_LastValue;

        RectInt m_Value;

        readonly IntField m_WField;

        readonly IntField m_XField;

        readonly IntField m_YField;

        readonly IntField m_HField;

        Func<RectInt, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RectIntField()
        {
            AddToClassList(ussClassName);

            var xFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_XField = new IntField { name = xFieldUssClassName, unit = "X" };
            xFieldContainer.AddToClassList(xFieldUssClassName);
            xFieldContainer.Add(m_XField);

            var yFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_YField = new IntField { name = yFieldUssClassName, unit = "Y" };
            yFieldContainer.AddToClassList(yFieldUssClassName);
            yFieldContainer.Add(m_YField);

            var wFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_WField = new IntField { name = wFieldUssClassName, unit = "W" };
            wFieldContainer.AddToClassList(wFieldUssClassName);
            wFieldContainer.Add(m_WField);

            var hFieldContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_HField = new IntField { name = hFieldUssClassName, unit = "H" };
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
            SetValueWithoutNotify(new RectInt(0, 0, 0, 0));

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
        /// The content container of the RectIntField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the RectIntField.
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
        /// Set the value of the RectIntField without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the RectIntField. </param>
        public void SetValueWithoutNotify(RectInt newValue)
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
        /// The value of the RectIntField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public RectInt value
        {
            get => m_Value;
            set
            {
                if (m_LastValue.Equals(m_Value) && m_Value.Equals(value))
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(value);
                using var evt = ChangeEvent<RectInt>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The invalid state of the RectIntField.
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
        /// The callback to validate the value of the RectIntField.
        /// </summary>
        [CreateProperty]
        public Func<RectInt, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !validateValue?.Invoke(m_Value) ?? false;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        void OnHFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, value.y, value.width, evt.newValue);
        }

        void OnWFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, value.y, evt.newValue, value.height);
        }

        void OnYFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(value.x, evt.newValue, value.width, value.height);
        }

        void OnXFieldChanged(ChangeEvent<int> evt)
        {
            value = new RectInt(evt.newValue, value.y, value.width, value.height);
        }

        void OnHFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            var val = new RectInt(value.x, value.y, value.width, evt.newValue);
            TrySendChangingEvent(val);
        }

        void OnWFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            var val = new RectInt(value.x, value.y, evt.newValue, value.height);
            TrySendChangingEvent(val);
        }

        void OnYFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            var val = new RectInt(value.x, evt.newValue, value.width, value.height);
            TrySendChangingEvent(val);
        }

        void OnXFieldChanging(ChangingEvent<int> evt)
        {
            evt.StopPropagation();
            var val = new RectInt(evt.newValue, value.y, value.width, value.height);
            TrySendChangingEvent(val);
        }

        void TrySendChangingEvent(RectInt newVal)
        {
            var previousValue = m_Value;
            m_Value = newVal;

            if (!m_Value.Equals(previousValue))
            {
                if (validateValue != null) invalid = !validateValue(m_Value);

                using var changeEvent = ChangingEvent<RectInt>.GetPooled();
                changeEvent.target = this;
                changeEvent.previousValue = previousValue;
                changeEvent.newValue = m_Value;
                SendEvent(changeEvent);
            }
        }

    }
}
