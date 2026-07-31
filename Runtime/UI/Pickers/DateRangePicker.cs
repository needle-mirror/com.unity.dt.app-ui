using System;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A customizable component for selecting a range of dates.
    /// </summary>
    /// <remarks>
    /// The DateRangePicker component allows users to select a range of dates through an intuitive calendar
    /// interface. It's particularly useful when users need to specify date ranges for filtering, scheduling, or
    /// booking scenarios.
    ///
    /// The component provides a calendar view with the following features:
    /// - Visual selection of start and end dates
    /// - Navigation between months and years
    /// - Support for different display modes (days, months, years)
    /// - Localization support for month names and weekdays
    /// - Customizable first day of the week
    ///
    /// The date range selection is done in two steps: first click selects the start date, second click selects
    /// the end date. The dates in between are automatically highlighted to show the selected range.
    /// </remarks>
    /// <example>
    /// <para>Basic usage example showing how to create a DateRangePicker and handle value changes:</para>
    /// <code lang="csharp"><![CDATA[
    /// var dateRangePicker = new DateRangePicker();
    ///
    /// // Register to value change events
    /// dateRangePicker.RegisterValueChangedCallback(evt => {
    ///     var startDate = evt.newValue.start;
    ///     var endDate = evt.newValue.end;
    ///     Debug.Log($"Selected date range: {startDate} to {endDate}");
    /// });
    ///
    /// // Add to the visual tree
    /// rootElement.Add(dateRangePicker);
    /// ]]></code>
    /// <para>UXML definition with custom configuration:</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements">
    ///     <ui:DateRangePicker
    ///         name="booking-dates"
    ///         value="2024-01-01,2024-01-15"
    ///         first-day-of-week="Monday"
    ///         display-mode="Days"
    ///     />
    /// </ui:UXML>
    /// ]]></code>
    /// <para>Integration with a booking system example:</para>
    /// <code lang="csharp"><![CDATA[
    /// public class BookingWidget : VisualElement
    /// {
    ///     DateRangePicker m_DateRangePicker;
    ///     Button m_ConfirmButton;
    ///
    ///     public BookingWidget()
    ///     {
    ///         // Create and configure the date range picker
    ///         m_DateRangePicker = new DateRangePicker();
    ///         m_DateRangePicker.value = new DateRange(
    ///             DateTime.Now,
    ///             DateTime.Now.AddDays(1)
    ///         );
    ///
    ///         // Create confirm button
    ///         m_ConfirmButton = new Button(OnConfirmBooking);
    ///         m_ConfirmButton.text = "Confirm Booking";
    ///
    ///         // Add to the visual hierarchy
    ///         Add(m_DateRangePicker);
    ///         Add(m_ConfirmButton);
    ///     }
    ///
    ///     void OnConfirmBooking()
    ///     {
    ///         var booking = new Booking
    ///         {
    ///             CheckIn = m_DateRangePicker.value.start,
    ///             CheckOut = m_DateRangePicker.value.end
    ///         };
    ///         BookingSystem.Reserve(booking);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class DateRangePicker : BaseDatePicker, INotifyValueChanged<DateRange>
    {

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        DateRange m_Value;

        Date m_TempStartDate;

        bool m_IsTemporarilySelected;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DateRangePicker()
        {
            value = new DateRange(DateTime.Now, DateTime.Now.AddDays(1));
        }

        /// <summary>
        /// Set the value of the date picker without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(DateRange newValue)
        {
            m_IsTemporarilySelected = false;
            m_Value = newValue;
            GoTo(newValue.end);
            displayMode = DisplayMode.Days;
        }

        /// <summary>
        /// The current value of the date picker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public DateRange value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;

                using var evt = ChangeEvent<DateRange>.GetPooled(m_Value, value);
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
            if (!m_IsTemporarilySelected)
            {
                m_TempStartDate = date;
                m_IsTemporarilySelected = true;
                RefreshUI();
            }
            else
            {
                m_IsTemporarilySelected = false;
                var startDate = m_TempStartDate;
                var endDate = date;
                if (startDate > endDate)
                {
                    startDate = date;
                    endDate = m_TempStartDate;
                }
                value = new DateRange(startDate, endDate);
            }
        }

        internal override bool IsStartDate(Date date)
        {
            return m_IsTemporarilySelected ? date == m_TempStartDate : date == m_Value.start;
        }

        internal override bool IsEndDate(Date date)
        {
            return !m_IsTemporarilySelected && date == m_Value.end;
        }

        internal override bool IsInRange(Date date)
        {
            return !m_IsTemporarilySelected && m_Value.Contains(date, includeStartAndEnd: false);
        }

    }
}
