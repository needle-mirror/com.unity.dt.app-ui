using System;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Attachment UI element. Displays an attachment with a thumbnail area, title, subtitle,
    /// and a delete action button.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class Attachment : BaseVisualElement, IPressable
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId subtitleProperty = nameof(subtitle);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId readOnlyProperty = nameof(readOnly);

        internal static readonly BindingId compactProperty = nameof(compact);
#endif

        /// <summary>
        /// The Attachment main styling class.
        /// </summary>
        public const string ussClassName = "appui-attachment";

        /// <summary>
        /// The Attachment thumbnail styling class.
        /// </summary>
        public const string thumbnailUssClassName = ussClassName + "__thumbnail";

        /// <summary>
        /// The Attachment info container styling class.
        /// </summary>
        public const string infoUssClassName = ussClassName + "__info";

        /// <summary>
        /// The Attachment title styling class.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The Attachment subtitle styling class.
        /// </summary>
        public const string subtitleUssClassName = ussClassName + "__subtitle";

        /// <summary>
        /// The Attachment delete button styling class.
        /// </summary>
        public const string deleteBtnUssClassName = ussClassName + "__delete-btn";

        /// <summary>
        /// The Attachment read-only styling class.
        /// </summary>
        public const string readOnlyUssClassName = ussClassName + "--readonly";

        /// <summary>
        /// The Attachment compact styling class.
        /// </summary>
        public const string compactUssClassName = ussClassName + "--compact";

        /// <summary>
        /// The Attachment icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        readonly VisualElement m_ThumbnailContainer;

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_TitleElement;

        readonly LocalizedTextElement m_SubtitleElement;

        readonly ActionButton m_DeleteButton;

        Pressable m_Clickable;

        /// <summary>
        /// Event invoked when the delete button is clicked.
        /// </summary>
        public event Action deleteClicked;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Attachment()
        {
            AddToClassList(ussClassName);

            clickable = new Pressable();
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            // Icon (used in compact mode)
            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore, size = IconSize.S };
            m_Icon.AddToClassList(iconUssClassName);

            // Thumbnail
            m_ThumbnailContainer = new VisualElement { name = thumbnailUssClassName, pickingMode = PickingMode.Ignore };
            m_ThumbnailContainer.AddToClassList(thumbnailUssClassName);

            // Info
            var info = new VisualElement { name = infoUssClassName, pickingMode = PickingMode.Ignore };
            info.AddToClassList(infoUssClassName);

            m_TitleElement = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_TitleElement.AddToClassList(titleUssClassName);

            m_SubtitleElement = new LocalizedTextElement { name = subtitleUssClassName, pickingMode = PickingMode.Ignore };
            m_SubtitleElement.AddToClassList(subtitleUssClassName);

            info.hierarchy.Add(m_TitleElement);
            info.hierarchy.Add(m_SubtitleElement);

            // Delete button
            m_DeleteButton = new ActionButton
            {
                name = deleteBtnUssClassName,
                quiet = true,
                size = Size.S,
                icon = "x",
                iconVariant = IconVariant.Regular
            };
            m_DeleteButton.AddToClassList(deleteBtnUssClassName);
            m_DeleteButton.clicked += OnDeleteClicked;

            hierarchy.Add(m_Icon);
            hierarchy.Add(m_ThumbnailContainer);
            hierarchy.Add(info);
            hierarchy.Add(m_DeleteButton);

            compact = true;
        }

        /// <summary>
        /// The content container of the Attachment. Returns the thumbnail container
        /// so that <c>attachment.Add(element)</c> adds elements to the thumbnail area.
        /// </summary>
        public override VisualElement contentContainer => m_ThumbnailContainer;

        /// <summary>
        /// Clickable Manipulator for this Attachment.
        /// </summary>
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = value != m_Clickable;
                if (m_Clickable != null)
                {
                    if (m_Clickable.target == this)
                        this.RemoveManipulator(m_Clickable);
                }
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
#endif
            }
        }

        /// <summary>
        /// The Attachment click event.
        /// </summary>
        public event Action clicked
        {
            add => clickable.clicked += value;
            remove => clickable.clicked -= value;
        }

        /// <summary>
        /// The title text of the attachment.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string title
        {
            get => m_TitleElement.text;
            set
            {
                var changed = m_TitleElement.text != value;
                m_TitleElement.text = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in titleProperty);
#endif
            }
        }

        /// <summary>
        /// The subtitle text of the attachment.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string subtitle
        {
            get => m_SubtitleElement.text;
            set
            {
                var changed = m_SubtitleElement.text != value;
                m_SubtitleElement.text = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in subtitleProperty);
#endif
            }
        }

        /// <summary>
        /// Whether the Attachment is in read-only mode. When true, the delete button is hidden.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool readOnly
        {
            get => ClassListContains(readOnlyUssClassName);
            set
            {
                var changed = ClassListContains(readOnlyUssClassName) != value;
                EnableInClassList(readOnlyUssClassName, value);
                m_DeleteButton.style.display = value ? DisplayStyle.None : DisplayStyle.Flex;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in readOnlyProperty);
#endif
            }
        }

        /// <summary>
        /// The Icon displayed in compact mode. Configure via <c>icon.iconName</c> and <c>icon.variant</c>.
        /// </summary>
        public Icon icon => m_Icon;

        /// <summary>
        /// Whether the Attachment is in compact mode. When true (the default), the Attachment renders as a one-line row
        /// using <see cref="icon"/> instead of the thumbnail container.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool compact
        {
            get => ClassListContains(compactUssClassName);
            set
            {
                var changed = ClassListContains(compactUssClassName) != value;
                EnableInClassList(compactUssClassName, value);
                m_Icon.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                m_ThumbnailContainer.style.display = value ? DisplayStyle.None : DisplayStyle.Flex;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in compactProperty);
#endif
            }
        }

        void OnDeleteClicked()
        {
            deleteClicked?.Invoke();
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the Attachment.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<Attachment, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Attachment"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription
            {
                name = "title",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Subtitle = new UxmlStringAttributeDescription
            {
                name = "subtitle",
                defaultValue = null
            };

            readonly UxmlBoolAttributeDescription m_ReadOnly = new UxmlBoolAttributeDescription
            {
                name = "read-only",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Compact = new UxmlBoolAttributeDescription
            {
                name = "compact",
                defaultValue = true
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

                var el = (Attachment)ve;
                el.title = m_Title.GetValueFromBag(bag, cc);
                el.subtitle = m_Subtitle.GetValueFromBag(bag, cc);
                el.readOnly = m_ReadOnly.GetValueFromBag(bag, cc);
                el.compact = m_Compact.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
