#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TabItem))]
    class TabItemBehaviourTests : UIComponentTests<TabItem>
    {
        TabItem MountTabItem() => MountElement(new TabItem { label = "Tab" });

        [Test]
        public void TabItem_Click_ShouldSendActionTriggeredEvent()
        {
            var item = MountTabItem();
            var notifications = 0;
            item.RegisterCallback<ActionTriggeredEvent>(_ => notifications++);

            simulate.Click(item);

            Assert.AreEqual(1, notifications, "Clicking a TabItem should send an ActionTriggeredEvent.");
        }

        [Test]
        public void TabItem_SubmitKey_ShouldSendActionTriggeredEvent()
        {
            var item = MountTabItem();
            var notifications = 0;
            item.RegisterCallback<ActionTriggeredEvent>(_ => notifications++);

            item.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, notifications, "Submitting on a focused TabItem should send an ActionTriggeredEvent.");
        }

        [Test]
        public void TabItem_WhenDisabled_Click_ShouldNotSendActionTriggeredEvent()
        {
            var item = MountTabItem();
            var notifications = 0;
            item.RegisterCallback<ActionTriggeredEvent>(_ => notifications++);
            item.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(item);

            Assert.AreEqual(0, notifications, "A disabled TabItem should not react to clicks.");
        }

        [Test]
        public void TabItem_Click_ShouldNotChangeSelectedState()
        {
            // Selection is owned by the enclosing Tabs; a bare TabItem's own click handler only
            // raises ActionTriggeredEvent and never flips its own selected state.
            var item = MountTabItem();

            simulate.Click(item);

            Assert.IsFalse(item.selected, "A standalone TabItem should not select itself on click.");
        }
    }
}
#endif
