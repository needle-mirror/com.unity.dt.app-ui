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
    [TestOf(typeof(CardGrid))]
    class CardGridTests : VisualElementTests<CardGrid>
    {
        protected override string mainUssClassName => CardGrid.ussClassName;

        // Handed over to CardGridSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Fill", (ctx) => BuildGrid(true));
                yield return new Story("FixedColumns", (ctx) => BuildGrid(false));
            }
        }

        static CardGrid BuildGrid(bool fill)
        {
            var grid = new CardGrid { minItemWidth = 160, fill = fill, style = { width = 620 } };
            for (var i = 0; i < 6; i++)
            {
                var card = new Card { style = { height = 90 } };
                var header = new CardHeader();
                header.Add(new CardTitle { text = "Template " + (i + 1) });
                card.Add(header);
                grid.Add(card);
            }
            return grid;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CardGrid />",
            @"<appui:CardGrid min-item-width=""160"" fill=""false"" />",
            @"<appui:CardGrid min-item-width=""240""><appui:Card /><appui:Card /></appui:CardGrid>",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/CardGrid.uss";

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var grid = new CardGrid();
            Assert.AreEqual(200f, grid.minItemWidth);
            Assert.IsTrue(grid.fill);
        }

        [UnityTest]
        public IEnumerator Fill_LaysOutEqualColumnsThatSpanTheFullWidth()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620);

            var gap = grid.resolvedGap;
            // 620 wide, min 160, gap g -> columns = floor((620 + g) / (160 + g)) = 3
            Assert.AreEqual(3, grid.resolvedColumnCount, "Wrong auto-fill column count.");

            Assert.Greater(gap, 0f, "The grid gap resolved to zero; the gap probe is not sized by a token.");
            // UITK snaps resolved layout to whole pixels, hence the 1px tolerance.
            var expectedWidth = (620f - gap * 2f) / 3f;
            foreach (var item in items)
                Assert.AreEqual(expectedWidth, item.resolvedStyle.width, 1f,
                    "Fill columns must share the leftover width equally.");

            Assert.Greater(items[0].resolvedStyle.width, 160f,
                "With fill=true the tiles must grow past minItemWidth to fill the row.");
        }

        [UnityTest]
        public IEnumerator Fixed_KeepsTilesAtExactlyMinItemWidth()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = false };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620);

            foreach (var item in items)
                Assert.AreEqual(160f, item.resolvedStyle.width, 1f,
                    "With fill=false the tile width must be exactly minItemWidth.");
        }

        [UnityTest]
        public IEnumerator Items_WrapOntoAnewRowPastTheColumnCount()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620);

            Assert.AreEqual(items[0].layout.y, items[2].layout.y, 0.5f, "Items 0..2 must be on the first row.");
            Assert.Greater(items[3].layout.y, items[0].layout.y, "Item 3 must wrap onto a second row.");
        }

        [UnityTest]
        public IEnumerator NarrowingTheGrid_ReducesTheColumnCount()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            AddItems(grid, 6);
            yield return Mount(grid, 620);
            Assert.AreEqual(3, grid.resolvedColumnCount);

            grid.style.width = 340;
            for (var i = 0; i < 4; i++)
                yield return null;
            Assert.AreEqual(2, grid.resolvedColumnCount,
                "The grid did not re-flow when its width changed.");
        }

        [UnityTest]
        public IEnumerator TheLastColumn_CarriesNoTrailingGap()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620);

            var gap = grid.resolvedGap;
            Assert.Greater(gap, 0f, "The grid gap resolved to zero; the --appui-card-grid-gap token is not applied.");
            Assert.AreEqual(gap, items[0].resolvedStyle.marginRight, 0.01f);
            Assert.AreEqual(0f, items[2].resolvedStyle.marginRight, 0.01f,
                "The last column must not carry a trailing gap, or the row overflows and wraps early.");
        }

        [UnityTest]
        public IEnumerator Gap_ShrinksUnderCompactDensity()
        {
            var normalGrid = new CardGrid { minItemWidth = 160 };
            AddItems(normalGrid, 4);
            yield return Mount(normalGrid, 620, "normal");
            var normalGap = normalGrid.resolvedGap;

            var compactGrid = new CardGrid { minItemWidth = 160 };
            AddItems(compactGrid, 4);
            yield return Mount(compactGrid, 620, "compact");
            var compactGap = compactGrid.resolvedGap;

            Assert.Greater(normalGap, 0f);
            Assert.Less(compactGap, normalGap,
                $"The grid gap did not shrink under compact density (normal {normalGap}, compact {compactGap}). " +
                "It is probably a hardcoded pixel value.");
        }

        [UnityTest]
        public IEnumerator Ltr_FlowsFromTheLeftEdgeAndPutsTheGapOnTheRight()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620, dir: Dir.Ltr);

            Assert.AreEqual(Dir.Ltr, grid.resolvedLayoutDirection);
            var gap = grid.resolvedGap;
            Assert.Greater(gap, 0f);

            Assert.Less(items[0].layout.x, items[2].layout.x,
                "In LTR the tiles must flow left to right.");
            Assert.AreEqual(gap, items[0].resolvedStyle.marginRight, 0.01f,
                "In LTR the gap belongs on margin-right.");
            Assert.AreEqual(0f, items[0].resolvedStyle.marginLeft, 0.01f,
                "In LTR no gap may sit on margin-left.");
        }

        [UnityTest]
        public IEnumerator Rtl_FlowsFromTheRightEdgeAndPutsTheGapOnTheLeft()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620, dir: Dir.Rtl);

            Assert.AreEqual(Dir.Rtl, grid.resolvedLayoutDirection,
                "The grid did not pick the layout direction up from the DirContext.");

            Assert.Greater(items[0].layout.x, items[2].layout.x,
                "In RTL tile 0 must sit against the right edge and the row must run right to left.");
            Assert.AreEqual(grid.contentRect.xMax, items[0].layout.xMax, 1f,
                "In RTL the first tile must be flush with the grid's right edge.");

            var gap = grid.resolvedGap;
            Assert.Greater(gap, 0f);
            Assert.AreEqual(gap, items[0].resolvedStyle.marginLeft, 0.01f,
                "In RTL the gap must move to margin-left, the trailing edge under row-reverse.");
            Assert.AreEqual(0f, items[0].resolvedStyle.marginRight, 0.01f,
                "In RTL no gap may be left on margin-right, or the row overflows and wraps early.");
            Assert.AreEqual(0f, items[2].resolvedStyle.marginLeft, 0.01f,
                "The last column of an RTL row must not carry a trailing gap.");
        }

        [UnityTest]
        public IEnumerator Rtl_KeepsTheSameColumnCountAndTileWidthsAsLtr()
        {
            var ltr = new CardGrid { minItemWidth = 160, fill = true };
            var ltrItems = AddItems(ltr, 6);
            yield return Mount(ltr, 620, dir: Dir.Ltr);

            var rtl = new CardGrid { minItemWidth = 160, fill = true };
            var rtlItems = AddItems(rtl, 6);
            yield return Mount(rtl, 620, dir: Dir.Rtl);

            Assert.AreEqual(3, ltr.resolvedColumnCount);
            Assert.AreEqual(ltr.resolvedColumnCount, rtl.resolvedColumnCount,
                "The column arithmetic must be direction-agnostic.");
            Assert.AreEqual(ltrItems[0].resolvedStyle.width, rtlItems[0].resolvedStyle.width, 1f,
                "Tile widths must be identical in both directions.");
            Assert.AreEqual(ltrItems[0].layout.width * 3f + rtl.resolvedGap * 2f, 620f, 1.5f,
                "The RTL row must still span the full grid width.");
        }

        [UnityTest]
        public IEnumerator Rtl_WrapsDownwardsJustLikeLtr()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620, dir: Dir.Rtl);

            Assert.AreEqual(items[0].layout.y, items[2].layout.y, 0.5f,
                "Items 0..2 must share the first row in RTL too.");
            Assert.Greater(items[3].layout.y, items[0].layout.y,
                "Item 3 must wrap onto a second row below, not above.");
            Assert.AreEqual(items[0].layout.x, items[3].layout.x, 1f,
                "The second RTL row must start from the right edge as well.");
        }

        [UnityTest]
        public IEnumerator FlippingTheDirectionAtRuntime_MovesTheGapAndLeavesNoStaleMargin()
        {
            var grid = new CardGrid { minItemWidth = 160, fill = true };
            var items = AddItems(grid, 6);
            yield return Mount(grid, 620, dir: Dir.Ltr);
            var gap = grid.resolvedGap;
            Assert.AreEqual(gap, items[0].resolvedStyle.marginRight, 0.01f);

            m_Panel.layoutDirection = Dir.Rtl;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(Dir.Rtl, grid.resolvedLayoutDirection,
                "The grid did not react to the panel's layout direction change.");
            Assert.AreEqual(gap, items[0].resolvedStyle.marginLeft, 0.01f);
            Assert.AreEqual(0f, items[0].resolvedStyle.marginRight, 0.01f,
                "The LTR margin-right survived the flip, so the tiles carry a double gap.");
            Assert.Greater(items[0].layout.x, items[2].layout.x);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Rtl_FlipsTheRowInUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var rule = Regex.Match(uss, @"\.appui--rtl\s+\.appui-card-grid\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success,
                "CardGrid.uss has no `.appui--rtl .appui-card-grid` rule, so the grid cannot mirror.");
            StringAssert.Contains("flex-direction: row-reverse", rule.Groups["body"].Value,
                "The RTL rule must reverse the row.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Gap_BindsToADensityAwareSizingToken()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var rule = Regex.Match(uss, @"\.appui-card-grid\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "CardGrid.uss has no .appui-card-grid rule.");
            var body = rule.Groups["body"].Value;

            StringAssert.Contains("flex-wrap: wrap", body, "The grid must wrap; UITK has no CSS grid.");

            var probe = Regex.Match(uss, @"\.appui-card-grid__gap-probe\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(probe.Success, "CardGrid.uss has no .appui-card-grid__gap-probe rule.");
            Assert.IsTrue(Regex.IsMatch(probe.Groups["body"].Value, @"width:\s*var\(--appui-sizing-\d+\)"),
                "The gap probe must be sized by a density-aware --appui-sizing step, not a literal.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "CardGrid.uss contains a magic pixel value.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "CardGrid.uss contains a literal hex colour.");
        }

        static List<Card> AddItems(CardGrid grid, int count)
        {
            var items = new List<Card>();
            for (var i = 0; i < count; i++)
            {
                var card = new Card { style = { height = 40 } };
                grid.Add(card);
                items.Add(card);
            }
            return items;
        }

        IEnumerator Mount(CardGrid grid, float width, string density = "normal", Dir dir = Dir.Ltr)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en", layoutDirection = dir };
            m_Panel = panel;
            m_TestUI.rootVisualElement.Add(panel);
            grid.style.width = width;
            panel.Add(grid);

            for (var i = 0; i < 5; i++)
                yield return null;
        }
    }
}
