using System;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadMessage UI element. Displays a single message within a thread.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ThreadMessage : BaseVisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId authorNameProperty = nameof(authorName);

        internal static readonly BindingId timestampProperty = nameof(timestamp);

        internal static readonly BindingId timestampTooltipProperty = nameof(timestampTooltip);

        internal static readonly BindingId contentProperty = nameof(message);

        internal static readonly BindingId stateProperty = nameof(state);

        internal static readonly BindingId likeCountProperty = nameof(likeCount);

        internal static readonly BindingId dislikeCountProperty = nameof(dislikeCount);

        internal static readonly BindingId isLikedProperty = nameof(isLiked);

        internal static readonly BindingId isDislikedProperty = nameof(isDisliked);

        internal static readonly BindingId authorAvatarProperty = nameof(authorAvatar);

        internal static readonly BindingId authorInitialsProperty = nameof(authorInitials);

        internal static readonly BindingId authorAvatarColorProperty = nameof(authorAvatarColor);

        internal static readonly BindingId reactionsProperty = nameof(reactions);

        internal static readonly BindingId makeActionMenuItemsProperty = nameof(makeActionMenuItems);
#endif

        /// <summary>
        /// The ThreadMessage main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-message";

        /// <summary>
        /// The ThreadMessage header styling class.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The ThreadMessage avatar styling class.
        /// </summary>
        public const string avatarUssClassName = ussClassName + "__avatar";

        /// <summary>
        /// The ThreadMessage meta styling class.
        /// </summary>
        public const string metaUssClassName = ussClassName + "__meta";

        /// <summary>
        /// The ThreadMessage author name styling class.
        /// </summary>
        public const string authorNameUssClassName = ussClassName + "__author-name";

        /// <summary>
        /// The ThreadMessage timestamp styling class.
        /// </summary>
        public const string timestampUssClassName = ussClassName + "__timestamp";

        /// <summary>
        /// The ThreadMessage actions styling class.
        /// </summary>
        public const string actionsUssClassName = ussClassName + "__actions";

        /// <summary>
        /// The ThreadMessage action Button styling class.
        /// </summary>
        public const string actionButtonUssClassName = ussClassName + "__action-button";

        /// <summary>
        /// The ThreadMessage body styling class.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>
        /// The ThreadMessage content styling class.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The ThreadMessage attachments styling class.
        /// </summary>
        public const string attachmentsUssClassName = ussClassName + "__attachments";

        /// <summary>
        /// The ThreadMessage reactions styling class.
        /// </summary>
        public const string reactionsUssClassName = ussClassName + "__reactions";

        /// <summary>
        /// The ThreadMessage footer styling class.
        /// </summary>
        public const string footerUssClassName = ussClassName + "__footer";

        /// <summary>
        /// The ThreadMessage like button styling class.
        /// </summary>
        public const string likeBtnUssClassName = ussClassName + "__like-btn";

        /// <summary>
        /// The ThreadMessage dislike button styling class.
        /// </summary>
        public const string dislikeBtnUssClassName = ussClassName + "__dislike-btn";

        /// <summary>
        /// The ThreadMessage content container styling class.
        /// </summary>
        public const string contentContainerUssClassName = ussClassName + "__content-container";

        /// <summary>
        /// The ThreadMessage actions-open modifier styling class.
        /// </summary>
        public const string actionsOpenUssClassName = ussClassName + "--actions-open";

        /// <summary>
        /// The ThreadMessage state modifier styling class.
        /// </summary>
        [EnumName("GetStateUssClassName", typeof(ThreadMessageState))]
        public const string stateUssClassName = ussClassName + "--";

        readonly Avatar m_AvatarElement;

        readonly TextElement m_AuthorNameElement;

        readonly TextElement m_TimestampElement;

        readonly ActionButton m_ActionsButton;

        readonly TextElement m_ContentElement;

        readonly ThreadReactionBar m_ReactionBar;

        readonly VisualElement m_FooterContainer;

        readonly ActionButton m_LikeButton;

        readonly ActionButton m_DislikeButton;

        readonly VisualElement m_ContentContainer;

        ThreadMessageState m_State;

        int m_LikeCount;

        int m_DislikeCount;

        IList<ReactionInfo> m_Reactions;

        string m_RawMessage;

        /// <summary>
        /// The attachments container.
        /// Add attachment elements to this container to have them displayed in the message attachments area.
        /// </summary>
        public VisualElement attachments { get; }

        /// <summary>
        /// Event invoked when the like button  is toggled.
        /// </summary>
        public event Action<bool> likeToggled;

        /// <summary>
        /// Event invoked when the dislike button is toggled.
        /// </summary>
        public event Action<bool> dislikeToggled;

        /// <summary>
        /// Event invoked when a reaction is toggled.
        /// </summary>
        public event Action<string, bool> reactionToggled;

        Action<ThreadMessage, MenuBuilder> m_MakeActionMenuItems;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreadMessage()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;

            focusable = true;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            delegatesFocus = true;

            // Header
            var header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore };
            header.AddToClassList(headerUssClassName);

            m_AvatarElement = new Avatar { name = avatarUssClassName, size = Size.S, variant = AvatarVariant.Circular, pickingMode = PickingMode.Ignore };
            m_AvatarElement.AddToClassList(avatarUssClassName);

            m_AvatarElement.autoLabelColor = true;

            var meta = new VisualElement { name = metaUssClassName, pickingMode = PickingMode.Ignore };
            meta.AddToClassList(metaUssClassName);

            m_AuthorNameElement = new TextElement { name = authorNameUssClassName, pickingMode = PickingMode.Ignore };
            m_AuthorNameElement.AddToClassList(authorNameUssClassName);

            m_TimestampElement = new TextElement { name = timestampUssClassName, pickingMode = PickingMode.Ignore };
            m_TimestampElement.AddToClassList(timestampUssClassName);

            meta.hierarchy.Add(m_AuthorNameElement);
            meta.hierarchy.Add(m_TimestampElement);

            var actions = new VisualElement { name = actionsUssClassName, pickingMode = PickingMode.Ignore };
            actions.AddToClassList(actionsUssClassName);

            m_ActionsButton = new ActionButton
            {
                name = actionButtonUssClassName,
                quiet = true,
                size = Size.S,
                icon = "dots-three",
                iconVariant = IconVariant.Bold
            };
            m_ActionsButton.AddToClassList(actionButtonUssClassName);
            m_ActionsButton.clicked += OnActionButtonClicked;

            actions.hierarchy.Add(m_ActionsButton);

            header.hierarchy.Add(m_AvatarElement);
            header.hierarchy.Add(meta);
            header.hierarchy.Add(actions);

            // Body
            var body = new VisualElement { name = bodyUssClassName, pickingMode = PickingMode.Ignore };
            body.AddToClassList(bodyUssClassName);

            m_ContentElement = new TextElement
            {
                name = contentUssClassName,
                pickingMode = PickingMode.Ignore,
                enableRichText = true,
                displayTooltipWhenElided = false
            };
            m_ContentElement.AddToClassList(contentUssClassName);
            body.hierarchy.Add(m_ContentElement);

            // Attachments
            attachments = new VisualElement { name = attachmentsUssClassName, pickingMode = PickingMode.Ignore };
            attachments.AddToClassList(attachmentsUssClassName);

            // Footer
            m_FooterContainer = new VisualElement { name = footerUssClassName, pickingMode = PickingMode.Ignore };
            m_FooterContainer.AddToClassList(footerUssClassName);

            m_LikeButton = new ActionButton { name = likeBtnUssClassName, quiet = true, icon = "thumbs-up", size = Size.M };
            m_LikeButton.AddToClassList(likeBtnUssClassName);
            m_LikeButton.clicked += OnLikeClicked;

            m_DislikeButton = new ActionButton { name = dislikeBtnUssClassName, quiet = true, icon = "thumbs-down", size = Size.M };
            m_DislikeButton.AddToClassList(dislikeBtnUssClassName);
            m_DislikeButton.clicked += OnDislikeClicked;

            // Reactions
            m_ReactionBar = new ThreadReactionBar { name = reactionsUssClassName, pickingMode = PickingMode.Ignore };
            m_ReactionBar.AddToClassList(reactionsUssClassName);
            m_ReactionBar.reactionToggled += OnReactionToggled;

            m_FooterContainer.hierarchy.Add(m_LikeButton);
            m_FooterContainer.hierarchy.Add(m_DislikeButton);
            m_FooterContainer.hierarchy.Add(m_ReactionBar);

            m_ContentContainer = new VisualElement { name = contentContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_ContentContainer.AddToClassList(contentContainerUssClassName);

            hierarchy.Add(header);
            hierarchy.Add(m_ContentContainer);
            hierarchy.Add(body);
            hierarchy.Add(attachments);
            hierarchy.Add(m_FooterContainer);

            // Defaults
            state = ThreadMessageState.Default;
            makeActionMenuItems = null;
            likeCount = 0;
            dislikeCount = 0;
            isLiked = false;
            isDisliked = false;

            this.RegisterContextChangedCallback<ThreadContext>(OnThreadContextChanged);
        }

        void OnThreadContextChanged(ContextChangedEvent<ThreadContext> evt)
        {
            var ctx = evt.context;
            m_ReactionBar.EnableInClassList(Styles.hiddenUssClassName, ctx is not {enableReactions: true});
            m_LikeButton.EnableInClassList(Styles.hiddenUssClassName, ctx is not {enableLikes: true});
            m_DislikeButton.EnableInClassList(Styles.hiddenUssClassName, ctx is not {enableDislikes: true});
            RefreshContent();
        }

        void OnActionButtonClicked()
        {
            var menu = MenuBuilder.Build(m_ActionsButton, new Menu());
            makeActionMenuItems?.Invoke(this, menu);
            AddToClassList(actionsOpenUssClassName);
            menu.dismissed += (_, _) => RemoveFromClassList(actionsOpenUssClassName);
            menu.Show();
        }

        void OnLikeClicked()
        {
            isLiked = !isLiked;
            likeToggled?.Invoke(isLiked);
        }

        void OnDislikeClicked()
        {
            isDisliked = !isDisliked;
            dislikeToggled?.Invoke(isDisliked);
        }

        void OnReactionToggled(string emoji, bool isAdding)
        {
            reactionToggled?.Invoke(emoji, isAdding);
        }

        /// <summary>
        /// The container for the message. Use this to add a composer in draft mode.
        /// </summary>
        public override VisualElement contentContainer => m_ContentContainer;

        /// <summary>
        /// The author name displayed in the message header.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
        [Header("Thread Message")]
