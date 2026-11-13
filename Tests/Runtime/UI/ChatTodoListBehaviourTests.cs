#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatTodoList))]
    class ChatTodoListBehaviourTests : UIComponentTests<ChatTodoList>
    {
        static VisualElement HeaderOf(ChatTodoList list) =>
            list.Q(name: ChatTodoList.headerUssClassName);

        static VisualElement MaximizeButtonOf(ChatTodoList list) =>
            list.Q(name: ChatTodoList.maximizeUssClassName);

        static VisualElement CloseButtonOf(ChatTodoList list) =>
            list.Q(name: ChatTodoList.closeUssClassName);

        ChatTodoList MountList() => MountElement(new ChatTodoList
        {
            items = new[] { new ChatTodoItem("Write tests", ChatTodoStatus.Pending) },
            showMaximize = true,
            showClose = true,
        });

        [Test]
        public void ChatTodoList_ClickHeader_ShouldToggleOpen()
        {
            var list = MountList();
            Assert.IsTrue(list.open, "The list should be expanded by default.");

            simulate.Click(HeaderOf(list));
            Assert.IsFalse(list.open, "Clicking the header should collapse the list.");

            simulate.Click(HeaderOf(list));
            Assert.IsTrue(list.open, "Clicking the header again should expand the list.");
        }

        [Test]
        public void ChatTodoList_WhenNotCollapsible_ClickHeader_ShouldNotChangeOpen()
        {
            var list = MountElement(new ChatTodoList
            {
                items = new[] { new ChatTodoItem("Write tests", ChatTodoStatus.Pending) },
                collapsible = false,
            });
            Assert.IsTrue(list.open);

            simulate.Click(HeaderOf(list));

            Assert.IsTrue(list.open, "A non-collapsible list should ignore header clicks.");
        }

        [Test]
        public void ChatTodoList_ClickMaximizeButton_ShouldInvokeMaximizeRequested()
        {
            var list = MountList();
            var notifications = 0;
            list.maximizeRequested += () => notifications++;

            simulate.Click(MaximizeButtonOf(list));

            Assert.AreEqual(1, notifications, "Clicking the maximize button should raise maximizeRequested.");
        }

        [Test]
        public void ChatTodoList_ClickCloseButton_ShouldInvokeCloseRequested()
        {
            var list = MountList();
            var notifications = 0;
            list.closeRequested += () => notifications++;

            simulate.Click(CloseButtonOf(list));

            Assert.AreEqual(1, notifications, "Clicking the close button should raise closeRequested.");
        }
    }
}
#endif
