#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AlertDialog))]
    class AlertDialogBehaviourTests : UIComponentTests<AlertDialog>
    {
        AlertDialog MountDialog()
        {
            var dialog = new AlertDialog { style = { width = 300, height = 150 } };
            dialog.SetPrimaryAction(1, "OK", null);
            dialog.SetSecondaryAction(2, "Maybe", null);
            dialog.SetCancelAction(3, "Cancel");
            return MountElement(dialog);
        }

        [Test]
        public void AlertDialog_OnlyPrimaryAction_ShouldBeAccented()
        {
            var dialog = new AlertDialog();
            Assert.AreEqual(ButtonVariant.Default, dialog.primaryButton.variant);
            Assert.AreEqual(ButtonVariant.Secondary, dialog.secondaryButton.variant);
            Assert.AreEqual(ButtonVariant.Secondary, dialog.cancelButton.variant);
        }

        [Test]
        public void Dialog_CloseButton_ShouldBeGhost()
        {
            Assert.AreEqual(ButtonVariant.Ghost, new Dialog().closeButton.variant);
        }

        [Test]
        public void AlertDialog_ClickPrimaryButton_ShouldSendDismissRequestedWithActionType()
        {
            var dialog = MountDialog();
            var invoked = DismissType.Manual;
            var notified = 0;
            dialog.dismissRequested += type =>
            {
                invoked = type;
                notified++;
            };

            simulate.Click(dialog.primaryButton);

            Assert.AreEqual(1, notified, "Clicking the primary button should raise dismissRequested once.");
            Assert.AreEqual(DismissType.Action, invoked,
                "A button click should report DismissType.Action, not a manual dismissal.");
        }

        [Test]
        public void AlertDialog_ClickSecondaryButton_ShouldSendDismissRequested()
        {
            var dialog = MountDialog();
            var notified = 0;
            dialog.dismissRequested += _ => notified++;

            simulate.Click(dialog.secondaryButton);

            Assert.AreEqual(1, notified, "Clicking the secondary button should raise dismissRequested.");
        }

        [Test]
        public void AlertDialog_WhenPrimaryActionDisabled_Click_ShouldNotSendDismissRequested()
        {
            var dialog = MountDialog();
            dialog.isPrimaryActionDisabled = true;
            simulate.FrameUpdate();
            var notified = 0;
            dialog.dismissRequested += _ => notified++;

            simulate.Click(dialog.primaryButton);

            Assert.AreEqual(0, notified,
                "A disabled primary action should swallow the click and never dismiss the dialog.");
        }

        // #1517: the footer wraps, so actions too wide for one row stack inside the dialog instead of
        // pushing past its edge.
        [Test]
        public void AlertDialog_FooterTooWideForOneRow_WrapsInsideTheDialog()
        {
            var dialog = new AlertDialog { title = "Title", description = "Description", size = DialogSize.Sm };
            dialog.SetPrimaryAction(1, "Overwrite every selected asset now", null);
            dialog.SetSecondaryAction(2, "Keep both copies side by side", null);
            dialog.SetCancelAction(3, "Cancel this operation");
            MountElement(dialog);
            simulate.FrameUpdate();

            var bounds = dialog.contentRect;
            var buttons = new[] { dialog.cancelButton, dialog.secondaryButton, dialog.primaryButton };
            foreach (var button in buttons)
            {
                var local = dialog.WorldToLocal(button.worldBound);
                Assert.GreaterOrEqual(local.xMin, bounds.xMin - 0.5f, $"{button.title} starts outside the dialog.");
                Assert.LessOrEqual(local.xMax, bounds.xMax + 0.5f, $"{button.title} runs past the dialog's edge.");
            }

            Assert.Greater(dialog.primaryButton.worldBound.yMin, dialog.cancelButton.worldBound.yMin + 0.5f,
                "Actions that cannot share one row should wrap onto another.");
        }

        // #1517: long content scrolls inside the dialog's room, between a pinned heading and footer.
        [Test]
        public void AlertDialog_ContentTallerThanTheModal_ScrollsAndKeepsTheActionsInside()
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < 120; i++)
                lines.AppendLine($"Line {i} of a description far taller than the screen.");

            var dialog = new AlertDialog { title = "Title", description = lines.ToString() };
            dialog.SetPrimaryAction(1, "OK", null);
            dialog.SetCancelAction(3, "Cancel");

            var reference = new VisualElement();
            appUiPanel.Add(reference);
            simulate.FrameUpdate();
            var modal = Modal.Build(reference, dialog);
            modal.Show();
            for (var i = 0; i < 10; i++)
                simulate.FrameUpdate();

            var panelBounds = appUiPanel.worldBound;
            var dialogBounds = dialog.worldBound;
            Assert.Greater(dialogBounds.height, 0f, "The dialog needs a layout.");
            Assert.LessOrEqual(dialogBounds.yMax, panelBounds.yMax + 0.5f, "The dialog should stay inside the panel.");
            Assert.GreaterOrEqual(dialogBounds.yMin, panelBounds.yMin - 0.5f, "The dialog should stay inside the panel.");

            var actions = dialog.actionContainer.worldBound;
            Assert.LessOrEqual(actions.yMax, dialogBounds.yMax + 0.5f, "The actions should stay inside the dialog.");
            var button = dialog.primaryButton.worldBound;
            Assert.LessOrEqual(button.yMax, dialogBounds.yMax + 0.5f, "The primary action should stay inside the dialog.");
            Assert.GreaterOrEqual(actions.height + 0.5f, button.height,
                "The footer should keep its height; only the content gives way.");
            var heading = dialog.Q(BaseDialog.headingUssClassName).layout;
            var header = dialog.Q(BaseDialog.headerUssClassName).layout;
            Assert.GreaterOrEqual(heading.height + 0.5f, header.height,
                "The heading should keep its height; only the content gives way.");

            var scroll = dialog.Q<ScrollView>(BaseDialog.contentScrollViewUssClassName);
            Assert.IsNotNull(scroll, "The content should sit in a scroll view.");
            Assert.Greater(scroll.contentContainer.layout.height, scroll.contentViewport.layout.height + 1f,
                "The content should be taller than its viewport, so it scrolls.");

            modal.Dismiss();
            simulate.FrameUpdate();
        }

        // The heading does not shrink and the title now wraps, so an unbounded title could push the
        // actions out of the modal, where scrolling the content cannot bring them back. It is capped.
        [Test]
        public void AlertDialog_TitleTallerThanTheModal_IsCappedAndKeepsTheActionsInside()
        {
            var title = new System.Text.StringBuilder();
            for (var i = 0; i < 300; i++)
                title.Append("word ");

            var dialog = new AlertDialog { title = title.ToString(), description = "Description" };
            dialog.SetPrimaryAction(1, "OK", null);
            dialog.SetCancelAction(3, "Cancel");

            var reference = new VisualElement();
            appUiPanel.Add(reference);
            simulate.FrameUpdate();
            var modal = Modal.Build(reference, dialog);
            modal.Show();
            for (var i = 0; i < 10; i++)
                simulate.FrameUpdate();

            var panelBounds = appUiPanel.worldBound;
            Assert.LessOrEqual(dialog.actionContainer.worldBound.yMax, panelBounds.yMax + 0.5f,
                "A very long title should not push the actions out of the panel.");
            Assert.LessOrEqual(dialog.primaryButton.worldBound.yMax, dialog.worldBound.yMax + 0.5f,
                "The primary action should stay inside the dialog.");

            modal.Dismiss();
            simulate.FrameUpdate();
        }

        // #1468: a long unbroken token (a file name, a URL) breaks inside the dialog, in the title as
        // well as in the description, rather than running out of it.
        [Test]
        public void AlertDialog_LongUnbrokenToken_WrapsInTheTitleAndTheDescription()
        {
            const string token = "https://example.com/a/really/long/path/without/any/space/to/break/on/" +
                "assets/textures/environment/terrain/ground_albedo_with_a_very_long_file_name_v0042.png";
            var dialog = new AlertDialog { title = "Short", description = "Short", size = DialogSize.Sm };
            dialog.SetPrimaryAction(1, "OK", null);
            MountElement(dialog);
            simulate.FrameUpdate();

            var header = dialog.Q(BaseDialog.headerUssClassName);
            var widthBefore = dialog.worldBound.width;
            var headerLine = header.layout.height;
            var contentLine = dialog.contentContainer.layout.height;

            dialog.title = token;
            dialog.description = token;
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.AreEqual(widthBefore, dialog.worldBound.width, 0.5f, "A long token should not widen the dialog.");
            var inner = dialog.worldBound;
            Assert.LessOrEqual(header.worldBound.xMax, inner.xMax + 0.5f, "The title should stay inside the dialog.");
            Assert.Greater(header.layout.height, headerLine * 1.5f, "The title should wrap the token onto more lines.");
            Assert.LessOrEqual(dialog.contentContainer.worldBound.xMax, inner.xMax + 0.5f,
                "The description should stay inside the dialog.");
            Assert.Greater(dialog.contentContainer.layout.height, contentLine * 1.5f,
                "The description should wrap the token onto more lines.");
        }
    }
}
#endif