#endif
        public string authorName
        {
            get => m_AuthorNameElement.text;
            set
            {
                var changed = m_AuthorNameElement.text != value;
                m_AuthorNameElement.text = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in authorNameProperty);
#endif
            }
        }

        /// <summary>
        /// The timestamp text displayed in the message header.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string timestamp
        {
            get => m_TimestampElement.text;
            set
            {
                var changed = m_TimestampElement.text != value;
                m_TimestampElement.text = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in timestampProperty);
#endif
            }
        }

        /// <summary>
        /// The tooltip displayed on the timestamp element.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string timestampTooltip
        {
            get => m_TimestampElement.tooltip;
            set
            {
                var changed = m_TimestampElement.tooltip != value;
                m_TimestampElement.tooltip = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in timestampTooltipProperty);
#endif
            }
        }

        /// <summary>
        /// The rich text content of the message body.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string message
        {
            get => m_RawMessage;
            set
            {
                var changed = m_RawMessage != value;
                m_RawMessage = value;
                RefreshContent();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in contentProperty);
#endif
            }
        }

        /// <summary>
        /// The state of the message. Adds a modifier class using the <c>--{state}</c> pattern.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public ThreadMessageState state
        {
            get => m_State;
            set
            {
                var changed = m_State != value;
                RemoveFromClassList(GetStateUssClassName(m_State));
                m_State = value;
                AddToClassList(GetStateUssClassName(m_State));
                SetEnabled(m_State != ThreadMessageState.Sending);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in stateProperty);
#endif
            }
        }

        /// <summary>
        /// Callback to populate the action menu when the action button is clicked.
        /// When set, the action button is visible; when <c>null</c>, it is hidden.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Action<ThreadMessage, MenuBuilder> makeActionMenuItems
        {
            get => m_MakeActionMenuItems;
            set
            {
                var changed = m_MakeActionMenuItems != value;
                m_MakeActionMenuItems = value;
                m_ActionsButton.EnableInClassList(Styles.hiddenUssClassName, m_MakeActionMenuItems == null);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in makeActionMenuItemsProperty);
#endif
            }
        }

        /// <summary>
        /// The number of likes displayed on the like button.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int likeCount
        {
            get => m_LikeCount;
            set
            {
                var changed = m_LikeCount != value;
                m_LikeCount = value;
                m_LikeButton.label = m_LikeCount > 0 ? m_LikeCount.ToString() : null;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in likeCountProperty);
#endif
            }
        }

        /// <summary>
        /// The number of dislikes displayed on the dislike button.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int dislikeCount
        {
            get => m_DislikeCount;
            set
            {
                var changed = m_DislikeCount != value;
                m_DislikeCount = value;
                m_DislikeButton.label = m_DislikeCount > 0 ? m_DislikeCount.ToString() : null;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in dislikeCountProperty);
#endif
            }
        }

        /// <summary>
        /// Whether the like button is in a selected (highlighted) state.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool isLiked
        {
            get => m_LikeButton.selected;
            set
            {
                var changed = m_LikeButton.selected != value;
                m_LikeButton.selected = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in isLikedProperty);
#endif
            }
        }

        /// <summary>
        /// Whether the dislike button is in a selected (highlighted) state.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool isDisliked
        {
            get => m_DislikeButton.selected;
            set
            {
                var changed = m_DislikeButton.selected != value;
                m_DislikeButton.selected = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in isDislikedProperty);
#endif
            }
        }

        /// <summary>
        /// The avatar background image for the message author.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Background authorAvatar
        {
            get => m_AvatarElement.src;
            set
            {
                var changed = m_AvatarElement.src != value;
                m_AvatarElement.src = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in authorAvatarProperty);
#endif
            }
        }

        /// <summary>
        /// The initials text displayed inside the avatar when no image is set.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string authorInitials
        {
            get => m_AvatarElement.label;
            set
            {
                var changed = m_AvatarElement.label != value;
                m_AvatarElement.label = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in authorInitialsProperty);
#endif
            }
        }

        /// <summary>
        /// The background color of the avatar.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Optional<Color> authorAvatarColor
        {
            get => m_AvatarElement.backgroundColor;
            set
            {
                var changed = !m_AvatarElement.backgroundColor.Equals(value);
                m_AvatarElement.backgroundColor = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in authorAvatarColorProperty);
#endif
            }
        }

        /// <summary>
        /// The list of reactions to display on the message.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public IList<ReactionInfo> reactions
        {
            get => m_Reactions;
            set
            {
                var changed = m_Reactions != value;
                m_Reactions = value;
                m_ReactionBar.reactions = m_Reactions;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in reactionsProperty);
#endif
            }
        }

        void RefreshContent()
        {
            if (string.IsNullOrEmpty(m_RawMessage))
            {
                m_ContentElement.text = m_RawMessage;
                return;
            }

            if (this.GetContext<ThreadContext>() is { mentionProvider: {} mentionProvider })
            {
                // ResolveContent is responsible for sanitizing user content
                // and producing trusted rich text output.
                m_ContentElement.pickingMode = PickingMode.Position;
                m_ContentElement.text = mentionProvider.ConvertToRichText(m_RawMessage);
            }
            else
            {
                // No provider — wrap in <noparse> to prevent rich text injection.
                m_ContentElement.text = "<noparse>" + m_RawMessage + "</noparse>";
            }
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the ThreadMessage.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ThreadMessage, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ThreadMessage"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_AuthorName = new UxmlStringAttributeDescription
            {
                name = "author-name",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Timestamp = new UxmlStringAttributeDescription
            {
                name = "timestamp",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_TimestampTooltip = new UxmlStringAttributeDescription
            {
                name = "timestamp-tooltip",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Content = new UxmlStringAttributeDescription
            {
                name = "message",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<ThreadMessageState> m_State = new UxmlEnumAttributeDescription<ThreadMessageState>
            {
                name = "state",
                defaultValue = ThreadMessageState.Default
            };

            readonly UxmlIntAttributeDescription m_LikeCount = new UxmlIntAttributeDescription
            {
                name = "like-count",
                defaultValue = 0
            };

            readonly UxmlIntAttributeDescription m_DislikeCount = new UxmlIntAttributeDescription
            {
                name = "dislike-count",
                defaultValue = 0
            };

            readonly UxmlBoolAttributeDescription m_IsLiked = new UxmlBoolAttributeDescription
            {
                name = "is-liked",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_IsDisliked = new UxmlBoolAttributeDescription
            {
                name = "is-disliked",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_AuthorInitials = new UxmlStringAttributeDescription
            {
                name = "author-initials",
                defaultValue = null
            };

            readonly UxmlColorAttributeDescription m_AuthorAvatarColor = new UxmlColorAttributeDescription
            {
                name = "author-avatar-color",
                defaultValue = Color.gray
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

                var el = (ThreadMessage)ve;
                el.authorName = m_AuthorName.GetValueFromBag(bag, cc);
                el.timestamp = m_Timestamp.GetValueFromBag(bag, cc);
                el.timestampTooltip = m_TimestampTooltip.GetValueFromBag(bag, cc);
                el.message = m_Content.GetValueFromBag(bag, cc);
                el.state = m_State.GetValueFromBag(bag, cc);
                el.likeCount = m_LikeCount.GetValueFromBag(bag, cc);
                el.dislikeCount = m_DislikeCount.GetValueFromBag(bag, cc);
                el.isLiked = m_IsLiked.GetValueFromBag(bag, cc);
                el.isDisliked = m_IsDisliked.GetValueFromBag(bag, cc);
                el.authorInitials = m_AuthorInitials.GetValueFromBag(bag, cc);
                var avatarColor = Color.gray;
                if (m_AuthorAvatarColor.TryGetValueFromBag(bag, cc, ref avatarColor))
                    el.authorAvatarColor = avatarColor;
            }
        }
#endif
    }
}
