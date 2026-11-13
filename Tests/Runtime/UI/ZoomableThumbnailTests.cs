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
    [TestOf(typeof(ZoomableThumbnail))]
    class ZoomableThumbnailTests : VisualElementTests<ZoomableThumbnail>
    {
        protected override string mainUssClassName => ZoomableThumbnail.ussClassName;

        // Handed over to ZoomableThumbnailSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ZoomableThumbnail.uss";

        readonly List<Texture2D> m_Textures = new List<Texture2D>();

        [TearDown]
        public void DisposeTextures()
        {
            foreach (var texture in m_Textures)
                Object.DestroyImmediate(texture);
            m_Textures.Clear();
        }

        Background Image(int width, int height)
        {
            var texture = new Texture2D(width, height);
            m_Textures.Add(texture);
            return Background.FromTexture2D(texture);
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Corner", (ctx) => new ZoomableThumbnail
                {
                    src = Image(64, 64),
                    caption = "cliff-face-01.png",
                    alt = "Cliff face, tiling",
                    style = { width = 96, height = 96 },
                });
                yield return new Story("Whole", (ctx) => new ZoomableThumbnail
                {
                    src = Image(64, 64),
                    caption = "cliff-face-01.png",
                    triggerMode = ZoomTriggerMode.Whole,
                    style = { width = 48, height = 48 },
                });
                yield return new Story("NoImage", (ctx) => new ZoomableThumbnail
                {
                    caption = "missing.png",
                    fallback = new Text { text = "No preview" },
                    style = { width = 96, height = 96 },
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ZoomableThumbnail />",
            @"<appui:ZoomableThumbnail caption=""a.png"" alt=""A"" trigger-mode=""Whole"" trigger=""false"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var thumb = new ZoomableThumbnail();
            Assert.AreEqual(ZoomTriggerMode.Corner, thumb.triggerMode,
                "The registry defaults triggerMode to `corner`.");
            Assert.IsTrue(thumb.trigger, "The registry defaults `trigger` to true.");
            Assert.IsFalse(thumb.hasImage);
        }

        [Test]
        public void TriggerMode_SwapsItsClassWithoutLeavingTheOldOne()
        {
            var thumb = new ZoomableThumbnail();
            Assert.IsTrue(thumb.ClassListContains("appui-zoomablethumbnail--corner"));

            thumb.triggerMode = ZoomTriggerMode.Whole;
            Assert.IsTrue(thumb.ClassListContains("appui-zoomablethumbnail--whole"));
            Assert.IsFalse(thumb.ClassListContains("appui-zoomablethumbnail--corner"),
                "A stale mode class would leave the corner control's absolute positioning applied.");
        }

        [Test]
        public void EnumMemberNames_MatchTheClassNamesTheUssDefines()
        {
            // The [EnumName] generator lower-cases the member name, so a rename here silently leaves a
            // mode with no styling at all.
            foreach (var mode in new[] { ZoomTriggerMode.Corner, ZoomTriggerMode.Whole })
            {
                var thumb = new ZoomableThumbnail { triggerMode = mode };
                Assert.IsTrue(
                    thumb.ClassListContains("appui-zoomablethumbnail--" + mode.ToString().ToLowerInvariant()),
                    $"{mode} must emit the class name the USS defines.");
            }
        }

        [Test]
        public void Caption_NamesTheControlAndTitlesTheViewer()
        {
            var thumb = new ZoomableThumbnail { caption = "cliff-face-01.png" };
            Assert.AreEqual("cliff-face-01.png", thumb.triggerButton.assetName,
                "The zoom control's accessible wording comes from the caption.");
            Assert.AreEqual("cliff-face-01.png", thumb.lightbox.caption);
        }

        [Test]
        public void Control_HidesWhileThereIsNothingToEnlarge()
        {
            var thumb = new ZoomableThumbnail();
            Assert.AreEqual(DisplayStyle.None, thumb.triggerButton.resolvedStyle.display,
                "With no image, offering to enlarge it is a lie.");

            thumb.src = Image(64, 64);
            Assert.AreEqual(DisplayStyle.Flex, thumb.triggerButton.resolvedStyle.display);

            thumb.trigger = false;
            Assert.AreEqual(DisplayStyle.None, thumb.triggerButton.resolvedStyle.display,
                "A caller nested inside another interactive element hosts the control itself.");
        }

        [UnityTest]
        public IEnumerator Hover_RevealsTheControl()
        {
            var thumb = new ZoomableThumbnail { src = Image(64, 64), caption = "a.png" };
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(thumb);
            yield return null;

            Assert.IsFalse(thumb.triggerButton.revealed);

            using (var enter = PointerEnterEvent.GetPooled())
            {
                enter.target = thumb;
                thumb.SendEvent(enter);
            }
            Assert.IsTrue(thumb.triggerButton.revealed,
                "USS has no group-hover, so the thumbnail raises the reveal itself.");

            using (var leave = PointerLeaveEvent.GetPooled())
            {
                leave.target = thumb;
                thumb.SendEvent(leave);
            }
            Assert.IsFalse(thumb.triggerButton.revealed,
                "A stuck reveal leaves the control painted over every tile in a grid.");
        }

        [UnityTest]
        public IEnumerator LoadFullSrc_IsNotCalledOnMount_AndAtMostOnceOnOpen()
        {
            var calls = 0;
            var thumb = new ZoomableThumbnail { src = Image(16, 16), caption = "a.png" };
            thumb.loadFullSrc = _ => calls++;
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(thumb);
            yield return null;

            Assert.AreEqual(0, calls, "A grid of these must issue no requests until someone zooms.");

            thumb.OpenZoom();
            thumb.CloseZoom();
            thumb.OpenZoom();
            thumb.CloseZoom();
            yield return null;

            Assert.AreEqual(1, calls, "The resolver is called at most once.");
        }

        [UnityTest]
        public IEnumerator OpenZoom_UsesADismissableModal()
        {
            var thumb = new ZoomableThumbnail { src = Image(16, 16), caption = "a.png" };
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(thumb);
            yield return null;

            thumb.OpenZoom();
            var modal = thumb.zoomModal;
            Assert.IsNotNull(modal, "Opening the zoom builds a Modal through the overlay system.");
            Assert.IsTrue(modal.outsideClickDismissEnabled, "A click outside the viewer must close it.");
            Assert.IsTrue(modal.keyboardDismissEnabled, "Esc must close the viewer.");
            Assert.AreSame(thumb.lightbox, modal.contentView, "The modal hosts the thumbnail's own viewer.");

            thumb.OpenZoom();
            Assert.AreSame(modal, thumb.zoomModal, "Opening again while open must not stack a second modal.");

            thumb.CloseZoom();
            Assert.IsNull(thumb.zoomModal);
        }

        [UnityTest]
        public IEnumerator LateResolution_ForAnImageThatHasMovedOn_IsDropped()
        {
            System.Action<Background> resolve = null;
            var thumb = new ZoomableThumbnail { src = Image(16, 16), caption = "a.png" };
            thumb.loadFullSrc = done => resolve = done;
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(thumb);
            yield return null;

            thumb.OpenZoom();
            Assert.IsNotNull(resolve);

            // The user moved on to a different picture before the fetch came back.
            var next = Image(20, 20);
            thumb.src = next;
            resolve(Image(400, 400));
            thumb.CloseZoom();

            thumb.OpenZoom();
            yield return null;
            Assert.AreEqual(next, thumb.lightbox.src,
                "A resolution belonging to the previous image must not be shown for the new one.");
        }

        [UnityTest]
        public IEnumerator FullSrc_WinsOverTheThumbnail_AndSkipsTheResolver()
        {
            var calls = 0;
            var full = Image(400, 400);
            var thumb = new ZoomableThumbnail { src = Image(16, 16), fullSrc = full, caption = "a.png" };
            thumb.loadFullSrc = _ => calls++;
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(thumb);
            yield return null;

            thumb.OpenZoom();
            yield return null;

            Assert.AreEqual(full, thumb.lightbox.src, "A larger image the caller already has wins.");
            Assert.AreEqual(0, calls, "There is nothing to fetch when the caller already handed one over.");
            thumb.CloseZoom();
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsOnTokensAndSizesGlyphsFromTheIconAliases()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var corner = RuleBody(uss, @"\.appui-zoomablethumbnail--corner \.appui-zoomablethumbnail__trigger");
            StringAssert.Contains("var(--appui-alias-surface-overlay)", corner,
                "The corner control sits on the overlay surface token.");
            StringAssert.Contains("var(--appui-alias-control-border-color-default)", corner);

            var cornerGlyph = RuleBody(uss,
                @"\.appui-zoomablethumbnail--corner \.appui-zoomablethumbnail__trigger \.appui-icon");
            StringAssert.Contains("var(--appui-alias-control-sizing-icon-xs)", cornerGlyph,
                "A glyph inside a control is sized by the control-sizing icon alias.");
            Assert.IsFalse(cornerGlyph.Contains("--appui-sizing-"),
                "A raw sizing step here resolves to the right pixels today and breaks density silently.");

            var whole = RuleBody(uss, @"\.appui-zoomablethumbnail--whole \.appui-zoomablethumbnail__trigger");
            StringAssert.Contains("var(--appui-color-transparent-black-400)", whole,
                "The scrim that says the whole picture is a button must be a token.");

            Assert.IsFalse(uss.Contains(":focus-within"), ":focus-within is not a USS selector.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "No literal colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"rgba?\("), "No rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*[0-9]+(\.[0-9]+)?px"), "No magic pixel value.");
        }

        /// <summary>
        /// Anchored to the start of a line, so a bare class pattern cannot match a descendant rule
        /// further up the file and quietly assert against someone else's declarations.
        /// </summary>
        static string RuleBody(string uss, string selector)
        {
            var match = Regex.Match(uss, @"^" + selector + @"\s*\{(?<body>[^}]*)\}", RegexOptions.Multiline);
            Assert.IsTrue(match.Success, $"ZoomableThumbnail.uss has no rule starting a line with {selector}.");
            return match.Groups["body"].Value;
        }
    }
}
