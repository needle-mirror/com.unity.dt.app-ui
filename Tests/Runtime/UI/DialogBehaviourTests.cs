#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dialog))]
    class DialogBehaviourTests : UIComponentTests<Dialog>
    {
        Dialog MountDialog() => MountElement(new Dialog
        {
            title = "Title",
            description = "Description",
            dismissable = true,
        });

        [Test]
        public void Dialog_CloseButtonClick_ShouldRaiseDismissRequested()
        {
            var dialog = MountDialog();
            DismissType? received = null;
            dialog.dismissRequested += reason => received = reason;

            simulate.Click(dialog.closeButton);

            Assert.AreEqual(DismissType.Manual, received,
                "Clicking the close button on a dismissable Dialog should raise dismissRequested with Manual.");
        }

        [Test]
        public void Dialog_WhenNotDismissable_CloseButtonClick_ShouldNotRaiseDismissRequested()
        {
            var dialog = MountDialog();
            dialog.dismissable = false;
            simulate.FrameUpdate();
            var raised = false;
            dialog.dismissRequested += _ => raised = true;

            simulate.Click(dialog.closeButton);

            Assert.IsFalse(raised, "OnCloseButtonClicked bails out when the Dialog is not dismissable.");
        }

        // The content scroll view clips, so custom content needs a gutter inside it, or a focused
        // full-width control's outline (drawn outside its bounds) is cut off at the viewport edge.
        [Test]
        public void Dialog_CustomContent_LeavesRoomForAFocusOutlineInsideTheScrollViewport()
        {
            var dialog = MountDialog();
            dialog.description = null;
            var field = new TextField();
            dialog.Add(field);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var scroll = UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.ScrollView>(
                dialog, BaseDialog.contentScrollViewUssClassName);
            var viewport = scroll.contentViewport.worldBound;
            var bounds = field.worldBound;
            Assert.Greater(bounds.width, 0f, "The field needs a layout.");
            // --appui-alias-control-outline-offset (2) plus --appui-alias-control-outline-width (2).
            const float outline = 4f;
            Assert.GreaterOrEqual(bounds.xMin - viewport.xMin, outline - 0.5f, "No room for the outline on the left.");
            Assert.GreaterOrEqual(viewport.xMax - bounds.xMax, outline - 0.5f, "No room for the outline on the right.");
            Assert.GreaterOrEqual(bounds.yMin - viewport.yMin, outline - 0.5f, "No room for the outline above.");
        }

        // A ScrollView does not reveal a focused descendant by itself; the dialog does it, so tabbing
        // through a form taller than the dialog never lands on a field out of sight.
        [Test]
        public void Dialog_FocusingAFieldBelowTheFold_ScrollsItIntoView()
        {
            var dialog = new Dialog { title = "Form" };
            TextField last = null;
            for (var i = 0; i < 40; i++)
                dialog.Add(last = new TextField());

            var reference = new UnityEngine.UIElements.VisualElement();
            appUiPanel.Add(reference);
            simulate.FrameUpdate();
            var modal = Modal.Build(reference, dialog);
            modal.Show();
            for (var i = 0; i < 10; i++)
                simulate.FrameUpdate();

            var scroll = UnityEngine.UIElements.UQueryExtensions.Q<UnityEngine.UIElements.ScrollView>(
                dialog, BaseDialog.contentScrollViewUssClassName);
            Assert.Greater(last.worldBound.yMin, scroll.contentViewport.worldBound.yMax,
                "The last field should start out of sight.");

            last.Focus();
            for (var i = 0; i < 3; i++)
                simulate.FrameUpdate();

            var viewport = scroll.contentViewport.worldBound;
            Assert.LessOrEqual(last.worldBound.yMax, viewport.yMax + 1f, "The focused field should be scrolled into view.");
            Assert.GreaterOrEqual(last.worldBound.yMin, viewport.yMin - 1f, "The focused field should be scrolled into view.");

            // The scroll view's own scrollbar is outside its content; focusing it must not ask the
            // scroll view to reveal it, which throws.
            var slider = scroll.verticalScroller.slider;
            slider.focusable = true;
            Assert.DoesNotThrow(() =>
            {
                slider.Focus();
                simulate.FrameUpdate();
            }, "Focusing the scrollbar should not try to scroll it into view.");

            modal.Dismiss();
            simulate.FrameUpdate();
        }

        [Test]
        public void Dialog_WhenCloseButtonDisabled_Click_ShouldNotRaiseDismissRequested()
        {
            var dialog = MountDialog();
            dialog.closeButton.SetEnabled(false);
            simulate.FrameUpdate();
            var raised = false;
            dialog.dismissRequested += _ => raised = true;

            simulate.Click(dialog.closeButton);

            Assert.IsFalse(raised, "A disabled close button should not raise dismissRequested.");
        }
    }
}
#endif
