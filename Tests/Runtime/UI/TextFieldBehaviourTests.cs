#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextField))]
    class TextFieldBehaviourTests : UIComponentTests<TextField>
    {
        static UnityEngine.UIElements.TextField InputFieldOf(TextField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName);

        TextField MountTextField() => MountElement(new TextField { style = { width = 200 } });

        [Test]
        public void TextField_Click_ShouldFocusTheField()
        {
            var field = MountTextField();

            simulate.Click(field);
            simulate.FrameUpdate();

            Assert.IsTrue(field.ClassListContains(Styles.focusedUssClassName),
                "Clicking a TextField should focus it.");
        }

        [Test]
        public void TextField_TypingIntoInput_ShouldUpdateValueAndNotify()
        {
            var field = MountTextField();
            var notified = string.Empty;
            field.RegisterCallback<ChangingEvent<string>>(evt => notified = evt.newValue);

            InputFieldOf(field).value = "hello";

            Assert.AreEqual("hello", field.value, "Typing into the inner field should update the TextField value.");
            Assert.AreEqual("hello", notified, "Typing should report the value as changing through ChangingEvent<string>, since TextField only fires ChangeEvent<string> on commit.");
        }

        [Test]
        public void TextField_WhenDisabled_Click_ShouldNotFocus()
        {
            var field = MountTextField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field);
            simulate.FrameUpdate();

            Assert.IsFalse(field.ClassListContains(Styles.focusedUssClassName),
                "A disabled TextField should not accept focus from a click.");
        }
    }
}
#endif
