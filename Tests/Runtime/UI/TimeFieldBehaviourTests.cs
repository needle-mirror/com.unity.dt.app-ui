#if APPUI_UITF_PRESENT
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TimeField))]
    class TimeFieldBehaviourTests : UIComponentTests<TimeField>
    {
        static UnityEngine.UIElements.TextField InputFieldOf(TimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: TimeField.inputUssClassName);

        static UnityEngine.UIElements.VisualElement PickerButtonOf(TimeField field) =>
            field.Q(className: TimeField.pickerButtonUssClassName);

        TimeField MountTimeField() => MountElement(new TimeField { style = { width = 160 } });

        [Test]
        public void TimeField_TypingAValidTime_ShouldUpdateValueAndNotify()
        {
            var field = MountTimeField();
            TimeSpan? notified = null;
            field.RegisterValueChangedCallback(evt => notified = evt.newValue);

            InputFieldOf(field).value = "09:30";

            Assert.AreEqual(new TimeSpan(9, 30, 0), field.value, "A valid typed time should become the field's value.");
            Assert.AreEqual(new TimeSpan(9, 30, 0), notified, "The change should be reported through RegisterValueChangedCallback.");
        }

        [Test]
        public void TimeField_TypingAnUnparsableString_ShouldKeepThePreviousValue()
        {
            var field = MountTimeField();
            field.value = new TimeSpan(9, 30, 0);

            InputFieldOf(field).value = "not a time";

            Assert.AreEqual(new TimeSpan(9, 30, 0), field.value,
                "An unparsable string should leave the current value untouched.");
        }

        [Test]
        public void TimeField_ClickPickerButton_ShouldFocusTheField()
        {
            var field = MountTimeField();

            simulate.Click(PickerButtonOf(field));

            Assert.IsTrue(field.ClassListContains(Styles.focusedUssClassName),
                "Clicking the picker button should open the picker and mark the field focused.");
        }

        [Test]
        public void TimeField_WhenDisabled_ClickPickerButton_ShouldNotOpenPicker()
        {
            var field = MountTimeField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(PickerButtonOf(field));

            Assert.IsFalse(field.ClassListContains(Styles.focusedUssClassName),
                "A disabled TimeField should not open its picker on click.");
        }
    }
}
#endif
