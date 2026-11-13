using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AudioPreview))]
    class AudioPreviewTests : VisualElementTests<AudioPreview>
    {
        protected override string mainUssClassName => AudioPreview.ussClassName;

        // Handed over to AudioPreviewSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static List<float> Envelope()
        {
            var peaks = new List<float>();
            for (var i = 0; i < 32; i++)
                peaks.Add(0.2f + 0.6f * (i % 8) / 8f);
            return peaks;
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new AudioPreview
                {
                    peaks = Envelope(), durationLabel = "0:12", style = { width = 280 },
                });
                yield return new Story("Playing", (ctx) => new AudioPreview
                {
                    peaks = Envelope(), progress = 0.4f, playing = true, style = { width = 280 },
                });
                yield return new Story("Loading", (ctx) => new AudioPreview
                {
                    loading = true, style = { width = 280 },
                });
                yield return new Story("Message", (ctx) => new AudioPreview
                {
                    message = "Clip too large to preview", style = { width = 280 },
                });
                yield return new Story("Fallback", (ctx) => new AudioPreview
                {
                    fallbackIcon = "music-note", style = { width = 280 },
                });
                yield return new Story("Strip", (ctx) => new AudioPreview
                {
                    previewLayout = AudioPreviewLayout.Strip, peaks = Envelope(), progress = 0.4f,
                    durationLabel = "0:12", style = { width = 280 },
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:AudioPreview />",
            @"<appui:AudioPreview playing=""true"" progress=""0.4"" duration-label=""0:12"" />",
            @"<appui:AudioPreview loading=""true"" />",
            @"<appui:AudioPreview message=""Clip too large to preview"" />",
            @"<appui:AudioPreview fallback-icon=""music-note"" play-label=""Play"" stop-label=""Stop"" />",
            @"<appui:AudioPreview layout=""Strip"" duration-label=""0:12"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/AudioPreview.uss";

        static string ReadUss() => File.ReadAllText(Path.GetFullPath(k_UssPath));

        /// <summary>
        /// A clip whose envelope was never computed is still playable, so a null <c>peaks</c> must
        /// draw a flat baseline rather than an empty box.
        /// </summary>
        [Test]
        public void NullPeaks_DrawAFlatBaselineRatherThanNothing()
        {
            var preview = new AudioPreview { peaks = null };

            var waveform = preview.currentBody as AudioWaveform;
            Assert.IsNotNull(waveform, "A null envelope must still render the waveform.");
            Assert.AreEqual(AudioPreview.flatPeakCount, waveform.peaks.Count,
                "The flat baseline must have the registry's FLAT_PEAKS bar count.");
            foreach (var peak in waveform.peaks)
            {
                Assert.AreEqual(AudioPreview.flatPeakAmplitude, peak, 0.0001f);
            }
            Assert.IsTrue(waveform.ClassListContains(AudioPreview.flatUssClassName),
                "The flat baseline must be marked so it can be drawn more quietly than a real envelope.");
        }

        [Test]
        public void RealPeaks_AreDrawnAndNotMarkedFlat()
        {
            var peaks = Envelope();
            var preview = new AudioPreview { peaks = peaks };

            var waveform = preview.currentBody as AudioWaveform;
            Assert.IsNotNull(waveform);
            Assert.AreEqual(peaks.Count, waveform.peaks.Count);
            Assert.IsFalse(waveform.ClassListContains(AudioPreview.flatUssClassName));
        }

        /// <summary>
        /// The four body states have a documented precedence. Testing them one at a time would let
        /// a wrong order pass, so each case here sets every competing prop at once.
        /// </summary>
        [Test]
        public void Message_BeatsLoadingAndTheFallbackAndTheEnvelope()
        {
            var preview = new AudioPreview
            {
                peaks = Envelope(),
                loading = true,
                fallbackIcon = "music-note",
                message = "Clip too large to preview",
            };

            Assert.IsInstanceOf<LocalizedTextElement>(preview.currentBody,
                "message must replace the envelope outright.");
        }

        [Test]
        public void Loading_BeatsTheFallbackAndTheEnvelope()
        {
            var preview = new AudioPreview
            {
                peaks = null, fallbackIcon = "music-note", loading = true,
            };

            Assert.IsInstanceOf<Skeleton>(preview.currentBody,
                "loading must win over the fallback symbol.");
        }

        [Test]
        public void FallbackIcon_ShowsOnlyForANullEnvelope()
        {
            var withEnvelope = new AudioPreview { peaks = Envelope(), fallbackIcon = "music-note" };
            Assert.IsInstanceOf<AudioWaveform>(withEnvelope.currentBody,
                "A real envelope must win over the fallback symbol.");

            var withoutEnvelope = new AudioPreview { peaks = null, fallbackIcon = "music-note" };
            Assert.IsInstanceOf<Icon>(withoutEnvelope.currentBody);
        }

        /// <summary>
        /// The control reports intent only. The caller owns playback, so the component must not
        /// flip <see cref="AudioPreview.playing"/> by itself — a host whose clip failed to start
        /// would otherwise show a stop button for silence.
        /// </summary>
        [Test]
        public void Toggling_RaisesTheEventWithoutChangingTheState()
        {
            var preview = new AudioPreview();
            var raised = 0;
            preview.toggled += () => raised++;

            preview.Q<IconButton>(className: AudioPreview.playButtonUssClassName).clickable.InvokePressed(null);

            Assert.AreEqual(1, raised);
            Assert.IsFalse(preview.playing, "AudioPreview must not drive its own playback state.");
        }

        [Test]
        public void Playing_SwapsTheSymbolAndTheLabel()
        {
            var preview = new AudioPreview { playLabel = "Play it", stopLabel = "Stop it" };
            var button = preview.Q<IconButton>(className: AudioPreview.playButtonUssClassName);

            Assert.AreEqual("play", button.icon);
            Assert.AreEqual("Play it", button.tooltip);

            preview.playing = true;
            Assert.AreEqual("stop", button.icon);
            Assert.AreEqual("Stop it", button.tooltip);
        }

        [Test]
        public void DurationLabel_IsHiddenWhenEmpty()
        {
            var preview = new AudioPreview();
            var duration = preview.Q(className: AudioPreview.durationUssClassName);
            Assert.AreEqual(DisplayStyle.None, duration.style.display.value,
                "An empty readout must not reserve space.");

            preview.durationLabel = "0:12";
            Assert.AreEqual(DisplayStyle.Flex, duration.style.display.value);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColoursOrSizes()
        {
            var uss = ReadUss();
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "AudioPreview.uss contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "AudioPreview.uss contains a literal rgb()/rgba() colour.");
        }

        /// <summary>
        /// The body owns the height so the component does not resize as a clip moves between its
        /// four states, and that height has to be a density-aware token.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Body_TakesItsHeightFromASizingToken()
        {
            var body = RuleBody(ReadUss(), @"\.appui-audio-preview__body");
            StringAssert.Contains("height: var(--appui-sizing-", body,
                "The body height must come from a sizing token, not a literal.");
        }

        /// <summary>
        /// An orphaned stylesheet is silent: the component renders unstyled and nothing errors.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheStyleManifest()
        {
            var manifest = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains(@"@import ""./Components/AudioPreview.uss"";", manifest,
                "AudioPreview.uss must be imported by App UI.uss or it is never loaded.");
        }

        [Test]
        public void Layout_DefaultsToOverlayWithTheControlLaidOverTheEnvelope()
        {
            var preview = new AudioPreview { durationLabel = "0:12" };

            Assert.AreEqual(AudioPreviewLayout.Overlay, preview.previewLayout);
            Assert.IsFalse(preview.ClassListContains(AudioPreview.stripUssClassName));
            Assert.AreEqual(AudioPreview.bodyUssClassName, preview.hierarchy[0].name,
                "In the overlay layout the envelope comes first and the control is laid over it.");
        }

        /// <summary>
        /// The strip reads control, envelope, length on one line, so the control has to lead the
        /// hierarchy rather than float over the envelope's corner.
        /// </summary>
        [Test]
        public void Strip_LaysTheControlEnvelopeAndLengthOutInOrder()
        {
            var preview = new AudioPreview { durationLabel = "0:12", previewLayout = AudioPreviewLayout.Strip };

            Assert.IsTrue(preview.ClassListContains(AudioPreview.stripUssClassName));
            Assert.AreEqual(AudioPreview.playButtonUssClassName, preview.hierarchy[0].name);
            Assert.AreEqual(AudioPreview.bodyUssClassName, preview.hierarchy[1].name);
            Assert.AreEqual(AudioPreview.durationUssClassName, preview.hierarchy[2].name);

            preview.previewLayout = AudioPreviewLayout.Overlay;
            Assert.IsFalse(preview.ClassListContains(AudioPreview.stripUssClassName));
            Assert.AreEqual(AudioPreview.bodyUssClassName, preview.hierarchy[0].name,
                "Going back to overlay must restore the envelope-first order.");
        }

        /// <summary>
        /// A strip's envelope is too short to hold a symbol, so a null envelope draws the flat
        /// baseline even with a fallback icon set, as the registry does.
        /// </summary>
        [Test]
        public void Strip_DrawsTheFlatBaselineInsteadOfTheFallbackIcon()
        {
            var preview = new AudioPreview { fallbackIcon = "music-note" };
            Assert.IsInstanceOf<Icon>(preview.currentBody);

            preview.previewLayout = AudioPreviewLayout.Strip;
            Assert.IsInstanceOf<AudioWaveform>(preview.currentBody);
            Assert.IsTrue(preview.currentBody.ClassListContains(AudioPreview.flatUssClassName));
        }


        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Strip_TakesItsShortHeightFromASizingToken()
        {
            var uss = ReadUss();
            var body = RuleBody(uss, @"\.appui-audio-preview--strip \.appui-audio-preview__body");
            StringAssert.Contains("height: var(--appui-sizing-750)", body,
                "The strip's short envelope must come from a sizing token.");
            var button = RuleBody(uss, @"\.appui-audio-preview--strip \.appui-audio-preview__play-button");
            StringAssert.Contains("position: relative", button,
                "In the strip the control sits in the row, not laid over the envelope.");
        }

        static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"AudioPreview.uss has no {selectorPattern} rule.");
            return rule.Groups["body"].Value;
        }
    }
}
