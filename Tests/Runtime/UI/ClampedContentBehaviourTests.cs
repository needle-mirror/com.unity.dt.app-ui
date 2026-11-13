#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ClampedContent))]
    class ClampedContentBehaviourTests : UIComponentTests<ClampedContent>
    {
        static VisualElement ToggleOf(ClampedContent clamped) =>
            clamped.Q(name: ClampedContent.toggleUssClassName);

        ClampedContent MountOverflowing()
        {
            var clamped = new ClampedContent
            {
                maxHeight = 50f,
                style = { width = 200 },
            };
            // A tall child guarantees the container measures past maxHeight, so the toggle appears.
            clamped.Add(new VisualElement { style = { height = 400 } });
            return MountElement(clamped);
        }

        [Test]
        public void ClampedContent_ToggleClick_ShouldExpand()
        {
            var clamped = MountOverflowing();
            simulate.FrameUpdate();
            Assert.IsTrue(clamped.overflowing, "The tall child should overflow the 50px cap.");
            Assert.IsFalse(clamped.expanded, "The content should start collapsed.");

            simulate.Click(ToggleOf(clamped));

            Assert.IsTrue(clamped.expanded, "Clicking the toggle should expand the content.");
        }

        [Test]
        public void ClampedContent_ToggleClick_ShouldRaiseExpandedChanged()
        {
            var clamped = MountOverflowing();
            simulate.FrameUpdate();
            var notifications = 0;
            clamped.expandedChanged += _ => notifications++;

            simulate.Click(ToggleOf(clamped));

            Assert.AreEqual(1, notifications, "A toggle click should raise expandedChanged exactly once.");
        }

        [Test]
        public void ClampedContent_ToggleClickTwice_ShouldCollapseAgain()
        {
            var clamped = MountOverflowing();
            simulate.FrameUpdate();

            simulate.Click(ToggleOf(clamped));
            Assert.IsTrue(clamped.expanded, "The first click should expand the content.");

            simulate.Click(ToggleOf(clamped));
            Assert.IsFalse(clamped.expanded, "The second click should collapse the content again.");
        }

        [Test]
        public void ClampedContent_ToggleFocus_SubmitKey_ShouldExpand()
        {
            var clamped = MountOverflowing();
            simulate.FrameUpdate();
            var toggle = ToggleOf(clamped);

            toggle.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsTrue(clamped.expanded, "Submitting on the focused toggle should expand the content.");
        }
    }
}
#endif
