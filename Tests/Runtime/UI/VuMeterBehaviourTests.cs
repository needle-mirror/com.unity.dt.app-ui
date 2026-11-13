#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VuMeter))]
    class VuMeterBehaviourTests : UIComponentTests<VuMeter>
    {
        VuMeter MountMeter() => MountElement(new VuMeter
        {
            min = -80f,
            max = 20f,
            fader = 0f,
            style = { width = 80, height = 300 },
        });

        static VisualElement DragLaneOf(VuMeter meter) =>
            meter.Q(className: VuMeter.dragLaneUssClassName);

        /// <summary>
        /// Drags vertically from the centre of <paramref name="target"/> by <paramref name="dy"/>
        /// panel pixels. A negative <paramref name="dy"/> drags upward.
        /// </summary>
        void DragBy(VisualElement target, float dy)
        {
            var from = target.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(0f, dy));
            simulate.FrameUpdate();
        }

        /// <summary>
        /// Drags vertically by <paramref name="dy"/> panel pixels, starting from the point on
        /// <paramref name="target"/> that is currently aligned with the fader (the marker's own
        /// worldBound.center.y), not from the target's own centre. VuMeter.OnDrag sets the fader
        /// from the pointer's absolute position along the lane rather than from a delta off the
        /// press point, so starting anywhere else teleports the fader to that arbitrary point on
        /// the very first move event, before <paramref name="dy"/> is ever applied.
        /// </summary>
        void DragFaderAlignedBy(VisualElement target, VisualElement marker, float dy)
        {
            var from = new Vector2(target.worldBound.center.x, marker.worldBound.center.y);
            simulate.DragAndDrop(from, from + new Vector2(0f, dy));
            simulate.FrameUpdate();
        }

        [Test]
        public void VuMeter_DragOnDragLane_ShouldChangeFader()
        {
            var meter = MountMeter();
            var dragLane = DragLaneOf(meter);
            Assert.IsNotNull(dragLane, "The VuMeter should expose a drag lane to grab the fader from.");

            // The drag lane spans the meter's full height, so its own centre has no relation to
            // the current fader value. VuMeter.OnDrag reads the pointer's absolute position along
            // the lane, so starting the press at the lane's centre would itself relocate the fader
            // to the lane's midpoint value (-30 dB here) before any upward movement is applied.
            // Start the press at the y that already matches the fader (the marker's own position,
            // since RefreshFader keeps it in sync) so the drag measures purely the -60px move.
            // Dragging upward reduces the local y, which SetFaderFromLocalY reads as a higher fader.
            DragFaderAlignedBy(dragLane, meter.markerElement, -60f);

            Assert.Greater(meter.fader, 0f, "Dragging the lane upward should raise the fader above its start value.");
        }

        [Test]
        public void VuMeter_DragOnDragLane_ShouldNotifyFaderChanged()
        {
            var meter = MountMeter();
            var notifications = 0;
            meter.faderChanged += _ => notifications++;

            DragBy(DragLaneOf(meter), -60f);

            Assert.Greater(notifications, 0, "A drag that moves the fader should raise faderChanged at least once.");
        }

        [Test]
        public void VuMeter_DragOnMarker_ShouldChangeFader()
        {
            var meter = MountMeter();
            var marker = meter.markerElement;
            Assert.IsNotNull(marker, "The VuMeter should expose its marker element.");

            DragBy(marker, -60f);

            Assert.Greater(meter.fader, 0f, "Dragging the marker upward should raise the fader above its start value.");
        }

        [Test]
        public void VuMeter_DragBelowMinimum_ShouldClampFaderToMin()
        {
            var meter = MountMeter();

            DragBy(DragLaneOf(meter), 10000f);

            Assert.AreEqual(meter.min, meter.fader, "The fader should clamp at the meter's minimum, never leave the scale.");
        }
    }
}
#endif
