using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Attachment))]
    class AttachmentTests : VisualElementTests<Attachment>
    {
        protected override string mainUssClassName => Attachment.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Attachment />",
            @"<appui:Attachment title=""Document.pdf"" subtitle=""2.4 MB"" />",
        };

        [Test, Order(2)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("My File.pdf")]
        public void Title_ShouldBeSettable(string title)
        {
            element.title = title;
            Assert.AreEqual(title, element.title);
        }

        [Test, Order(2)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("2.4 MB")]
        public void Subtitle_ShouldBeSettable(string subtitle)
        {
            element.subtitle = subtitle;
            Assert.AreEqual(subtitle, element.subtitle);
        }

        [Test, Order(2)]
        public void ContentContainer_ShouldReturnThumbnailContainer()
        {
            Assert.IsNotNull(element.contentContainer);
            Assert.IsTrue(element.contentContainer.ClassListContains(Attachment.thumbnailUssClassName));
        }

        [Test, Order(2)]
        public void Add_ShouldAddToThumbnailContainer()
        {
            var child = new VisualElement();
            element.Add(child);
            Assert.AreEqual(1, element.contentContainer.childCount);
            Assert.AreEqual(child, element.contentContainer[0]);
        }

        [Test, Order(2)]
        public void DeleteClicked_ShouldBeInvoked()
        {
            var invoked = false;
            element.deleteClicked += () => invoked = true;

            // Find the delete button and invoke its pressable
            var deleteBtn = element.Q<ActionButton>(Attachment.deleteBtnUssClassName);
            Assert.IsNotNull(deleteBtn);

            deleteBtn.clickable.InvokePressed(null);

            Assert.IsTrue(invoked);
        }
    }
}
