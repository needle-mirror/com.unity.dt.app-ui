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
    [TestOf(typeof(AudioWaveform))]
    class AudioWaveformTests : VisualElementTests<AudioWaveform>
    {
        const string k_Uss = "AudioWaveform.uss";

        protected override string mainUssClassName => AudioWaveform.ussClassName;

        // Handed over to AudioWaveformSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new AudioWaveform());
                yield return new Story("WithPeaks", (ctx) => new AudioWaveform
                {
                    peaks = new List<float> { 0.1f, 0.6f, 1f, 0.4f, 0.2f }
                });
                yield return new Story("WithPlayhead", (ctx) => new AudioWaveform
                {
                    peaks = new List<float> { 0.1f, 0.6f, 1f, 0.4f, 0.2f },
                    progress = new Optional<float>(0.5f)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:AudioWaveform />",
            @"<appui:AudioWaveform progress=""0.25"" />",
        };

        static AudioWaveform Waveform(params float[] peaks) => new AudioWaveform
        {
            peaks = new List<float>(peaks)
        };

        [Test]
        public void Peaks_DefaultToEmpty_AndSurviveNull()
        {
            var w = new AudioWaveform();
            Assert.IsNotNull(w.peaks, "peaks must never be null, so a caller can append without a guard.");
            Assert.AreEqual(0, w.peaks.Count);

            Assert.DoesNotThrow(() => w.peaks = null);
            Assert.IsNotNull(w.peaks);
            Assert.AreEqual(0, w.peaks.Count);
        }

        [Test]
        public void Progress_IsUnsetByDefault()
        {
            Assert.IsFalse(new AudioWaveform().progress.IsSet,
                "No progress means no playhead — the registry hides it when progress is null.");
        }

        [Test]
        public void Bars_AreMirroredAroundTheBaseline_AndScaleWithThePeak()
        {
            var w = Waveform(1f, 0.5f);
            var rect = new Rect(0f, 0f, 100f, 100f);
            w.BuildBars(rect);

            Assert.AreEqual(2, w.bars.Count, "One bar per peak.");

            // x = (i + 0.5) * width / count, so the two bars sit at a quarter and three quarters.
            Assert.AreEqual(25f, w.bars[0].x, 0.001f);
            Assert.AreEqual(75f, w.bars[1].x, 0.001f);

            // A full peak spans the whole height, mirrored around the midline.
            Assert.AreEqual(0f, w.bars[0].y, 0.001f);
            Assert.AreEqual(100f, w.bars[0].z, 0.001f);

            // Half a peak spans half the height, still centred.
            Assert.AreEqual(25f, w.bars[1].y, 0.001f);
            Assert.AreEqual(75f, w.bars[1].z, 0.001f);

            var mid0 = (w.bars[0].y + w.bars[0].z) * 0.5f;
            var mid1 = (w.bars[1].y + w.bars[1].z) * 0.5f;
            Assert.AreEqual(mid0, mid1, 0.001f, "Every bar is mirrored around the same baseline.");
        }

        [Test]
        public void Bars_ClampOutOfRangePeaks_AndSkipSilence()
        {
            var w = Waveform(2f, -1f, 0f, 0.5f);
            w.BuildBars(new Rect(0f, 0f, 100f, 100f));

            // Silent and negative peaks draw nothing; the registry `continue`s on p <= 0.
            Assert.AreEqual(2, w.bars.Count,
                "A zero or negative peak draws no bar, so silence reads as the bare baseline.");

            // The over-range peak is clamped to 1, not allowed to overflow the element.
            Assert.AreEqual(0f, w.bars[0].y, 0.001f);
            Assert.AreEqual(100f, w.bars[0].z, 0.001f);
        }

        [Test]
        public void Playhead_TracksProgress_AndClampsTo01()
        {
            var w = Waveform(1f);

            Assert.AreEqual(0f, w.PlayheadX(200f), 0.001f,
                "An unset progress parks the playhead at zero and it is not drawn.");

            w.progress = new Optional<float>(0.25f);
            Assert.AreEqual(50f, w.PlayheadX(200f), 0.001f);

            w.progress = new Optional<float>(4f);
            Assert.AreEqual(200f, w.PlayheadX(200f), 0.001f, "Progress above 1 clamps to the right edge.");

            w.progress = new Optional<float>(-4f);
            Assert.AreEqual(0f, w.PlayheadX(200f), 0.001f, "Progress below 0 clamps to the left edge.");
        }

        [Test]
        public void Element_PaintsItself()
        {
            Assert.IsNotNull(new AudioWaveform().generateVisualContent,
                "The waveform draws with Painter2D; without a generateVisualContent hook it is a blank box.");
        }

        [UnityTest]
        public IEnumerator Colours_ResolveFromTheStylesheetRatherThanLiterals()
        {
            var w = Waveform(0.5f, 1f);
            var host = new VisualElement { style = { width = 400, height = 120 } };
            host.Add(w);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreNotEqual(Color.clear, w.resolvedWaveformColor,
                "The bar colour must come from --waveform-color. The C# field starts transparent on "
                + "purpose, so an unresolved custom property cannot pass for a plausible default.");
            Assert.AreNotEqual(Color.clear, w.resolvedBaselineColor);
            Assert.AreNotEqual(Color.clear, w.resolvedPlayheadColor);

            Assert.AreNotEqual(w.resolvedWaveformColor, w.resolvedBaselineColor,
                "The faint centre baseline must read differently from the bars.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Colours_BindToTokensThroughCustomProperties()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"^\.appui-audio-waveform");
            StringAssert.Contains("--waveform-color: var(--appui-alias-actions-accent-background-color-default)", body);
            StringAssert.Contains("--waveform-baseline-color: var(--appui-base-500)", body);
            StringAssert.Contains("--waveform-playhead-color: var(--appui-alias-text-primary)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Height_ComesFromASizingToken()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"^\.appui-audio-waveform");
            StringAssert.Contains("height: var(--appui-sizing-", body,
                "The waveform's height must ride the density-aware sizing ramp, not a literal.");
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
            StringAssert.Contains("./Components/AudioWaveform.uss", main,
                "An unimported stylesheet silently styles nothing.");
        }
    }
}
