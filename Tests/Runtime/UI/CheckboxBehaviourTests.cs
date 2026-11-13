#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Checkbox))]
    class CheckboxBehaviourTests : UIComponentTests<Checkbox>
    {
        [Test]
        public void Checkbox_Click_ShouldToggleValue()
        {
            var checkbox = MountElement(new Checkbox { label = "Accept" });
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value, "The checkbox should start unchecked.");

            simulate.Click(checkbox);
            Assert.AreEqual(CheckboxState.Checked, checkbox.value, "Clicking should check the checkbox.");

            simulate.Click(checkbox);
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value, "Clicking again should uncheck it.");
        }

        [Test]
        public void Checkbox_Click_ShouldSendValueChangedEvent()
        {
            var checkbox = MountElement(new Checkbox { label = "Accept" });
            var notifications = 0;
            checkbox.RegisterValueChangedCallback(_ => notifications++);

            simulate.Click(checkbox);

            Assert.AreEqual(1, notifications, "Clicking the checkbox should send exactly one ChangeEvent.");
        }

        [Test]
        public void Checkbox_ClickFromIntermediate_ShouldGoToChecked()
        {
            var checkbox = MountElement(new Checkbox { label = "Accept" });
            checkbox.value = CheckboxState.Intermediate;

            simulate.Click(checkbox);

            Assert.AreEqual(CheckboxState.Checked, checkbox.value,
                "A user click can never land on Intermediate, only toggle between Checked and Unchecked.");
        }

        [Test]
        public void Checkbox_SubmitKey_ShouldToggleValue()
        {
            var checkbox = MountElement(new Checkbox { label = "Accept" });

            checkbox.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(CheckboxState.Checked, checkbox.value, "Submitting on a focused checkbox should check it.");
        }

        [Test]
        public void Checkbox_WhenDisabled_Click_ShouldNotChangeValue()
        {
            var checkbox = MountElement(new Checkbox { label = "Accept" });
            checkbox.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(checkbox);

            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value, "A disabled checkbox should not respond to a click.");
        }
    }
}
#endif
