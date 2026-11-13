#if APPUI_UITF_PRESENT
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldBehaviourTests : UIComponentTests<ColorField>
    {
        ColorField MountField() => MountElement(new ColorField { inlinePicker = true });

        [Test]
        public void ColorField_SetValue_ShouldSendChangeEvent()
        {
            var field = MountField();
            Color? received = null;
            field.RegisterCallback<ChangeEvent<Color>>(evt => received = evt.newValue);

            field.value = Color.red;

            Assert.IsNotNull(received, "Setting the value should send a ChangeEvent<Color>.");
            Assert.AreEqual(Color.red, received.Value);
        }

        [Test]
        public void ColorField_SetSameValue_ShouldNotSendChangeEvent()
        {
            var field = MountField();
            field.value = Color.red;
            var notifications = 0;
            field.RegisterCallback<ChangeEvent<Color>>(_ => notifications++);

            field.value = Color.red;

            Assert.AreEqual(0, notifications, "Re-assigning the same color should not raise a ChangeEvent.");
        }

        [Test]
        public void ColorField_Click_WithInlinePicker_ShouldOpenPicker()
        {
            var field = MountField();
            Assert.IsFalse(field.ClassListContains(ColorField.inlinePickerOpenedUssClassName));

            // ColorField's own worldBound center usually lands on the read-only label TextField,
            // which stops PointerDownEvent propagation so typing/selecting text doesn't reopen the
            // picker. Click the color-picker icon instead: it has PickingMode.Ignore, so the click
            // falls through to the field itself and reaches its Pressable.
            var icon = field.Q(name: ColorField.colorPickerIconUssClassName);
            Assert.IsNotNull(icon, "The ColorField should have a color-picker icon element.");
            simulate.Click(icon);

            Assert.IsTrue(field.ClassListContains(ColorField.inlinePickerOpenedUssClassName),
                "Clicking a ColorField with inlinePicker enabled should open the inline picker.");
            Assert.IsNotNull(appUiPanel.Q<ColorPicker>(), "The inline ColorPicker should be inserted into the hierarchy.");
        }

        [Test]
        public void ColorField_InlinePickerDrag_PreviewsLiveBeforeRelease()
        {
            var field = MountField();
            field.value = Color.red;
            simulate.FrameUpdate();
            simulate.Click(field.Q(name: ColorField.colorPickerIconUssClassName));
            simulate.FrameUpdate();

            var picker = appUiPanel.Q<ColorPicker>();
            var before = picker.value;
            var square = picker.Q<SVSquare>();
            var b = square.worldBound;
            simulate.MouseDown(b.center);
            simulate.MouseMove(b.center + new Vector2(b.width * 0.15f, 0f));
            simulate.MouseMove(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
            simulate.FrameUpdate();

            Assert.AreNotEqual(before, picker.value, "The drag should move the inline picker.");
            Assert.AreEqual(picker.value, field.value,
                "The ColorField should follow the inline picker while the drag is still in progress.");
            simulate.MouseUp(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
        }

        [Test]
        public void ColorField_PopoverPickerDrag_PreviewsLiveBeforeRelease()
        {
            var field = MountElement(new ColorField { inlinePicker = false, style = { width = 120 } });
            simulate.Click(field.Q(name: ColorField.colorPickerIconUssClassName));
            simulate.FrameUpdate();
            for (var i = 0; i < 30; i++)
                simulate.FrameUpdate(0.05);

            var picker = field.panel.visualTree.Q<ColorPicker>();
            Assert.IsNotNull(picker, "Clicking the field should open the popover picker.");
            var before = picker.value;
            var square = picker.Q<SVSquare>();
            var b = square.worldBound;
            simulate.MouseDown(b.center);
            simulate.MouseMove(b.center + new Vector2(b.width * 0.15f, 0f));
            simulate.MouseMove(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
            simulate.FrameUpdate();

            Assert.AreNotEqual(before, picker.value, "The drag should move the popover picker.");
            Assert.AreEqual(picker.value, field.value,
                "The ColorField should follow the popover picker while the drag is still in progress.");
            simulate.MouseUp(b.center + new Vector2(b.width * 0.3f, b.height * 0.3f));
        }

        [Test]
        public void ColorField_WhenDisabled_Click_ShouldNotOpenPicker()
        {
            var field = MountField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field);

            Assert.IsFalse(field.ClassListContains(ColorField.inlinePickerOpenedUssClassName),
                "A disabled ColorField should not open its picker on click.");
        }
    }
}
#endif
