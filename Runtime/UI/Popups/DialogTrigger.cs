using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// By providing a type prop, you can specify the type of Dialog that is rendered by your DialogTrigger.
    /// </summary>
    /// <remarks>
    /// Note that pressing the Esc key will close the Dialog regardless of its type.
    /// </remarks>
    public enum PopupPresentationType
    {
        /// <summary>
        /// Modal Dialogs create an underlay that blocks access to the underlying user interface until the Dialog is closed.
        /// Sizing options can be found on the Dialog page.
        /// Focus is trapped inside the Modal.
        /// </summary>
        Modal,

        /// <summary>
        /// If a Dialog without an underlay is needed, consider using a Popover Dialog.
        /// See Dialog placement for how you can customize the positioning.
        /// Note that popovers are automatically rendered as modals on mobile by default.
        /// See the mobile type option for more information.
        /// </summary>
        Popover,

        /// <summary>
        /// Tray Dialogs are typically used to portray information on mobile devices or smaller screens.
        /// </summary>
        Tray,

        /// <summary>
        /// Fullscreen Dialogs are a fullscreen variant of the Modal Dialog, only revealing a small portion of the page
        /// behind the underlay. Use this variant for more complex workflows that do not fit in the available
        /// Modal Dialog sizes.
        /// This variant does not support dismissible.
        /// </summary>
        FullScreen,

        /// <summary>
        /// Fullscreen takeover Dialogs are similar to the fullscreen variant except that the Dialog covers the entire screen.
        /// </summary>
        FullScreenTakeOver,
    }

    /// <summary>
    /// Same as <see cref="PopupPresentationType"/> but for Mobile explicitly.
    /// </summary>
    public enum MobilePopupPresentationType
    {
        /// <summary>
        /// Modal Dialogs create an underlay that blocks access to the underlying user interface until the Dialog is closed.
        /// Sizing options can be found on the Dialog page.
        /// Focus is trapped inside the Modal.
        /// </summary>
        Modal,

        /// <summary>
        /// Tray Dialogs are typically used to portray information on mobile devices or smaller screens.
        /// </summary>
        Tray,

        /// <summary>
        /// Fullscreen Dialogs are a fullscreen variant of the Modal Dialog, only revealing a small portion of the page
        /// behind the underlay. Use this variant for more complex workflows that do not fit in the available
        /// Modal Dialog sizes.
        /// This variant does not support dismissible.
        /// </summary>
        FullScreen,

        /// <summary>
        /// Fullscreen takeover Dialogs are similar to the fullscreen variant except that the Dialog covers the entire screen.
        /// </summary>
        FullScreenTakeOver,
    }

    /// <summary>
    /// DialogTrigger serves as a wrapper around a Dialog and its associated trigger,
    /// linking the Dialog's open state with the trigger's press state. Additionally,
    /// it allows you to customize the type and positioning of the Dialog.
    /// </summary>
    [UxmlElement]
    public partial class DialogTrigger : BaseVisualElement
    {

        internal static readonly BindingId triggerProperty = new BindingId(nameof(trigger));

        internal static readonly BindingId anchorProperty = new BindingId(nameof(anchor));

        internal static readonly BindingId dialogProperty = new BindingId(nameof(dialog));

        internal static readonly BindingId typeProperty = new BindingId(nameof(type));

        internal static readonly BindingId trayPositionProperty = new BindingId(nameof(trayPosition));

        internal static readonly BindingId transitionDurationProperty = new BindingId(nameof(transitionDuration));

        internal static readonly BindingId hideArrowProperty = new BindingId(nameof(hideArrow));

        internal static readonly BindingId mobileTypeProperty = new BindingId(nameof(mobileType));

        internal static readonly BindingId containerPaddingProperty = new BindingId(nameof(containerPadding));

        internal static readonly BindingId offsetProperty = new BindingId(nameof(offset));

        internal static readonly BindingId crossOffsetProperty = new BindingId(nameof(crossOffset));

        internal static readonly BindingId shouldFlipProperty = new BindingId(nameof(shouldFlip));

        internal static readonly BindingId isOpenProperty = new BindingId(nameof(isOpen));

        internal static readonly BindingId keyboardDismissEnabledProperty = new BindingId(nameof(keyboardDismissEnabled));

        internal static readonly BindingId outsideClickDismissEnabledProperty = new BindingId(nameof(outsideClickDismissEnabled));

        internal static readonly BindingId modalBackdropProperty = new BindingId(nameof(modalBackdrop));

        internal static readonly BindingId placementProperty = new BindingId(nameof(placement));

        internal static readonly BindingId movableProperty = new BindingId(nameof(movable));

        internal static readonly BindingId resizableProperty = new BindingId(nameof(resizable));

        internal static readonly BindingId resizeDirectionProperty = new BindingId(nameof(resizeDirection));

        internal static readonly BindingId disableAnimationProperty = new BindingId(nameof(disableAnimation));


        string m_AnchorName = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DialogTrigger()
        {
            pickingMode = PickingMode.Ignore;

            anchor = null;
            type = PopupPresentationType.Modal;
            trayPosition = TrayPosition.Bottom;
            transitionDuration = 150;
            hideArrow = false;
            mobileType = MobilePopupPresentationType.Modal;
            containerPadding = 0;
            offset = 0;
            crossOffset = 0;
            shouldFlip = true;
            keyboardDismissEnabled = true;
            outsideClickDismissEnabled = true;
            modalBackdrop = false;
            placement = PopoverPlacement.Top;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        BaseDialog m_Dialog;

        /// <summary>
        /// The dialog to display.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public BaseDialog dialog
        {
            get => m_Dialog;
            private set
            {
                var changed = m_Dialog != value;
                m_Dialog = value;

                if (changed)
                    NotifyPropertyChanged(in dialogProperty);
            }
        }

        /// <summary>
        /// The trigger that will be used to start the display of the <see cref="dialog"/> element.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public VisualElement trigger { get; private set; }

        PopupPresentationType m_Type;

        /// <summary>
        /// The type of presentation used for this <see cref="dialog"/> element.
        /// </summary>
        /// <remarks>
        /// Some types are not available on mobile, to specify different presentation on mobile context use the <see cref="mobileType"/> property.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public PopupPresentationType type
        {
            get => m_Type;
            set
            {
                var changed = m_Type != value;
                m_Type = value;

                if (changed)
                    NotifyPropertyChanged(in typeProperty);
            }
        }

        TrayPosition m_TrayPosition;

        /// <summary>
        /// The position of the Tray element.
        /// </summary>
        /// <remarks>
        /// This property is useful only if you set the <see cref="type"/> property to <see cref="PopupPresentationType.Tray"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public TrayPosition trayPosition
        {
            get => m_TrayPosition;
            set
            {
                var changed = m_TrayPosition != value;
                m_TrayPosition = value;

                if (changed)
                    NotifyPropertyChanged(in trayPositionProperty);
            }
        }

        int m_TransitionDuration;

        /// <summary>
        /// The duration of the transition in milliseconds.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int transitionDuration
        {
            get => m_TransitionDuration;
            set
            {
                var changed = m_TransitionDuration != value;
                m_TransitionDuration = value;

                if (changed)
                    NotifyPropertyChanged(in transitionDurationProperty);
            }
        }

        bool m_HideArrow;

        /// <summary>
        /// Should the arrow be hidden.
        /// </summary>
        /// <remarks>
        /// This property is only useful with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool hideArrow
        {
            get => m_HideArrow;
            set
            {
                var changed = m_HideArrow != value;
                m_HideArrow = value;

                if (changed)
                    NotifyPropertyChanged(in hideArrowProperty);
            }
        }

        MobilePopupPresentationType m_MobileType;

        /// <summary>
        /// The type of presentation used for this <see cref="dialog"/> element on mobile platforms.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public MobilePopupPresentationType mobileType
        {
            get => m_MobileType;
            set
            {
                var changed = m_MobileType != value;
                m_MobileType = value;

                if (changed)
                    NotifyPropertyChanged(in mobileTypeProperty);
            }
        }

        int m_ContainerPadding;

        /// <summary>
        /// The padding in pixels of the content inside the Popup.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int containerPadding
        {
            get => m_ContainerPadding;
            set
            {
                var changed = m_ContainerPadding != value;
                m_ContainerPadding = value;

                if (changed)
                    NotifyPropertyChanged(in containerPaddingProperty);
            }
        }

        int m_Offset;

        /// <summary>
        /// The offset in pixels in the direction of the <see cref="placement"/> primary vector.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int offset
        {
            get => m_Offset;
            set
            {
                var changed = m_Offset != value;
                m_Offset = value;

                if (changed)
                    NotifyPropertyChanged(in offsetProperty);
            }
        }

        int m_CrossOffset;

        /// <summary>
        /// The offset in pixels in the direction of the <see cref="placement"/> secondary vector.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int crossOffset
        {
            get => m_CrossOffset;
            set
            {
                var changed = m_CrossOffset != value;
                m_CrossOffset = value;

                if (changed)
                    NotifyPropertyChanged(in crossOffsetProperty);
            }
        }

        bool m_ShouldFlip;

        /// <summary>
        /// Should the Popover <see cref="placement"/> be flipped if there's not enough space.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool shouldFlip
        {
            get => m_ShouldFlip;
            set
            {
                var changed = m_ShouldFlip != value;
                m_ShouldFlip = value;

                if (changed)
                    NotifyPropertyChanged(in shouldFlipProperty);
            }
        }

        bool m_IsOpen;

        /// <summary>
        /// The open state of the dialog.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public bool isOpen
        {
            get => m_IsOpen;
            private set
            {
                var changed = m_IsOpen != value;
                m_IsOpen = value;

                if (changed)
                    NotifyPropertyChanged(in isOpenProperty);
            }
        }

        bool m_KeyboardDismissEnabled = true;

        /// <summary>
        /// Disallow the use of Escape key or Return button to dismiss the <see cref="dialog"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool keyboardDismissEnabled
        {
            get => m_KeyboardDismissEnabled;
            set
            {
                var changed = m_KeyboardDismissEnabled != value;
                m_KeyboardDismissEnabled = value;

                if (changed)
                    NotifyPropertyChanged(in keyboardDismissEnabledProperty);
            }
        }

        bool m_OutsideClickDismissEnabled;

        /// <summary>
        /// Allow the use of clicking outside the <see cref="dialog"/> to dismiss it.
        /// </summary>
        /// <remarks>
        /// This property works only with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool outsideClickDismissEnabled
        {
            get => m_OutsideClickDismissEnabled;
            set
            {
                var changed = m_OutsideClickDismissEnabled != value;
                m_OutsideClickDismissEnabled = value;

                if (changed)
                    NotifyPropertyChanged(in outsideClickDismissEnabledProperty);
            }
        }

        bool m_ModalBackdrop;

        /// <summary>
        /// Enable or disable the blocking of the UI behind the <see cref="dialog"/>.
        /// </summary>
        /// <remarks>
        /// This property works only with <see cref="PopupPresentationType.Popover"/> presentation type.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool modalBackdrop
        {
            get => m_ModalBackdrop;
            set
            {
                var changed = m_ModalBackdrop != value;
                m_ModalBackdrop = value;

                if (changed)
                    NotifyPropertyChanged(in modalBackdropProperty);
            }
        }

        VisualElement m_Anchor;

        /// <summary>
        /// The UI element used as an anchor.
        /// </summary>
        /// <remarks>
        /// This is only useful for presentations using popups of type <see cref="AnchorPopup{T}"/>.
        /// </remarks>
        [CreateProperty]
        public VisualElement anchor
        {
            get => m_Anchor;
            set
            {
                var changed = m_Anchor != value;
                m_Anchor = value;

                if (changed)
                    NotifyPropertyChanged(in anchorProperty);
            }
        }

        PopoverPlacement m_Placement;

        /// <summary>
        /// The placement of the Popover.
        /// </summary>
        /// <remarks>
        /// This is only useful for presentations using popups of type <see cref="AnchorPopup{T}"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public PopoverPlacement placement
        {
            get => m_Placement;
            set
            {
                var changed = m_Placement != value;
                m_Placement = value;

                if (changed)
                    NotifyPropertyChanged(in placementProperty);
            }
        }

        bool m_Movable;

        /// <summary>
        /// Whether the dialog is movable by dragging.
        /// </summary>
        /// <remarks>
        /// This is only useful for presentations using popups of type <see cref="Popover"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool movable
        {
            get => m_Movable;
            set
            {
                var changed = m_Movable != value;
                m_Movable = value;

                if (changed)
                    NotifyPropertyChanged(in movableProperty);
            }
        }

        bool m_Resizable;

        /// <summary>
        /// Whether the dialog is resizable.
        /// </summary>
        /// <remarks>
        /// This is only useful for presentations using popups of type <see cref="Popover"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool resizable
        {
            get => m_Resizable;
            set
            {
                var changed = m_Resizable != value;
                m_Resizable = value;

                if (changed)
                    NotifyPropertyChanged(in resizableProperty);
            }
        }

        Draggable.DragDirection m_ResizeDirection = Draggable.DragDirection.Free;

        /// <summary>
        /// Which direction the dialog can be resized.
        /// </summary>
        /// <remarks>
        /// This is only useful for presentations using popups of type <see cref="Popover"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public Draggable.DragDirection resizeDirection
        {
            get => m_ResizeDirection;
            set
            {
                var changed = m_ResizeDirection != value;
                m_ResizeDirection = value;

                if (changed)
                    NotifyPropertyChanged(in resizeDirectionProperty);
            }
        }

        bool m_DisableAnimation;

        /// <summary>
        /// Whether to disable the opening and closing animation of the dialog.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool disableAnimation
        {
            get => m_DisableAnimation;
            set
            {
                var changed = m_DisableAnimation != value;
                m_DisableAnimation = value;

                if (changed)
                    NotifyPropertyChanged(in disableAnimationProperty);
            }
        }

        /// <summary>
        /// The content container of the DialogTrigger.
        /// </summary>
        public override VisualElement contentContainer => this;

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            BaseDialog dlg = null;
            VisualElement ve = null;

            foreach (var child in Children())
            {
                if (dlg == null && child is BaseDialog d)
                    dlg = d;

                if (ve == null && !(child is BaseDialog))
                    ve = child;

                if (dlg != null && ve != null)
                    break;
            }

            if (dlg != null && dlg != dialog)
            {
                // New Dialog attached as child
                dialog = dlg;
                Remove(dlg);
            }

            if (ve != null && ve != trigger)
            {
                if (trigger is IPressable c1)
                    c1.clickable.clicked -= OnActionTriggered;
                trigger = ve;
                if (trigger is IPressable c2)
                    c2.clickable.clicked += OnActionTriggered;
            }

            // we can also try to find the anchor (if any has been given with the UXML attribute)
            if (!string.IsNullOrEmpty(m_AnchorName) && panel != null)
            {
                var anchorElement = panel.visualTree.Q<VisualElement>(m_AnchorName);
                if (anchorElement != null)
                    anchor = anchorElement;
                else
                    Debug.LogWarning($"Unable to find {m_AnchorName}");
            }
        }

        void OnActionTriggered()
        {
            switch (type)
            {
                case PopupPresentationType.Modal:
                    Modal.Build(trigger, dialog)
                        .SetDisableAnimation(disableAnimation)
                        .SetOutsideClickDismiss(outsideClickDismissEnabled)
                        .Show();
                    break;
                case PopupPresentationType.Popover:
                    Popover.Build(trigger, dialog)
                        .SetDisableAnimation(disableAnimation)
                        .SetPlacement(placement)
                        .SetShouldFlip(shouldFlip)
                        .SetOffset(offset)
                        .SetCrossOffset(crossOffset)
                        .SetArrowVisible(!hideArrow)
                        .SetContainerPadding(containerPadding)
                        .SetOutsideClickDismiss(outsideClickDismissEnabled)
                        .SetModalBackdrop(modalBackdrop)
                        .SetKeyboardDismiss(keyboardDismissEnabled)
                        .SetMovable(movable)
                        .SetResizable(resizable)
                        .SetResizeDirection(resizeDirection)
                        .Show();
                    break;
                case PopupPresentationType.Tray:
                    Tray.Build(trigger, dialog)
                        .SetDisableAnimation(disableAnimation)
                        .SetPosition(trayPosition)
                        .SetTransitionDuration(transitionDuration)
                        .Show();
                    break;
                case PopupPresentationType.FullScreen:
                    Modal.Build(trigger, dialog)
                        .SetDisableAnimation(disableAnimation)
                        .SetFullScreenMode(ModalFullScreenMode.FullScreen)
                        .Show();
                    break;
                case PopupPresentationType.FullScreenTakeOver:
                    Modal.Build(trigger, dialog)
                        .SetDisableAnimation(disableAnimation)
                        .SetFullScreenMode(ModalFullScreenMode.FullScreenTakeOver)
                        .Show();
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(type), type);
            }

            isOpen = true;
        }

    }
}
