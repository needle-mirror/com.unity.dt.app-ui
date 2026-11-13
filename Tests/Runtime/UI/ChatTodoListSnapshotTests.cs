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
    /// Snapshot stories for <see cref="ChatTodoList"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatTodoListTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatTodoList))]
    class ChatTodoListSnapshotTests : UISnapshotTests<ChatTodoList>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Open", (ctx) => new ChatTodoList { items = SampleItems() });
                yield return new Story("Collapsed", (ctx) => new ChatTodoList { items = SampleItems(), open = false });
                yield return new Story("CustomTitle", (ctx) => new ChatTodoList { items = SampleItems(), title = "Plan" });
            }
        }


        static ChatTodoItem[] SampleItems() => new[]
        {
            new ChatTodoItem("Read the player controller", ChatTodoStatus.Completed),
            new ChatTodoItem("Raise the jump force", ChatTodoStatus.InProgress),
            new ChatTodoItem("Re-run the tests"),
            new ChatTodoItem("Ship it", ChatTodoStatus.Cancelled),
        };
    }
}
#endif
