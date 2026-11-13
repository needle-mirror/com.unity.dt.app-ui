using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A two-choice confirmation dialog, and the imperative helper that shows one and awaits the answer.
    /// </summary>
    /// <remarks>
    /// <see cref="ConfirmDialog"/> is the ergonomic layer over <see cref="AlertDialog"/> and
    /// <see cref="Modal"/>: instead of wiring a primary action, a cancel action and a modal by hand
    /// every time you need a yes/no answer, you call <see cref="Show(VisualElement,string,string,string,string,bool)"/>
    /// and await a <see cref="Task{TResult}"/> of <see cref="bool"/>.
    ///
    /// The task completes <c>true</c> when the user takes the confirm action and <c>false</c> for
    /// every other outcome: the cancel action, the escape key, or a click outside the modal. It
    /// resolves exactly once, so a dismiss that follows a confirm cannot flip the answer.
    ///
    /// Concurrent calls queue. Only one confirmation is on screen at a time and the next one is
    /// shown as soon as the current one closes, so a loop over several items does not stack a pile
    /// of modals on top of each other.
    ///
    /// <see cref="ConfirmDelete"/> is the preset for the common destructive case. It frames the
    /// dialog with the <see cref="AlertSemantic.Destructive"/> semantic, labels the confirm action
    /// "Delete" and adds the "This action cannot be undone." description.
    ///
    /// The element itself is a normal <see cref="AlertDialog"/> subclass, so it can also be
    /// declared in UXML and driven manually through <see cref="Confirm"/> and <see cref="Cancel"/>.
    /// </remarks>
    /// <example>
    /// <para>Awaiting a confirmation before running a destructive action.</para>
    /// <code lang="csharp"><![CDATA[
    /// async void OnDeleteClicked()
    /// {
    ///     if (await ConfirmDialog.ConfirmDelete(rootElement, selection.name))
    ///         DeleteSelection();
    /// }
    /// ]]></code>
    /// <para>A custom confirmation with its own labels.</para>
    /// <code lang="csharp"><![CDATA[
    /// var confirmed = await ConfirmDialog.Show(
    ///     rootElement,
    ///     title: "Discard changes?",
    ///     description: "Your edits to this scene will be lost.",
    ///     confirmLabel: "Discard",
    ///     cancelLabel: "Keep editing",
    ///     destructive: true);
    /// ]]></code>
    /// <para>Declaring the dialog in UXML and showing it yourself.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <appui:ConfirmDialog
    ///     title="Delete asset?"
    ///     description="This action cannot be undone."
    ///     confirm-label="Delete"
    ///     cancel-label="Cancel"
    ///     destructive="true" />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups")]
    public partial class ConfirmDialog : AlertDialog
    {
        internal static readonly BindingId confirmLabelProperty = nameof(confirmLabel);

        internal static readonly BindingId cancelLabelProperty = nameof(cancelLabel);

        internal static readonly BindingId destructiveProperty = nameof(destructive);

        /// <summary>
        /// The default label of the confirm action.
        /// </summary>
        public const string defaultConfirmLabel = "Confirm";

        /// <summary>
        /// The default label of the cancel action.
        /// </summary>
        public const string defaultCancelLabel = "Cancel";

        /// <summary>
        /// The description used by <see cref="ConfirmDelete"/>.
        /// </summary>
        public const string deleteDescription = "This action cannot be undone.";

        /// <summary>
        /// The main styling class of the ConfirmDialog. This is the class that is used to style the element
        /// in addition to the <see cref="BaseDialog.ussClassName"/> class.
        /// </summary>
        public const string confirmUssClassName = "appui-confirm-dialog";

        /// <summary>
        /// The styling class applied when <see cref="destructive"/> is `true`.
        /// </summary>
        public const string destructiveUssClassName = confirmUssClassName + "--destructive";

        const int k_ConfirmActionId = 1;

        const int k_CancelActionId = 2;

        readonly TaskCompletionSource<bool> m_Completion =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        string m_ConfirmLabel = defaultConfirmLabel;

        string m_CancelLabel = defaultCancelLabel;

        bool m_Destructive;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConfirmDialog()
        {
            AddToClassList(confirmUssClassName);

            SetPrimaryAction(k_ConfirmActionId, m_ConfirmLabel, Confirm);
            SetCancelAction(k_CancelActionId, m_CancelLabel);

            dismissRequested += OnDismissRequested;
        }

        /// <summary>
        /// The task that completes with the user's answer: `true` when the confirm action is taken,
        /// `false` for a cancel, an escape key press or an outside click. It resolves exactly once.
        /// </summary>
        public Task<bool> confirmed => m_Completion.Task;

        /// <summary>
        /// The label of the confirm action. Defaults to <see cref="defaultConfirmLabel"/>.
        /// </summary>
        [Tooltip("The label of the confirm action.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Confirm Dialog")]
        public string confirmLabel
        {
            get => m_ConfirmLabel;
            set
            {
                var newValue = string.IsNullOrEmpty(value) ? defaultConfirmLabel : value;
                var changed = m_ConfirmLabel != newValue;
                m_ConfirmLabel = newValue;
                SetPrimaryAction(k_ConfirmActionId, m_ConfirmLabel, Confirm);
                if (changed)
                    NotifyPropertyChanged(in confirmLabelProperty);
            }
        }

        /// <summary>
        /// The label of the cancel action. Defaults to <see cref="defaultCancelLabel"/>.
        /// </summary>
        [Tooltip("The label of the cancel action.")]
        [CreateProperty]
        [UxmlAttribute]
        public string cancelLabel
        {
            get => m_CancelLabel;
            set
            {
                var newValue = string.IsNullOrEmpty(value) ? defaultCancelLabel : value;
                var changed = m_CancelLabel != newValue;
                m_CancelLabel = newValue;
                SetCancelAction(k_CancelActionId, m_CancelLabel);
                if (changed)
                    NotifyPropertyChanged(in cancelLabelProperty);
            }
        }

        /// <summary>
        /// Frames the confirmation as destructive: the dialog takes the
        /// <see cref="AlertSemantic.Destructive"/> semantic and the confirm action becomes a
        /// <see cref="ButtonVariant.Destructive"/> button.
        /// </summary>
        [Tooltip("Frames the confirmation as destructive.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool destructive
        {
            get => m_Destructive;
            set
            {
                var changed = m_Destructive != value;
                m_Destructive = value;
                EnableInClassList(destructiveUssClassName, m_Destructive);
                variant = m_Destructive ? AlertSemantic.Destructive : AlertSemantic.Default;
                primaryButton.variant = m_Destructive ? ButtonVariant.Destructive : ButtonVariant.Default;
                if (changed)
                    NotifyPropertyChanged(in destructiveProperty);
            }
        }

        /// <summary>
        /// Resolve <see cref="confirmed"/> with `true`. Called by the confirm action; safe to call
        /// more than once, later calls are ignored.
        /// </summary>
        public void Confirm()
        {
            if (m_Completion.TrySetResult(true))
                resolved?.Invoke(true);
        }

        /// <summary>
        /// Resolve <see cref="confirmed"/> with `false`. Called by the cancel action and by any
        /// dismissal; safe to call more than once, later calls are ignored.
        /// </summary>
        public void Cancel()
        {
            if (m_Completion.TrySetResult(false))
                resolved?.Invoke(false);
        }

        /// <summary>
        /// Raised once, the first time <see cref="confirmed"/> resolves. This is what lets the
        /// modal host close as soon as the user answers, without awaiting the task.
        /// </summary>
        internal event Action<bool> resolved;

        void OnDismissRequested(DismissType reason) => Cancel();

        /// <summary>
        /// Build an unshown <see cref="ConfirmDialog"/> configured as a delete confirmation.
        /// </summary>
        /// <param name="what">Names the thing being deleted — an object's name, or a plural noun
        /// such as "objects" when deleting several.</param>
        /// <param name="count">How many things are being deleted. Above one the title reads
        /// "Delete N &lt;what&gt;?".</param>
        /// <returns>The configured dialog.</returns>
        public static ConfirmDialog BuildDelete(string what, int count = 1)
        {
            return new ConfirmDialog
            {
                title = count > 1 ? $"Delete {count} {what}?" : $"Delete {what}?",
                description = deleteDescription,
                confirmLabel = "Delete",
                destructive = true,
            };
        }

        /// <summary>
        /// Show a confirmation dialog and await the user's answer.
        /// </summary>
        /// <param name="referenceView">Any <see cref="VisualElement"/> currently in the UI panel that
        /// should host the modal.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="description">An optional supporting line under the title.</param>
        /// <param name="confirmLabel">The confirm action's label. Defaults to "Confirm".</param>
        /// <param name="cancelLabel">The cancel action's label. Defaults to "Cancel".</param>
        /// <param name="destructive">Frames the confirmation as destructive.</param>
        /// <returns>A task that completes `true` when confirmed, `false` otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="referenceView"/> is null.</exception>
        public static Task<bool> Show(
            VisualElement referenceView,
            string title,
            string description = null,
            string confirmLabel = null,
            string cancelLabel = null,
            bool destructive = false)
        {
            var dialog = new ConfirmDialog
            {
                title = title,
                description = description,
                destructive = destructive,
            };
            if (!string.IsNullOrEmpty(confirmLabel))
                dialog.confirmLabel = confirmLabel;
            if (!string.IsNullOrEmpty(cancelLabel))
                dialog.cancelLabel = cancelLabel;
            return Show(referenceView, dialog);
        }

        /// <summary>
        /// Show an already configured <see cref="ConfirmDialog"/> and await the user's answer.
        /// </summary>
        /// <param name="referenceView">Any <see cref="VisualElement"/> currently in the UI panel that
        /// should host the modal.</param>
        /// <param name="dialog">The dialog to show.</param>
        /// <returns>A task that completes `true` when confirmed, `false` otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="referenceView"/> or
        /// <paramref name="dialog"/> is null.</exception>
        public static Task<bool> Show(VisualElement referenceView, ConfirmDialog dialog)
        {
            if (referenceView == null)
                throw new ArgumentNullException(nameof(referenceView));
            if (dialog == null)
                throw new ArgumentNullException(nameof(dialog));

            s_Queue.Enqueue(new Request(referenceView, dialog));
            PumpQueue();
            return dialog.confirmed;
        }

        /// <summary>
        /// Show a delete confirmation and await the user's answer. The destructive preset of
        /// <see cref="Show(VisualElement,string,string,string,string,bool)"/>.
        /// </summary>
        /// <param name="referenceView">Any <see cref="VisualElement"/> currently in the UI panel that
        /// should host the modal.</param>
        /// <param name="what">Names the thing being deleted.</param>
        /// <param name="count">How many things are being deleted.</param>
        /// <returns>A task that completes `true` when confirmed, `false` otherwise.</returns>
        public static Task<bool> ConfirmDelete(VisualElement referenceView, string what, int count = 1)
        {
            return Show(referenceView, BuildDelete(what, count));
        }

        readonly struct Request
        {
            public Request(VisualElement referenceView, ConfirmDialog dialog)
            {
                this.referenceView = referenceView;
                this.dialog = dialog;
            }

            public VisualElement referenceView { get; }

            public ConfirmDialog dialog { get; }
        }

        static readonly Queue<Request> s_Queue = new Queue<Request>();

        static bool s_Showing;

        static void PumpQueue()
        {
            if (s_Showing)
                return;

            while (s_Queue.Count > 0)
            {
                // A request is queued against a host that is in the panel at the time, but it can
                // reach its turn after that host has been removed. Modal.Build has no popup parent
                // to attach to then and throws — and it throws with s_Showing already set, which
                // would strand the queue and leave every later confirmation unanswered.
                if (s_Queue.Peek().referenceView?.panel == null)
                {
                    s_Queue.Dequeue().dialog.Cancel();
                    continue;
                }

                s_Showing = true;
                var request = s_Queue.Dequeue();
                try
                {
                    ShowRequest(request);
                    return;
                }
                catch (Exception e)
                {
                    // Nothing will dismiss a modal that never showed, so release the queue here and
                    // resolve the request rather than letting one bad host block the rest.
                    s_Showing = false;
                    request.dialog.Cancel();
                    Debug.LogException(e);
                }
            }
        }

        static void ShowRequest(Request request)
        {
            var dialog = request.dialog;

            var modal = Modal.Build(request.referenceView, dialog)
                .SetKeyboardDismiss(true)
                .SetOutsideClickDismiss(true);

            void OnResolved(bool _)
            {
                dialog.resolved -= OnResolved;
                modal.Dismiss(DismissType.Manual);
            }

            dialog.resolved += OnResolved;
            modal.dismissed += (_, __) =>
            {
                dialog.resolved -= OnResolved;
                // Esc, an outside click or a programmatic dismiss resolves to false; TrySetResult
                // keeps an already-resolved answer, so a confirm cannot be flipped here.
                dialog.Cancel();
                s_Showing = false;
                PumpQueue();
            };
            modal.Show();
        }
    }
}
