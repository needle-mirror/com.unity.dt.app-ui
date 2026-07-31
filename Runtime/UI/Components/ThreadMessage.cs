using System;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadMessage UI element. Displays a single message within a thread.
    /// </summary>
    [UxmlElement]
    public partial class ThreadMessage : BaseVisualElement
    {
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
        [CreateProperty]
        [UxmlAttribute]
        [Header("Thread Message")]
        public string authorName
        {
            get => m_AuthorNameElement.text;
            set
            {
                var changed = m_AuthorNameElement.text != value;
                m_AuthorNameElement.text = value;
                if (changed)
                    NotifyPropertyChanged(in authorNameProperty);
            }
        }

        /// <summary>
        /// The timestamp text displayed in the message header.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string timestamp
        {
            get => m_TimestampElement.text;
            set
            {
                var changed = m_TimestampElement.text != value;
                m_TimestampElement.text = value;
                if (changed)
                    NotifyPropertyChanged(in timestampProperty);
            }
        }

        /// <summary>
        /// The tooltip displayed on the timestamp element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string timestampTooltip
        {
            get => m_TimestampElement.tooltip;
            set
            {
                var changed = m_TimestampElement.tooltip != value;
                m_TimestampElement.tooltip = value;
                if (changed)
                    NotifyPropertyChanged(in timestampTooltipProperty);
            }
        }

        /// <summary>
        /// The rich text content of the message body.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string message
        {
            get => m_RawMessage;
            set
            {
                var changed = m_RawMessage != value;
                m_RawMessage = value;
                RefreshContent();
                if (changed)
                    NotifyPropertyChanged(in contentProperty);
            }
        }

        /// <summary>
        /// The state of the message. Adds a modifier class using the <c>--{state}</c> pattern.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
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
                if (changed)
                    NotifyPropertyChanged(in stateProperty);
            }
        }

        /// <summary>
        /// Callback to populate the action menu when the action button is clicked.
        /// When set, the action button is visible; when <c>null</c>, it is hidden.
        /// </summary>
        [CreateProperty]
        public Action<ThreadMessage, MenuBuilder> makeActionMenuItems
        {
            get => m_MakeActionMenuItems;
            set
            {
                var changed = m_MakeActionMenuItems != value;
                m_MakeActionMenuItems = value;
                m_ActionsButton.EnableInClassList(Styles.hiddenUssClassName, m_MakeActionMenuItems == null);
                if (changed)
                    NotifyPropertyChanged(in makeActionMenuItemsProperty);
            }
        }

        /// <summary>
        /// The number of likes displayed on the like button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int likeCount
        {
            get => m_LikeCount;
            set
            {
                var changed = m_LikeCount != value;
                m_LikeCount = value;
                m_LikeButton.label = m_LikeCount > 0 ? m_LikeCount.ToString() : null;
                if (changed)
                    NotifyPropertyChanged(in likeCountProperty);
            }
        }

        /// <summary>
        /// The number of dislikes displayed on the dislike button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int dislikeCount
        {
            get => m_DislikeCount;
            set
            {
                var changed = m_DislikeCount != value;
                m_DislikeCount = value;
                m_DislikeButton.label = m_DislikeCount > 0 ? m_DislikeCount.ToString() : null;
                if (changed)
                    NotifyPropertyChanged(in dislikeCountProperty);
            }
        }

        /// <summary>
        /// Whether the like button is in a selected (highlighted) state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isLiked
        {
            get => m_LikeButton.selected;
            set
            {
                var changed = m_LikeButton.selected != value;
                m_LikeButton.selected = value;
                if (changed)
                    NotifyPropertyChanged(in isLikedProperty);
            }
        }

        /// <summary>
        /// Whether the dislike button is in a selected (highlighted) state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isDisliked
        {
            get => m_DislikeButton.selected;
            set
            {
                var changed = m_DislikeButton.selected != value;
                m_DislikeButton.selected = value;
                if (changed)
                    NotifyPropertyChanged(in isDislikedProperty);
            }
        }

        /// <summary>
        /// The avatar background image for the message author.
        /// </summary>
        [CreateProperty]
        public Background authorAvatar
        {
            get => m_AvatarElement.src;
            set
            {
                var changed = m_AvatarElement.src != value;
                m_AvatarElement.src = value;
                if (changed)
                    NotifyPropertyChanged(in authorAvatarProperty);
            }
        }

        /// <summary>
        /// The initials text displayed inside the avatar when no image is set.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string authorInitials
        {
            get => m_AvatarElement.label;
            set
            {
                var changed = m_AvatarElement.label != value;
                m_AvatarElement.label = value;
                if (changed)
                    NotifyPropertyChanged(in authorInitialsProperty);
            }
        }

        /// <summary>
        /// The background color of the avatar.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> authorAvatarColor
        {
            get => m_AvatarElement.backgroundColor;
            set
            {
                var changed = !m_AvatarElement.backgroundColor.Equals(value);
                m_AvatarElement.backgroundColor = value;
                if (changed)
                    NotifyPropertyChanged(in authorAvatarColorProperty);
            }
        }

        /// <summary>
        /// The list of reactions to display on the message.
        /// </summary>
        [CreateProperty]
        public IList<ReactionInfo> reactions
        {
            get => m_Reactions;
            set
            {
                var changed = m_Reactions != value;
                m_Reactions = value;
                m_ReactionBar.reactions = m_Reactions;
                if (changed)
                    NotifyPropertyChanged(in reactionsProperty);
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

    }
}
