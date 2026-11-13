#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using BoundsIntField = Unity.AppUI.UI.BoundsIntField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsIntField))]
    class BoundsIntFieldBehaviourTests : UIComponentTests<BoundsIntField>
    {
        static IntField XFieldOf(BoundsIntField field) =>
            field.Q<IntField>(name: BoundsIntField.xFieldUssClassName);

        static IntField SxFieldOf(BoundsIntField field) =>
            field.Q<IntField>(name: BoundsIntField.sxFieldUssClassName);

        [Test]
        public void BoundsIntField_EditingPositionSubField_ShouldNotifyValueChangedWithUpdatedPosition()
        {
            var field = MountElement(new BoundsIntField
            {
                value = new BoundsInt(Vector3Int.zero, new Vector3Int(1, 1, 1)),
            });
            BoundsInt? notified = null;
            field.RegisterValueChangedCallback(evt => notified = evt.newValue);

            XFieldOf(field).value = 5;

            Assert.IsTrue(notified.HasValue, "Committing the X sub-field should raise a value changed event.");
            Assert.AreEqual(5, notified.Value.position.x,
                "The BoundsIntField value should reflect the edited position X component.");
        }

        [Test]
        public void BoundsIntField_EditingSizeSubField_ShouldUpdateValueSize()
        {
            var field = MountElement(new BoundsIntField
            {
                value = new BoundsInt(Vector3Int.zero, new Vector3Int(1, 1, 1)),
            });

            SxFieldOf(field).value = 7;

            Assert.AreEqual(7, field.value.size.x,
                "Editing the size X sub-field should update the BoundsIntField's size.");
        }

        [Test]
        public void BoundsIntField_SetValue_ShouldRefreshSubFieldsWithoutNotifyLoop()
        {
            var field = MountElement(new BoundsIntField());
            var notifications = 0;
            field.RegisterValueChangedCallback(_ => notifications++);

            field.value = new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6));

            Assert.AreEqual(1, notifications, "Setting value programmatically should notify exactly once.");
            Assert.AreEqual(1, XFieldOf(field).value,
                "The X sub-field should reflect the value that was set.");
        }
    }
}
#endif
