using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ConfirmDialog))]
    class ConfirmDialogTests : VisualElementTests<ConfirmDialog>
    {
        protected override string mainUssClassName => ConfirmDialog.confirmUssClassName;

        // Handed over to ConfirmDialogSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new ConfirmDialog
                {
                    title = "Discard changes?",
                    description = "Your edits will be lost.",
                });
                yield return new Story("Destructive", ctx => new ConfirmDialog
                {
                    title = "Delete asset?",
                    description = "This action cannot be undone.",
                    confirmLabel = "Delete",
                    destructive = true,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ConfirmDialog />",
            @"<appui:ConfirmDialog title=""Delete?"" confirm-label=""Delete"" cancel-label=""Keep"" destructive=""true"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var dialog = new ConfirmDialog();
            Assert.AreEqual("Confirm", dialog.confirmLabel);
            Assert.AreEqual("Cancel", dialog.cancelLabel);
            Assert.IsFalse(dialog.destructive);
            Assert.AreEqual(AlertSemantic.Default, dialog.variant);
        }

        [Test]
        public void Labels_ReachTheActionButtons()
        {
            var dialog = new ConfirmDialog { confirmLabel = "Ship it", cancelLabel = "Nope" };
            Assert.AreEqual("Ship it", dialog.primaryButton.title);
            Assert.AreEqual("Nope", dialog.cancelButton.title);
        }

        [Test]
        public void Destructive_SwitchesSemanticVariantAndButtonVariant()
        {
            var dialog = new ConfirmDialog();
            Assert.AreEqual(ButtonVariant.Default, dialog.primaryButton.variant);
            Assert.IsFalse(dialog.ClassListContains(ConfirmDialog.destructiveUssClassName));

            dialog.destructive = true;
            Assert.AreEqual(AlertSemantic.Destructive, dialog.variant);
            Assert.AreEqual(ButtonVariant.Destructive, dialog.primaryButton.variant);
            Assert.IsTrue(dialog.ClassListContains(ConfirmDialog.destructiveUssClassName));

            dialog.destructive = false;
            Assert.AreEqual(AlertSemantic.Default, dialog.variant);
            Assert.AreEqual(ButtonVariant.Default, dialog.primaryButton.variant);
            Assert.IsFalse(dialog.ClassListContains(ConfirmDialog.destructiveUssClassName),
                "The destructive class must be removed, not accumulated.");
        }

        [Test]
        public void Confirmed_CompletesTrue_WhenThePrimaryActionIsInvoked()
        {
            var dialog = new ConfirmDialog();
            Assert.IsFalse(dialog.confirmed.IsCompleted);
            dialog.Confirm();
            Assert.IsTrue(dialog.confirmed.IsCompleted);
            Assert.IsTrue(dialog.confirmed.Result);
        }

        [Test]
        public void Confirmed_CompletesFalse_WhenCancelled()
        {
            var dialog = new ConfirmDialog();
            dialog.Cancel();
            Assert.IsTrue(dialog.confirmed.IsCompleted);
            Assert.IsFalse(dialog.confirmed.Result);
        }

        [Test]
        public void Confirmed_IsResolvedOnlyOnce()
        {
            var dialog = new ConfirmDialog();
            dialog.Confirm();
            dialog.Cancel();
            Assert.IsTrue(dialog.confirmed.Result,
                "A later cancel must not override an already-resolved confirm.");
        }

        [Test]
        public void ConfirmDelete_UsesTheDestructivePreset()
        {
            var single = ConfirmDialog.BuildDelete("Cube");
            Assert.AreEqual("Delete Cube?", single.title);
            Assert.AreEqual("This action cannot be undone.", single.description);
            Assert.AreEqual("Delete", single.confirmLabel);
            Assert.IsTrue(single.destructive);

            var many = ConfirmDialog.BuildDelete("objects", 3);
            Assert.AreEqual("Delete 3 objects?", many.title);
        }

        [UnityTest]
        public IEnumerator Show_ResolvesFalse_WhenTheModalIsDismissed()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            var task = ConfirmDialog.Show(panel, "Sure?");
            yield return null;
            yield return null;

            Assert.IsFalse(task.IsCompleted);
            var dialog = m_TestUI.rootVisualElement.panel.visualTree.Q<ConfirmDialog>();
            Assert.IsNotNull(dialog, "Show() must mount a ConfirmDialog in a Modal.");

            dialog.Cancel();
            yield return null;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.Result);
        }

        [UnityTest]
        public IEnumerator Show_QueuesConcurrentRequests_OneAtATime()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            yield return null;

            var first = ConfirmDialog.Show(panel, "First");
            var second = ConfirmDialog.Show(panel, "Second");
            yield return null;
            yield return null;

            var dialogs = m_TestUI.rootVisualElement.panel.visualTree.Query<ConfirmDialog>().ToList();
            Assert.AreEqual(1, dialogs.Count, "Only one confirm dialog may be on screen at a time.");
            Assert.AreEqual("First", dialogs[0].title);

            dialogs[0].Confirm();
            yield return null;
            yield return null;
            yield return null;

            Assert.IsTrue(first.IsCompleted);
            Assert.IsTrue(first.Result);

            var next = m_TestUI.rootVisualElement.panel.visualTree.Q<ConfirmDialog>();
            Assert.IsNotNull(next, "The queued request must be shown once the first one closes.");
            Assert.AreEqual("Second", next.title);
            next.Cancel();
            yield return null;
            yield return null;
            Assert.IsTrue(second.IsCompleted);
            Assert.IsFalse(second.Result);
        }

        [UnityTest]
        public IEnumerator Show_SkipsAQueuedRequestWhoseHostLeftThePanel_WithoutStrandingTheQueue()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var host = new VisualElement();
            panel.Add(host);
            yield return null;

            var first = ConfirmDialog.Show(panel, "First");
            // Queued while its host is in the panel, but the host is gone before its turn comes.
            var doomed = ConfirmDialog.Show(host, "Doomed");
            yield return null;
            yield return null;

            host.RemoveFromHierarchy();
            yield return null;

            m_TestUI.rootVisualElement.panel.visualTree.Q<ConfirmDialog>().Confirm();
            for (var i = 0; i < 3; i++)
                yield return null;

            Assert.IsTrue(first.IsCompleted);
            Assert.IsTrue(doomed.IsCompleted,
                "A request whose host is gone can never be shown, so it has to resolve rather than "
                + "sit in the queue forever.");
            Assert.IsFalse(doomed.Result);

            // The real damage was to everything after it: the queue has to keep running.
            var third = ConfirmDialog.Show(panel, "Third");
            yield return null;
            yield return null;

            var dialog = m_TestUI.rootVisualElement.panel.visualTree.Q<ConfirmDialog>();
            Assert.IsNotNull(dialog, "The queue was stranded — no later confirmation can ever show.");
            Assert.AreEqual("Third", dialog.title);

            dialog.Cancel();
            yield return null;
            yield return null;
            Assert.IsTrue(third.IsCompleted);
        }
    }
}
