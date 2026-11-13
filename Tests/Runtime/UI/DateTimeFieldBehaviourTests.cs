#if APPUI_UITF_PRESENT
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DateTimeField))]
    class DateTimeFieldBehaviourTests : UIComponentTests<DateTimeField>
    {
        static readonly DateTime k_Value = new DateTime(2024, 3, 7, 14, 5, 0);

        DateTimeField MountField() => MountElement(new DateTimeField
        {
            value = k_Value,
            style = { width = 260 },
        });

        static TextElement DateInputTextOf(DateTimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: DateTimeField.dateInputUssClassName).Q<TextElement>();

        static TextElement TimeInputTextOf(DateTimeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: DateTimeField.timeInputUssClassName).Q<TextElement>();

        [Test]
        public void DateTimeField_UpArrowOnDateInput_ShouldAddOneDay()
        {
            var field = MountField();

            DateInputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(k_Value.AddDays(1), field.value, "Up arrow on the date input should step the date forward by one day.");
        }

        [Test]
        public void DateTimeField_UpArrowOnTimeInput_ShouldStepHourSegment()
        {
            var field = MountField();

            TimeInputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(k_Value.AddHours(1), field.value, "The caret starts in the hours segment by default.");
        }

        [Test]
        public void DateTimeField_SetValue_ShouldSendChangeEvent()
        {
            var field = MountField();
            DateTime? received = null;
            field.RegisterCallback<ChangeEvent<DateTime>>(evt => received = evt.newValue);

            var newValue = k_Value.AddDays(2);
            field.value = newValue;

            Assert.IsNotNull(received, "Setting the value should send a ChangeEvent<DateTime>.");
            Assert.AreEqual(newValue, received.Value);
        }

        [Test]
        public void DateTimeField_WhenDisabled_UpArrowOnDateInput_ShouldNotChangeValue()
        {
            var field = MountField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            DateInputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(k_Value, field.value, "A disabled DateTimeField should not respond to keyboard input.");
        }
    }
}
#endif
