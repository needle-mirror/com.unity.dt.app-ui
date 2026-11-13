using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [TestOf(typeof(UnityLogo))]
    class UnityLogoTests : VisualElementTests<UnityLogo>
    {
        protected override string mainUssClassName => UnityLogo.ussClassName;

        // Handed over to UnityLogoSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new UnityLogo());
                yield return new Story("Variants", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Filled });
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Empty });
                    row.Add(new UnityLogo { variant = UnityLogoVariant.Animated });
                    return row;
                });
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new UnityLogo { size = size });
                    return row;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:UnityLogo />",
            @"<appui:UnityLogo variant=""Filled"" />",
            @"<appui:UnityLogo variant=""Empty"" size=""Lg"" />",
            @"<appui:UnityLogo variant=""Animated"" size=""Sm"" />",
        };

        static readonly IconSize[] AllSizes =
        {
            IconSize.Xxs, IconSize.Xs, IconSize.Sm, IconSize.Md, IconSize.Lg
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/UnityLogo.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        [Test]
        public void Default_IsFilledAndMedium()
        {
            var logo = new UnityLogo();
            Assert.AreEqual(UnityLogoVariant.Filled, logo.variant);
            Assert.AreEqual(IconSize.Md, logo.size);
            Assert.IsTrue(logo.ClassListContains(UnityLogo.variantUssClassName + "filled"));
        }

        [Test]
        [TestCase(UnityLogoVariant.Filled, ExpectedResult = "appui-unity-logo--filled")]
        [TestCase(UnityLogoVariant.Empty, ExpectedResult = "appui-unity-logo--empty")]
        [TestCase(UnityLogoVariant.Animated, ExpectedResult = "appui-unity-logo--animated")]
        public string Variant_AppliesTheMatchingUssClass(UnityLogoVariant variant)
        {
            var logo = new UnityLogo { variant = variant };
            return logo.GetClasses().Single(c => c.StartsWith(UnityLogo.variantUssClassName));
        }

        [Test]
        public void Variant_RemovesThePreviousVariantUssClass()
        {
            var logo = new UnityLogo { variant = UnityLogoVariant.Empty };
            logo.variant = UnityLogoVariant.Filled;
            var variantClasses = logo.GetClasses().Where(c => c.StartsWith(UnityLogo.variantUssClassName)).ToList();
            CollectionAssert.AreEqual(new[] { "appui-unity-logo--filled" }, variantClasses);
        }

        [Test]
        [TestCase(IconSize.Xxs)]
        [TestCase(IconSize.Xs)]
        [TestCase(IconSize.Sm)]
        [TestCase(IconSize.Md)]
        [TestCase(IconSize.Lg)]
        public void Size_ForwardsToTheIcon(IconSize size)
        {
            var logo = new UnityLogo { size = size };
            Assert.AreEqual(size, logo.size);
            Assert.AreEqual(size, logo.icon.size,
                "UnityLogo must forward its size to the Icon it renders through.");
        }

        /// <summary>
        /// The logo must be drawn through the Icon component, using the brand asset the package
        /// already ships, not a new or invented image.
        /// </summary>
        [Test]
        public void Logo_IsRenderedThroughTheIconComponent()
        {
            var logo = new UnityLogo();
            Assert.IsNotNull(logo.icon);
            Assert.IsTrue(logo.icon.ClassListContains(Icon.ussClassName));
            Assert.IsTrue(logo.icon.ClassListContains(UnityLogo.iconUssClassName));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Logo_UsesThePackagesShippedBrandAsset()
        {
            var uss = ReadUss();
            StringAssert.Contains("../../Logos/Unity-Logo-White.png", uss,
                "UnityLogo.uss must point at the brand asset the package already ships.");
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-icon-primary)", uss,
                "The logo tint must come from the icon token, mirroring the registry's currentColor fill.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void EmptyVariant_DimsTheLogoToTheRegistrysEmptyOpacity()
        {
            var body = RuleBody(ReadUss(), @"\.appui-unity-logo--empty > \.appui-unity-logo__icon");
            StringAssert.Contains("opacity: " + UnityLogo.emptyOpacity.ToString("0.0#"), body,
                "The empty variant must use the same dim opacity as the registry.");
        }

        [UnityTest]
        public IEnumerator EmptyVariant_ResolvesToADimmerLogoThanFilled()
        {
            var filled = new UnityLogo();
            var empty = new UnityLogo { variant = UnityLogoVariant.Empty };
            yield return Mount(filled, empty);

            Assert.AreEqual(1f, filled.icon.resolvedStyle.opacity, 0.01f);
            Assert.AreEqual(UnityLogo.emptyOpacity, empty.icon.resolvedStyle.opacity, 0.01f,
                "The empty variant must resolve to the dimmed opacity; its USS rule is probably missing.");
        }

        /// <summary>
        /// The fill envelope is only right if its constants are. Pinned to the literal values of the
        /// web implementation: a 0.3 dim for the unfilled logo and a two second sweep. Asserting
        /// against <see cref="UnityLogo.emptyOpacity"/> elsewhere is tautological.
        /// </summary>
        [Test]
        public void SweepConstants_MatchTheWebSweep()
        {
            Assert.AreEqual(0.3f, UnityLogo.emptyOpacity, 0.001f,
                "The registry dims the unfilled logo to 0.3.");
            Assert.AreEqual(2000f, UnityLogo.sweepPeriodMs, 0.001f,
                "The registry sweep runs on a 2s cycle.");
        }

        /// <summary>
        /// The animated variant fills in from empty and restarts, mirroring the registry's radial
        /// sweep envelope. Time is frozen by the harness, so the tick is driven by hand.
        /// </summary>
        [Test]
        public void FillOpacity_RampsFromEmptyToFullAcrossThePeriod()
        {
            var periodSeconds = UnityLogo.sweepPeriodMs / 1000f;
            Assert.AreEqual(UnityLogo.emptyOpacity, UnityLogo.ComputeSweepOpacity(0f), 0.001f,
                "The sweep must start from the empty logo.");
            Assert.AreEqual(1f, UnityLogo.ComputeSweepOpacity(periodSeconds * 0.999f), 0.01f,
                "The sweep must reach the full logo at the end of the period.");
            Assert.AreEqual(UnityLogo.emptyOpacity, UnityLogo.ComputeSweepOpacity(periodSeconds), 0.001f,
                "The sweep must restart from empty.");

            var previous = UnityLogo.ComputeSweepOpacity(0f);
            for (var i = 1; i < 20; i++)
            {
                var value = UnityLogo.ComputeSweepOpacity(periodSeconds * i / 20f);
                Assert.Greater(value, previous, "The sweep must increase monotonically within a period.");
                previous = value;
            }
        }

        [UnityTest]
        public IEnumerator AnimatedVariant_DrivesTheIconOpacity()
        {
            var logo = new UnityLogo { variant = UnityLogoVariant.Animated };
            yield return Mount(logo);
            Assert.IsTrue(logo.isAnimating, "An attached, animated UnityLogo must be driving its sweep.");

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = 0f;
                logo.TickSweep();
                Assert.AreEqual(UnityLogo.emptyOpacity, logo.icon.resolvedStyle.opacity, 0.01f);

                TimeUtils.timeOverride = UnityLogo.sweepPeriodMs / 2000f;
                logo.TickSweep();
                Assert.Greater(logo.icon.resolvedStyle.opacity, UnityLogo.emptyOpacity + 0.05f,
                    "Half way through the period the logo must be visibly more filled.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator LeavingTheAnimatedVariant_StopsTheDriverAndClearsTheInlineOpacity()
        {
            var logo = new UnityLogo { variant = UnityLogoVariant.Animated };
            yield return Mount(logo);

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = 0f;
                logo.TickSweep();
                Assert.Less(logo.icon.resolvedStyle.opacity, 1f);

                logo.variant = UnityLogoVariant.Filled;
                yield return null;
                Assert.IsFalse(logo.isAnimating, "Leaving the animated variant must stop the sweep driver.");
                Assert.AreEqual(1f, logo.icon.resolvedStyle.opacity, 0.01f,
                    "The inline opacity from the sweep must be cleared so the filled variant is fully opaque.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator NonAnimatedVariants_NeverStartTheDriver()
        {
            var filled = new UnityLogo();
            var empty = new UnityLogo { variant = UnityLogoVariant.Empty };
            yield return Mount(filled, empty);
            Assert.IsFalse(filled.isAnimating);
            Assert.IsFalse(empty.isAnimating);
        }

        [UnityTest]
        public IEnumerator DetachingFromThePanel_StopsTheDriver()
        {
            var logo = new UnityLogo { variant = UnityLogoVariant.Animated };
            yield return Mount(logo);
            Assert.IsTrue(logo.isAnimating);

            logo.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(logo.isAnimating, "A detached UnityLogo must not keep a scheduled item alive.");
        }

        /// <summary>
        /// The size ramp is Icon's, so it must stay bound to the icon control-sizing aliases and keep
        /// rescaling with density.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_ResolvesToAStrictlyIncreasingSquareRamp()
        {
            var logos = AllSizes.Select(s => new UnityLogo { size = s }).ToArray();
            yield return Mount(logos);

            var previous = 0f;
            for (var i = 0; i < AllSizes.Length; i++)
            {
                var width = logos[i].icon.resolvedStyle.width;
                var height = logos[i].icon.resolvedStyle.height;
                Assert.Greater(width, 0f, $"{AllSizes[i]} resolved to a zero width.");
                Assert.AreEqual(width, height, 0.01f, $"{AllSizes[i]} is not square.");
                Assert.Greater(width, previous, $"{AllSizes[i]} is not larger than the previous step.");
                previous = width;
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColoursOrPixels()
        {
            var uss = ReadUss();
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "UnityLogo.uss contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "UnityLogo.uss contains a literal rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*\d+(\.\d+)?px"), "UnityLogo.uss contains a literal pixel value.");
        }

        static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"UnityLogo.uss has no {selectorPattern} rule.");
            return rule.Groups["body"].Value;
        }

        IEnumerator Mount(params VisualElement[] elements)
        {
            return Mount("normal", elements);
        }

        IEnumerator Mount(string density, params VisualElement[] elements)
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
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            panel.Add(row);
            foreach (var element in elements)
                row.Add(element);

            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
