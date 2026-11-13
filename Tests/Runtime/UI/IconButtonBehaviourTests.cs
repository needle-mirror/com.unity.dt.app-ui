#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(IconButton))]
    class IconButtonBehaviourTests : UIComponentTests<IconButton>
    {
        [Test]
        public void IconButton_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new IconButton("settings"));
            button.clicked += () => clickCount++;

            simulate.Click(button);

            Assert.AreEqual(1, clickCount, "Clicking the IconButton should invoke its clicked event.");
        }

        [Test]
        public void IconButton_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new IconButton("settings"));
            button.clicked += () => clickCount++;

            button.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting on a focused IconButton should invoke clicked.");
        }

        [Test]
        public void IconButton_MouseDown_ShouldSetActiveState()
        {
            var button = MountElement(new IconButton("settings"));

            simulate.MouseDown(button);
            Assert.IsTrue(button.clickable.active, "The button should be active while pressed.");

            simulate.MouseUp(button);
            Assert.IsFalse(button.clickable.active, "The button should not be active once released.");
        }

        [Test]
        public void IconButton_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var button = MountElement(new IconButton("settings"));
            button.clicked += () => clickCount++;
            button.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(button);

            Assert.AreEqual(0, clickCount, "A disabled IconButton should not invoke clicked.");
        }

        // ── selected (registry #1392) ──────────────────────────────────────

        Color BackgroundOf(IconButton button, bool hovered)
        {
            // Park the pointer on the button or well away from it, and let the transition settle.
            simulate.MouseMove(hovered ? button.worldBound.center : button.worldBound.max + new Vector2(60f, 60f));
            simulate.FrameUpdate();
            Assert.AreEqual(hovered, button.ClassListContains(Styles.hoveredUssClassName),
                "Positive control: the pointer should drive the hovered state.");
            simulate.FrameUpdateMs(1000);
            return button.resolvedStyle.backgroundColor;
        }

        [Test]
        public void IconButton_Selected_PaintsTheSelectedFill_AndItsOwnHover()
        {
            var plain = MountElement(new IconButton("settings"));
            var plainRest = BackgroundOf(plain, false);
            var plainHover = BackgroundOf(plain, true);

            var selected = MountElement(new IconButton("settings") { selected = true });
            var selectedRest = BackgroundOf(selected, false);
            var selectedHover = BackgroundOf(selected, true);

            Assert.Greater(selectedRest.a, 0f, "A selected IconButton should paint a fill.");
            Assert.AreNotEqual(plainRest, selectedRest, "Selected should paint differently from unselected.");
            Assert.AreNotEqual(selectedRest, selectedHover,
                "Hovering a selected IconButton should paint the selected hover fill.");
            Assert.AreNotEqual(plainHover, selectedHover,
                "Hovering must keep the pressed state, not fall back to the unselected hover fill.");
        }

        [Test]
        public void IconButton_Selected_ClickDoesNotToggleIt()
        {
            var button = MountElement(new IconButton("settings") { selected = true });

            simulate.Click(button);

            Assert.IsTrue(button.selected, "selected is controlled by the host, as in the registry.");
        }
    }
}
#endif
