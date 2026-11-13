#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    // Declared inside the namespace: a compilation-unit alias loses to the root
    // `Timeline` namespace the editor puts in scope, but an alias at this level is
    // found before the lookup walks outward.
    using Timeline = Unity.AppUI.UI.Timeline;

    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineBehaviourTests : UIComponentTests<Timeline>
    {
        Timeline MountTimeline() => MountElement(new Timeline
        {
            duration = 100f,
            pxPerUnit = 8f,
            style = { width = 400, height = 200 },
        });

        [Test]
        public void Timeline_ClickOnRuler_ShouldSeekValue()
        {
            var timeline = MountTimeline();
            Assert.AreEqual(0f, timeline.value, "The playhead should start at zero.");
            Assert.Greater(timeline.ruler.worldBound.width, 0f,
                "The ruler needs a resolved layout before it can be clicked by position.");

            // The ruler starts at Timeline.origin pixels from the left of the viewport, so clicking
            // further right lands on a strictly positive time.
            var from = timeline.ruler.worldBound.position + new Vector2(Timeline.origin + 80f, 4f);
            simulate.Click(from);

            Assert.Greater(timeline.value, 0f, "Clicking the ruler should seek the playhead to the clicked time.");
        }

        [Test]
        public void Timeline_ClickOnRuler_ShouldNotifyValueChanged()
        {
            var timeline = MountTimeline();
            var notifications = 0;
            timeline.valueChanged += _ => notifications++;

            var from = timeline.ruler.worldBound.position + new Vector2(Timeline.origin + 80f, 4f);
            simulate.Click(from);

            Assert.Greater(notifications, 0, "A seek should raise valueChanged at least once.");
        }

        [Test]
        public void Timeline_DragOnRuler_ShouldScrubValueContinuously()
        {
            var timeline = MountTimeline();
            var from = timeline.ruler.worldBound.position + new Vector2(Timeline.origin + 20f, 4f);

            simulate.DragAndDrop(from, from + new Vector2(120f, 0f));
            simulate.FrameUpdate();

            Assert.Greater(timeline.value, 0f, "Dragging right on the ruler should scrub the playhead forward.");
        }

        [Test]
        public void Timeline_WhenSeekOnClickDisabled_ClickOnRuler_ShouldNotSeek()
        {
            var timeline = MountTimeline();
            timeline.ruler.seekOnClick = false;

            var from = timeline.ruler.worldBound.position + new Vector2(Timeline.origin + 80f, 4f);
            simulate.Click(from);

            Assert.AreEqual(0f, timeline.value, "With seekOnClick disabled, a ruler click should not move the playhead.");
        }

        [Test]
        public void Timeline_WhenDisabled_ClickOnRuler_ShouldNotSeek()
        {
            var timeline = MountTimeline();
            timeline.disabled = true;
            simulate.FrameUpdate();

            var from = timeline.ruler.worldBound.position + new Vector2(Timeline.origin + 80f, 4f);
            simulate.Click(from);

            Assert.AreEqual(0f, timeline.value, "A disabled Timeline should not respond to a ruler click.");
        }

        // ── Non-finite duration (registry #1415) ───────────────────────────

        [Test]
        public void Timeline_EndKey_WithAFiniteDuration_SeeksToTheEnd()
        {
            var timeline = MountTimeline();

            timeline.playhead.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.End);

            Assert.AreEqual(100f, timeline.value, "Positive control: End should seek the playhead to the duration.");
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Timeline_NonFiniteDuration_ReadsAsAnEmptyAxis(float nonFinite)
        {
            var timeline = MountTimeline();

            timeline.duration = nonFinite;
            simulate.FrameUpdate();

            Assert.AreEqual(0f, timeline.duration,
                "A non-finite duration has no extent, so it should read as an empty axis, as the registry does.");
            Assert.IsTrue(float.IsFinite(timeline.stripWidth), "The strip must keep a finite width.");

            timeline.playhead.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.End);

            Assert.IsTrue(float.IsFinite(timeline.value), "End must not seek the playhead to a non-finite time.");
            Assert.AreEqual(0f, timeline.value, "End on an empty axis stays at zero.");
        }
    }
}
#endif
