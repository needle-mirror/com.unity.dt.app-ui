#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using DoubleField = Unity.AppUI.UI.DoubleField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DoubleField))]
    class DoubleFieldBehaviourTests : UIComponentTests<DoubleField>
    {
        DoubleField MountField() => MountElement(new DoubleField { value = 1d, style = { width = 100 } });

        [Test]
        public void DoubleField_SetValue_ShouldSendChangeEvent()
        {
            var field = MountField();
            double? received = null;
            field.RegisterCallback<ChangeEvent<double>>(evt => received = evt.newValue);

            field.value = 42d;

            Assert.IsNotNull(received, "Setting the value should send a ChangeEvent<double>.");
            Assert.AreEqual(42d, received.Value);
        }

        [Test]
        public void DoubleField_SetSameValue_ShouldNotSendChangeEvent()
        {
            var field = MountField();
            var notifications = 0;
            field.RegisterCallback<ChangeEvent<double>>(_ => notifications++);

            field.value = 1d;

            Assert.AreEqual(0, notifications, "Re-assigning the current value should not raise a ChangeEvent.");
        }

        [Test]
        public void DoubleField_SetValue_ShouldClampToLowHighValue()
        {
            var field = MountField();
            field.lowValue = 0d;
            field.highValue = 10d;

            field.value = 100d;

            Assert.AreEqual(10d, field.value, "The value should be clamped to highValue.");
        }
    }
}
#endif
