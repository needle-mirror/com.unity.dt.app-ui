#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatPlan))]
    class ChatPlanBehaviourTests : UIComponentTests<ChatPlan>
    {
        ChatPlan MountPlan() => MountElement(new ChatPlan
        {
            title = "Plan",
            editable = true,
            expandable = true,
            style = { width = 300, height = 200 },
        });

        [Test]
        public void ChatPlan_ClickRunButton_ShouldInvokeRunRequested()
        {
            var plan = MountPlan();
            var notifications = 0;
            plan.runRequested += () => notifications++;

            simulate.Click(plan.runButton);

            Assert.AreEqual(1, notifications, "Clicking the run button should raise runRequested.");
        }

        [Test]
        public void ChatPlan_ClickDiscardButton_ShouldInvokeDiscardRequested()
        {
            var plan = MountPlan();
            var notifications = 0;
            plan.discardRequested += () => notifications++;

            simulate.Click(plan.discardButton);

            Assert.AreEqual(1, notifications, "Clicking the discard button should raise discardRequested.");
        }

        [Test]
        public void ChatPlan_ClickEditButton_ShouldInvokeEditRequested()
        {
            var plan = MountPlan();
            var notifications = 0;
            plan.editRequested += () => notifications++;

            simulate.Click(plan.editButton);

            Assert.AreEqual(1, notifications, "Clicking the edit button should raise editRequested.");
        }

        [Test]
        public void ChatPlan_ClickExpandButton_ShouldInvokeExpandRequested()
        {
            var plan = MountPlan();
            var notifications = 0;
            plan.expandRequested += () => notifications++;

            simulate.Click(plan.expandButton);

            Assert.AreEqual(1, notifications, "Clicking the expand button should raise expandRequested.");
        }
    }
}
#endif
