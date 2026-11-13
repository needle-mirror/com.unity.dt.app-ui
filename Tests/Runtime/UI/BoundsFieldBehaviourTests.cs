#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using BoundsField = Unity.AppUI.UI.BoundsField;
using FloatField = Unity.AppUI.UI.FloatField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsField))]
    class BoundsFieldBehaviourTests : UIComponentTests<BoundsField>
    {
        static FloatField XFieldOf(BoundsField field) =>
            field.Q<FloatField>(name: BoundsField.xFieldUssClassName);

        static FloatField SxFieldOf(BoundsField field) =>
            field.Q<FloatField>(name: BoundsField.sxFieldUssClassName);

        [Test]
        public void BoundsField_EditingCenterSubField_ShouldNotifyValueChangedWithUpdatedCenter()
        {
            var field = MountElement(new BoundsField { value = new Bounds(Vector3.zero, Vector3.one) });
            Bounds? notified = null;
            field.RegisterValueChangedCallback(evt => notified = evt.newValue);

            XFieldOf(field).value = 5f;

            Assert.IsTrue(notified.HasValue, "Committing the X sub-field should raise a value changed event.");
            Assert.AreEqual(5f, notified.Value.center.x,
                "The BoundsField value should reflect the edited center X component.");
        }

        [Test]
        public void BoundsField_EditingSizeSubField_ShouldUpdateValueSize()
        {
            var field = MountElement(new BoundsField { value = new Bounds(Vector3.zero, Vector3.one) });

            SxFieldOf(field).value = 42f;

            Assert.AreEqual(42f, field.value.size.x,
                "Editing the size X sub-field should update the BoundsField's size.");
        }

        [Test]
        public void BoundsField_SetValue_ShouldRefreshSubFieldsWithoutNotifyLoop()
        {
            var field = MountElement(new BoundsField());
            var notifications = 0;
            field.RegisterValueChangedCallback(_ => notifications++);

            field.value = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));

            Assert.AreEqual(1, notifications, "Setting value programmatically should notify exactly once.");
            Assert.AreEqual(1f, XFieldOf(field).value,
                "The center X sub-field should reflect the value that was set.");
        }
    }
}
#endif
