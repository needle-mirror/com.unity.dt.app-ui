#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Attachment))]
    class AttachmentBehaviourTests : UIComponentTests<Attachment>
    {
        Attachment MountAttachment() => MountElement(new Attachment
        {
            title = "file.png",
            subtitle = "120 KB",
        });

        [Test]
        public void Attachment_Click_ShouldInvokeClicked()
        {
            var attachment = MountAttachment();
            var notified = 0;
            attachment.clicked += () => notified++;

            simulate.Click(attachment);

            Assert.AreEqual(1, notified, "Clicking the Attachment should invoke its clicked event.");
        }

        [Test]
        public void Attachment_ClickDeleteButton_ShouldInvokeDeleteClickedWithoutTriggeringClicked()
        {
            var attachment = MountAttachment();
            var deleteNotified = 0;
            var clickNotified = 0;
            attachment.deleteClicked += () => deleteNotified++;
            attachment.clicked += () => clickNotified++;

            simulate.Click(attachment.Q(name: Attachment.deleteBtnUssClassName));

            Assert.AreEqual(1, deleteNotified, "Clicking the delete button should invoke deleteClicked.");
        }

        [Test]
        public void Attachment_WhenReadOnly_ShouldHideDeleteButton()
        {
            var attachment = MountAttachment();

            attachment.readOnly = true;
            simulate.FrameUpdate();

            var deleteButton = attachment.Q(name: Attachment.deleteBtnUssClassName);
            Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.None, deleteButton.resolvedStyle.display,
                "A read-only Attachment should hide its delete button so it can't be triggered.");
        }
    }
}
#endif
