#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Stepper))]
    class StepperBehaviourTests : UIComponentTests<Stepper>
    {
        static VisualElement IncButtonOf(Stepper stepper) => stepper.Q(name: Stepper.incButtonUssClassName);

        static VisualElement DecButtonOf(Stepper stepper) => stepper.Q(name: Stepper.decButtonUssClassName);

        [Test]
        public void Stepper_ClickIncrement_ShouldSendValueOfOne()
        {
            var stepper = MountElement();
            var notified = 0;
            stepper.RegisterValueChangedCallback(evt => notified = evt.newValue);

            simulate.Click(IncButtonOf(stepper));

            Assert.AreEqual(1, stepper.value, "Clicking the increment button should send a value of 1.");
            Assert.AreEqual(1, notified, "The change should be reported through RegisterValueChangedCallback.");
        }

        [Test]
        public void Stepper_ClickDecrement_ShouldSendValueOfMinusOne()
        {
            var stepper = MountElement();
            var notified = 0;
            stepper.RegisterValueChangedCallback(evt => notified = evt.newValue);

            simulate.Click(DecButtonOf(stepper));

            Assert.AreEqual(-1, stepper.value, "Clicking the decrement button should send a value of -1.");
            Assert.AreEqual(-1, notified, "The change should be reported through RegisterValueChangedCallback.");
        }

        [Test]
        public void Stepper_WhenDisabled_Click_ShouldNotSendValue()
        {
            var stepper = MountElement();
            var notifications = 0;
            stepper.RegisterValueChangedCallback(_ => notifications++);
            stepper.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(IncButtonOf(stepper));

            Assert.AreEqual(0, notifications, "A disabled Stepper should not react to clicks.");
        }
    }
}
#endif
