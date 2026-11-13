#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Sidebar))]
    class SidebarBehaviourTests : UIComponentTests<Sidebar>
    {
        Sidebar MountSidebar(out SidebarItem assets, out SidebarItem scenes, out SidebarItem settings)
        {
            var sidebar = new Sidebar { style = { height = 300 } };
            var content = new SidebarContent();
            var section = new SidebarSection { heading = "Workspace" };
            assets = new SidebarItem { value = "assets", label = "Assets" };
            scenes = new SidebarItem { value = "scenes", label = "Scenes" };
            section.Add(assets);
            section.Add(scenes);
            content.Add(section);
            sidebar.Add(content);

            var footer = new SidebarFooter();
            settings = new SidebarItem { label = "Settings" };
            footer.Add(settings);
            sidebar.Add(footer);

            return MountElement(sidebar);
        }

        [Test]
        public void Sidebar_ClickOnItem_ShouldSetSelectedIdAndNotify()
        {
            var sidebar = MountSidebar(out var assets, out var scenes, out _);
            string notified = null;
            sidebar.selectionChanged += id => notified = id;

            simulate.Click(scenes);

            Assert.AreEqual("scenes", sidebar.selectedId, "Clicking a valued item should select it.");
            Assert.AreEqual("scenes", notified, "selectionChanged should carry the clicked item's value.");
            Assert.IsTrue(scenes.active, "The clicked item should paint itself active.");
            Assert.IsFalse(assets.active, "The previously selected item should no longer be active.");
        }

        [Test]
        public void Sidebar_ClickOnValuelessItem_ShouldRaiseClickedButNotChangeSelection()
        {
            var sidebar = MountSidebar(out var assets, out _, out var settings);
            sidebar.selectedId = "assets";
            var clickCount = 0;
            settings.clicked += () => clickCount++;
            var notifications = 0;
            sidebar.selectionChanged += _ => notifications++;

            simulate.Click(settings);

            Assert.AreEqual(1, clickCount, "A valueless SidebarItem should still raise clicked.");
            Assert.AreEqual(0, notifications, "A valueless item must never change the Sidebar's selection.");
            Assert.AreEqual("assets", sidebar.selectedId);
            Assert.IsTrue(assets.active, "The previously selected item should remain active.");
        }

        [Test]
        public void Sidebar_WhenItemDisabled_Click_ShouldNotSelect()
        {
            var sidebar = MountSidebar(out _, out var scenes, out _);
            scenes.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(scenes);

            Assert.IsNull(sidebar.selectedId, "A disabled item should not become selected.");
        }
    }
}
#endif
