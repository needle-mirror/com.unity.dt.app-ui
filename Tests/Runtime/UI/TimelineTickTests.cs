#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    using Timeline = Unity.AppUI.UI.Timeline;

    /// <summary>
    /// minTickStep, ticksAt and the fixed-tick coarsening, from registry #1415.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineTickTests : UIComponentTests<Timeline>
    {
        // ------------------------------------------------------------------ minTickStep

        [Test]
        public void MinTickStep_LetsAnAdaptiveAxisSubdivideTheUnit()
        {
            var timeline = new Timeline { duration = 10f, adaptiveTicks = true, minTickStep = 0.1f };

            timeline.ResolveTicks(200f, out var minor, out var major);
            Assert.AreEqual(0.1f, minor, 1e-4f, "200 px per unit leaves 20px between tenths: fine enough.");
            Assert.Greater(major, minor);
            var ratio = major / minor;
            Assert.AreEqual(Mathf.Round(ratio), ratio, 1e-3f, "Every label sits on a tick.");

            timeline.ResolveTicks(40f, out minor, out _);
            Assert.AreEqual(0.2f, minor, 1e-4f, "Zoomed out, the next level up takes over.");
        }

        [Test]
        public void MinTickStep_Unset_KeepsTheAdaptiveAxisOnMinorTick()
        {
            var timeline = new Timeline { duration = 10f, adaptiveTicks = true };
            Assert.AreEqual(0f, timeline.minTickStep);

            timeline.ResolveTicks(200f, out var minor, out _);
            Assert.AreEqual(1f, minor, 1e-4f, "Without minTickStep the floor stays minorTick.");
        }

        [Test]
        public void MinTickStep_DoesNotTouchAFixedAxis()
        {
            var timeline = new Timeline { duration = 10f, minTickStep = 0.1f, minorTick = 1f, majorTick = 5f };
            timeline.ResolveTicks(200f, out var minor, out var major);
            Assert.AreEqual(1f, minor);
            Assert.AreEqual(5f, major);
        }

        // ------------------------------------------------------------------ coarsening

        [Test]
        public void FixedTicks_PastTheCap_CoarsenAndStillCoverTheWholeDuration()
        {
            const float duration = 50000f;
            var timeline = new Timeline { duration = duration, minorTick = 1f, majorTick = 10f };
            var axis = new TimelineAxisContext(timeline, duration, 1f, Timeline.origin, 0f, 0f,
                1f, 10f, 24f, 24f, false, null);
            var ticks = new List<TimelineTick>();
            Timeline.CollectTicks(axis, ticks);

            Assert.Greater(ticks.Count, 0, "Too many ticks must coarsen, not vanish.");
            Assert.LessOrEqual(ticks.Count, Timeline.maxTicks + 1);
            var step = ticks[1].time - ticks[0].time;
            Assert.GreaterOrEqual(ticks.Max(t => t.time), duration - step, "The ticks reach the end of the axis.");
            Assert.IsTrue(ticks.Any(t => t.major), "Majors still land on the coarser step.");
        }

        // ------------------------------------------------------------------ ticksAt

        static List<VisualElement> Ticks(Timeline t) =>
            t.ruler.Query<VisualElement>(className: TimelineRuler.tickUssClassName).ToList();

        [Test]
        public void TicksAt_DefaultsToTheRulersFoot_AndTopMovesThemToItsHead()
        {
            var timeline = MountElement(new Timeline { duration = 20f, style = { width = 400, height = 200 } });
            Assert.AreEqual(TimelineTicksAt.Bottom, timeline.ticksAt);
            var rulerHeight = timeline.ruler.layout.height;
            var tick = Ticks(timeline)[1];
            Assert.AreEqual(rulerHeight, tick.layout.yMax, 1.5f, "Bottom: the ticks stand on the ruler's foot.");

            timeline.ticksAt = TimelineTicksAt.Top;
            simulate.FrameUpdate();
            tick = Ticks(timeline)[1];
            Assert.AreEqual(0f, tick.layout.yMin, 1.5f, "Top: the ticks hang from the ruler's head.");
        }
    }
}
#endif
