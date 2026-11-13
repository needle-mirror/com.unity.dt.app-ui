#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toggle))]
    class ToggleBehaviourTests : UIComponentTests<Toggle>
    {
        [Test]
        public void Toggle_Click_ShouldFlipValue()
        {
            var toggle = MountElement(new Toggle { label = "Test" });
            Assert.IsFalse(toggle.value, "The Toggle should start unchecked.");

            simulate.Click(toggle);

            Assert.IsTrue(toggle.value, "Clicking the Toggle should flip its value on.");

            simulate.Click(toggle);

            Assert.IsFalse(toggle.value, "Clicking again should flip it back off.");
        }

        [Test]
        public void Toggle_Click_ShouldNotifyValueChanged()
        {
            var toggle = MountElement(new Toggle { label = "Test" });
            var notified = 0;
            toggle.RegisterValueChangedCallback(_ => notified++);

            simulate.Click(toggle);

            Assert.AreEqual(1, notified, "A click should raise exactly one value-changed event.");
        }

        [Test]
        public void Toggle_SubmitKey_ShouldFlipValue()
        {
            var toggle = MountElement(new Toggle { label = "Test" });

            toggle.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsTrue(toggle.value, "Submitting while focused should flip the Toggle on.");
        }

        [Test]
        public void Toggle_WhenDisabled_Click_ShouldNotChangeValue()
        {
            var toggle = MountElement(new Toggle { label = "Test" });
            toggle.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(toggle);

            Assert.IsFalse(toggle.value, "A disabled Toggle should not respond to a click.");
        }
    }
}
#endif
