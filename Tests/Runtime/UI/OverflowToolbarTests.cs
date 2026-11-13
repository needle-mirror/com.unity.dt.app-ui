using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    [TestOf(typeof(OverflowToolbar))]
    class OverflowToolbarTests : VisualElementTests<OverflowToolbar>
    {
        protected override string mainUssClassName => OverflowToolbar.ussClassName;

        // Handed over to OverflowToolbarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/OverflowToolbar.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Roomy", (ctx) => Build(6, 600f));
                yield return new Story("Collapsed", (ctx) => Build(6, 160f));
            }
        }

        static OverflowToolbar Build(int items, float width)
        {
            var bar = new OverflowToolbar { style = { width = width } };
            AddItems(bar, items);
            return bar;
        }

        static List<VisualElement> AddItems(OverflowToolbar bar, int count, float itemWidth = 60f)
        {
            var items = new List<VisualElement>();
            for (var i = 0; i < count; i++)
            {
                var item = new VisualElement { name = "item" + i, style = { width = itemWidth, height = 20 } };
                bar.Add(item);
                items.Add(item);
            }
            return items;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:OverflowToolbar />",
            @"<appui:OverflowToolbar overflow-label=""More tools"" overflow-icon=""caret-down"" />",
            @"<appui:OverflowToolbar><appui:ActionButton /><appui:ActionButton /></appui:OverflowToolbar>",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var bar = new OverflowToolbar();
            Assert.AreEqual("More", bar.overflowLabel);
            Assert.AreEqual("caret-down", bar.overflowIcon);
            Assert.AreEqual(0, bar.itemCount);
            Assert.IsFalse(bar.overflowing);
            Assert.IsFalse(bar.menuOpen);
        }

        [Test]
        public void Items_LandInTheStripNotBesideTheCaret()
        {
            var bar = new OverflowToolbar();
            var item = new VisualElement();
            bar.Add(item);

            Assert.AreEqual(1, bar.itemCount);
            Assert.IsTrue(item.hierarchy.parent.ClassListContains(OverflowToolbar.containerUssClassName),
                "Items must go into the inline strip, not next to the caret.");
        }

        [UnityTest]
        public IEnumerator EverythingFits_ShowsEveryItemAndHidesTheCaret()
        {
            var bar = Build(4, 600f);
            yield return Mount(bar);

            Assert.AreEqual(4, bar.visibleItemCount, "All four items fit in 600px and must stay inline.");
            Assert.IsFalse(bar.overflowing);

            var caret = bar.Q(className: OverflowToolbar.caretUssClassName);
            Assert.AreEqual(DisplayStyle.None, caret.resolvedStyle.display,
                "The caret must not appear when nothing has collapsed.");
        }

        [UnityTest]
        public IEnumerator NotEnoughRoom_CollapsesTrailingItemsAndShowsTheCaret()
        {
            var bar = Build(6, 200f);
            yield return Mount(bar);

            Assert.IsTrue(bar.overflowing, "Six 60px items cannot fit in 200px. " + Diagnostics(bar));
            Assert.Less(bar.visibleItemCount, 6);
            Assert.Greater(bar.visibleItemCount, 0, "Leading items must win; the bar must not collapse entirely.");
            Assert.IsTrue(bar.ClassListContains(OverflowToolbar.overflowingUssClassName));

            var caret = bar.Q(className: OverflowToolbar.caretUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, caret.resolvedStyle.display);

            // Trailing items collapse first; leading items stay.
            for (var i = 0; i < bar.itemCount; i++)
            {
                var expected = i < bar.visibleItemCount ? DisplayStyle.Flex : DisplayStyle.None;
                Assert.AreEqual(expected, bar[i].resolvedStyle.display,
                    $"Item {i} has the wrong visibility for a visibleItemCount of {bar.visibleItemCount}.");
            }
        }

        [UnityTest]
        public IEnumerator TheInlineRun_AndTheCaret_BothFitInsideTheBar()
        {
            var bar = Build(6, 200f);
            yield return Mount(bar);
            Assert.IsTrue(bar.overflowing);
            Assert.Greater(bar.visibleItemCount, 0);

            var strip = bar.Q(className: OverflowToolbar.containerUssClassName);
            var caret = bar.Q(className: OverflowToolbar.caretUssClassName);

            // The caret lives outside the strip and never shrinks, so the strip is already narrower
            // than the bar by exactly the caret. Both halves of that have to hold.
            var lastInline = bar[bar.visibleItemCount - 1];
            Assert.LessOrEqual(lastInline.layout.xMax, strip.contentRect.width + 1f,
                $"The last inline item is clipped by the strip ({lastInline.layout.xMax} > " +
                $"{strip.contentRect.width}); one item too many stayed inline. " + Diagnostics(bar));
            Assert.LessOrEqual(caret.layout.xMax, bar.contentRect.width + 1f,
                $"The caret is pushed outside the bar ({caret.layout.xMax} > {bar.contentRect.width}); " +
                "the strip did not give up the room the caret needs. " + Diagnostics(bar));
        }

        [UnityTest]
        public IEnumerator NarrowingTheBar_CollapsesMoreItems()
        {
            var bar = Build(6, 600f);
            yield return Mount(bar);
            Assert.AreEqual(6, bar.visibleItemCount);

            bar.style.width = 200f;
            for (var i = 0; i < 6; i++)
                yield return null;

            Assert.Less(bar.visibleItemCount, 6,
                "The bar did not re-flow when it shrank; the GeometryChangedEvent hook is missing.");
        }

        [UnityTest]
        public IEnumerator WideningTheBar_BringsCollapsedItemsBackInline()
        {
            var bar = Build(6, 200f);
            yield return Mount(bar);
            var collapsed = bar.visibleItemCount;
            Assert.Less(collapsed, 6);

            bar.style.width = 600f;
            for (var i = 0; i < 6; i++)
                yield return null;

            Assert.AreEqual(6, bar.visibleItemCount, "Widening the bar must un-collapse the items.");
            Assert.IsFalse(bar.overflowing);
            var caret = bar.Q(className: OverflowToolbar.caretUssClassName);
            Assert.AreEqual(DisplayStyle.None, caret.resolvedStyle.display,
                "The caret survived the widening, so a menu with nothing in it is still offered.");
        }

        [UnityTest]
        public IEnumerator VisibleItemCountChanged_FiresOnlyWhenTheCountMoves()
        {
            var bar = Build(6, 600f);
            var raised = 0;
            var last = -1;
            bar.visibleItemCountChanged += v => { raised++; last = v; };
            yield return Mount(bar);

            var afterMount = raised;
            Assert.Greater(afterMount, 0);
            Assert.AreEqual(6, last);

            // A layout pass that changes nothing must not raise the event again.
            for (var i = 0; i < 5; i++)
                yield return null;
            Assert.AreEqual(afterMount, raised,
                "visibleItemCountChanged fired on a re-flow that changed nothing; the bar is not converging.");
        }

        [UnityTest]
        public IEnumerator OpeningTheOverflowMenu_MovesTheCollapsedItemsIntoIt()
        {
            var bar = Build(6, 200f);
            yield return Mount(bar);
            var visible = bar.visibleItemCount;
            var firstCollapsed = bar[visible];
            Assert.IsTrue(bar.overflowing);

            bar.OpenOverflowMenu();
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.IsTrue(bar.menuOpen);
            Assert.AreEqual(visible, bar.itemCount,
                "The collapsed items must leave the bar while the menu holds them.");
            Assert.IsNotNull(firstCollapsed.GetFirstAncestorOfType<Menu>(),
                "A collapsed item must be re-parented into the overflow Menu.");
            Assert.IsFalse(firstCollapsed.ClassListContains(Styles.hiddenUssClassName),
                "An item shown in the menu must not still carry the hidden marker.");

            bar.CloseOverflowMenu();
            for (var i = 0; i < 6; i++)
                yield return null;

            Assert.IsFalse(bar.menuOpen);
            Assert.AreEqual(6, bar.itemCount, "Dismissing the menu must return every item to the bar.");
            Assert.AreEqual("item0", bar[0].name);
            Assert.AreEqual("item5", bar[5].name, "The items came back in the wrong order.");
        }

        [UnityTest]
        public IEnumerator EmptyBar_ShowsNoCaret()
        {
            var bar = new OverflowToolbar { style = { width = 200f } };
            yield return Mount(bar);

            Assert.AreEqual(0, bar.itemCount);
            Assert.IsFalse(bar.overflowing);
            var caret = bar.Q(className: OverflowToolbar.caretUssClassName);
            Assert.AreEqual(DisplayStyle.None, caret.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator ItemGap_ShrinksUnderCompactDensity()
        {
            var normal = Build(4, 600f);
            yield return Mount(normal, "normal");
            var normalGap = normal[1].resolvedStyle.marginLeft;

            var compact = Build(4, 600f);
            yield return Mount(compact, "compact");
            var compactGap = compact[1].resolvedStyle.marginLeft;

            Assert.Greater(normalGap, 0f, "The inter-item gap resolved to zero; it is not token-driven. " + Diagnostics(normal));
            Assert.AreEqual(0f, normal[0].resolvedStyle.marginLeft, 0.01f,
                "The first item must not carry a leading gap.");
            Assert.LessOrEqual(compactGap, normalGap,
                $"The inter-item gap did not track density (normal {normalGap}, compact {compactGap}).");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var container = Regex.Match(uss, @"\.appui-overflow-toolbar__container\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(container.Success, "OverflowToolbar.uss has no container rule.");
            StringAssert.Contains("overflow: hidden", container.Groups["body"].Value,
                "The inline strip must clip, or an item mid-collapse spills out of the bar.");

            var gap = Regex.Match(uss, @"\.appui-overflow-toolbar__item\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(gap.Success, "OverflowToolbar.uss defines no inter-item gap rule.");
            Assert.IsTrue(Regex.IsMatch(gap.Groups["body"].Value, @"margin-left:\s*var\(--appui-sizing-\d+\)"),
                "The inter-item gap must be a density-aware --appui-sizing step, not a literal.");

            var rtl = Regex.Match(uss, @"\.appui--rtl\s+\.appui-overflow-toolbar\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rtl.Success, "OverflowToolbar.uss has no RTL rule, so the bar cannot mirror.");
            StringAssert.Contains("flex-direction: row-reverse", rtl.Groups["body"].Value);

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "OverflowToolbar.uss contains a magic pixel value.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "OverflowToolbar.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "OverflowToolbar.uss contains a literal rgb()/rgba() colour.");
        }

        static string Diagnostics(OverflowToolbar bar)
        {
            var container = bar.Q(className: OverflowToolbar.containerUssClassName);
            return $"[bar w={bar.layout.width} dir={bar.resolvedStyle.flexDirection} " +
                $"container w={container.layout.width} content={container.contentRect.width} " +
                $"grow={container.resolvedStyle.flexGrow} " +
                $"visible={bar.visibleItemCount} item0w={bar[0].layout.width} item1ml={bar[1].resolvedStyle.marginLeft}]";
        }

        IEnumerator Mount(OverflowToolbar bar, string density = "normal", Dir dir = Dir.Ltr)
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
            panel.Add(bar);

            for (var i = 0; i < 8; i++)
                yield return null;
        }
    }
}
