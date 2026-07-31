using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A base date picker control.
    /// </summary>
    [UxmlElement]
    public partial class BaseDatePicker : BaseVisualElement
    {

        internal static readonly BindingId displayModeProperty = new BindingId(nameof(displayMode));

        internal static readonly BindingId currentYearProperty = new BindingId(nameof(currentYear));

        internal static readonly BindingId currentMonthProperty = new BindingId(nameof(currentMonth));

        internal static readonly BindingId firstDayOfWeekProperty = new BindingId(nameof(firstDayOfWeek));


        /// <summary>
        /// Represents the display mode of the date picker.
        /// </summary>
        public enum DisplayMode
        {
            /// <summary>
            /// A list of years is displayed.
            /// </summary>
            Years,

            /// <summary>
            /// A list of months is displayed.
            /// </summary>
            Months,

            /// <summary>
            /// A list of days is displayed.
            /// </summary>
            Days
        }

        /// <summary>
        /// Main USS class name of elements of this type.
        /// </summary>
        public const string ussClassName = "appui-date-picker";

        /// <summary>
        /// The display mode USS class name.
        /// </summary>
        public static readonly string displayModeUssClassName = ussClassName + "--display-mode-";

        /// <summary>
        /// The years pane USS class name.
        /// </summary>
        public static readonly string yearPickerUssClassName = ussClassName + "__year-picker";

        /// <summary>
        /// The months pane USS class name.
        /// </summary>
        public static readonly string monthsPaneUssClassName = ussClassName + "__month-picker";

        /// <summary>
        /// The days pane USS class name.
        /// </summary>
        public static readonly string dayPickerUssClassName = ussClassName + "__day-picker";

        DisplayMode m_DisplayMode;

        Vector2Int m_DisplayedYearAndMonth;

        readonly YearPicker m_YearPicker;

        readonly MonthPicker m_MonthPicker;

        readonly DayPicker m_DayPicker;

        DayOfWeek m_FirstDayOfWeek;

        internal CultureInfo cultureInfo;

        /// <summary>
        /// Child elements are added to it, usually this is the same as the element itself.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseDatePicker()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            m_YearPicker = new YearPicker(this) {name = yearPickerUssClassName, pickingMode = PickingMode.Ignore};
            m_YearPicker.AddToClassList(yearPickerUssClassName);
            hierarchy.Add(m_YearPicker);
            m_MonthPicker = new MonthPicker(this) {name = monthsPaneUssClassName, pickingMode = PickingMode.Ignore};
            m_MonthPicker.AddToClassList(monthsPaneUssClassName);
            hierarchy.Add(m_MonthPicker);
            m_DayPicker = new DayPicker(this) {name = dayPickerUssClassName, pickingMode = PickingMode.Ignore};
            m_DayPicker.AddToClassList(dayPickerUssClassName);
            hierarchy.Add(m_DayPicker);

            displayMode = DisplayMode.Days;
            firstDayOfWeek = DayOfWeek.Sunday;

            this.RegisterContextChangedCallback<LangContext>(OnLangContextChanged);
        }

        void OnLangContextChanged(ContextChangedEvent<LangContext> evt)
        {
            var lang = evt.context?.lang;
            var cultureInfo = string.IsNullOrEmpty(lang) ? CultureInfo.InvariantCulture : new CultureInfo(lang);
            if (cultureInfo.Equals(this.cultureInfo))
                return;
            this.cultureInfo = cultureInfo;
            RefreshUI();
        }

        /// <summary>
        /// The current display mode of the date picker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public DisplayMode displayMode
        {
            get => m_DisplayMode;
            set
            {
                if (value == m_DisplayMode)
                    return;

                RemoveFromClassList(displayModeUssClassName + m_DisplayMode.ToString().ToLower());
                m_DisplayMode = value;
                AddToClassList(displayModeUssClassName + m_DisplayMode.ToString().ToLower());
                RefreshUI();

                NotifyPropertyChanged(in displayModeProperty);
            }
        }

        /// <summary>
        /// The first day of the week displayed in the date picker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public DayOfWeek firstDayOfWeek
        {
            get => m_FirstDayOfWeek;
            set
            {
                var changed = m_FirstDayOfWeek != value;
                m_FirstDayOfWeek = value;

                RefreshUI();

                if (changed)
                    NotifyPropertyChanged(in firstDayOfWeekProperty);
            }
        }

        /// <summary>
        /// The current year displayed in the date picker.
        /// </summary>
        public int currentYear => m_DisplayedYearAndMonth.x;

        /// <summary>
        /// The current month displayed in the date picker.
        /// </summary>
        public int currentMonth => m_DisplayedYearAndMonth.y;

        /// <summary>
        /// Show the specified year in the date picker.
        /// </summary>
        /// <param name="year"> The year to show. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToYear(int year)
        {
            GoTo(new DateTime(year, currentMonth, 1));
        }

        /// <summary>
        /// Show the next year in the date picker.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToNextYear()
        {
            var next = new DateTime(currentYear + 1, currentMonth, 1);
            GoTo(next);
        }

        /// <summary>
        /// Show the previous year in the date picker.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToPreviousYear()
        {
            var previous = new DateTime(currentYear - 1, currentMonth, 1);
            GoTo(previous);
        }

        /// <summary>
        /// Show the specified month in the date picker.
        /// </summary>
        /// <param name="month"> The month to show. </param>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if the month is not between 1 and 12. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToMonth(int month)
        {
            if (month is < 1 or > 12)
                throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");

            GoTo(new DateTime(currentYear, month, 1));
        }

        /// <summary>
        /// Show the next month in the date picker.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToNextMonth()
        {
            var next = new DateTime(currentYear, currentMonth, 1).AddMonths(1);
            GoTo(next);
        }

        /// <summary>
        /// Show the previous month in the date picker.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToPreviousMonth()
        {
            var previous = new DateTime(currentYear, currentMonth, 1).AddMonths(-1);
            GoTo(previous);
        }

        /// <summary>
        /// Show the specified date in the date picker.
        /// </summary>
        /// <param name="date"> The date to show. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoTo(Date date)
        {
            GoTo((DateTime)date);
        }

        /// <summary>
        /// Show the specified date in the date picker.
        /// </summary>
        /// <param name="date"> The date to show. </param>
        public void GoTo(DateTime date)
        {
            m_DisplayedYearAndMonth = new Vector2Int(date.Year, date.Month);
            RefreshUI();

            NotifyPropertyChanged(in currentYearProperty);
            NotifyPropertyChanged(in currentMonthProperty);
        }

        internal virtual void OnDaySelected(EventBase evt) { }

        internal virtual bool IsSelectedDate(Date date) => false;

        internal virtual bool IsStartDate(Date date) => false;

        internal virtual bool IsEndDate(Date date) => false;

        internal virtual bool IsInRange(Date date) => false;

        internal void OnMonthSelected(EventBase evt)
        {
            if (evt.target is not VisualElement { userData: int month })
                return;

            evt.StopPropagation();
            m_DisplayedYearAndMonth = new Vector2Int(currentYear, month);
            displayMode = DisplayMode.Days;
        }

        internal void OnYearSelected(EventBase evt)
        {
            if (evt.target is not VisualElement { userData: int year })
                return;

            evt.StopPropagation();
            m_DisplayedYearAndMonth = new Vector2Int(year, currentMonth);
            displayMode = DisplayMode.Months;
        }

        /// <summary>
        /// Refresh the overall UI of the date picker.
        /// </summary>
        protected void RefreshUI()
        {
            m_YearPicker.RefreshUI();
            m_MonthPicker.RefreshUI();
            m_DayPicker.RefreshUI();
        }

    }
}
