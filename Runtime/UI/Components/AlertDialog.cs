using System;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Semantic values used for <see cref="AlertDialog"/> variants.
    /// <para>
    /// See <see cref="AlertDialog.variant"/>.
    /// </para>
    /// </summary>
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
    /// AlertDialog UI element.
    /// <remarks>
    /// Use a <see cref="Modal"/> to display an <see cref="AlertDialog"/> object.
    /// </remarks>
    /// </summary>
    public class AlertDialog : BaseDialog, IDismissInvocator
    {
        /// <summary>
        /// The AlertDialog primary action styling class.
        /// </summary>
        public static readonly string primaryActionUssClassName = ussClassName + "__primary-action";

        /// <summary>
        /// The AlertDialog secondary action styling class.
        /// </summary>
        public static readonly string secondaryActionUssClassName = ussClassName + "__secondary-action";

        /// <summary>
        /// The AlertDialog cancel action styling class.
        /// </summary>
        public static readonly string cancelActionUssClassName = ussClassName + "__cancel-action";
        
        /// <summary>
        /// The AlertDialog icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";

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
                iconName = "warning",
                variant = IconVariant.Fill,
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
        public AlertSemantic variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(ussClassName + "--" + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(ussClassName + "--" + m_Variant.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// Is the primary action button disabled.
        /// </summary>
        public bool isPrimaryActionDisabled
        {
            get => !m_PrimaryButton.enabledSelf;
            set => m_PrimaryButton.SetEnabled(!value);
        }
        
        /// <summary>
        /// Is the secondary action button disabled.
        /// </summary>
        public bool isSecondaryActionDisabled
        {
            get => !m_SecondaryButton.enabledSelf;
            set => m_SecondaryButton.SetEnabled(!value);
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
        /// Bind an Action as a the primary action of the Alert.
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
        /// Bind an Action as a the secondary action of the Alert.
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
        /// Bind an Action as a the cancel action of the Alert.
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

        /// <summary>
        /// The UXML factory for the AlertDialog.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AlertDialog, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="AlertDialog"/>.
        /// </summary>
        public new class UxmlTraits : BaseDialog.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<AlertSemantic> m_Variant = new UxmlEnumAttributeDescription<AlertSemantic>
            {
                name = "variant",
                defaultValue = AlertSemantic.Default,
            };
            
            readonly UxmlBoolAttributeDescription m_IsPrimaryActionDisabled = new UxmlBoolAttributeDescription
            {
                name = "is-primary-action-disabled",
                defaultValue = false
            };
            
            readonly UxmlBoolAttributeDescription m_IsSecondaryActionDisabled = new UxmlBoolAttributeDescription
            {
                name = "is-secondary-action-disabled",
                defaultValue = false
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (AlertDialog)ve;
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                element.isPrimaryActionDisabled = m_IsPrimaryActionDisabled.GetValueFromBag(bag, cc);
                element.isSecondaryActionDisabled = m_IsSecondaryActionDisabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
