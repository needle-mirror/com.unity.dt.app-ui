using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One conversation in a chat sidebar's history list.
    /// </summary>
    /// <remarks>
    /// The row is two targets, not one. The body selects the conversation; the trailing star toggles
    /// its favourite state without selecting it. The star only appears on hover, on keyboard focus, or
    /// when the conversation is already a favourite, so a long list stays quiet until you reach for it.
    ///
    /// The row does not decide what "active" means: <see cref="isActive"/> is set by the list that owns
    /// the selection.
    /// </remarks>
    /// <example>
    /// <para>A favourited row in a history list.</para>
    /// <code lang="csharp">
    /// var item = new ChatConversationListItem
    /// {
    ///     title = "Megaman clone",
    ///     subtitle = "2 hours ago",
    ///     isFavorite = true
    /// };
    /// item.clicked += () => OpenConversation(id);
    /// item.favoriteToggled += () => ToggleFavorite(id);
    /// parentElement.Add(item);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatConversationListItem title="Megaman clone" subtitle="2 hours ago" is-active="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatConversationListItem : BaseVisualElement
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId subtitleProperty = nameof(subtitle);

        internal static readonly BindingId isActiveProperty = nameof(isActive);

        internal static readonly BindingId isFavoriteProperty = nameof(isFavorite);

        internal static readonly BindingId favoritableProperty = nameof(favoritable);

        /// <summary>
        /// The ChatConversationListItem main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-conversation-list-item";

        /// <summary>
        /// The styling class of the selectable body.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>
        /// The styling class of the title.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the subtitle.
        /// </summary>
        public const string subtitleUssClassName = ussClassName + "__subtitle";

        /// <summary>
        /// The styling class of the favourite toggle.
        /// </summary>
        public const string favoriteButtonUssClassName = ussClassName + "__favorite-button";

        /// <summary>
        /// The styling class applied while the conversation is a favourite.
        /// </summary>
        public const string favoriteUssClassName = ussClassName + "--favorite";

        readonly VisualElement m_Body;

        readonly LocalizedTextElement m_Title;

        readonly LocalizedTextElement m_Subtitle;

        readonly IconButton m_FavoriteButton;

        readonly Pressable m_Clickable;

        bool m_IsActive;

        bool m_IsFavorite;

        bool m_Favoritable = true;

        /// <summary>
        /// Raised when the conversation body is activated.
        /// </summary>
        public event Action clicked;

        /// <summary>
        /// Raised when the favourite star is activated. The caller owns the resulting state change.
        /// </summary>
        public event Action favoriteToggled;

        /// <summary>
        /// The body's press handler.
        /// </summary>
        public Pressable clickable => m_Clickable;

        /// <summary>
        /// The trailing favourite toggle.
        /// </summary>
        public IconButton favoriteButton => m_FavoriteButton;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatConversationListItem()
        {
            AddToClassList(ussClassName);

            m_Body = new VisualElement { name = bodyUssClassName, focusable = true };
            m_Body.AddToClassList(bodyUssClassName);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            m_Subtitle = new LocalizedTextElement { name = subtitleUssClassName, pickingMode = PickingMode.Ignore };
            m_Subtitle.AddToClassList(subtitleUssClassName);

            m_Body.hierarchy.Add(m_Title);
            m_Body.hierarchy.Add(m_Subtitle);

            m_FavoriteButton = new IconButton
            {
                name = favoriteButtonUssClassName,
                icon = "star",
                quiet = true,
                size = Size.Sm,
            };
            m_FavoriteButton.AddToClassList(favoriteButtonUssClassName);
            m_FavoriteButton.clicked += () => favoriteToggled?.Invoke();

            hierarchy.Add(m_Body);
            hierarchy.Add(m_FavoriteButton);

            m_Clickable = new Pressable(() => clicked?.Invoke());
            m_Body.AddManipulator(m_Clickable);

            subtitle = null;
            isActive = false;
            isFavorite = false;
            favoritable = true;
        }

        /// <summary>
        /// The conversation's title. Also used as the row's tooltip, since a long title truncates.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title.text;
            set
            {
                var changed = m_Title.text != value;
                m_Title.text = value;
                tooltip = value;

                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// A second line under the title, for instance when the conversation was last touched. Null or
        /// empty hides it entirely rather than leaving a gap.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string subtitle
        {
            get => m_Subtitle.text;
            set
            {
                var changed = m_Subtitle.text != value;
                m_Subtitle.text = value;
                m_Subtitle.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in subtitleProperty);
            }
        }

        /// <summary>
        /// Whether this is the conversation currently open.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isActive
        {
            get => m_IsActive;
            set
            {
                var changed = m_IsActive != value;
                m_IsActive = value;
                EnableInClassList(Styles.selectedUssClassName, m_IsActive);

                if (changed)
                    NotifyPropertyChanged(in isActiveProperty);
            }
        }

        /// <summary>
        /// Whether the conversation is a favourite. A favourite row keeps its star visible.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isFavorite
        {
            get => m_IsFavorite;
            set
            {
                var changed = m_IsFavorite != value;
                m_IsFavorite = value;
                EnableInClassList(favoriteUssClassName, m_IsFavorite);
                m_FavoriteButton.variant = m_IsFavorite ? IconVariant.Fill : IconVariant.Regular;

                if (changed)
                    NotifyPropertyChanged(in isFavoriteProperty);
            }
        }

        /// <summary>
        /// Whether the favourite toggle is offered at all. True by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool favoritable
        {
            get => m_Favoritable;
            set
            {
                var changed = m_Favoritable != value;
                m_Favoritable = value;
                m_FavoriteButton.EnableInClassList(Styles.hiddenUssClassName, !m_Favoritable);

                if (changed)
                    NotifyPropertyChanged(in favoritableProperty);
            }
        }
    }
}
