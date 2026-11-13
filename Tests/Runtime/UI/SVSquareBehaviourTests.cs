#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SVSquare))]
    class SVSquareBehaviourTests : UIComponentTests<SVSquare>
    {
        SVSquare MountSquare() => MountElement(new SVSquare { style = { width = 200, height = 200 } });

        [Test]
        public void SVSquare_Drag_ShouldChangeValue()
        {
            var square = MountSquare();
            square.value = Vector2.zero;
            simulate.FrameUpdate();

            // Drag from the bottom-left corner (saturation 0, brightness 0) toward the centre.
            var from = square.worldBound.min + new Vector2(2f, square.worldBound.height - 2f);
            var to = square.worldBound.center;
            simulate.DragAndDrop(from, to);
            simulate.FrameUpdate();

            Assert.Greater(square.value.x, 0f, "Dragging toward the centre should raise saturation.");
            Assert.Greater(square.value.y, 0f, "Dragging toward the centre should raise brightness.");
        }

        [Test]
        public void SVSquare_Drag_ShouldSendChangeEvent()
        {
            var square = MountSquare();
            square.value = Vector2.zero;
            simulate.FrameUpdate();
            var notified = false;
            square.RegisterCallback<ChangeEvent<Vector2>>(_ => notified = true);

            var from = square.worldBound.min + new Vector2(2f, square.worldBound.height - 2f);
            simulate.DragAndDrop(from, square.worldBound.center);
            simulate.FrameUpdate();

            Assert.IsTrue(notified, "A drag that changes the value should send a ChangeEvent<Vector2> on release.");
        }

        [Test]
        public void SVSquare_RightArrowKey_ShouldIncreaseSaturation()
        {
            var square = MountSquare();
            square.value = new Vector2(0.5f, 0.5f);
            simulate.FrameUpdate();

            square.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);

            Assert.Greater(square.value.x, 0.5f, "Right arrow should nudge saturation up by incrementFactor.");
        }

        [Test]
        public void SVSquare_WhenDisabled_Drag_ShouldNotChangeValue()
        {
            var square = MountSquare();
            square.value = Vector2.zero;
            square.SetEnabled(false);
            simulate.FrameUpdate();

            var from = square.worldBound.min + new Vector2(2f, square.worldBound.height - 2f);
            simulate.DragAndDrop(from, square.worldBound.center);
            simulate.FrameUpdate();

            Assert.AreEqual(Vector2.zero, square.value, "A disabled SVSquare should not respond to a drag.");
        }
    }
}
#endif
