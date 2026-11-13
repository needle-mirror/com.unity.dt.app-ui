#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Linq;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatMessages"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatMessagesTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatMessages))]
    class ChatMessagesSnapshotTests : UISnapshotTests<ChatMessages>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Empty", (ctx) => new ChatMessages());
                yield return new Story("Conversation", (ctx) => new ChatMessages
                {
                    messages = new[]
                    {
                        User("u1", "Make the player jump higher"),
                        AssistantParts("a1",
                            ChatMessagePart.Thinking("The jump force lives on the controller."),
                            ChatMessagePart.ToolCall("t1", "read_file", "Read PlayerController.cs"),
                            ChatMessagePart.Text("I raised the jump force to 8.")),
                    },
                });
                yield return new Story("LoadingConversation", (ctx) => new ChatMessages
                {
                    isLoadingConversation = true,
                });
                yield return new Story("BudgetExhausted", (ctx) => new ChatMessages
                {
                    budgetExhausted = true,
                });
            }
        }


        static ChatMessage AssistantParts(string id, params ChatMessagePart[] parts) => new ChatMessage
        {
            id = id,
            role = ChatRole.Assistant,
            parts = parts,
        };


        static ChatMessage User(string id, string text, DateTime? at = null) => new ChatMessage
        {
            id = id,
            role = ChatRole.User,
            content = text,
            createdAt = at,
        };
    }
}
#endif
