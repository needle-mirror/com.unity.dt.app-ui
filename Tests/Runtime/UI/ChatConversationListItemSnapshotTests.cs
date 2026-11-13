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
    /// Snapshot stories for <see cref="ChatConversationListItem"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatConversationListItemTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatConversationListItem))]
    class ChatConversationListItemSnapshotTests : UISnapshotTests<ChatConversationListItem>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    subtitle = "2 hours ago",
                });
                yield return new Story("Active", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    isActive = true,
                });
                yield return new Story("Favorite", (ctx) => new ChatConversationListItem
                {
                    title = "Megaman clone",
                    isFavorite = true,
                });
            }
        }
    }
}
#endif
