using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(IconButton))]
    class IconButtonTests : VisualElementTests<IconButton>
    {
        protected override string mainUssClassName => IconButton.ussClassName;

        // Handed over to IconButtonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:IconButton />",
            @"<appui:IconButton icon=""settings"" selected=""true"" />",
        };

        const string k_ButtonUssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Button.uss";

        [Test]
        public void Selected_TogglesTheSelectedClass()
        {
            var button = new IconButton("settings");
            Assert.IsFalse(button.selected, "An IconButton starts unselected, as in the registry.");

            button.selected = true;
            Assert.IsTrue(button.ClassListContains(Styles.selectedUssClassName));
            Assert.IsTrue(button.selected);

            button.selected = false;
            Assert.IsFalse(button.ClassListContains(Styles.selectedUssClassName));
            Assert.IsFalse(button.selected);
        }

        /// <summary>
        /// Pins the selected fill to the registry's tokens: the selected role's own fill, and its
        /// hover alias under the pointer rather than the variant's hover fill.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Selected_PaintsTheSelectedBackgroundAliases()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_ButtonUssPath));

            string Body(string selector)
            {
                var rule = Regex.Match(uss, "^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}",
                    RegexOptions.Multiline);
                Assert.IsTrue(rule.Success, $"Button.uss has no `{selector}` rule.");
                return rule.Groups["body"].Value;
            }

            StringAssert.Contains(
                "background-color: var(--appui-alias-actions-selected-background-color-default)",
                Body(".appui-button--icon-only.is-selected"));
            StringAssert.Contains(
                "background-color: var(--appui-alias-actions-selected-background-color-hover)",
                Body(".appui-button--icon-only.is-selected.is-hovered"));
        }
    }
}
