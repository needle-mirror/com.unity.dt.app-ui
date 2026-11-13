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
    [TestOf(typeof(Link))]
    class LinkTests : VisualElementTests<Link>
    {
        protected override string mainUssClassName => Link.ussClassName;

        // Handed over to LinkSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly LinkSize[] AllSizes = { LinkSize.Sm, LinkSize.Base, LinkSize.Lg };

        static readonly string[] AllSizeNames = { "sm", "base", "lg" };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Link.uss";

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Link />",
            @"<appui:Link text=""Click me"" url=""https://example.com"" />",
            @"<appui:Link text=""Small"" size=""Sm"" />",
            @"<appui:Link text=""Large"" size=""Lg"" />",
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Link("Read the docs", "https://example.com"));
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new Link(size.ToString(), "https://example.com") { size = size });
                    return row;
                });
            }
        }

        /// <summary>
        /// The registry's `linkVariants` is `sm | base | lg` with `base` as the default variant.
        /// </summary>
        [Test]
        public void Size_DefaultsToBase_MatchingTheRegistry()
        {
            var link = new Link("hello");
            Assert.AreEqual(LinkSize.Base, link.size);
            Assert.IsTrue(link.ClassListContains("appui-link--size-base"));
        }

        [Test]
        [TestCase(LinkSize.Sm, ExpectedResult = "appui-link--size-sm")]
        [TestCase(LinkSize.Base, ExpectedResult = "appui-link--size-base")]
        [TestCase(LinkSize.Lg, ExpectedResult = "appui-link--size-lg")]
        public string Size_AppliesTheMatchingUssClass(LinkSize size)
        {
            var link = new Link("hello") { size = size };
            return link.GetClasses().Single(c => c.StartsWith(Link.sizeUssClassName));
        }

        [Test]
        public void Size_RemovesThePreviousSizeUssClass()
        {
            var link = new Link("hello") { size = LinkSize.Lg };
            link.size = LinkSize.Sm;
            var sizeClasses = link.GetClasses().Where(c => c.StartsWith(Link.sizeUssClassName)).ToList();
            CollectionAssert.AreEqual(new[] { "appui-link--size-sm" }, sizeClasses);
        }

        /// <summary>
        /// The regression this fixture exists for: `Link.uss` used to define no `--size-*` rule at
        /// all, so every step rendered at the same inherited font size and the property was dead.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_ResolvesToAStrictlyIncreasingFontSizeRamp()
        {
            var links = new List<Link>();
            yield return MountSizeRamp(links);

            var previous = 0f;
            for (var i = 0; i < AllSizes.Length; i++)
            {
                var fontSize = links[i].resolvedStyle.fontSize;
                Assert.Greater(fontSize, 0f,
                    $"{AllSizes[i]} resolved to a zero font size; the size class is not defined in Link.uss.");
                Assert.Greater(fontSize, previous,
                    $"{AllSizes[i]} is not larger than the previous step " +
                    $"({fontSize} vs {previous}); the Link size classes style nothing.");
                previous = fontSize;
            }
        }

        /// <summary>
        /// A Link must render at the same size as the Text tier it mirrors, since both bind the same
        /// `--appui-font-sizes-*` token.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_MatchesTheEquivalentTextTier()
        {
            var links = new List<Link>();
            yield return MountSizeRamp(links);
            var linkSizes = links.Select(l => l.resolvedStyle.fontSize).ToArray();

            var texts = new List<Text>();
            yield return MountTextRamp(texts);
            var textSizes = texts.Select(t => t.resolvedStyle.fontSize).ToArray();

            for (var i = 0; i < AllSizes.Length; i++)
                Assert.AreEqual(textSizes[i], linkSizes[i], 0.01f,
                    $"Link {AllSizes[i]} does not render at the same size as the matching Text tier.");
        }

        /// <summary>
        /// Compact density shrinks the steps that still vary by density, which only holds while the
        /// steps go through the density-aware font-size tokens rather than literal pixels. `Sm` reads
        /// font-sizes-100, which the 10px text floor holds at 12px in both densities, so it must stay
        /// level instead; `Base` (step 200, 14 to 12) and `Lg` (step 400) still shrink.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_FollowsCompactDensity()
        {
            var normalLinks = new List<Link>();
            yield return MountSizeRamp(normalLinks, "normal");
            var normalSizes = normalLinks.Select(l => l.resolvedStyle.fontSize).ToArray();

            var compactLinks = new List<Link>();
            yield return MountSizeRamp(compactLinks, "compact");
            var compactSizes = compactLinks.Select(l => l.resolvedStyle.fontSize).ToArray();

            for (var i = 0; i < AllSizes.Length; i++)
            {
                if (AllSizes[i] == LinkSize.Sm)
                {
                    Assert.AreEqual(12f, normalSizes[i], 0.01f, "Sm must render at font-sizes-100 (12px) at normal density.");
                    Assert.AreEqual(12f, compactSizes[i], 0.01f,
                        "Sm must stay at 12px under compact density: the text floor holds font-sizes-100 at 12px.");
                    continue;
                }

                Assert.Less(compactSizes[i], normalSizes[i],
                    $"{AllSizes[i]} did not shrink under compact density " +
                    $"(normal {normalSizes[i]}, compact {compactSizes[i]}). " +
                    "The size is probably a hardcoded pixel value.");
            }
        }

        /// <summary>
        /// Pins the styling to the token contract, and to the exact tier mapping Text.uss uses.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void SizeSteps_BindToTheFontSizeTokens()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            foreach (var name in AllSizeNames)
            {
                var rule = Regex.Match(uss, @"\.appui-link--size-" + name + @"\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success, $"Link.uss has no .appui-link--size-{name} rule, " +
                    "so that size step styles nothing.");
                var body = rule.Groups["body"].Value;
                Assert.IsTrue(Regex.IsMatch(body, @"font-size:\s*var\(--appui-font-sizes-\d+\)"),
                    $".appui-link--size-{name} must bind its font-size to an --appui-font-sizes token.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-link--size-{name} uses a literal pixel value instead of a token.");
            }
        }

        /// <summary>
        /// Every step of the enum must have a rule. This is the failure mode where an enum member
        /// renamed on the C# side silently stops matching anything in the stylesheet.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void EveryLinkSize_HasAMatchingUssRule()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            foreach (LinkSize size in System.Enum.GetValues(typeof(LinkSize)))
            {
                var className = Link.sizeUssClassName + size.ToString().ToLower();
                StringAssert.Contains("." + className, uss,
                    $"LinkSize.{size} emits `{className}`, which Link.uss does not define.");
            }
        }

        IEnumerator MountSizeRamp(List<Link> links, string density = "normal")
        {
            var panel = MountPanel(density);
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            panel.Add(row);
            foreach (var size in AllSizes)
            {
                var link = new Link(size.ToString(), "https://example.com") { size = size };
                row.Add(link);
                links.Add(link);
            }

            for (var i = 0; i < 4; i++)
                yield return null;
        }

        IEnumerator MountTextRamp(List<Text> texts)
        {
            var panel = MountPanel("normal");
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            panel.Add(row);
            foreach (var size in new[] { TextSize.S, TextSize.M, TextSize.L })
            {
                var text = new Text(size.ToString()) { size = size };
                row.Add(text);
                texts.Add(text);
            }

            for (var i = 0; i < 4; i++)
                yield return null;
        }

        Panel MountPanel(string density)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            return panel;
        }
    }
}
