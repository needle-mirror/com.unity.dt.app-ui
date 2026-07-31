using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The FullScreen mode used by a <see cref="Modal"/> component.
    /// </summary>
    public enum ModalFullScreenMode
    {
        /// <summary>
        /// The <see cref="Modal"/> is displayed as a normal size.
        /// </summary>
        None,

        /// <summary>
        /// The <see cref="Modal"/> is displayed in fullscreen but a small margin still present
        /// to display the <see cref="Modal"/> smir.
        /// </summary>
        FullScreen,

        /// <summary>
        /// The <see cref="Modal"/> is displayed in fullscreen without any margin.
        /// The <see cref="Modal"/> smir won't be reachable.
        /// </summary>
        FullScreenTakeOver
    }

    /// <summary>
    /// Interface that must be implemented by any UI component which wants to
    /// request a <see cref="Popup.Dismiss(DismissType)"/> if this component is displayed
    /// inside a <see cref="Popup"/> component.
    /// </summary>
    public interface IDismissInvocator
    {
        /// <summary>
        /// Event triggered when the UI component wants to request a <see cref="Popup.Dismiss(DismissType)"/>
        /// </summary>
        event Action<DismissType> dismissRequested;
    }

    /// <summary>
    /// A modal dialog that displays as an overlay blocking interaction with the rest of the UI.
    /// </summary>
    /// <remarks>
    /// A modal is a dialog window that appears on top of the current interface, blocking interaction with the
    /// parent application until the modal is closed. It typically features a backdrop overlay that dims the
    /// content behind it.
    ///
    /// Modals are ideal for critical information that requires user attention, such as confirmations, alerts, or
    /// forms that must be completed before proceeding. They maintain focus and prevent users from interacting with
    /// other parts of the interface.
    ///
    /// The modal component supports various display modes including normal windowed, fullscreen with margins, and
    /// complete fullscreen takeover. It can be configured to dismiss when clicking outside the modal content area.
    ///
    /// Use modals sparingly as they interrupt the user workflow. Consider alternatives like inline editing,
    /// sidebars, or separate pages for non-critical interactions.
    ///
    /// ## Anatomy
    /// Modal Examples:
    ///
    /// Confirmation modal.
    /// ```xml
    /// &lt;appui:Modal title="Confirmation" size="M" visible="true"&gt;
    ///     &lt;appui:Text text="Are you sure you want to delete this item?" size="M" /&gt;
    ///     &lt;appui:ActionGroup&gt;
    ///         &lt;appui:Button title="Cancel" quiet="true" /&gt;
    ///         &lt;appui:Button title="Delete" variant="Destructive" /&gt;
    ///     &lt;/appui:ActionGroup&gt;
    /// &lt;/appui:Modal&gt;
    /// ```
    ///
    /// Form modal with inputs.
    /// ```xml
    /// &lt;appui:Modal title="Settings" size="L" visible="true"&gt;
    ///     &lt;appui:TextField placeholder-text="Enter name..." size="M" /&gt;
    ///     &lt;appui:TextArea placeholder-text="Description..." size="M" /&gt;
    ///     &lt;appui:ActionGroup&gt;
    ///         &lt;appui:Button title="Cancel" quiet="true" /&gt;
    ///         &lt;appui:Button title="Save" variant="Accent" /&gt;
    ///     &lt;/appui:ActionGroup&gt;
    /// &lt;/appui:Modal&gt;
    /// ```
    ///
    /// Different Sizes.
    /// ```xml
    /// &lt;appui:Modal title="Small Modal" size="S" visible="true"&gt;
    ///     &lt;appui:Text text="Small content" size="S" /&gt;
    /// &lt;/appui:Modal&gt;
    /// &lt;appui:Modal title="Medium Modal" size="M" visible="true"&gt;
    ///     &lt;appui:Text text="Medium content area" size="M" /&gt;
    /// &lt;/appui:Modal&gt;
    /// &lt;appui:Modal title="Large Modal" size="L" visible="true"&gt;
    ///     &lt;appui:Text text="Large content area with more space" size="M" /&gt;
    /// &lt;/appui:Modal&gt;
    /// ```
    /// </remarks>
    /// <example>
    /// <para>Basic modal with content. Creating a simple modal dialog with custom content.</para>
    /// <code lang="csharp"><![CDATA[
    /// var content = new VisualElement();
    /// content.Add(new Text("Are you sure you want to delete this item?"));
    ///
    /// var buttonContainer = new VisualElement();
    /// var cancelButton = new Button { title = "Cancel" };
    /// var confirmButton = new Button { title = "Delete", variant = ButtonVariant.Destructive };
    ///
    /// buttonContainer.Add(cancelButton);
    /// buttonContainer.Add(confirmButton);
    /// content.Add(buttonContainer);
    ///
    /// var modal = Modal.Build(rootElement, content)
    ///     .SetOutsideClickDismiss(true);
    ///
    /// cancelButton.clicked += modal.Dismiss;
    /// confirmButton.clicked += () => {
    ///     DeleteItem();
    ///     modal.Dismiss();
    /// };
    ///
    /// modal.Show();
    /// ]]></code>
    /// <para>Fullscreen modal configuration. Different fullscreen modes for modal presentation.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Normal modal (default)
    /// var normalModal = Modal.Build(rootElement, contentElement)
    ///     .SetFullScreenMode(ModalFullScreenMode.None);
    ///
    /// // Fullscreen with backdrop margin
    /// var fullscreenModal = Modal.Build(rootElement, contentElement)
    ///     .SetFullScreenMode(ModalFullScreenMode.FullScreen);
    ///
    /// // Complete fullscreen takeover
    /// var takeoverModal = Modal.Build(rootElement, contentElement)
    ///     .SetFullScreenMode(ModalFullScreenMode.FullScreenTakeOver);
    ///
    /// // Show any of them
    /// normalModal.Show();
    /// ]]></code>
    /// <para>Modal with outside click handling. Configuring modal dismissal behavior for outside clicks.</para>
    /// <code lang="csharp"><![CDATA[
    /// var modal = Modal.Build(rootElement, contentElement)
    ///     .SetOutsideClickDismiss(true)
    ///     .SetOutsideClickStrategy(OutsideClickStrategy.Bounds);
    ///
    /// // Handle modal events
    /// modal.shown += () => Debug.Log("Modal shown");
    /// modal.dismissed += (reason) => {
    ///     switch (reason)
    ///     {
    ///         case DismissType.OutsideClick:
    ///             Debug.Log("Modal dismissed by outside click");
    ///             break;
    ///         case DismissType.Keyboard:
    ///             Debug.Log("Modal dismissed by ESC key");
    ///             break;
    ///         default:
    ///             Debug.Log("Modal dismissed programmatically");
    ///             break;
    ///     }
    /// };
    ///
    /// modal.Show();
    /// ]]></code>
    /// </example>
    [VisualDocPage("popups")]
    public sealed class Modal : Popup<Modal>
    {
        /// <summary>
        /// Callback for Event triggered when the popup has been shown.
        /// </summary>
        readonly EventCallback<ITransitionEvent> m_OnAnimatedInAction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="referenceView">The view used as context provider for the Modal.</param>
        /// <param name="modalView">The popup visual element itself.</param>
        /// <param name="content">The content that will appear inside this popup.</param>
        Modal(VisualElement referenceView, ModalVisualElement modalView, VisualElement content)
            : base(referenceView, modalView, content)
        {
            m_OnAnimatedInAction = OnAnimatedInInternal;
        }

        ModalVisualElement modal => (ModalVisualElement)view;

        /// <summary>
        /// <para>Set the fullscreen mode for this <see cref="Modal"/>.</para>
        /// <para>
        /// See <see cref="ModalFullScreenMode"/> values for more info.
        /// </para>
        /// </summary>
        public ModalFullScreenMode fullscreenMode
        {
            get => modal.fullScreenMode;
            set => modal.fullScreenMode = value;
        }

        /// <summary>
        /// `True` if the Modal can be dismissed by clicking outside of it, `False` otherwise.
        /// </summary>
        public bool outsideClickDismissEnabled { get; set; }

        /// <summary>
        /// The strategy used to determine if the click is outside the Modal.
        /// </summary>
        public OutsideClickStrategy outsideClickStrategy { get; set; } = OutsideClickStrategy.Bounds;

        /// <inheritdoc />
        internal override bool focusOutDismissable => outsideClickDismissEnabled;

        /// <summary>
        /// Set a new value for <see cref="fullscreenMode"/> property.
        /// </summary>
        /// <param name="mode">The new value.</param>
        /// <returns>The <see cref="Modal"/> object.</returns>
        public Modal SetFullScreenMode(ModalFullScreenMode mode)
        {
            fullscreenMode = mode;
            return this;
        }

        /// <summary>
        /// Activate the possibility to dismiss the Modal by clicking outside of it.
        /// </summary>
        /// <param name="dismissEnabled"> `True` to activate the feature, `False` otherwise.</param>
        /// <returns> The modal </returns>
        public Modal SetOutsideClickDismiss(bool dismissEnabled)
        {
            outsideClickDismissEnabled = dismissEnabled;
            return this;
        }

        /// <summary>
        /// Set the strategy used to determine if the click is outside the Modal.
        /// </summary>
        /// <param name="strategy"> The strategy to use.</param>
        /// <returns> The modal </returns>
        public Modal SetOutsideClickStrategy(OutsideClickStrategy strategy)
        {
            outsideClickStrategy = strategy;
            return this;
        }

        void OnTreeDown(PointerDownEvent evt)
        {
            if (!outsideClickDismissEnabled || outsideClickStrategy == 0 || view.parent == null)
                return;

            var index = view.parent.IndexOf(view);
            if (index != view.parent.childCount - 1)
                return;

            var shouldDismiss = true;
            if ((outsideClickStrategy & OutsideClickStrategy.Bounds) != 0)
                shouldDismiss = !modal.contentContainer.worldBound.Contains((Vector2)evt.position);

            if (shouldDismiss && (outsideClickStrategy & OutsideClickStrategy.Pick) != 0)
            {
                var picked = view.panel.Pick(evt.position);
                var commonAncestor = picked?.FindCommonAncestor(view);
                if (commonAncestor == view) // if the picked element is a child of the popover, don't dismiss
                    shouldDismiss = false;
            }

            if (!shouldDismiss)
                return;

            // prevent reopening the same modal again...
            evt.StopImmediatePropagation();
            Dismiss(DismissType.OutOfBounds);
        }

        /// <inheritdoc />
        protected override bool ShouldDismiss(DismissType reason) => outsideClickDismissEnabled || base.ShouldDismiss(reason);

        /// <inheritdoc />
        protected override bool ShouldAnimate() => base.ShouldAnimate();

        /// <inheritdoc />
        protected override void AnimateViewIn()
        {
            base.AnimateViewIn();
            view.RegisterCallback<TransitionEndEvent>(m_OnAnimatedInAction);
            view.RegisterCallback<TransitionCancelEvent>(m_OnAnimatedInAction);
        }

        /// <summary>
        /// Called when the popup has been animated in.
        /// </summary>
        /// <param name="evt"> The transition event.</param>
        void OnAnimatedInInternal(ITransitionEvent evt)
        {
            view.UnregisterCallback<TransitionEndEvent>(m_OnAnimatedInAction);
            view.UnregisterCallback<TransitionCancelEvent>(m_OnAnimatedInAction);
            m_InvokeShownAction();
        }

        /// <inheritdoc />
        protected override void InvokeShownEventHandlers()
        {
            base.InvokeShownEventHandlers();
            rootView?.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
        }

        /// <inheritdoc />
        protected override void HideView(DismissType reason)
        {
            rootView?.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            base.HideView(reason);
        }

        /// <summary>
        /// Build a new Modal component.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element inside the UI panel.</param>
        /// <param name="content">The <see cref="VisualElement"/> UI element to display inside this <see cref="Modal"/>.</param>
        /// <returns>The <see cref="Modal"/> instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="referenceView"/> is null.</exception>
        public static Modal Build(VisualElement referenceView, VisualElement content)
        {
            if (referenceView == null)
                throw new ArgumentNullException(nameof(referenceView));

            var popup = new Modal(referenceView, new ModalVisualElement(content), content)
                .SetLastFocusedElement(referenceView);
            return popup;
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return modal.contentContainer;
        }

        /// <summary>
        /// The Modal UI Element.
        /// </summary>
        class ModalVisualElement : VisualElement
        {
            public const string ussClassName = "appui-modal";

            public const string fullScreenUssClassName = ussClassName + "--fullscreen";

            public const string fullScreenTakeOverUssClassName = ussClassName + "--fullscreen-takeover";

            public const string contentContainerUssClassName = ussClassName + "__content";

            readonly VisualElement m_ContentContainer;

            ModalFullScreenMode m_FullScreenMode = ModalFullScreenMode.None;

            public ModalFullScreenMode fullScreenMode
            {
                get => m_FullScreenMode;
                set
                {
                    m_FullScreenMode = value;
                    EnableInClassList(fullScreenUssClassName, m_FullScreenMode == ModalFullScreenMode.FullScreen);
                    EnableInClassList(fullScreenTakeOverUssClassName, m_FullScreenMode == ModalFullScreenMode.FullScreenTakeOver);
                }
            }

            public ModalVisualElement(VisualElement content)
            {
                AddToClassList(ussClassName);

                pickingMode = PickingMode.Position;

                m_ContentContainer = new ExVisualElement { name = contentContainerUssClassName, pickingMode = PickingMode.Position, focusable = true, passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows };

                m_ContentContainer.AddToClassList(contentContainerUssClassName);

                hierarchy.Add(m_ContentContainer);

                m_ContentContainer.Add(content);
                fullScreenMode = ModalFullScreenMode.None;
            }

            public override VisualElement contentContainer => m_ContentContainer;
        }
    }
}
