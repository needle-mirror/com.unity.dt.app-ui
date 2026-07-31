using System;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Semantic values used for <see cref="AlertDialog"/> variants.
    /// </summary>
    /// <seealso cref="AlertDialog.variant"/>
    public enum AlertSemantic
    {
        /// <summary>
        /// Default color scheme.
        /// </summary>
        Default,

        /// <summary>
        /// Color scheme with positive tone.
        /// </summary>
        Confirmation,

        /// <summary>
        /// Color scheme with neutral tone.
        /// </summary>
        Information,

        /// <summary>
        /// Color scheme with negative tone.
        /// </summary>
        Destructive,

        /// <summary>
        /// Color scheme with negative tone.
        /// </summary>
        Error,

        /// <summary>
        /// Color scheme with caution tone.
        /// </summary>
        Warning,
    }

    /// <summary>
    /// A dialog that interrupts the user's workflow to communicate an important message and acquire a response.
    /// </summary>
    /// <remarks>
    /// Alert dialogs interrupt users with urgent information, details, or actions. They are displayed on top of the
    /// current window using a Modal component and require user interaction to be dismissed.
    ///
    /// Alert dialogs are used for:
    /// - Interrupting workflows with critical information or requiring decisions
    /// - Getting user confirmation before destructive actions
    /// - Displaying important error, warning or success messages
    ///
    /// The AlertDialog component supports different semantic variants to convey the nature of the dialog: Default,
    /// Confirmation, Information, Destructive, Error, and Warning.
    ///
    /// AlertDialogs can have up to three actions:
    /// - Primary action (Required): The main action that confirms or accepts
    /// - Secondary action (Optional): An alternative action
    /// - Cancel action (Optional): Allows users to dismiss the dialog
    ///
    /// Use a <see cref="Modal"/> to display an <see cref="AlertDialog"/> object.
    /// </remarks>
    /// <example>
    /// <para>Creating a warning dialog for unsaved changes with multiple actions</para>
    /// <code lang="csharp"><![CDATA[
    /// var alertDialog = new AlertDialog();
    /// alertDialog.variant = AlertSemantic.Warning;
    /// alertDialog.title = "Unsaved Changes";
    /// alertDialog.description = "You have unsaved changes. Do you want to save them before leaving?";
    ///
    /// alertDialog.SetPrimaryAction(1, "Save", () => SaveChanges());
    /// alertDialog.SetSecondaryAction(2, "Don't Save", () => DiscardChanges());
    /// alertDialog.SetCancelAction(3, "Cancel");
    ///
    /// // Show the alert dialog using a modal
    /// Modal.Build(parentElement, alertDialog)
    ///     .SetKeyboardDismiss(true)
    ///     .SetOutsideClickDismiss(false)
    ///     .Show();
    /// ]]></code>
    /// <para>Defining an alert dialog in UXML</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ui:AlertDialog
    ///     variant="Information"
    ///     title="Update Available"
    ///     description="A new version is available. Would you like to update now?"
    ///     is-primary-action-disabled="false"
    ///     is-secondary-action-disabled="false" />
    /// </UXML>
    /// ]]></code>
    /// <para>Showing a simple success confirmation dialog</para>
    /// <code lang="csharp"><![CDATA[
    /// var confirmationDialog = new AlertDialog();
    /// confirmationDialog.variant = AlertSemantic.Confirmation;
    /// confirmationDialog.title = "Success";
    /// confirmationDialog.description = "Your changes have been saved successfully.";
    /// confirmationDialog.SetPrimaryAction(1, "OK", null);
    ///
    /// Modal.Build(parentElement, confirmationDialog)
    ///     .SetKeyboardDismiss(true)
    ///     .Show();
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class AlertDialog : BaseDialog, IDismissInvocator
    {
        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId isPrimaryActionDisabledProperty = nameof(isPrimaryActionDisabled);

        internal static readonly BindingId isSecondaryActionDisabledProperty = nameof(isSecondaryActionDisabled);
        /// <summary>
        /// The AlertDialog primary action styling class.
        /// </summary>
        public const string primaryActionUssClassName = ussClassName + "__primary-action";

        /// <summary>
        /// The AlertDialog secondary action styling class.
        /// </summary>
        public const string secondaryActionUssClassName = ussClassName + "__secondary-action";

        /// <summary>
        /// The AlertDialog cancel action styling class.
        /// </summary>
        public const string cancelActionUssClassName = ussClassName + "__cancel-action";

        /// <summary>
        /// The AlertDialog icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        readonly Button m_CancelButton;

        ActionItem m_PrimaryAction;

        readonly Button m_PrimaryButton;

        ActionItem m_SecondaryAction;

        readonly Button m_SecondaryButton;

        AlertSemantic m_Variant = AlertSemantic.Default;

        readonly Icon m_IconElement;

        /// <summary>
        /// The AlertDialog primary action button.
        /// </summary>
        public Button primaryButton => m_PrimaryButton;

        /// <summary>
        /// The AlertDialog secondary action button.
        /// </summary>
        public Button secondaryButton => m_SecondaryButton;

        /// <summary>
        /// The AlertDialog cancel action button.
        /// </summary>
        public Button cancelButton => m_CancelButton;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AlertDialog()
        {
            focusable = true;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            delegatesFocus = true;

            RegisterCallback<FocusInEvent>(OnFocusIn);

            m_PrimaryButton = new Button { name = primaryActionUssClassName, variant = ButtonVariant.Accent };
            m_PrimaryButton.AddToClassList(primaryActionUssClassName);

            m_SecondaryButton = new Button { name = secondaryActionUssClassName };
            m_SecondaryButton.AddToClassList(secondaryActionUssClassName);

            m_CancelButton = new Button { name = cancelActionUssClassName };
            m_CancelButton.AddToClassList(cancelActionUssClassName);

            actionContainer.Add(m_CancelButton);
            actionContainer.Add(m_SecondaryButton);
            actionContainer.Add(m_PrimaryButton);

            m_PrimaryButton.AddToClassList(Styles.hiddenUssClassName);
            m_SecondaryButton.AddToClassList(Styles.hiddenUssClassName);
            m_CancelButton.AddToClassList(Styles.hiddenUssClassName);

            m_PrimaryButton.clicked += OnPrimaryActionClicked;
            m_SecondaryButton.clicked += OnSecondaryActionClicked;
            m_CancelButton.clicked += OnCancelActionClicked;

            m_IconElement = new Icon
            {
                name = iconUssClassName,
                iconName = "",
                variant = IconVariant.Regular,
                pickingMode = PickingMode.Ignore
            };
            m_IconElement.AddToClassList(iconUssClassName);
            m_Heading.hierarchy.Add(m_IconElement);

            variant = AlertSemantic.Default;
        }

        void OnFocusIn(FocusInEvent evt)
        {
            schedule.Execute(DeferFocusFirstAction);
            UnregisterCallback<FocusInEvent>(OnFocusIn);
        }

        void DeferFocusFirstAction()
        {
            if (m_PrimaryButton.userData != null)
                m_PrimaryButton.Focus();
            else if (m_SecondaryButton.userData != null)
                m_SecondaryButton.Focus();
            else if (m_CancelButton.userData != null)
                m_CancelButton.Focus();
        }

        /// <summary>
        /// The current variant used by the AlertDialog.
        /// </summary>
        [Tooltip("The current semantic variant used by the AlertDialog.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Alert Dialog")]
        public AlertSemantic variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));
                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// Is the primary action button disabled.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isPrimaryActionDisabled
        {
            get => !m_PrimaryButton.enabledSelf;
            set
            {
                var changed = !m_PrimaryButton.enabledSelf != value;
                m_PrimaryButton.SetEnabled(!value);
                if (changed)
                    NotifyPropertyChanged(in isPrimaryActionDisabledProperty);
            }
        }

        /// <summary>
        /// Is the secondary action button disabled.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isSecondaryActionDisabled
        {
            get => !m_SecondaryButton.enabledSelf;
            set
            {
                var changed = !m_SecondaryButton.enabledSelf != value;
                m_SecondaryButton.SetEnabled(!value);
                if (changed)
                    NotifyPropertyChanged(in isSecondaryActionDisabledProperty);
            }
        }

        /// <summary>
        /// An event invoked when the user requests to dismiss the AlertDialog.
        /// </summary>
        public event Action<DismissType> dismissRequested;

        void OnCancelActionClicked()
        {
            dismissRequested?.Invoke(DismissType.Manual);
        }

        void OnSecondaryActionClicked()
        {
            m_SecondaryAction.callback?.Invoke();
            dismissRequested?.Invoke(DismissType.Action);
        }

        void OnPrimaryActionClicked()
        {
            m_PrimaryAction.callback?.Invoke();
            dismissRequested?.Invoke(DismissType.Action);
        }

        /// <summary>
        /// Bind an Action as the primary action of the Alert.
        /// </summary>
        /// <param name="actionId">The Action Identifier.</param>
        /// <param name="displayText">The text to display inside the action's button.</param>
        /// <param name="callback">The callback invoked if the action is triggered.</param>
        public void SetPrimaryAction(int actionId, string displayText, Action callback)
        {
            m_PrimaryAction = new ActionItem { key = actionId, text = displayText, callback = callback };
            m_PrimaryButton.title = displayText;
            m_PrimaryButton.userData = m_PrimaryAction;
            m_PrimaryButton.RemoveFromClassList(Styles.hiddenUssClassName);
        }

        /// <summary>
        /// Bind an Action as the secondary action of the Alert.
        /// </summary>
        /// <param name="actionId">The Action Identifier.</param>
        /// <param name="displayText">The text to display inside the action's button.</param>
        /// <param name="callback">The callback invoked if the action is triggered.</param>
        public void SetSecondaryAction(int actionId, string displayText, Action callback)
        {
            m_SecondaryAction = new ActionItem { key = actionId, text = displayText, callback = callback };
            m_SecondaryButton.title = displayText;
            m_SecondaryButton.userData = m_SecondaryAction;
            m_SecondaryButton.RemoveFromClassList(Styles.hiddenUssClassName);
        }

        /// <summary>
        /// Bind an Action as the cancel action of the Alert.
        /// </summary>
        /// <param name="actionId">The Action Identifier.</param>
        /// <param name="displayText">The text to display inside the action's button.</param>
        public void SetCancelAction(int actionId, string displayText)
        {
            var cancelAction = new ActionItem { key = actionId, text = displayText };
            m_CancelButton.title = displayText;
            m_CancelButton.userData = cancelAction;
            m_CancelButton.RemoveFromClassList(Styles.hiddenUssClassName);
        }
    }
}
