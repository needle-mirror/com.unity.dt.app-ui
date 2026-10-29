using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DatePicker))]
    class DatePickerTests : VisualElementTests<DatePicker>
    {
        protected override string mainUssClassName => DatePicker.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DatePicker
                {
                    value = new Date(2020, 1, 1)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:DatePicker />",
            @"<appui:DatePicker value=""1998-07-12"" />",
        };

        [TestCase(1969, 7, 20)]
        [TestCase(1900, 1, 1)]
        [TestCase(1970, 8, 20)]
        public void DatePicker_DayGrid_ContainsValue_BeforeAndAfterEpoch(int year, int month, int day)
        {
            var date = new Date(year, month, day);
            var datePicker = new DatePicker { value = date };
            Assert.AreEqual(year, datePicker.currentYear);
            Assert.AreEqual(month, datePicker.currentMonth);
            var found = datePicker.Query<VisualElement>().Where(e => e.userData is Date d && d == date).ToList();
            Assert.IsNotEmpty(found);
        }

        [TestCase(1, 1, 1)]
        [TestCase(9999, 12, 31)]
        public void DatePicker_DayGrid_DoesNotThrow_AtSupportedRangeLimits(int year, int month, int day)
        {
            var date = new Date(year, month, day);
            DatePicker datePicker = null;
            Assert.DoesNotThrow(() => datePicker = new DatePicker { value = date });
            Assert.AreEqual(year, datePicker.currentYear);
            Assert.AreEqual(month, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_Navigation_StaysInRange_AtFirstSupportedMonth()
        {
            var datePicker = new DatePicker { value = new Date(1, 1, 1) };
            Assert.DoesNotThrow(() => datePicker.GoToPreviousMonth());
            Assert.DoesNotThrow(() => datePicker.GoToPreviousYear());
            Assert.AreEqual(1, datePicker.currentYear);
            Assert.AreEqual(1, datePicker.currentMonth);

            var dayPicker = datePicker.Q<DayPicker>();
            Assert.IsFalse(dayPicker.Q<IconButton>(className: DayPicker.previousMonthButtonUssClassName).enabledSelf);
            Assert.IsFalse(dayPicker.Q<IconButton>(className: DayPicker.previousYearButtonUssClassName).enabledSelf);
            Assert.IsTrue(dayPicker.Q<IconButton>(className: DayPicker.nextMonthButtonUssClassName).enabledSelf);
            Assert.IsTrue(dayPicker.Q<IconButton>(className: DayPicker.nextYearButtonUssClassName).enabledSelf);

            var yearPicker = datePicker.Q<YearPicker>();
            Assert.IsFalse(yearPicker.Q<IconButton>(className: YearPicker.previousDecadeButtonUssClassName).enabledSelf);
            var yearZero = yearPicker.Query<VisualElement>().Where(e => e.userData is int y && y == 0).First();
            Assert.IsNotNull(yearZero);
            Assert.IsFalse(yearZero.enabledSelf);

            datePicker.GoToNextMonth();
            Assert.AreEqual(1, datePicker.currentYear);
            Assert.AreEqual(2, datePicker.currentMonth);
            Assert.IsTrue(dayPicker.Q<IconButton>(className: DayPicker.previousMonthButtonUssClassName).enabledSelf);
        }

        [Test]
        public void DatePicker_Navigation_StaysInRange_AtLastSupportedMonth()
        {
            var datePicker = new DatePicker { value = new Date(9999, 12, 31) };
            Assert.DoesNotThrow(() => datePicker.GoToNextMonth());
            Assert.DoesNotThrow(() => datePicker.GoToNextYear());
            Assert.AreEqual(9999, datePicker.currentYear);
            Assert.AreEqual(12, datePicker.currentMonth);

            var dayPicker = datePicker.Q<DayPicker>();
            Assert.IsFalse(dayPicker.Q<IconButton>(className: DayPicker.nextMonthButtonUssClassName).enabledSelf);
            Assert.IsFalse(dayPicker.Q<IconButton>(className: DayPicker.nextYearButtonUssClassName).enabledSelf);
            Assert.IsTrue(dayPicker.Q<IconButton>(className: DayPicker.previousMonthButtonUssClassName).enabledSelf);
            Assert.IsTrue(dayPicker.Q<IconButton>(className: DayPicker.previousYearButtonUssClassName).enabledSelf);

            var monthPicker = datePicker.Q<MonthPicker>();
            Assert.IsFalse(monthPicker.Q<IconButton>(className: MonthPicker.nextYearButtonUssClassName).enabledSelf);
            var yearPicker = datePicker.Q<YearPicker>();
            Assert.IsFalse(yearPicker.Q<IconButton>(className: YearPicker.nextDecadeButtonUssClassName).enabledSelf);

            datePicker.GoToPreviousMonth();
            Assert.AreEqual(9999, datePicker.currentYear);
            Assert.AreEqual(11, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_DecadeNavigation_ClampsToSupportedRange()
        {
            var datePicker = new DatePicker { value = new Date(9995, 6, 1) };
            var yearPicker = datePicker.Q<YearPicker>();
            Assert.DoesNotThrow(() => yearPicker.OnNextDecadeButtonClick());
            Assert.AreEqual(9999, datePicker.currentYear);
            Assert.AreEqual(6, datePicker.currentMonth);

            datePicker.GoTo(new Date(5, 6, 1));
            Assert.DoesNotThrow(() => yearPicker.OnPreviousDecadeButtonClick());
            Assert.AreEqual(1, datePicker.currentYear);
            Assert.AreEqual(6, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoTo()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 1));
            Assert.AreEqual(2020, datePicker.currentYear);
            Assert.AreEqual(1, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToPreviousYear()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 1));
            datePicker.GoToPreviousYear();
            Assert.AreEqual(2019, datePicker.currentYear);
            Assert.AreEqual(1, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToNextYear()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 1));
            datePicker.GoToNextYear();
            Assert.AreEqual(2021, datePicker.currentYear);
            Assert.AreEqual(1, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToYear()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 2, 29));
            datePicker.GoToYear(2019);
            Assert.AreEqual(2019, datePicker.currentYear);
            Assert.AreEqual(2, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToPreviousMonth()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 1));
            datePicker.GoToPreviousMonth();
            Assert.AreEqual(2019, datePicker.currentYear);
            Assert.AreEqual(12, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToNextMonth()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 1));
            datePicker.GoToNextMonth();
            Assert.AreEqual(2020, datePicker.currentYear);
            Assert.AreEqual(2, datePicker.currentMonth);
        }

        [Test]
        public void DatePicker_CanGoToMonth()
        {
            var datePicker = new DatePicker();
            datePicker.GoTo(new Date(2020, 1, 31));
            datePicker.GoToMonth(2);
            Assert.AreEqual(2020, datePicker.currentYear);
            Assert.AreEqual(2, datePicker.currentMonth);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => datePicker.GoToMonth(13));
        }

        [Test]
        public void DatePicker_CanSetValue()
        {
            var datePicker = new DatePicker();
            datePicker.SetValueWithoutNotify(new Date(2020, 1, 1));
            Assert.AreEqual(2020, datePicker.currentYear);
            Assert.AreEqual(1, datePicker.currentMonth);
        }

        [UnityTest]
        public IEnumerator DatePicker_CanSetValueWithEvent()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var datePicker = new DatePicker();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(datePicker);

            yield return null;

            var called = 0;
            datePicker.RegisterValueChangedCallback(evt =>
            {
                called++;
                Assert.AreEqual(new Date(2020, 1, 1), evt.newValue);
            });
            datePicker.value = new Date(2020, 1, 1);

            yield return null;

            Assert.AreEqual(1, called);
            Assert.AreEqual(new Date(2020, 1, 1), datePicker.value);

            datePicker.value = new Date(2020, 1, 1);

            yield return null;

            Assert.AreEqual(1, called);
        }
    }
}
