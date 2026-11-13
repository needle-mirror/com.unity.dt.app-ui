#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BottomNavBar))]
    class BottomNavBarBehaviourTests : UIComponentTests<BottomNavBar>
    {
        BottomNavBarItem m_Home;
        BottomNavBarItem m_Search;

        BottomNavBar MountBottomNavBar()
        {
            m_Home = new BottomNavBarItem("home", "Home", null) { isSelected = true };
            m_Search = new BottomNavBarItem("search", "Search", null);
            var bar = new BottomNavBar();
            bar.Add(m_Home);
            bar.Add(m_Search);
            return MountElement(bar);
        }

        [Test]
        public void BottomNavBar_ClickItem_ShouldInvokeThatItemClickHandler()
        {
            MountBottomNavBar();
            // MountElement's two passes settle the BottomNavBar container itself -- the element
            // it actually asserts layout for -- but m_Search is a grandchild (bar -> item -> icon
            // /label) added before the mount. Its own box only becomes final once that extra level
            // of resolved styles (icon/label sizing) has cascaded back up into the item's own
            // layout, so give it one more frame before trusting m_Search.worldBound.center for a
            // position-based click.
            simulate.FrameUpdate();
            Assert.Greater(m_Search.worldBound.width, 0f,
                "m_Search has no resolved layout, so simulate.Click would silently miss it.");
            var notified = 0;
            m_Search.clickable.clicked += () => notified++;

            simulate.Click(m_Search);

            Assert.AreEqual(1, notified, "Clicking a BottomNavBarItem should invoke its own click handler.");
        }

        [Test]
        public void BottomNavBar_ClickOneItem_ShouldNotChangeAnotherItemsSelectionState()
        {
            // The container does not coordinate selection between its items -- unlike Tabs or an
            // exclusive Accordion -- so each BottomNavBarItem's isSelected stays under the
            // consumer's own control.
            MountBottomNavBar();

            simulate.Click(m_Search);

            Assert.IsTrue(m_Home.isSelected,
                "BottomNavBar has no built-in exclusivity, so clicking one item should not " +
                "deselect another on its own.");
            Assert.IsFalse(m_Search.isSelected,
                "Clicking an item does not select it by itself -- that is left to the click handler.");
        }

        [Test]
        public void BottomNavBar_WhenItemDisabled_Click_ShouldNotInvokeClickHandler()
        {
            MountBottomNavBar();
            m_Search.SetEnabled(false);
            simulate.FrameUpdate();
            var notified = 0;
            m_Search.clickable.clicked += () => notified++;

            simulate.Click(m_Search);

            Assert.AreEqual(0, notified, "A disabled BottomNavBarItem should not react to clicks.");
        }
    }
}
#endif
