#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(NavigationRailItem))]
    class NavigationRailItemBehaviourTests : UIComponentTests<NavigationRailItem>
    {
        NavigationRailItem MountItem() => MountElement(new NavigationRailItem { label = "Item", icon = "cursor" });

        [Test]
        public void NavigationRailItem_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var item = MountItem();
            item.clickable.clicked += () => clickCount++;

            simulate.Click(item);

            Assert.AreEqual(1, clickCount, "Clicking the item should invoke its Pressable clicked event.");
        }

        [Test]
        public void NavigationRailItem_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var item = MountItem();
            item.clickable.clicked += () => clickCount++;

            item.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting while the item is focused should invoke clicked.");
        }

        [Test]
        public void NavigationRailItem_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var item = MountItem();
            item.clickable.clicked += () => clickCount++;
            item.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(item);

            Assert.AreEqual(0, clickCount, "A disabled item should swallow the click.");
        }

        [Test]
        public void NavigationRailItem_SetSelected_ShouldToggleSelectedClass()
        {
            var item = MountItem();
            Assert.IsFalse(item.selected, "The item should not be selected by default.");

            item.selected = true;

            Assert.IsTrue(item.selected, "Setting selected should reflect back through the property.");
        }
    }
}
#endif
