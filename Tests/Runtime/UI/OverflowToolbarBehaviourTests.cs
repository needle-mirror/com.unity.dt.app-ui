#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(OverflowToolbar))]
    class OverflowToolbarBehaviourTests : UIComponentTests<OverflowToolbar>
    {
        static VisualElement CaretOf(OverflowToolbar toolbar) => toolbar.Q(name: OverflowToolbar.caretUssClassName);

        OverflowToolbar MountNarrowToolbar()
        {
            // Each ActionButton is wide enough on its own that four of them cannot possibly fit in
            // 120px, forcing the bar to collapse the trailing ones into the caret menu.
            var toolbar = new OverflowToolbar { style = { width = 120 } };
            for (var i = 0; i < 4; i++)
                toolbar.Add(new ActionButton { label = $"Button {i}" });

            var mounted = MountElement(toolbar);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            return mounted;
        }

        [Test]
        public void OverflowToolbar_WhenTooNarrow_ShouldCollapseOverflowingItems()
        {
            var toolbar = MountNarrowToolbar();

            Assert.IsTrue(toolbar.overflowing, "A bar too narrow for all its items should report overflowing.");
            Assert.Less(toolbar.visibleItemCount, toolbar.itemCount,
                "Fewer items than the total should remain visible once the bar has collapsed some.");
        }

        [Test]
        public void OverflowToolbar_ClickCaret_ShouldOpenMenu()
        {
            var toolbar = MountNarrowToolbar();
            var caret = CaretOf(toolbar);
            Assert.IsNotNull(caret, "An overflowing bar should show its caret.");
            Assert.IsFalse(toolbar.menuOpen, "The overflow menu should not start open.");

            simulate.Click(caret);

            Assert.IsTrue(toolbar.menuOpen, "Clicking the caret should open the overflow menu.");
        }

        [Test]
        public void OverflowToolbar_CloseOverflowMenu_ShouldRestoreCollapsedItems()
        {
            var toolbar = MountNarrowToolbar();
            var visibleBefore = toolbar.visibleItemCount;
            simulate.Click(CaretOf(toolbar));
            Assert.IsTrue(toolbar.menuOpen, "The menu should be open after clicking the caret.");

            // The popover's show animation sets its view visible on its own scheduled pass. Dismiss
            // guards its fade-out on that visibility flag, so closing before the popover has become
            // visible drops the dismiss entirely and the menu would appear stuck open forever.
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            toolbar.CloseOverflowMenu();
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.IsFalse(toolbar.menuOpen, "Closing the overflow menu should clear its open state.");
            Assert.AreEqual(visibleBefore, toolbar.visibleItemCount,
                "Closing the menu should re-flow back to the same visible count.");
        }
    }
}
#endif
