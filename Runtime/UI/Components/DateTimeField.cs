using System;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A form input control that allows users to select both a date and a time of day.
    /// </summary>
    /// <remarks>
    /// The DateTimeField is a composition rather than a new parser: it pairs the date entry rules of
    /// <see cref="DateField"/> with the segmented time entry of <see cref="TimeField"/>, and its picker
    /// popover stacks a <see cref="DatePicker"/> above a <see cref="TimePicker"/>.
    ///
    /// Both halves read and write one <see cref="DateTime"/>. Picking a date preserves the time of day and
    /// changing the time preserves the calendar date, so neither half can silently reset the other.
    ///
    /// Key features:
    /// - A date segment with a configurable <see cref="dateFormat"/>, stepped a day at a time with the arrow keys
    /// - A time segment with per-segment stepping, a 12 or 24-hour clock and optional seconds
    /// - A combined calendar and spinner picker
    /// - Optional <see cref="min"/> and <see cref="max"/> bounds, which clamp the value
    /// - Size variants and value validation
    ///
    /// NOTE: Like <see cref="DateField"/>, the date is formatted and parsed with the invariant culture, so
    /// the displayed order of the parts follows <see cref="dateFormat"/> and not the machine locale.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:DateTimeField name="starts-at" />
    /// </UXML>
    /// ]]></code>
    /// <para>A US-style 12-hour field bounded to a single day:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:DateTimeField
    ///         name="appointment"
    ///         size="Lg"
    ///         date-format="MM/dd/yyyy"
    ///         hour12="true"
    ///         minute-step="15"
    ///         value="2024-01-01 09:00:00"
    ///         min="2024-01-01 08:00:00"
    ///         max="2024-01-01 18:00:00" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating and configuring a DateTimeField in C#:</para>
    /// <code lang="csharp"><![CDATA[
    /// var field = new DateTimeField
    /// {
    ///     value = new DateTime(2024, 1, 1, 9, 0, 0),
    ///     minuteStep = 15,
    ///     showSeconds = false
    /// };
    ///
    /// field.validateValue = dt => dt.DayOfWeek != DayOfWeek.Sunday;
    ///
    /// field.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Selected: {evt.newValue:yyyy-MM-dd HH:mm}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class DateTimeField : ExVisualElement, IInputElement<DateTime>, INotifyValueChanging<DateTime>, ISizeableElement
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId dateFormatProperty = nameof(dateFormat);

        internal static readonly BindingId hour12Property = nameof(hour12);

        internal static readonly BindingId showSecondsProperty = nameof(showSeconds);

        internal static readonly BindingId minuteStepProperty = nameof(minuteStep);

        internal static readonly BindingId minProperty = nameof(min);

        internal static readonly BindingId maxProperty = nameof(max);

        /// <summary>
        /// The DateTimeField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-datetime-field";

        /// <summary>
        /// The DateTimeField date input styling class.
        /// </summary>
        public static readonly string dateInputUssClassName = ussClassName + "__date-input";

        /// <summary>
        /// The DateTimeField time input styling class.
        /// </summary>
        public static readonly string timeInputUssClassName = ussClassName + "__time-input";

        /// <summary>
        /// The DateTimeField separator styling class.
        /// </summary>
        public static readonly string separatorUssClassName = ussClassName + "__separator";

        /// <summary>
        /// The DateTimeField picker button styling class.
        /// </summary>
        public static readonly string pickerButtonUssClassName = ussClassName + "__picker-button";

        /// <summary>
        /// The DateTimeField picker pane styling class, applied to the popover content.
        /// </summary>
        public static readonly string pickerPaneUssClassName = ussClassName + "__picker-pane";

        /// <summary>
        /// The DateTimeField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The default date format string.
        /// </summary>
        public const string defaultDateFormat = "yyyy-MM-dd";

        readonly UnityEngine.UIElements.TextField m_DateInput;

        readonly UnityEngine.UIElements.TextField m_TimeInput;

        readonly VisualElement m_PickerButton;

        DateTime m_Value;

        DateTime m_PreviousValue;

        Size m_Size;

        Func<DateTime, bool> m_ValidateValue;

        Popover m_Popover;

        DatePicker m_DatePicker;

        TimePicker m_TimePicker;

        string m_DateFormat = defaultDateFormat;

        bool m_Hour12;

        bool m_ShowSeconds;

        int m_MinuteStep = 1;

        DateTime m_Min = DateTime.MinValue;

        DateTime m_Max = DateTime.MaxValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DateTimeField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;

            m_DateInput = new UnityEngine.UIElements.TextField
            {
                name = dateInputUssClassName,
                pickingMode = PickingMode.Position,
                isDelayed = true,
            };
            m_DateInput.AddToClassList(dateInputUssClassName);
            m_DateInput.RegisterValueChangedCallback(OnDateInputValueChanged);
            m_DateInput.RegisterCallback<KeyDownEvent>(OnDateInputKeyDown, TrickleDown.TrickleDown);

            var separator = new VisualElement
            {
                name = separatorUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            separator.AddToClassList(separatorUssClassName);

            m_TimeInput = new UnityEngine.UIElements.TextField
            {
                name = timeInputUssClassName,
                pickingMode = PickingMode.Position,
                isDelayed = true,
            };
            m_TimeInput.AddToClassList(timeInputUssClassName);
            m_TimeInput.RegisterValueChangedCallback(OnTimeInputValueChanged);
            m_TimeInput.RegisterCallback<KeyDownEvent>(OnTimeInputKeyDown, TrickleDown.TrickleDown);

            m_PickerButton = new VisualElement
            {
                focusable = true,
                pickingMode = PickingMode.Position,
                name = pickerButtonUssClassName,
            };
            m_PickerButton.AddToClassList(pickerButtonUssClassName);
            m_PickerButton.AddManipulator(new Pressable(OnClick));
            var icon = new Icon { primary = true, iconName = "calendar", pickingMode = PickingMode.Ignore };
            m_PickerButton.Add(icon);

            hierarchy.Add(m_DateInput);
            hierarchy.Add(separator);
            hierarchy.Add(m_TimeInput);
            hierarchy.Add(m_PickerButton);

            size = Size.Md;
            SetValueWithoutNotify(DateTime.Now);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// The content container of this DateTimeField. This is null for DateTimeField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The DateTimeField size.
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
        /// The DateTimeField invalid state.
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
        /// The DateTimeField validation function.
        /// </summary>
        [CreateProperty]
        public Func<DateTime, bool> validateValue
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
        /// The format string of the date part. Defaults to `yyyy-MM-dd`.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("date-format")]
        public string dateFormat
        {
            get => m_DateFormat;
            set
            {
                var newValue = string.IsNullOrEmpty(value) ? defaultDateFormat : value;
                var changed = m_DateFormat != newValue;
                m_DateFormat = newValue;
                SetFormattedStrings();

                if (changed)
                    NotifyPropertyChanged(in dateFormatProperty);
            }
        }

        /// <summary>
        /// Whether the time part uses a 12-hour clock with an AM/PM segment. Defaults to false.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("hour12")]
        public bool hour12
        {
            get => m_Hour12;
            set
            {
                var changed = m_Hour12 != value;
                m_Hour12 = value;
                SetFormattedStrings();
                if (m_TimePicker != null)
                    m_TimePicker.hour12 = value;

                if (changed)
                    NotifyPropertyChanged(in hour12Property);
            }
        }

        /// <summary>
        /// Whether the time part displays a seconds segment. Defaults to false.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("show-seconds")]
        public bool showSeconds
        {
            get => m_ShowSeconds;
            set
            {
                var changed = m_ShowSeconds != value;
                m_ShowSeconds = value;
                SetFormattedStrings();
                if (m_TimePicker != null)
                    m_TimePicker.showSeconds = value;

                if (changed)
                    NotifyPropertyChanged(in showSecondsProperty);
            }
        }

        /// <summary>
        /// The number of minutes stepped per arrow press or spinner click. Defaults to 1.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("minute-step")]
        public int minuteStep
        {
            get => m_MinuteStep;
            set
            {
                var newValue = Mathf.Max(1, value);
                var changed = m_MinuteStep != newValue;
                m_MinuteStep = newValue;
                if (m_TimePicker != null)
                    m_TimePicker.minuteStep = newValue;

                if (changed)
                    NotifyPropertyChanged(in minuteStepProperty);
            }
        }

        /// <summary>
        /// The earliest accepted value. Defaults to <see cref="DateTime.MinValue"/>, meaning unbounded.
        /// </summary>
        /// <remarks>
        /// Setting a bound clamps the current value into range immediately.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public DateTime min
        {
            get => m_Min;
            set
            {
                var changed = m_Min != value;
                m_Min = value;
                this.value = Clamp(m_Value);

                if (changed)
                    NotifyPropertyChanged(in minProperty);
            }
        }

        /// <summary>
        /// The latest accepted value. Defaults to <see cref="DateTime.MaxValue"/>, meaning unbounded.
        /// </summary>
        /// <remarks>
        /// Setting a bound clamps the current value into range immediately.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public DateTime max
        {
            get => m_Max;
            set
            {
                var changed = m_Max != value;
                m_Max = value;
                this.value = Clamp(m_Value);

                if (changed)
                    NotifyPropertyChanged(in maxProperty);
            }
        }

        /// <summary>
        /// The DateTimeField value.
        /// </summary>
        /// <remarks>
        /// The value is clamped into <see cref="min"/> and <see cref="max"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public DateTime value
        {
            get => m_Value;
            set
            {
                var newValue = Clamp(value);
                if (m_Value == newValue)
                    return;

                using var evt = ChangeEvent<DateTime>.GetPooled(m_Value, newValue);
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The date part of the value.
        /// </summary>
        public Date date => new Date(m_Value);

        /// <summary>
        /// The time-of-day part of the value.
        /// </summary>
        public TimeSpan time => m_Value.TimeOfDay;

        /// <summary>
        /// Sets the DateTimeField value without notifying the DateTimeField.
        /// </summary>
        /// <param name="newValue"> The new DateTimeField value. It is clamped into the bounds. </param>
        public void SetValueWithoutNotify(DateTime newValue)
        {
            m_Value = Clamp(newValue);
            SetFormattedStrings();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// Steps a single segment of the time part, wrapping inside that segment's own unit.
        /// </summary>
        /// <param name="segment"> The segment to step. </param>
        /// <param name="delta"> The number of steps, negative to step down. </param>
        public void StepSegment(TimeSegment segment, int delta)
        {
            var stepped = TimeField.Step(m_Value.TimeOfDay, segment, delta, m_MinuteStep);
            value = m_Value.Date + stepped;
        }

        DateTime Clamp(DateTime dateTime)
        {
            if (m_Min > m_Max)
                return dateTime;
            if (dateTime < m_Min)
                return m_Min;
            if (dateTime > m_Max)
                return m_Max;
            return dateTime;
        }

        void SetFormattedStrings()
        {
            m_DateInput.SetValueWithoutNotify(m_Value.ToString(m_DateFormat, CultureInfo.InvariantCulture));
            m_TimeInput.SetValueWithoutNotify(TimeField.Format(m_Value.TimeOfDay, m_Hour12, m_ShowSeconds));
        }

        void OnDateInputValueChanged(ChangeEvent<string> e)
        {
            if (!string.IsNullOrEmpty(e.newValue) && DateTime.TryParse(e.newValue,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                value = parsed.Date + m_Value.TimeOfDay;
            SetFormattedStrings();
        }

        void OnTimeInputValueChanged(ChangeEvent<string> e)
        {
            if (TimeField.TryParseTime(e.newValue, m_Hour12, m_ShowSeconds, out var parsed))
                value = m_Value.Date + parsed;
            SetFormattedStrings();
        }

        void OnDateInputKeyDown(KeyDownEvent evt)
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
            value = AddDaysClamped(m_Value, delta);
        }

        /// <summary>
        /// Adds whole days without overflowing the representable range: <see cref="DateTime.AddDays"/>
        /// throws at <see cref="DateTime.MaxValue"/> and <see cref="DateTime.MinValue"/>, which are
        /// both legal values here, so stepping past the boundary saturates instead.
        /// </summary>
        static DateTime AddDaysClamped(DateTime dateTime, int delta)
        {
            if (delta > 0 && (DateTime.MaxValue - dateTime).TotalDays < delta)
                return DateTime.MaxValue;
            if (delta < 0 && (dateTime - DateTime.MinValue).TotalDays < -delta)
                return DateTime.MinValue;
            return dateTime.AddDays(delta);
        }

        void OnTimeInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode is not (KeyCode.UpArrow or KeyCode.DownArrow))
                return;

            evt.StopPropagation();
            var segment = TimeField.SegmentAtCaret(m_TimeInput.textSelection.cursorIndex, m_Hour12, m_ShowSeconds);
            StepSegment(segment, evt.keyCode == KeyCode.UpArrow ? 1 : -1);
        }

        void OnClick()
        {
            m_PreviousValue = value;

            m_DatePicker ??= new DatePicker();
            m_TimePicker ??= new TimePicker();
            m_TimePicker.hour12 = m_Hour12;
            m_TimePicker.showSeconds = m_ShowSeconds;
            m_TimePicker.minuteStep = m_MinuteStep;

            m_DatePicker.SetValueWithoutNotify(new Date(m_PreviousValue));
            m_TimePicker.SetValueWithoutNotify(m_PreviousValue.TimeOfDay);
            m_DatePicker.RegisterValueChangedCallback(OnDatePickerValueChanged);
            m_TimePicker.RegisterValueChangedCallback(OnTimePickerValueChanged);

            var pane = new VisualElement { name = pickerPaneUssClassName };
            pane.AddToClassList(pickerPaneUssClassName);
            pane.Add(m_DatePicker);
            pane.Add(m_TimePicker);

            if (m_Popover != null)
            {
                m_Popover.Dismiss(DismissType.Consecutive);
                m_Popover.dismissed -= OnPopoverDismissed;
            }
            m_Popover = Popover.Build(this, pane);
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
            m_DatePicker.UnregisterValueChangedCallback(OnDatePickerValueChanged);
            m_TimePicker.UnregisterValueChangedCallback(OnTimePickerValueChanged);
            m_DatePicker.parent?.Remove(m_DatePicker);
            m_TimePicker.parent?.Remove(m_TimePicker);

            var picked = Clamp(((DateTime)m_DatePicker.value).Date + m_TimePicker.value);
            if (m_PreviousValue != picked)
            {
                using var evt = ChangeEvent<DateTime>.GetPooled(m_PreviousValue, picked);
                SetValueWithoutNotify(picked);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
            Focus();
        }

        void OnDatePickerValueChanged(ChangeEvent<Date> e)
        {
            NotifyChanging(((DateTime)e.newValue).Date + m_Value.TimeOfDay);
        }

        void OnTimePickerValueChanged(ChangeEvent<TimeSpan> e)
        {
            NotifyChanging(m_Value.Date + e.newValue);
        }

        void NotifyChanging(DateTime newValue)
        {
            newValue = Clamp(newValue);
            if (newValue == m_Value)
                return;

            SetValueWithoutNotify(newValue);
            using var evt = ChangingEvent<DateTime>.GetPooled();
            evt.previousValue = m_PreviousValue;
            evt.newValue = newValue;
            evt.target = this;
            SendEvent(evt);
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
    }
}
