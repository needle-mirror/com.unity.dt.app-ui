#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ThreadMessage))]
    class ThreadMessageBehaviourTests : UIComponentTests<ThreadMessage>
    {
        static VisualElement LikeButtonOf(ThreadMessage message) => message.Q(name: ThreadMessage.likeBtnUssClassName);

        static VisualElement DislikeButtonOf(ThreadMessage message) => message.Q(name: ThreadMessage.dislikeBtnUssClassName);

        static VisualElement ActionsButtonOf(ThreadMessage message) => message.Q(name: ThreadMessage.actionButtonUssClassName);

        ThreadMessage MountMessage() => MountElement(new ThreadMessage
        {
            authorName = "Ada",
            message = "Hello there",
            style = { width = 300 },
        });

        [Test]
        public void ThreadMessage_ClickLikeButton_ShouldToggleIsLikedAndNotify()
        {
            var message = MountMessage();
            var notified = new System.Collections.Generic.List<bool>();
            message.likeToggled += notified.Add;

            // The like/dislike buttons are hidden until a ThreadContext enabling them reaches the
            // message (normally provided by an ancestor Thread), so provide one directly and let
            // layout catch up before clicking a position-based target.
            message.ProvideContext(new ThreadContext(true, true, true, true, null));
            simulate.FrameUpdate();

            simulate.Click(LikeButtonOf(message));

            Assert.IsTrue(message.isLiked, "Clicking the like button should like the message.");
            CollectionAssert.AreEqual(new[] { true }, notified, "likeToggled should carry the new state.");

            simulate.Click(LikeButtonOf(message));

            Assert.IsFalse(message.isLiked, "Clicking the like button again should un-like the message.");
        }

        [Test]
        public void ThreadMessage_ClickDislikeButton_ShouldToggleIsDislikedAndNotify()
        {
            var message = MountMessage();
            var notified = new System.Collections.Generic.List<bool>();
            message.dislikeToggled += notified.Add;

            // Same as the like button: needs an enabling ThreadContext and a layout pass before
            // the dislike button has a real worldBound to click.
            message.ProvideContext(new ThreadContext(true, true, true, true, null));
            simulate.FrameUpdate();

            simulate.Click(DislikeButtonOf(message));

            Assert.IsTrue(message.isDisliked, "Clicking the dislike button should dislike the message.");
            CollectionAssert.AreEqual(new[] { true }, notified, "dislikeToggled should carry the new state.");
        }

        [Test]
        public void ThreadMessage_ClickActionsButton_WithMenuItems_ShouldOpenActionsMenu()
        {
            var message = MountMessage();
            message.makeActionMenuItems = (_, menu) => menu.AddAction(0, item => item.label = "Delete");

            // The actions row is `display: none` until the message is hovered -- see the
            // `.appui-thread-message:hover > ... > .appui-thread-message__actions` rule in
            // ThreadMessage.uss. A hidden row has no worldBound, so a position-based click would
            // land on whatever is underneath it. Hover first, then let layout catch up, so the
            // button is actually on screen when the click is sent.
            // Vector2 overload on purpose: MouseMove(VisualElement) only exists in
            // com.unity.ui.test-framework 6.7+, and this fixture also compiles on 6000.3, which
            // bundles 1.0.0.
            simulate.MouseMove(message.worldBound.center);
            simulate.FrameUpdate();

            simulate.Click(ActionsButtonOf(message));

            Assert.IsTrue(message.ClassListContains(ThreadMessage.actionsOpenUssClassName),
                "Clicking the actions button should open the actions menu.");
        }

        [Test]
        public void ThreadMessage_WhenSending_ShouldBeDisabled()
        {
            var message = MountMessage();

            message.state = ThreadMessageState.Sending;
            simulate.FrameUpdate();

            Assert.IsFalse(message.enabledSelf, "A message in the Sending state should disable itself.");
        }
    }
}
#endif
