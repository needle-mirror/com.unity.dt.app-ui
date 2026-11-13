#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TouchSliderFloat))]
    class TouchSliderFloatBehaviourTests : UIComponentTests<TouchSliderFloat>
    {
        TouchSliderFloat MountSlider() => MountElement(new TouchSliderFloat
        {
            lowValue = 0f,
            highValue = 100f,
            value = 0f,
            style = { width = 400 },
        });

        /// <summary>
        /// Drags horizontally from the centre of <paramref name="slider"/> by
        /// <paramref name="distance"/> panel pixels.
        /// </summary>
        void DragBy(VisualElement slider, float distance)
        {
            var from = slider.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(distance, 0f));
            simulate.FrameUpdate();
        }

        [Test]
        public void TouchSlider_HorizontalDrag_ShouldIncreaseValue()
        {
            var slider = MountSlider();
            Assert.Greater(slider.worldBound.width, 0f, "The slider needs a resolved layout to be draggable.");

            DragBy(slider, 100f);

            Assert.Greater(slider.value, 0f, "Dragging right should raise the value.");
            Assert.LessOrEqual(slider.value, slider.highValue);
        }

        [Test]
        public void TouchSlider_HorizontalDrag_ShouldNotifyValueChanged()
        {
            var slider = MountSlider();
            var notifications = 0;
            slider.RegisterValueChangedCallback(_ => notifications++);

            DragBy(slider, 100f);

            Assert.Greater(notifications, 0, "A drag should notify at least once.");
        }

        [Test]
        public void TouchSlider_WhenDisabled_Drag_ShouldNotChangeValue()
        {
            var slider = MountSlider();
            slider.SetEnabled(false);
            simulate.FrameUpdate();

            DragBy(slider, 100f);

            Assert.AreEqual(0f, slider.value);
        }
    }
}
#endif
