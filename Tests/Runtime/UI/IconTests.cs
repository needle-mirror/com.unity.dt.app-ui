using System;
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
    [TestOf(typeof(Icon))]
    class IconTests : VisualElementTests<Icon>
    {
        protected override string mainUssClassName => Icon.ussClassName;

        // Handed over to IconSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Icon { iconName = "info" });
                yield return new Story("Sizes", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in AllSizes)
                        row.Add(new Icon { iconName = "info", size = size });
                    return row;
                });
                yield return new Story("Disabled", (ctx) =>
                {
                    // Disabled coverage from the UUM-108227 audit.
                    var icon = new Icon { iconName = "info" };
                    icon.SetEnabled(false);
                    return icon;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Icon />",
            @"<appui:Icon icon-name=""info"" size=""Xxs"" />",
            @"<appui:Icon icon-name=""info"" size=""Xs"" />",
            @"<appui:Icon icon-name=""info"" size=""Sm"" />",
            @"<appui:Icon icon-name=""info"" size=""Md"" />",
            @"<appui:Icon icon-name=""info"" size=""Lg"" primary=""false"" />",
            @"<appui:Icon icon-name=""info"" enabled=""false"" />",
        };

        static readonly IconSize[] AllSizes =
        {
            IconSize.Xxs, IconSize.Xs, IconSize.Sm, IconSize.Md, IconSize.Lg
        };

        /// <summary>
        /// The size step names, in ascending order. These must match both the generated USS class
        /// suffixes and the <c>--appui-alias-control-sizing-icon-*</c> token names.
        /// </summary>
        static readonly string[] AllSizeNames = { "xxs", "xs", "sm", "md", "lg" };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Icon.uss";

        [Test]
        public void DefaultSize_IsMd()
        {
            var icon = new Icon();
            Assert.AreEqual(IconSize.Md, icon.size);
            Assert.IsTrue(icon.ClassListContains(Icon.sizeUssClassName + "md"));
        }

        [Test]
        [TestCase(IconSize.Xxs, ExpectedResult = "appui-icon--size-xxs")]
        [TestCase(IconSize.Xs, ExpectedResult = "appui-icon--size-xs")]
        [TestCase(IconSize.Sm, ExpectedResult = "appui-icon--size-sm")]
        [TestCase(IconSize.Md, ExpectedResult = "appui-icon--size-md")]
        [TestCase(IconSize.Lg, ExpectedResult = "appui-icon--size-lg")]
        public string Size_AppliesTheMatchingUssClass(IconSize size)
        {
            var icon = new Icon { size = size };
            return icon.GetClasses().Single(c => c.StartsWith(Icon.sizeUssClassName));
        }

        [Test]
        public void Size_RemovesThePreviousSizeUssClass()
        {
            var icon = new Icon { size = IconSize.Lg };
            icon.size = IconSize.Xs;
            var sizeClasses = icon.GetClasses().Where(c => c.StartsWith(Icon.sizeUssClassName)).ToList();
            CollectionAssert.AreEqual(new[] { "appui-icon--size-xs" }, sizeClasses);
        }

        /// <summary>
        /// The registry's `icon.tsx` defaults `primary = false`, so a bare icon inherits the muted
        /// foreground. UITK used to default it to `true`, which is the divergence this test pins.
        /// </summary>
        [Test]
        public void Primary_DefaultsToFalse_MatchingTheRegistry()
        {
            var icon = new Icon();
            Assert.IsFalse(icon.primary,
                "Icon.primary must default to false, like the registry's `primary = false`.");
            Assert.IsFalse(icon.ClassListContains(Icon.primaryUssClassName),
                "A default Icon must not carry the --primary class.");
        }

        /// <summary>
        /// Flipping the default retints every icon a component builds for itself unless that component
        /// asks for `primary` explicitly. This enumerates the in-package builders and pins that they do.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(ComponentsThatBuildTheirOwnIcons))]
        public void ComponentsThatBuildTheirOwnIcons_AskForPrimaryExplicitly(string label, Func<VisualElement> factory)
        {
            var element = factory();
            var icons = element.Query<Icon>().ToList();
            Assert.IsNotEmpty(icons, $"{label} builds no Icon; the test case is stale.");

            foreach (var icon in icons)
                Assert.IsTrue(icon.primary,
                    $"{label} builds an Icon that does not set primary explicitly, so flipping the " +
                    "Icon default silently retints it from --appui-foregrounds-100 to -50.");
        }

        static IEnumerable<TestCaseData> ComponentsThatBuildTheirOwnIcons()
        {
            TestCaseData Case(string label, Func<VisualElement> factory) =>
                new TestCaseData(label, factory).SetName($"ComponentsThatBuildTheirOwnIcons_AskForPrimaryExplicitly({label})");

            yield return Case("Button", () => new Unity.AppUI.UI.Button { leadingIcon = "info", trailingIcon = "info" });
            yield return Case("ActionButton", () => new ActionButton { icon = "info", trailingIcon = "info" });
            yield return Case("IconButton", () => new IconButton("info"));
            yield return Case("TextField", () => new Unity.AppUI.UI.TextField { leadingIconName = "info", trailingIconName = "info" });
            yield return Case("HelpBox", () => new Unity.AppUI.UI.HelpBox());
            yield return Case("ColorField", () => new ColorField());
            yield return Case("MenuItem", () => new MenuItem());
            yield return Case("Checkbox", () => new Checkbox());
            yield return Case("Dropdown", () => new Dropdown());
            yield return Case("Accordion", () => new AccordionItem());
            yield return Case("Chip", () => new Chip());
            yield return Case("TabItem", () => new TabItem());
            yield return Case("Stepper", () => new Stepper());
            yield return Case("DateField", () => new DateField());
            yield return Case("DateRangeField", () => new DateRangeField());
            yield return Case("AlertDialog", () => new AlertDialog());
            yield return Case("Attachment", () => new Attachment());
            yield return Case("NavigationRailItem", () => new NavigationRailItem());
            yield return Case("BottomNavBarItem", () => new BottomNavBarItem("home", "home", null));
            yield return Case("UnityLogo", () => new UnityLogo());
        }

        [Test]
        public void Primary_TogglesTheUssClass()
        {
            var icon = new Icon { primary = false };
            Assert.IsFalse(icon.ClassListContains(Icon.primaryUssClassName));
            icon.primary = true;
            Assert.IsTrue(icon.ClassListContains(Icon.primaryUssClassName));
        }

        /// <summary>
        /// Every size step must resolve to a real, square, non-zero box, and the ramp must be
        /// strictly increasing. A size class the stylesheet does not define resolves to 0 and fails
        /// here, which is what a name mismatch between the enum and the USS looks like.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_ResolvesToAStrictlyIncreasingSquareRamp()
        {
            var icons = new List<Icon>();
            yield return MountSizeRamp(icons);

            var previous = 0f;
            for (var i = 0; i < AllSizes.Length; i++)
            {
                var width = icons[i].resolvedStyle.width;
                var height = icons[i].resolvedStyle.height;
                Assert.Greater(width, 0f, $"{AllSizes[i]} resolved to a zero width. " +
                    "The size USS class is probably not defined in Icon.uss.");
                Assert.AreEqual(width, height, 0.01f, $"{AllSizes[i]} is not square.");
                Assert.Greater(width, previous, $"{AllSizes[i]} is not larger than the previous step.");
                previous = width;
            }
        }

        /// <summary>
        /// Compact density must shrink the whole ramp. This only holds while the steps go through
        /// the density-aware sizing tokens rather than being hardcoded pixel values.
        /// </summary>
        [UnityTest]
        public IEnumerator EverySize_ShrinksUnderCompactDensity()
        {
            var normalIcons = new List<Icon>();
            yield return MountSizeRamp(normalIcons, "normal");
            var normalWidths = normalIcons.Select(i => i.resolvedStyle.width).ToArray();

            var compactIcons = new List<Icon>();
            yield return MountSizeRamp(compactIcons, "compact");
            var compactWidths = compactIcons.Select(i => i.resolvedStyle.width).ToArray();

            for (var i = 0; i < AllSizes.Length; i++)
            {
                Assert.Less(compactWidths[i], normalWidths[i],
                    $"{AllSizes[i]} did not shrink under compact density " +
                    $"(normal {normalWidths[i]}, compact {compactWidths[i]}). " +
                    "The size is probably a hardcoded pixel value.");
            }
        }

        /// <summary>
        /// Pins the styling to the token contract: every size step must bind to its
        /// <c>--appui-alias-control-sizing-icon-*</c> alias, never to a raw <c>--appui-sizing-NNN</c>
        /// step and never to a literal pixel value. Reverting Icon.uss to raw steps fails here.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void SizeSteps_BindToTheControlSizingIconAliases()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            foreach (var name in AllSizeNames)
            {
                var rule = Regex.Match(uss, @"\.appui-icon--size-" + name + @"\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success, $"Icon.uss has no .appui-icon--size-{name} rule.");
                var body = rule.Groups["body"].Value;
                var expected = "var(--appui-alias-control-sizing-icon-" + name + ")";

                StringAssert.Contains("width: " + expected, body,
                    $".appui-icon--size-{name} must bind its width to {expected}.");
                StringAssert.Contains("height: " + expected, body,
                    $".appui-icon--size-{name} must bind its height to {expected}.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-icon--size-{name} uses a raw --appui-sizing step instead of the alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-icon--size-{name} uses a literal pixel value instead of the alias.");
            }
        }

        IEnumerator MountSizeRamp(List<Icon> icons, string density = "normal")
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
            foreach (var size in AllSizes)
            {
                var icon = new Icon { iconName = "info", size = size };
                row.Add(icon);
                icons.Add(icon);
            }

            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
