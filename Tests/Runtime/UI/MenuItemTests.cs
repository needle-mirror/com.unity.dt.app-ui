using System.Collections;
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
    [TestOf(typeof(MenuItem))]
    class MenuItemTests : VisualElementTests<MenuItem>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Menu.uss";

        protected override string mainUssClassName => MenuItem.ussClassName;

        // Handed over to MenuItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new MenuItem { label = "Open", icon = "folder-open" });
                yield return new Story("Shortcut", ctx => new MenuItem { label = "Copy", icon = "copy", shortcut = "Ctrl+C" });
                yield return new Story("Checkbox", ctx => new MenuItem { label = "Word Wrap", selectable = true, value = true });
                yield return new Story("Radio", ctx => new MenuItem
                {
                    label = "Medium",
                    key = "md",
                    selectable = true,
                    indicator = MenuItemIndicator.Radio,
                    value = true
                });
                yield return new Story("Destructive", ctx => new MenuItem
                {
                    label = "Delete",
                    icon = "trash",
                    variant = MenuItemVariant.Destructive
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:MenuItem />",
            @"<appui:MenuItem label=""Delete"" variant=""Destructive"" />",
            @"<appui:MenuItem label=""Medium"" key=""md"" selectable=""true"" indicator=""Radio"" />",
        };

        [Test]
        public void Variant_DefaultsToDefault_AndSwapsTheUssClass()
        {
            var item = new MenuItem();
            Assert.AreEqual(MenuItemVariant.Default, item.variant);
            Assert.IsTrue(item.ClassListContains(MenuItem.variantUssClassName + "default"),
                "A freshly constructed MenuItem must carry the default variant class.");

            item.variant = MenuItemVariant.Destructive;
            Assert.IsTrue(item.ClassListContains(MenuItem.variantUssClassName + "destructive"));
            Assert.IsFalse(item.ClassListContains(MenuItem.variantUssClassName + "default"),
                "The previous variant class must be removed, not accumulated.");
        }

        [Test]
        public void Indicator_DefaultsToCheckmark_AndSwapsTheUssClass()
        {
            var item = new MenuItem();
            Assert.AreEqual(MenuItemIndicator.Checkmark, item.indicator);
            Assert.IsTrue(item.ClassListContains(MenuItem.indicatorUssClassName + "checkmark"));

            item.indicator = MenuItemIndicator.Radio;
            Assert.IsTrue(item.ClassListContains(MenuItem.indicatorUssClassName + "radio"));
            Assert.IsFalse(item.ClassListContains(MenuItem.indicatorUssClassName + "checkmark"),
                "The previous indicator class must be removed, not accumulated.");
        }

        [Test]
        public void Key_RoundTrips()
        {
            var item = new MenuItem { key = "md" };
            Assert.AreEqual("md", item.key);
        }

        [Test]
        public void RadioIndicator_IsPartOfTheHierarchy()
        {
            var item = new MenuItem();
            Assert.IsNotNull(item.Q(className: MenuItem.radioUssClassName),
                "MenuItem must own a radio bullet element so a radio row can show its selection.");
        }

        /// <summary>
        /// The variant is a colour contract: destructive rows must read the semantic destructive
        /// tokens and never a literal colour.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void DestructiveVariant_BindsToTheSemanticDestructiveTokens()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            var rule = Regex.Match(uss, @"\.appui-menuitem--destructive\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "Menu.uss has no .appui-menuitem--destructive rule.");
            var body = rule.Groups["body"].Value;
            StringAssert.Contains("var(--appui-alias-semantic-destructive)", body,
                "The destructive row must take its colour from --appui-alias-semantic-destructive.");
            Assert.IsFalse(Regex.IsMatch(body, @"#[0-9a-fA-F]{3,8}\b"),
                ".appui-menuitem--destructive uses a literal hex colour instead of a token.");
            Assert.IsFalse(Regex.IsMatch(body, @"\brgba?\("),
                ".appui-menuitem--destructive uses a literal rgb()/rgba() colour instead of a token.");
        }

        /// <summary>
        /// The radio bullet is a sizing contract: it must come from the token ramp, never a
        /// literal pixel value, so it rescales with density.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void RadioBullet_IsSizedFromTokens()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var rule = Regex.Match(uss, @"\.appui-menuitem__radio\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "Menu.uss has no .appui-menuitem__radio rule.");
            var body = rule.Groups["body"].Value;
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                ".appui-menuitem__radio uses a literal pixel value instead of a sizing token.");
            StringAssert.Contains("var(--appui-", body,
                ".appui-menuitem__radio must size itself from an --appui-* token.");
        }

        /// <summary>
        /// Only the matching indicator may be displayed, otherwise a radio row shows a checkmark
        /// as well as a bullet.
        /// </summary>
        [UnityTest]
        public IEnumerator RadioIndicator_HidesTheCheckmark()
        {
            var menu = new Menu();
            var item = new MenuItem
            {
                label = "Medium",
                selectable = true,
                indicator = MenuItemIndicator.Radio,
                value = true
            };
            menu.Add(item);
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(menu);

            for (var i = 0; i < 4; i++)
                yield return null;

            var checkmark = item.Q(className: MenuItem.checkmarkUssClassName);
            var bullet = item.Q(className: MenuItem.radioUssClassName);
            Assert.IsNotNull(checkmark);
            Assert.IsNotNull(bullet);
            Assert.AreEqual(DisplayStyle.None, checkmark.resolvedStyle.display,
                "A radio row must not display the checkmark glyph.");
            Assert.AreEqual(DisplayStyle.Flex, bullet.resolvedStyle.display,
                "A radio row must display its bullet.");
        }
    }
}
