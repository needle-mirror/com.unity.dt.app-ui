using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Thread UI element. Main container for a threaded conversation.
    /// </summary>
    [UxmlElement]
    public partial class Thread : BaseVisualElement
    {
        internal static readonly BindingId enableResolutionProperty = nameof(enableResolution);

        internal static readonly BindingId isResolvedProperty = nameof(isResolved);

        internal static readonly BindingId enableReactionsProperty = nameof(enableReactions);

        internal static readonly BindingId enableLikesProperty = nameof(enableLikes);

        internal static readonly BindingId enableDislikesProperty = nameof(enableDislikes);

        internal static readonly BindingId replyCountProperty = nameof(replyCount);

        internal static readonly BindingId participantsProperty = nameof(participants);

        internal static readonly BindingId bindParticipantProperty = nameof(bindParticipant);

        internal static readonly BindingId makeActionMenuItemsProperty = nameof(makeActionMenuItems);

        /// <summary>
        /// The Thread main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread";

        /// <summary>
        /// The Thread header styling class.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The Thread actions button styling class.
        /// </summary>
        public const string actionsUssClassName = ussClassName + "__actions";

        /// <summary>
        /// The Thread resolve button styling class.
        /// </summary>
        public const string resolveBtnUssClassName = ussClassName + "__resolve-btn";

        /// <summary>
        /// The Thread participants styling class.
        /// </summary>
        public const string participantsUssClassName = ussClassName + "__participants";

        /// <summary>
        /// The Thread flex spacer styling class,
        /// applied to the spacer element between participants
        /// and resolve button in the header to push the resolve button to the right.
        /// </summary>
        public const string flexSpacerUssClassName = ussClassName + "__flex-spacer";

        /// <summary>
        /// The Thread replies container styling class.
        /// </summary>
        public const string repliesUssClassName = ussClassName + "__replies";

        /// <summary>
        /// The Thread resolved modifier styling class.
        /// </summary>
        public const string resolvedUssClassName = ussClassName + "--resolved";

        /// <summary>
        /// The Thread with replies modifier styling class, applied when there is at least 1 reply to show on the collapse button label.
        /// </summary>
        public const string withRepliesUssClassName = ussClassName + "--with-replies";

        /// <summary>
        /// The ThreadMessage actions-open modifier styling class.
        /// </summary>
        public const string actionsOpenUssClassName = ussClassName + "--actions-open";

        readonly ActionButton m_ResolveButton;

        readonly  ActionButton m_ActionsButton;

        readonly AvatarGroup m_ParticipantsGroup;

        readonly RepliesView m_RepliesContainer;

        bool m_EnableResolution;

        bool m_IsResolved;

        bool m_EnableReactions;

        bool m_EnableLikes;

        bool m_EnableDislikes;

        bool m_RepliesCollapsed;

        int m_ReplyCount;

        Action<Thread, MenuBuilder> m_MakeActionMenuItems;

        /// <summary>
        /// Event invoked when the resolve button is toggled.
        /// </summary>
        public event Action<bool> resolveToggled;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Thread()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = false;

            // Header
            var header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore  };
            header.AddToClassList(headerUssClassName);

            m_ParticipantsGroup = new AvatarGroup { name = participantsUssClassName, max = 3, size = Size.S };
            m_ParticipantsGroup.AddToClassList(participantsUssClassName);

            header.hierarchy.Add(m_ParticipantsGroup);

            var spacer = new VisualElement { pickingMode = PickingMode.Ignore };
            spacer.AddToClassList(flexSpacerUssClassName);
            header.hierarchy.Add(spacer);

            m_ActionsButton = new ActionButton
            {
                name = actionsUssClassName,
                quiet = true, icon = "dots-three",
                iconVariant = IconVariant.Bold,
                size = Size.M
            };
            m_ActionsButton.AddToClassList(actionsUssClassName);
            m_ActionsButton.clicked += OnActionButtonClicked;
            header.hierarchy.Add(m_ActionsButton);

            m_ResolveButton = new ActionButton { name = resolveBtnUssClassName, quiet = true, icon = "check-circle", size = Size.M };
            m_ResolveButton.AddToClassList(resolveBtnUssClassName);
            m_ResolveButton.clicked += OnResolveClicked;

            header.hierarchy.Add(m_ResolveButton);

            // Replies — use a non-virtualized ScrollView wrapper to keep popover
            // layout stable. ListView with DynamicHeight kept relayouting in this
            // context for no apparent reason.
            m_RepliesContainer = new RepliesView { name = repliesUssClassName };
            m_RepliesContainer.AddToClassList(repliesUssClassName);

            hierarchy.Add(header);
            hierarchy.Add(m_RepliesContainer);

            // Defaults
            enableResolution = false;
            isResolved = false;
            enableReactions = true;
            enableLikes = true;
            replyCount = 0;
            makeActionMenuItems = null;
        }

        void OnActionButtonClicked()
        {
            var menu = MenuBuilder.Build(m_ActionsButton, new Menu());
            makeActionMenuItems?.Invoke(this, menu);
            AddToClassList(actionsOpenUssClassName);
            menu.dismissed += (_, _) => RemoveFromClassList(actionsOpenUssClassName);
            menu.Show();
        }

        void OnResolveClicked()
        {
            isResolved = !isResolved;
            resolveToggled?.Invoke(isResolved);
        }

        /// <summary>
        /// The replies container where reply messages are added. The API mirrors
        /// the subset of <see cref="ListView"/> previously used here so existing
        /// callers continue to compile unchanged.
        /// </summary>
        public RepliesView repliesListView => m_RepliesContainer;

        /// <summary>
        /// The content container of the Thread does not exist as replies are added directly to the <see cref="repliesListView"/>.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Callback to populate the action menu when the action button is clicked.
        /// When set, the action button is visible; when <c>null</c>, it is hidden.
        /// </summary>
        [CreateProperty]
        public Action<Thread, MenuBuilder> makeActionMenuItems
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
        /// Whether the resolve button is visible.
        /// </summary>
        [Tooltip("Whether the resolve button is visible.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Thread")]
        public bool enableResolution
        {
            get => m_EnableResolution;
            set
            {
                var changed = m_EnableResolution != value;
                m_EnableResolution = value;
                m_ResolveButton.EnableInClassList(Styles.hiddenUssClassName, !value);
                if (changed)
                {
                    NotifyPropertyChanged(in enableResolutionProperty);
                }
            }
        }

        /// <summary>
        /// The resolution state of the thread.
        /// </summary>
        [Tooltip("The resolution state of the thread.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool isResolved
        {
            get => m_IsResolved;
            set
            {
                var changed = m_IsResolved != value;
                m_IsResolved = value;
                EnableInClassList(resolvedUssClassName, value);
                m_ResolveButton.selected = value;
                if (changed)
                    NotifyPropertyChanged(in isResolvedProperty);
            }
        }

        /// <summary>
        /// Whether reactions are enabled. Propagated via <see cref="ThreadContext"/>.
        /// </summary>
        [Tooltip("Whether reactions are enabled.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool enableReactions
        {
            get => m_EnableReactions;
            set
            {
                var changed = m_EnableReactions != value;
                m_EnableReactions = value;
                if (changed)
                {
                    NotifyPropertyChanged(in enableReactionsProperty);
                }
            }
        }

        /// <summary>
        /// Whether likes are enabled. Propagated via <see cref="ThreadContext"/>.
        /// </summary>
        [Tooltip("Whether likes/dislikes are enabled.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool enableLikes
        {
            get => m_EnableLikes;
            set
            {
                var changed = m_EnableLikes != value;
                m_EnableLikes = value;
                if (changed)
                {
                    NotifyPropertyChanged(in enableLikesProperty);
                }
            }
        }

        /// <summary>
        /// Whether dislikes are enabled. Propagated via <see cref="ThreadContext"/>.
        /// </summary>
        [Tooltip("Whether likes/dislikes are enabled.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool enableDislikes
        {
            get => m_EnableDislikes;
            set
            {
                var changed = m_EnableDislikes != value;
                m_EnableDislikes = value;
                if (changed)
                {
                    NotifyPropertyChanged(in enableDislikesProperty);
                }
            }
        }

        /// <summary>
        /// The number of replies displayed on the collapse button label.
        /// </summary>
        [Tooltip("The number of replies displayed on the collapse button.")]
        [CreateProperty]
        [UxmlAttribute]
        public int replyCount
        {
            get => m_ReplyCount;
            set
            {
                var changed = m_ReplyCount != value;
                m_ReplyCount = value;
                if (changed)
                    NotifyPropertyChanged(in replyCountProperty);
            }
        }

        /// <summary>
        /// The participants data source for the <see cref="AvatarGroup"/>.
        /// </summary>
        [CreateProperty]
        public IList participants
        {
            get => m_ParticipantsGroup.sourceItems;
            set
            {
                var changed = m_ParticipantsGroup.sourceItems != value;
                m_ParticipantsGroup.sourceItems = value;
                if (changed)
                    NotifyPropertyChanged(in participantsProperty);
            }
        }

        /// <summary>
        /// The bind callback for participant avatars.
        /// </summary>
        [CreateProperty]
        public Action<Avatar, int> bindParticipant
        {
            get => m_ParticipantsGroup.bindItem;
            set
            {
                var changed = m_ParticipantsGroup.bindItem != value;
                m_ParticipantsGroup.bindItem = value;
                if (changed)
                    NotifyPropertyChanged(in bindParticipantProperty);
            }
        }


    }
}
