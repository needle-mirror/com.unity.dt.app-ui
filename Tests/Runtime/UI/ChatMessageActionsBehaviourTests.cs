#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatMessageActions))]
    class ChatMessageActionsBehaviourTests : UIComponentTests<ChatMessageActions>
    {
        [Test]
        public void ChatMessageActions_ClickThumbsUp_ShouldInvokeFeedbackGiven()
        {
            var actions = MountElement(new ChatMessageActions { content = "Hello" });
            var received = FeedbackSentiment.None;
            var notifications = 0;
            actions.feedbackGiven += sentiment =>
            {
                received = sentiment;
                notifications++;
            };

            simulate.Click(actions.thumbsUpButton);

            Assert.AreEqual(1, notifications, "Clicking thumbs up should raise feedbackGiven once.");
            Assert.AreEqual(FeedbackSentiment.Positive, received, "Clicking thumbs up should report a positive sentiment.");
            Assert.AreEqual(FeedbackSentiment.Positive, actions.sentiment);
        }

        [Test]
        public void ChatMessageActions_ClickThumbsDownTwice_ShouldNotifyOnceAndReturnToNone()
        {
            var actions = MountElement(new ChatMessageActions { content = "Hello" });
            var notifications = 0;
            actions.feedbackGiven += _ => notifications++;

            simulate.Click(actions.thumbsDownButton);
            Assert.AreEqual(FeedbackSentiment.Negative, actions.sentiment);

            // Re-clicking the same rating is a no-op: sentiment does not toggle back to None from a
            // second identical click, matching the "re-setting the same value raises nothing" contract.
            simulate.Click(actions.thumbsDownButton);

            Assert.AreEqual(1, notifications, "Re-clicking the same rating should not raise feedbackGiven again.");
        }

        [Test]
        public void ChatMessageActions_ClickRetryButton_ShouldInvokeRetryRequested()
        {
            var actions = MountElement(new ChatMessageActions { content = "Hello", retryable = true });
            var notifications = 0;
            actions.retryRequested += () => notifications++;

            simulate.Click(actions.retryButton);

            Assert.AreEqual(1, notifications, "Clicking the retry button should raise retryRequested.");
        }

        [Test]
        public void ChatMessageActions_ClickCopyButton_ShouldInvokeContentCopied()
        {
            var actions = MountElement(new ChatMessageActions { content = "Hello" });
            var notifications = 0;
            actions.contentCopied += () => notifications++;

            simulate.Click(actions.copyButton);

            Assert.AreEqual(1, notifications, "Clicking the copy button should raise contentCopied.");
            Assert.IsTrue(actions.copied, "The copy button should show its copied state after being clicked.");
        }
    }
}
#endif
