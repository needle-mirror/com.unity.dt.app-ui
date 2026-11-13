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
    [TestOf(typeof(TreeViewItem))]
    class TreeViewItemTests : VisualElementTests<TreeViewItem>
    {
        protected override string mainUssClassName => TreeViewItem.ussClassName;

        // Handed over to TreeViewItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TreeViewItem { label = "Item" });
                yield return new Story("WithIcon", (ctx) => new TreeViewItem { label = "Item", icon = "folder" });
                yield return new Story("CaretEnd", (ctx) => new TreeViewItem
                {
                    label = "Item",
                    showCaret = true,
                    caretPosition = CaretPosition.End
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TreeViewItem />",
            @"<appui:TreeViewItem label=""A"" icon=""folder"" />",
            @"<appui:TreeViewItem label=""A"" caret-position=""End"" draggable=""false"" />",
            @"<appui:TreeViewItem label=""A"" trailing-visible=""true"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/TreeViewItem.uss";

        IEnumerator Mount(VisualElement element)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.3f);
        }

        static VisualElement Caret(TreeViewItem item) =>
            item.Q(className: TreeViewItem.caretButtonUssClassName);

        static VisualElement Gutter(TreeViewItem item) =>
            item.Q(className: TreeViewItem.caretGutterUssClassName);

        static VisualElement Header(TreeViewItem item) =>
            item.Q(className: TreeViewItem.headerUssClassName);

        // ── Caret position ──────────────────────────────────────────────────

        [Test]
        public void DefaultCaretPosition_IsStart()
        {
            var item = new TreeViewItem();
            Assert.AreEqual(CaretPosition.Start, item.caretPosition);
            Assert.IsTrue(item.ClassListContains(TreeViewItem.caretPositionUssClassName + "start"));
        }

        [Test]
        [TestCase(CaretPosition.Start, ExpectedResult = "appui-tree-view-item--caret-start")]
        [TestCase(CaretPosition.End, ExpectedResult = "appui-tree-view-item--caret-end")]
        public string CaretPosition_AppliesTheMatchingUssClass(CaretPosition position)
        {
            var item = new TreeViewItem { caretPosition = position };
            return item.GetClasses().Single(c => c.StartsWith(TreeViewItem.caretPositionUssClassName));
        }

        [Test]
        public void CaretPosition_RemovesThePreviousUssClass()
        {
            var item = new TreeViewItem { caretPosition = CaretPosition.End };
            item.caretPosition = CaretPosition.Start;
            CollectionAssert.AreEqual(
                new[] { "appui-tree-view-item--caret-start" },
                item.GetClasses().Where(c => c.StartsWith(TreeViewItem.caretPositionUssClassName)).ToList());
        }

        /// <summary>
        /// Start puts the caret inside the always-reserved leading gutter; End takes it out of the
        /// gutter and drops it right after the label. The gutter itself never leaves, so rows keep
        /// their icons column-aligned either way.
        /// </summary>
        [Test]
        public void CaretPosition_MovesTheCaretButtonBetweenTheGutterAndAfterTheLabel()
        {
            var item = new TreeViewItem { label = "a", showCaret = true };
            var gutter = Gutter(item);
            var header = Header(item);
            var caret = Caret(item);
            var labelEl = item.labelElement;

            Assert.IsNotNull(gutter, "The leading caret gutter must always exist.");
            Assert.AreSame(gutter, caret.hierarchy.parent, "A start caret must live inside the gutter.");

            item.caretPosition = CaretPosition.End;
            Assert.AreSame(header, caret.hierarchy.parent, "An end caret must be a direct header child.");
            Assert.Greater(header.IndexOf(caret), header.IndexOf(labelEl),
                "An end caret must come after the label.");
            Assert.IsNotNull(Gutter(item), "The leading gutter must stay reserved with an end caret.");

            item.caretPosition = CaretPosition.Start;
            Assert.AreSame(gutter, Caret(item).hierarchy.parent, "The caret must return to the gutter.");
        }

        [UnityTest]
        public IEnumerator CaretGutter_ReservesWidth_EvenOnALeafRow()
        {
            var leaf = new TreeViewItem { label = "leaf" };
            var tree = new TreeList();
            tree.Add(leaf);
            yield return Mount(tree);

            Assert.Greater(Gutter(leaf).resolvedStyle.width, 0f,
                "The caret gutter collapsed on a leaf row, so nested icons would not line up.");
        }

        // ── Icon slot ───────────────────────────────────────────────────────

        [Test]
        public void Icon_IsAbsentByDefault()
        {
            var item = new TreeViewItem();
            Assert.IsTrue(string.IsNullOrEmpty(item.icon));
            Assert.IsFalse(item.ClassListContains(TreeViewItem.withIconVariantUssClassName));
            Assert.IsTrue(item.iconElement.ClassListContains(Styles.hiddenUssClassName),
                "An unset icon must be hidden, not an empty box in the leading column.");
        }

        [Test]
        public void Icon_SetsTheIconName_AndFlagsTheRow()
        {
            var item = new TreeViewItem { icon = "folder" };
            Assert.AreEqual("folder", item.icon);
            Assert.AreEqual("folder", item.iconElement.iconName);
            Assert.IsTrue(item.ClassListContains(TreeViewItem.withIconVariantUssClassName));
            Assert.IsFalse(item.iconElement.ClassListContains(Styles.hiddenUssClassName));

            item.icon = null;
            Assert.IsFalse(item.ClassListContains(TreeViewItem.withIconVariantUssClassName),
                "Clearing the icon must drop the with-icon class.");
            Assert.IsTrue(item.iconElement.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void LabelElement_IsExposed_SoCallersCanStyleTheLabel()
        {
            var item = new TreeViewItem { label = "a" };
            Assert.IsNotNull(item.labelElement);
            Assert.AreEqual("a", item.labelElement.text);

            item.labelElement.AddToClassList("my-accent");
            Assert.IsTrue(item.labelElement.ClassListContains("my-accent"));
        }

        // ── Drag flags ──────────────────────────────────────────────────────

        [Test]
        public void Draggable_DefaultsToTrue_AndCanBeTurnedOff()
        {
            var item = new TreeViewItem();
            Assert.IsTrue(item.draggable);
            item.draggable = false;
            Assert.IsFalse(item.draggable);
        }

        [Test]
        public void DropPosition_TogglesTheMatchingUssClass_AndClearsThePrevious()
        {
            var item = new TreeViewItem();
            Assert.IsNull(item.dropPosition);

            item.dropPosition = DropPosition.Before;
            Assert.IsTrue(item.ClassListContains("appui-tree-view-item--drop-before"));

            item.dropPosition = DropPosition.Inside;
            Assert.IsFalse(item.ClassListContains("appui-tree-view-item--drop-before"),
                "The previous drop-position class must be removed, not accumulated.");
            Assert.IsTrue(item.ClassListContains("appui-tree-view-item--drop-inside"));

            item.dropPosition = null;
            CollectionAssert.IsEmpty(
                item.GetClasses().Where(c => c.StartsWith(TreeViewItem.dropPositionUssClassName)).ToList());
        }

        [Test]
        public void Dragging_TogglesTheDraggingUssClass()
        {
            var item = new TreeViewItem();
            Assert.IsFalse(item.dragging);
            item.dragging = true;
            Assert.IsTrue(item.ClassListContains(TreeViewItem.draggingVariantUssClassName));
            item.dragging = false;
            Assert.IsFalse(item.ClassListContains(TreeViewItem.draggingVariantUssClassName));
        }

        // ── Trailing slot ───────────────────────────────────────────────────

        [Test]
        public void TrailingContainer_IsEmptyByDefault_AndTheRowIsNotFlagged()
        {
            var item = new TreeViewItem();
            Assert.IsNotNull(item.trailingContainer);
            Assert.AreEqual(0, item.trailingContainer.childCount);
            Assert.IsFalse(item.ClassListContains(TreeViewItem.withTrailingVariantUssClassName));
            Assert.IsFalse(item.trailingCollapsed);
        }

        [Test]
        public void TrailingContainer_FlagsTheRow_OnceItHasContent()
        {
            var item = new TreeViewItem();
            item.trailingContainer.Add(new IconButton { icon = "eye" });
            item.RefreshTrailing();
            Assert.IsTrue(item.ClassListContains(TreeViewItem.withTrailingVariantUssClassName));
        }

        [Test]
        public void TrailingVisible_ShowsTheStrip_WithoutAHover()
        {
            var item = new TreeViewItem();
            item.trailingContainer.Add(new IconButton { icon = "eye" });
            item.RefreshTrailing();
            Assert.IsFalse(item.ClassListContains(TreeViewItem.trailingShownVariantUssClassName),
                "A trailing strip must stay hidden until hovered or explicitly made visible.");

            item.trailingVisible = true;
            Assert.IsTrue(item.ClassListContains(TreeViewItem.trailingShownVariantUssClassName));

            item.trailingVisible = false;
            Assert.IsFalse(item.ClassListContains(TreeViewItem.trailingShownVariantUssClassName));
        }

        /// <summary>
        /// The strip overlays the row, so the only thing keeping the label out from under it is the
        /// margin the row reserves from the measured strip width. No reserve means text running under
        /// the actions.
        /// </summary>
        [UnityTest]
        public IEnumerator Trailing_ReservesTheStripWidth_InTheLabelMargin()
        {
            var item = new TreeViewItem { label = "a fairly long row label", trailingVisible = true };
            item.trailingContainer.Add(new IconButton { icon = "eye" });
            item.RefreshTrailing();

            var tree = new TreeList();
            tree.Add(item);
            tree.style.width = 400f;
            yield return Mount(tree);
            yield return null;

            Assert.Greater(item.labelElement.resolvedStyle.marginRight, 0f,
                "The label reserves no room for the visible trailing strip.");

            item.trailingVisible = false;
            yield return null;
            Assert.AreEqual(0f, item.labelElement.resolvedStyle.marginRight, 0.01f,
                "A hidden trailing strip must not keep reserving room.");
        }

        /// <summary>
        /// The narrow-row overflow: below the token-driven minimum readable label width the inline
        /// strip collapses into the ⋮ button, and it comes back when the row grows again.
        /// </summary>
        [UnityTest]
        public IEnumerator Trailing_CollapsesOnANarrowRow_AndComesBackWhenItGrows()
        {
            var item = new TreeViewItem { label = "a fairly long row label", trailingVisible = true };
            item.trailingContainer.Add(new IconButton { icon = "eye" });
            item.trailingContainer.Add(new IconButton { icon = "lock" });
            item.RefreshTrailing();

            var tree = new TreeList();
            tree.Add(item);
            tree.style.width = 400f;
            yield return Mount(tree);
            yield return null;

            Assert.IsFalse(item.trailingCollapsed, "A wide row must keep its inline trailing strip.");
            Assert.IsFalse(item.ClassListContains(TreeViewItem.trailingCollapsedVariantUssClassName));

            tree.style.width = 60f;
            yield return null;
            yield return null;

            Assert.IsTrue(item.trailingCollapsed,
                "A row too narrow for a readable label next to the actions must collapse to the overflow button.");
            Assert.IsTrue(item.ClassListContains(TreeViewItem.trailingCollapsedVariantUssClassName));

            tree.style.width = 400f;
            yield return null;
            yield return null;

            Assert.IsFalse(item.trailingCollapsed, "The strip must come back once the row is wide enough again.");
        }

        [UnityTest]
        public IEnumerator OverflowButton_MovesTheTrailingContentIntoAPopover_AndBack()
        {
            var toggle = new IconButton { icon = "eye" };
            var item = new TreeViewItem { label = "a fairly long row label", trailingVisible = true };
            item.trailingContainer.Add(toggle);
            item.trailingContainer.Add(new IconButton { icon = "lock" });
            item.RefreshTrailing();

            var tree = new TreeList();
            tree.Add(item);
            tree.style.width = 60f;
            yield return Mount(tree);
            yield return null;
            yield return null;

            Assume.That(item.trailingCollapsed, Is.True, "The row did not collapse, nothing to overflow.");

            item.overflowButton.clickable.InvokePressed(null);
            yield return null;

            Assert.AreNotSame(item.trailingContainer, toggle.hierarchy.parent,
                "Opening the overflow must move the very same controls into the popover, not clone them.");
            Assert.IsTrue(toggle.ClassListContains(IconButton.ussClassName));
        }

        // ── USS contract ────────────────────────────────────────────────────

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColors()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "TreeViewItem.uss contains a literal hex color.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "TreeViewItem.uss contains a literal rgb()/rgba() color.");
        }

        /// <summary>
        /// The drop indicators and the inside-drop ring are design-system surfaces: they have to come
        /// from the accent action token and a border-width token, not from a hand-picked colour or a
        /// literal 2px.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_DropIndicators_BindToTokens()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var rule = Regex.Match(uss,
                @"\.appui-tree-view-item__drop-indicator-before,\s*\.appui-tree-view-item__drop-indicator-after\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "TreeViewItem.uss has no shared drop-indicator rule.");
            var body = rule.Groups["body"].Value;

            StringAssert.Contains("var(--appui-alias-actions-accent-background-color-default)", body,
                "The drop indicator must take its colour from the accent action token.");
            StringAssert.Contains("var(--appui-border-width-", body,
                "The drop indicator thickness must come from a border-width token.");
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                "The drop indicator rule uses a literal pixel value.");
        }

        /// <summary>
        /// The minimum readable label width is a dimension C# needs at runtime, so it goes through a
        /// probe element sized from a density-aware sizing step — never a constant in C#.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_MinLabelProbe_IsSizedFromASizingStep()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var rule = Regex.Match(uss, @"\.appui-tree-view-item__min-label-probe\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "TreeViewItem.uss has no min-label probe rule.");
            var body = rule.Groups["body"].Value;
            Assert.IsTrue(Regex.IsMatch(body, @"width:\s*var\(--appui-sizing-\d+\)"),
                "The min-label probe must take its width from an --appui-sizing step so it follows density.");
        }

        /// <summary>
        /// The caret gutter and the leading icon are glyph-sized boxes, so they bind to the
        /// control-sizing icon aliases rather than a raw sizing step.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_CaretGutterAndIcon_BindToTheControlSizingIconAliases()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));

            foreach (var selector in new[] { "caret-gutter", "icon", "caret-icon" })
            {
                var rule = Regex.Match(uss, @"\.appui-tree-view-item__" + selector + @"\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success, $"TreeViewItem.uss has no .appui-tree-view-item__{selector} rule.");
                var body = rule.Groups["body"].Value;
                StringAssert.Contains("var(--appui-alias-control-sizing-icon-", body,
                    $".appui-tree-view-item__{selector} must size from a control-sizing icon alias.");
                Assert.IsFalse(Regex.IsMatch(body, @"(width|height):\s*\d+(\.\d+)?px"),
                    $".appui-tree-view-item__{selector} uses a literal pixel size.");
            }
        }

        /// <summary>
        /// The trailing strip has to stay laid out while hidden, because the row measures the width it
        /// <i>would</i> take to decide whether to collapse. `display: none` would zero that measurement
        /// and the row could never un-collapse.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_TrailingStrip_HidesWithVisibility_NotDisplay()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var rule = Regex.Match(uss, @"\.appui-tree-view-item__trailing-container\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "TreeViewItem.uss has no trailing container rule.");
            var body = rule.Groups["body"].Value;
            StringAssert.Contains("visibility: hidden", body);
            StringAssert.DoesNotContain("display: none", body,
                "display:none would zero the strip's measured width and break the un-collapse path.");
            StringAssert.Contains("position: absolute", body,
                "The strip overlays the row, so the label box can run to the right edge.");
        }
        // ── Label badge ─────────────────────────────────────────────────────

        [Test]
        public void LabelBadge_IsAbsentByDefault()
        {
            var item = new TreeViewItem { label = "a" };
            Assert.IsTrue(string.IsNullOrEmpty(item.labelBadge));
            Assert.IsFalse(item.ClassListContains(TreeViewItem.withLabelBadgeVariantUssClassName));
            Assert.IsTrue(item.labelBadgeElement.ClassListContains(Styles.hiddenUssClassName),
                "An unset badge must be hidden, not an empty box after the name.");
        }

        /// <summary>
        /// The badge belongs to the name, so it follows the label rather than pinning to the row's end
        /// the way the trailing strip does.
        /// </summary>
        [Test]
        public void LabelBadge_SitsRightAfterTheLabel_NotAtTheRowEnd()
        {
            var item = new TreeViewItem { label = "a", labelBadge = "star" };
            var header = Header(item);

            Assert.AreEqual("star", item.labelBadgeElement.iconName);
            Assert.IsTrue(item.ClassListContains(TreeViewItem.withLabelBadgeVariantUssClassName));
            Assert.IsFalse(item.labelBadgeElement.ClassListContains(Styles.hiddenUssClassName));
            Assert.AreEqual(header.IndexOf(item.labelElement) + 1, header.IndexOf(item.labelBadgeElement),
                "The badge must be the label's immediate neighbour on the name line.");
            Assert.Less(header.IndexOf(item.labelBadgeElement),
                header.IndexOf(item.Q(className: TreeViewItem.trailingContainerUssClassName)),
                "The badge must come before the trailing strip.");

            item.labelBadge = null;
            Assert.IsFalse(item.ClassListContains(TreeViewItem.withLabelBadgeVariantUssClassName));
            Assert.IsTrue(item.labelBadgeElement.ClassListContains(Styles.hiddenUssClassName));
        }

        /// <summary>
        /// A glyph says nothing to someone who has not seen it before, and the badge is too small to
        /// label in place, so the words go in its tooltip. The badge has to be pickable for one.
        /// </summary>
        [Test]
        public void LabelBadgeLabel_IsTheBadgesTooltip_AndMakesItHittable()
        {
            var item = new TreeViewItem { label = "Main", labelBadge = "star" };
            Assert.AreEqual(PickingMode.Ignore, item.labelBadgeElement.pickingMode,
                "With nothing to say, the badge must not swallow the row's pointer events.");

            item.labelBadgeLabel = "Startup scene";
            Assert.AreEqual("Startup scene", item.labelBadgeElement.tooltip);
            Assert.AreEqual(PickingMode.Position, item.labelBadgeElement.pickingMode);

            item.labelBadgeLabel = null;
            Assert.AreEqual(PickingMode.Ignore, item.labelBadgeElement.pickingMode);
        }

        /// <summary>
        /// The reserve that keeps text clear of the trailing strip has to move to whichever of the two
        /// is last on the name line: left on the label, the margin would push the badge under the strip,
        /// which is the one thing the reserve exists to prevent.
        /// </summary>
        [UnityTest]
        public IEnumerator LabelBadge_TakesOverTheTrailingReserve()
        {
            var item = new TreeViewItem { label = "a long row name", trailingVisible = true };
            item.trailingContainer.Add(new IconButton("dots-three-vertical"));
            item.labelBadge = "star";
            item.RefreshTrailing();
            yield return Mount(item);

            Assert.AreEqual(0f, item.labelElement.resolvedStyle.marginRight,
                "With a badge on the line, the label must not carry the reserve.");
            Assert.Greater(item.labelBadgeElement.resolvedStyle.marginRight, 0f,
                "The badge must carry the reserve so it stops short of the trailing strip.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_BadgedRow_StopsTheLabelFromGrowing()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            var rule = Regex.Match(uss,
                @"\.appui-tree-view-item--with-label-badge[^{]*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success,
                "Without this rule the label fills the row and carries the badge out to the trailing edge.");
            StringAssert.Contains("flex-grow: 0", rule.Groups["body"].Value);
        }

    }
}
