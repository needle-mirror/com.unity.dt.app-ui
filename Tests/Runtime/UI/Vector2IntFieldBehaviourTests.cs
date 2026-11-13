#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2IntField = Unity.AppUI.UI.Vector2IntField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Vector2IntField))]
    class Vector2IntFieldBehaviourTests : UIComponentTests<Vector2IntField>
    {
        static IntField XFieldOf(Vector2IntField field) =>
            field.Q<IntField>(name: Vector2IntField.xFieldUssClassName);

        static IntField YFieldOf(Vector2IntField field) =>
            field.Q<IntField>(name: Vector2IntField.yFieldUssClassName);

        [Test]
        public void Vector2IntField_ChangingXSubField_ShouldUpdateValue()
        {
            var field = MountElement(new Vector2IntField { value = new Vector2Int(1, 2) });

            XFieldOf(field).value = 5;

            Assert.AreEqual(new Vector2Int(5, 2), field.value,
                "Changing the X sub-field should update only the X component of the value.");
        }

        [Test]
        public void Vector2IntField_ChangingYSubField_ShouldUpdateValue()
        {
            var field = MountElement(new Vector2IntField { value = new Vector2Int(1, 2) });

            YFieldOf(field).value = 9;

            Assert.AreEqual(new Vector2Int(1, 9), field.value,
                "Changing the Y sub-field should update only the Y component of the value.");
        }

        [Test]
        public void Vector2IntField_ChangingSubField_ShouldNotifyValueChanged()
        {
            var field = MountElement(new Vector2IntField { value = Vector2Int.zero });
            var notified = 0;
            Vector2Int newValue = default;
            field.RegisterValueChangedCallback(evt =>
            {
                notified++;
                newValue = evt.newValue;
            });

            XFieldOf(field).value = 3;

            Assert.AreEqual(1, notified, "A sub-field change should raise exactly one value-changed event.");
            Assert.AreEqual(new Vector2Int(3, 0), newValue);
        }

        [Test]
        public void Vector2IntField_SettingSameValue_ShouldNotNotify()
        {
            var field = MountElement(new Vector2IntField { value = new Vector2Int(4, 4) });
            var notified = 0;
            field.RegisterValueChangedCallback(_ => notified++);

            field.value = new Vector2Int(4, 4);

            Assert.AreEqual(0, notified, "Re-assigning the same value should not raise a change event.");
        }
    }
}
#endif
