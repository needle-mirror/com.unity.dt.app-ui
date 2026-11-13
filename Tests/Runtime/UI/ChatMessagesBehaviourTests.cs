#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatMessages))]
    class ChatMessagesBehaviourTests : UIComponentTests<ChatMessages>
    {
        // A finished group (closed by text), then the live one.
        static ChatMessage TwoGroupTurn() => new ChatMessage
        {
            id = "a1",
            role = ChatRole.Assistant,
            parts = new[]
            {
                ChatMessagePart.ToolCall("t1", "read_file", "Read A"),
                ChatMessagePart.Text("between"),
                ChatMessagePart.ToolCall("t2", "search", "Search B", pending: true),
            },
        };

        static VisualElement SummaryOf(ChatActivityTimeline timeline) =>
            timeline.Q(name: ChatActivityTimeline.summaryUssClassName);

        List<ChatActivityTimeline> Timelines() => element.Query<ChatActivityTimeline>().ToList();

        ChatMessages MountList(List<bool> reported)
        {
            var list = MountElement(new ChatMessages { isLoading = true, messages = new[] { TwoGroupTurn() } });
            list.activityOpenChanged += reported.Add;
            simulate.FrameUpdate();
            return list;
        }

        [Test]
        public void ChatMessages_ToggleLiveGroup_ShouldWriteActivityOpenBackAndReportIt()
        {
            var reported = new List<bool>();
            var list = MountList(reported);

            simulate.Click(SummaryOf(Timelines()[1]));

            Assert.IsTrue(list.activityOpen, "The reader's toggle is held by the list.");
            CollectionAssert.AreEqual(new[] { true }, reported);

            // A streaming update rebuilds every message; the live group must not snap shut.
            list.streamingStats = "42 tokens";
            simulate.FrameUpdate();
            Assert.IsTrue(Timelines()[1].open, "The rebuilt live group keeps what the reader asked for.");
        }

        [Test]
        public void ChatMessages_ToggleFinishedGroup_ShouldLeaveActivityOpenAlone()
        {
            var reported = new List<bool>();
            var list = MountList(reported);

            simulate.Click(SummaryOf(Timelines()[0]));

            Assert.IsTrue(Timelines()[0].open, "A finished group answers to its own header.");
            Assert.IsFalse(list.activityOpen);
            Assert.AreEqual(0, reported.Count);
        }

        [Test]
        public void ChatMessages_BoolControlInsideLiveGroup_ShouldNotCountAsAToggle()
        {
            var reported = new List<bool>();
            var list = MountList(reported);
            var live = Timelines()[1];
            var toggle = new Unity.AppUI.UI.Toggle();
            live.hierarchy.Add(toggle);
            simulate.FrameUpdate();

            toggle.value = true;

            Assert.IsFalse(list.activityOpen, "A ChangeEvent bubbling from a child is not the group's header.");
            Assert.AreEqual(0, reported.Count);
        }
    }
}
#endif
