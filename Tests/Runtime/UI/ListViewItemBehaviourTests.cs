#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ListViewItem))]
    class ListViewItemBehaviourTests : UIComponentTests<ListViewItem>
    {
        ListViewItem MountItem() => MountElement(new ListViewItem
        {
            title = "Project Assets",
            subtitle = "Contains 42 files",
            style = { width = 240 },
        });

        [Test]
        public void ListViewItem_OptionsButtonClick_ShouldInvokeClicked()
        {
            var item = MountItem();
            var clickCount = 0;
            item.optionsButton.clicked += () => clickCount++;

            simulate.Click(item.optionsButton);

            Assert.AreEqual(1, clickCount, "Clicking the options button should invoke its clicked event.");
        }

        [Test]
        public void ListViewItem_Focus_ShouldBecomeFocused()
        {
            var item = MountItem();
            Assert.IsTrue(item.focusable, "A non-loading ListViewItem should be focusable.");

            item.Focus();
            simulate.FrameUpdate();

            Assert.IsTrue(item.panel.focusController.focusedElement == item,
                "Focusing the ListViewItem should make it the panel's focused element.");
        }

        [Test]
        public void ListViewItem_WhenLoading_ShouldNotBeFocusable()
        {
            var item = MountItem();

            item.isLoading = true;
            simulate.FrameUpdate();

            Assert.IsFalse(item.focusable, "A loading ListViewItem should not be focusable.");
            Assert.AreEqual(-1, item.tabIndex, "A loading ListViewItem should be removed from tab order.");
        }
    }
}
#endif
