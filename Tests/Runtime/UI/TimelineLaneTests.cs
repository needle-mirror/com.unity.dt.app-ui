#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    using Timeline = Unity.AppUI.UI.Timeline;

    /// <summary>
    /// Lane extents, clip variants, TimelineHandle and key sizes, from registry #1415.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineLaneTests : UIComponentTests<Timeline>
    {
        const float k_Ppu = 8f;

        Timeline MountTimeline(TimelineTrack track, float width = 1000f)
        {
            var timeline = new Timeline
            {
                duration = 100f,
                pxPerUnit = k_Ppu,
                style = { width = width, height = 200 },
            };
            timeline.Add(track);
            return MountElement(timeline);
        }

        // ------------------------------------------------------------------ lanes

        [Test]
        public void Lane_DefaultsToTheWholeDuration()
        {
            var track = new TimelineTrack { id = "a" };
            MountTimeline(track);

            Assert.AreEqual(Timeline.origin, track.laneElement.resolvedStyle.left, 0.5f);
            Assert.AreEqual(100f * k_Ppu, track.laneElement.resolvedStyle.width, 0.5f);
        }

        [Test]
        public void LaneExtent_EndsEveryLaneEarlier_ButNeverPastTheDuration()
        {
            var track = new TimelineTrack { id = "a" };
            var timeline = MountTimeline(track);

            timeline.laneExtent = 50f;
            simulate.FrameUpdate();
            Assert.AreEqual(50f * k_Ppu, track.laneElement.resolvedStyle.width, 0.5f);

            timeline.laneExtent = 500f;
            simulate.FrameUpdate();
            Assert.AreEqual(100f * k_Ppu, track.laneElement.resolvedStyle.width, 0.5f, "Clamped to the duration.");
        }

        [Test]
        public void Track_LaneStartAndEnd_DrawItsOwnSpan()
        {
            var track = new TimelineTrack { id = "a", laneStart = 10f, laneEnd = 30f };
            var timeline = MountTimeline(track);

            Assert.AreEqual(timeline.TimeToX(10f), track.laneElement.resolvedStyle.left, 0.5f);
            Assert.AreEqual(20f * k_Ppu, track.laneElement.resolvedStyle.width, 0.5f);
        }

        [Test]
        public void Track_LaneOff_HidesTheLane()
        {
            var track = new TimelineTrack { id = "a", lane = false };
            MountTimeline(track);
            Assert.AreEqual(DisplayStyle.None, track.laneElement.resolvedStyle.display);
        }

        // ------------------------------------------------------------------ clip variants

        [Test]
        public void ClipVariants_AreNotResizableUnlessAsked()
        {
            var clip = new TimelineClip();
            Assert.IsTrue(clip.resizable, "A bar is resizable by default.");

            clip.variant = TimelineClipVariant.Handle;
            Assert.IsFalse(clip.resizable, "A handle draws no span to resize by.");
            clip.variant = TimelineClipVariant.Surface;
            Assert.IsFalse(clip.resizable);

            clip.resizable = true;
            Assert.IsTrue(clip.resizable, "An explicit resizable wins over the variant.");
            clip.variant = TimelineClipVariant.Handle;
            Assert.IsTrue(clip.resizable);
        }

        [Test]
        public void HandleClip_DragsByItsGripOnly_AndDrawsNoBar()
        {
            var track = new TimelineTrack { id = "a", color = Color.red };
            var clip = new TimelineClip { start = 10f, duration = 20f, variant = TimelineClipVariant.Handle };
            track.Add(clip);
            MountTimeline(track);

            Assert.AreEqual(0f, clip.resolvedStyle.backgroundColor.a, 0.001f, "The lane shows the span; no bar.");
            var grip = clip.Q<VisualElement>(className: TimelineClip.dragHandleUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, grip.resolvedStyle.display);
            Assert.AreEqual(PickingMode.Ignore, clip.pickingMode, "Only the grip takes the pointer.");
            var edge = clip.Q<VisualElement>(className: TimelineClip.edgeUssClassName);
            Assert.AreEqual(DisplayStyle.None, edge.resolvedStyle.display);

            var from = grip.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(5f * k_Ppu, 0f));
            simulate.FrameUpdate();
            Assert.AreEqual(15f, clip.start, 0.01f, "Dragging the grip moves the clip.");
        }

        [Test]
        public void HandleClip_GripShowsItsIconInTheTrackColour_EvenOnAShortClip()
        {
            var track = new TimelineTrack { id = "a", color = Color.red };
            var clip = new TimelineClip { start = 10f, duration = 0.1f, variant = TimelineClipVariant.Handle };
            track.Add(clip);
            var timeline = MountTimeline(track);
            simulate.FrameUpdate();

            var icon = clip.Q<Icon>(className: TimelineClip.dragHandleIconUssClassName);
            Assert.IsTrue(icon.vectorImage != null || icon.image != null,
                "The grip's icon must resolve with the default (minimal) icon sheet.");
            Assert.AreEqual(Color.red, icon.tintColor, "The grip takes the track colour.");

            var grip = clip.Q<VisualElement>(className: TimelineClip.dragHandleUssClassName);
            Assert.Greater(grip.worldBound.width, clip.worldBound.width, "Positive control: the grip is wider than the span.");
            var right = new Vector2(grip.worldBound.xMax - 1f, grip.worldBound.center.y);
            Assert.AreSame(grip, timeline.panel.Pick(right), "A short clip must not clip its grip away.");
        }

        [Test]
        public void HandleClip_GripStaysInsideItsLane_OnAShortTrack()
        {
            var track = new TimelineTrack { id = "a" };
            var clip = new TimelineClip { start = 10f, duration = 20f, variant = TimelineClipVariant.Handle };
            track.Add(clip);
            var timeline = MountTimeline(track);
            timeline.trackHeight = 20f;
            simulate.FrameUpdate();

            var grip = clip.Q<VisualElement>(className: TimelineClip.dragHandleUssClassName);
            Assert.LessOrEqual(grip.worldBound.yMax, track.worldBound.yMax + 0.5f,
                "The grip must not reach into the next lane, where it would take that lane's clicks.");
        }

        [Test]
        public void HandleClip_UncolouredGrip_KeepsTheStylesheetTint()
        {
            var track = new TimelineTrack { id = "a" };
            var clip = new TimelineClip { start = 10f, duration = 20f, variant = TimelineClipVariant.Handle };
            track.Add(clip);
            var timeline = MountTimeline(track);
            timeline.RefreshAxis();
            simulate.FrameUpdate();
            var enabledTint = clip.Q<Icon>(className: TimelineClip.dragHandleIconUssClassName).tintColor;

            timeline.disabled = true;
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var disabledTint = clip.Q<Icon>(className: TimelineClip.dragHandleIconUssClassName).tintColor;
            Assert.AreNotEqual(enabledTint, disabledTint,
                "An uncoloured grip must stay on the stylesheet, so the disabled tint reaches it.");

            track.color = Color.red;
            Assert.AreEqual(Color.red, clip.Q<Icon>(className: TimelineClip.dragHandleIconUssClassName).tintColor);
            track.color = Optional<Color>.none;
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            Assert.AreEqual(disabledTint, clip.Q<Icon>(className: TimelineClip.dragHandleIconUssClassName).tintColor,
                "Unsetting the colour hands the tint back to the stylesheet.");
        }

        [Test]
        public void SurfaceClip_IsAnInvisibleDragSurface()
        {
            var track = new TimelineTrack { id = "a", color = Color.red };
            var clip = new TimelineClip { start = 10f, duration = 20f, label = "Walk", variant = TimelineClipVariant.Surface };
            track.Add(clip);
            MountTimeline(track);

            Assert.AreEqual(0f, clip.resolvedStyle.backgroundColor.a, 0.001f);
            Assert.AreEqual(DisplayStyle.None, clip.Q<LocalizedTextElement>(className: TimelineClip.labelUssClassName).resolvedStyle.display);

            var from = clip.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(-5f * k_Ppu, 0f));
            simulate.FrameUpdate();
            Assert.AreEqual(5f, clip.start, 0.01f, "The whole span drags.");
        }

        // ------------------------------------------------------------------ TimelineHandle

        (Timeline, TimelineHandle) MountHandle(float time, float max = -1f, float width = 1000f)
        {
            var track = new TimelineTrack { id = "a" };
            var handle = new TimelineHandle { time = time, max = max };
            track.Add(handle);
            var timeline = MountTimeline(track, width);
            simulate.FrameUpdate();
            return (timeline, handle);
        }

        [Test]
        public void Handle_Drag_ReportsLiveAndCommitsOnceOnRelease()
        {
            var (_, handle) = MountHandle(20f, max: 40f);
            var moves = 0;
            var commits = 0;
            var committed = -1f;
            handle.dragMove += _ => moves++;
            handle.changed += t => { commits++; committed = t; };

            var from = handle.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(5f * k_Ppu, 0f));
            simulate.MouseMove(from + new Vector2(10f * k_Ppu, 0f));
            Assert.AreEqual(0, commits, "Nothing commits mid-drag.");
            Assert.Greater(moves, 0);

            simulate.MouseMove(from + new Vector2(60f * k_Ppu, 0f));
            simulate.MouseUp(from + new Vector2(60f * k_Ppu, 0f));
            Assert.AreEqual(1, commits);
            Assert.AreEqual(40f, committed, 0.01f, "Clamped to max.");
            Assert.AreEqual(40f, handle.time, 0.01f);
        }

        [Test]
        public void Handle_PreviewThroughTheAxis_KeepsTheGripUnderThePointer()
        {
            var (timeline, handle) = MountHandle(20f);
            handle.dragMove += t => timeline.laneExtent = t;

            var from = handle.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(10f * k_Ppu, 0f));
            simulate.FrameUpdate();

            Assert.AreEqual(timeline.TimeToX(30f), handle.resolvedStyle.left, 0.5f,
                "A host previewing through the axis must not snap the grip back to its committed time.");
            simulate.MouseUp(from + new Vector2(10f * k_Ppu, 0f));
        }

        [Test]
        public void Handle_ClickWithoutMoving_CommitsNothing()
        {
            var (_, handle) = MountHandle(20f);
            var commits = 0;
            handle.changed += _ => commits++;

            simulate.Click(handle.worldBound.center);
            Assert.AreEqual(0, commits);
            Assert.AreEqual(20f, handle.time, 0.01f);
        }

        [Test]
        public void Handle_SecondaryButton_DoesNotDrag()
        {
            var (_, handle) = MountHandle(20f);
            var from = handle.worldBound.center;
            simulate.MouseDown(from, MouseButton.RightMouse);
            simulate.MouseMove(from + new Vector2(10f * k_Ppu, 0f));
            simulate.MouseUp(from + new Vector2(10f * k_Ppu, 0f), MouseButton.RightMouse);
            Assert.AreEqual(20f, handle.time, 0.01f);
        }

        [Test]
        public void Handle_Keys_StepAndJumpToTheBounds_AndPanIntoView()
        {
            var (timeline, handle) = MountHandle(0f, width: 400f);
            handle.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual(1f, handle.time, 0.01f, "One Timeline step.");

            simulate.KeyPress(KeyCode.End);
            Assert.AreEqual(100f, handle.time, 0.01f);
            var x = timeline.TimeToX(100f) - timeline.offset;
            Assert.That(x, Is.InRange(0f, timeline.viewport.layout.width), "A keyboard move pans the grip into view.");

            simulate.KeyPress(KeyCode.Home);
            Assert.AreEqual(0f, handle.time, 0.01f);
        }

        [Test]
        public void Handle_AtThePlayheadsTime_IsTheOneGrabbed()
        {
            var (timeline, handle) = MountHandle(50f);
            timeline.value = 50f;
            simulate.FrameUpdate();

            var at = handle.worldBound.center;
            Assert.IsTrue(timeline.playhead.worldBound.Contains(at), "Positive control: the playhead covers the grip.");

            var picked = timeline.panel.Pick(at);
            Assert.AreSame(handle, picked, "The grip must win the pick over the playhead drawn above it.");

            var away = new Vector2(at.x, timeline.ruler.worldBound.center.y);
            Assert.AreSame(timeline.playhead, timeline.panel.Pick(away), "Off the grip, the playhead still picks.");
        }

        // ------------------------------------------------------------------ key size

        [Test]
        public void KeySize_DefaultsToSmallForMutedKeysOnly()
        {
            var track = new TimelineTrack { id = "a" };
            var key = new TimelineKeyframe { time = 10f };
            var muted = new TimelineKeyframe { time = 20f, muted = true };
            track.Add(key);
            track.Add(muted);
            MountTimeline(track);

            Assert.IsFalse(key.ClassListContains(TimelineKeyframe.smallUssClassName));
            Assert.IsTrue(muted.ClassListContains(TimelineKeyframe.smallUssClassName));
        }

        [Test]
        public void KeySize_OnTheTimelineSizesEveryKey_AndAKeysOwnSizeWins()
        {
            var track = new TimelineTrack { id = "a" };
            var key = new TimelineKeyframe { time = 10f };
            var muted = new TimelineKeyframe { time = 20f, muted = true };
            var own = new TimelineKeyframe { time = 30f, size = TimelineKeySize.Small };
            track.Add(key);
            track.Add(muted);
            track.Add(own);
            var timeline = MountTimeline(track);

            timeline.keySize = TimelineKeySize.Regular;
            Assert.IsFalse(muted.ClassListContains(TimelineKeyframe.smallUssClassName), "One size for the sheet.");
            Assert.IsTrue(own.ClassListContains(TimelineKeyframe.smallUssClassName), "A key's own size wins.");

            timeline.keySize = TimelineKeySize.Small;
            simulate.FrameUpdate();
            Assert.IsTrue(key.ClassListContains(TimelineKeyframe.smallUssClassName));
            Assert.AreEqual(0.8f, key.Q(className: TimelineKeyframe.glyphUssClassName).resolvedStyle.scale.value.x, 0.01f,
                "The class shrinks the glyph.");
        }
    }
}
#endif
