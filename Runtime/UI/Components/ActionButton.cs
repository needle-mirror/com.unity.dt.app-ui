using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ActionButton UI element.
    /// </summary>
    [UxmlElement]
    public partial class ActionButton : ExVisualElement, ISizeableElement, ISelectableElement, IPressable
    {
        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId trailingIconProperty = nameof(trailingIcon);

        internal static readonly BindingId iconVariantProperty = nameof(iconVariant);

        internal static readonly BindingId trailingIconVariantProperty = nameof(trailingIconVariant);

        internal static readonly BindingId quietProperty = nameof(quiet);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId accentProperty = nameof(accent);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        /// <summary>
        /// The ActionButton main styling class.
        /// </summary>
        public const string ussClassName = "appui-actionbutton";

        /// <summary>
        /// The ActionButton icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The ActionButton trailing icon styling class.
        /// </summary>
        public const string trailingIconUssClassName = ussClassName + "__trailing-icon";

        /// <summary>
        /// The ActionButton label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The ActionButton icon and label variant styling class.
        /// </summary>
        public const string iconAndLabelUssClassName = ussClassName + "--icon-and-label";

        /// <summary>
        /// The ActionButton with trailing icon variant styling class.
        /// </summary>
        public const string withTrailingIconUSsClassName = ussClassName + "--with-trailing-icon";

        /// <summary>
        /// The ActionButton icon only variant styling class.
        /// </summary>
        public const string iconOnlyUssClassName = ussClassName + "--icon-only";

        /// <summary>
        /// The ActionButton quiet variant styling class.
        /// </summary>
        public const string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The ActionButton size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ActionButton accent styling class.
        /// </summary>
        public const string accentUssClassName = ussClassName + "--accent";

        readonly Icon m_IconElement;

        readonly LocalizedTextElement m_LabelElement;

        readonly Icon m_TrailingIconElement;

        Size m_Size;

        Pressable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionButton() : this(null) { }

        /// <summary>
        /// Construct a <see cref="ActionButton"/> with a given click event callback.
        /// </summary>
        /// <param name="clickEvent">THe given click event callback.</param>
        public ActionButton(Action clickEvent)
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(clickEvent);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            m_IconElement = new Icon { name = iconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);
            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, text = null, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            m_TrailingIconElement = new Icon { name = trailingIconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            m_TrailingIconElement.AddToClassList(trailingIconUssClassName);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnFocus));

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_LabelElement);
            hierarchy.Add(m_TrailingIconElement);

            passMask = 0;
            size = Size.M;
            accent = false;
            quiet = false;
            iconVariant = IconVariant.Regular;
            trailingIconVariant = IconVariant.Regular;

            Refresh();
        }

        void OnFocus(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// Clickable Manipulator for this ActionButton.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = value != m_Clickable;
                if (m_Clickable != null)
                {
                    m_Clickable.clicked -= OnClick;
                    if (m_Clickable.target == this)
                        this.RemoveManipulator(m_Clickable);
                }
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                m_Clickable.clicked += OnClick;
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// The ActionButton click event.
        /// </summary>
        public event Action clicked
        {
            add => clickable.clicked += value;
            remove => clickable.clicked -= value;
        }

        /// <summary>
        /// The ActionButton label.
        /// </summary>
        [Tooltip("The ActionButton label.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Action Button")]
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                var changed = m_LabelElement.text != value;
                m_LabelElement.text = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The ActionButton icon.
        /// </summary>
        [Tooltip("The ActionButton icon.")]
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_IconElement.iconName;
            set
            {
                var changed = m_IconElement.iconName != value;
                m_IconElement.iconName = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The ActionButton trailing icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string trailingIcon
        {
            get => m_TrailingIconElement.iconName;
            set
            {
                var changed = m_TrailingIconElement.iconName != value;
                m_TrailingIconElement.iconName = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in trailingIconProperty);
            }
        }

        /// <summary>
        /// The ActionButton icon variant.
        /// </summary>
        [Tooltip("The ActionButton icon variant.")]
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant iconVariant
        {
            get => m_IconElement.variant;
            set
            {
                var changed = m_IconElement.variant != value;
                m_IconElement.variant = value;
                if (changed)
                    NotifyPropertyChanged(in iconVariantProperty);
            }
        }

        /// <summary>
        /// The ActionButton trailing icon variant.
        /// </summary>
        [Tooltip("The ActionButton trailing icon variant.")]
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant trailingIconVariant
        {
            get => m_TrailingIconElement.variant;
            set
            {
                var changed = m_TrailingIconElement.variant != value;
                m_TrailingIconElement.variant = value;
                if (changed)
                    NotifyPropertyChanged(in trailingIconVariantProperty);
            }
        }

        /// <summary>
        /// The selected state of the ActionButton.
        /// </summary>
        [Tooltip("The selected state of the ActionButton")]
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = selected != value;
                SetSelectedWithoutNotify(value);
                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// The quiet state of the ActionButton.
        /// </summary>
        [Tooltip("The quiet state of the ActionButton")]
        [CreateProperty]
        [UxmlAttribute]
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set
            {
                var changed = quiet != value;
                EnableInClassList(quietUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in quietProperty);
            }
        }

        /// <summary>
        /// The accent variant of the ActionButton.
        /// </summary>
        [Tooltip("The accent variant of the ActionButton")]
        [CreateProperty]
        [UxmlAttribute]
        public bool accent
        {
            get => ClassListContains(accentUssClassName);
            set
            {
                var changed = accent != value;
                EnableInClassList(accentUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in accentProperty);
            }
        }

        /// <summary>
        /// The content container of the ActionButton.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The current size of the ActionButton.
        /// </summary>
        [Tooltip("The current size of the ActionButton.")]
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Set the selected state of the ActionButton without notifying the click event.
        /// </summary>
        /// <param name="newValue"> The new selected state.</param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }

        void Refresh()
        {
            EnableInClassList(iconAndLabelUssClassName, !string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(label));
            EnableInClassList(withTrailingIconUSsClassName, !string.IsNullOrEmpty(trailingIcon));
            EnableInClassList(iconOnlyUssClassName, !string.IsNullOrEmpty(icon) && string.IsNullOrEmpty(label) && string.IsNullOrEmpty(trailingIcon));
            m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(label));
            m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon));
            m_TrailingIconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(trailingIcon));
        }

        void OnClick()
        {
            using var evt = ActionTriggeredEvent.GetPooled();
            evt.target = this;
            SendEvent(evt);
        }

    }
}
