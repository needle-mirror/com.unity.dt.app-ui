using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TimeField))]
    class TimeFieldTests : VisualElementTests<TimeField>
    {
        protected override string mainUssClassName => TimeField.ussClassName;

        // Handed over to TimeFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/TimeField.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TimeField
                {
                    value = new TimeSpan(9, 30, 0)
                });
                yield return new Story("12-hour with seconds", (ctx) => new TimeField
                {
                    hour12 = true,
                    showSeconds = true,
                    value = new TimeSpan(14, 5, 30)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TimeField />",
            @"<appui:TimeField value=""09:30:00"" size=""Lg"" hour12=""true"" show-seconds=""true"" minute-step=""5"" />",
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

        static UnityEngine.UIElements.TextField Input(TimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: TimeField.inputUssClassName);

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
            var field = new TimeField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(TimeField.sizeUssClassName + "md"));
        }

        [Test]
        public void Size_SwapsTheSizeClassInsteadOfAccumulating()
        {
            var field = new TimeField { size = Size.Lg };
            Assert.IsTrue(field.ClassListContains(TimeField.sizeUssClassName + "lg"));
            Assert.IsFalse(field.ClassListContains(TimeField.sizeUssClassName + "md"),
                "The previous size class was left behind.");

            field.size = Size.Sm;
            Assert.IsTrue(field.ClassListContains(TimeField.sizeUssClassName + "sm"));
            Assert.IsFalse(field.ClassListContains(TimeField.sizeUssClassName + "lg"),
                "The previous size class was left behind.");
        }

        // ── Value model ─────────────────────────────────────────────────────

        [Test]
        public void DefaultValue_IsMidnight()
        {
            Assert.AreEqual(TimeSpan.Zero, new TimeField().value);
        }

        [Test]
        public void Value_WrapsIntoASingleDay()
        {
            var field = new TimeField { value = new TimeSpan(25, 0, 0) };
            Assert.AreEqual(new TimeSpan(1, 0, 0), field.value);

            field.value = new TimeSpan(-1, 0, 0);
            Assert.AreEqual(new TimeSpan(23, 0, 0), field.value);
        }

        [UnityTest]
        public IEnumerator Value_SendsAChangeEvent()
        {
            var field = new TimeField();
            yield return Mount(field);
            var received = TimeSpan.Zero;
            var count = 0;
            field.RegisterValueChangedCallback(evt =>
            {
                received = evt.newValue;
                count++;
            });

            field.value = new TimeSpan(7, 45, 0);
            Assert.AreEqual(1, count);
            Assert.AreEqual(new TimeSpan(7, 45, 0), received);

            // Setting the same value again must not re-notify.
            field.value = new TimeSpan(7, 45, 0);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SetValueWithoutNotify_DoesNotSendAChangeEvent()
        {
            var field = new TimeField();
            var count = 0;
            field.RegisterValueChangedCallback(_ => count++);
            field.SetValueWithoutNotify(new TimeSpan(3, 0, 0));
            Assert.AreEqual(0, count);
            Assert.AreEqual(new TimeSpan(3, 0, 0), field.value);
        }

        // ── Formatting ──────────────────────────────────────────────────────

        [Test]
        public void FormatString_FollowsHour12AndShowSeconds()
        {
            var field = new TimeField();
            Assert.AreEqual("HH:mm", field.formatString);

            field.showSeconds = true;
            Assert.AreEqual("HH:mm:ss", field.formatString);

            field.hour12 = true;
            Assert.AreEqual("hh:mm:ss tt", field.formatString);

            field.showSeconds = false;
            Assert.AreEqual("hh:mm tt", field.formatString);
        }

        [Test]
        public void InputText_MirrorsTheValueInTheCurrentFormat()
        {
            var field = new TimeField { value = new TimeSpan(14, 5, 9) };
            Assert.AreEqual("14:05", Input(field).value);

            field.showSeconds = true;
            Assert.AreEqual("14:05:09", Input(field).value);

            field.hour12 = true;
            Assert.AreEqual("02:05:09 PM", Input(field).value);
        }

        [UnityTest]
        public IEnumerator InputText_IsRewrittenAfterAnEdit()
        {
            var field = new TimeField { value = new TimeSpan(9, 0, 0) };
            yield return Mount(field);
            Input(field).value = "7:5";
            Assert.AreEqual(new TimeSpan(7, 5, 0), field.value);
            Assert.AreEqual("07:05", Input(field).value, "The input keeps a loosely typed string.");
        }

        [UnityTest]
        public IEnumerator UnparseableInput_RevertsToThePreviousValue()
        {
            var field = new TimeField { value = new TimeSpan(9, 0, 0) };
            yield return Mount(field);
            Input(field).value = "not a time";
            Assert.AreEqual(new TimeSpan(9, 0, 0), field.value);
            Assert.AreEqual("09:00", Input(field).value);
        }

        [Test]
        public void TryParse_AcceptsTheDisplayedFormatAndCommonFallbacks()
        {
            var field = new TimeField { hour12 = true };
            Assert.IsTrue(field.TryParse("02:30 PM", out var pm));
            Assert.AreEqual(new TimeSpan(14, 30, 0), pm);

            Assert.IsTrue(field.TryParse("14:30", out var iso));
            Assert.AreEqual(new TimeSpan(14, 30, 0), iso);

            Assert.IsFalse(field.TryParse("banana", out _));
            Assert.IsFalse(field.TryParse("", out _));
        }

        // ── Segment stepping ────────────────────────────────────────────────

        [Test]
        public void StepSegment_WrapsInsideTheSegmentWithoutCarrying()
        {
            var field = new TimeField { value = new TimeSpan(9, 59, 0) };
            field.StepSegment(TimeSegment.Minutes, 1);
            Assert.AreEqual(new TimeSpan(9, 0, 0), field.value,
                "Minutes must wrap to 00 without carrying into the hour.");

            field.value = new TimeSpan(23, 30, 0);
            field.StepSegment(TimeSegment.Hours, 1);
            Assert.AreEqual(new TimeSpan(0, 30, 0), field.value,
                "Hours must wrap to 00 and leave the minutes alone.");

            field.value = new TimeSpan(0, 30, 0);
            field.StepSegment(TimeSegment.Hours, -1);
            Assert.AreEqual(new TimeSpan(23, 30, 0), field.value);
        }

        [Test]
        public void StepSegment_Minutes_UsesMinuteStep()
        {
            var field = new TimeField { minuteStep = 15, value = new TimeSpan(9, 0, 0) };
            field.StepSegment(TimeSegment.Minutes, 1);
            Assert.AreEqual(new TimeSpan(9, 15, 0), field.value);

            field.StepSegment(TimeSegment.Minutes, -1);
            Assert.AreEqual(new TimeSpan(9, 0, 0), field.value);
        }

        [Test]
        public void MinuteStep_IsClampedToAtLeastOne()
        {
            var field = new TimeField { minuteStep = 0 };
            Assert.AreEqual(1, field.minuteStep);

            field.minuteStep = -5;
            Assert.AreEqual(1, field.minuteStep);
        }

        [Test]
        public void StepSegment_Seconds_StepsBySeconds()
        {
            var field = new TimeField { showSeconds = true, value = new TimeSpan(9, 0, 59) };
            field.StepSegment(TimeSegment.Seconds, 1);
            Assert.AreEqual(new TimeSpan(9, 0, 0), field.value);
        }

        [Test]
        public void StepSegment_DayPeriod_ShiftsByTwelveHours()
        {
            var field = new TimeField { hour12 = true, value = new TimeSpan(9, 30, 0) };
            field.StepSegment(TimeSegment.DayPeriod, 1);
            Assert.AreEqual(new TimeSpan(21, 30, 0), field.value);

            field.StepSegment(TimeSegment.DayPeriod, 1);
            Assert.AreEqual(new TimeSpan(9, 30, 0), field.value);
        }

        [Test]
        public void SegmentAtCaret_MapsCaretPositionsToSegments()
        {
            Assert.AreEqual(TimeSegment.Hours, TimeField.SegmentAtCaret(0, false, false));
            Assert.AreEqual(TimeSegment.Hours, TimeField.SegmentAtCaret(1, false, false));
            Assert.AreEqual(TimeSegment.Minutes, TimeField.SegmentAtCaret(4, false, false));
            Assert.AreEqual(TimeSegment.Seconds, TimeField.SegmentAtCaret(7, false, true));
            Assert.AreEqual(TimeSegment.DayPeriod, TimeField.SegmentAtCaret(7, true, false));
        }

        [UnityTest]
        public IEnumerator ArrowKeys_StepTheSegmentUnderTheCaret()
        {
            var field = new TimeField { value = new TimeSpan(9, 30, 0) };
            yield return Mount(field);
            var input = Input(field);

            // Caret defaults to the hours segment.
            Key(input, KeyCode.UpArrow);
            Assert.AreEqual(new TimeSpan(10, 30, 0), field.value);

            Key(input, KeyCode.DownArrow);
            Assert.AreEqual(new TimeSpan(9, 30, 0), field.value);
        }

        // ── Structure ───────────────────────────────────────────────────────

        [Test]
        public void Structure_HasAnInputAndAPickerButton()
        {
            var field = new TimeField();
            Assert.IsNotNull(Input(field), "TimeField must expose an input carrying the input USS class.");
            Assert.IsNotNull(field.Q(className: TimeField.pickerButtonUssClassName),
                "TimeField must expose a picker button.");
        }

        [Test]
        public void Invalid_TracksValidateValue()
        {
            var field = new TimeField
            {
                validateValue = t => t >= new TimeSpan(9, 0, 0)
            };

            field.value = new TimeSpan(8, 0, 0);
            Assert.IsTrue(field.invalid);

            field.value = new TimeSpan(10, 0, 0);
            Assert.IsFalse(field.invalid);
        }

        // ── USS source ──────────────────────────────────────────────────────

        static string RuleBody(string uss, string selector)
        {
            // Anchor at the start of a line so `.appui-other .appui-time-field {` cannot match.
            var rule = Regex.Match(uss, @"^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, $"TimeField.uss has no {selector} rule.");
            return rule.Groups["body"].Value;
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            foreach (var name in new[] { "sm", "md", "lg" })
            {
                var body = RuleBody(uss, ".appui-time-field--size-" + name);
                StringAssert.Contains("height: var(--appui-alias-control-sizing-" + name + "-height)", body,
                    $".appui-time-field--size-{name} must bind its height to the control-sizing alias.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-time-field--size-{name} uses a raw --appui-sizing step instead of the alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-time-field--size-{name} uses a literal pixel value instead of the alias.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void PickerButton_IsSquareFromTheControlSizingAlias()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var body = RuleBody(uss, ".appui-time-field__picker-button");

            StringAssert.Contains("height: var(--appui-alias-control-sizing-sm-square)", body);
            StringAssert.Contains("width: var(--appui-alias-control-sizing-sm-square)", body);
            Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"));
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesNoLiteralColours()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "TimeField.uss holds a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "TimeField.uss holds a literal rgb() colour.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStyleSheet()
        {
            var appUi = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("@import \"./Components/TimeField.uss\";", appUi,
                "TimeField.uss is not imported by App UI.uss, so none of its rules ever load.");
        }

        /// <summary>
        /// The registry's <c>time-field</c> wraps <c>textFieldVariants</c>: <c>text-sm</c> at
        /// <c>sm</c> and <c>text-base</c> at <c>default</c> and <c>lg</c>.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, TimeField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new TimeField { size = size };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var value = field.Q(className: TimeField.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, value, FieldTextSize.TextFieldTier(size), $"a {size} TimeField's value");
                    FieldTextSize.AssertFits(stage, value, field, $"a {size} TimeField's value");
                }
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in FieldTextSize.sizes)
                FieldTextSize.AssertFontSize("TimeField.uss",
                    $".appui-time-field--size-{suffix} .appui-time-field__input",
                    FieldTextSize.Step(FieldTextSize.TextFieldTier(size)));
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("TimeField.uss"),
                "TimeField reads the numbered steps the registry reads, never a body-* alias.");
        }
    }
}
