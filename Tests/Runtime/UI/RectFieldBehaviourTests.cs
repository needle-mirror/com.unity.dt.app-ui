#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using FloatField = Unity.AppUI.UI.FloatField;
using RectField = Unity.AppUI.UI.RectField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectField))]
    class RectFieldBehaviourTests : UIComponentTests<RectField>
    {
        RectField MountField() => MountElement(new RectField { style = { width = 300 } });

        static FloatField XFieldOf(RectField field) => field.Q<FloatField>(RectField.xFieldUssClassName);

        static FloatField WFieldOf(RectField field) => field.Q<FloatField>(RectField.wFieldUssClassName);

        [Test]
        public void RectField_ChangingXSubField_ShouldUpdateValue()
        {
            var field = MountField();
            field.value = new Rect(0f, 0f, 10f, 10f);

            XFieldOf(field).value = 5f;

            Assert.AreEqual(5f, field.value.x, "Changing the X sub-field should update the RectField's value.");
        }

        [Test]
        public void RectField_ChangingSubField_ShouldSendChangeEvent()
        {
            var field = MountField();
            field.value = new Rect(0f, 0f, 10f, 10f);
            Rect? newValue = null;
            field.RegisterCallback<ChangeEvent<Rect>>(evt => newValue = evt.newValue);

            WFieldOf(field).value = 42f;

            Assert.IsTrue(newValue.HasValue, "Changing a sub-field should send a ChangeEvent<Rect>.");
            Assert.AreEqual(42f, newValue.Value.width, "The change event's new value should carry the updated width.");
        }

        [Test]
        public void RectField_SetValue_ShouldUpdateSubFields()
        {
            var field = MountField();

            field.value = new Rect(1f, 2f, 3f, 4f);

            Assert.AreEqual(1f, XFieldOf(field).value, "Setting the RectField's value should push it down to the X sub-field.");
            Assert.AreEqual(4f, field.Q<FloatField>(RectField.hFieldUssClassName).value, "...and to the H sub-field.");
        }
    }
}
#endif
