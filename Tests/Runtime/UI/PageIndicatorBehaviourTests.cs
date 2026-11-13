#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PageIndicator))]
    class PageIndicatorBehaviourTests : UIComponentTests<PageIndicator>
    {
        PageIndicator MountIndicator() => MountElement(new PageIndicator { count = 3 });

        static VisualElement DotAt(PageIndicator indicator, int index) => indicator.Q(name: $"dot-{index}");

        [Test]
        public void PageIndicator_ClickOnDot_ShouldChangeValue()
        {
            var indicator = MountIndicator();
            Assert.AreEqual(0, indicator.value, "The indicator should start on its first dot.");

            simulate.Click(DotAt(indicator, 2));

            Assert.AreEqual(2, indicator.value, "Clicking a dot should select it.");
        }

        [Test]
        public void PageIndicator_ClickOnDot_ShouldSendChangeEvent()
        {
            var indicator = MountIndicator();
            var newValue = -1;
            indicator.RegisterCallback<ChangeEvent<int>>(evt => newValue = evt.newValue);

            simulate.Click(DotAt(indicator, 1));

            Assert.AreEqual(1, newValue, "Clicking a dot should send a ChangeEvent with its index.");
        }

        [Test]
        public void PageIndicator_RightArrowOnFocusedDot_ShouldGoToNext()
        {
            var indicator = MountIndicator();
            DotAt(indicator, 0).Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);

            Assert.AreEqual(1, indicator.value, "Right arrow in a horizontal indicator should move to the next dot.");
        }

        [Test]
        public void PageIndicator_LeftArrowAtFirstDot_ShouldNotChangeValue()
        {
            var indicator = MountIndicator();
            DotAt(indicator, 0).Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.LeftArrow);

            Assert.AreEqual(0, indicator.value, "Left arrow at the first dot should not go out of range.");
        }
    }
}
#endif
