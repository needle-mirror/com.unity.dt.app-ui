#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatMessageActions"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatMessageActionsTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatMessageActions))]
    class ChatMessageActionsSnapshotTests : UISnapshotTests<ChatMessageActions>
    {
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
    }
}
#endif
