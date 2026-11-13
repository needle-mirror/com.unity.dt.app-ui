#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    using Timeline = Unity.AppUI.UI.Timeline;

    /// <summary>
    /// The overlay scrollbar, the pan clamp and the playhead panning into view, from registry #1415.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineNavigationTests : UIComponentTests<Timeline>
    {
        // 100 units at 8 px/unit is an 840px strip in a ~400px viewport: pannable.
        Timeline MountTimeline(float duration = 100f) => MountElement(new Timeline
        {
            duration = duration,
            pxPerUnit = 8f,
            style = { width = 400, height = 200 },
        });

        static float MaxOffset(Timeline t) => Mathf.Max(0f, t.stripWidth - t.viewport.layout.width);

        // ------------------------------------------------------------------ pan clamp

        [Test]
        public void Offset_IsReclampedWhenTheAxisShrinks()
        {
            var timeline = MountTimeline();
            timeline.offset = MaxOffset(timeline);
            Assert.Greater(timeline.offset, 100f, "Positive control: the strip is panned well right.");

            timeline.duration = 20f;
            simulate.FrameUpdate();

            Assert.LessOrEqual(timeline.offset, MaxOffset(timeline) + 0.01f,
                "A shorter axis must pull the pan back inside it, not leave a blank viewport.");
        }

        // ------------------------------------------------------------------ playhead reveal

        [Test]
        public void Playhead_EndKey_PansThePlayheadIntoView()
        {
            var timeline = MountTimeline();
            timeline.playhead.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.End);
            simulate.FrameUpdate();

            Assert.AreEqual(100f, timeline.value);
            var x = timeline.TimeToX(timeline.value) - timeline.offset;
            Assert.That(x, Is.InRange(0f, timeline.viewport.layout.width),
                "A keyboard-moved playhead must pan into view.");
        }

        [Test]
        public void Playhead_ArrowKey_InView_DoesNotPan()
        {
            var timeline = MountTimeline();
            timeline.playhead.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual(0f, timeline.offset, 0.01f, "A playhead still well inside the view leaves the pan alone.");
        }

        [Test]
        public void Value_AssignedFromCode_DoesNotPan()
        {
            var timeline = MountTimeline();
            timeline.value = 100f;
            simulate.FrameUpdate();

            Assert.AreEqual(0f, timeline.offset, 0.01f, "Only the keyboard path reveals the playhead.");
        }

        [Test]
        public void RevealTime_KeepsAMarginFromTheEdge()
        {
            var timeline = MountTimeline();
            timeline.RevealTime(60f);

            var vp = timeline.viewport.layout.width;
            var x = timeline.TimeToX(60f) - timeline.offset;
            Assert.AreEqual(vp - Mathf.Min(Timeline.revealMargin, vp / 4f), x, 0.5f,
                "A time past the right edge lands one margin in from it.");
        }

        // ------------------------------------------------------------------ scrollbar

        [Test]
        public void Scrollbar_ShowsOnlyWhenTheAxisOverflows()
        {
            var timeline = MountTimeline();
            Assert.AreEqual(DisplayStyle.Flex, timeline.scrollbar.resolvedStyle.display, "Overflowing: shown.");

            timeline.duration = 10f;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, timeline.scrollbar.resolvedStyle.display, "Everything fits: hidden.");

            timeline.duration = 100f;
            simulate.FrameUpdate();
            timeline.infinite = true;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, timeline.scrollbar.resolvedStyle.display,
                "An infinite axis has no extent to scroll through.");

            timeline.infinite = false;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.Flex, timeline.scrollbar.resolvedStyle.display,
                "Back to a finite axis, the bar returns without any other change.");
        }

        [Test]
        public void Scrollbar_ShownLater_SizesItsThumbOnceLaidOut()
        {
            var timeline = MountTimeline(duration: 10f);
            Assert.AreEqual(DisplayStyle.None, timeline.scrollbar.resolvedStyle.display);

            timeline.duration = 100f;
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var track = timeline.scrollbar.Q(className: "unity-base-slider__drag-container").layout.width;
            var expected = track * timeline.viewport.layout.width / timeline.stripWidth;
            Assert.AreEqual(expected, timeline.scrollbar.Q(className: "unity-base-slider__dragger").layout.width, 2f,
                "The thumb is the visible share of the strip, measured on the laid-out track.");
        }

        [Test]
        public void Scrollbar_InAnAutoHeightTimeline_LeavesTheLastLaneUncovered()
        {
            var timeline = new Timeline { duration = 100f, pxPerUnit = 8f, style = { width = 400 } };
            var track = new TimelineTrack { id = "a" };
            timeline.Add(track);
            MountElement(timeline);
            simulate.FrameUpdate();

            Assert.AreEqual(DisplayStyle.Flex, timeline.scrollbar.resolvedStyle.display);
            Assert.GreaterOrEqual(timeline.scrollbar.worldBound.yMin, track.worldBound.yMax - 0.5f,
                "The strip keeps the bar's height under the last lane.");
        }

        [Test]
        public void Scrollbar_OverlaysTheViewportsBottomEdge()
        {
            var timeline = MountTimeline();
            var bar = timeline.scrollbar.worldBound;
            var vp = timeline.viewport.worldBound;
            Assert.AreEqual(vp.yMax, bar.yMax, 0.5f, "Pinned to the bottom of the viewport.");
            Assert.AreEqual(vp.width, bar.width, 0.5f, "As wide as the viewport.");
        }

        [Test]
        public void Scrollbar_FollowsThePan_AndPansWhenMoved()
        {
            var timeline = MountTimeline();
            timeline.offset = 100f;
            simulate.FrameUpdate();
            Assert.AreEqual(100f, timeline.scrollbar.value, 0.5f, "The thumb follows the pan.");
            Assert.AreEqual(MaxOffset(timeline), timeline.scrollbar.highValue, 0.5f);

            timeline.scrollbar.value = 50f;
            Assert.AreEqual(50f, timeline.offset, 0.5f, "Moving the thumb pans.");
        }

        [Test]
        public void Scrollbar_Keys_PanByATenthOfTheViewport_AndHomeEndGoToTheEnds()
        {
            var timeline = MountTimeline();
            timeline.scrollbar.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);
            var step = Mathf.Max(16f, timeline.viewport.layout.width / 10f);
            Assert.AreEqual(step, timeline.offset, 0.5f);

            simulate.KeyPress(KeyCode.End);
            Assert.AreEqual(MaxOffset(timeline), timeline.offset, 0.5f);

            simulate.KeyPress(KeyCode.Home);
            Assert.AreEqual(0f, timeline.offset, 0.5f);
        }

        [Test]
        public void Scrollbar_PointerDragsTheThumb_JumpsOnATrackClick_AndIgnoresTheSecondaryButton()
        {
            var timeline = MountTimeline();
            var thumb = timeline.scrollbar.Q(className: "unity-base-slider__dragger");
            var from = thumb.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(60f, 0f));
            simulate.FrameUpdate();
            Assert.Greater(timeline.offset, 30f, "Dragging the thumb pans.");

            timeline.offset = 0f;
            simulate.FrameUpdate();
            var track = timeline.scrollbar.Q(className: "unity-base-slider__drag-container").worldBound;
            var nearEnd = new Vector2(track.xMax - 4f, track.center.y);

            simulate.MouseDown(nearEnd, MouseButton.RightMouse);
            simulate.MouseUp(nearEnd, MouseButton.RightMouse);
            Assert.AreEqual(0f, timeline.offset, 0.5f, "The secondary button does nothing.");

            simulate.Click(nearEnd);
            simulate.FrameUpdate();
            Assert.Greater(timeline.offset, MaxOffset(timeline) * 0.8f, "A click near the track's end jumps there.");
        }

        [Test]
        public void Scrollbar_WhenDisabled_IgnoresKeys()
        {
            var timeline = MountTimeline();
            timeline.disabled = true;
            timeline.scrollbar.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.End);
            Assert.AreEqual(0f, timeline.offset, 0.01f);
            Assert.IsFalse(timeline.scrollbar.enabledSelf, "A disabled Timeline disables its scrollbar.");
        }
    }
}
#endif
