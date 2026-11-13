#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using RectIntField = Unity.AppUI.UI.RectIntField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectIntField))]
    class RectIntFieldBehaviourTests : UIComponentTests<RectIntField>
    {
        RectIntField MountField() => MountElement(new RectIntField { style = { width = 300 } });

        static IntField XFieldOf(RectIntField field) => field.Q<IntField>(RectIntField.xFieldUssClassName);

        static IntField WFieldOf(RectIntField field) => field.Q<IntField>(RectIntField.wFieldUssClassName);

        [Test]
        public void RectIntField_ChangingXSubField_ShouldUpdateValue()
        {
            var field = MountField();
            field.value = new RectInt(0, 0, 10, 10);

            XFieldOf(field).value = 5;

            Assert.AreEqual(5, field.value.x, "Changing the X sub-field should update the RectIntField's value.");
        }

        [Test]
        public void RectIntField_ChangingSubField_ShouldSendChangeEvent()
        {
            var field = MountField();
            field.value = new RectInt(0, 0, 10, 10);
            RectInt? newValue = null;
            field.RegisterCallback<ChangeEvent<RectInt>>(evt => newValue = evt.newValue);

            WFieldOf(field).value = 42;

            Assert.IsTrue(newValue.HasValue, "Changing a sub-field should send a ChangeEvent<RectInt>.");
            Assert.AreEqual(42, newValue.Value.width, "The change event's new value should carry the updated width.");
        }

        [Test]
        public void RectIntField_SetValue_ShouldUpdateSubFields()
        {
            var field = MountField();

            field.value = new RectInt(1, 2, 3, 4);

            Assert.AreEqual(1, XFieldOf(field).value, "Setting the RectIntField's value should push it down to the X sub-field.");
            Assert.AreEqual(4, field.Q<IntField>(RectIntField.hFieldUssClassName).value, "...and to the H sub-field.");
        }
    }
}
#endif
