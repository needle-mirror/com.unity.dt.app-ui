#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3IntField = Unity.AppUI.UI.Vector3IntField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Vector3IntField))]
    class Vector3IntFieldBehaviourTests : UIComponentTests<Vector3IntField>
    {
        static IntField XFieldOf(Vector3IntField field) =>
            field.Q<IntField>(name: Vector3IntField.xFieldUssClassName);

        static IntField YFieldOf(Vector3IntField field) =>
            field.Q<IntField>(name: Vector3IntField.yFieldUssClassName);

        static IntField ZFieldOf(Vector3IntField field) =>
            field.Q<IntField>(name: Vector3IntField.zFieldUssClassName);

        [Test]
        public void Vector3IntField_ChangingXSubField_ShouldUpdateValue()
        {
            var field = MountElement(new Vector3IntField { value = new Vector3Int(1, 2, 3) });

            XFieldOf(field).value = 5;

            Assert.AreEqual(new Vector3Int(5, 2, 3), field.value,
                "Changing the X sub-field should update only the X component of the value.");
        }

        [Test]
        public void Vector3IntField_ChangingZSubField_ShouldUpdateValue()
        {
            var field = MountElement(new Vector3IntField { value = new Vector3Int(1, 2, 3) });

            ZFieldOf(field).value = 9;

            Assert.AreEqual(new Vector3Int(1, 2, 9), field.value,
                "Changing the Z sub-field should update only the Z component of the value.");
        }

        [Test]
        public void Vector3IntField_ChangingSubField_ShouldNotifyValueChanged()
        {
            var field = MountElement(new Vector3IntField { value = Vector3Int.zero });
            var notified = 0;
            Vector3Int newValue = default;
            field.RegisterValueChangedCallback(evt =>
            {
                notified++;
                newValue = evt.newValue;
            });

            YFieldOf(field).value = 7;

            Assert.AreEqual(1, notified, "A sub-field change should raise exactly one value-changed event.");
            Assert.AreEqual(new Vector3Int(0, 7, 0), newValue);
        }

        [Test]
        public void Vector3IntField_SettingSameValue_ShouldNotNotify()
        {
            var field = MountElement(new Vector3IntField { value = new Vector3Int(4, 4, 4) });
            var notified = 0;
            field.RegisterValueChangedCallback(_ => notified++);

            field.value = new Vector3Int(4, 4, 4);

            Assert.AreEqual(0, notified, "Re-assigning the same value should not raise a change event.");
        }
    }
}
#endif
