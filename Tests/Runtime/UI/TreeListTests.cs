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
    [TestOf(typeof(TreeList))]
    class TreeListTests : VisualElementTests<TreeList>
    {
        protected override string mainUssClassName => TreeList.ussClassName;

        // Handed over to TreeListSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => MakeTree(out _));
                yield return new Story("Multiple", MultipleSelection);
            }
        }

        /// <summary>
        /// Two items actually selected under <see cref="SelectionType.Multiple"/>.
        /// </summary>
        /// <remarks>
        /// <c>selectionType</c> on its own has no resting visual representation, so a story that
        /// only sets it is pixel-identical to <c>Default</c>.
        /// </remarks>
        static IEnumerator MultipleSelection(StoryContext ctx)
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            ctx.container.Add(tree);

            for (var i = 0; i < 3; i++)
                yield return null;

            // a and a1, both visible in the canonical fixture tree.
            tree.SetSelectionWithoutNotify(new[] { items[0], items[1] });
            yield return null;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TreeList />",
            @"<appui:TreeList size=""Sm"" />",
            @"<appui:TreeList selection-type=""Multiple"" allow-no-selection=""false"" />",
            @"<appui:TreeList><appui:TreeViewItem label=""A"" /></appui:TreeList>",
        };

        [UnityTest]
        public IEnumerator MultipleStory_ActuallySelects()
        {
            yield return RunStory("Multiple", ctx =>
            {
                var tree = ctx.container.Q<TreeList>();
                Assert.IsNotNull(tree);
                Assert.AreEqual(SelectionType.Multiple, tree.selectionType);
                Assert.AreEqual(2, tree.selectedItems.Count,
                    "selectionType has no resting visual representation, so without a selection this "
                    + "story is pixel-identical to Default.");
            });
        }

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/TreeList.uss";

        const string k_MainUssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss";

        static readonly Size[] k_AllSizes = { Size.Sm, Size.Md, Size.Lg };

        static readonly string[] k_AllSizeNames = { "sm", "md", "lg" };

        /// <summary>
        /// Builds the canonical fixture tree:
        /// <code>
        /// a           (expanded)
        ///   a1
        ///   a2        (collapsed)
        ///     a2x
        /// b
        /// </code>
        /// Visible order is therefore a, a1, a2, b.
        /// </summary>
        static TreeList MakeTree(out TreeViewItem[] items)
        {
            var a = new TreeViewItem { label = "a", showCaret = true, expanded = true };
            var a1 = new TreeViewItem { label = "a1" };
            var a2 = new TreeViewItem { label = "a2", showCaret = true };
            var a2x = new TreeViewItem { label = "a2x" };
            var b = new TreeViewItem { label = "b" };

            a2.Add(a2x);
            a.Add(a1);
            a.Add(a2);

            var tree = new TreeList();
            tree.Add(a);
            tree.Add(b);

            items = new[] { a, a1, a2, a2x, b };
            return tree;
        }

        IEnumerator Mount(TreeList tree)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(tree);
            yield return null;
            yield return null;
            // Expansion animates the content viewport open over 125ms; geometry assertions and
            // position hit-testing are meaningless until that has settled.
            yield return new WaitForSeconds(0.3f);
        }

        static void Key(TreeList tree, KeyCode code, EventModifiers modifiers = EventModifiers.None)
        {
            using var evt = KeyDownEvent.GetPooled('\0', code, modifiers);
            evt.target = tree;
            tree.SendEvent(evt);
        }

        // ── Size ────────────────────────────────────────────────────────────

        [Test]
        public void DefaultSize_IsMd()
        {
            var tree = new TreeList();
            Assert.AreEqual(Size.Md, tree.size);
            Assert.IsTrue(tree.ClassListContains(TreeList.sizeUssClassName + "md"));
        }

        [Test]
        [TestCase(Size.Sm, ExpectedResult = "appui-tree-list--size-sm")]
        [TestCase(Size.Md, ExpectedResult = "appui-tree-list--size-md")]
        [TestCase(Size.Lg, ExpectedResult = "appui-tree-list--size-lg")]
        public string Size_AppliesTheMatchingUssClass(Size size)
        {
            var tree = new TreeList { size = size };
            return tree.GetClasses().Single(c => c.StartsWith(TreeList.sizeUssClassName));
        }

        [Test]
        public void Size_RemovesThePreviousSizeUssClass()
        {
            var tree = new TreeList { size = Size.Lg };
            tree.size = Size.Sm;
            var classes = tree.GetClasses().Where(c => c.StartsWith(TreeList.sizeUssClassName)).ToList();
            CollectionAssert.AreEqual(new[] { "appui-tree-list--size-sm" }, classes);
        }

        /// <summary>
        /// The container's size class must actually reach the rows: every step has to resolve to a
        /// real, non-zero row height and the ramp must strictly increase. A missing descendant rule
        /// in TreeList.uss shows up here as a flat ramp.
        /// </summary>
        [UnityTest]
        public IEnumerator Size_PropagatesToItemHeaders_AsAStrictlyIncreasingRamp()
        {
            var heights = new float[k_AllSizes.Length];
            for (var i = 0; i < k_AllSizes.Length; i++)
            {
                var tree = MakeTree(out var items);
                tree.size = k_AllSizes[i];
                yield return Mount(tree);
                heights[i] = items[1].Q(className: TreeViewItem.headerUssClassName).resolvedStyle.height;
                Assert.Greater(heights[i], 0f, $"{k_AllSizes[i]} resolved to a zero row height.");
            }

            Assert.Greater(heights[1], heights[0], "Md rows are not taller than Sm rows.");
            Assert.Greater(heights[2], heights[1], "Lg rows are not taller than Md rows.");
        }

        /// <summary>
        /// Pins the row height to the token contract: each size step must bind to
        /// <c>--appui-alias-control-sizing-{sm,md,lg}-height</c>, never to a raw sizing step and
        /// never to a literal pixel value.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            foreach (var name in k_AllSizeNames)
            {
                var rule = Regex.Match(uss,
                    @"\.appui-tree-list--size-" + name + @"[^{]*\.appui-tree-view-item__header\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success,
                    $"TreeList.uss has no .appui-tree-list--size-{name} rule targeting the item header.");
                var body = rule.Groups["body"].Value;
                var expected = "var(--appui-alias-control-sizing-" + name + "-height)";

                StringAssert.Contains("height: " + expected, body,
                    $".appui-tree-list--size-{name} must bind the row height to {expected}.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-tree-list--size-{name} uses a raw --appui-sizing step instead of the alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                    $".appui-tree-list--size-{name} uses a literal pixel value instead of the alias.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColors()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "TreeList.uss contains a literal hex color.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "TreeList.uss contains a literal rgb()/rgba() color.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStyleSheet()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_MainUssPath));
            StringAssert.Contains("./Components/TreeList.uss", uss,
                "TreeList.uss is not imported from App UI.uss, so none of its rules ever apply.");
        }

        // ── Item collection ─────────────────────────────────────────────────

        [Test]
        public void Items_AreCollectedInDocumentOrder()
        {
            var tree = MakeTree(out var items);
            CollectionAssert.AreEqual(
                new[] { "a", "a1", "a2", "a2x", "b" },
                tree.items.Select(i => i.label).ToList());
            Assert.AreSame(items[0], tree.items.First());
        }

        [Test]
        public void VisibleItems_ExcludeItemsInsideACollapsedSubtree()
        {
            var tree = MakeTree(out var items);
            CollectionAssert.AreEqual(
                new[] { "a", "a1", "a2", "b" },
                tree.visibleItems.Select(i => i.label).ToList());

            items[2].expanded = true;
            CollectionAssert.AreEqual(
                new[] { "a", "a1", "a2", "a2x", "b" },
                tree.visibleItems.Select(i => i.label).ToList());

            items[0].expanded = false;
            CollectionAssert.AreEqual(new[] { "a", "b" }, tree.visibleItems.Select(i => i.label).ToList());
        }

        // ── Selection ───────────────────────────────────────────────────────

        [Test]
        public void DefaultSelectionType_IsSingle()
        {
            Assert.AreEqual(SelectionType.Single, new TreeList().selectionType);
            Assert.IsTrue(new TreeList().allowNoSelection);
            CollectionAssert.IsEmpty(new TreeList().selectedItems);
            Assert.IsNull(new TreeList().selectedItem);
        }

        [Test]
        public void SetSelection_MarksTheItemAsSelected()
        {
            var tree = MakeTree(out var items);
            tree.SetSelection(items[1]);
            CollectionAssert.AreEqual(new[] { items[1] }, tree.selectedItems);
            Assert.AreSame(items[1], tree.selectedItem);
            Assert.IsTrue(items[1].selected);
        }

        [Test]
        public void SetSelection_ClearsThePreviousItemSelectedFlag()
        {
            var tree = MakeTree(out var items);
            tree.SetSelection(items[1]);
            tree.SetSelection(items[4]);
            Assert.IsFalse(items[1].selected, "The previously selected item kept its selected state.");
            Assert.IsTrue(items[4].selected);
            CollectionAssert.AreEqual(new[] { items[4] }, tree.selectedItems);
        }

        [Test]
        public void SetSelection_InSingleMode_KeepsOnlyTheLastItem()
        {
            var tree = MakeTree(out var items);
            tree.SetSelection(new[] { items[0], items[1], items[4] });
            CollectionAssert.AreEqual(new[] { items[4] }, tree.selectedItems);
        }

        [Test]
        public void SetSelection_InMultipleMode_KeepsEveryItem()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.SetSelection(new[] { items[0], items[1], items[4] });
            CollectionAssert.AreEqual(new[] { items[0], items[1], items[4] }, tree.selectedItems);
            Assert.AreSame(items[0], tree.selectedItem);
        }

        [Test]
        public void SelectionType_None_RejectsAnySelection()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.None;
            tree.SetSelection(items[1]);
            CollectionAssert.IsEmpty(tree.selectedItems);
            Assert.IsFalse(items[1].selected);
        }

        [Test]
        public void SelectionType_DowngradeToSingle_TrimsTheSelection()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.SetSelection(new[] { items[0], items[1] });
            tree.selectionType = SelectionType.Single;
            Assert.AreEqual(1, tree.selectedItems.Count);
            Assert.IsFalse(items[1].selected);
        }

        [Test]
        public void AllowNoSelection_False_BlocksClearingTheSelection()
        {
            var tree = MakeTree(out var items);
            tree.allowNoSelection = false;
            tree.SetSelection(items[1]);
            tree.ClearSelection();
            CollectionAssert.AreEqual(new[] { items[1] }, tree.selectedItems);

            tree.allowNoSelection = true;
            tree.ClearSelection();
            CollectionAssert.IsEmpty(tree.selectedItems);
            Assert.IsFalse(items[1].selected);
        }

        [Test]
        public void RemovingTheSelectedItem_ClearsItsFlag_AndNotifies()
        {
            var tree = MakeTree(out var items);
            var calls = 0;
            tree.SetSelection(items[4]);
            tree.selectionChanged += _ => calls++;

            tree.Remove(items[4]);

            CollectionAssert.IsEmpty(tree.selectedItems);
            Assert.IsFalse(items[4].selected,
                "A detached row keeps its selected styling and comes back highlighted when reused.");
            Assert.AreEqual(1, calls, "Losing the selection to a removal is a selection change.");
        }

        [Test]
        public void RemovingTheSelectedItem_FallsBackToARow_WhenNoSelectionIsNotAllowed()
        {
            var tree = MakeTree(out var items);
            tree.allowNoSelection = false;
            tree.SetSelection(items[4]);

            tree.Remove(items[4]);

            Assert.AreEqual(1, tree.selectedItems.Count,
                "allowNoSelection false must never report an empty selection, removal included.");
            Assert.IsFalse(items[4].selected);
            Assert.IsTrue(tree.selectedItem.selected);
        }

        [Test]
        public void SelectionChanged_FiresOncePerChange_AndNotWhenUnchanged()
        {
            var tree = MakeTree(out var items);
            var calls = 0;
            IEnumerable<TreeViewItem> last = null;
            tree.selectionChanged += s => { calls++; last = s; };

            tree.SetSelection(items[1]);
            Assert.AreEqual(1, calls);
            CollectionAssert.AreEqual(new[] { items[1] }, last.ToList());

            tree.SetSelection(items[1]);
            Assert.AreEqual(1, calls, "Re-selecting the same item must not raise selectionChanged.");

            tree.SetSelectionWithoutNotify(new[] { items[4] });
            Assert.AreEqual(1, calls, "SetSelectionWithoutNotify must not raise selectionChanged.");
            Assert.IsTrue(items[4].selected);
        }

        [Test]
        public void SelectAll_SelectsEveryVisibleItem_InMultipleModeOnly()
        {
            var tree = MakeTree(out _);
            tree.SelectAll();
            CollectionAssert.IsEmpty(tree.selectedItems, "SelectAll must be a no-op in Single mode.");

            tree.selectionType = SelectionType.Multiple;
            tree.SelectAll();
            CollectionAssert.AreEqual(
                new[] { "a", "a1", "a2", "b" },
                tree.selectedItems.Select(i => i.label).ToList());
        }

        [Test]
        public void Selection_DropsItemsRemovedFromTheTree()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.SetSelection(new[] { items[1], items[4] });
            items[4].RemoveFromHierarchy();
            CollectionAssert.AreEqual(new[] { items[1] }, tree.selectedItems);
        }

        // ── Select() modifiers ──────────────────────────────────────────────

        [Test]
        public void Select_PlainClick_ReplacesTheSelection()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.Select(items[0]);
            tree.Select(items[4]);
            CollectionAssert.AreEqual(new[] { items[4] }, tree.selectedItems);
        }

        [Test]
        public void Select_Toggle_AddsAndRemoves_AndPutsTheClickedItemFirst()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.Select(items[0]);
            tree.Select(items[4], toggle: true);
            CollectionAssert.AreEqual(new[] { items[4], items[0] }, tree.selectedItems);
            Assert.AreSame(items[4], tree.selectedItem, "The last toggled item must become the primary.");

            tree.Select(items[4], toggle: true);
            CollectionAssert.AreEqual(new[] { items[0] }, tree.selectedItems);
        }

        [Test]
        public void Select_Toggle_IsIgnoredInSingleMode()
        {
            var tree = MakeTree(out var items);
            tree.Select(items[0]);
            tree.Select(items[4], toggle: true);
            CollectionAssert.AreEqual(new[] { items[4] }, tree.selectedItems);
        }

        [Test]
        public void Select_Shift_SelectsTheVisibleRangeFromTheAnchor_ClickedItemFirst()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.Select(items[0]);                      // anchor = a
            tree.Select(items[2], shift: true);         // a..a2 over visible order

            CollectionAssert.AreEqual(
                new[] { "a2", "a", "a1" },
                tree.selectedItems.Select(i => i.label).ToList());
        }

        [Test]
        public void Select_Shift_KeepsTheAnchorAcrossRepeatedShiftClicks()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.Select(items[1]);                      // anchor = a1
            tree.Select(items[4], shift: true);         // a1, a2, b
            tree.Select(items[0], shift: true);         // recomputed from a1 -> a, a1

            CollectionAssert.AreEquivalent(
                new[] { "a", "a1" },
                tree.selectedItems.Select(i => i.label).ToList());
        }

        [Test]
        public void Select_ToggleShift_UnionsTheRangeWithTheCurrentSelection()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.Select(items[4]);                      // b, anchor = b
            tree.Select(items[0], toggle: true);        // a + b, anchor = a
            tree.Select(items[1], shift: true, toggle: true); // range a..a1 unioned with a, b

            CollectionAssert.AreEquivalent(
                new[] { "a", "a1", "b" },
                tree.selectedItems.Select(i => i.label).ToList());
        }

        // ── Expansion ───────────────────────────────────────────────────────

        [Test]
        public void ExpandAll_And_CollapseAll_ReachEveryItemWithChildren()
        {
            var tree = MakeTree(out var items);
            tree.ExpandAll();
            Assert.IsTrue(items[0].expanded);
            Assert.IsTrue(items[2].expanded);

            tree.CollapseAll();
            Assert.IsFalse(items[0].expanded);
            Assert.IsFalse(items[2].expanded);
        }

        [UnityTest]
        public IEnumerator ClickingTheCaret_TogglesExpansion_WithoutSelecting()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            var caret = items[2].Q(className: TreeViewItem.caretButtonUssClassName);
            Assert.IsNotNull(caret);

            // Pointer events are dispatched by position, so the event has to actually land on the
            // caret — setting evt.target alone is not enough.
            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = caret.worldBound.center
                   }))
            {
                evt.target = caret;
                caret.SendEvent(evt);
            }
            yield return null;

            Assert.IsTrue(items[2].expanded, "The caret click did not expand the item.");
            CollectionAssert.IsEmpty(tree.selectedItems, "A caret click must not change the selection.");
        }

        [UnityTest]
        public IEnumerator ClickingTheHeader_SelectsTheItem()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            var header = items[1].Q(className: TreeViewItem.headerUssClassName);
            using (var evt = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = header.worldBound.center
                   }))
            {
                evt.target = header;
                header.SendEvent(evt);
            }
            yield return null;

            CollectionAssert.AreEqual(new[] { items[1] }, tree.selectedItems);
        }

        // ── Keyboard ────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator ArrowDownAndUp_MoveTheSelectionOverVisibleItems()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            Key(tree, KeyCode.DownArrow);
            Assert.AreSame(items[0], tree.selectedItem, "The first ArrowDown must select the first row.");

            Key(tree, KeyCode.DownArrow);
            Assert.AreSame(items[1], tree.selectedItem);

            Key(tree, KeyCode.DownArrow);
            Key(tree, KeyCode.DownArrow);
            Assert.AreSame(items[4], tree.selectedItem, "ArrowDown must skip the collapsed subtree.");

            Key(tree, KeyCode.DownArrow);
            Assert.AreSame(items[4], tree.selectedItem, "ArrowDown must clamp at the last row.");

            Key(tree, KeyCode.UpArrow);
            Assert.AreSame(items[2], tree.selectedItem);
        }

        [UnityTest]
        public IEnumerator HomeAndEnd_JumpToTheExtremes()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            Key(tree, KeyCode.End);
            Assert.AreSame(items[4], tree.selectedItem);
            Key(tree, KeyCode.Home);
            Assert.AreSame(items[0], tree.selectedItem);
        }

        [UnityTest]
        public IEnumerator ArrowRight_ExpandsThenStepsIntoTheFirstChild()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            tree.SetSelection(items[2]);            // a2, collapsed, has children
            Key(tree, KeyCode.RightArrow);
            Assert.IsTrue(items[2].expanded, "ArrowRight on a collapsed row must expand it.");
            Assert.AreSame(items[2], tree.selectedItem, "Expanding must not move the selection.");

            Key(tree, KeyCode.RightArrow);
            Assert.AreSame(items[3], tree.selectedItem, "ArrowRight on an expanded row must step into it.");
        }

        [UnityTest]
        public IEnumerator ArrowLeft_CollapsesThenJumpsToTheParent()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            tree.SetSelection(items[0]);            // a, expanded
            Key(tree, KeyCode.LeftArrow);
            Assert.IsFalse(items[0].expanded, "ArrowLeft on an expanded row must collapse it.");

            tree.SetSelection(items[1]);            // a1, a leaf inside a
            Key(tree, KeyCode.LeftArrow);
            Assert.AreSame(items[0], tree.selectedItem, "ArrowLeft on a leaf must jump to its parent.");
        }

        [UnityTest]
        public IEnumerator ShiftArrowDown_ExtendsTheRange_InMultipleModeOnly()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            yield return Mount(tree);

            tree.Select(items[0]);
            Key(tree, KeyCode.DownArrow, EventModifiers.Shift);
            CollectionAssert.AreEquivalent(new[] { "a", "a1" }, tree.selectedItems.Select(i => i.label).ToList());

            tree.selectionType = SelectionType.Single;
            tree.Select(items[0]);
            Key(tree, KeyCode.DownArrow, EventModifiers.Shift);
            CollectionAssert.AreEqual(new[] { items[1] }, tree.selectedItems);
        }

        [UnityTest]
        public IEnumerator ActionKeyA_SelectsEveryVisibleItem_AndEscapeClears()
        {
            var tree = MakeTree(out _);
            tree.selectionType = SelectionType.Multiple;
            yield return Mount(tree);

            Key(tree, KeyCode.A, Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
                ? EventModifiers.Command
                : EventModifiers.Control);
            CollectionAssert.AreEquivalent(
                new[] { "a", "a1", "a2", "b" },
                tree.selectedItems.Select(i => i.label).ToList());

            Key(tree, KeyCode.Escape);
            CollectionAssert.IsEmpty(tree.selectedItems);
        }

        // ── Delete / Backspace ──────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Delete_InvokesOnDeleteSelection_WithTheSelectionInPriorityOrder()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            yield return Mount(tree);

            List<TreeViewItem> received = null;
            var calls = 0;
            tree.onDeleteSelection = s => { calls++; received = s.ToList(); };

            tree.Select(items[1]);
            tree.Select(items[4], toggle: true);
            Key(tree, KeyCode.Delete);

            Assert.AreEqual(1, calls);
            CollectionAssert.AreEqual(new[] { items[4], items[1] }, received,
                "onDeleteSelection must receive the selection in priority order, primary first.");
        }

        [UnityTest]
        public IEnumerator Backspace_AlsoInvokesOnDeleteSelection()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            var calls = 0;
            tree.onDeleteSelection = _ => calls++;
            tree.SetSelection(items[1]);
            Key(tree, KeyCode.Backspace);

            Assert.AreEqual(1, calls);
        }

        [UnityTest]
        public IEnumerator Delete_DoesNothing_WithoutAHandlerOrWithAnEmptySelection()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            // No handler at all: must not throw, and must not remove anything.
            Key(tree, KeyCode.Delete);
            Assert.AreEqual(5, tree.items.Count, "The tree must never delete a row on its own.");

            var calls = 0;
            tree.onDeleteSelection = _ => calls++;
            Key(tree, KeyCode.Delete);
            Assert.AreEqual(0, calls, "An empty selection must not invoke onDeleteSelection.");

            tree.SetSelection(items[1]);
            Key(tree, KeyCode.Delete);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(5, tree.items.Count, "onDeleteSelection must not imply a default removal.");
        }

        // ── Drag to reorder ─────────────────────────────────────────────────

        /// <summary>
        /// A point inside a row's header, <paramref name="t"/> of the way down it.
        /// </summary>
        static Vector2 RowPoint(TreeViewItem item, float t)
        {
            var rect = item.header.worldBound;
            return new Vector2(rect.center.x, rect.yMin + rect.height * t);
        }

        [UnityTest]
        public IEnumerator BeginDrag_IsANoOp_WithoutAnOnReparentHandler()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            tree.BeginDrag(items[1]);

            Assert.IsNull(tree.draggedItem, "Drag and drop must stay off until onReparent is set.");
            Assert.IsNull(tree.dragPayload);
            Assert.IsFalse(items[1].dragging);
        }

        [UnityTest]
        public IEnumerator BeginDrag_IsANoOp_ForANonDraggableRow()
        {
            var tree = MakeTree(out var items);
            tree.onReparent = (_, _, _) => { };
            items[1].draggable = false;
            yield return Mount(tree);

            tree.BeginDrag(items[1]);
            Assert.IsNull(tree.draggedItem);

            tree.BeginDrag(items[4]);
            Assert.AreSame(items[4], tree.draggedItem, "A draggable sibling must still be pickable.");
        }

        [UnityTest]
        public IEnumerator BeginDrag_FlagsTheRow_AndPublishesATypedPayload()
        {
            var tree = MakeTree(out var items);
            tree.onReparent = (_, _, _) => { };
            yield return Mount(tree);

            tree.BeginDrag(items[1]);

            Assert.AreSame(items[1], tree.draggedItem);
            Assert.IsTrue(items[1].dragging, "The dragged row must carry the dragging class.");
            Assert.IsTrue(tree.ClassListContains(TreeList.draggingUssClassName));
            Assert.IsNotNull(tree.dragPayload);
            Assert.AreEqual(TreeList.treeItemMimeType, tree.dragPayload.mimeType);
            Assert.AreEqual("application/x-appui-tree-item", tree.dragPayload.mimeType);
            CollectionAssert.AreEqual(new[] { items[1] }, tree.dragPayload.items.ToList());
        }

        [UnityTest]
        public IEnumerator DragPayload_CarriesTheWholeSelection_WhenTheGrabbedRowIsPartOfIt()
        {
            var tree = MakeTree(out var items);
            tree.selectionType = SelectionType.Multiple;
            tree.onReparent = (_, _, _) => { };
            yield return Mount(tree);

            tree.SetSelection(new[] { items[1], items[4] });
            tree.BeginDrag(items[1]);
            CollectionAssert.AreEquivalent(new[] { items[1], items[4] }, tree.dragPayload.items.ToList());

            tree.CancelDrag();
            tree.BeginDrag(items[0]);
            CollectionAssert.AreEqual(new[] { items[0] }, tree.dragPayload.items.ToList(),
                "Grabbing a row outside the selection must drag only that row.");
        }

        [Test]
        public void CanDrop_RejectsSelfAndOwnDescendants()
        {
            var tree = MakeTree(out var items);

            Assert.IsFalse(tree.CanDrop(items[0], items[0]), "A row cannot be dropped onto itself.");
            Assert.IsFalse(tree.CanDrop(items[0], items[1]), "A row cannot be dropped onto its own child.");
            Assert.IsFalse(tree.CanDrop(items[0], items[3]), "A row cannot be dropped onto a deeper descendant.");
            Assert.IsTrue(tree.CanDrop(items[1], items[4]));
            Assert.IsTrue(tree.CanDrop(items[4], items[3]), "Dropping into an unrelated subtree is legal.");
        }

        [UnityTest]
        public IEnumerator GetDropPosition_ReadsTheTopAndBottomQuartersAsBeforeAndAfter()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            Assert.AreEqual(DropPosition.Before, tree.GetDropPosition(items[4], RowPoint(items[4], 0.1f)));
            Assert.AreEqual(DropPosition.Inside, tree.GetDropPosition(items[4], RowPoint(items[4], 0.5f)));
            Assert.AreEqual(DropPosition.After, tree.GetDropPosition(items[4], RowPoint(items[4], 0.9f)));
        }

        [UnityTest]
        public IEnumerator UpdateDrag_MarksTheDropTarget_AndClearsThePreviousOne()
        {
            var tree = MakeTree(out var items);
            tree.onReparent = (_, _, _) => { };
            yield return Mount(tree);

            tree.BeginDrag(items[1]);
            tree.UpdateDrag(RowPoint(items[4], 0.5f));

            Assert.AreSame(items[4], tree.dropTargetItem);
            Assert.AreEqual(DropPosition.Inside, tree.dropPosition);
            Assert.AreEqual(DropPosition.Inside, items[4].dropPosition);
            Assert.IsTrue(items[4].ClassListContains("appui-tree-view-item--drop-inside"));

            tree.UpdateDrag(RowPoint(items[2], 0.9f));
            Assert.IsNull(items[4].dropPosition, "The previous drop target kept its indicator.");
            Assert.IsFalse(items[4].ClassListContains("appui-tree-view-item--drop-inside"));
            Assert.AreSame(items[2], tree.dropTargetItem);
            Assert.AreEqual(DropPosition.After, items[2].dropPosition);
        }

        [UnityTest]
        public IEnumerator UpdateDrag_RefusesADescendantOfTheDraggedRow()
        {
            var tree = MakeTree(out var items);
            tree.onReparent = (_, _, _) => { };
            yield return Mount(tree);

            Assert.AreSame(items[1], tree.GetItemAtPosition(RowPoint(items[1], 0.5f)),
                "The fixture is not laid out, so this test would pass vacuously.");

            tree.BeginDrag(items[0]);
            tree.UpdateDrag(RowPoint(items[1], 0.5f));

            Assert.IsNull(tree.dropTargetItem, "A row must not be droppable into its own subtree.");
            Assert.IsNull(tree.dropPosition);
            Assert.IsNull(items[1].dropPosition);
        }

        [UnityTest]
        public IEnumerator EndDrag_InvokesOnReparent_WithTheDraggedRowTargetAndPosition()
        {
            var tree = MakeTree(out var items);
            TreeViewItem gotDrag = null, gotTarget = null;
            DropPosition? gotPosition = null;
            var calls = 0;
            tree.onReparent = (d, t, p) => { calls++; gotDrag = d; gotTarget = t; gotPosition = p; };
            yield return Mount(tree);

            tree.BeginDrag(items[1]);
            tree.UpdateDrag(RowPoint(items[4], 0.1f));
            Assert.IsTrue(tree.EndDrag());

            Assert.AreEqual(1, calls);
            Assert.AreSame(items[1], gotDrag);
            Assert.AreSame(items[4], gotTarget);
            Assert.AreEqual(DropPosition.Before, gotPosition);

            Assert.IsNull(tree.draggedItem, "EndDrag must clear the drag state.");
            Assert.IsNull(tree.dropTargetItem);
            Assert.IsNull(tree.dragPayload);
            Assert.IsFalse(items[1].dragging);
            Assert.IsNull(items[4].dropPosition);
            Assert.IsFalse(tree.ClassListContains(TreeList.draggingUssClassName));

            // The tree never moves anything itself; reparenting is the handler's job.
            Assert.AreSame(items[0], items[1].hierarchy.parent.hierarchy.parent.hierarchy.parent);
        }

        [UnityTest]
        public IEnumerator EndDrag_OverNothing_DoesNotReparent()
        {
            var tree = MakeTree(out var items);
            var calls = 0;
            tree.onReparent = (_, _, _) => calls++;
            yield return Mount(tree);

            tree.BeginDrag(items[1]);
            tree.UpdateDrag(new Vector2(-1000f, -1000f));
            Assert.IsFalse(tree.EndDrag());
            Assert.AreEqual(0, calls);
            Assert.IsNull(tree.draggedItem);
        }

        [UnityTest]
        public IEnumerator CancelDrag_ClearsEverything_WithoutReparenting()
        {
            var tree = MakeTree(out var items);
            var calls = 0;
            tree.onReparent = (_, _, _) => calls++;
            yield return Mount(tree);

            tree.BeginDrag(items[1]);
            tree.UpdateDrag(RowPoint(items[4], 0.5f));
            tree.CancelDrag();

            Assert.AreEqual(0, calls);
            Assert.IsNull(tree.draggedItem);
            Assert.IsNull(tree.dropTargetItem);
            Assert.IsFalse(items[1].dragging);
            Assert.IsNull(items[4].dropPosition);
            Assert.IsFalse(tree.ClassListContains(TreeList.draggingUssClassName));
        }

        /// <summary>
        /// The Dragger is what turns a real press into a drag, and it must refuse to start one when
        /// the tree has no onReparent handler, when the row opted out, or when the press landed on
        /// the caret — a disclosure, not a move.
        /// </summary>
        [UnityTest]
        public IEnumerator Dragger_StartsADrag_OnlyWhereADragIsLegal()
        {
            var tree = MakeTree(out var items);
            yield return Mount(tree);

            Assert.IsNotNull(tree.dragger, "The tree must carry a Dragger, like every other App UI drag surface.");
            Assert.IsFalse(tree.dragger.acceptStartDrag(RowPoint(items[4], 0.5f)),
                "No onReparent handler must mean no drag at all.");

            tree.onReparent = (_, _, _) => { };
            Assert.IsTrue(tree.dragger.acceptStartDrag(RowPoint(items[4], 0.5f)));

            items[4].draggable = false;
            Assert.IsFalse(tree.dragger.acceptStartDrag(RowPoint(items[4], 0.5f)),
                "A row that opted out of dragging must not start one.");

            var caret = items[0].Q(className: TreeViewItem.caretButtonUssClassName);
            Assert.IsFalse(tree.dragger.acceptStartDrag(caret.worldBound.center),
                "A press on the caret is a disclosure, never the start of a move.");
        }

        /// <summary>
        /// A pointer-driven drag the tree is not running produces none of the events its own drag
        /// handling watches for, so a host has to be able to paint the indicator itself.
        /// </summary>
        [Test]
        public void DropIndicator_PaintsTheSameIndicatorTheTreesOwnDragWould()
        {
            var item = new TreeViewItem { label = "A" };

            Assert.IsFalse(item.ClassListContains(
                TreeViewItem.GetDropPositionUssClassName(DropPosition.Before)));

            item.dropIndicator = new Optional<DropPosition>(DropPosition.Before);
            Assert.IsTrue(item.ClassListContains(
                    TreeViewItem.GetDropPositionUssClassName(DropPosition.Before)),
                "A host-driven indicator must reuse the tree's own highlight styles.");

            item.dropIndicator = default;
            Assert.IsFalse(item.ClassListContains(
                    TreeViewItem.GetDropPositionUssClassName(DropPosition.Before)),
                "Clearing the host-driven indicator must clear the highlight.");
        }

        /// <summary>
        /// The two sources must never contradict each other on screen. Asserting only that the
        /// host-driven one paints would let a real drag be overpainted by a stale outside value.
        /// </summary>
        [Test]
        public void TreesOwnDropTarget_WinsOverTheHostDrivenIndicator()
        {
            var item = new TreeViewItem { label = "A" };
            item.dropIndicator = new Optional<DropPosition>(DropPosition.Before);

            item.dropPosition = DropPosition.Inside;
            Assert.IsTrue(item.ClassListContains(
                    TreeViewItem.GetDropPositionUssClassName(DropPosition.Inside)),
                "The tree's own drag must win while it is targeting this row.");
            Assert.IsFalse(item.ClassListContains(
                TreeViewItem.GetDropPositionUssClassName(DropPosition.Before)));

            item.dropPosition = null;
            Assert.IsTrue(item.ClassListContains(
                    TreeViewItem.GetDropPositionUssClassName(DropPosition.Before)),
                "Once the tree's drag leaves, the host-driven indicator must come back.");
        }
        // ── Caret glyph ─────────────────────────────────────────────────────

        static Icon CaretIcon(TreeViewItem item) =>
            item.caretIconElement;

        [Test]
        public void CaretIcon_DefaultsToTheStandardCaret()
        {
            var item = new TreeViewItem { label = "a", showCaret = true };
            Assert.AreEqual("caret-down", CaretIcon(item).iconName);
        }

        /// <summary>
        /// The tree hands the glyph down as a context rather than walking its rows, so a row added after
        /// the property was set picks it up as well.
        /// </summary>
        [UnityTest]
        public IEnumerator CaretIcon_ReachesEveryRow_IncludingOnesAddedLater()
        {
            var tree = new TreeList { caretIcon = "caret-down-shallow" };
            var first = new TreeViewItem { label = "a", showCaret = true };
            tree.Add(first);

            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(tree);
            yield return null;
            yield return null;

            Assert.AreEqual("caret-down-shallow", CaretIcon(first).iconName);

            var later = new TreeViewItem { label = "b", showCaret = true };
            tree.Add(later);
            yield return null;

            Assert.AreEqual("caret-down-shallow", CaretIcon(later).iconName,
                "A row added after the glyph was chosen must still draw it.");

            tree.caretIcon = null;
            yield return null;

            Assert.AreEqual("caret-down", CaretIcon(first).iconName,
                "Clearing the override must fall back to the default caret, not to nothing.");
        }

    }
}
