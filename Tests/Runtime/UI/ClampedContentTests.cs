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
    [TestOf(typeof(ClampedContent))]
    class ClampedContentTests : VisualElementTests<ClampedContent>
    {
        protected override string mainUssClassName => ClampedContent.ussClassName;

        // Handed over to ClampedContentSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ClampedContent.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Overflowing", (ctx) => Build(100f, 400f));
                yield return new Story("Fitting", (ctx) => Build(200f, 60f));
            }
        }

        static ClampedContent Build(float maxHeight, float contentHeight)
        {
            var clamped = new ClampedContent { maxHeight = maxHeight, style = { width = 320 } };
            clamped.Add(new VisualElement { style = { height = contentHeight } });
            return clamped;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ClampedContent />",
            @"<appui:ClampedContent max-height=""120"" more-label=""Read more"" less-label=""Collapse"" />",
            @"<appui:ClampedContent max-height=""80""><appui:Text text=""Long"" /></appui:ClampedContent>",
            @"<appui:ClampedContent max-height=""60"" more-label=""Read more"" less-label=""Collapse"" fade-visible=""false"" expanded=""true"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var clamped = new ClampedContent();
            Assert.AreEqual(ClampedContent.defaultMaxHeight, clamped.maxHeight);
            Assert.IsFalse(clamped.expanded);
            Assert.IsFalse(clamped.overflowing);
            Assert.IsTrue(clamped.fadeVisible);
            Assert.AreEqual("Show more", clamped.moreLabel);
            Assert.AreEqual("Show less", clamped.lessLabel);
        }

        [Test]
        public void Content_LandsInTheClippedContainerNotBesideTheToggle()
        {
            var clamped = new ClampedContent();
            var child = new VisualElement();
            clamped.Add(child);

            Assert.AreEqual(1, clamped.childCount);
            Assert.IsTrue(child.hierarchy.parent.ClassListContains(ClampedContent.containerUssClassName),
                "Content must be added to the clipped container, not to the root beside the toggle.");
        }

        [UnityTest]
        public IEnumerator ShortContent_NeitherClipsNorShowsTheToggle()
        {
            var clamped = Build(200f, 60f);
            yield return Mount(clamped);

            Assert.IsFalse(clamped.overflowing, "Content shorter than the cap must not report overflow.");
            var toggle = clamped.Q(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual(DisplayStyle.None, toggle.resolvedStyle.display,
                "The toggle must be hidden when the content fits.");
            var fade = clamped.Q(className: ClampedContent.fadeUssClassName);
            Assert.AreEqual(DisplayStyle.None, fade.resolvedStyle.display,
                "The fade must be hidden when the content fits.");
        }

        [UnityTest]
        public IEnumerator TallContent_ClipsToMaxHeightAndRevealsTheToggle()
        {
            var clamped = Build(100f, 400f);
            yield return Mount(clamped);

            Assert.IsTrue(clamped.overflowing, "Content taller than the cap must report overflow.");
            Assert.IsTrue(clamped.ClassListContains(ClampedContent.overflowingUssClassName));

            var viewport = clamped.Q(className: ClampedContent.viewportUssClassName);
            Assert.AreEqual(100f, viewport.resolvedStyle.height, 1f,
                "The viewport must be capped at maxHeight while collapsed.");

            var toggle = clamped.Q<LocalizedTextElement>(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, toggle.resolvedStyle.display);
            Assert.AreEqual("Show more", toggle.text);

            var fade = clamped.Q(className: ClampedContent.fadeUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, fade.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Expanding_LiftsTheCapSwapsTheLabelAndKeepsTheToggle()
        {
            var clamped = Build(100f, 400f);
            yield return Mount(clamped);
            Assert.IsTrue(clamped.overflowing);

            var raised = 0;
            var lastValue = false;
            clamped.expandedChanged += v => { raised++; lastValue = v; };

            clamped.expanded = true;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(1, raised);
            Assert.IsTrue(lastValue);

            var viewport = clamped.Q(className: ClampedContent.viewportUssClassName);
            Assert.AreEqual(400f, viewport.resolvedStyle.height, 1f,
                "Expanding must lift the height cap entirely.");

            var toggle = clamped.Q<LocalizedTextElement>(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual("Show less", toggle.text);
            Assert.AreEqual(DisplayStyle.Flex, toggle.resolvedStyle.display,
                "The toggle must survive expanding, or the user cannot collapse again. " +
                "Re-measuring while expanded is exactly the bug that hides it.");

            var fade = clamped.Q(className: ClampedContent.fadeUssClassName);
            Assert.AreEqual(DisplayStyle.None, fade.resolvedStyle.display,
                "The fade must go away once nothing is clipped.");
        }

        [UnityTest]
        public IEnumerator Expanded_KeepsReportingOverflow_BecauseTheContainerIsMeasuredUncapped()
        {
            var clamped = Build(100f, 400f);
            yield return Mount(clamped);
            clamped.expanded = true;

            // Several extra layout passes, each of which re-measures. The web component has to skip
            // measuring here or the toggle disappears; measuring the uncapped container instead of the
            // clipped viewport is what makes that impossible in UI Toolkit.
            for (var i = 0; i < 8; i++)
                yield return null;

            Assert.IsTrue(clamped.overflowing,
                "Re-measuring while expanded reported 'fits', so the toggle would vanish and the user " +
                "could never collapse again. The measurement must read the uncapped container.");
            var toggle = clamped.Q(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, toggle.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Collapsing_RestoresTheCapAndTheMoreLabel()
        {
            var clamped = Build(100f, 400f);
            yield return Mount(clamped);

            clamped.expanded = true;
            for (var i = 0; i < 4; i++)
                yield return null;
            clamped.expanded = false;
            for (var i = 0; i < 4; i++)
                yield return null;

            var viewport = clamped.Q(className: ClampedContent.viewportUssClassName);
            Assert.AreEqual(100f, viewport.resolvedStyle.height, 1f);
            var toggle = clamped.Q<LocalizedTextElement>(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual("Show more", toggle.text);
            Assert.IsTrue(clamped.overflowing);
        }

        [UnityTest]
        public IEnumerator GrowingTheContent_StartsOverflowingWithoutAnExplicitRemeasure()
        {
            var clamped = Build(100f, 60f);
            yield return Mount(clamped);
            Assert.IsFalse(clamped.overflowing);

            clamped[0].style.height = 400f;
            for (var i = 0; i < 5; i++)
                yield return null;

            Assert.IsTrue(clamped.overflowing,
                "The block did not react to its content growing; there is no ResizeObserver equivalent wired up.");
        }

        [UnityTest]
        public IEnumerator RaisingMaxHeightPastTheContent_HidesTheToggleAgain()
        {
            var clamped = Build(100f, 200f);
            yield return Mount(clamped);
            Assert.IsTrue(clamped.overflowing);

            clamped.maxHeight = 400f;
            for (var i = 0; i < 5; i++)
                yield return null;

            Assert.IsFalse(clamped.overflowing);
            var toggle = clamped.Q(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual(DisplayStyle.None, toggle.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator FadeVisible_False_KeepsTheToggleButDropsTheBand()
        {
            var clamped = Build(100f, 400f);
            clamped.fadeVisible = false;
            yield return Mount(clamped);

            Assert.IsTrue(clamped.overflowing);
            var fade = clamped.Q(className: ClampedContent.fadeUssClassName);
            Assert.AreEqual(DisplayStyle.None, fade.resolvedStyle.display);
            var toggle = clamped.Q(className: ClampedContent.toggleUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, toggle.resolvedStyle.display,
                "Hiding the fade must not hide the toggle.");
        }

        [UnityTest]
        public IEnumerator ToggleHeight_ShrinksUnderCompactDensity()
        {
            var normal = Build(100f, 400f);
            yield return Mount(normal, "normal");
            var normalHeight = normal.Q(className: ClampedContent.toggleUssClassName).resolvedStyle.height;

            var compact = Build(100f, 400f);
            yield return Mount(compact, "compact");
            var compactHeight = compact.Q(className: ClampedContent.toggleUssClassName).resolvedStyle.height;

            Assert.Greater(normalHeight, 0f);
            Assert.Less(compactHeight, normalHeight,
                $"The toggle height did not shrink under compact density (normal {normalHeight}, compact {compactHeight}). " +
                "It is probably a hardcoded pixel value instead of the control-sizing alias.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_BindsSizesToTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var toggle = Regex.Match(uss, @"\.appui-clamped-content__toggle\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(toggle.Success, "ClampedContent.uss has no toggle rule.");
            StringAssert.Contains("var(--appui-alias-control-sizing-sm-height)", toggle.Groups["body"].Value,
                "The toggle height must come from the small control-sizing alias.");

            var viewport = Regex.Match(uss, @"\.appui-clamped-content__viewport\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(viewport.Success);
            StringAssert.Contains("overflow: hidden", viewport.Groups["body"].Value,
                "The viewport must clip, or nothing is actually clamped.");
            Assert.IsFalse(Regex.IsMatch(viewport.Groups["body"].Value, @"max-height:"),
                "maxHeight is a per-use content budget written inline from C#, not a USS constant.");

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "ClampedContent.uss contains a magic pixel value.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "ClampedContent.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "ClampedContent.uss contains a literal rgb()/rgba() colour.");
        }

        IEnumerator Mount(ClampedContent clamped, string density = "normal", Dir dir = Dir.Ltr)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en", layoutDirection = dir };
            m_Panel = panel;
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(clamped);

            for (var i = 0; i < 6; i++)
                yield return null;
        }
    }
}
