using System;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A component for selecting dates with an intuitive calendar interface.
    /// </summary>
    /// <remarks>
    /// The DatePicker component provides an intuitive way to select dates through a calendar interface. It supports
    /// various display modes including days, months, and years views, making it flexible for different date
    /// selection needs.
    ///
    /// The component follows common date picker patterns found in modern applications, with navigation controls for
    /// moving between months and years, and a clear visual hierarchy for date selection.
    ///
    /// Key features include:
    /// - Three display modes: days, months, and years views
    /// - Navigation between months and years
    /// - Configurable first day of the week
    /// - Localization support
    /// - Keyboard navigation support
    /// - Full UXML support
    ///
    /// NOTE: The DatePicker supports both programmatic and user interface-based date selection.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML. Create a simple date picker with default settings.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:DatePicker name="date-picker" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating a pre-configured date picker in UXML. Create a date picker with custom initial date, first day of
    /// week, and display mode.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:DatePicker
    ///         name="custom-date-picker"
    ///         value="2024-01-01"
    ///         first-day-of-week="Monday"
    ///         display-mode="Months" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating and configuring a date picker in C#. Create a date picker, set its initial value, configure it, and
    /// handle value changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// var datePicker = new DatePicker();
    /// datePicker.value = new Date(2024, 1, 1);
    /// datePicker.firstDayOfWeek = DayOfWeek.Monday;
    ///
    /// // Register for value changes
    /// datePicker.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Selected date: {evt.newValue}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class DatePicker : BaseDatePicker, INotifyValueChanged<Date>
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        Date m_Value;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DatePicker()
        {
            value = Date.now;
        }

        /// <summary>
        /// Set the value of the date picker without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(Date newValue)
        {
            m_Value = newValue;

            GoTo(newValue);
            displayMode = DisplayMode.Days;
        }

        /// <summary>
        /// The current value of the date picker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Date value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;

                using var evt = ChangeEvent<Date>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        internal override void OnDaySelected(EventBase evt)
        {
            if (evt.target is not VisualElement { userData: Date date })
                return;

            evt.StopPropagation();
            value = date;
        }

        internal override bool IsSelectedDate(Date date)
        {
            return date == value;
        }

    }
}
