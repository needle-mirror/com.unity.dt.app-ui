using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SplitButton))]
    class SplitButtonTests : VisualElementTests<SplitButton>
    {
        protected override string mainUssClassName => SplitButton.ussClassName;

        // Handed over to SplitButtonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new SplitButton { label = "Play" });
                yield return new Story("WithIcon", (ctx) => new SplitButton { label = "Play", icon = "play" });
                yield return new Story("IconOnly", (ctx) => new SplitButton { icon = "play" });
                yield return new Story("Selected", (ctx) => new SplitButton { label = "Mute", selected = true });
                yield return new Story("Small", (ctx) => new SplitButton { label = "Play", size = Size.Sm });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:SplitButton />",
            @"<appui:SplitButton label=""Play"" icon=""play"" size=""Lg"" selected=""true"" caret-tooltip=""Play options"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/SplitButton.uss";

        [Test]
        public void TheTwoPartsAreSeparatePressTargets()
        {
            var button = new SplitButton();

            Assert.IsNotNull(button.mainElement);
            Assert.IsNotNull(button.caretElement);
            Assert.AreNotSame(button.mainElement, button.caretElement);
            Assert.AreSame(button.mainElement, button.clickable.target,
                "The main part's Pressable must be on the main part.");
            Assert.AreSame(button.caretElement, button.caretClickable.target,
                "The caret's Pressable must be on the caret, or a click on the caret would run the action.");
        }

        /// <summary>
        /// The caret is the anchor a caller hangs a menu off, so it has to be pickable and focusable in
        /// its own right rather than a decorative glyph inside the main part.
        /// </summary>
        [Test]
        public void TheCaretIsItsOwnFocusableTarget()
        {
            var button = new SplitButton();

            Assert.AreEqual(PickingMode.Position, button.caretElement.pickingMode);
            Assert.IsTrue(button.caretElement.focusable);
            Assert.AreEqual(PickingMode.Ignore, button.caretIconElement.pickingMode,
                "The glyph must not eat the caret's own pointer events.");
        }

        [Test]
        public void ClickedAndCaretClicked_AreSeparateEvents()
        {
            var button = new SplitButton();
            var main = 0;
            var caret = 0;
            button.clicked += () => main++;
            button.caretClicked += () => caret++;

            button.clickable.InvokePressed(null);
            Assert.AreEqual(1, main);
            Assert.AreEqual(0, caret, "Running the action must not also open the options.");

            button.caretClickable.InvokePressed(null);
            Assert.AreEqual(1, main, "Opening the options must not also run the action.");
            Assert.AreEqual(1, caret);
        }

        /// <summary>
        /// The selected fill belongs to the main part: a pressed toggle must not light the caret, which
        /// offers options and carries none of the state.
        /// </summary>
        [Test]
        public void Selected_MarksTheMainPartOnly()
        {
            var button = new SplitButton();
            Assert.IsFalse(button.selected);

            button.selected = true;
            Assert.IsTrue(button.mainElement.ClassListContains(Styles.selectedUssClassName));
            Assert.IsFalse(button.caretElement.ClassListContains(Styles.selectedUssClassName));
            Assert.IsFalse(button.ClassListContains(Styles.selectedUssClassName));

            button.selected = false;
            Assert.IsFalse(button.mainElement.ClassListContains(Styles.selectedUssClassName));
        }

        [Test]
        public void Size_AppliesOneSizeClassAtATime()
        {
            var button = new SplitButton { size = Size.Lg };
            Assert.AreEqual("appui-split-button--size-lg",
                button.GetClasses().Single(c => c.StartsWith(SplitButton.sizeUssClassName)));

            button.size = Size.Sm;
            CollectionAssert.AreEqual(
                new[] { "appui-split-button--size-sm" },
                button.GetClasses().Where(c => c.StartsWith(SplitButton.sizeUssClassName)).ToList());
        }

        [Test]
        public void IconAndLabel_DriveTheLayoutClasses()
        {
            var button = new SplitButton();
            Assert.IsFalse(button.ClassListContains(SplitButton.iconOnlyUssClassName));
            Assert.IsFalse(button.ClassListContains(SplitButton.iconAndLabelUssClassName));
            Assert.IsTrue(button.iconElement.ClassListContains(Styles.hiddenUssClassName));

            button.icon = "play";
            Assert.IsTrue(button.ClassListContains(SplitButton.iconOnlyUssClassName),
                "A glyph with no label is the square icon-only main part.");
            Assert.IsFalse(button.iconElement.ClassListContains(Styles.hiddenUssClassName));

            button.label = "Play";
            Assert.IsFalse(button.ClassListContains(SplitButton.iconOnlyUssClassName));
            Assert.IsTrue(button.ClassListContains(SplitButton.iconAndLabelUssClassName),
                "USS has no adjacent-sibling selector, so the gap between glyph and label is a class.");

            button.icon = null;
            Assert.IsFalse(button.ClassListContains(SplitButton.iconAndLabelUssClassName));
            Assert.IsTrue(button.iconElement.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void CaretTooltip_IsWhatNamesTheCaret()
        {
            var button = new SplitButton { caretTooltip = "Play options" };
            Assert.AreEqual("Play options", button.caretElement.tooltip);
        }

        /// <summary>
        /// Heights come from the control-sizing aliases. A raw <c>--appui-sizing-*</c> step renders the
        /// control one size off and nothing errors, which is exactly the bug no layout assertion can
        /// see: the token and the literal resolve to the same pixels.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_SizesFromTheControlSizingAliases()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));

            foreach (var step in new[] { "sm", "md", "lg" })
            {
                var rule = Regex.Match(uss, @"\.appui-split-button--size-" + step + @"\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success, $"SplitButton.uss has no rule for the {step} size.");
                StringAssert.Contains($"var(--appui-alias-control-sizing-{step}-height)", rule.Groups["body"].Value,
                    $"The {step} height must come from the control-sizing alias.");
            }

            // Gaps may read a raw sizing step (the registry's gap-2 is --appui-sizing-160); sizes may not.
            Assert.IsFalse(Regex.IsMatch(uss, @"(?<![-\w])(height|width)\s*:\s*var\(--appui-sizing-\d+\)"),
                "SplitButton.uss reaches for a raw sizing step instead of a control-sizing alias.");
        }

        /// <summary>
        /// The registry's main part reads <c>--appui-alias-actions-compact-padding-horizontal-md</c>
        /// at every size (#1418): it has never followed the split button's size, and a size-matched
        /// inset would move sm and lg. So no size rule may set the main part's inset.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_MainPartInset_IsTheCompactActionsMdInset_AtEverySize()
        {
            var uss = UssSource.Read("SplitButton.uss");

            // The main part's own rule, not the one it shares with the caret.
            var own = Regex.Match(uss, @"(?m)^\.appui-split-button__main\s*\{([^}]*)\}");
            Assert.IsTrue(own.Success, "Expected a rule of the main part's own.");
            const string token = "--appui-alias-actions-compact-padding-horizontal-md";
            StringAssert.Contains($"padding-left: var({token});", own.Groups[1].Value);
            StringAssert.Contains($"padding-right: var({token});", own.Groups[1].Value);

            Assert.IsFalse(Regex.IsMatch(uss,
                    @"\.appui-split-button--size-\w+\s+\.appui-split-button__main\s*\{[^}]*padding"),
                "A size rule must not override the main part's inset.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensOnly_NoLiteralColours()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "SplitButton.uss must not contain a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "SplitButton.uss must not contain a literal rgb()/rgba() colour.");
        }

        /// <summary>
        /// An unsupported selector does not fail its own rule, it fails the whole stylesheet, and the
        /// component then renders unstyled with no error to point at.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_AvoidsSelectorsUssDoesNotSupport()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(uss.Contains(":nth-child"), "USS rejects :nth-child, taking the file with it.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\}\s*[^{}]*[+~]\s*[.*\[]"),
                "USS has no sibling combinators.");
            Assert.IsFalse(Regex.IsMatch(uss, @":[^;{}\r\n]*calc\("), "USS has no calc().");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheManifest()
        {
            var manifest = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("@import \"./Components/SplitButton.uss\";", manifest,
                "A stylesheet missing from App UI.uss does nothing, silently.");
        }
    }
}
