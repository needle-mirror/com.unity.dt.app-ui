#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// The picker brackets a drag on any sub-control as one gesture, as the registry's
    /// <c>color-picker</c> does: <see cref="ChangingEvent{T}"/>s while the pointer is down, then a
    /// single <see cref="ChangeEvent{T}"/> at release, so undo and consumers see one change per drag.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ColorPicker))]
    class ColorPickerBehaviourTests : UIComponentTests<ColorPicker>
    {
        static readonly Color k_Start = new Color(1f, 0f, 0f, 1f);

        ColorPicker MountPicker()
        {
            var picker = MountElement(new ColorPicker());
            picker.SetValueWithoutNotify(k_Start);
            simulate.FrameUpdate();
            return picker;
        }

        class Recorder
        {
            public int changingCount;
            public int changeCount;
            public Color changePrevious;
            public Color changeNew;
            public Color firstChangingPrevious;

            public Recorder(ColorPicker picker)
            {
                picker.RegisterCallback<ChangingEvent<Color>>(evt =>
                {
                    if (changingCount == 0)
                        firstChangingPrevious = evt.previousValue;
                    changingCount++;
                });
                picker.RegisterValueChangedCallback(evt =>
                {
                    changeCount++;
                    changePrevious = evt.previousValue;
                    changeNew = evt.newValue;
                });
            }
        }

        void DragInSteps(VisualElement target, Vector2 from, Vector2 to, Recorder rec, int steps = 4)
        {
            simulate.MouseDown(from);
            for (var i = 1; i <= steps; i++)
                simulate.MouseMove(Vector2.Lerp(from, to, i / (float)steps));
            simulate.FrameUpdate();

            Assert.Greater(rec.changingCount, 0, $"Dragging the {target.GetType().Name} should send ChangingEvent<Color>s.");
            Assert.AreEqual(0, rec.changeCount, $"No ChangeEvent<Color> may be sent while the {target.GetType().Name} drag is in progress.");

            simulate.MouseUp(to);
            simulate.FrameUpdate();
        }

        [Test]
        public void ColorPicker_DragOnSVSquare_SendsChangingThenOneChange()
        {
            var picker = MountPicker();
            var square = picker.Q<SVSquare>();
            var rec = new Recorder(picker);

            var b = square.worldBound;
            DragInSteps(square, b.center, b.center + new Vector2(b.width * 0.3f, b.height * 0.3f), rec);

            Assert.AreEqual(1, rec.changeCount, "Releasing the drag should send exactly one ChangeEvent<Color>.");
            Assert.AreEqual(k_Start, rec.changePrevious, "The ChangeEvent should span the whole drag, from the value before it.");
            Assert.AreEqual(picker.value, rec.changeNew);
            Assert.AreEqual(k_Start, rec.firstChangingPrevious, "ChangingEvents should report the value before the gesture as previous.");
        }

        [Test]
        public void ColorPicker_DragOnWheel_SendsChangingThenOneChange()
        {
            var picker = MountPicker();
            var wheel = picker.Q<ColorWheel>();
            var rec = new Recorder(picker);

            var b = wheel.worldBound;
            var from = b.center + new Vector2(b.width * 0.45f, 0f);
            var to = b.center + new Vector2(0f, -b.height * 0.45f);
            DragInSteps(wheel, from, to, rec);

            Assert.AreEqual(1, rec.changeCount, "Releasing the drag should send exactly one ChangeEvent<Color>.");
            Assert.AreEqual(k_Start, rec.changePrevious);
        }

        [Test]
        public void ColorPicker_CaptureStolenMidDrag_CommitsTheGesture()
        {
            var picker = MountPicker();
            var square = picker.Q<SVSquare>();
            var thief = new VisualElement();
            appUiPanel.Add(thief);
            var rec = new Recorder(picker);

            var b = square.worldBound;
            simulate.MouseDown(b.center);
            simulate.MouseMove(b.center + new Vector2(b.width * 0.15f, 0f));
            simulate.MouseMove(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
            simulate.FrameUpdate();
            Assert.AreEqual(0, rec.changeCount);

            thief.CapturePointer(PointerId.mousePointerId);
            // Capture changes are applied on the next pointer event, as when a SwipeView takes over mid-move.
            simulate.MouseMove(b.center + new Vector2(b.width * 0.35f, b.height * 0.35f));
            simulate.FrameUpdate();

            Assert.AreEqual(1, rec.changeCount, "Losing the pointer to another element should end the drag with one ChangeEvent<Color>.");
            Assert.AreEqual(k_Start, rec.changePrevious);

            thief.ReleasePointer(PointerId.mousePointerId);
            simulate.MouseUp(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
            simulate.FrameUpdate();

            picker.Q<ColorWheel>().Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);
            simulate.FrameUpdate();
            Assert.AreEqual(2, rec.changeCount, "After the stolen drag, a key press should still commit at once.");
        }

        [Test]
        public void ColorPicker_KeyboardOnWheel_CommitsEachPress()
        {
            var picker = MountPicker();
            var wheel = picker.Q<ColorWheel>();
            var rec = new Recorder(picker);

            wheel.Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.RightArrow);
            simulate.KeyPress(KeyCode.RightArrow);
            simulate.FrameUpdate();

            Assert.AreEqual(2, rec.changeCount, "A key press is a discrete edit: each one should commit a ChangeEvent<Color>.");
        }

        [Test]
        public void ColorPicker_SettingValue_SendsOneChangeAndNoChanging()
        {
            var picker = MountPicker();
            var rec = new Recorder(picker);

            picker.value = Color.blue;

            Assert.AreEqual(1, rec.changeCount);
            Assert.AreEqual(0, rec.changingCount);
        }
    }
}
#endif
