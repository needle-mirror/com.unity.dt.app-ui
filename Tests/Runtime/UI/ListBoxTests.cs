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
    [TestOf(typeof(ListBox))]
    class ListBoxTests : VisualElementTests<ListBox>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ListBox.uss";

        const string k_ItemUssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/ListViewItem.uss";

        static readonly Size[] k_AllSizes = { Size.Sm, Size.Md, Size.Lg };

        protected override string mainUssClassName => ListBox.ussClassName;

        // Handed over to ListBoxSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => MakeList(20));

                yield return new Story("Sizes", _ =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    foreach (var size in k_AllSizes)
                    {
                        var list = MakeList(10);
                        list.size = size;
                        list.style.width = 200;
                        row.Add(list);
                    }
                    return row;
                });

                yield return new Story("MultipleSelection", MultipleSelection);
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ListBox />",
            @"<appui:ListBox size=""Sm"" />",
            @"<appui:ListBox size=""Md"" />",
            @"<appui:ListBox size=""Lg"" />",
            @"<appui:ListBox item-height=""40"" selection-type=""Multiple"" allow-no-selection=""false"" />",
        };

        /// <summary>
        /// Two rows actually selected under <see cref="SelectionType.Multiple"/>.
        /// </summary>
        /// <remarks>
        /// <c>selectionType</c> on its own has no resting visual representation, so a story that
        /// only sets it is pixel-identical to <c>Default</c>. The rows are generated, so the
        /// selection can only be made once the list has laid out.
        /// </remarks>
        static IEnumerator MultipleSelection(StoryContext ctx)
        {
            var list = MakeList(20);
            list.selectionType = SelectionType.Multiple;
            ctx.container.Add(list);

            for (var i = 0; i < 3; i++)
                yield return null;

            list.SetSelectionWithoutNotify(new[] { 1, 2 });
            yield return null;
        }

        [UnityTest]
        public IEnumerator MultipleSelectionStory_ActuallySelects()
        {
            yield return RunStory("MultipleSelection", ctx =>
            {
                var list = ctx.container.Q<ListBox>();
                Assert.IsNotNull(list);
                Assert.AreEqual(SelectionType.Multiple, list.selectionType);
                CollectionAssert.AreEquivalent(new[] { 1, 2 }, list.selectedIndices,
                    "selectionType has no resting visual representation, so without a selection this "
                    + "story is pixel-identical to Default.");
            });
        }

        static ListBox MakeList(int count)
        {
            return new ListBox
            {
                style = { height = 400 },
                itemHeight = 40,
                itemsSource = Enumerable.Range(0, count).Select(i => (object)$"Item {i}").ToList(),
            };
        }

        IEnumerator Mount(ListBox list)
        {
            m_TestUI.rootVisualElement.Clear();
            m_Panel = new Panel();
            m_TestUI.rootVisualElement.Add(m_Panel);
            m_Panel.StretchToParentSize();
            m_Panel.Add(list);
            yield return null;
            yield return null;
        }

        [Test, Order(10)]
        public void Size_DefaultsToMd_AndCarriesTheGridViewBaseClass()
        {
            var list = new ListBox();

            Assert.AreEqual(Size.Md, list.size,
                "ListBox must default to Size.Md like every other App UI control.");
            Assert.IsTrue(list.ClassListContains(ListBox.ussClassName),
                "ListBox must carry its own main USS class.");
            Assert.IsTrue(list.ClassListContains(BaseGridView.ussClassName),
                "ListBox must keep the BaseGridView USS class so the shared grid rules still apply.");
            Assert.IsTrue(list.ClassListContains(ListBox.GetSizeUssClassName(Size.Md)),
                "ListBox must add the size class for its default size.");
        }

        [Test, Order(11)]
        public void Size_SwapsTheSizeUssClass_WithoutLeavingStaleOnes()
        {
            var list = new ListBox();

            foreach (var size in k_AllSizes)
            {
                list.size = size;
                Assert.IsTrue(list.ClassListContains(ListBox.GetSizeUssClassName(size)),
                    $"ListBox must add {ListBox.GetSizeUssClassName(size)} when size is {size}.");
                foreach (var other in k_AllSizes.Where(s => s != size))
                {
                    Assert.IsFalse(list.ClassListContains(ListBox.GetSizeUssClassName(other)),
                        $"ListBox kept the stale class {ListBox.GetSizeUssClassName(other)} after switching to {size}.");
                }
            }
        }

        [Test, Order(12)]
        public void SizeUssClassNames_MatchTheClassesDefinedInUss()
        {
            foreach (var size in k_AllSizes)
            {
                var expected = "appui-list-box--size-" + size.ToString().ToLower();
                Assert.AreEqual(expected, ListBox.GetSizeUssClassName(size),
                    "The generated size class name must match the one the USS defines.");
            }
        }

        [Test, Order(13)]
        public void ColumnCount_IsOne_BecauseAListIsASingleColumnGrid()
        {
            Assert.AreEqual(1, new ListBox().columnCount);
        }

        [UnityTest, Order(20)]
        public IEnumerator Size_PropagatesToListViewItemsThroughSizeContext()
        {
            var list = new ListBox
            {
                style = { height = 400 },
                itemHeight = 48,
                itemsSource = Enumerable.Range(0, 5).Select(i => (object)$"Item {i}").ToList(),
                size = Size.Lg,
            };

            yield return Mount(list);

            var items = list.Query<ListViewItem>().ToList();
            Assert.IsNotEmpty(items,
                "ListBox must build ListViewItem elements by default so it works without an explicit makeItem.");
            foreach (var item in items)
            {
                Assert.AreEqual(Size.Lg, item.size,
                    "ListBox must propagate its size to its items through SizeContext.");
            }

            list.size = Size.Sm;
            yield return null;

            foreach (var item in list.Query<ListViewItem>().ToList())
            {
                Assert.AreEqual(Size.Sm, item.size,
                    "Changing ListBox.size must re-propagate to the items already realized.");
            }

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(21)]
        public IEnumerator DefaultBinding_PutsTheItemTextInTheItemTitle()
        {
            var list = MakeList(5);

            yield return Mount(list);

            var item = list.Query<ListViewItem>().First();
            Assert.AreEqual("Item 0", item.title,
                "The default bindItem must render the data item into ListViewItem.title.");

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(30)]
        public IEnumerator Selection_IsSingleByDefault_AndSelectedIndexRoundTrips()
        {
            var list = MakeList(20);
            Assert.AreEqual(SelectionType.Single, list.selectionType,
                "ListBox must default to single selection, matching the registry's selectedId model.");

            yield return Mount(list);

            var notified = new List<int>();
            list.selectedIndicesChanged += indices => notified.Clear();
            list.selectionChanged += _ => notified.Add(list.selectedIndex);

            list.selectedIndex = 3;
            yield return null;

            Assert.AreEqual(3, list.selectedIndex);
            Assert.AreEqual(1, list.selectionCount,
                "Single selection must never hold more than one index.");

            list.selectedIndex = 7;
            yield return null;

            Assert.AreEqual(7, list.selectedIndex);
            Assert.AreEqual(1, list.selectionCount);

            list.ClearSelection();
            yield return null;

            Assert.AreEqual(-1, list.selectedIndex,
                "ListBox must support an empty selection, the equivalent of a null selectedId.");

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(31)]
        public IEnumerator Selection_MarksTheSelectedItemWithIsSelected()
        {
            var list = MakeList(10);

            yield return Mount(list);

            list.selectedIndex = 2;
            yield return null;

            var items = list.Query<ListViewItem>().ToList();
            var selected = items.Where(i => i.ClassListContains(Styles.selectedUssClassName)).ToList();
            Assert.AreEqual(1, selected.Count,
                "Exactly one item must carry the is-selected class in single selection mode.");
            Assert.AreEqual("Item 2", selected[0].title);

            list.selectedIndex = 4;
            yield return null;

            selected = list.Query<ListViewItem>().ToList()
                .Where(i => i.ClassListContains(Styles.selectedUssClassName)).ToList();
            Assert.AreEqual(1, selected.Count,
                "The previously selected item must lose the is-selected class.");
            Assert.AreEqual("Item 4", selected[0].title);

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(32)]
        public IEnumerator KeyboardNavigation_MovesTheSelectionOneRowAtATime()
        {
            var list = MakeList(20);

            yield return Mount(list);

            list.selectedIndex = 5;
            yield return null;

            list.Apply(GridView.GridOperations.Down);
            yield return null;
            Assert.AreEqual(6, list.selectedIndex,
                "Down must move the selection by exactly one item in a single-column list.");

            list.Apply(GridView.GridOperations.Up);
            yield return null;
            Assert.AreEqual(5, list.selectedIndex,
                "Up must move the selection back by exactly one item.");

            list.Apply(GridView.GridOperations.Begin);
            yield return null;
            Assert.AreEqual(0, list.selectedIndex, "Begin must select the first item.");

            list.Apply(GridView.GridOperations.End);
            yield return null;
            Assert.AreEqual(19, list.selectedIndex, "End must select the last item.");

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(33)]
        public IEnumerator Selection_Multiple_AllowsMoreThanOneIndex()
        {
            var list = MakeList(20);
            list.selectionType = SelectionType.Multiple;

            yield return Mount(list);

            list.SetSelection(new[] { 1, 2, 3 });
            yield return null;

            Assert.AreEqual(3, list.selectionCount);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, list.selectedIndices.ToList());

            m_TestUI.rootVisualElement.Clear();
        }

        [UnityTest, Order(34)]
        public IEnumerator Selection_None_ClearsAndRefusesSelection()
        {
            var list = MakeList(20);

            yield return Mount(list);

            list.selectedIndex = 4;
            yield return null;
            Assert.AreEqual(4, list.selectedIndex);

            list.selectionType = SelectionType.None;
            yield return null;

            Assert.AreEqual(-1, list.selectedIndex,
                "Switching to SelectionType.None must clear the selection.");

            list.selectedIndex = 2;
            yield return null;
            Assert.AreEqual(-1, list.selectedIndex,
                "SelectionType.None must refuse new selections.");

            m_TestUI.rootVisualElement.Clear();
        }

        [Test, Order(40)]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsRegisteredInTheMainStyleSheet()
        {
            var appUiUss = Path.GetFullPath("Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss");
            StringAssert.Contains("@import \"./Components/ListBox.uss\";", File.ReadAllText(appUiUss),
                "ListBox.uss must be imported by App UI.uss or none of its rules ever apply.");
        }

        [Test, Order(41)]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_DefinesEverySizeStep_BoundToTheControlSizingAliases()
        {
            var uss = ReadUss(k_UssPath);

            foreach (var size in k_AllSizes)
            {
                var name = size.ToString().ToLower();
                var rule = Regex.Match(uss,
                    @"\.appui-list-box--size-" + name + @"\s+\.appui-listview-item\s*\{(?<body>[^}]*)\}");
                Assert.IsTrue(rule.Success,
                    $"ListBox.uss has no .appui-list-box--size-{name} .appui-listview-item rule, " +
                    $"so that size step styles nothing.");

                var body = rule.Groups["body"].Value;
                var expected = "var(--appui-alias-control-sizing-" + name + "-height)";
                StringAssert.Contains("min-height: " + expected, body,
                    $"The {name} row must bind its height to {expected}, not to a raw sizing step.");
                Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                    $".appui-list-box--size-{name} uses a raw --appui-sizing step instead of the control alias.");
                Assert.IsFalse(Regex.IsMatch(body, @":\s*-?\d+(\.\d+)?px"),
                    $".appui-list-box--size-{name} uses a literal pixel value instead of a token.");
            }
        }

        [Test, Order(42)]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesNoLiteralColours()
        {
            var uss = ReadUss(k_UssPath);

            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "ListBox.uss must not contain a hex colour; use an --appui-* token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "ListBox.uss must not contain an rgb()/rgba() colour; use an --appui-* token.");
        }

        [Test, Order(43)]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void ItemUss_BindsHoverAndSelectionToTheActionAliases()
        {
            var uss = ReadUss(k_ItemUssPath);

            var hover = Regex.Match(uss, @"\.appui-listview-item:hover\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(hover.Success, "ListViewItem.uss has no :hover rule.");
            StringAssert.Contains("var(--appui-alias-control-background-color-hover)", hover.Groups["body"].Value,
                "The item hover background must use the control hover alias, matching the registry's bg-control-hover.");

            var selected = Regex.Match(uss, @"\.appui-listview-item\.is-selected\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(selected.Success,
                "ListViewItem.uss has no .is-selected rule, so a selected row is invisible.");
            var selectedBody = selected.Groups["body"].Value;
            StringAssert.Contains("var(--appui-alias-actions-selected-background-color-default)", selectedBody,
                "The selected background must use the actions-selected alias, matching the registry's bg-selected.");

            var selectedHover = Regex.Match(uss,
                @"\.appui-listview-item\.is-selected:hover\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(selectedHover.Success,
                "ListViewItem.uss has no .is-selected:hover rule.");
            StringAssert.Contains("var(--appui-alias-actions-selected-background-color-hover)",
                selectedHover.Groups["body"].Value,
                "The selected hover background must use the actions-selected hover alias.");
        }

        static string ReadUss(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return File.ReadAllText(fullPath);
        }
    }
}
