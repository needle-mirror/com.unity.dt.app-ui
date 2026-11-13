using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ContextCounter))]
    class ContextCounterTests : VisualElementTests<ContextCounter>
    {
        const string k_Uss = "ContextCounter.uss";

        protected override string mainUssClassName => ContextCounter.ussClassName;

        // Handed over to ContextCounterSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Partial", (ctx) => new ContextCounter { used = 120000, total = 200000 });
                yield return new Story("NearlyFull", (ctx) => new ContextCounter { used = 190000, total = 200000 });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ContextCounter />",
            @"<appui:ContextCounter used=""190000"" total=""200000"" label=""Window"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var counter = new ContextCounter();
            Assert.AreEqual(ContextCounter.defaultLabel, counter.label);
            Assert.AreEqual(0.85f, ContextCounter.nearThreshold, 0.001f,
                "The registry warns from 85% of the window.");
        }

        [Test]
        public void Ratio_TracksUsageAndClampsAtFull()
        {
            var counter = new ContextCounter { total = 200, used = 50 };
            Assert.AreEqual(0.25f, counter.ratio, 0.001f);
            Assert.AreEqual(25, counter.percent);
            Assert.AreEqual(0.25f, counter.ring.value, 0.001f);

            counter.used = 400;
            Assert.AreEqual(1f, counter.ratio, 0.001f);
        }

        [Test]
        public void ZeroTotal_ReadsAsEmptyRatherThanDividingByZero()
        {
            var counter = new ContextCounter { total = 0, used = 10 };
            Assert.AreEqual(0f, counter.ratio, 0.001f);
            Assert.IsFalse(counter.near);
        }

        /// <summary>
        /// The warning must switch on exactly at the threshold, not one step past it.
        /// </summary>
        [Test]
        public void Near_SwitchesOnAtTheThreshold()
        {
            var counter = new ContextCounter { total = 100, used = 84 };
            Assert.IsFalse(counter.near);
            Assert.IsFalse(counter.ClassListContains(ContextCounter.nearUssClassName));

            counter.used = 85;
            Assert.IsTrue(counter.near);
            Assert.IsTrue(counter.ClassListContains(ContextCounter.nearUssClassName));

            counter.used = 10;
            Assert.IsFalse(counter.ClassListContains(ContextCounter.nearUssClassName),
                "The near class must come off, not accumulate.");
        }

        [Test]
        public void Tooltip_ShowsTheCompactReadingAndThePercentage()
        {
            var counter = new ContextCounter { used = 120000, total = 200000 };
            Assert.AreEqual("120k / 200k (60%)", counter.tooltip);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The ring is sized off the icon control-sizing alias, never a raw sizing step: the two
        /// resolve identically today, so only reading the source defends the density binding.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Ring_BindsToTheControlSizingIconAliasAndTheAccentToken()
        {
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"^\.appui-context-counter__ring");
            StringAssert.Contains("width: var(--appui-alias-control-sizing-icon-sm)", body);
            StringAssert.Contains("--progress-color: var(--appui-alias-actions-accent-background-color-default)", body);
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(body, @"(width|height):\s*var\(--appui-sizing-\d"),
                "The ring must not size itself off a raw sizing step.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void NearState_TurnsWarningColoured()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("color: var(--appui-alias-semantic-warning)",
                ChatTestUtils.RuleBody(uss, @"\.appui-context-counter--near > \.appui-context-counter__percent"));
        }
    }
}
