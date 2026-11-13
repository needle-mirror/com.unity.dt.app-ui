using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DateTimeField))]
    class DateTimeFieldTests : VisualElementTests<DateTimeField>
    {
        protected override string mainUssClassName => DateTimeField.ussClassName;

        // Handed over to DateTimeFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/DateTimeField.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DateTimeField
                {
                    value = new DateTime(2020, 1, 1, 9, 30, 0)
                });
                yield return new Story("12-hour with seconds", (ctx) => new DateTimeField
                {
                    hour12 = true,
                    showSeconds = true,
                    value = new DateTime(2020, 1, 1, 14, 5, 30)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:DateTimeField />",
            @"<appui:DateTimeField value=""1998-07-12 09:30:00"" size=""Lg"" date-format=""MM/dd/yyyy"" hour12=""true"" minute-step=""15"" />",
        };

        // Change and key events are only dispatched once the element belongs to a panel, so any test
        // that goes through the event system has to mount first.
        IEnumerator Mount(VisualElement element)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);
            yield return null;
            yield return null;
        }

        static UnityEngine.UIElements.TextField DateInput(DateTimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: DateTimeField.dateInputUssClassName);

        static UnityEngine.UIElements.TextField TimeInput(DateTimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: DateTimeField.timeInputUssClassName);

        static void Key(VisualElement target, KeyCode code)
        {
            using var evt = KeyDownEvent.GetPooled('\0', code, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }

        // ── Size ────────────────────────────────────────────────────────────

        [Test]
        public void DefaultSize_IsMd()
        {
            var field = new DateTimeField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(DateTimeField.sizeUssClassName + "md"));
        }

        [Test]
        public void Size_SwapsTheSizeClassInsteadOfAccumulating()
        {
            var field = new DateTimeField { size = Size.Sm };
            Assert.IsTrue(field.ClassListContains(DateTimeField.sizeUssClassName + "sm"));
            Assert.IsFalse(field.ClassListContains(DateTimeField.sizeUssClassName + "md"),
                "The previous size class was left behind.");
        }

        // ── Structure ───────────────────────────────────────────────────────

        [Test]
        public void Structure_HasADateInputATimeInputAndOnePickerButton()
        {
            var field = new DateTimeField();
            Assert.IsNotNull(DateInput(field), "DateTimeField must expose a date input.");
            Assert.IsNotNull(TimeInput(field), "DateTimeField must expose a time input.");
            Assert.IsNotNull(field.Q(className: DateTimeField.separatorUssClassName));
            Assert.AreEqual(1, field.Query(className: DateTimeField.pickerButtonUssClassName).ToList().Count,
                "The two halves share a single picker button.");
        }

        // ── Composition ─────────────────────────────────────────────────────

        [Test]
        public void BothHalvesMirrorTheSingleValue()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            Assert.AreEqual("2024-03-07", DateInput(field).value);
            Assert.AreEqual("14:05", TimeInput(field).value);
            Assert.AreEqual(new Date(2024, 3, 7), field.date);
            Assert.AreEqual(new TimeSpan(14, 5, 0), field.time);
        }

        [UnityTest]
        public IEnumerator EditingTheDate_PreservesTheTimeOfDay()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            DateInput(field).value = "2024-12-25";
            Assert.AreEqual(new DateTime(2024, 12, 25, 14, 5, 0), field.value);
        }

        [UnityTest]
        public IEnumerator EditingTheTime_PreservesTheCalendarDate()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            TimeInput(field).value = "08:45";
            Assert.AreEqual(new DateTime(2024, 3, 7, 8, 45, 0), field.value);
        }

        [UnityTest]
        public IEnumerator UnparseableDate_RevertsToThePreviousValue()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            DateInput(field).value = "not a date";
            Assert.AreEqual(new DateTime(2024, 3, 7, 14, 5, 0), field.value);
            Assert.AreEqual("2024-03-07", DateInput(field).value);
        }

        [UnityTest]
        public IEnumerator UnparseableTime_RevertsToThePreviousValue()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            TimeInput(field).value = "banana";
            Assert.AreEqual(new DateTime(2024, 3, 7, 14, 5, 0), field.value);
            Assert.AreEqual("14:05", TimeInput(field).value);
        }

        [Test]
        public void DateFormat_DrivesTheDateDisplay()
        {
            var field = new DateTimeField
            {
                dateFormat = "MM/dd/yyyy",
                value = new DateTime(2024, 3, 7, 14, 5, 0)
            };
            Assert.AreEqual("03/07/2024", DateInput(field).value);
        }

        [Test]
        public void Hour12AndShowSeconds_DriveTheTimeDisplay()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 9) };
            Assert.AreEqual("14:05", TimeInput(field).value);

            field.showSeconds = true;
            Assert.AreEqual("14:05:09", TimeInput(field).value);

            field.hour12 = true;
            Assert.AreEqual("02:05:09 PM", TimeInput(field).value);
        }

        // ── Keyboard ────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator ArrowKeysOnTheDate_StepWholeDays()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            Key(DateInput(field), KeyCode.UpArrow);
            Assert.AreEqual(new DateTime(2024, 3, 8, 14, 5, 0), field.value);

            Key(DateInput(field), KeyCode.DownArrow);
            Assert.AreEqual(new DateTime(2024, 3, 7, 14, 5, 0), field.value);
        }

        [UnityTest]
        public IEnumerator ArrowKeysOnTheDate_SaturateAtTheRepresentableBoundaries()
        {
            var high = new DateTimeField { value = DateTime.MaxValue };
            yield return Mount(high);
            Assert.DoesNotThrow(() => Key(DateInput(high), KeyCode.UpArrow),
                "AddDays overflows at DateTime.MaxValue, which is itself a legal value here.");
            Assert.AreEqual(DateTime.MaxValue, high.value);

            var low = new DateTimeField { value = DateTime.MinValue };
            yield return Mount(low);
            Assert.DoesNotThrow(() => Key(DateInput(low), KeyCode.DownArrow));
            Assert.AreEqual(DateTime.MinValue, low.value);
        }

        [UnityTest]
        public IEnumerator ArrowKeysOnTheTime_StepTheSegmentUnderTheCaret()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 14, 5, 0) };
            yield return Mount(field);
            Key(TimeInput(field), KeyCode.UpArrow);
            Assert.AreEqual(new DateTime(2024, 3, 7, 15, 5, 0), field.value,
                "The caret sits in the hours segment by default.");
        }

        [Test]
        public void StepSegment_WrapsWithoutMovingTheDate()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 23, 30, 0) };
            field.StepSegment(TimeSegment.Hours, 1);
            Assert.AreEqual(new DateTime(2024, 3, 7, 0, 30, 0), field.value,
                "Wrapping the hours must not roll the calendar date over.");
        }

        [Test]
        public void StepSegment_Minutes_UsesMinuteStep()
        {
            var field = new DateTimeField { minuteStep = 15, value = new DateTime(2024, 3, 7, 9, 0, 0) };
            field.StepSegment(TimeSegment.Minutes, 1);
            Assert.AreEqual(new DateTime(2024, 3, 7, 9, 15, 0), field.value);
        }

        // ── Bounds and validation ───────────────────────────────────────────

        [Test]
        public void MinAndMax_ClampTheValue()
        {
            var field = new DateTimeField
            {
                min = new DateTime(2024, 3, 7, 8, 0, 0),
                max = new DateTime(2024, 3, 7, 18, 0, 0),
                value = new DateTime(2024, 3, 7, 12, 0, 0)
            };

            field.value = new DateTime(2024, 3, 7, 6, 0, 0);
            Assert.AreEqual(new DateTime(2024, 3, 7, 8, 0, 0), field.value);

            field.value = new DateTime(2024, 3, 7, 23, 0, 0);
            Assert.AreEqual(new DateTime(2024, 3, 7, 18, 0, 0), field.value);
        }

        [Test]
        public void SettingABound_ClampsTheCurrentValueImmediately()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 6, 0, 0) };
            field.min = new DateTime(2024, 3, 7, 8, 0, 0);
            Assert.AreEqual(new DateTime(2024, 3, 7, 8, 0, 0), field.value);
        }

        [Test]
        public void Unbounded_ByDefault()
        {
            var field = new DateTimeField();
            Assert.AreEqual(DateTime.MinValue, field.min);
            Assert.AreEqual(DateTime.MaxValue, field.max);

            var far = new DateTime(1900, 1, 1, 0, 0, 0);
            field.value = far;
            Assert.AreEqual(far, field.value);
        }

        [Test]
        public void Invalid_TracksValidateValue()
        {
            var field = new DateTimeField
            {
                validateValue = dt => dt.Hour >= 9
            };

            field.value = new DateTime(2024, 3, 7, 8, 0, 0);
            Assert.IsTrue(field.invalid);

            field.value = new DateTime(2024, 3, 7, 10, 0, 0);
            Assert.IsFalse(field.invalid);
        }

        [UnityTest]
        public IEnumerator Value_SendsAChangeEventOnceAndOnlyOnChange()
        {
            var field = new DateTimeField { value = new DateTime(2024, 3, 7, 9, 0, 0) };
            yield return Mount(field);
            var count = 0;
            field.RegisterValueChangedCallback(_ => count++);

            field.value = new DateTime(2024, 3, 7, 10, 0, 0);
            Assert.AreEqual(1, count);

            field.value = new DateTime(2024, 3, 7, 10, 0, 0);
            Assert.AreEqual(1, count);
        }

        // ── USS source ──────────────────────────────────────────────────────

        static string RuleBody(string uss, string selector)
        {
            var rule = Regex.Match(uss, @"^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, $"DateTimeField.uss has no {selector} rule.");
            return rule.Groups["body"].Value;
        }

        /// <summary>
        /// The registry's <c>datetime-field</c> wraps <c>textFieldVariants</c>: <c>text-sm</c> at
        /// <c>sm</c> and <c>text-base</c> at <c>default</c> and <c>lg</c>, for both segments.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, DateTimeField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new DateTimeField { size = size };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var tier = FieldTextSize.TextFieldTier(size);
                    var date = field.Q(className: DateTimeField.dateInputUssClassName).Q<TextElement>();
                    var time = field.Q(className: DateTimeField.timeInputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, date, tier, $"a {size} DateTimeField's date");
                    FieldTextSize.AssertTier(stage, time, tier, $"a {size} DateTimeField's time");
                    FieldTextSize.AssertFits(stage, date, field, $"a {size} DateTimeField's date");
                }
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in FieldTextSize.sizes)
                FieldTextSize.AssertFontSize("DateTimeField.uss",
                    $".appui-datetime-field--size-{suffix} .appui-datetime-field__date-input",
                    FieldTextSize.Step(FieldTextSize.TextFieldTier(size)));
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("DateTimeField.uss"),
                "DateTimeField reads the numbered steps the registry reads, never a body-* alias.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));

            foreach (var name in new[] { "sm", "md", "lg" })
            {
                var body = RuleBody(uss, ".appui-datetime-field--size-" + name);
                StringAssert.Contains("height: var(--appui-alias-control-sizing-" + name + "-height)", body,
                    $".appui-datetime-field--size-{name} must bind its height to the control-sizing alias.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-datetime-field--size-{name} uses a raw --appui-sizing step instead of the alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-datetime-field--size-{name} uses a literal pixel value instead of the alias.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesNoLiteralColoursOrMagicPixels()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "DateTimeField.uss holds a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "DateTimeField.uss holds a literal rgb() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*\d+(\.\d+)?px"),
                "DateTimeField.uss holds a magic pixel value where a token belongs.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStyleSheet()
        {
            var appUi = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("@import \"./Components/DateTimeField.uss\";", appUi,
                "DateTimeField.uss is not imported by App UI.uss, so none of its rules ever load.");
        }
    }
}
