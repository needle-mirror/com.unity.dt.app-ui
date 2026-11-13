#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AppBar))]
    class AppBarBehaviourTests : UIComponentTests<AppBar>
    {
        AppBar MountAppBar()
        {
            var appBar = new AppBar { title = "Title", showBackButton = true, showDrawerButton = true };
            return MountElement(appBar);
        }

        [Test]
        public void AppBar_ClickBackButton_ShouldInvokeBackButtonTriggered()
        {
            var appBar = MountAppBar();
            var notified = 0;
            appBar.backButtonTriggered += () => notified++;

            simulate.Click(appBar.Q(name: AppBar.backButtonUssClassName));

            Assert.AreEqual(1, notified, "Clicking the back button should invoke backButtonTriggered.");
        }

        [Test]
        public void AppBar_ClickDrawerButton_ShouldInvokeDrawerButtonTriggered()
        {
            var appBar = MountAppBar();
            var notified = 0;
            appBar.drawerButtonTriggered += () => notified++;

            simulate.Click(appBar.Q(name: AppBar.drawerButtonUssClassName));

            Assert.AreEqual(1, notified, "Clicking the drawer button should invoke drawerButtonTriggered.");
        }

        [Test]
        public void AppBar_WhenShowBackButtonIsFalse_ShouldHideBackButton()
        {
            var appBar = MountAppBar();
            appBar.showBackButton = false;
            simulate.FrameUpdate();

            var backButton = appBar.Q(name: AppBar.backButtonUssClassName);

            Assert.IsTrue(backButton.ClassListContains(Styles.hiddenUssClassName),
                "Hiding the back button should apply the hidden USS class so it stops taking input.");
        }
    }
}
#endif
