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
    [TestOf(typeof(TreeListVirtual))]
    class TreeListVirtualTests : UIComponentTests<TreeListVirtual>
    {
        const float k_ViewHeight = 200f;

        // A two-level tree: `parents` parents of `kids` children each, ids "pN" and "pN-C".
        readonly HashSet<string> m_Expanded = new HashSet<string>();

        int m_Parents;
        int m_Kids;

        // Parents whose children are still loading: their level flattens to a placeholder row.
        readonly HashSet<string> m_Loading = new HashSet<string>();

        List<TreeListRow> Flatten()
        {
            return TreeListFlatten.Flatten(new TreeListFlattenOptions<string>
            {
                getChildren = parentId =>
                {
                    if (parentId == null)
                        return Enumerable.Range(0, m_Parents).Select(p => $"p{p}").ToList();
                    if (m_Loading.Contains(parentId))
                        return null;
                    return Enumerable.Range(0, m_Kids).Select(c => $"{parentId}-{c}").ToList();
                },
                getId = id => id,
                hasChildren = id => !id.Contains("-"),
                isExpanded = id => m_Expanded.Contains(id),
            }).rows.Cast<TreeListRow>().ToList();
        }

        TreeListVirtual Mount(int parents, int kids, bool expandAll, SelectionType selectionType = SelectionType.Single)
        {
            m_Parents = parents;
            m_Kids = kids;
            m_Expanded.Clear();
            m_Loading.Clear();
            if (expandAll)
                for (var p = 0; p < parents; p++)
                    m_Expanded.Add($"p{p}");

            var tree = new TreeListVirtual
            {
                selectionType = selectionType,
                style = { width = 300, height = k_ViewHeight, flexGrow = 0 },
            };
            tree.onExpandedChange = (id, open) =>
            {
                if (open) m_Expanded.Add(id);
                else m_Expanded.Remove(id);
                tree.rows = Flatten();
            };
            tree.rows = Flatten();
            MountElement(tree);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            return tree;
        }

        int ElementBound(TreeListVirtual tree)
        {
            var viewRows = Mathf.CeilToInt(k_ViewHeight / tree.rowSize);
            var overscan = Mathf.Max(10, viewRows);
            // The window, its overscan on both sides, and the lead row pinned out of view.
            return viewRows + 2 * overscan + 2;
        }

        [Test]
        public void TreeListVirtual_LargeTree_ShouldRealiseOnlyTheRowsNearTheView()
        {
            var tree = Mount(100, 99, expandAll: true);

            Assert.AreEqual(10000, tree.rows.Count);
            Assert.Greater(tree.realisedRowCount, 0, "The rows in view should be realised.");
            Assert.LessOrEqual(tree.realisedRowCount, ElementBound(tree),
                "Only the window and its overscan should be realised, not the whole tree.");
            Assert.LessOrEqual(tree.Query<TreeViewItem>().ToList().Count, ElementBound(tree),
                "The element count should stay bounded, pooled rows included.");
            Assert.IsNotNull(tree.GetRealisedItem(0));
            Assert.IsNull(tree.GetRealisedItem(9999), "A row far below the view should not exist.");
        }

        [Test]
        public void TreeListVirtual_Scroll_ShouldMoveTheWindow()
        {
            var tree = Mount(100, 99, expandAll: true);

            tree.ScrollToIndex(5000);
            simulate.FrameUpdate();

            var item = tree.GetRealisedItem(5000);
            Assert.IsNotNull(item, "The row scrolled to should be realised.");
            Assert.AreEqual(tree.rows[5000].id, item.label, "The recycled row should be bound to its row.");
            Assert.IsNull(tree.GetRealisedItem(0), "Rows far above the view should be released.");
            Assert.IsTrue(tree.IsIndexVisible(5000));
            Assert.LessOrEqual(tree.Query<TreeViewItem>().ToList().Count, ElementBound(tree),
                "Scrolling should recycle rows rather than create new ones.");
        }

        [Test]
        public void TreeListVirtual_ClickRow_ShouldSelectItById()
        {
            var tree = Mount(3, 2, expandAll: true);
            var item = tree.GetRealisedItem(2);

            simulate.Click(item.Q(className: TreeViewItem.titleLabelUssClassName));

            Assert.AreEqual(tree.rows[2].id, tree.selectedId);
            Assert.IsTrue(item.selected);
        }

        [Test]
        public void TreeListVirtual_CaretClick_ShouldRequestExpansion()
        {
            var tree = Mount(3, 2, expandAll: false);
            Assert.AreEqual(3, tree.rows.Count);

            simulate.Click(tree.GetRealisedItem(0).Q(className: TreeViewItem.caretButtonUssClassName));
            simulate.FrameUpdate();

            Assert.AreEqual(5, tree.rows.Count, "The caret should expand the row through onExpandedChange.");
            Assert.IsNull(tree.selectedId, "The caret should not select.");
        }

        [Test]
        public void TreeListVirtual_DownArrowPastTheView_ShouldReachUnrealisedRowsAndScroll()
        {
            var tree = Mount(100, 99, expandAll: true);
            tree.SetSelection(new[] { tree.rows[0].id });
            tree.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.End);
            simulate.FrameUpdate();
            Assert.AreEqual(tree.rows[9999].id, tree.selectedId, "End should reach the last row.");
            Assert.IsNotNull(tree.GetRealisedItem(9999), "The lead row should be scrolled into view.");

            simulate.KeyPress(KeyCode.UpArrow);
            Assert.AreEqual(tree.rows[9998].id, tree.selectedId);
        }

        [Test]
        public void TreeListVirtual_LeadRow_ShouldStayRealisedWhenScrolledAway()
        {
            var tree = Mount(100, 99, expandAll: true);
            tree.SetSelection(new[] { tree.rows[0].id });

            tree.ScrollToIndex(8000);
            simulate.FrameUpdate();

            Assert.IsNotNull(tree.GetRealisedItem(0), "The lead row is pinned so it survives scrolling.");
            Assert.IsTrue(tree.GetRealisedItem(0).selected);
        }

        [Test]
        public void TreeListVirtual_LeftArrow_ShouldCollapseThenMoveToParent()
        {
            var tree = Mount(3, 2, expandAll: true);
            tree.SetSelection(new[] { "p0-1" });
            tree.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.LeftArrow);
            Assert.AreEqual("p0", tree.selectedId, "Left on a child should move to its parent.");

            simulate.KeyPress(KeyCode.LeftArrow);
            Assert.IsFalse(m_Expanded.Contains("p0"), "Left on an open parent should collapse it.");
        }

        [Test]
        public void TreeListVirtual_RightArrow_ShouldMoveToTheFirstChildOnlyOnceLoaded()
        {
            var tree = Mount(3, 2, expandAll: true);
            m_Loading.Add("p0");
            tree.rows = Flatten();
            Assert.IsTrue(tree.rows[1].placeholder, "p0's children should still be loading.");
            tree.SetSelection(new[] { "p0" });
            tree.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual("p0", tree.selectedId,
                "Right on an open parent whose children are loading should stay, not jump to the next sibling.");

            m_Loading.Clear();
            tree.rows = Flatten();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual("p0-0", tree.selectedId, "Right on an open, loaded parent should move to its first child.");
        }

        [Test]
        public void TreeListVirtual_ShiftRange_ShouldSpanRowsOutsideTheWindow()
        {
            var tree = Mount(100, 99, expandAll: true, SelectionType.Multiple);
            tree.Select(tree.rows[0].id);

            tree.Select(tree.rows[5000].id, shift: true);

            Assert.AreEqual(5001, tree.selectedIds.Count);
            Assert.AreEqual(tree.rows[5000].id, tree.selectedId, "The row acted on leads the range.");
        }

        [Test]
        public void TreeListVirtual_MultiplePlainPress_ShouldSelectOnRelease()
        {
            var tree = Mount(3, 2, expandAll: true, SelectionType.Multiple);
            tree.Select("p0");
            tree.Select("p1", toggle: true);
            var target = tree.GetRealisedItem(tree.IndexOf("p1")).Q(className: TreeViewItem.titleLabelUssClassName);

            simulate.MouseDown(target.worldBound.center);
            CollectionAssert.AreEquivalent(new[] { "p0", "p1" }, tree.selectedIds,
                "A plain press should leave a group whole until the release.");
            simulate.MouseUp(target.worldBound.center);

            CollectionAssert.AreEqual(new[] { "p1" }, tree.selectedIds);
        }

        [Test]
        public void TreeListVirtual_RowsInsertedAbove_ShouldKeepEachElementOnItsRow()
        {
            var tree = Mount(3, 2, expandAll: false);
            var p1 = tree.GetRealisedItem(tree.IndexOf("p1"));

            tree.onExpandedChange("p0", true);

            Assert.AreSame(p1, tree.GetRealisedItem(tree.IndexOf("p1")),
                "A realised row's element should follow it to its new index, so pinned state survives.");
            Assert.AreEqual("p1", p1.label);
        }

        [Test]
        public void TreeListVirtual_PressThatLeavesAndReturns_ShouldNotSelect()
        {
            var tree = Mount(3, 2, expandAll: true, SelectionType.Multiple);
            tree.Select("p0");
            tree.Select("p1", toggle: true);
            var center = tree.GetRealisedItem(tree.IndexOf("p1"))
                .Q(className: TreeViewItem.titleLabelUssClassName).worldBound.center;

            simulate.MouseDown(center);
            simulate.MouseMove(new Vector2(tree.worldBound.xMax + 50f, center.y));
            simulate.MouseMove(center);
            simulate.MouseUp(center);

            CollectionAssert.AreEquivalent(new[] { "p0", "p1" }, tree.selectedIds,
                "A press that travelled outside the tree is a drag, not a click.");
        }

        [Test]
        public void TreeListVirtual_Depth_ShouldIndentNestedRows()
        {
            var tree = Mount(1, 1, expandAll: true);

            var top = tree.GetRealisedItem(0).Q(className: TreeViewItem.caretGutterUssClassName);
            var child = tree.GetRealisedItem(1).Q(className: TreeViewItem.caretGutterUssClassName);

            Assert.Greater(child.worldBound.xMin, top.worldBound.xMin + 1f, "A level-1 row should be indented.");
        }
    }
}
#endif
