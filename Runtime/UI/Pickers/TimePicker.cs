using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A component for picking a time of day with spinner columns.
    /// </summary>
    /// <remarks>
    /// The TimePicker holds the shared time value and lays out one <see cref="TimePickerColumn"/> per
    /// segment. By default it builds hours and minutes columns, adding a seconds column when
    /// <see cref="showSeconds"/> is set and an AM/PM column when <see cref="hour12"/> is set.
    ///
    /// The default columns are only built when the picker has none of its own, so a UXML author can
    /// reorder or subset them by declaring <see cref="TimePickerColumn"/> children explicitly. Once you
    /// do, the picker stops managing the column set and leaves your layout alone.
    ///
    /// The value is a <see cref="TimeSpan"/> time of day, normalized into `[00:00:00, 24:00:00)`, the same
    /// model <see cref="TimeField"/> uses. Each column steps its own segment and wraps inside that
    /// segment's unit, so minutes rolling over from 59 to 00 never carry into the hour.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:TimePicker value="14:30:00" />
    /// </UXML>
    /// ]]></code>
    /// <para>A custom column layout, minutes first and no hours:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:TimePicker minute-step="5">
    ///         <appui:TimePickerColumn segment="Minutes" />
    ///         <appui:TimePickerColumn segment="Seconds" />
    ///     </appui:TimePicker>
    /// </UXML>
    /// ]]></code>
    /// <para>Creating a 12-hour picker in C#:</para>
    /// <code lang="csharp"><![CDATA[
    /// var picker = new TimePicker
    /// {
    ///     hour12 = true,
    ///     showSeconds = true,
    ///     value = new TimeSpan(14, 30, 0)
    /// };
    ///
    /// picker.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Picked time: {evt.newValue}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TimePicker : BaseVisualElement, INotifyValueChanged<TimeSpan>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId hour12Property = nameof(hour12);

        internal static readonly BindingId showSecondsProperty = nameof(showSeconds);

        internal static readonly BindingId minuteStepProperty = nameof(minuteStep);

        /// <summary>
        /// The TimePicker main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-time-picker";

        /// <summary>
        /// The TimePicker segment separator styling class.
        /// </summary>
        public static readonly string separatorUssClassName = ussClassName + "__separator";

        TimeSpan m_Value;

        bool m_Hour12;

        bool m_ShowSeconds;

        int m_MinuteStep = 1;

        bool m_AutoColumns;

        bool m_ColumnsInitialized;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimePicker()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (m_ColumnsInitialized)
                return;

            m_ColumnsInitialized = true;
            // Only take ownership of the layout when the author declared no columns of their own.
            m_AutoColumns = this.Q<TimePickerColumn>() == null;
            if (m_AutoColumns)
                RebuildColumns();
            else
                RefreshColumns();
        }

        void RebuildColumns()
        {
            Clear();

            Add(new TimePickerColumn { segment = TimeSegment.Hours });
            AddSeparator();
            Add(new TimePickerColumn { segment = TimeSegment.Minutes });
            if (m_ShowSeconds)
            {
                AddSeparator();
                Add(new TimePickerColumn { segment = TimeSegment.Seconds });
            }
            if (m_Hour12)
                Add(new TimePickerColumn { segment = TimeSegment.DayPeriod });

            RefreshColumns();
        }

        void AddSeparator()
        {
            var separator = new Text(":") { pickingMode = PickingMode.Ignore };
            separator.AddToClassList(separatorUssClassName);
            Add(separator);
        }

        internal void RefreshColumns()
        {
            this.Query<TimePickerColumn>().ForEach(column => column.Refresh());
        }

        void OnClockChanged()
        {
            if (m_AutoColumns && m_ColumnsInitialized)
                RebuildColumns();
            else
                RefreshColumns();
        }

        /// <summary>
        /// Whether the picker uses a 12-hour clock, adding an AM/PM column. Defaults to false.
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
                if (changed)
                {
                    OnClockChanged();
                    NotifyPropertyChanged(in hour12Property);
                }
            }
        }

        /// <summary>
        /// Whether the picker shows a seconds column. Defaults to false.
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
                if (changed)
                {
                    OnClockChanged();
                    NotifyPropertyChanged(in showSecondsProperty);
                }
            }
        }

        /// <summary>
        /// The number of minutes stepped per spinner click. Defaults to 1. Values below 1 are clamped to 1.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("minute-step")]
        public int minuteStep
        {
            get => m_MinuteStep;
            set
            {
                var newValue = value < 1 ? 1 : value;
                var changed = m_MinuteStep != newValue;
                m_MinuteStep = newValue;

                if (changed)
                    NotifyPropertyChanged(in minuteStepProperty);
            }
        }

        /// <summary>
        /// The current value of the picker, a time of day in `[00:00:00, 24:00:00)`.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TimeSpan value
        {
            get => m_Value;
            set
            {
                var newValue = TimeField.Normalize(value);
                if (newValue == m_Value)
                    return;

                using var evt = ChangeEvent<TimeSpan>.GetPooled(m_Value, newValue);
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Sets the value of the picker without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new value. It is normalized into `[00:00:00, 24:00:00)`. </param>
        public void SetValueWithoutNotify(TimeSpan newValue)
        {
            m_Value = TimeField.Normalize(newValue);
            RefreshColumns();
        }

        /// <summary>
        /// Steps a single segment of the value, wrapping inside that segment's own unit.
        /// </summary>
        /// <param name="segment"> The segment to step. </param>
        /// <param name="delta"> The number of steps, negative to step down. </param>
        public void StepSegment(TimeSegment segment, int delta)
        {
            value = TimeField.Step(m_Value, segment, delta, m_MinuteStep);
        }
    }
}
