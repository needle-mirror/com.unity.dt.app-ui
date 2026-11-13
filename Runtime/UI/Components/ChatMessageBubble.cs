using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Who authored a chat message.
    /// </summary>
    public enum ChatMessageRole
    {
        /// <summary>
        /// The person talking to the agent. Rendered as a filled, right-aligned bubble.
        /// </summary>
        User = 1,

        /// <summary>
        /// The agent answering. Rendered flush with the conversation, with no bubble.
        /// </summary>
        Assistant
    }

    /// <summary>
    /// One message in a chat conversation.
    /// </summary>
    /// <remarks>
    /// The two roles look deliberately different. A <see cref="ChatMessageRole.User"/> message is a
    /// filled, right-aligned bubble carrying the prompt text and, above it, whatever the message was
    /// sent with. A <see cref="ChatMessageRole.Assistant"/> message has no bubble at all: its content
    /// runs flush with the conversation so long answers do not sit inside a box.
    ///
    /// Four containers are exposed as slots. <see cref="sentWith"/> holds the context chips and file
    /// pills the message was sent with, and only shows inside a user bubble. The content container
    /// (what <c>Add</c> reaches) holds richer assistant content, for instance a
    /// <see cref="ChatActivityTimeline"/> or a <see cref="ChatTodoList"/>. <see cref="footer"/> and
    /// <see cref="actions"/> sit under the message, the latter typically holding a
    /// <see cref="ChatMessageActions"/> row. Every slot collapses to nothing while it is empty, so
    /// there is no state to keep in sync.
    ///
    /// <see cref="trimContent"/> is opt-in on purpose: backends often pad prompt text, but the bubble
    /// must never mutate whitespace-sensitive caller content unless it is asked to. Internal newlines
    /// are always preserved.
    /// </remarks>
    /// <example>
    /// <para>A user message with an action row under it.</para>
    /// <code lang="csharp">
    /// var message = new ChatMessageBubble
    /// {
    ///     role = ChatMessageRole.User,
    ///     content = "  Make the player jump higher  ",
    ///     trimContent = true
    /// };
    /// message.actions.Add(new ChatMessageActions { content = message.content });
    /// parentElement.Add(message);
    /// </code>
    /// <para>An assistant message carrying an activity timeline under its answer.</para>
    /// <code lang="csharp">
    /// var answer = new ChatMessageBubble
    /// {
    ///     role = ChatMessageRole.Assistant,
    ///     content = "I raised the jump force to 8."
    /// };
    /// answer.Add(new ChatActivityTimeline());
    /// parentElement.Add(answer);
    /// </code>
    /// <para>In UXML. Children land in the content container.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatMessageBubble role="User" content="Hello" /&gt;
    /// &lt;appui:ChatMessageBubble role="Assistant" content="Hi there" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatMessageBubble : BaseVisualElement
    {
        internal static readonly BindingId roleProperty = nameof(role);

        internal static readonly BindingId contentProperty = nameof(content);

        internal static readonly BindingId trimContentProperty = nameof(trimContent);

        /// <summary>
        /// The ChatMessageBubble main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-message-bubble";

        /// <summary>
        /// The ChatMessageBubble role styling class.
        /// </summary>
        [EnumName("GetRoleUssClassName", typeof(ChatMessageRole))]
        public const string roleUssClassName = ussClassName + "--";

        /// <summary>
        /// The styling class of the bubble surface.
        /// </summary>
        public const string bubbleUssClassName = ussClassName + "__bubble";

        /// <summary>
        /// The styling class of the "sent with" row.
        /// </summary>
        public const string sentWithUssClassName = ussClassName + "__sent-with";

        /// <summary>
        /// The styling class of the message text.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The styling class of the content container.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The styling class of the footer slot.
        /// </summary>
        public const string footerUssClassName = ussClassName + "__footer";

        /// <summary>
        /// The styling class of the actions slot.
        /// </summary>
        public const string actionsUssClassName = ussClassName + "__actions";

        readonly VisualElement m_Bubble;

        readonly VisualElement m_SentWith;

        readonly LocalizedTextElement m_Content;

        readonly VisualElement m_Container;

        readonly VisualElement m_Footer;

        readonly VisualElement m_Actions;

        ChatMessageRole m_Role;

        string m_RawContent;

        bool m_TrimContent;

        /// <summary>
        /// Richer message content, for instance an activity timeline or a todo list. This is what
        /// <c>Add</c> reaches.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// What the message was sent with: context chips and file pills. Only visible on a user message.
        /// </summary>
        public VisualElement sentWith => m_SentWith;

        /// <summary>
        /// The slot under the message, before the actions.
        /// </summary>
        public VisualElement footer => m_Footer;

        /// <summary>
        /// The per-message action row, under everything else.
        /// </summary>
        public VisualElement actions => m_Actions;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatMessageBubble()
        {
            AddToClassList(ussClassName);

            m_Bubble = new VisualElement { name = bubbleUssClassName, pickingMode = PickingMode.Ignore };
            m_Bubble.AddToClassList(bubbleUssClassName);

            m_SentWith = new VisualElement { name = sentWithUssClassName };
            m_SentWith.AddToClassList(sentWithUssClassName);

            m_Content = new LocalizedTextElement
            {
                name = contentUssClassName,
                pickingMode = PickingMode.Ignore,
                enableRichText = true,
            };
            m_Content.AddToClassList(contentUssClassName);

            m_Container = new VisualElement { name = containerUssClassName };
            m_Container.AddToClassList(containerUssClassName);

            m_Footer = new VisualElement { name = footerUssClassName };
            m_Footer.AddToClassList(footerUssClassName);

            m_Actions = new VisualElement { name = actionsUssClassName };
            m_Actions.AddToClassList(actionsUssClassName);

            m_Bubble.hierarchy.Add(m_SentWith);
            m_Bubble.hierarchy.Add(m_Content);

            hierarchy.Add(m_Bubble);
            hierarchy.Add(m_Container);
            hierarchy.Add(m_Footer);
            hierarchy.Add(m_Actions);

            role = ChatMessageRole.Assistant;
            trimContent = false;
            content = null;
        }

        /// <summary>
        /// Who authored the message. <see cref="ChatMessageRole.Assistant"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ChatMessageRole role
        {
            get => m_Role;
            set
            {
                var changed = m_Role != value;
                RemoveFromClassList(GetRoleUssClassName(m_Role));
                m_Role = value;
                AddToClassList(GetRoleUssClassName(m_Role));

                if (changed)
                    NotifyPropertyChanged(in roleProperty);
            }
        }

        /// <summary>
        /// The message text.
        /// </summary>
        /// <remarks>
        /// Reading this back returns exactly what was set, not the trimmed rendering, so a caller can
        /// hand the same string to a copy action without it having been silently rewritten.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string content
        {
            get => m_RawContent;
            set
            {
                var changed = m_RawContent != value;
                m_RawContent = value;
                RefreshContent();

                if (changed)
                    NotifyPropertyChanged(in contentProperty);
            }
        }

        /// <summary>
        /// Whether leading and trailing whitespace is trimmed from <see cref="content"/> before it is
        /// displayed. False by default. Internal newlines are always preserved.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool trimContent
        {
            get => m_TrimContent;
            set
            {
                var changed = m_TrimContent != value;
                m_TrimContent = value;
                RefreshContent();

                if (changed)
                    NotifyPropertyChanged(in trimContentProperty);
            }
        }

        /// <summary>
        /// The text actually rendered, after the optional trim.
        /// </summary>
        internal string displayedContent => m_Content.text;

        void RefreshContent()
        {
            var text = m_TrimContent ? m_RawContent?.Trim() : m_RawContent;
            m_Content.text = text;
            m_Content.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(text));
        }
    }
}
