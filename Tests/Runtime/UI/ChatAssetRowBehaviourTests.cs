#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatAssetRow))]
    class ChatAssetRowBehaviourTests : UIComponentTests<ChatAssetRow>
    {
        static VisualElement CreatorLabelOf(ChatAssetRow row) =>
            row.Q(name: ChatAssetRow.creatorUssClassName);

        [Test]
        public void ChatAssetRow_DetailsButtonClick_ShouldInvokeDetailsRequested()
        {
            var row = MountElement(new ChatAssetRow { title = "Asset", details = true });
            var notifications = 0;
            row.detailsRequested += () => notifications++;

            simulate.Click(row.detailsButton);

            Assert.AreEqual(1, notifications, "Clicking the details button should raise detailsRequested.");
        }

        [Test]
        public void ChatAssetRow_CreatorLinkClick_ShouldInvokeCreatorRequested()
        {
            var row = MountElement(new ChatAssetRow { title = "Asset", creatorLabel = "Someone", creatorLink = true });
            var notifications = 0;
            row.creatorRequested += () => notifications++;

            simulate.Click(CreatorLabelOf(row));

            Assert.AreEqual(1, notifications, "Clicking the creator label as a link should raise creatorRequested.");
        }

        [Test]
        public void ChatAssetRow_WhenCreatorLinkDisabled_Click_ShouldNotInvokeCreatorRequested()
        {
            var row = MountElement(new ChatAssetRow { title = "Asset", creatorLabel = "Someone", creatorLink = false });
            var notifications = 0;
            row.creatorRequested += () => notifications++;

            simulate.Click(CreatorLabelOf(row));

            Assert.AreEqual(0, notifications, "The creator label should ignore clicks when creatorLink is false.");
        }
    }
}
#endif
