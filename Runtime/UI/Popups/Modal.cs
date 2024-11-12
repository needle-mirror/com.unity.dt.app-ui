using System;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
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
    /// The Modal Popup class.
    /// </summary>
    public sealed class Modal : Popup<Modal>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parentView">The popup container.</param>
        /// <param name="modalView">The popup visual element itself.</param>
        /// <param name="content">The content that will appear inside this popup.</param>
        Modal(VisualElement parentView, ModalVisualElement modalView, VisualElement content)
            : base(parentView, modalView, content)
        {
        }

        ModalVisualElement modal => (ModalVisualElement)view;

        /// <summary>
        /// <para>
        /// Set the fullscreen mode for this <see cref="Modal"/>.
        /// </para>
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
        /// Build a new Modal component.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element inside the UI panel.</param>
        /// <param name="content">The <see cref="VisualElement"/> UI element to display inside this <see cref="Modal"/>.</param>
        /// <returns>The <see cref="Modal"/> instance.</returns>
        public static Modal Build(VisualElement referenceView, VisualElement content)
        {
            var panel = referenceView as Panel ?? referenceView.GetFirstAncestorOfType<Panel>();

            if (panel == null)
                throw new ArgumentException("The reference view must be attached to a panel.", nameof(referenceView));

            var parentView = panel.popupContainer;
            var popup = new Modal(parentView, new ModalVisualElement(content), content)
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
                m_ContentContainer.SetIsCompositeRoot(true);
                m_ContentContainer.SetExcludeFromFocusRing(true);
                m_ContentContainer.delegatesFocus = true;

                m_ContentContainer.AddToClassList(contentContainerUssClassName);

                hierarchy.Add(m_ContentContainer);

                m_ContentContainer.Add(content);
                fullScreenMode = ModalFullScreenMode.None;
            }

            public override VisualElement contentContainer => m_ContentContainer;
        }
    }
}
