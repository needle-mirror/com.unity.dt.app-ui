using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    // A one-for-one port of the registry's tests/unit/tree-view-flatten.test.ts.
    [TestFixture]
    [TestOf(typeof(TreeListFlatten))]
    class TreeListFlattenTests
    {
        class Node
        {
            public string id;
            public int kids;
            public Node(string id, int kids) { this.id = id; this.kids = kids; }
        }

        static TreeListFlattenOptions<Node> Options(Dictionary<string, Node[]> levels, System.Func<string, bool> isExpanded) =>
            new TreeListFlattenOptions<Node>
            {
                getChildren = parentId => levels.TryGetValue(parentId ?? "root", out var kids) ? kids : null,
                getId = n => n.id,
                hasChildren = n => n.kids > 0,
                isExpanded = isExpanded,
            };

        [Test]
        public void Flatten_ListsOpenRowsDepthFirstWithLevelParentAndPosition()
        {
            var result = TreeListFlatten.Flatten(Options(new Dictionary<string, Node[]>
            {
                ["root"] = new[] { new Node("a", 2), new Node("b", 0) },
                ["a"] = new[] { new Node("a1", 0), new Node("a2", 0) },
            }, id => id == "a"));

            var rows = result.rows;
            CollectionAssert.AreEqual(new[] { "a", "a1", "a2", "b" }, rows.Select(r => r.id));
            CollectionAssert.AreEqual(new[] { 0, 1, 1, 0 }, rows.Select(r => r.level));
            CollectionAssert.AreEqual(new[] { null, "a", "a", null }, rows.Select(r => r.parentId));
            CollectionAssert.AreEqual(new[] { 2, 2, 2, 2 }, rows.Select(r => r.setSize));
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 2 }, rows.Select(r => r.posInSet));
            Assert.IsTrue(rows[0].expandable && rows[0].expanded);
            Assert.IsFalse(rows[3].expandable || rows[3].expanded);
            CollectionAssert.AreEqual(new[] { null, "a" }, result.openLevels);
            CollectionAssert.IsEmpty(result.unfetched);
        }

        [Test]
        public void Flatten_SkipsTheChildrenOfACollapsedParent()
        {
            var result = TreeListFlatten.Flatten(Options(new Dictionary<string, Node[]>
            {
                ["root"] = new[] { new Node("a", 1) },
                ["a"] = new[] { new Node("a1", 0) },
            }, _ => false));

            CollectionAssert.AreEqual(new[] { "a" }, result.rows.Select(r => r.id));
            Assert.IsTrue(result.rows[0].expandable);
            Assert.IsFalse(result.rows[0].expanded);
            CollectionAssert.AreEqual(new string[] { null }, result.openLevels);
        }

        [Test]
        public void Flatten_DrawsAnUnloadedOpenParentAsOnePlaceholderAndReportsIt()
        {
            var options = Options(new Dictionary<string, Node[]> { ["root"] = new[] { new Node("a", 3) } }, _ => true);
            options.placeholderCount = parent => parent?.kids ?? 5;
            var result = TreeListFlatten.Flatten(options);

            Assert.AreEqual(2, result.rows.Count);
            var placeholder = result.rows[1];
            Assert.IsTrue(placeholder.placeholder);
            Assert.AreEqual("a", placeholder.parentId);
            Assert.AreEqual(1, placeholder.level);
            Assert.AreEqual(3, placeholder.placeholderCount);
            CollectionAssert.AreEqual(new[] { "a" }, result.unfetched);
            CollectionAssert.AreEqual(new[] { null, "a" }, result.openLevels);
        }

        [Test]
        public void Flatten_DrawsAnUnloadedTopLevelAsAPlaceholder()
        {
            var options = Options(new Dictionary<string, Node[]>(), _ => false);
            options.placeholderCount = parent => parent != null ? 1 : 5;
            var result = TreeListFlatten.Flatten(options);

            Assert.AreEqual(1, result.rows.Count);
            Assert.IsTrue(result.rows[0].placeholder);
            Assert.IsNull(result.rows[0].parentId);
            Assert.AreEqual(0, result.rows[0].level);
            Assert.AreEqual(5, result.rows[0].placeholderCount);
            CollectionAssert.AreEqual(new string[] { null }, result.unfetched);
        }

        [Test]
        public void Flatten_ShowsAPlaceholderForAPendingParentWithoutAskingForItsLevel()
        {
            var options = Options(new Dictionary<string, Node[]>
            {
                ["root"] = new[] { new Node("a", 2) },
                ["a"] = new[] { new Node("stale", 0) },
            }, _ => true);
            options.isPending = n => n.id == "a";
            var result = TreeListFlatten.Flatten(options);

            CollectionAssert.AreEqual(new[] { false, true }, result.rows.Select(r => r.placeholder));
            CollectionAssert.IsEmpty(result.unfetched);
            CollectionAssert.AreEqual(new string[] { null }, result.openLevels);
        }

        [Test]
        public void Flatten_EmitsAnIdOnceWhenAStaleCacheListsItUnderTwoParents()
        {
            var result = TreeListFlatten.Flatten(Options(new Dictionary<string, Node[]>
            {
                ["root"] = new[] { new Node("a", 1), new Node("b", 1) },
                ["a"] = new[] { new Node("b", 1) },
                ["b"] = new[] { new Node("a", 1) },
            }, _ => true));

            var ids = result.rows.Where(r => !r.placeholder).Select(r => r.id).ToList();
            CollectionAssert.AreEqual(new[] { "a", "b" }, ids);
        }

        [Test]
        public void Flatten_FlattensTenThousandRows()
        {
            var levels = new Dictionary<string, Node[]>();
            var root = new List<Node>();
            for (var p = 0; p < 100; p++)
            {
                root.Add(new Node($"p{p}", 99));
                levels[$"p{p}"] = Enumerable.Range(0, 99).Select(c => new Node($"p{p}-{c}", 0)).ToArray();
            }

            levels["root"] = root.ToArray();
            var result = TreeListFlatten.Flatten(Options(levels, _ => true));

            Assert.AreEqual(10000, result.rows.Count);
            Assert.AreEqual("p99-98", result.rows[9999].id);
            Assert.AreEqual("p99", result.rows[9999].parentId);
        }

        [Test]
        public void Flatten_FollowsAPartlyLoadedLevelWithOnePlaceholderPerChildStillToCome()
        {
            var options = Options(new Dictionary<string, Node[]>
            {
                ["root"] = new[] { new Node("a", 3), new Node("b", 0) },
                ["a"] = new[] { new Node("a1", 0) },
            }, id => id == "a");
            options.pendingCount = parentId => parentId == "a" ? 2 : 0;
            var rows = TreeListFlatten.Flatten(options).rows;

            CollectionAssert.AreEqual(new[] { "a", "a1", "a#0", "a#1", "b" }, rows.Select(r => r.id));
            CollectionAssert.AreEqual(new[] { false, false, true, true, false }, rows.Select(r => r.placeholder));
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 3, 2 }, rows.Select(r => r.posInSet));
            CollectionAssert.AreEqual(new[] { 2, 3, 3, 3, 2 }, rows.Select(r => r.setSize));
            Assert.AreEqual(1, rows[2].level);
            Assert.AreEqual("a", rows[2].parentId);
            Assert.AreEqual(0, rows[2].pendingIndex);
        }
    }
}
