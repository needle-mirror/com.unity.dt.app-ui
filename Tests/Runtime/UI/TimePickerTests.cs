using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TimePicker))]
    class TimePickerTests : VisualElementTests<TimePicker>
    {
        protected override string mainUssClassName => TimePicker.ussClassName;

        // Handed over to TimePickerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/TimePicker.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TimePicker
                {
                    value = new TimeSpan(9, 30, 0)
                });
                yield return new Story("12-hour with seconds", (ctx) => new TimePicker
                {
                    hour12 = true,
                    showSeconds = true,
                    value = new TimeSpan(14, 5, 30)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TimePicker />",
            @"<appui:TimePicker value=""09:30:00"" hour12=""true"" show-seconds=""true"" minute-step=""5"" />",
            @"<appui:TimePicker><appui:TimePickerColumn segment=""Minutes"" /></appui:TimePicker>",
        };

        IEnumerator Mount(TimePicker picker)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(picker);
            yield return null;
            yield return null;
        }

        static List<TimePickerColumn> Columns(TimePicker picker) =>
            picker.Query<TimePickerColumn>().ToList();

        static TimePickerColumn Column(TimePicker picker, TimeSegment segment) =>
            Columns(picker).FirstOrDefault(c => c.segment == segment);

        // ── Default column layout ───────────────────────────────────────────

        [UnityTest]
        public IEnumerator DefaultColumns_AreHoursAndMinutes()
        {
            var picker = new TimePicker { value = new TimeSpan(9, 30, 0) };
            yield return Mount(picker);

            var segments = Columns(picker).Select(c => c.segment).ToList();
            CollectionAssert.AreEqual(new[] { TimeSegment.Hours, TimeSegment.Minutes }, segments);
        }

        [UnityTest]
        public IEnumerator ShowSeconds_AddsASecondsColumn()
        {
            var picker = new TimePicker { showSeconds = true };
            yield return Mount(picker);

            CollectionAssert.AreEqual(
                new[] { TimeSegment.Hours, TimeSegment.Minutes, TimeSegment.Seconds },
                Columns(picker).Select(c => c.segment).ToList());
        }

        [UnityTest]
        public IEnumerator Hour12_AddsADayPeriodColumn()
        {
            var picker = new TimePicker { hour12 = true };
            yield return Mount(picker);

            CollectionAssert.AreEqual(
                new[] { TimeSegment.Hours, TimeSegment.Minutes, TimeSegment.DayPeriod },
                Columns(picker).Select(c => c.segment).ToList());
        }

        [UnityTest]
        public IEnumerator ChangingTheClock_RebuildsTheDefaultColumns()
        {
            var picker = new TimePicker();
            yield return Mount(picker);
            Assert.AreEqual(2, Columns(picker).Count);

            picker.showSeconds = true;
            yield return null;
            Assert.AreEqual(3, Columns(picker).Count,
                "Turning seconds on must rebuild the auto-managed column set.");

            picker.showSeconds = false;
            yield return null;
            Assert.AreEqual(2, Columns(picker).Count,
                "Turning seconds off must drop the seconds column again.");
        }

        [UnityTest]
        public IEnumerator AuthoredColumns_AreLeftAlone()
        {
            var picker = new TimePicker();
            picker.Add(new TimePickerColumn { segment = TimeSegment.Minutes });
            yield return Mount(picker);

            var columns = Columns(picker);
            Assert.AreEqual(1, columns.Count, "Authored columns must not be replaced by the defaults.");
            Assert.AreEqual(TimeSegment.Minutes, columns[0].segment);

            picker.showSeconds = true;
            yield return null;
            Assert.AreEqual(1, Columns(picker).Count,
                "The picker must not take over a column set it did not create.");
        }

        // ── Column display ──────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Columns_ShowTheValueOfTheirSegment()
        {
            var picker = new TimePicker { showSeconds = true, value = new TimeSpan(9, 5, 7) };
            yield return Mount(picker);

            Assert.AreEqual("09", Column(picker, TimeSegment.Hours).text);
            Assert.AreEqual("05", Column(picker, TimeSegment.Minutes).text);
            Assert.AreEqual("07", Column(picker, TimeSegment.Seconds).text);
        }

        [UnityTest]
        public IEnumerator Columns_FollowTheValue()
        {
            var picker = new TimePicker { value = new TimeSpan(9, 5, 0) };
            yield return Mount(picker);
            Assert.AreEqual("09", Column(picker, TimeSegment.Hours).text);

            picker.value = new TimeSpan(18, 45, 0);
            yield return null;
            Assert.AreEqual("18", Column(picker, TimeSegment.Hours).text);
            Assert.AreEqual("45", Column(picker, TimeSegment.Minutes).text);
        }

        [UnityTest]
        public IEnumerator Hour12_DisplaysA12HourClockAndADayPeriod()
        {
            var picker = new TimePicker { hour12 = true, value = new TimeSpan(0, 30, 0) };
            yield return Mount(picker);

            Assert.AreEqual("12", Column(picker, TimeSegment.Hours).text, "Midnight reads as 12 AM.");
            Assert.AreEqual("AM", Column(picker, TimeSegment.DayPeriod).text);

            picker.value = new TimeSpan(13, 30, 0);
            yield return null;
            Assert.AreEqual("01", Column(picker, TimeSegment.Hours).text);
            Assert.AreEqual("PM", Column(picker, TimeSegment.DayPeriod).text);
        }

        [UnityTest]
        public IEnumerator Column_CarriesItsSegmentUssClass()
        {
            var picker = new TimePicker { hour12 = true };
            yield return Mount(picker);

            Assert.IsTrue(Column(picker, TimeSegment.Hours)
                .ClassListContains(TimePickerColumn.segmentUssClassName + "hours"));
            Assert.IsTrue(Column(picker, TimeSegment.DayPeriod)
                .ClassListContains(TimePickerColumn.segmentUssClassName + "dayperiod"));
        }

        [Test]
        public void Column_SwapsItsSegmentClassInsteadOfAccumulating()
        {
            var column = new TimePickerColumn { segment = TimeSegment.Minutes };
            Assert.IsTrue(column.ClassListContains(TimePickerColumn.segmentUssClassName + "minutes"));
            Assert.IsFalse(column.ClassListContains(TimePickerColumn.segmentUssClassName + "hours"),
                "The previous segment class was left behind.");
        }

        [Test]
        public void Column_OutsideAPicker_RendersEmptyAndDoesNotThrow()
        {
            var column = new TimePickerColumn { segment = TimeSegment.Hours };
            Assert.IsNull(column.picker);
            Assert.DoesNotThrow(() => column.Refresh());
            Assert.AreEqual(string.Empty, column.text);
        }

        // ── Stepping ────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator ClickingTheIncrementButton_StepsTheSegment()
        {
            var picker = new TimePicker { value = new TimeSpan(9, 30, 0) };
            yield return Mount(picker);

            var button = Column(picker, TimeSegment.Minutes)
                .Q(className: TimePickerColumn.incrementUssClassName);
            Assert.IsNotNull(button, "A column must expose an increment button.");

            // Pointer events are dispatched by position, so the event has to actually land on the
            // button — setting evt.target alone is not enough.
            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = button.worldBound.center
                   }))
            {
                evt.target = button;
                button.SendEvent(evt);
            }
            yield return null;

            Assert.AreEqual(new TimeSpan(9, 31, 0), picker.value);
            Assert.AreEqual("31", Column(picker, TimeSegment.Minutes).text);
        }

        [Test]
        public void StepSegment_WrapsInsideTheSegment()
        {
            var picker = new TimePicker { value = new TimeSpan(9, 59, 0) };
            picker.StepSegment(TimeSegment.Minutes, 1);
            Assert.AreEqual(new TimeSpan(9, 0, 0), picker.value);
        }

        [Test]
        public void StepSegment_UsesMinuteStep()
        {
            var picker = new TimePicker { minuteStep = 10, value = new TimeSpan(9, 0, 0) };
            picker.StepSegment(TimeSegment.Minutes, 1);
            Assert.AreEqual(new TimeSpan(9, 10, 0), picker.value);
        }

        [UnityTest]
        public IEnumerator Value_WrapsIntoASingleDayAndNotifies()
        {
            var picker = new TimePicker();
            yield return Mount(picker);
            var count = 0;
            picker.RegisterValueChangedCallback(_ => count++);

            picker.value = new TimeSpan(25, 0, 0);
            Assert.AreEqual(new TimeSpan(1, 0, 0), picker.value);
            Assert.AreEqual(1, count);

            picker.value = new TimeSpan(1, 0, 0);
            Assert.AreEqual(1, count, "Setting the same value again must not re-notify.");
        }

        // ── USS source ──────────────────────────────────────────────────────

        static string RuleBody(string uss, string selector)
        {
            var rule = Regex.Match(uss, @"^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, $"TimePicker.uss has no {selector} rule.");
            return rule.Groups["body"].Value;
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void StepperButtons_AreSquareFromTheControlSizingAlias()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var body = RuleBody(uss, ".appui-time-picker-column__button");

            StringAssert.Contains("height: var(--appui-alias-control-sizing-sm-square)", body);
            StringAssert.Contains("width: var(--appui-alias-control-sizing-sm-square)", body);
            Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                "The stepper button uses a raw --appui-sizing step instead of the alias.");
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                "The stepper button uses a literal pixel value instead of the alias.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void StepperGlyphs_BindToTheControlSizingIconAlias()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var body = RuleBody(uss, ".appui-time-picker-column__button > .appui-icon");

            StringAssert.Contains("var(--appui-alias-control-sizing-icon-sm)", body);
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_DefinesTheSegmentClassNamesTheGeneratorActuallyEmits()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            // The class is built from segment.ToString().ToLower(), so DayPeriod emits `--dayperiod`.
            StringAssert.Contains(".appui-time-picker-column--dayperiod", uss,
                "The USS must use the lowercased enum member name the C# emits.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesNoLiteralColours()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "TimePicker.uss holds a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "TimePicker.uss holds a literal rgb() colour.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStyleSheet()
        {
            var appUi = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("@import \"./Components/TimePicker.uss\";", appUi,
                "TimePicker.uss is not imported by App UI.uss, so none of its rules ever load.");
        }
    }
}
