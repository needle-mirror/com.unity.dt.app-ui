#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ThreadComposer))]
    class ThreadComposerBehaviourTests : UIComponentTests<ThreadComposer>
    {
        static UnityEngine.UIElements.TextField InputFieldOf(ThreadComposer composer) =>
            composer.textArea.Q<UnityEngine.UIElements.TextField>(className: TextArea.inputUssClassName);

        static UnityEngine.UIElements.VisualElement SubmitButtonOf(ThreadComposer composer) =>
            composer.Q(name: ThreadComposer.submitBtnUssClassName);

        ThreadComposer MountComposer() => MountElement(new ThreadComposer { style = { width = 300 } });

        [Test]
        public void ThreadComposer_TypingIntoTextArea_ShouldUpdateValue()
        {
            var composer = MountComposer();

            InputFieldOf(composer).value = "hello team";

            Assert.AreEqual("hello team", composer.value,
                "Typing into the composer's TextArea should update its value.");
        }

        [Test]
        public void ThreadComposer_ClickSubmit_WithValue_ShouldInvokeSubmitClicked()
        {
            var composer = MountComposer();
            InputFieldOf(composer).value = "hello team";
            simulate.FrameUpdate();
            string submitted = null;
            composer.submitClicked += text => submitted = text;

            simulate.Click(SubmitButtonOf(composer));

            Assert.AreEqual("hello team", submitted, "Clicking submit should invoke submitClicked with the composed text.");
        }

        [Test]
        public void ThreadComposer_ClickSubmit_WhenEmpty_ShouldNotInvokeSubmitClicked()
        {
            var composer = MountComposer();
            var notifications = 0;
            composer.submitClicked += _ => notifications++;

            simulate.Click(SubmitButtonOf(composer));

            Assert.AreEqual(0, notifications, "An empty composer should not raise submitClicked.");
        }

        [Test]
        public void ThreadComposer_SubmitOnEnter_ShouldInvokeSubmitClicked()
        {
            var composer = MountComposer();
            composer.textArea.submitOnEnter = true;
            InputFieldOf(composer).value = "hello team";
            simulate.FrameUpdate();
            string submitted = null;
            composer.submitClicked += text => submitted = text;

            InputFieldOf(composer).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual("hello team", submitted, "Submitting the TextArea should submit the composed message.");
        }

        [Test]
        public void ThreadComposer_WhenSending_ShouldDisableTheComposer()
        {
            var composer = MountComposer();

            composer.sending = true;
            simulate.FrameUpdate();

            Assert.IsFalse(composer.enabledSelf, "A sending composer should disable itself to block further input.");
        }
    }
}
#endif
