using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatMessageActions))]
    class ChatMessageActionsTests : VisualElementTests<ChatMessageActions>
    {
        const string k_Uss = "ChatMessageActions.uss";

        protected override string mainUssClassName => ChatMessageActions.ussClassName;

        // Handed over to ChatMessageActionsSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new ChatMessageActions { content = "Hello" });
                yield return new Story("Retryable", (ctx) => new ChatMessageActions { content = "Hello", retryable = true });
                yield return new Story("Rated", (ctx) => new ChatMessageActions
                {
                    content = "Hello",
                    sentiment = FeedbackSentiment.Positive,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatMessageActions />",
            @"<appui:ChatMessageActions content=""Hello"" retryable=""true"" sentiment=""Negative"" />",
        };

        [Test]
        public void Defaults_AreUnratedAndWithoutRetry()
        {
            var actions = new ChatMessageActions();
            Assert.AreEqual(FeedbackSentiment.None, actions.sentiment);
            Assert.IsFalse(actions.retryable);
            Assert.IsTrue(actions.retryButton.ClassListContains("unity-hidden"),
                "Retry is only offered when the caller can honour it.");
        }

        /// <summary>
        /// A rating made in code and one made by clicking have to travel the same path, otherwise a
        /// restored conversation reports feedback it never received — or fails to mark it.
        /// </summary>
        [Test]
        public void Rating_MarksTheControlAndReportsOnce()
        {
            var actions = new ChatMessageActions();
            var reported = new List<FeedbackSentiment>();
            actions.feedbackGiven += reported.Add;

            actions.thumbsUpButton.clickable.InvokePressed(null);
            Assert.AreEqual(FeedbackSentiment.Positive, actions.sentiment);
            Assert.IsTrue(actions.thumbsUpButton.ClassListContains("is-selected"));

            actions.thumbsUpButton.clickable.InvokePressed(null);
            Assert.AreEqual(1, reported.Count, "Re-rating the same way must not report twice.");

            actions.thumbsDownButton.clickable.InvokePressed(null);
            Assert.IsFalse(actions.thumbsUpButton.ClassListContains("is-selected"),
                "Switching sides must clear the previous mark.");
            Assert.IsTrue(actions.thumbsDownButton.ClassListContains("is-selected"));
            CollectionAssert.AreEqual(new[] { FeedbackSentiment.Positive, FeedbackSentiment.Negative }, reported);
        }

        [Test]
        public void Retry_RaisesItsEvent()
        {
            var actions = new ChatMessageActions { retryable = true };
            var seen = 0;
            actions.retryRequested += () => seen++;

            actions.retryButton.clickable.InvokePressed(null);
            Assert.AreEqual(1, seen);
        }

        /// <summary>
        /// Copy is the one action with a real side effect, so it is asserted against the clipboard
        /// rather than against the event alone.
        /// </summary>
        [Test]
        public void Copy_WritesTheContentToTheClipboard()
        {
            var previous = GUIUtility.systemCopyBuffer;
            try
            {
                var actions = new ChatMessageActions { content = "the answer" };
                actions.copyButton.clickable.InvokePressed(null);

                Assert.AreEqual("the answer", GUIUtility.systemCopyBuffer);
            }
            finally
            {
                GUIUtility.systemCopyBuffer = previous;
            }
        }

        [Test]
        public void Copy_ConfirmsWithACheckAndGoesBack()
        {
            var previous = GUIUtility.systemCopyBuffer;
            try
            {
                var actions = new ChatMessageActions { content = "x" };
                Assert.AreEqual("copy", actions.copyButton.icon);

                actions.Copy();
                Assert.IsTrue(actions.copied);
                Assert.AreEqual("check", actions.copyButton.icon);

                actions.ResetCopied();
                Assert.IsFalse(actions.copied);
                Assert.AreEqual("copy", actions.copyButton.icon,
                    "The confirmation must be temporary, not a permanent state change.");
            }
            finally
            {
                GUIUtility.systemCopyBuffer = previous;
            }
        }

        [UnityTest]
        public IEnumerator Copy_DoesNotThrowOnNullContent()
        {
            var previous = GUIUtility.systemCopyBuffer;
            try
            {
                var actions = new ChatMessageActions();
                yield return ChatTestUtils.Mount(m_TestUI, actions);

                Assert.DoesNotThrow(() => actions.Copy());
                Assert.AreEqual(string.Empty, GUIUtility.systemCopyBuffer);
            }
            finally
            {
                GUIUtility.systemCopyBuffer = previous;
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void RatedAndCopiedStates_TakeTheirColoursFromTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-actions-accent-background-color-default)", uss);
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-semantic-positive)",
                ChatTestUtils.RuleBody(uss, @"\.appui-chat-message-actions__copy-button--copied \.appui-icon"));
        }
    }
}
