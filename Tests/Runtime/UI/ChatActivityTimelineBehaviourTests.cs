#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatActivityTimeline))]
    class ChatActivityTimelineBehaviourTests : UIComponentTests<ChatActivityTimeline>
    {
        static VisualElement SummaryOf(ChatActivityTimeline timeline) =>
            timeline.Q(name: ChatActivityTimeline.summaryUssClassName);

        ChatActivityTimeline MountTimeline() => MountElement(new ChatActivityTimeline { label = "Thinking" });

        [Test]
        public void ChatActivityTimeline_ClickSummary_ShouldToggleOpen()
        {
            var timeline = MountTimeline();
            Assert.IsFalse(timeline.open, "The timeline should be collapsed by default.");

            simulate.Click(SummaryOf(timeline));
            Assert.IsTrue(timeline.open, "Clicking the summary should expand the timeline.");

            simulate.Click(SummaryOf(timeline));
            Assert.IsFalse(timeline.open, "Clicking the summary again should collapse it.");
        }

        [Test]
        public void ChatActivityTimeline_ClickSummary_ShouldInvokeClickable()
        {
            var timeline = MountTimeline();
            var clickCount = 0;
            timeline.clickable.clicked += () => clickCount++;

            simulate.Click(SummaryOf(timeline));

            Assert.AreEqual(1, clickCount, "Clicking the summary should invoke the clickable manipulator.");
        }

        [Test]
        public void ChatActivityTimeline_SummarySubmitKey_ShouldToggleOpen()
        {
            var timeline = MountTimeline();

            SummaryOf(timeline).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsTrue(timeline.open, "Submitting while the summary is focused should expand the timeline.");
        }

        [Test]
        public void ChatActivityTimeline_ClickSummary_ShouldSendAChangeEventOnlyForTheReadersToggle()
        {
            var timeline = MountTimeline();
            var events = new System.Collections.Generic.List<(bool previous, bool next)>();
            timeline.RegisterCallback<ChangeEvent<bool>>(evt => events.Add((evt.previousValue, evt.newValue)));

            timeline.open = true;
            timeline.open = false;
            Assert.AreEqual(0, events.Count, "Setting open from code is not a reader's toggle.");

            simulate.Click(SummaryOf(timeline));
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual((false, true), events[0]);

            simulate.Click(SummaryOf(timeline));
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual((true, false), events[1]);
        }

        [Test]
        public void ChatActivityTimeline_MaxPinnedShare_ShouldBudgetEntriesFromTheEnclosingScrollView()
        {
            var scroll = new ScrollView { style = { height = 100 } };
            var timeline = new ChatActivityTimeline
            {
                open = true,
                pinEntries = ChatActivityPinEntries.HeadAndFoot,
                parts = new[]
                {
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "a"),
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "b"),
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "c"),
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "d"),
                },
            };
            scroll.Add(timeline);
            appUiPanel.Clear();
            appUiPanel.Add(scroll);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.AreEqual(ChatActivityTimeline.maxPinnedEntries, timeline.pinBudget,
                "A share of 1 leaves the six-entry cap as the only limit.");

            timeline.maxPinnedShare = 0f;
            simulate.FrameUpdate();
            Assert.AreEqual(0, timeline.pinBudget);
            Assert.IsNull(timeline.Q(className: ChatActivityTimeline.rowPinnedHeadUssClassName),
                "No room: nothing pins.");

            var rowHeight = timeline.Q(className: ChatActivityTimeline.rowUssClassName).layout.height;
            Assume.That(rowHeight, Is.GreaterThan(0f));
            timeline.maxPinnedShare = 0.5f;
            simulate.FrameUpdate();
            var expected = ChatActivityTimeline.ComputePinBudget(scroll.contentViewport.layout.height, rowHeight, 0.5f);
            Assert.AreEqual(expected, timeline.pinBudget);
            Assert.Less(timeline.pinBudget, ChatActivityTimeline.maxPinnedEntries,
                "Half a 100px view holds fewer than six entries.");

            // The viewport grows while the timeline's own rect stays put.
            var before = timeline.layout;
            scroll.style.height = 40;
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            Assume.That(timeline.layout.height, Is.EqualTo(before.height).Within(0.01f));
            var resized = ChatActivityTimeline.ComputePinBudget(scroll.contentViewport.layout.height, rowHeight, 0.5f);
            Assume.That(resized, Is.Not.EqualTo(expected), "The resize has to change the budget to prove anything.");
            Assert.AreEqual(resized, timeline.pinBudget, "Resizing the scroll area re-measures the budget.");
        }

        [Test]
        public void ChatActivityTimeline_FootHeaderPress_ShouldKeepTheHeaderInView([Values] bool keyboard)
        {
            var parts = new System.Collections.Generic.List<ChatActivityPart>();
            for (var i = 0; i < 20; i++)
                parts.Add(new ChatActivityPart(ChatActivityPartKind.Thinking, "step " + i));
            var scroll = new ScrollView { style = { height = 80 } };
            var timeline = new ChatActivityTimeline
            {
                headerPlacement = ChatActivityHeaderPlacement.Bottom,
                parts = parts,
            };
            scroll.Add(timeline);
            appUiPanel.Clear();
            appUiPanel.Add(scroll);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            bool HeaderInView()
            {
                var view = scroll.contentViewport.worldBound;
                var header = SummaryOf(timeline).worldBound;
                return header.yMin >= view.yMin - 0.5f && header.yMax <= view.yMax + 0.5f;
            }

            Assume.That(HeaderInView(), "Closed, the chip is the whole group and sits in view.");

            if (keyboard)
            {
                SummaryOf(timeline).Focus();
                simulate.FrameUpdate();
                simulate.ReturnKeyPress();
            }
            else
            {
                simulate.Click(SummaryOf(timeline));
            }
            Assert.IsTrue(timeline.open);
            for (var i = 0; i < 4; i++)
                simulate.FrameUpdate();

            Assert.IsTrue(HeaderInView(), "The rows opened above the chip; it must stay under the eye.");
        }

        [Test]
        public void ChatActivityTimeline_PagerSubmit_ShouldKeepFocusOnThePager()
        {
            var timeline = MountElement(new ChatActivityTimeline
            {
                open = true,
                foldRowsByKind = true,
                parts = new[]
                {
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "one"),
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "two"),
                    new ChatActivityPart(ChatActivityPartKind.Thinking, "three"),
                },
            });

            timeline.Q<IconButton>("previous").Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();
            simulate.FrameUpdate();

            var focused = timeline.focusController.focusedElement as IconButton;
            Assert.IsNotNull(focused, "The rebuilt pager must take focus back.");
            Assert.AreEqual("previous", focused.name);
            Assert.AreEqual("two", timeline.Q<LocalizedTextElement>(name: ChatActivityTimeline.rowTextUssClassName).text);

            simulate.ReturnKeyPress();
            simulate.FrameUpdate();
            Assert.AreEqual("one", timeline.Q<LocalizedTextElement>(name: ChatActivityTimeline.rowTextUssClassName).text,
                "A second submit keeps paging.");
            focused = timeline.focusController.focusedElement as IconButton;
            Assert.IsNotNull(focused);
            Assert.AreEqual("next", focused.name, "At the oldest, focus moves to the step still enabled.");
        }
    }
}
#endif
