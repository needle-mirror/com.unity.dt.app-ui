using System;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Numerical Field UI element.
    /// </summary>
    /// <typeparam name="TValue">The type of the numerical value.</typeparam>
    [UxmlElement]
    public abstract partial class NumericalField<TValue>
        : ExVisualElement,
            IInputElement<TValue>,
            ISizeableElement,
            INotifyValueChanging<TValue>,
            IFormattable<TValue>
            , IValueField<TValue>
        where TValue : struct, IComparable, IComparable<TValue>, IFormattable
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId formatStringProperty = new BindingId(nameof(formatString));

        internal static readonly BindingId formatFunctionProperty = new BindingId(nameof(formatFunction));

        internal static readonly BindingId unitProperty = new BindingId(nameof(unit));

        internal static readonly BindingId invalidProperty = new BindingId(nameof(invalid));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId lowValueProperty = new BindingId(nameof(lowValue));

        internal static readonly BindingId highValueProperty = new BindingId(nameof(highValue));

        internal static readonly BindingId validateValueProperty = new BindingId(nameof(validateValue));

        internal static readonly BindingId acceptDraggingProperty = new BindingId(nameof(acceptDragging));


        /// <summary>
        /// The NumericalField main styling class.
        /// </summary>
        public const string ussClassName = "appui-numericalfield";

        /// <summary>
        /// The NumericalField input container styling class.
        /// </summary>
        public const string inputContainerUssClassName = ussClassName + "__inputcontainer";

        /// <summary>
        /// The NumericalField input styling class.
        /// </summary>
        public const string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The NumericalField unit styling class.
        /// </summary>
        public const string unitUssClassName = ussClassName + "__unit";

        /// <summary>
        /// The NumericalField trailing container styling class.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The NumericalField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The input container.
        /// </summary>
        protected readonly VisualElement m_InputContainer;

        /// <summary>
        /// The input element.
        /// </summary>
        protected readonly UnityEngine.UIElements.TextField m_InputElement;

        /// <summary>
        /// The size of the element.
        /// </summary>
        protected Size m_Size;

        /// <summary>
        /// The trailing container.
        /// </summary>
        protected readonly VisualElement m_TrailingContainer;

        /// <summary>
        /// The unit element.
        /// </summary>
        protected readonly LocalizedTextElement m_UnitElement;

        /// <summary>
        /// The value of the element.
        /// </summary>
        protected TValue m_Value;

        string m_FormatString;

        string m_PreviousValue;

        /// <summary>
        /// The last value of the element set during <see cref="SetValueWithoutNotify"/>.
        /// </summary>
        protected TValue m_LastValue;

        Optional<TValue> m_LowValue;

        Optional<TValue> m_HighValue;

        Func<TValue, bool> m_ValidateValue;

        FormatFunction<TValue> m_FormatFunction;


        /// <summary>
        /// The unit dragger.
        /// </summary>
        protected readonly FieldMouseDragger<TValue> m_UnitDragger;

        /// <summary>
        /// Base value at the start of a drag operation.
        /// </summary>
        TValue m_DragBaseValue;

        /// <summary>
        /// Whether a drag operation is in progress.
        /// </summary>
        protected bool isDragging { get; private set; }

        /// <summary>
        /// Whether the field accepts dragging to change value.
        /// </summary>
        bool m_AcceptDragging;

        /// <summary>
        /// Whether the field accepts dragging to change value.
        /// Setting this to true will enable drag context listening when the element is attached to a panel.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool acceptDragging
        {
            get => m_AcceptDragging;
            set
            {
                var changed = m_AcceptDragging != value;
                m_AcceptDragging = value;

                if (changed)
                    NotifyPropertyChanged(in acceptDraggingProperty);
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
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

        /// <summary>
        /// The format function of the element.
        /// </summary>
        [CreateProperty]
        public FormatFunction<TValue> formatFunction
        {
            get => m_FormatFunction;
            set
            {
                var changed = m_FormatFunction != value;
                m_FormatFunction = value;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in formatFunctionProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected NumericalField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            delegatesFocus = true;

            m_InputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_InputContainer.AddToClassList(inputContainerUssClassName);
            m_TrailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);

            m_InputElement = new UnityEngine.UIElements.TextField { name = inputUssClassName, pickingMode = PickingMode.Ignore };
            m_InputElement.AddToClassList(inputUssClassName);
            m_InputElement.AddManipulator(new BlinkingCursor());
            m_UnitElement = new LocalizedTextElement { name = unitUssClassName, pickingMode = PickingMode.Position };
            m_UnitElement.AddToClassList(unitUssClassName);

            m_InputContainer.hierarchy.Add(m_InputElement);
            m_TrailingContainer.hierarchy.Add(m_UnitElement);

            hierarchy.Add(m_InputContainer);
            hierarchy.Add(m_TrailingContainer);

            m_InputElement.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_InputElement.RuntimeContextMenu();
            m_InputElement.RegisterValueChangedCallback(OnInputValueChanged);

            m_UnitDragger = new FieldMouseDragger<TValue>(this);
            m_UnitDragger.SetDragZone(m_UnitElement);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            acceptDragging = false;

            size = Size.M;
        }


        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (!acceptDragging)
                return;

            // make sure to unregister first to avoid multiple registrations
            this.UnregisterContextChangedCallback<DragContext>(OnDragContextChanged);
            this.RegisterContextChangedCallback<DragContext>(OnDragContextChanged);
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            this.UnregisterContextChangedCallback<DragContext>(OnDragContextChanged);
        }

        void OnDragContextChanged(ContextChangedEvent<DragContext> evt)
        {
            // DragContext is sent only when the pointer has moved.
            var context = evt.context;
            if (context == null)
                return;

            if (context.phase == DragPhase.Ended)
            {
                // Drag ended, reset state
                StopDragging();
                return;
            }

            if (context.phase == DragPhase.Started)
            {
                // Drag started, initialize state
                StartDragging();
                return;
            }

            ApplyInputDeviceDelta(context.delta, context.speed, m_DragBaseValue);
        }

        // IValueField implementation for drag handling

        /// <summary>
        /// Apply input device delta to the <see cref="IValueField{T}"/> element.
        /// </summary>
        /// <param name="delta"> The delta from the input device. </param>
        /// <param name="speed"> The speed of the delta application. </param>
        /// <param name="startValue"> The starting value for the delta application. </param>
        public abstract void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, TValue startValue);

        /// <summary>
        /// Start dragging operation.
        /// </summary>
        public virtual void StartDragging()
        {
            isDragging = true;
            m_InputElement.textSelection.SelectNone();
            m_DragBaseValue = m_Value;
            MarkDirtyRepaint();
        }

        /// <summary>
        /// Stop dragging operation.
        /// </summary>
        public virtual void StopDragging()
        {
            isDragging = false;
            TrySendChangeEvent(m_DragBaseValue, m_Value);
            MarkDirtyRepaint();
        }


        /// <summary>
        /// Try to send a Changing event.
        /// </summary>
        /// <param name="previousValue"> The previous value. </param>
        /// <param name="newValue"> The new value. </param>
        protected void TrySendChangingEvent(TValue previousValue, TValue newValue)
        {
            if (newValue.CompareTo(previousValue) == 0)
                return;

            using var changeEvent = ChangingEvent<TValue>.GetPooled();
            changeEvent.target = this;
            changeEvent.previousValue = previousValue;
            changeEvent.newValue = m_Value;
            SendEvent(changeEvent);
        }

        void TrySendChangeEvent(TValue previousValue, TValue newValue)
        {
            if (newValue.CompareTo(previousValue) == 0)
                return;

            using var changeEvent = ChangeEvent<TValue>.GetPooled(previousValue, newValue);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }

        void OnInputValueChanged(ChangeEvent<string> evt)
        {

            evt.StopPropagation();
            if (ParseStringToValue(evt.newValue, out var newValue))
            {
                if (lowValue.IsSet)
                    newValue = Max(newValue, lowValue.Value);
                if (highValue.IsSet)
                    newValue = Min(newValue, highValue.Value);

                var previousValue = m_Value;
                m_Value = newValue;

                if (validateValue != null) invalid = !validateValue(newValue);

                if (previousValue.CompareTo(m_Value) == 0)
                    return;

                TrySendChangingEvent(previousValue, m_Value);
            }
            else if (validateValue != null)
            {
                invalid = true;
            }
        }

        /// <summary>
        /// The unit of the element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string unit
        {
            get => m_UnitElement.text;
            set
            {
                var changed = m_UnitElement.text != value;
                m_UnitElement.text = value;
                m_UnitElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_UnitElement.text));

                if (changed)
                    NotifyPropertyChanged(in unitProperty);
            }
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<TValue> lowValue
        {
            get => m_LowValue;
            set
            {
                var changed = m_LowValue != value;
                m_LowValue = value;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in lowValueProperty);
            }
        }

        /// <summary>
        /// Maximum value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<TValue> highValue
        {
            get => m_HighValue;
            set
            {
                var changed = m_HighValue != value;
                m_HighValue = value;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in highValueProperty);
            }
        }

        /// <summary>
        /// The content container of the element.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the element.
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

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Set the value of the element without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the element. </param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            if (lowValue.IsSet)
                newValue = Max(newValue, lowValue.Value);
            if (highValue.IsSet)
                newValue = Min(newValue, highValue.Value);
            m_Value = newValue;
            m_LastValue = m_Value;
            var valStr = ParseValueToString(newValue);
            m_InputElement.SetValueWithoutNotify(valStr);
            if (validateValue != null) invalid = !validateValue(newValue);
        }

        /// <summary>
        /// The value of the element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TValue value
        {
            get => string.IsNullOrEmpty(m_InputElement.value) && ParseStringToValue(m_InputElement.value, out var val) ? val : m_Value;
            set
            {
                var val = value;
                if (lowValue.IsSet)
                    val = Max(val, lowValue.Value);
                if (highValue.IsSet)
                    val = Min(val, highValue.Value);
                if (AreEqual(m_LastValue, val) && AreEqual(m_Value, val))
                    return;

                var previousValue = m_LastValue;
                SetValueWithoutNotify(val);
                TrySendChangeEvent(previousValue, m_Value);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The invalid state of the element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = ClassListContains(Styles.invalidUssClassName) != value;
                EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// Method to validate the value.
        /// </summary>
        [CreateProperty]
        public Func<TValue, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                if (m_ValidateValue != null)
                    invalid = !m_ValidateValue(this.value);

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        void OnFocusedOut(FocusOutEvent evt)
        {
            RemoveFromClassList(Styles.focusedUssClassName);
            RemoveFromClassList(Styles.keyboardFocusUssClassName);

            var valueStr = ParseValueToString(m_Value);
            var val = valueStr != m_InputElement.value && ParseStringToValue(m_InputElement.value, out var newValue) ? newValue : m_Value;
            value = val;
            SetValueWithoutNotify(val);
            m_InputElement.cursorIndex = 0;
        }

        void OnFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            passMask = 0;
            m_InputElement.SetValueWithoutNotify(ParseRawValueToString(m_Value));
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            AddToClassList(Styles.keyboardFocusUssClassName);
            passMask = Passes.Clear | Passes.Outline;
            m_InputElement.SetValueWithoutNotify(ParseRawValueToString(m_Value));
        }

        /// <summary>
        /// Define the conversion from the <see cref="string"/> value to a <typeparamref name="TValue"/> value.
        /// </summary>
        /// <param name="strValue">The <see cref="string"/> value to convert.</param>
        /// <param name="val">The <typeparamref name="TValue"/> value returned.</param>
        /// <returns>True if the conversion is possible, False otherwise.</returns>
        protected abstract bool ParseStringToValue(string strValue, out TValue val);

        /// <summary>
        /// Define the conversion from a <typeparamref name="TValue"/> value to a <see cref="string"/> value.
        /// </summary>
        /// <param name="val">The <typeparamref name="TValue"/> value to convert.</param>
        /// <returns>The converted value.</returns>
        protected abstract string ParseValueToString(TValue val);

        /// <summary>
        /// Define the conversion from a <typeparamref name="TValue"/> value to a <see cref="string"/> value.
        /// </summary>
        /// <param name="val"> The <typeparamref name="TValue"/> value to convert. </param>
        /// <returns> The converted value. </returns>
        /// <remarks>
        /// This method is used to convert the value to a string without any formatting.
        /// </remarks>
        protected abstract string ParseRawValueToString(TValue val);

        /// <summary>
        /// Check if two values of type <typeparamref name="TValue"/> are equal.
        /// </summary>
        /// <param name="a">The first value to test.</param>
        /// <param name="b">The second value to test.</param>
        /// <returns>True if both values are considered equals, false otherwise.</returns>
        protected abstract bool AreEqual(TValue a, TValue b);

        /// <summary>
        /// Increment a given value with a given delta.
        /// </summary>
        /// <param name="originalValue">The original value.</param>
        /// <param name="delta">The delta used for increment.</param>
        /// <returns>The incremented value.</returns>
        protected abstract TValue Increment(TValue originalValue, float delta);

        /// <summary>
        /// Return the smallest value between a and b.
        /// </summary>
        /// <param name="a">The first value to test.</param>
        /// <param name="b">The second value to test.</param>
        /// <returns>The smallest value.</returns>
        protected abstract TValue Min(TValue a, TValue b);

        /// <summary>
        /// Return the biggest value between a and b.
        /// </summary>
        /// <param name="a">The first value to test.</param>
        /// <param name="b">The second value to test.</param>
        /// <returns>The biggest value.</returns>
        protected abstract TValue Max(TValue a, TValue b);

        /// <summary>
        /// Calculate the increment factor based on a base value.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <returns>The increment factor.</returns>
        protected abstract float GetIncrementFactor(TValue baseValue);

        /// <summary>
        /// Calculate the increment factor based on a base value and modifiers.
        /// </summary>
        /// <param name="baseValue"> The base value.</param>
        /// <param name="fastModifier"> The fast modifier key state (e.g., Shift key).</param>
        /// <param name="slowModifier"> The slow modifier key state (e.g., Alt key).</param>
        /// <returns> The increment factor.</returns>
        protected virtual float GetIncrementFactor(TValue baseValue, bool fastModifier, bool slowModifier) =>
            GetIncrementFactor(baseValue) * (fastModifier ? 10f : slowModifier ? 0.1f : 1f);

        /// <summary>
        /// Internal method to invoke GetIncrementFactor from tests.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <returns>The increment factor.</returns>
        internal float InvokeGetIncrementFactor(TValue baseValue) => GetIncrementFactor(baseValue);

    }
}
