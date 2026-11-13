#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatingActionButton))]
    class FloatingActionButtonBehaviourTests : UIComponentTests<FloatingActionButton>
    {
        [Test]
        public void FloatingActionButton_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var fab = MountElement(new FloatingActionButton { style = { width = 56, height = 56 } });
            fab.clicked += () => clickCount++;

            simulate.Click(fab);

            Assert.AreEqual(1, clickCount, "Clicking the FloatingActionButton should invoke its clicked event.");
        }

        [Test]
        public void FloatingActionButton_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var fab = MountElement(new FloatingActionButton { style = { width = 56, height = 56 } });
            fab.clicked += () => clickCount++;

            fab.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting on a focused FloatingActionButton should invoke clicked.");
        }

        [Test]
        public void FloatingActionButton_MouseDown_ShouldSetActiveState()
        {
            var fab = MountElement(new FloatingActionButton { style = { width = 56, height = 56 } });

            simulate.MouseDown(fab);
            Assert.IsTrue(fab.clickable.active, "The FAB should be active while pressed.");

            simulate.MouseUp(fab);
            Assert.IsFalse(fab.clickable.active, "The FAB should not be active once released.");
        }

        [Test]
        public void FloatingActionButton_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var fab = MountElement(new FloatingActionButton { style = { width = 56, height = 56 } });
            fab.clicked += () => clickCount++;
            fab.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(fab);

            Assert.AreEqual(0, clickCount, "A disabled FloatingActionButton should not invoke clicked.");
        }
    }
}
#endif
