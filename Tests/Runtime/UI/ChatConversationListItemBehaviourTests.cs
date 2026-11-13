#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatConversationListItem))]
    class ChatConversationListItemBehaviourTests : UIComponentTests<ChatConversationListItem>
    {
        static VisualElement BodyOf(ChatConversationListItem item) =>
            item.Q(name: ChatConversationListItem.bodyUssClassName);

        ChatConversationListItem MountItem() => MountElement(new ChatConversationListItem
        {
            title = "Megaman clone",
            subtitle = "2 hours ago",
        });

        [Test]
        public void ChatConversationListItem_ClickBody_ShouldInvokeClicked()
        {
            var item = MountItem();
            var notifications = 0;
            item.clicked += () => notifications++;

            simulate.Click(BodyOf(item));

            Assert.AreEqual(1, notifications, "Clicking the body should raise clicked.");
        }

        [Test]
        public void ChatConversationListItem_ClickFavoriteButton_ShouldInvokeFavoriteToggled()
        {
            var item = MountItem();
            var clicks = 0;
            var favoriteToggles = 0;
            item.clicked += () => clicks++;
            item.favoriteToggled += () => favoriteToggles++;

            simulate.Click(item.favoriteButton);

            Assert.AreEqual(1, favoriteToggles, "Clicking the favorite button should raise favoriteToggled.");
            Assert.AreEqual(0, clicks, "The favorite toggle is a separate target and should not also select the row.");
        }

        [Test]
        public void ChatConversationListItem_BodyFocusSubmitKey_ShouldInvokeClicked()
        {
            var item = MountItem();
            var notifications = 0;
            item.clicked += () => notifications++;

            BodyOf(item).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, notifications, "Submitting while the body is focused should activate the row.");
        }
    }
}
#endif
