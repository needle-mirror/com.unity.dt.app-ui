#if APPUI_UITF_PRESENT
using System.Globalization;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    using Timeline = Unity.AppUI.UI.Timeline;

    /// <summary>
    /// The resizable gutter, the hover line and the corner time from registry #1415.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineGutterTests : UIComponentTests<Timeline>
    {
        Timeline MountTimeline(float width = 600f, float labelWidth = 120f, bool resizable = true)
        {
            var timeline = new Timeline
            {
                duration = 100f,
                pxPerUnit = 8f,
                labelWidth = labelWidth,
                labelResizable = resizable,
                style = { width = width, height = 200 },
            };
            timeline.Add(new TimelineTrack { id = "a", label = "A" });
            return MountElement(timeline);
        }

        float BodyWidth(Timeline t) => t.Q<VisualElement>(className: Timeline.bodyUssClassName).layout.width;

        // ------------------------------------------------------------------ clamp

        [Test]
        public void LabelWidth_WiderThanThePanel_KeepsMinLanesWidthForTheLanes()
        {
            var timeline = MountTimeline(width: 400f, labelWidth: 1000f, resizable: false);
            simulate.FrameUpdate();

            Assert.AreEqual(1000f, timeline.labelWidth, "The stored width is kept as asked.");
            Assert.AreEqual(BodyWidth(timeline) - Timeline.minLanesWidth, timeline.gutter.resolvedStyle.width, 0.5f,
                "The gutter must leave minLanesWidth for the lanes, with or without a sash.");
        }

        [Test]
        public void LabelResizable_RaisesANarrowWidthToMinLabelWidth()
        {
            var timeline = MountTimeline(labelWidth: 40f, resizable: false);
            Assert.AreEqual(40f, timeline.gutter.resolvedStyle.width, 0.5f, "Without a sash, a narrow width is honoured.");

            timeline.labelResizable = true;
            simulate.FrameUpdate();
            Assert.AreEqual(Timeline.defaultMinLabelWidth, timeline.gutter.resolvedStyle.width, 0.5f,
                "With a sash, the gutter renders at minLabelWidth at least.");
        }

        [Test]
        public void Sash_IsShownOnlyWhenLabelResizable()
        {
            var timeline = MountTimeline(resizable: false);
            Assert.AreEqual(DisplayStyle.None, timeline.sash.resolvedStyle.display);

            timeline.labelResizable = true;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.Flex, timeline.sash.resolvedStyle.display);
            Assert.Greater(timeline.sash.worldBound.width, 0f);
        }

        // ------------------------------------------------------------------ sash pointer

        [Test]
        public void Sash_Drag_ResizesTheGutterAndRaisesLabelWidthChanged()
        {
            var timeline = MountTimeline();
            var reported = -1f;
            timeline.labelWidthChanged += w => reported = w;

            var from = timeline.sash.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(30f, 0f));
            simulate.MouseMove(from + new Vector2(60f, 0f));
            simulate.MouseUp(from + new Vector2(60f, 0f));
            simulate.FrameUpdate();

            Assert.AreEqual(180f, timeline.labelWidth, 1f, "Dragging the sash 60px right widens the gutter by 60px.");
            Assert.AreEqual(timeline.labelWidth, reported, 0.01f, "labelWidthChanged carries the new width.");
            Assert.AreEqual(180f, timeline.gutter.resolvedStyle.width, 1f);
        }

        [Test]
        public void Sash_DragPastEitherEnd_ClampsBetweenMinAndMax()
        {
            var timeline = MountTimeline();

            var from = timeline.sash.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(-500f, 0f));
            simulate.MouseUp(from + new Vector2(-500f, 0f));
            simulate.FrameUpdate();
            Assert.AreEqual(Timeline.defaultMinLabelWidth, timeline.labelWidth, 0.01f, "Never under minLabelWidth.");

            from = timeline.sash.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(2000f, 0f));
            simulate.MouseUp(from + new Vector2(2000f, 0f));
            simulate.FrameUpdate();
            Assert.AreEqual(timeline.maxLabelWidth, timeline.labelWidth, 1f,
                "Never past the width that leaves minLanesWidth for the lanes.");
        }

        [Test]
        public void Sash_SecondaryButton_DoesNotResize()
        {
            var timeline = MountTimeline();
            var from = timeline.sash.worldBound.center;
            simulate.MouseDown(from, MouseButton.RightMouse);
            simulate.MouseMove(from + new Vector2(60f, 0f));
            simulate.MouseUp(from + new Vector2(60f, 0f), MouseButton.RightMouse);
            simulate.FrameUpdate();

            Assert.AreEqual(120f, timeline.labelWidth, 0.01f, "The sash answers the primary button only.");
        }

        [Test]
        public void Sash_WhenDisabled_DoesNotResize()
        {
            var timeline = MountTimeline();
            timeline.disabled = true;
            simulate.FrameUpdate();

            var from = timeline.sash.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(60f, 0f));
            simulate.MouseUp(from + new Vector2(60f, 0f));

            Assert.AreEqual(120f, timeline.labelWidth, 0.01f);
        }

        // ------------------------------------------------------------------ sash keyboard

        [Test]
        public void Sash_ArrowsStepBySashKeyStep_AndHomeGoesToTheMinimum()
        {
            var timeline = MountTimeline();
            timeline.sash.Focus();
            simulate.FrameUpdate();

            simulate.KeyPress(KeyCode.RightArrow);
            Assert.AreEqual(120f + Timeline.sashKeyStep, timeline.labelWidth, 0.01f);

            simulate.KeyPress(KeyCode.LeftArrow);
            simulate.KeyPress(KeyCode.LeftArrow);
            Assert.AreEqual(120f - Timeline.sashKeyStep, timeline.labelWidth, 0.01f);

            simulate.KeyPress(KeyCode.Home);
            Assert.AreEqual(Timeline.defaultMinLabelWidth, timeline.labelWidth, 0.01f);
        }

        // ------------------------------------------------------------------ hover line

        [Test]
        public void HoverLine_FollowsThePointerOverTheViewport_AndHidesOnLeave()
        {
            var timeline = MountTimeline();
            timeline.hoverLine = true;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, timeline.hoverLineElement.resolvedStyle.display, "Hidden until hovered.");

            var vp = timeline.viewport.worldBound;
            simulate.MouseMove(new Vector2(vp.x + 100f, vp.center.y));
            simulate.FrameUpdate();

            Assert.AreEqual(DisplayStyle.Flex, timeline.hoverLineElement.resolvedStyle.display);
            Assert.AreEqual(100f, timeline.hoverLineElement.resolvedStyle.left, 0.5f, "The line sits at the pointer.");

            simulate.MouseMove(new Vector2(vp.xMax + 300f, vp.yMax + 300f));
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, timeline.hoverLineElement.resolvedStyle.display, "Leaving hides it.");
        }

        [Test]
        public void HoverLine_OverTheOriginInset_HoldsAtTimeZero()
        {
            var timeline = MountTimeline();
            timeline.hoverLine = true;
            simulate.FrameUpdate();

            var vp = timeline.viewport.worldBound;
            simulate.MouseMove(new Vector2(vp.x + 3f, vp.center.y));
            simulate.FrameUpdate();

            Assert.AreEqual(Timeline.origin, timeline.hoverLineElement.resolvedStyle.left, 0.5f,
                "Nothing before time 0: over the inset the guide holds at the origin.");
        }

        [Test]
        public void HoverLine_Off_StaysHidden()
        {
            var timeline = MountTimeline();
            var vp = timeline.viewport.worldBound;
            simulate.MouseMove(new Vector2(vp.x + 100f, vp.center.y));
            simulate.FrameUpdate();

            Assert.AreEqual(DisplayStyle.None, timeline.hoverLineElement.resolvedStyle.display);
        }

        [Test]
        public void Sash_DisabledMidDrag_StopsResizing()
        {
            var timeline = MountTimeline();
            var from = timeline.sash.worldBound.center;
            simulate.MouseDown(from);
            simulate.MouseMove(from + new Vector2(30f, 0f));
            Assert.AreEqual(150f, timeline.labelWidth, 1f, "Positive control: the drag resizes.");

            timeline.disabled = true;
            simulate.MouseMove(from + new Vector2(60f, 0f));
            simulate.MouseUp(from + new Vector2(60f, 0f));

            Assert.AreEqual(150f, timeline.labelWidth, 1f, "Disabling mid-drag ends the gesture.");
            Assert.IsFalse(timeline.sash.HasPointerCapture(PointerId.mousePointerId), "And releases the capture.");
        }

        [Test]
        public void HoverLine_KeepsUpDuringARulerDrag()
        {
            var timeline = MountTimeline();
            timeline.hoverLine = true;
            simulate.FrameUpdate();

            var vp = timeline.viewport.worldBound;
            var y = timeline.ruler.worldBound.center.y;
            simulate.MouseDown(new Vector2(vp.x + 60f, y));
            simulate.MouseMove(new Vector2(vp.x + 160f, y));
            simulate.FrameUpdate();

            Assert.AreEqual(160f, timeline.hoverLineElement.resolvedStyle.left, 0.5f,
                "The ruler stops its drag moves, and the guide must still follow the pointer.");
            simulate.MouseUp(new Vector2(vp.x + 160f, y));
        }

        // ------------------------------------------------------------------ corner time

        [Test]
        public void CornerTime_FollowsAPanUnderAStillPointer()
        {
            var timeline = MountTimeline();
            timeline.timeLabel = Fmt;
            simulate.FrameUpdate();

            var vp = timeline.viewport.worldBound;
            simulate.MouseMove(new Vector2(vp.x + Timeline.origin + 10f * 8f, vp.center.y));
            simulate.FrameUpdate();
            Assert.AreEqual("10", timeline.cornerTime.text);

            timeline.offset = 80f;
            Assert.AreEqual("20", timeline.cornerTime.text, "A pan moves the time under the pointer.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_CornerRulesAreUnconditional_AndTheDisabledChevronKeepsItsPrefix()
        {
            var uss = System.IO.File.ReadAllText(System.IO.Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Timeline.uss"));
            StringAssert.IsMatch(@"(?m)^\.appui-timeline__corner-time\s*\{[^}]*font-size", uss,
                "The corner time's base rule must apply whether or not the Timeline is disabled.");
            StringAssert.IsMatch(@"(?m)^\.appui-timeline--disabled \.appui-timeline__gutter-chevron\s*\{[^}]*text-disabled", uss,
                "The disabled chevron tint must stay scoped to the disabled Timeline.");
        }

        static string Fmt(float t) => t.ToString("0.##", CultureInfo.InvariantCulture);

        [Test]
        public void CornerTime_ShowsThePointerTimeSnapped_ThenThePlayheadsOnLeave()
        {
            var timeline = MountTimeline();
            timeline.timeLabel = Fmt;
            timeline.value = 7f;
            simulate.FrameUpdate();
            Assert.AreEqual("7", timeline.cornerTime.text, "Not hovered: the playhead's time.");

            // origin + 10.4 units at 8 px/unit: the step-1 snap reads 10.
            var vp = timeline.viewport.worldBound;
            simulate.MouseMove(new Vector2(vp.x + Timeline.origin + 10.4f * 8f, vp.center.y));
            simulate.FrameUpdate();
            Assert.AreEqual("10", timeline.cornerTime.text, "Hovered: the pointer's time, snapped to step.");

            timeline.value = 20f;
            Assert.AreEqual("10", timeline.cornerTime.text, "A playhead move must not overwrite the pointer's time.");

            simulate.MouseMove(new Vector2(vp.xMax + 300f, vp.yMax + 300f));
            simulate.FrameUpdate();
            Assert.AreEqual("20", timeline.cornerTime.text, "Leaving goes back to the playhead's time.");
        }

        [Test]
        public void CornerTime_IsHiddenWithoutATimeLabel_AndShowsTheSuffix()
        {
            var timeline = MountTimeline();
            timeline.timeSuffix = "/ 100";
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, timeline.cornerTime.resolvedStyle.display);

            timeline.timeLabel = Fmt;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.Flex, timeline.cornerTime.resolvedStyle.display);
            var suffix = timeline.Q<LocalizedTextElement>(className: Timeline.cornerSuffixUssClassName);
            Assert.AreEqual(DisplayStyle.Flex, suffix.resolvedStyle.display);
            Assert.AreEqual("/ 100", suffix.text);
        }
    }
}
#endif
