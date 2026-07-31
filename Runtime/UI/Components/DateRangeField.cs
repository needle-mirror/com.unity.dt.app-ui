using System;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A form input component that allows users to select a date range with a visual calendar picker.
    /// </summary>
    /// <remarks>
    /// DateRangeField is a specialized input component designed for selecting a range of dates. It combines two
    /// text inputs for direct date entry with a calendar picker interface for visual date selection.
    ///
    /// The component provides two ways to input dates:
    /// 1. Direct text input: Users can type dates directly into the start and end date fields
    /// 2. Calendar picker: Clicking the calendar icon opens a popover with a visual date range picker
    ///
    /// **Tip:** The field supports keyboard navigation, validation, and various date formatting options to ensure
    /// proper date entry.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML — Creates a medium-sized date range field with MM/dd/yyyy format and initial date
    /// range.</para>
    /// <code lang="xml"><![CDATA[
    /// <DateRangeField name="bookingDates"
    ///              size="M"
    ///              format-string="MM/dd/yyyy"
    ///              value="2023-12-25,2023-12-31" />
    /// ]]></code>
    /// <para>Complete C# example with validation and change handling — Creates a date range field that validates the
    /// selection is between 2 and 14 days and logs changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// var dateRangeField = new DateRangeField {
    ///     size = Size.M,
    ///     formatString = "MM/dd/yyyy",
    ///     validateValue = range => {
    ///         var minStay = 2;
    ///         var maxStay = 14;
    ///         var days = (range.end - range.start).Days;
    ///         return days >= minStay && days <= maxStay;
    ///     }
    /// };
    ///
    /// dateRangeField.RegisterValueChangedCallback(evt => {
    ///     var range = evt.newValue;
    ///     Debug.Log($"Selected range: {range.start} to {range.end}");
    /// });
    /// ]]></code>
    /// </example>
    [VisualDocPage("inputs")]
    [UxmlElement]
    public partial class DateRangeField : ExVisualElement, IInputElement<DateRange>, INotifyValueChanging<DateRange>, ISizeableElement
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId formatStringProperty = nameof(formatString);


        /// <summary>
        /// The DateField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-date-field";

        /// <summary>
        /// The DateField input container styling class.
        /// </summary>
        public static readonly string inputContainerUssClassName = ussClassName + "__input-container";

        /// <summary>
        /// The DateField input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The DateField separator styling class.
        /// </summary>
        public static readonly string separatorUssClassName = ussClassName + "__separator";

        /// <summary>
        /// The DateField picker button styling class.
        /// </summary>
        public static readonly string pickerButtonUssClassName = ussClassName + "__picker-button";

        /// <summary>
        /// The DateField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly UnityEngine.UIElements.TextField m_StartInputElement;

        readonly UnityEngine.UIElements.TextField m_EndInputElement;

        readonly VisualElement m_PickerButton;

        DateRange m_Value;

        Size m_Size;

        DateRange m_PreviousValue;

        DateRangePicker m_Picker;

        Func<DateRange, bool> m_ValidateValue;

        Popover m_Popover;

        string m_FormatString;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DateRangeField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;

            var inputContainer = new VisualElement
            {
                name = inputContainerUssClassName,
                pickingMode = PickingMode.Ignore
            };
            inputContainer.AddToClassList(inputContainerUssClassName);

            m_StartInputElement = new UnityEngine.UIElements.TextField()
            {
                name = inputUssClassName + "__start",
                pickingMode = PickingMode.Ignore,
                isDelayed = true
            };
            m_StartInputElement.AddToClassList(inputUssClassName);
            m_StartInputElement.RegisterValueChangedCallback(OnInputValueChanged);
            m_StartInputElement.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);

            var separator = new Text
            {
                text = " - ",
                name = separatorUssClassName,
                pickingMode = PickingMode.Ignore
            };
            separator.AddToClassList(separatorUssClassName);

            m_EndInputElement = new UnityEngine.UIElements.TextField()
            {
                name = inputUssClassName + "__end",
                pickingMode = PickingMode.Ignore,
                isDelayed = true
            };
            m_EndInputElement.AddToClassList(inputUssClassName);
            m_EndInputElement.RegisterValueChangedCallback(OnInputValueChanged);
            m_EndInputElement.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);

            m_PickerButton = new VisualElement()
            {
                focusable = true,
                pickingMode = PickingMode.Position,
                name = pickerButtonUssClassName,
            };
            m_PickerButton.AddToClassList(pickerButtonUssClassName);
            m_PickerButton.AddManipulator(new Pressable(OnClick));
            var icon = new Icon { iconName = "calendar", pickingMode = PickingMode.Ignore };
            m_PickerButton.Add(icon);

            inputContainer.Add(m_StartInputElement);
            inputContainer.Add(separator);
            inputContainer.Add(m_EndInputElement);
            hierarchy.Add(inputContainer);
            hierarchy.Add(m_PickerButton);

            size = Size.M;
            SetValueWithoutNotify(new DateRange(Date.now, new Date(DateTime.Now.AddDays(1))));
            formatString = "yyyy-MM-dd";
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnInputKeyDown(KeyDownEvent evt)
        {
            var delta = evt.keyCode switch
            {
                KeyCode.UpArrow => 1,
                KeyCode.DownArrow => -1,
                _ => 0
            };

            if (delta == 0)
                return;

            evt.StopPropagation();
            if (evt.target == m_StartInputElement.Q<TextElement>())
            {
                var newStartDate = ((DateTime)m_Value.start).AddDays(delta);
                var newEndDate = newStartDate > m_Value.end ? newStartDate : m_Value.end;
                value = new DateRange(new Date(newStartDate), new Date(newEndDate));
            }
            else if (evt.target == m_EndInputElement.Q<TextElement>())
            {
                var newEndDate = ((DateTime)m_Value.end).AddDays(delta);
                var newStartDate = newEndDate < m_Value.start ? newEndDate : m_Value.start;
                value = new DateRange(new Date(newStartDate), new Date(newEndDate));
            }
        }

        void OnInputValueChanged(ChangeEvent<string> e)
        {
            if (e.target == m_StartInputElement)
            {
                var newStartDate = !string.IsNullOrEmpty(e.newValue) && DateTime.TryParse(e.newValue, out var date)
                    ? new Date(date)
                    : m_PreviousValue.start;
                var endDate = newStartDate > m_PreviousValue.end ? ((DateTime)newStartDate).AddDays(1) : m_PreviousValue.end;
                value = new DateRange(newStartDate, new Date(endDate));
            }
            else if (e.target == m_EndInputElement)
            {
                var newEndDate = !string.IsNullOrEmpty(e.newValue) && DateTime.TryParse(e.newValue, out var date)
                    ? new Date(date)
                    : m_PreviousValue.end;
                var startDate = newEndDate < m_PreviousValue.start ? ((DateTime)newEndDate).AddDays(-1) : m_PreviousValue.start;
                value = new DateRange(new Date(startDate), newEndDate);
            }
            SetFormattedString();
        }

        void OnClick()
        {
            m_Picker?.parent?.Remove(m_Picker);

            m_PreviousValue = value;
            m_Picker ??= new DateRangePicker
            {
                //todo add settings
            };
            m_Picker.SetValueWithoutNotify(m_PreviousValue);
            m_Picker.RegisterValueChangedCallback(OnPickerValueChanged);
            if (m_Popover != null)
            {
                m_Popover.Dismiss(DismissType.Consecutive);
                m_Popover.dismissed -= OnPopoverDismissed;
            }
            m_Popover = Popover.Build(this, m_Picker);
            m_Popover.dismissed += OnPopoverDismissed;
            m_Popover.Show();
            AddToClassList(Styles.focusedUssClassName);
        }

        void OnPopoverDismissed(Popover popover, DismissType reason)
        {
            popover.dismissed -= OnPopoverDismissed;
            if (popover == m_Popover)
                m_Popover = null;
            RemoveFromClassList(Styles.focusedUssClassName);
            m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
            if (m_PreviousValue != m_Picker.value)
            {
                using var evt = ChangeEvent<DateRange>.GetPooled(m_PreviousValue, m_Picker.value);
                SetValueWithoutNotify(m_Picker.value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
            Focus();
        }

        void OnPickerValueChanged(ChangeEvent<DateRange> e)
        {
            if (e.newValue != value)
            {
                SetValueWithoutNotify(e.newValue);
                using var evt = ChangingEvent<DateRange>.GetPooled();
                evt.previousValue = m_PreviousValue;
                evt.newValue = e.newValue;
                evt.target = this;
                SendEvent(evt);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_PreviousValue = value;
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_PreviousValue = value;
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// The content container of this DateField. This is null for DateField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The DateField size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The DateField invalid state.
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
        /// The DateField validation function.
        /// </summary>
        [CreateProperty]
        public Func<DateRange, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Sets the DateField value without notifying the DateField.
        /// </summary>
        /// <param name="newValue"> The new DateField value. </param>
        public void SetValueWithoutNotify(DateRange newValue)
        {
            m_Value = newValue;
            SetFormattedString();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        void SetFormattedString()
        {
            var formattedStartValue = string.IsNullOrEmpty(m_FormatString)
                ? ((DateTime)m_Value.start).ToString(CultureInfo.InvariantCulture)
                : ((DateTime)m_Value.start).ToString(m_FormatString, CultureInfo.InvariantCulture);
            m_StartInputElement.SetValueWithoutNotify(formattedStartValue);

            var formattedEndValue = string.IsNullOrEmpty(m_FormatString)
                ? ((DateTime)m_Value.end).ToString(CultureInfo.InvariantCulture)
                : ((DateTime)m_Value.end).ToString(m_FormatString, CultureInfo.InvariantCulture);
            m_EndInputElement.SetValueWithoutNotify(formattedEndValue);
        }

        /// <summary>
        /// The DateField value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public DateRange value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;

                using var evt = ChangeEvent<DateRange>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The DateField string formatting.
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
                SetFormattedString();

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

    }
}
