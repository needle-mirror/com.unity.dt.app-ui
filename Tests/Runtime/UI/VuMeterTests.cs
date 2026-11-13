using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VuMeter))]
    class VuMeterTests : VisualElementTests<VuMeter>
    {
        const string k_Uss = "VuMeter.uss";

        protected override string mainUssClassName => VuMeter.ussClassName;

        // Handed over to VuMeterSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new VuMeter());
                yield return new Story("Stereo", (ctx) => new VuMeter
                {
                    levels = new List<float> { -12f, -18f },
                    peak = new Optional<float>(-6f)
                });
                yield return new Story("NoReadout", (ctx) => new VuMeter
                {
                    levels = new List<float> { -3f },
                    showReadout = false
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:VuMeter />",
            @"<appui:VuMeter min=""-60"" max=""6"" show-readout=""false"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var m = new VuMeter();
            Assert.AreEqual(-80f, m.min, "Registry default `min = -80`.");
            Assert.AreEqual(20f, m.max, "Registry default `max = 20`.");
            Assert.AreEqual(0f, m.fader, "Registry default `defaultFader = 0`.");
            Assert.IsTrue(m.showReadout, "Registry default `showReadout = true`.");
            CollectionAssert.AreEqual(new[] { 20f, 0f, -20f, -40f, -60f, -80f }, m.ticks,
                "Registry default ticks.");
            Assert.IsFalse(m.peak.IsSet, "No peak-hold marker unless one is supplied.");
        }

        [Test]
        public void Ratio_MapsTheDbScaleOntoZeroToOne_AndClamps()
        {
            var m = new VuMeter { min = -80f, max = 20f };
            Assert.AreEqual(0f, m.Ratio(-80f), 0.0001f);
            Assert.AreEqual(1f, m.Ratio(20f), 0.0001f);
            Assert.AreEqual(0.8f, m.Ratio(0f), 0.0001f, "0 dB sits 80 of the 100 dB up the scale.");
            Assert.AreEqual(0f, m.Ratio(-500f), 0.0001f, "Below the floor clamps to the bottom.");
            Assert.AreEqual(1f, m.Ratio(500f), 0.0001f, "Above the ceiling clamps to the top.");
        }

        [Test]
        public void Ratio_AndDbAt_AreInverses()
        {
            var m = new VuMeter { min = -60f, max = 12f };
            foreach (var db in new[] { -60f, -30f, -6f, 0f, 12f })
                Assert.AreEqual(db, m.DbAt(m.Ratio(db)), 0.001f);
        }

        [Test]
        public void Fader_ClampsToTheScale_AndNotifies()
        {
            var m = new VuMeter { min = -80f, max = 20f };
            var seen = new List<float>();
            m.faderChanged += v => seen.Add(v);

            m.fader = -12f;
            Assert.AreEqual(-12f, m.fader);

            m.fader = 999f;
            Assert.AreEqual(20f, m.fader, "The fader can never leave the scale.");

            m.fader = -999f;
            Assert.AreEqual(-80f, m.fader);

            CollectionAssert.AreEqual(new[] { -12f, 20f, -80f }, seen,
                "Every accepted fader change raises faderChanged exactly once.");

            var before = seen.Count;
            m.fader = -80f;
            Assert.AreEqual(before, seen.Count, "Setting the same value again must not re-notify.");
        }

        [UnityTest]
        public IEnumerator Colours_ResolveFromTheStylesheetRatherThanLiterals()
        {
            var m = new VuMeter { levels = new List<float> { -6f } };
            var host = new VisualElement { style = { width = 120, height = 300 } };
            host.Add(m);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreNotEqual(Color.clear, m.resolvedSafeColor,
                "Every painted colour must come from a token-bound custom property. The fields start "
                + "transparent so an unresolved stylesheet cannot masquerade as a plausible default.");
            Assert.AreNotEqual(Color.clear, m.resolvedWarningColor);
            Assert.AreNotEqual(Color.clear, m.resolvedClipColor);
            Assert.AreNotEqual(Color.clear, m.resolvedTrackColor);
            Assert.AreNotEqual(Color.clear, m.resolvedPeakColor);

            Assert.AreNotEqual(m.resolvedSafeColor, m.resolvedClipColor,
                "Safe and clipping levels must not read the same.");
        }

        [UnityTest]
        public IEnumerator LevelColour_IsAnchoredToTheFader()
        {
            var m = new VuMeter { min = -80f, max = 20f, fader = 0f };
            var host = new VisualElement { style = { width = 120, height = 300 } };
            host.Add(m);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreEqual(m.resolvedSafeColor, m.LevelColorAt(-40f),
                "Well below the fader is safe.");
            Assert.AreEqual(m.resolvedWarningColor, m.LevelColorAt(-6f),
                "Within 12 dB of the fader is the warning band.");
            Assert.AreEqual(m.resolvedClipColor, m.LevelColorAt(6f),
                "Above the fader is clipping.");

            // The band moves with the fader — that is the whole point of anchoring it.
            m.fader = -40f;
            yield return null;
            Assert.AreEqual(m.resolvedClipColor, m.LevelColorAt(-6f),
                "Lowering the fader must drag the clipping band down with it.");
            Assert.AreEqual(m.resolvedSafeColor, m.LevelColorAt(-70f));
        }

        [UnityTest]
        public IEnumerator Readout_ShowsTheLoudestChannel_AndHidesOnRequest()
        {
            var m = new VuMeter { levels = new List<float> { -24f, -9f } };
            var host = new VisualElement { style = { width = 120, height = 300 } };
            host.Add(m);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.IsTrue(m.readoutElement.text.Contains("-9.0"),
                $"The readout reports the loudest channel, got '{m.readoutElement.text}'.");
            StringAssert.Contains("dB", m.readoutElement.text);
            Assert.AreEqual(DisplayStyle.Flex, m.readoutElement.resolvedStyle.display);

            m.showReadout = false;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, m.readoutElement.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Ticks_RenderOneLabelPerEntry()
        {
            var m = new VuMeter { ticks = new List<float> { 0f, -20f, -40f } };
            var host = new VisualElement { style = { width = 120, height = 300 } };
            host.Add(m);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreEqual(3, m.rulerElement.childCount,
                "The ruler is rebuilt from `ticks`, one row each.");

            m.ticks = new List<float> { 0f };
            yield return null;
            Assert.AreEqual(1, m.rulerElement.childCount,
                "Changing ticks must rebuild the ruler rather than accumulate stale rows.");
        }

        [UnityTest]
        public IEnumerator Bars_AreRebuiltPerChannel()
        {
            var m = new VuMeter { levels = new List<float> { -6f } };
            var host = new VisualElement { style = { width = 120, height = 300 } };
            host.Add(m);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreEqual(1, m.barCount, "One bar per channel.");

            m.levels = new List<float> { -6f, -12f };
            yield return null;
            Assert.AreEqual(2, m.barCount, "A stereo source paints two bars.");

            m.levels = new List<float>();
            yield return null;
            Assert.AreEqual(1, m.barCount,
                "With no channels the meter still paints one empty bar rather than collapsing.");
        }

        [Test]
        public void Element_PaintsItself()
        {
            var m = new VuMeter();
            Assert.AreEqual(1, m.barCount, "A meter with no channels still paints one bar.");
            Assert.IsNotNull(m.barsElement[0].generateVisualContent,
                "The bars are drawn with Painter2D; without the hook the meter is a blank strip.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Colours_BindToTokensThroughCustomProperties()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"^\.appui-vu-meter");
            StringAssert.Contains("--vu-meter-safe-color: var(--appui-alias-semantic-positive)", body);
            StringAssert.Contains("--vu-meter-warning-color: var(--appui-alias-semantic-warning)", body);
            StringAssert.Contains("--vu-meter-clip-color: var(--appui-alias-semantic-destructive)", body);
            StringAssert.Contains("--vu-meter-track-color: var(--appui-alias-surface-sunken)", body);
            StringAssert.Contains("--vu-meter-peak-color: var(--appui-alias-text-primary)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStylesheet()
        {
            var main = System.IO.File.ReadAllText(System.IO.Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("./Components/VuMeter.uss", main,
                "An unimported stylesheet silently styles nothing.");
        }
    }
}
