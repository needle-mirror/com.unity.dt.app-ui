#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextArea))]
    class TextAreaBehaviourTests : UIComponentTests<TextArea>
    {
        static UnityEngine.UIElements.TextField InputFieldOf(TextArea textArea) =>
            textArea.Q<UnityEngine.UIElements.TextField>(className: TextArea.inputUssClassName);

        static UnityEngine.UIElements.VisualElement ResizeHandleOf(TextArea textArea) =>
            textArea.Q(className: TextArea.resizeHandleUssClassName);

        TextArea MountTextArea() => MountElement(new TextArea
        {
            style = { width = 300, height = 100 },
        });

        [Test]
        public void TextArea_TypingIntoInput_ShouldUpdateValueAndNotify()
        {
            var textArea = MountTextArea();
            var notified = string.Empty;
            textArea.RegisterCallback<ChangingEvent<string>>(evt => notified = evt.newValue);

            InputFieldOf(textArea).value = "hello world";

            Assert.AreEqual("hello world", textArea.value, "Typing into the inner field should update the TextArea value.");
            Assert.AreEqual("hello world", notified, "Typing should report the value as changing through ChangingEvent<string>, since TextArea only fires ChangeEvent<string> on commit.");
        }

        [Test]
        public void TextArea_DraggingResizeHandle_ShouldGrowTheArea()
        {
            var textArea = MountTextArea();
            var heightBefore = textArea.resolvedStyle.height;
            var handle = ResizeHandleOf(textArea);
            Assert.IsNotNull(handle, "A TextArea should carry a resize handle.");

            var from = handle.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(0f, 60f));
            simulate.FrameUpdate();

            Assert.Greater(textArea.resolvedStyle.height, heightBefore,
                "Dragging the resize handle down should grow the TextArea's height.");
        }

        [Test]
        public void TextArea_SubmitOnEnter_WhenEnabled_ShouldInvokeSubmitted()
        {
            var textArea = MountTextArea();
            textArea.submitOnEnter = true;
            var submittedCount = 0;
            textArea.submitted += () => submittedCount++;

            InputFieldOf(textArea).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, submittedCount, "Pressing Enter with submitOnEnter should invoke submitted.");
        }

        [Test]
        public void TextArea_SubmitOnEnter_WhenDisabled_ShouldNotInvokeSubmitted()
        {
            var textArea = MountTextArea();
            textArea.submitOnEnter = false;
            var submittedCount = 0;
            textArea.submitted += () => submittedCount++;

            InputFieldOf(textArea).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(0, submittedCount, "Enter should not submit when submitOnEnter is false.");
        }
    }
}
#endif
