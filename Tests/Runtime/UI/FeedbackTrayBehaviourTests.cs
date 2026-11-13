#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FeedbackTray))]
    class FeedbackTrayBehaviourTests : UIComponentTests<FeedbackTray>
    {
        [Test]
        public void FeedbackTray_ClickGoodButton_ShouldSetValue()
        {
            var tray = MountElement(new FeedbackTray());

            simulate.Click(tray.goodButton);

            Assert.AreEqual(FeedbackValue.Good, tray.value, "Clicking the good button should rate positively.");
        }

        [Test]
        public void FeedbackTray_ClickActiveRatingAgain_ShouldClearValue()
        {
            var tray = MountElement(new FeedbackTray());
            simulate.Click(tray.poorButton);
            Assert.AreEqual(FeedbackValue.Poor, tray.value, "Precondition: the poor rating should be active.");

            simulate.Click(tray.poorButton);

            Assert.AreEqual(FeedbackValue.None, tray.value, "Clicking the active rating again should clear it.");
        }

        [Test]
        public void FeedbackTray_ClickRatingButton_ShouldNotifyValueChanged()
        {
            var tray = MountElement(new FeedbackTray());
            var notifications = 0;
            tray.RegisterValueChangedCallback(_ => notifications++);

            simulate.Click(tray.goodButton);

            Assert.AreEqual(1, notifications, "Rating the tray should send exactly one ChangeEvent.");
        }

        [Test]
        public void FeedbackTray_ClickRefreshButton_ShouldInvokeRefreshRequested()
        {
            var tray = MountElement(new FeedbackTray { canRefresh = true });
            simulate.FrameUpdate();
            var requested = 0;
            tray.refreshRequested += () => requested++;

            simulate.Click(tray.refreshButton);

            Assert.AreEqual(1, requested, "Clicking the refresh button should raise refreshRequested.");
        }

        [Test]
        public void FeedbackTray_ClickCopyButton_ShouldInvokeCopyRequested()
        {
            var tray = MountElement(new FeedbackTray { canCopy = true });
            simulate.FrameUpdate();
            var requested = 0;
            tray.copyRequested += () => requested++;

            simulate.Click(tray.copyButton);

            Assert.AreEqual(1, requested, "Clicking the copy button should raise copyRequested.");
        }
    }
}
#endif
