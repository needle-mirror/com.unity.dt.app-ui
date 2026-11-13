#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonBehaviourTests : UIComponentTests<ActionButton>
    {
        [Test]
        public void ActionButton_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new ActionButton { label = "Test" });
            button.clicked += () => clickCount++;

            simulate.Click(button);

            Assert.AreEqual(1, clickCount);
        }

        [Test]
        public void ActionButton_MouseDown_ShouldSetActiveState()
        {
            var button = MountElement(new ActionButton { label = "Test" });

            simulate.MouseDown(button);
            Assert.IsTrue(button.clickable.active, "The button should be active while pressed.");

            simulate.MouseUp(button);
            Assert.IsFalse(button.clickable.active, "The button should not be active once released.");
        }

        [Test]
        public void ActionButton_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new ActionButton { label = "Test" });
            button.clicked += () => clickCount++;

            button.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount);
        }

        [Test]
        public void ActionButton_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new ActionButton { label = "Test" });
            button.clicked += () => clickCount++;
            button.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(button);

            Assert.AreEqual(0, clickCount);
        }
    }
}
#endif
