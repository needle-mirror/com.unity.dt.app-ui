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
    [TestOf(typeof(Sidebar))]
    class SidebarTests : VisualElementTests<Sidebar>
    {
        protected override string mainUssClassName => Sidebar.ussClassName;

        // Handed over to SidebarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Sidebar.uss";

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildSidebar("assets"));
                yield return new Story("NoSelection", (ctx) => BuildSidebar(null));
            }
        }

        static Sidebar BuildSidebar(string selected)
        {
            var sidebar = new Sidebar { selectedId = selected };

            var header = new SidebarHeader();
            header.Add(new Text { text = "My Project" });
            sidebar.Add(header);

            var content = new SidebarContent();
            var section = new SidebarSection { heading = "Workspace" };
            section.Add(new SidebarItem { value = "assets", label = "Assets", icon = "folder" });
            section.Add(new SidebarItem { value = "scenes", label = "Scenes", icon = "stack", shortcut = "Ctrl+2" });
            content.Add(section);
            sidebar.Add(content);

            var footer = new SidebarFooter();
            footer.Add(new SidebarItem { label = "Settings", icon = "gear" });
            sidebar.Add(footer);

            return sidebar;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Sidebar />",
            @"<appui:Sidebar selected-id=""assets"" width=""200"" />",
            @"<appui:SidebarItem value=""a"" label=""Assets"" icon=""folder"" shortcut=""Ctrl+1"" active=""true"" />",
            @"<appui:Sidebar><appui:SidebarHeader /><appui:SidebarContent><appui:SidebarSection heading=""Workspace""><appui:SidebarItem value=""assets"" label=""Assets"" /></appui:SidebarSection></appui:SidebarContent><appui:SidebarFooter /></appui:Sidebar>",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var sidebar = new Sidebar();
            Assert.IsNull(sidebar.selectedId);
            Assert.AreEqual(Sidebar.defaultWidth, sidebar.width);

            var item = new SidebarItem();
            Assert.IsNull(item.value);
            Assert.IsFalse(item.active, "A SidebarItem must not start active.");
        }

        [Test]
        public void SubElements_CarryTheirBemClassNames()
        {
            Assert.IsTrue(new SidebarHeader().ClassListContains(SidebarHeader.ussClassName));
            Assert.IsTrue(new SidebarContent().ClassListContains(SidebarContent.ussClassName));
            Assert.IsTrue(new SidebarFooter().ClassListContains(SidebarFooter.ussClassName));
            Assert.IsTrue(new SidebarSection().ClassListContains(SidebarSection.ussClassName));
            Assert.IsTrue(new SidebarSectionHeading().ClassListContains(SidebarSectionHeading.ussClassName));
            Assert.IsTrue(new SidebarItem().ClassListContains(SidebarItem.ussClassName));
        }

        [Test]
        public void Section_AddsAndRemovesItsHeadingElement()
        {
            var section = new SidebarSection();
            Assert.IsNull(section.heading);
            Assert.AreEqual(1, section.hierarchy.childCount, "A headingless section must only hold its container.");

            section.heading = "Workspace";
            Assert.AreEqual(2, section.hierarchy.childCount);
            Assert.IsInstanceOf<SidebarSectionHeading>(section.hierarchy[0],
                "The heading must be inserted BEFORE the item container.");
            Assert.AreEqual("Workspace", section.heading);

            section.heading = null;
            Assert.AreEqual(1, section.hierarchy.childCount, "Clearing the heading must remove the element.");
            Assert.IsNull(section.heading);
        }

        [Test]
        public void Section_AddsItemsToItsContainerNotBesideTheHeading()
        {
            var section = new SidebarSection { heading = "Workspace" };
            var item = new SidebarItem { value = "a" };
            section.Add(item);

            Assert.AreEqual(1, section.childCount);
            Assert.AreEqual(section.contentContainer, item.hierarchy.parent);
            Assert.AreNotEqual(section, item.hierarchy.parent,
                "Items must land in the section's container, not next to the heading.");
        }

        [UnityTest]
        public IEnumerator SelectedId_MarksTheMatchingItemActiveAndOnlyThatOne()
        {
            var sidebar = BuildSidebar("assets");
            yield return Mount(sidebar);

            var items = sidebar.Query<SidebarItem>().ToList();
            Assert.AreEqual(3, items.Count);

            Assert.IsTrue(items[0].active, "The item matching selectedId must be active.");
            Assert.IsFalse(items[1].active, "A non-matching valued item must not be active.");
            Assert.IsFalse(items[2].active, "A valueless action item must never be activated by selection.");
        }

        [UnityTest]
        public IEnumerator ChangingSelectedId_MovesTheActiveStateAndLeavesNoStaleOne()
        {
            var sidebar = BuildSidebar("assets");
            yield return Mount(sidebar);
            var items = sidebar.Query<SidebarItem>().ToList();
            Assert.IsTrue(items[0].active);

            sidebar.selectedId = "scenes";
            yield return null;

            Assert.IsTrue(items[1].active, "The newly selected item must become active.");
            Assert.IsFalse(items[0].active,
                "The previously selected item kept its active state, so two rows paint as current.");
        }

        [UnityTest]
        public IEnumerator ClickingAValuedItem_SelectsItAndRaisesSelectionChanged()
        {
            var sidebar = BuildSidebar(null);
            yield return Mount(sidebar);
            var items = sidebar.Query<SidebarItem>().ToList();

            string raised = null;
            sidebar.selectionChanged += id => raised = id;

            items[1].clickable.InvokePressed(null);
            yield return null;

            Assert.AreEqual("scenes", sidebar.selectedId);
            Assert.AreEqual("scenes", raised, "selectionChanged did not fire with the item's value.");
            Assert.IsTrue(items[1].active);
        }

        [UnityTest]
        public IEnumerator ClickingAValuelessItem_LeavesTheSelectionAlone()
        {
            var sidebar = BuildSidebar("assets");
            yield return Mount(sidebar);
            var items = sidebar.Query<SidebarItem>().ToList();

            var raisedCount = 0;
            sidebar.selectionChanged += _ => raisedCount++;

            items[2].clickable.InvokePressed(null);
            yield return null;

            Assert.AreEqual("assets", sidebar.selectedId,
                "A valueless action item must not change the sidebar's selection.");
            Assert.AreEqual(0, raisedCount);
        }

        [UnityTest]
        public IEnumerator Items_HideTheirIconAndShortcutWhenUnset()
        {
            var sidebar = BuildSidebar("assets");
            yield return Mount(sidebar);
            var items = sidebar.Query<SidebarItem>().ToList();

            var iconless = items[2].Q(className: SidebarItem.iconUssClassName);
            Assert.IsNotNull(iconless);

            var noShortcut = items[0].Q(className: SidebarItem.shortcutUssClassName);
            Assert.AreEqual(DisplayStyle.None, noShortcut.resolvedStyle.display,
                "An item with no shortcut must not reserve room for one.");

            var withShortcut = items[1].Q(className: SidebarItem.shortcutUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, withShortcut.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator FirstChildMarkers_ZeroTheLeadingGapAndFollowInsertions()
        {
            var sidebar = new Sidebar();
            var a = new SidebarHeader();
            var b = new SidebarContent();
            sidebar.Add(a);
            sidebar.Add(b);
            yield return Mount(sidebar);

            Assert.IsTrue(a.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(b.ClassListContains(Styles.firstChildUssClassName));
            Assert.AreEqual(0f, a.resolvedStyle.marginTop, 0.01f,
                "The first slot must not carry a leading gap.");
            Assert.Greater(b.resolvedStyle.marginTop, 0f,
                "The gap between slots is a leading margin and it resolved to zero.");

            var c = new SidebarHeader();
            sidebar.Insert(0, c);
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.IsTrue(c.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(a.ClassListContains(Styles.firstChildUssClassName),
                "The old first child kept its marker, so the top of the sidebar loses its gap.");
        }

        [UnityTest]
        public IEnumerator ItemHeight_ComesFromTheControlSizingAlias_AndShrinksUnderCompactDensity()
        {
            var normal = BuildSidebar("assets");
            yield return Mount(normal, "normal");
            var normalHeight = normal.Q<SidebarItem>().resolvedStyle.height;

            var compact = BuildSidebar("assets");
            yield return Mount(compact, "compact");
            var compactHeight = compact.Q<SidebarItem>().resolvedStyle.height;

            Assert.Greater(normalHeight, 0f);
            Assert.Less(compactHeight, normalHeight,
                $"The item height did not shrink under compact density (normal {normalHeight}, compact {compactHeight}). " +
                "It is probably a hardcoded pixel value instead of the control-sizing alias.");
        }

        [UnityTest]
        public IEnumerator Width_DefaultsToTheRegistryWidthAndIsSettable()
        {
            var sidebar = new Sidebar();
            yield return Mount(sidebar);
            Assert.AreEqual(Sidebar.defaultWidth, sidebar.resolvedStyle.width, 1f);

            sidebar.width = 180f;
            for (var i = 0; i < 3; i++)
                yield return null;
            Assert.AreEqual(180f, sidebar.resolvedStyle.width, 1f);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_BindsSizesToTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var item = Regex.Match(uss, @"\.appui-sidebar__item\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(item.Success, "Sidebar.uss has no .appui-sidebar__item rule.");
            StringAssert.Contains("var(--appui-alias-control-sizing-md-height)", item.Groups["body"].Value,
                "The item height must come from the medium control-sizing alias, not a raw sizing step.");
            Assert.IsFalse(item.Groups["body"].Value.Contains("--appui-sizing-4"),
                "The item height grabbed a raw --appui-sizing step; density rescaling then breaks silently.");

            var icon = Regex.Match(uss, @"\.appui-sidebar__item-icon\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(icon.Success);
            StringAssert.Contains("var(--appui-alias-control-sizing-icon-md)", icon.Groups["body"].Value,
                "The glyph must be sized by the control-sizing icon alias.");

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "Sidebar.uss contains a magic pixel value.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Sidebar.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Sidebar.uss contains a literal rgb()/rgba() colour.");
            var root = Regex.Match(uss, @"\.appui-sidebar\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(root.Success, "Sidebar.uss has no .appui-sidebar rule.");
            Assert.IsFalse(Regex.IsMatch(root.Groups["body"].Value, @"(?<![-\w])width:"),
                "Sidebar.uss must not set the sidebar width; Sidebar.width writes it inline instead, " +
                "because the registry's 240px has no App UI sizing token.");
        }

        IEnumerator Mount(Sidebar sidebar, string density = "normal", Dir dir = Dir.Ltr)
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
            panel.Add(sidebar);

            for (var i = 0; i < 5; i++)
                yield return null;
        }
    }
}
