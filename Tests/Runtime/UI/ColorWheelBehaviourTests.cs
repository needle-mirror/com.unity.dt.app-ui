#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorWheel))]
    class ColorWheelBehaviourTests : UIComponentTests<ColorWheel>
    {
        ColorWheel MountWheel() => MountElement(new ColorWheel { style = { width = 200, height = 200 } });

        [Test]
        public void ColorWheel_Drag_ShouldChangeValue()
        {
            var wheel = MountWheel();
            wheel.value = 0f;
            simulate.FrameUpdate();

            // Drag from the right edge (hue 0) to the top edge (hue 0.25), both on the ring.
            var center = wheel.worldBound.center;
            var from = center + new Vector2(wheel.worldBound.width * 0.4f, 0f);
            var to = center + new Vector2(0f, -wheel.worldBound.height * 0.4f);
            simulate.DragAndDrop(from, to);
            simulate.FrameUpdate();

            Assert.AreNotEqual(0f, wheel.value, "Dragging around the ring should change the hue value.");
        }

        [Test]
        public void ColorWheel_Drag_ShouldSendChangeEvent()
        {
            var wheel = MountWheel();
            wheel.value = 0f;
            simulate.FrameUpdate();
            var notified = false;
            wheel.RegisterCallback<ChangeEvent<float>>(_ => notified = true);

            var center = wheel.worldBound.center;
            var from = center + new Vector2(wheel.worldBound.width * 0.4f, 0f);
            var to = center + new Vector2(0f, -wheel.worldBound.height * 0.4f);
            simulate.DragAndDrop(from, to);
            simulate.FrameUpdate();

            Assert.IsTrue(notified, "A drag that changes the hue should send a ChangeEvent<float> on release.");
        }

        [Test]
        public void ColorWheel_RightArrowKey_ShouldIncreaseValue()
        {
            var wheel = MountWheel();
            wheel.value = 0.5f;
            simulate.FrameUpdate();

            wheel.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);

            Assert.Greater(wheel.value, 0.5f, "Right arrow should nudge the hue up by incrementFactor.");
        }

        [Test]
        public void ColorWheel_WhenDisabled_Drag_ShouldNotChangeValue()
        {
            var wheel = MountWheel();
            wheel.value = 0f;
            wheel.SetEnabled(false);
            simulate.FrameUpdate();

            var center = wheel.worldBound.center;
            var from = center + new Vector2(wheel.worldBound.width * 0.4f, 0f);
            var to = center + new Vector2(0f, -wheel.worldBound.height * 0.4f);
            simulate.DragAndDrop(from, to);
            simulate.FrameUpdate();

            Assert.AreEqual(0f, wheel.value, "A disabled ColorWheel should not respond to a drag.");
        }
    }
}
#endif
