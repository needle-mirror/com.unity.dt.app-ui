#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SplitButton))]
    class SplitButtonBehaviourTests : UIComponentTests<SplitButton>
    {
        SplitButton MountSplitButton() => MountElement(new SplitButton { label = "Play" });

        [Test]
        public void SplitButton_ClickMain_ShouldInvokeClicked()
        {
            var splitButton = MountSplitButton();
            var clickCount = 0;
            splitButton.clicked += () => clickCount++;

            simulate.Click(splitButton.mainElement);

            Assert.AreEqual(1, clickCount, "Clicking the main part should invoke clicked.");
        }

        [Test]
        public void SplitButton_ClickCaret_ShouldInvokeCaretClickedOnly()
        {
            var splitButton = MountSplitButton();
            // MountElement's two passes settle the SplitButton root's own box -- what it actually
            // asserts layout for -- but the caret is a fixed-width child whose height only comes
            // from the root's own size class stretching down through it. Give that one more frame
            // before trusting caretElement.worldBound.center for a position-based click.
            simulate.FrameUpdate();
            Assert.Greater(splitButton.caretElement.worldBound.width, 0f,
                "caretElement has no resolved layout, so simulate.Click would silently miss it.");
            Assert.Greater(splitButton.caretElement.worldBound.height, 0f,
                "caretElement has no resolved layout, so simulate.Click would silently miss it.");
            var clickCount = 0;
            var caretClickCount = 0;
            splitButton.clicked += () => clickCount++;
            splitButton.caretClicked += () => caretClickCount++;

            simulate.Click(splitButton.caretElement);

            Assert.AreEqual(0, clickCount, "Clicking the caret must not run the main action.");
            Assert.AreEqual(1, caretClickCount, "Clicking the caret should invoke caretClicked.");
        }

        [Test]
        public void SplitButton_SubmitKeyOnMain_ShouldInvokeClicked()
        {
            var splitButton = MountSplitButton();
            var clickCount = 0;
            splitButton.clicked += () => clickCount++;

            splitButton.mainElement.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting while the main part is focused should invoke clicked.");
        }

        [Test]
        public void SplitButton_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var splitButton = MountSplitButton();
            var clickCount = 0;
            splitButton.clicked += () => clickCount++;
            splitButton.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(splitButton.mainElement);

            Assert.AreEqual(0, clickCount, "A disabled SplitButton should not invoke clicked.");
        }
    }
}
#endif
