#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Radio))]
    class RadioBehaviourTests : UIComponentTests<Radio>
    {
        Radio MountRadio() => MountElement(new Radio { label = "Option" });

        [Test]
        public void Radio_Click_ShouldSetValueTrue()
        {
            var radio = MountRadio();
            Assert.IsFalse(radio.value, "A Radio should start unchecked.");

            simulate.Click(radio);

            Assert.IsTrue(radio.value, "Clicking a Radio should check it.");
        }

        [Test]
        public void Radio_Click_ShouldSendChangeEvent()
        {
            var radio = MountRadio();
            var notified = false;
            radio.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                notified = true;
                Assert.IsTrue(evt.newValue, "The change event should carry the new checked value.");
            });

            simulate.Click(radio);

            Assert.IsTrue(notified, "Clicking should send a ChangeEvent<bool>.");
        }

        [Test]
        public void Radio_ClickAgainWhileChecked_ShouldStayChecked()
        {
            // Radio.OnClick always sets value to true; a Radio, unlike a Checkbox, cannot be
            // unchecked by clicking it again.
            var radio = MountRadio();
            simulate.Click(radio);
            Assert.IsTrue(radio.value);

            simulate.Click(radio);

            Assert.IsTrue(radio.value, "Clicking a checked Radio again should leave it checked.");
        }

        [Test]
        public void Radio_SubmitKey_ShouldSetValueTrue()
        {
            var radio = MountRadio();

            radio.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsTrue(radio.value, "Submitting while focused should check the Radio.");
        }

        [Test]
        public void Radio_WhenDisabled_Click_ShouldNotChangeValue()
        {
            var radio = MountRadio();
            radio.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(radio);

            Assert.IsFalse(radio.value, "A disabled Radio should not respond to a click.");
        }
    }
}
#endif
