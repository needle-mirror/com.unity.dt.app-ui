#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TreeList))]
    class TreeListBehaviourTests : UIComponentTests<TreeList>
    {
        TreeList MountTree(SelectionType selectionType = SelectionType.Single)
        {
            var a = new TreeViewItem { label = "a" };
            var b = new TreeViewItem { label = "b" };
            var c = new TreeViewItem { label = "c" };

            var tree = new TreeList
            {
                selectionType = selectionType,
                style = { width = 300 },
            };
            tree.Add(a);
            tree.Add(b);
            tree.Add(c);

            return MountElement(tree);
        }

        [Test]
        public void TreeList_ClickItem_ShouldSelectIt()
        {
            var tree = MountTree();
            var items = tree.items;

            simulate.Click(items[1]);

            Assert.AreEqual(items[1], tree.selectedItem, "Clicking an item should select it.");
        }

        [Test]
        public void TreeList_ClickItem_ShouldRaiseSelectionChanged()
        {
            var tree = MountTree();
            var notified = new List<TreeViewItem>();
            tree.selectionChanged += selection => notified.AddRange(selection);

            simulate.Click(tree.items[2]);

            CollectionAssert.AreEqual(new[] { tree.items[2] }, notified,
                "A click that changes the selection should raise selectionChanged once with the new selection.");
        }

        [Test]
        public void TreeList_DownArrow_ShouldMoveSelectionToNextItem()
        {
            var tree = MountTree();
            tree.Select(tree.items[0]);

            tree.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.DownArrow);

            Assert.AreEqual(tree.items[1], tree.selectedItem,
                "Down arrow should move the selection to the next visible item.");
        }

        [Test]
        public void TreeList_MultipleSelection_ActionClickShouldToggleItemIntoSelection()
        {
            var tree = MountTree(SelectionType.Multiple);
            tree.Select(tree.items[0]);

            tree.Select(tree.items[1], toggle: true);

            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1] }, tree.selectedItems,
                "Select() with toggle should add the item to the existing multi-selection.");
        }

        // ── Press / click / drag (registry #1517) ──────────────────────────

        static Vector2 RowCenter(TreeViewItem item) =>
            item.Q(className: TreeViewItem.headerUssClassName).worldBound.center;

        TreeList MountGroupSelection()
        {
            var tree = MountTree(SelectionType.Multiple);
            tree.SetSelectionWithoutNotify(new[] { tree.items[0], tree.items[1] });
            simulate.FrameUpdate();
            return tree;
        }

        [Test]
        public void TreeList_MultipleSelection_PressOnGroupRow_DefersTheCollapseToTheRelease()
        {
            var tree = MountGroupSelection();
            var b = tree.items[1];

            simulate.MouseDown(RowCenter(b));

            CollectionAssert.AreEquivalent(new[] { tree.items[0], b }, tree.selectedItems,
                "Pressing a row of a multi-selection must keep the group whole until the release.");

            simulate.MouseUp(RowCenter(b));

            CollectionAssert.AreEqual(new[] { b }, tree.selectedItems,
                "A release that ends a click on a group row should collapse the selection onto it.");
        }

        [Test]
        public void TreeList_MultipleSelection_PressThatBecomesADrag_KeepsTheGroup()
        {
            var tree = MountGroupSelection();
            var b = tree.items[1];

            // A drag the consumer runs itself: the pointer leaves the row and comes back, so the
            // release lands where the press started and only the travel says it was a drag.
            simulate.MouseDown(RowCenter(b));
            simulate.MouseMove(RowCenter(tree.items[2]));
            simulate.MouseMove(RowCenter(b));
            simulate.MouseUp(RowCenter(b));

            CollectionAssert.AreEquivalent(new[] { tree.items[0], b }, tree.selectedItems,
                "A press that turned into a drag must leave the multi-selection as it was.");
        }

        [Test]
        public void TreeList_MultipleSelection_ReorderDragOfAGroupRow_KeepsTheGroup()
        {
            var tree = MountGroupSelection();
            var b = tree.items[1];
            var reparented = 0;
            tree.onReparent = (_, _, _) => reparented++;

            simulate.MouseDown(RowCenter(b));
            simulate.MouseMove(RowCenter(tree.items[2]));
            simulate.MouseUp(RowCenter(tree.items[2]));

            Assert.AreEqual(1, reparented, "Positive control: the press should have become a reorder drag.");
            CollectionAssert.AreEquivalent(new[] { tree.items[0], b }, tree.selectedItems,
                "A reorder drag of a group row must carry the group, not collapse it.");
        }

        [Test]
        public void TreeList_MultipleSelection_DragOfAnUnselectedRow_DoesNotSelectIt()
        {
            var tree = MountGroupSelection();
            var c = tree.items[2];

            simulate.MouseDown(RowCenter(c));
            simulate.MouseMove(RowCenter(tree.items[1]));
            simulate.MouseMove(RowCenter(c));
            simulate.MouseUp(RowCenter(c));

            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1] }, tree.selectedItems,
                "Dragging an unselected row must not first make it the selection.");
        }

        [Test]
        public void TreeList_MultipleSelection_ClickOnUnselectedRow_ReplacesOnRelease()
        {
            var tree = MountGroupSelection();
            var c = tree.items[2];

            simulate.MouseDown(RowCenter(c));
            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1] }, tree.selectedItems,
                "A plain press must change nothing until the release.");

            // Jitter under the 4px click distance still reads as a click.
            simulate.MouseMove(RowCenter(c) + new Vector2(2f, 0f));
            simulate.MouseUp(RowCenter(c) + new Vector2(2f, 0f));

            CollectionAssert.AreEqual(new[] { c }, tree.selectedItems,
                "A click on an unselected row should replace the selection with it.");
        }

        [Test]
        public void TreeList_MultipleSelection_ReleaseOnAnotherRow_SelectsNothing()
        {
            var tree = MountGroupSelection();
            var bHeader = tree.items[1].Q(className: TreeViewItem.headerUssClassName).worldBound;
            var cHeader = tree.items[2].Q(className: TreeViewItem.headerUssClassName).worldBound;

            // Across the boundary between two rows, inside the 4px click distance: the release does
            // not end a click on the pressed row, so it selects nothing, as on the web where the
            // other row's pointerup finds no press of its own.
            var from = new Vector2(bHeader.center.x, bHeader.yMax - 1f);
            var to = new Vector2(cHeader.center.x, cHeader.yMin + 1f);
            Assert.Less(Vector2.Distance(from, to), 4f, "Positive control: the travel stays a click.");

            simulate.MouseDown(from);
            simulate.MouseUp(to);

            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1] }, tree.selectedItems,
                "A release on a different row must not select the pressed row.");
        }

        [Test]
        public void TreeList_MultipleSelection_ModifiedPress_AppliesOnPress()
        {
            var tree = MountGroupSelection();
            var c = tree.items[2];

            // The action key is Cmd on macOS and Ctrl elsewhere; carrying both reads as it everywhere.
            const EventModifiers actionKey = EventModifiers.Command | EventModifiers.Control;
            simulate.MouseDown(RowCenter(c), MouseButton.LeftMouse, actionKey);

            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1], c }, tree.selectedItems,
                "An action-key press toggles the row in at once, before the release.");

            simulate.MouseUp(RowCenter(c), MouseButton.LeftMouse, actionKey);
            CollectionAssert.AreEquivalent(new[] { tree.items[0], tree.items[1], c }, tree.selectedItems,
                "The release of a modified press must not then collapse the selection.");
        }

        [Test]
        public void TreeList_SingleSelection_PressSelectsImmediately()
        {
            var tree = MountTree();
            var b = tree.items[1];

            simulate.MouseDown(RowCenter(b));

            Assert.AreEqual(b, tree.selectedItem, "Single selection should still select on press.");

            simulate.MouseUp(RowCenter(b));
        }

        static VisualElement HeaderOf(TreeViewItem item) => item.Q(TreeViewItem.headerUssClassName);

        [Test]
        public void TreeList_RightArrow_OnCaretRowWithoutNestedChildren_ShouldExpandThenStay()
        {
            // Flat rows laid out by a host: the caret row's children are not nested in it.
            var parent = new TreeViewItem { label = "Cube", showCaret = true };
            var sibling = new TreeViewItem { label = "Light" };
            var tree = new TreeList { style = { width = 300 } };
            tree.Add(parent);
            tree.Add(sibling);
            MountElement(tree);
            tree.Select(parent);

            tree.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);
            Assert.IsTrue(parent.expanded, "Right should expand a caret row that has no nested children.");

            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual(parent, tree.selectedItem,
                "Right on an expanded caret row should not step onto a sibling that is not its child.");

            simulate.KeyPress(KeyCode.LeftArrow);
            Assert.IsFalse(parent.expanded, "Left should collapse an expanded caret row.");
        }

        [Test]
        public void TreeList_KeyboardNavigationOff_ShouldNotTakeFocusOrKeys()
        {
            var tree = MountTree();
            tree.keyboardNavigation = false;
            tree.Select(tree.items[0]);

            Assert.IsFalse(tree.focusable, "A tree without keyboard navigation takes no focus.");
            HeaderOf(tree.items[0]).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.DownArrow);

            Assert.AreEqual(tree.items[0], tree.selectedItem,
                "Keys should pass through a tree without keyboard navigation.");
            simulate.Click(tree.items[1]);
            Assert.AreEqual(tree.items[1], tree.selectedItem, "The tree still provides selection.");
        }

        [Test]
        public void TreeViewItem_WithoutKeyboardRoot_ArrowKeysShouldExpandAndCollapseItself()
        {
            var tree = new TreeList { keyboardNavigation = false, style = { width = 300 } };
            var row = new TreeViewItem { label = "Cube", showCaret = true };
            tree.Add(row);
            MountElement(tree);

            HeaderOf(row).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);
            Assert.IsTrue(row.expanded, "Right on a rootless caret row should expand it.");

            simulate.KeyPress(KeyCode.LeftArrow);
            Assert.IsFalse(row.expanded, "Left on a rootless caret row should collapse it.");
        }

        [Test]
        public void TreeViewItem_WithoutKeyboardRoot_NestedLeafKeysShouldNotToggleParent()
        {
            var tree = new TreeList { keyboardNavigation = false, style = { width = 300 } };
            var parent = new TreeViewItem { label = "Cube", showCaret = true, expanded = true };
            var leaf = new TreeViewItem { label = "position", depth = 1 };
            parent.Add(leaf);
            tree.Add(parent);
            MountElement(tree);

            HeaderOf(leaf).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.LeftArrow);

            Assert.IsTrue(parent.expanded, "A key on a nested leaf should not fold its parent row.");
        }

        [Test]
        public void TreeViewItem_UnderKeyboardRoot_ShouldLeaveKeysToTheTree()
        {
            var tree = new TreeList { style = { width = 300 } };
            var row = new TreeViewItem { label = "Cube", showCaret = true };
            tree.Add(row);
            MountElement(tree);

            HeaderOf(row).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);

            // With no lead the tree selects the first row on Right; the row must not also expand itself.
            Assert.IsFalse(row.expanded, "Under a navigating tree the row should not handle arrows itself.");
        }
    }
}
#endif
