#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Button))]
    class ButtonBehaviourTests : UIComponentTests<Button>
    {
        [Test]
        public void Button_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new Button { title = "Test" });
            button.clicked += () => clickCount++;

            simulate.Click(button);

            Assert.AreEqual(1, clickCount, "Clicking the Button should invoke its clicked event.");
        }

        [Test]
        public void Button_MouseDown_ShouldSetActiveState()
        {
            var button = MountElement(new Button { title = "Test" });

            simulate.MouseDown(button);
            Assert.IsTrue(button.clickable.active, "The Button should be active while pressed.");

            simulate.MouseUp(button);
            Assert.IsFalse(button.clickable.active, "The Button should not be active once released.");
        }

        [Test]
        public void Button_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new Button { title = "Test" });
            button.clicked += () => clickCount++;

            button.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting on a focused Button should invoke clicked.");
        }

        [Test]
        public void Button_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new Button { title = "Test" });
            button.clicked += () => clickCount++;
            button.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(button);

            Assert.AreEqual(0, clickCount, "A disabled Button should not fire clicked.");
        }
    }
}
#endif
