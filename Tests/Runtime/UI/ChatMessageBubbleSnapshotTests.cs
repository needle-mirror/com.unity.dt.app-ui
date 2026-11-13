#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatMessageBubble"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatMessageBubbleTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatMessageBubble))]
    class ChatMessageBubbleSnapshotTests : UISnapshotTests<ChatMessageBubble>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("User", (ctx) => new ChatMessageBubble
                {
                    role = ChatMessageRole.User,
                    content = "Make the player jump higher",
                });
                yield return new Story("Assistant", (ctx) => new ChatMessageBubble
                {
                    role = ChatMessageRole.Assistant,
                    content = "I raised the jump force to 8.",
                });
                yield return new Story("UserWithChips", (ctx) =>
                {
                    var bubble = new ChatMessageBubble { role = ChatMessageRole.User, content = "Fix this" };
                    bubble.sentWith.Add(new Chip { label = "PlayerController.cs" });
                    bubble.actions.Add(new ChatMessageActions { content = "Fix this" });
                    return bubble;
                });
            }
        }
    }
}
#endif
