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
    [TestOf(typeof(DateField))]
    class DateFieldBehaviourTests : UIComponentTests<DateField>
    {
        DateField MountField() => MountElement(new DateField
        {
            value = new Date(new DateTime(2024, 3, 7)),
            style = { width = 160 },
        });

        static TextElement InputTextOf(DateField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: DateField.inputUssClassName).Q<TextElement>();

        [Test]
        public void DateField_Click_ShouldOpenPickerAndFocusState()
        {
            var field = MountField();
            Assert.IsFalse(field.ClassListContains(Styles.focusedUssClassName));

            simulate.Click(field.Q(name: null, className: DateField.pickerButtonUssClassName));

            Assert.IsTrue(field.ClassListContains(Styles.focusedUssClassName),
                "Opening the picker should mark the field as focused.");
        }

        [Test]
        public void DateField_UpArrowOnInput_ShouldAddOneDay()
        {
            var field = MountField();
            var before = field.value;

            InputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.UpArrow);

            Assert.AreEqual(new Date(((DateTime)before).AddDays(1)), field.value,
                "Up arrow on the text input should step the date forward by one day.");
        }

        [Test]
        public void DateField_DownArrowOnInput_ShouldSubtractOneDay()
        {
            var field = MountField();
            var before = field.value;

            InputTextOf(field).Focus();
            simulate.FrameUpdate();
            simulate.KeyPress(KeyCode.DownArrow);

            Assert.AreEqual(new Date(((DateTime)before).AddDays(-1)), field.value,
                "Down arrow on the text input should step the date backward by one day.");
        }

        [Test]
        public void DateField_WhenDisabled_Click_ShouldNotOpenPicker()
        {
            var field = MountField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field.Q(name: null, className: DateField.pickerButtonUssClassName));

            Assert.IsFalse(field.ClassListContains(Styles.focusedUssClassName),
                "A disabled DateField should not respond to a click on the picker button.");
        }
    }
}
#endif
