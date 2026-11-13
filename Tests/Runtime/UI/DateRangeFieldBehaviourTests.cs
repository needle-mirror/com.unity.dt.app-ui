#if APPUI_UITF_PRESENT
using System;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DateRangeField))]
    class DateRangeFieldBehaviourTests : UIComponentTests<DateRangeField>
    {
        static readonly Date k_Start = new Date(new DateTime(2024, 3, 7));

        static readonly Date k_End = new Date(new DateTime(2024, 3, 10));

        DateRangeField MountField() => MountElement(new DateRangeField
        {
            value = new DateRange(k_Start, k_End),
            style = { width = 260 },
        });

        static TextElement StartInputTextOf(DateRangeField field) =>
            field.Q<UnityEngine.UIElements.TextField>(name: DateRangeField.inputUssClassName + "__start").Q<TextElement>();

        [Test]
        public void DateRangeField_UpArrowOnStartInput_ShouldAddOneDayToStart()
        {
            var field = MountField();

            StartInputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(new Date(((DateTime)k_Start).AddDays(1)), field.value.start,
                "Up arrow on the start input should step the start date forward by one day.");
            Assert.AreEqual(k_End, field.value.end, "The end date should be unaffected.");
        }

        [Test]
        public void DateRangeField_SetValue_ShouldSendChangeEvent()
        {
            var field = MountField();
            DateRange? received = null;
            field.RegisterCallback<ChangeEvent<DateRange>>(evt => received = evt.newValue);

            var newRange = new DateRange(k_Start, new Date(((DateTime)k_End).AddDays(1)));
            field.value = newRange;

            Assert.IsNotNull(received, "Setting the value should send a ChangeEvent<DateRange>.");
            Assert.AreEqual(newRange, received.Value);
        }

        [Test]
        public void DateRangeField_WhenDisabled_UpArrowOnStartInput_ShouldNotChangeValue()
        {
            var field = MountField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            StartInputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(k_Start, field.value.start, "A disabled DateRangeField should not respond to keyboard input.");
        }
    }
}
#endif
