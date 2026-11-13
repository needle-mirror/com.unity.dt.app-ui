using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The individual parts of a time-of-day value that can be edited on their own.
    /// </summary>
    /// <remarks>
    /// A time control displays its value as a sequence of segments (`HH:mm`, plus `:ss` when seconds
    /// are shown and an AM/PM segment on a 12-hour clock). Each segment steps independently and wraps
    /// inside its own unit: stepping minutes past 59 goes back to 00 without carrying into the hour.
    /// </remarks>
    public enum TimeSegment
    {
        /// <summary>
        /// The hours segment. Always steps over the full 0-23 range, even on a 12-hour clock.
        /// </summary>
        Hours,

        /// <summary>
        /// The minutes segment. Steps by <see cref="TimeField.minuteStep"/>.
        /// </summary>
        Minutes,

        /// <summary>
        /// The seconds segment. Only present when seconds are displayed.
        /// </summary>
        Seconds,

        /// <summary>
        /// The AM/PM segment. Only present on a 12-hour clock. Stepping it shifts the hours by 12.
        /// </summary>
        DayPeriod,
    }

    /// <summary>
    /// A form input control that allows users to enter a time of day through text input or a spinner picker.
    /// </summary>
    /// <remarks>
    /// The TimeField component combines a text input with a <see cref="TimePicker"/> popover. Users can type
    /// the time directly, step the segment under the caret with the Up and Down arrow keys, hop between
    /// segments with Left and Right, or click the clock icon to spin the value in a picker.
    ///
    /// The value is a <see cref="TimeSpan"/> holding a wall-clock time of day, always normalized into
    /// `[00:00:00, 24:00:00)`. It is deliberately not a <see cref="DateTime"/>: a TimeField carries no
    /// calendar date. Use <see cref="DateTimeField"/> when you need both.
    ///
    /// Key features:
    /// - Segmented text input with per-segment stepping and within-unit wrapping
    /// - Spinner picker interface
    /// - 12-hour and 24-hour clocks
    /// - Optional seconds segment and a configurable minute step
    /// - Size variants
    /// - Value validation
    ///
    /// NOTE: The display format is derived from <see cref="hour12"/> and <see cref="showSeconds"/> and is
    /// always parsed and formatted with the invariant culture, the same way <see cref="DateField"/> handles
    /// its own format string.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:TimeField name="alarm" />
    /// </UXML>
    /// ]]></code>
    /// <para>A 12-hour field with seconds and a five minute step:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:TimeField
    ///         name="meeting-time"
    ///         size="Lg"
    ///         hour12="true"
    ///         show-seconds="true"
    ///         minute-step="5"
    ///         value="09:30:00" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating and validating a TimeField in C#:</para>
    /// <code lang="csharp"><![CDATA[
    /// var timeField = new TimeField
    /// {
    ///     value = new TimeSpan(9, 0, 0),
    ///     minuteStep = 15,
    ///     size = Size.Md
    /// };
    ///
    /// // Office hours only.
    /// timeField.validateValue = time => time >= new TimeSpan(9, 0, 0) && time <= new TimeSpan(18, 0, 0);
    ///
    /// timeField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Selected time: {evt.newValue}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TimeField : ExVisualElement, IInputElement<TimeSpan>, INotifyValueChanging<TimeSpan>, ISizeableElement
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId hour12Property = nameof(hour12);

        internal static readonly BindingId showSecondsProperty = nameof(showSeconds);

        internal static readonly BindingId minuteStepProperty = nameof(minuteStep);

        /// <summary>
        /// The TimeField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-time-field";

        /// <summary>
        /// The TimeField input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TimeField picker button styling class.
        /// </summary>
        public static readonly string pickerButtonUssClassName = ussClassName + "__picker-button";

        /// <summary>
        /// The TimeField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly UnityEngine.UIElements.TextField m_InputElement;

        readonly VisualElement m_PickerButton;

        TimeSpan m_Value;

        Size m_Size;

        TimeSpan m_PreviousValue;

        TimePicker m_Picker;

        Func<TimeSpan, bool> m_ValidateValue;

        Popover m_Popover;

        bool m_Hour12;

        bool m_ShowSeconds;

        int m_MinuteStep = 1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimeField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;

            m_InputElement = new UnityEngine.UIElements.TextField
            {
                name = inputUssClassName,
                pickingMode = PickingMode.Position,
                isDelayed = true,
            };
            m_InputElement.AddToClassList(inputUssClassName);
            m_InputElement.RegisterValueChangedCallback(OnInputValueChanged);
            m_InputElement.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);

            m_PickerButton = new VisualElement
            {
                focusable = true,
                pickingMode = PickingMode.Position,
                name = pickerButtonUssClassName,
            };
            m_PickerButton.AddToClassList(pickerButtonUssClassName);
            m_PickerButton.AddManipulator(new Pressable(OnClick));
            var icon = new Icon { primary = true, iconName = "clock", pickingMode = PickingMode.Ignore };
            m_PickerButton.Add(icon);

            hierarchy.Add(m_InputElement);
            hierarchy.Add(m_PickerButton);

            size = Size.Md;
            SetValueWithoutNotify(TimeSpan.Zero);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// The content container of this TimeField. This is null for TimeField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The TimeField size.
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
        /// The TimeField invalid state.
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
        /// The TimeField validation function.
        /// </summary>
        [CreateProperty]
        public Func<TimeSpan, bool> validateValue
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
        /// Whether the field uses a 12-hour clock with an AM/PM segment. Defaults to false, a 24-hour clock.
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
                SetFormattedString();
                if (m_Picker != null)
                    m_Picker.hour12 = value;

                if (changed)
                    NotifyPropertyChanged(in hour12Property);
            }
        }

        /// <summary>
        /// Whether the field displays a seconds segment. Defaults to false.
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
                SetFormattedString();
                if (m_Picker != null)
                    m_Picker.showSeconds = value;

                if (changed)
                    NotifyPropertyChanged(in showSecondsProperty);
            }
        }

        /// <summary>
        /// The number of minutes stepped per arrow press or spinner click. Defaults to 1.
        /// </summary>
        /// <remarks>
        /// Values below 1 are clamped to 1.
        /// </remarks>
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
                if (m_Picker != null)
                    m_Picker.minuteStep = newValue;

                if (changed)
                    NotifyPropertyChanged(in minuteStepProperty);
            }
        }

        /// <summary>
        /// The format string used to display and parse the value, derived from
        /// <see cref="hour12"/> and <see cref="showSeconds"/>.
        /// </summary>
        public string formatString => GetFormatString(m_Hour12, m_ShowSeconds);

        /// <summary>
        /// The TimeField value, a time of day in `[00:00:00, 24:00:00)`.
        /// </summary>
        /// <remarks>
        /// Values outside that range are wrapped into it, so `25:00` becomes `01:00` and a negative
        /// span counts back from midnight.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public TimeSpan value
        {
            get => m_Value;
            set
            {
                var newValue = Normalize(value);
                if (m_Value == newValue)
                    return;

                using var evt = ChangeEvent<TimeSpan>.GetPooled(m_Value, newValue);
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Sets the TimeField value without notifying the TimeField.
        /// </summary>
        /// <param name="newValue"> The new TimeField value. It is normalized into `[00:00:00, 24:00:00)`. </param>
        public void SetValueWithoutNotify(TimeSpan newValue)
        {
            m_Value = Normalize(newValue);
            SetFormattedString();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// Steps a single segment of the value, wrapping inside that segment's own unit.
        /// </summary>
        /// <param name="segment"> The segment to step. </param>
        /// <param name="delta"> The number of steps, negative to step down. </param>
        public void StepSegment(TimeSegment segment, int delta)
        {
            value = Step(m_Value, segment, delta, m_MinuteStep);
        }

        void SetFormattedString()
        {
            m_InputElement.SetValueWithoutNotify(Format(m_Value, m_Hour12, m_ShowSeconds));
        }

        void OnInputValueChanged(ChangeEvent<string> e)
        {
            value = TryParse(e.newValue, out var parsed) ? parsed : m_Value;
            // The input may hold a partially typed string even when it parsed to the current value,
            // so always push the canonical formatting back.
            SetFormattedString();
        }

        void OnInputKeyDown(KeyDownEvent evt)
        {
            var spans = GetSegmentSpans(m_Hour12, m_ShowSeconds);

            if (evt.keyCode is KeyCode.UpArrow or KeyCode.DownArrow)
            {
                evt.StopPropagation();
                var segment = SegmentAtCaret(m_InputElement.textSelection.cursorIndex, m_Hour12, m_ShowSeconds);
                StepSegment(segment, evt.keyCode == KeyCode.UpArrow ? 1 : -1);
                SelectSegment(segment);
                return;
            }

            if (evt.keyCode is KeyCode.LeftArrow or KeyCode.RightArrow)
            {
                var current = SegmentAtCaret(m_InputElement.textSelection.cursorIndex, m_Hour12, m_ShowSeconds);
                var index = spans.FindIndex(s => s.segment == current);
                var nextIndex = evt.keyCode == KeyCode.RightArrow
                    ? Mathf.Min(index + 1, spans.Count - 1)
                    : Mathf.Max(index - 1, 0);
                evt.StopPropagation();
                SelectSegment(spans[nextIndex].segment);
            }
        }

        void SelectSegment(TimeSegment segment)
        {
            foreach (var span in GetSegmentSpans(m_Hour12, m_ShowSeconds))
            {
                if (span.segment != segment)
                    continue;
                m_InputElement.textSelection.SelectRange(span.start, span.start + span.length);
                return;
            }
        }

        void OnClick()
        {
            m_Picker?.parent?.Remove(m_Picker);

            m_PreviousValue = value;
            m_Picker ??= new TimePicker();
            m_Picker.hour12 = m_Hour12;
            m_Picker.showSeconds = m_ShowSeconds;
            m_Picker.minuteStep = m_MinuteStep;
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
                using var evt = ChangeEvent<TimeSpan>.GetPooled(m_PreviousValue, m_Picker.value);
                SetValueWithoutNotify(m_Picker.value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
            Focus();
        }

        void OnPickerValueChanged(ChangeEvent<TimeSpan> e)
        {
            if (e.newValue != value)
            {
                SetValueWithoutNotify(e.newValue);
                using var evt = ChangingEvent<TimeSpan>.GetPooled();
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
        /// Tries to parse a string into a time of day, using the field's current format first and then
        /// a set of common fallbacks.
        /// </summary>
        /// <param name="str"> The string to parse. </param>
        /// <param name="parsed"> The parsed time of day, normalized into `[00:00:00, 24:00:00)`. </param>
        /// <returns> True if the string could be parsed, false otherwise. </returns>
        public bool TryParse(string str, out TimeSpan parsed)
        {
            return TryParseTime(str, m_Hour12, m_ShowSeconds, out parsed);
        }

        // ---------------------------------------------------------------------------------------------
        // Shared time-of-day helpers. TimePicker and DateTimeField drive the exact same segment model,
        // so the formatting, parsing and stepping rules live here once.
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// A segment of the formatted string, and where it sits in it.
        /// </summary>
        internal readonly struct SegmentSpan
        {
            public readonly TimeSegment segment;

            public readonly int start;

            public readonly int length;

            public SegmentSpan(TimeSegment segment, int start, int length)
            {
                this.segment = segment;
                this.start = start;
                this.length = length;
            }
        }

        /// <summary>
        /// The number of ticks in a full day, used to wrap a time of day.
        /// </summary>
        internal const long ticksPerDay = TimeSpan.TicksPerDay;

        /// <summary>
        /// Wraps a time span into `[00:00:00, 24:00:00)`.
        /// </summary>
        internal static TimeSpan Normalize(TimeSpan time)
        {
            var ticks = time.Ticks % ticksPerDay;
            if (ticks < 0)
                ticks += ticksPerDay;
            return TimeSpan.FromTicks(ticks);
        }

        /// <summary>
        /// The invariant format string for a given clock configuration.
        /// </summary>
        internal static string GetFormatString(bool hour12, bool showSeconds)
        {
            if (hour12)
                return showSeconds ? "hh:mm:ss tt" : "hh:mm tt";
            return showSeconds ? "HH:mm:ss" : "HH:mm";
        }

        /// <summary>
        /// Formats a time of day with the invariant culture.
        /// </summary>
        internal static string Format(TimeSpan time, bool hour12, bool showSeconds)
        {
            return (DateTime.MinValue + Normalize(time))
                .ToString(GetFormatString(hour12, showSeconds), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a time of day with the invariant culture, trying the exact format first.
        /// </summary>
        internal static bool TryParseTime(string str, bool hour12, bool showSeconds, out TimeSpan parsed)
        {
            parsed = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            if (DateTime.TryParseExact(str, GetFormatString(hour12, showSeconds),
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            {
                parsed = Normalize(exact.TimeOfDay);
                return true;
            }

            // Accept the other shapes too, so switching hour12/showSeconds does not reject what the
            // user already typed.
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var loose))
            {
                parsed = Normalize(loose.TimeOfDay);
                return true;
            }

            if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out var span))
            {
                parsed = Normalize(span);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Steps a single segment of a time of day, wrapping inside that segment's own unit.
        /// </summary>
        /// <remarks>
        /// Wrapping never carries: minutes 59 stepped up land on 00 and leave the hour alone. That is
        /// the spinner mental model the registry uses.
        /// </remarks>
        internal static TimeSpan Step(TimeSpan time, TimeSegment segment, int delta, int minuteStep)
        {
            time = Normalize(time);
            var hours = time.Hours;
            var minutes = time.Minutes;
            var seconds = time.Seconds;

            switch (segment)
            {
                case TimeSegment.Hours:
                    hours = Wrap(hours + delta, 24);
                    break;
                case TimeSegment.Minutes:
                    minutes = Wrap(minutes + delta * Mathf.Max(1, minuteStep), 60);
                    break;
                case TimeSegment.Seconds:
                    seconds = Wrap(seconds + delta, 60);
                    break;
                case TimeSegment.DayPeriod:
                    hours = Wrap(hours + 12, 24);
                    break;
            }

            return new TimeSpan(hours, minutes, seconds);
        }

        static int Wrap(int value, int modulo)
        {
            var wrapped = value % modulo;
            return wrapped < 0 ? wrapped + modulo : wrapped;
        }

        /// <summary>
        /// The character ranges of each segment inside the formatted string.
        /// </summary>
        internal static List<SegmentSpan> GetSegmentSpans(bool hour12, bool showSeconds)
        {
            var spans = new List<SegmentSpan>();
            var index = 0;
            spans.Add(new SegmentSpan(TimeSegment.Hours, index, 2));
            index += 3; // "HH" + ":"
            spans.Add(new SegmentSpan(TimeSegment.Minutes, index, 2));
            index += 2;
            if (showSeconds)
            {
                index += 1; // ":"
                spans.Add(new SegmentSpan(TimeSegment.Seconds, index, 2));
                index += 2;
            }
            if (hour12)
            {
                index += 1; // " "
                spans.Add(new SegmentSpan(TimeSegment.DayPeriod, index, 2));
            }
            return spans;
        }

        /// <summary>
        /// The segment the caret sits in. Falls back to the hours segment.
        /// </summary>
        internal static TimeSegment SegmentAtCaret(int caret, bool hour12, bool showSeconds)
        {
            foreach (var span in GetSegmentSpans(hour12, showSeconds))
            {
                if (caret >= span.start && caret <= span.start + span.length)
                    return span.segment;
            }
            return TimeSegment.Hours;
        }

        /// <summary>
        /// The text a spinner column shows for a segment.
        /// </summary>
        internal static string FormatSegment(TimeSpan time, TimeSegment segment, bool hour12)
        {
            time = Normalize(time);
            switch (segment)
            {
                case TimeSegment.Hours:
                    var hours = hour12 ? (time.Hours % 12 == 0 ? 12 : time.Hours % 12) : time.Hours;
                    return hours.ToString("D2", CultureInfo.InvariantCulture);
                case TimeSegment.Minutes:
                    return time.Minutes.ToString("D2", CultureInfo.InvariantCulture);
                case TimeSegment.Seconds:
                    return time.Seconds.ToString("D2", CultureInfo.InvariantCulture);
                case TimeSegment.DayPeriod:
                    return time.Hours < 12 ? "AM" : "PM";
                default:
                    return string.Empty;
            }
        }
    }
}
