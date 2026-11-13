#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Link))]
    class LinkBehaviourTests : UIComponentTests<Link>
    {
        [Test]
        public void Link_Click_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var link = MountElement(new Link("Click me", "https://example.com"));
            link.clickable.clicked += () => clickCount++;

            simulate.Click(link);

            Assert.AreEqual(1, clickCount, "Clicking the Link should invoke its clickable.clicked event.");
        }

        [Test]
        public void Link_SubmitKey_ShouldInvokeClicked()
        {
            var clickCount = 0;
            var link = MountElement(new Link("Click me"));
            link.clickable.clicked += () => clickCount++;

            link.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, clickCount, "Submitting on a focused Link should invoke clickable.clicked.");
        }

        [Test]
        public void Link_WhenDisabled_Click_ShouldNotInvokeClicked()
        {
            var clickCount = 0;
            var link = MountElement(new Link("Click me"));
            link.clickable.clicked += () => clickCount++;
            link.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(link);

            Assert.AreEqual(0, clickCount, "A disabled Link should not invoke clickable.clicked.");
        }
    }
}
#endif
