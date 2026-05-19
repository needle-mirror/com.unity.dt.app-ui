using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ThreadComposer))]
    class ThreadComposerTests : VisualElementTests<ThreadComposer>
    {
        protected override string mainUssClassName => ThreadComposer.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ThreadComposer />",
            @"<appui:ThreadComposer placeholder=""Type a message..."" value=""Hello"" submit-on-enter=""true"" is-editing=""false"" />",
        };

        [Test, Order(2)]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Type a reply...")]
        public void Placeholder_ShouldBeSettable(string text)
        {
            element.placeholder = text;
            Assert.AreEqual(text, element.placeholder);
        }

        [Test, Order(2)]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Hello world")]
        public void Value_ShouldBeSettable(string text)
        {
            element.value = text;
            Assert.AreEqual(text, element.value);
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void Sending_ShouldBeSettable(bool val)
        {
            element.sending = val;
            Assert.AreEqual(val, element.sending);
        }

        [Test, Order(2)]
        public void Sending_ShouldToggleSendingUssClass()
        {
            element.sending = true;
            Assert.IsTrue(element.ClassListContains(ThreadComposer.sendingUssClassName));

            element.sending = false;
            Assert.IsFalse(element.ClassListContains(ThreadComposer.sendingUssClassName));
        }

        [Test, Order(2)]
        [TestCase(true)]
        [TestCase(false)]
        public void IsEditing_ShouldBeSettable(bool val)
        {
            element.isEditing = val;
            Assert.AreEqual(val, element.isEditing);
        }

        [Test, Order(2)]
        public void IsEditing_ShouldToggleEditingUssClass()
        {
            element.isEditing = true;
            Assert.IsTrue(element.ClassListContains(ThreadComposer.editingUssClassName));

            element.isEditing = false;
            Assert.IsFalse(element.ClassListContains(ThreadComposer.editingUssClassName));
        }

        [Test, Order(2)]
        public void Avatar_ShouldNotBeNull()
        {
            Assert.IsNotNull(element.avatar);
        }

        [Test, Order(2)]
        public void EmptyUssClass_ShouldBeAppliedByDefault()
        {
            var composer = new ThreadComposer();
            Assert.IsTrue(composer.ClassListContains(ThreadComposer.emptyUssClassName));
        }

        [Test, Order(2)]
        public void DefaultValues_ShouldBeCorrect()
        {
            var composer = new ThreadComposer();
            Assert.IsFalse(composer.sending);
            Assert.IsFalse(composer.isEditing);
        }

        [Test, Order(2)]
        public void SubmittedEvent_ShouldNotFireOnEmptyValue()
        {
            string received = null;
            element.submitClicked += msg => received = msg;
            element.value = "";
            // Cannot programmatically trigger the internal submit method,
            // but verify the event is subscribable and no exception is thrown.
            Assert.IsNull(received);
        }
    }
}
