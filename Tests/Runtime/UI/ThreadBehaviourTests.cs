#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Thread))]
    class ThreadBehaviourTests : UIComponentTests<Thread>
    {
        static VisualElement ResolveButtonOf(Thread thread) => thread.Q(name: Thread.resolveBtnUssClassName);

        static VisualElement ActionsButtonOf(Thread thread) => thread.Q(name: Thread.actionsUssClassName);

        Thread MountThread() => MountElement(new Thread
        {
            enableResolution = true,
            style = { width = 300 },
        });

        [Test]
        public void Thread_ClickResolveButton_ShouldToggleIsResolvedAndNotify()
        {
            var thread = MountThread();
            var notified = new System.Collections.Generic.List<bool>();
            thread.resolveToggled += notified.Add;

            simulate.Click(ResolveButtonOf(thread));

            Assert.IsTrue(thread.isResolved, "Clicking the resolve button should resolve the thread.");
            CollectionAssert.AreEqual(new[] { true }, notified, "resolveToggled should carry the new state.");

            simulate.Click(ResolveButtonOf(thread));

            Assert.IsFalse(thread.isResolved, "Clicking the resolve button again should un-resolve the thread.");
        }

        [Test]
        public void Thread_WhenResolutionDisabled_ResolveButton_ShouldBeHidden()
        {
            var thread = MountElement(new Thread { enableResolution = false, style = { width = 300 } });

            Assert.IsTrue(ResolveButtonOf(thread).ClassListContains(Styles.hiddenUssClassName),
                "The resolve button should be hidden when enableResolution is false.");
        }

        [Test]
        public void Thread_ClickActionsButton_WithMenuItems_ShouldOpenActionsMenu()
        {
            var thread = MountThread();
            thread.makeActionMenuItems = (_, menu) => menu.AddAction(0, item => item.label = "Delete");

            // Two things gate the button. It carries the hidden class until a makeActionMenuItems
            // callback exists, and the actions row is `display: none` until the thread is hovered
            // -- see the `.appui-thread:hover > ... > .appui-thread__actions` rule in Thread.uss.
            // Hover first, then let layout catch up, so the button has a real worldBound when the
            // position-based click is sent. Vector2 overload because MouseMove(VisualElement) only
            // exists in com.unity.ui.test-framework 6.7+, and this fixture also compiles on 6000.3.
            simulate.MouseMove(thread.worldBound.center);
            simulate.FrameUpdate();

            simulate.Click(ActionsButtonOf(thread));

            Assert.IsTrue(thread.ClassListContains(Thread.actionsOpenUssClassName),
                "Clicking the actions button should open the actions menu.");
        }

        [Test]
        public void Thread_ActionsButton_WithNoMenuItemsCallback_ShouldBeHidden()
        {
            var thread = MountThread();

            Assert.IsTrue(ActionsButtonOf(thread).ClassListContains(Styles.hiddenUssClassName),
                "The actions button should be hidden when makeActionMenuItems is null.");
        }
    }
}
#endif
