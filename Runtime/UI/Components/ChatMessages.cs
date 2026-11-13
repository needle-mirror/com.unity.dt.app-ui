using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Who authored a message in a conversation.
    /// </summary>
    /// <remarks>
    /// Deliberately wider than <see cref="ChatMessageRole"/>. A message can come from the system, but
    /// a system message is never drawn as a bubble — <see cref="ChatMessages"/> renders it as a
    /// destructive banner — so the bubble's own role enum stays two-valued.
    /// </remarks>
    public enum ChatRole
    {
        /// <summary>
        /// The person talking to the agent.
        /// </summary>
        User = 1,

        /// <summary>
        /// The agent answering.
        /// </summary>
        Assistant,

        /// <summary>
        /// The application itself, reporting something out of band such as a failure.
        /// </summary>
        System
    }

    /// <summary>
    /// What one piece of a multi-part message carries.
    /// </summary>
    public enum ChatMessagePartKind
    {
        /// <summary>
        /// Answer text meant for the reader.
        /// </summary>
        Text = 0,

        /// <summary>
        /// A stretch of the agent reasoning, shown in the activity timeline.
        /// </summary>
        Thinking,

        /// <summary>
        /// The agent invoking a tool.
        /// </summary>
        ToolCall,

        /// <summary>
        /// What a tool returned. Paired onto its <see cref="ToolCall"/> by
        /// <see cref="ChatMessagePart.toolCallId"/> rather than shown on its own.
        /// </summary>
        ToolResult
    }

    /// <summary>
    /// One piece of a multi-part message.
    /// </summary>
    /// <remarks>
    /// Use the factory methods rather than the constructor: they name the fields each kind actually
    /// uses, and leave the rest at their neutral values.
    /// </remarks>
    public readonly struct ChatMessagePart
    {
        /// <summary>
        /// What this piece carries.
        /// </summary>
        public ChatMessagePartKind kind { get; }

        /// <summary>
        /// The piece's text: the answer for a <see cref="ChatMessagePartKind.Text"/> part, the reasoning
        /// for a <see cref="ChatMessagePartKind.Thinking"/> one, the tool's header line for a
        /// <see cref="ChatMessagePartKind.ToolCall"/>.
        /// </summary>
        public string text { get; }

        /// <summary>
        /// Identifier joining a <see cref="ChatMessagePartKind.ToolCall"/> to its
        /// <see cref="ChatMessagePartKind.ToolResult"/>. Null on the other kinds.
        /// </summary>
        public string toolCallId { get; }

        /// <summary>
        /// The tool's name. Null on the other kinds.
        /// </summary>
        public string toolName { get; }

        /// <summary>
        /// What a tool returned, untyped. Only a <see cref="ChatMessagePartKind.ToolResult"/> carries one.
        /// </summary>
        public object result { get; }

        /// <summary>
        /// Whether a tool call is still waiting on its result. A call whose result appears later in the
        /// same message is reported as finished regardless of what this said.
        /// </summary>
        public bool pending { get; }

        /// <summary>
        /// Wall-clock duration of the piece, in milliseconds, or <see cref="ChatMessage.unknownDuration"/>.
        /// </summary>
        public int elapsedMs { get; }

        /// <summary>
        /// Constructs a part. Prefer the factory methods.
        /// </summary>
        /// <param name="kind"> What the piece carries. </param>
        /// <param name="text"> The piece's text. </param>
        /// <param name="toolCallId"> Identifier joining a call to its result. </param>
        /// <param name="toolName"> The tool's name. </param>
        /// <param name="result"> What the tool returned. </param>
        /// <param name="pending"> Whether the call is still running. </param>
        /// <param name="elapsedMs"> Duration in milliseconds, or a negative value if unmeasured. </param>
        public ChatMessagePart(
            ChatMessagePartKind kind,
            string text = null,
            string toolCallId = null,
            string toolName = null,
            object result = null,
            bool pending = false,
            int elapsedMs = ChatMessage.unknownDuration)
        {
            this.kind = kind;
            this.text = text;
            this.toolCallId = toolCallId;
            this.toolName = toolName;
            this.result = result;
            this.pending = pending;
            this.elapsedMs = elapsedMs;
        }

        /// <summary>
        /// Answer text meant for the reader.
        /// </summary>
        /// <param name="text"> The answer text. </param>
        /// <returns> The part. </returns>
        public static ChatMessagePart Text(string text) =>
            new ChatMessagePart(ChatMessagePartKind.Text, text);

        /// <summary>
        /// A stretch of the agent reasoning.
        /// </summary>
        /// <param name="thinking"> The reasoning text. </param>
        /// <param name="elapsedMs"> Duration in milliseconds, or a negative value if unmeasured. </param>
        /// <returns> The part. </returns>
        public static ChatMessagePart Thinking(string thinking, int elapsedMs = ChatMessage.unknownDuration) =>
            new ChatMessagePart(ChatMessagePartKind.Thinking, thinking, elapsedMs: elapsedMs);

        /// <summary>
        /// The agent invoking a tool.
        /// </summary>
        /// <param name="toolCallId"> Identifier the matching result will carry. </param>
        /// <param name="toolName"> The tool's name. </param>
        /// <param name="text"> The header line shown in the timeline. Falls back to the tool's name. </param>
        /// <param name="pending"> Whether the call is still running. </param>
        /// <param name="elapsedMs"> Duration in milliseconds, or a negative value if unmeasured. </param>
        /// <returns> The part. </returns>
        public static ChatMessagePart ToolCall(
            string toolCallId,
            string toolName,
            string text = null,
            bool pending = false,
            int elapsedMs = ChatMessage.unknownDuration) =>
            new ChatMessagePart(ChatMessagePartKind.ToolCall, text ?? toolName, toolCallId, toolName,
                pending: pending, elapsedMs: elapsedMs);

        /// <summary>
        /// What a tool returned.
        /// </summary>
        /// <param name="toolCallId"> Identifier of the call this answers. </param>
        /// <param name="toolName"> The tool's name. </param>
        /// <param name="result"> The returned payload. </param>
        /// <returns> The part. </returns>
        public static ChatMessagePart ToolResult(string toolCallId, string toolName, object result) =>
            new ChatMessagePart(ChatMessagePartKind.ToolResult, toolCallId: toolCallId, toolName: toolName,
                result: result);
    }

    /// <summary>
    /// One message in a conversation, as data. Not a <see cref="VisualElement"/> — hand a sequence of
    /// these to <see cref="ChatMessages.messages"/> and it builds the tree.
    /// </summary>
    /// <remarks>
    /// A message carries either plain <see cref="content"/> or a list of <see cref="parts"/>. When
    /// <see cref="parts"/> is set it wins, and <see cref="content"/> is ignored.
    /// </remarks>
    public class ChatMessage
    {
        /// <summary>
        /// The value a duration takes when it was never measured. Kept out of every elapsed display.
        /// </summary>
        public const int unknownDuration = -1;

        /// <summary>
        /// Stable identifier. Used to decide which message is streaming.
        /// </summary>
        public string id;

        /// <summary>
        /// Who authored the message.
        /// </summary>
        public ChatRole role = ChatRole.Assistant;

        /// <summary>
        /// The message's plain text, used when <see cref="parts"/> is null.
        /// </summary>
        public string content;

        /// <summary>
        /// The message's pieces. Null for a plain-text message.
        /// </summary>
        public IEnumerable<ChatMessagePart> parts;

        /// <summary>
        /// Labels of the files the message was sent with. Shown as chips inside a user bubble.
        /// </summary>
        public IEnumerable<string> attachments;

        /// <summary>
        /// Labels of the context entries the message was sent with. Shown as chips inside a user bubble.
        /// </summary>
        public IEnumerable<string> contextItems;

        /// <summary>
        /// How long the answer took, in milliseconds, as reported by the backend. This always wins over
        /// the gap derived from <see cref="createdAt"/>.
        /// </summary>
        public int durationMs = unknownDuration;

        /// <summary>
        /// When the backend recorded the message. Two of these bracket a turn, which is where the
        /// elapsed-to-answer figure comes from when <see cref="durationMs"/> is absent.
        /// </summary>
        public DateTime? createdAt;
    }

    /// <summary>
    /// What <see cref="ChatMessages.renderStandalonePart"/> is told about the part it is building.
    /// </summary>
    public readonly struct ChatStandalonePartContext
    {
        /// <summary>
        /// The part pulled out of the bubble.
        /// </summary>
        public ChatMessagePart part { get; }

        /// <summary>
        /// The tool result matching <see cref="part"/>, when it is a tool call that has one.
        /// </summary>
        public object result { get; }

        /// <summary>
        /// Whether the message this part belongs to is still streaming.
        /// </summary>
        public bool isStreaming { get; }

        /// <summary>
        /// Constructs the context.
        /// </summary>
        /// <param name="part"> The part being rendered. </param>
        /// <param name="result"> The matching tool result, or null. </param>
        /// <param name="isStreaming"> Whether the message is still streaming. </param>
        public ChatStandalonePartContext(ChatMessagePart part, object result, bool isStreaming)
        {
            this.part = part;
            this.result = result;
            this.isStreaming = isStreaming;
        }
    }

    /// <summary>
    /// What <see cref="ChatMessages.renderMessageMeta"/> is told about an assistant message.
    /// </summary>
    public readonly struct ChatMessageMetaContext
    {
        /// <summary>
        /// The assistant message being rendered.
        /// </summary>
        public ChatMessage message { get; }

        /// <summary>
        /// The nearest user message before it, or null if the answer was unprompted.
        /// </summary>
        public ChatMessage precedingUserMessage { get; }

        /// <summary>
        /// Time from that prompt to this reply, in milliseconds, or
        /// <see cref="ChatMessage.unknownDuration"/>.
        /// </summary>
        public int answerElapsedMs { get; }

        /// <summary>
        /// Constructs the context.
        /// </summary>
        /// <param name="message"> The assistant message. </param>
        /// <param name="precedingUserMessage"> The prompt that started the turn. </param>
        /// <param name="answerElapsedMs"> Elapsed time to the answer. </param>
        public ChatMessageMetaContext(ChatMessage message, ChatMessage precedingUserMessage, int answerElapsedMs)
        {
            this.message = message;
            this.precedingUserMessage = precedingUserMessage;
            this.answerElapsedMs = answerElapsedMs;
        }
    }

    /// <summary>
    /// What <see cref="ChatMessages.renderActivity"/> is told about the activity group it is building.
    /// </summary>
    public readonly struct ChatActivityRenderContext
    {
        /// <summary>
        /// The assistant message the group belongs to.
        /// </summary>
        public ChatMessage message { get; }

        /// <summary>
        /// The group's rows as the default timeline would draw them: thinking merged per
        /// <see cref="ChatMessages.mergeThinking"/>, and a call no longer pending once its result arrived.
        /// </summary>
        public IReadOnlyList<ChatActivityPart> parts { get; }

        /// <summary>
        /// The message parts the group was folded from, in order, each tool call followed by its
        /// <see cref="ChatMessagePartKind.ToolResult"/> once one has arrived. Carries the call id
        /// and result payload that <see cref="parts"/> leaves out.
        /// </summary>
        public IReadOnlyList<ChatMessagePart> messageParts { get; }

        /// <summary>
        /// Which of the message's activity groups this is, counting from zero.
        /// </summary>
        public int groupIndex { get; }

        /// <summary>
        /// Whether the group is the message's last and the message is still streaming.
        /// </summary>
        public bool isStreaming { get; }

        /// <summary>
        /// Whether a text or standalone block follows the group.
        /// </summary>
        public bool isFollowedByText { get; }

        /// <summary>
        /// When the group's work began, if known.
        /// </summary>
        public DateTime? startTime { get; }

        /// <summary>
        /// Creates a context.
        /// </summary>
        /// <param name="message">The assistant message the group belongs to.</param>
        /// <param name="parts">The group's timeline rows.</param>
        /// <param name="messageParts">The message parts the group was folded from.</param>
        /// <param name="groupIndex">The group's place among the message's activity groups.</param>
        /// <param name="isStreaming">Whether the group is still running.</param>
        /// <param name="isFollowedByText">Whether a block follows the group.</param>
        /// <param name="startTime">When the group's work began.</param>
        public ChatActivityRenderContext(ChatMessage message, IReadOnlyList<ChatActivityPart> parts,
            IReadOnlyList<ChatMessagePart> messageParts, int groupIndex, bool isStreaming,
            bool isFollowedByText, DateTime? startTime)
        {
            this.message = message;
            this.parts = parts;
            this.messageParts = messageParts;
            this.groupIndex = groupIndex;
            this.isStreaming = isStreaming;
            this.isFollowedByText = isFollowedByText;
            this.startTime = startTime;
        }
    }

    /// <summary>
    /// What <see cref="ChatMessages.renderActions"/> is told about the message it is building a row for.
    /// </summary>
    public readonly struct ChatMessageActionsContext
    {
        /// <summary>
        /// The message the row belongs to.
        /// </summary>
        public ChatMessage message { get; }

        /// <summary>
        /// Who authored it. Never <see cref="ChatRole.System"/> — system messages carry no action row.
        /// </summary>
        public ChatRole role { get; }

        /// <summary>
        /// Constructs the context.
        /// </summary>
        /// <param name="message"> The message. </param>
        /// <param name="role"> Who authored it. </param>
        public ChatMessageActionsContext(ChatMessage message, ChatRole role)
        {
            this.message = message;
            this.role = role;
        }
    }

    /// <summary>
    /// The seam through which an application renders its own tools.
    /// </summary>
    /// <remarks>
    /// <see cref="ChatMessages"/> knows how to lay a conversation out, but not what any particular tool
    /// call should look like — that is a product decision, and the registry keeps it in a separate
    /// renderer registry. Implement this and hand it to <see cref="ChatMessages.toolRenderer"/> to place
    /// bespoke content under the activity timeline. Returning null falls back to the timeline row alone,
    /// which is what happens when no renderer is set.
    /// </remarks>
    /// <example>
    /// <code lang="csharp">
    /// class DiffToolRenderer : IChatToolRenderer
    /// {
    ///     public VisualElement CreateToolContent(ChatMessagePart call, object result)
    ///     {
    ///         if (call.toolName != "apply_patch")
    ///             return null;
    ///         return new CodeBlock { language = "diff", code = result as string };
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IChatToolRenderer
    {
        /// <summary>
        /// Builds the content shown for one tool call, or returns null to render nothing extra.
        /// </summary>
        /// <param name="call"> The <see cref="ChatMessagePartKind.ToolCall"/> part. </param>
        /// <param name="result"> What the matching result carried, or null while the call is pending. </param>
        /// <returns> The element to insert, or null. </returns>
        VisualElement CreateToolContent(ChatMessagePart call, object result);
    }

    /// <summary>
    /// A whole conversation: the scrolling list of messages, plus the empty, loading and
    /// budget-exhausted states that stand in for it.
    /// </summary>
    /// <remarks>
    /// This is the container the rest of the chat display suite composes into. It owns no message
    /// rendering of its own beyond layout: a user message becomes a <see cref="ChatMessageBubble"/> with
    /// its chips, an assistant message becomes a bubble whose reasoning and tool calls are collapsed into
    /// <see cref="ChatActivityTimeline"/> runs, and a system message becomes a destructive banner rather
    /// than a bubble at all.
    ///
    /// Three things are worth knowing before wiring one up.
    ///
    /// **Turns are resolved in one forward pass.** Each assistant message is told which user message
    /// prompted it and how long the answer took. Scanning backwards from every message would make a long
    /// conversation cost O(n²) to build; carrying the last user message forward is the same answer.
    /// A <see cref="ChatMessage.durationMs"/> reported by the backend always beats the gap derived from
    /// the two <see cref="ChatMessage.createdAt"/> stamps.
    ///
    /// **Which message streams has two sources.** <see cref="streamingMessageId"/> names one outright;
    /// with no id set, <see cref="isLoading"/> streams the last message instead. The explicit id wins,
    /// because a backend that inserts a message after the streaming one would otherwise move the spinner.
    ///
    /// **Tool rendering is a seam, not a feature.** What a tool call looks like is a product decision, so
    /// it is delegated to <see cref="IChatToolRenderer"/> the way a custom gradient picker is delegated
    /// to <c>ICustomColorGradientFieldPicker</c>. Without one, tool calls are still listed in the timeline.
    ///
    /// Setting any property rebuilds the tree, so a caller pushing a stream of tokens should re-set
    /// <see cref="messages"/> rather than mutating a <see cref="ChatMessage"/> in place.
    /// </remarks>
    /// <example>
    /// <para>A short conversation with a tool call.</para>
    /// <code lang="csharp">
    /// var conversation = new ChatMessages
    /// {
    ///     messages = new[]
    ///     {
    ///         new ChatMessage { id = "u1", role = ChatRole.User, content = "Make the player jump higher" },
    ///         new ChatMessage
    ///         {
    ///             id = "a1",
    ///             role = ChatRole.Assistant,
    ///             durationMs = 5200,
    ///             parts = new[]
    ///             {
    ///                 ChatMessagePart.Thinking("The jump force lives on the controller."),
    ///                 ChatMessagePart.ToolCall("t1", "read_file", "Read PlayerController.cs"),
    ///                 ChatMessagePart.ToolResult("t1", "read_file", "…"),
    ///                 ChatMessagePart.Text("I raised the jump force to 8."),
    ///             },
    ///         },
    ///     },
    /// };
    /// conversation.retryRequested += message =&gt; Resend(message);
    /// parentElement.Add(conversation);
    /// </code>
    /// <para>In UXML. Messages are supplied from C#, the copy and flags are attributes.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatMessages empty-title="How can I help you?" merge-thinking="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatMessages : BaseVisualElement
    {
        internal static readonly BindingId isLoadingProperty = nameof(isLoading);

        internal static readonly BindingId streamingMessageIdProperty = nameof(streamingMessageId);

        internal static readonly BindingId isLoadingConversationProperty = nameof(isLoadingConversation);

        internal static readonly BindingId budgetExhaustedProperty = nameof(budgetExhausted);

        internal static readonly BindingId emptyTitleProperty = nameof(emptyTitle);

        internal static readonly BindingId emptyDescriptionProperty = nameof(emptyDescription);

        internal static readonly BindingId exhaustedTitleProperty = nameof(exhaustedTitle);

        internal static readonly BindingId exhaustedDescriptionProperty = nameof(exhaustedDescription);

        internal static readonly BindingId budgetBannerProperty = nameof(budgetBanner);

        internal static readonly BindingId streamingStatsProperty = nameof(streamingStats);

        internal static readonly BindingId showCompletedIndicatorProperty = nameof(showCompletedIndicator);

        internal static readonly BindingId mergeThinkingProperty = nameof(mergeThinking);

        internal static readonly BindingId showStreamingIndicatorProperty = nameof(showStreamingIndicator);

        internal static readonly BindingId loadingLabelProperty = nameof(loadingLabel);

        internal static readonly BindingId activityOpenProperty = nameof(activityOpen);

        internal static readonly BindingId activityHeaderPlacementProperty = nameof(activityHeaderPlacement);

        internal static readonly BindingId activityFoldRowsByKindProperty = nameof(activityFoldRowsByKind);

        internal static readonly BindingId activityPinEntriesProperty = nameof(activityPinEntries);

        internal static readonly BindingId activityMaxPinnedShareProperty = nameof(activityMaxPinnedShare);

        /// <summary>
        /// The ChatMessages main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-messages";

        /// <summary>
        /// The styling class toggled while the conversation is empty.
        /// </summary>
        public const string emptyVariantUssClassName = ussClassName + "--empty";

        /// <summary>
        /// The styling class toggled while the conversation is still loading.
        /// </summary>
        public const string loadingVariantUssClassName = ussClassName + "--loading";

        /// <summary>
        /// The styling class of the scrolling area.
        /// </summary>
        public const string scrollUssClassName = ussClassName + "__scroll";

        /// <summary>
        /// The styling class of the centred column the messages live in.
        /// </summary>
        public const string listUssClassName = ussClassName + "__list";

        /// <summary>
        /// The styling class of the empty-state block.
        /// </summary>
        public const string emptyUssClassName = ussClassName + "__empty";

        /// <summary>
        /// The styling class of the warning glyph fronting the budget-exhausted empty state.
        /// </summary>
        public const string emptyIconUssClassName = ussClassName + "__empty-icon";

        /// <summary>
        /// The styling class of the empty-state title.
        /// </summary>
        public const string emptyTitleUssClassName = ussClassName + "__empty-title";

        /// <summary>
        /// The styling class of the empty-state description.
        /// </summary>
        public const string emptyDescriptionUssClassName = ussClassName + "__empty-description";

        /// <summary>
        /// The styling class of the conversation-loading block.
        /// </summary>
        public const string loadingUssClassName = ussClassName + "__loading";

        /// <summary>
        /// The styling class of the conversation-loading spinner.
        /// </summary>
        public const string loadingSpinnerUssClassName = ussClassName + "__loading-spinner";

        /// <summary>
        /// The styling class of the conversation-loading label.
        /// </summary>
        public const string loadingLabelUssClassName = ussClassName + "__loading-label";

        /// <summary>
        /// The styling class of a destructive banner: a system message, or the budget notice.
        /// </summary>
        public const string bannerUssClassName = ussClassName + "__banner";

        /// <summary>
        /// The styling class of a banner's warning glyph.
        /// </summary>
        public const string bannerIconUssClassName = ussClassName + "__banner-icon";

        /// <summary>
        /// The styling class of a banner's text.
        /// </summary>
        public const string bannerLabelUssClassName = ussClassName + "__banner-label";

        /// <summary>
        /// The styling class of the meta line above an assistant message's action row.
        /// </summary>
        public const string metaUssClassName = ussClassName + "__meta";

        /// <summary>
        /// The styling class of a chip in a user message's sent-with row.
        /// </summary>
        public const string chipUssClassName = ussClassName + "__chip";

        /// <summary>
        /// The styling class of content produced by an <see cref="IChatToolRenderer"/>.
        /// </summary>
        public const string toolContentUssClassName = ussClassName + "__tool-content";

        /// <summary>
        /// The styling class of an answer text block.
        /// </summary>
        public const string textUssClassName = ussClassName + "__text";

        /// <summary>
        /// The title shown over an empty conversation.
        /// </summary>
        public const string defaultEmptyTitle = "How can I help you?";

        /// <summary>
        /// The title shown when the budget is gone and there is nothing to show.
        /// </summary>
        public const string defaultExhaustedTitle = "Budget exhausted";

        /// <summary>
        /// The description shown under <see cref="defaultExhaustedTitle"/>.
        /// </summary>
        public const string defaultExhaustedDescription =
            "You have used all your available points. Start a new chat or contact your administrator to increase your budget.";

        /// <summary>
        /// The notice appended under a conversation whose budget is gone.
        /// </summary>
        public const string defaultBudgetBanner = "Budget exhausted. You can no longer send messages.";

        /// <summary>
        /// The label shown next to the conversation-loading spinner.
        /// </summary>
        public const string defaultLoadingLabel = "Loading conversation…";

        const string k_WarningIcon = "warning";

        readonly ScrollView m_Scroll;

        readonly VisualElement m_List;

        readonly List<ChatMessage> m_Messages = new();

        bool m_IsLoading;

        string m_StreamingMessageId;

        bool m_IsLoadingConversation;

        bool m_BudgetExhausted;

        string m_EmptyTitle = defaultEmptyTitle;

        string m_EmptyDescription;

        string m_ExhaustedTitle = defaultExhaustedTitle;

        string m_ExhaustedDescription = defaultExhaustedDescription;

        string m_BudgetBanner = defaultBudgetBanner;

        string m_LoadingLabel = defaultLoadingLabel;

        string m_StreamingStats;

        bool m_ShowCompletedIndicator = true;

        bool m_MergeThinking = true;

        bool m_ShowStreamingIndicator = true;

        bool m_ActivityOpen;

        ChatActivityHeaderPlacement m_ActivityHeaderPlacement = ChatActivityHeaderPlacement.Top;

        bool m_ActivityFoldRowsByKind;

        ChatActivityPinEntries m_ActivityPinEntries = ChatActivityPinEntries.None;

        float m_ActivityMaxPinnedShare = 1f;

        // Folded-row pager selections per default activity group, keyed by message id and group
        // ordinal, so a rebuild while a turn streams hands each new timeline what the reader paged to.
        readonly Dictionary<string, Dictionary<string, int>> m_PagerSelections =
            new Dictionary<string, Dictionary<string, int>>();

        Func<ChatActivityRenderContext, VisualElement> m_RenderActivity;

        IChatToolRenderer m_ToolRenderer;

        Func<ChatMessage, VisualElement> m_RenderSystemMessage;

        Func<ChatMessageActionsContext, VisualElement> m_RenderActions;

        Func<ChatMessageMetaContext, VisualElement> m_RenderMessageMeta;

        Func<ChatMessagePart, bool> m_StandalonePart;

        Func<ChatStandalonePartContext, VisualElement> m_RenderStandalonePart;

        Func<VisualElement> m_RenderLoadingConversation;

        /// <summary>
        /// Raised when the reader asks for a message to be sent again.
        /// </summary>
        public event Action<ChatMessage> retryRequested;

        /// <summary>
        /// Raised when the reader toggles the activity group of the turn in progress, with the state
        /// they asked for. <see cref="activityOpen"/> already holds it when this fires.
        /// </summary>
        public event Action<bool> activityOpenChanged;

        /// <summary>
        /// Children are managed internally — this element exposes no content container.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The scrolling area holding the conversation. Exposed so a caller can drive it to the bottom
        /// as new messages arrive.
        /// </summary>
        public ScrollView scrollView => m_Scroll;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatMessages()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = scrollUssClassName,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_Scroll.AddToClassList(scrollUssClassName);
            hierarchy.Add(m_Scroll);

            m_List = new VisualElement { name = listUssClassName };
            m_List.AddToClassList(listUssClassName);
            m_Scroll.Add(m_List);

            RefreshUI();
        }

        // -------------------------------------------------------------------
        // Data
        // -------------------------------------------------------------------

        /// <summary>
        /// The conversation. Setting this replaces what is rendered; it never appends.
        /// </summary>
        public IEnumerable<ChatMessage> messages
        {
            get => m_Messages;
            set
            {
                m_Messages.Clear();
                if (value != null)
                {
                    foreach (var message in value)
                    {
                        if (message != null)
                            m_Messages.Add(message);
                    }
                }
                PrunePagerSelections();
                RefreshUI();
            }
        }

        /// <summary>
        /// How many messages are rendered.
        /// </summary>
        public int messageCount => m_Messages.Count;

        // -------------------------------------------------------------------
        // Flags and copy
        // -------------------------------------------------------------------

        /// <summary>
        /// Whether the agent is answering. With no <see cref="streamingMessageId"/> set, this streams the
        /// last message in the conversation.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isLoading
        {
            get => m_IsLoading;
            set
            {
                var changed = m_IsLoading != value;
                m_IsLoading = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in isLoadingProperty);
            }
        }

        /// <summary>
        /// The id of the message currently streaming. When set it decides on its own which message
        /// streams, and the <see cref="isLoading"/> heuristic is not consulted.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string streamingMessageId
        {
            get => m_StreamingMessageId;
            set
            {
                var changed = m_StreamingMessageId != value;
                m_StreamingMessageId = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in streamingMessageIdProperty);
            }
        }

        /// <summary>
        /// Whether the conversation itself is still being fetched. This wins over every other state:
        /// nothing else is rendered while it is true.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isLoadingConversation
        {
            get => m_IsLoadingConversation;
            set
            {
                var changed = m_IsLoadingConversation != value;
                m_IsLoadingConversation = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in isLoadingConversationProperty);
            }
        }

        /// <summary>
        /// Whether the reader has run out of budget. Changes the empty state's copy, and appends a
        /// notice under a populated conversation.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool budgetExhausted
        {
            get => m_BudgetExhausted;
            set
            {
                var changed = m_BudgetExhausted != value;
                m_BudgetExhausted = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in budgetExhaustedProperty);
            }
        }

        /// <summary>
        /// The title shown over an empty conversation.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string emptyTitle
        {
            get => m_EmptyTitle;
            set
            {
                var changed = m_EmptyTitle != value;
                m_EmptyTitle = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in emptyTitleProperty);
            }
        }

        /// <summary>
        /// The optional description under <see cref="emptyTitle"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string emptyDescription
        {
            get => m_EmptyDescription;
            set
            {
                var changed = m_EmptyDescription != value;
                m_EmptyDescription = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in emptyDescriptionProperty);
            }
        }

        /// <summary>
        /// The title replacing <see cref="emptyTitle"/> when the budget is gone.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string exhaustedTitle
        {
            get => m_ExhaustedTitle;
            set
            {
                var changed = m_ExhaustedTitle != value;
                m_ExhaustedTitle = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in exhaustedTitleProperty);
            }
        }

        /// <summary>
        /// The description shown under <see cref="exhaustedTitle"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string exhaustedDescription
        {
            get => m_ExhaustedDescription;
            set
            {
                var changed = m_ExhaustedDescription != value;
                m_ExhaustedDescription = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in exhaustedDescriptionProperty);
            }
        }

        /// <summary>
        /// The notice appended under a populated conversation whose budget is gone.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string budgetBanner
        {
            get => m_BudgetBanner;
            set
            {
                var changed = m_BudgetBanner != value;
                m_BudgetBanner = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in budgetBannerProperty);
            }
        }

        /// <summary>
        /// The label shown next to the conversation-loading spinner.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string loadingLabel
        {
            get => m_LoadingLabel;
            set
            {
                var changed = m_LoadingLabel != value;
                m_LoadingLabel = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in loadingLabelProperty);
            }
        }

        /// <summary>
        /// Figures shown next to the streaming indicator, for instance a token count. Only the streaming
        /// message displays them.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string streamingStats
        {
            get => m_StreamingStats;
            set
            {
                var changed = m_StreamingStats != value;
                m_StreamingStats = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in streamingStatsProperty);
            }
        }

        /// <summary>
        /// Whether the last message reports how long it took once it has finished. True by default.
        /// A message with no measured <see cref="ChatMessage.durationMs"/> reports nothing either way.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showCompletedIndicator
        {
            get => m_ShowCompletedIndicator;
            set
            {
                var changed = m_ShowCompletedIndicator != value;
                m_ShowCompletedIndicator = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in showCompletedIndicatorProperty);
            }
        }

        /// <summary>
        /// Whether consecutive thinking parts collapse into one timeline row. True by default; set it
        /// false to give each thought its own row.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool mergeThinking
        {
            get => m_MergeThinking;
            set
            {
                var changed = m_MergeThinking != value;
                m_MergeThinking = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in mergeThinkingProperty);
            }
        }

        /// <summary>
        /// Whether the activity group of the turn in progress shows its detail. False by default.
        /// </summary>
        /// <remarks>
        /// Only the live group follows it: the last group of the streaming message, before any text
        /// follows it. A finished group is a record the reader opens to look something up, so it
        /// answers to its own header. The list rebuilds as a turn streams, so the reader's own toggle
        /// of the live group is written back here, and carries over from one turn to the next.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool activityOpen
        {
            get => m_ActivityOpen;
            set
            {
                var changed = m_ActivityOpen != value;
                m_ActivityOpen = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in activityOpenProperty);
            }
        }

        /// <summary>
        /// Where every default activity group draws its header; see
        /// <see cref="ChatActivityTimeline.headerPlacement"/>. A live group with its header at the foot
        /// is the turn's status line, so it carries <see cref="streamingStats"/> and the streaming
        /// indicator stands down. A group built by <see cref="renderActivity"/> is never that line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ChatActivityHeaderPlacement activityHeaderPlacement
        {
            get => m_ActivityHeaderPlacement;
            set
            {
                var changed = m_ActivityHeaderPlacement != value;
                m_ActivityHeaderPlacement = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in activityHeaderPlacementProperty);
            }
        }

        /// <summary>
        /// Folds the rows of every default activity group by kind; see
        /// <see cref="ChatActivityTimeline.foldRowsByKind"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool activityFoldRowsByKind
        {
            get => m_ActivityFoldRowsByKind;
            set
            {
                var changed = m_ActivityFoldRowsByKind != value;
                m_ActivityFoldRowsByKind = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in activityFoldRowsByKindProperty);
            }
        }

        /// <summary>
        /// Lays every default activity group out as pinned entries; see
        /// <see cref="ChatActivityTimeline.pinEntries"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ChatActivityPinEntries activityPinEntries
        {
            get => m_ActivityPinEntries;
            set
            {
                var changed = m_ActivityPinEntries != value;
                m_ActivityPinEntries = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in activityPinEntriesProperty);
            }
        }

        /// <summary>
        /// The share of the scroll area every default activity group's pinned entries may take; see
        /// <see cref="ChatActivityTimeline.maxPinnedShare"/>. Clamped to 0..1.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float activityMaxPinnedShare
        {
            get => m_ActivityMaxPinnedShare;
            set
            {
                var clamped = float.IsNaN(value) ? 1f : Math.Clamp(value, 0f, 1f);
                var changed = Math.Abs(m_ActivityMaxPinnedShare - clamped) > 1e-6f;
                m_ActivityMaxPinnedShare = clamped;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in activityMaxPinnedShareProperty);
            }
        }

        /// <summary>
        /// Whether a streaming message shows the streaming indicator under its content. True by
        /// default. Turn it off when the host reports a turn's progress somewhere else, so the list
        /// does not say it twice. The completed indicator is unaffected.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showStreamingIndicator
        {
            get => m_ShowStreamingIndicator;
            set
            {
                var changed = m_ShowStreamingIndicator != value;
                m_ShowStreamingIndicator = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(in showStreamingIndicatorProperty);
            }
        }

        // -------------------------------------------------------------------
        // Seams
        // -------------------------------------------------------------------

        /// <summary>
        /// The application's own tool rendering. See <see cref="IChatToolRenderer"/>.
        /// </summary>
        public IChatToolRenderer toolRenderer
        {
            get => m_ToolRenderer;
            set
            {
                m_ToolRenderer = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Replaces the default destructive banner used for a <see cref="ChatRole.System"/> message.
        /// </summary>
        public Func<ChatMessage, VisualElement> renderSystemMessage
        {
            get => m_RenderSystemMessage;
            set
            {
                m_RenderSystemMessage = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Replaces the default per-message action row. Called for user and assistant messages alike.
        /// </summary>
        public Func<ChatMessageActionsContext, VisualElement> renderActions
        {
            get => m_RenderActions;
            set
            {
                m_RenderActions = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Builds the always-visible meta line above an assistant message's action row, for instance the
        /// elapsed-to-answer figure. Returning null adds nothing.
        /// </summary>
        public Func<ChatMessageMetaContext, VisualElement> renderMessageMeta
        {
            get => m_RenderMessageMeta;
            set
            {
                m_RenderMessageMeta = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Picks the parts rendered as their own block instead of being folded into an activity
        /// group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a part the host would rather present itself -- a generated image, a diff, a preview
        /// -- where the one-line activity row undersells it. A part picked here is excluded from
        /// the activity group either way, so the two can never show it twice.
        /// </para>
        /// <para>
        /// Never consulted for a tool result: results are folded onto their call, so pulling one
        /// out on its own would orphan it. Inert without
        /// <see cref="renderStandalonePart"/>.
        /// </para>
        /// </remarks>
        public Func<ChatMessagePart, bool> standalonePart
        {
            get => m_StandalonePart;
            set
            {
                m_StandalonePart = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Builds the block for a part <see cref="standalonePart"/> picked. Returning null renders
        /// nothing, and the part stays out of the activity group regardless.
        /// </summary>
        public Func<ChatStandalonePartContext, VisualElement> renderStandalonePart
        {
            get => m_RenderStandalonePart;
            set
            {
                m_RenderStandalonePart = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Replaces the default <see cref="ChatActivityTimeline"/>: every activity group of an
        /// assistant message is built by this instead, in the timeline's place. Returning null
        /// renders nothing for that group. For a host that shows the work in its own structure.
        /// </summary>
        /// <remarks>
        /// <see cref="toolRenderer"/> content still follows the group, as it follows the default
        /// timeline. A host-built group carries no live figures, so the streaming indicator stays.
        /// </remarks>
        public Func<ChatActivityRenderContext, VisualElement> renderActivity
        {
            get => m_RenderActivity;
            set
            {
                m_RenderActivity = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// Replaces the default spinner shown while the conversation loads.
        /// </summary>
        public Func<VisualElement> renderLoadingConversation
        {
            get => m_RenderLoadingConversation;
            set
            {
                m_RenderLoadingConversation = value;
                RefreshUI();
            }
        }

        // -------------------------------------------------------------------
        // Building
        // -------------------------------------------------------------------

        void RefreshUI()
        {
            m_List.Clear();

            var loading = m_IsLoadingConversation;
            var empty = !loading && m_Messages.Count == 0;

            EnableInClassList(loadingVariantUssClassName, loading);
            EnableInClassList(emptyVariantUssClassName, empty);

            if (loading)
            {
                m_List.Add(BuildLoadingState());
                return;
            }

            if (empty)
            {
                m_List.Add(BuildEmptyState());
                return;
            }

            ChatMessage lastUserMessage = null;
            for (var i = 0; i < m_Messages.Count; i++)
            {
                var message = m_Messages[i];
                if (message.role == ChatRole.User)
                    lastUserMessage = message;

                m_List.Add(BuildMessage(message, i, lastUserMessage));
            }

            if (m_BudgetExhausted)
                m_List.Add(BuildBanner(m_BudgetBanner));
        }

        VisualElement BuildLoadingState()
        {
            if (m_RenderLoadingConversation != null)
                return m_RenderLoadingConversation() ?? new VisualElement();

            var block = new VisualElement { name = loadingUssClassName, pickingMode = PickingMode.Ignore };
            block.AddToClassList(loadingUssClassName);

            var spinner = new CircularProgress { name = loadingSpinnerUssClassName };
            spinner.AddToClassList(loadingSpinnerUssClassName);
            block.Add(spinner);

            var label = new LocalizedTextElement(m_LoadingLabel)
            {
                name = loadingLabelUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            label.AddToClassList(loadingLabelUssClassName);
            block.Add(label);

            return block;
        }

        VisualElement BuildEmptyState()
        {
            var block = new VisualElement { name = emptyUssClassName, pickingMode = PickingMode.Ignore };
            block.AddToClassList(emptyUssClassName);

            if (m_BudgetExhausted)
            {
                var icon = new Icon
                {
                    name = emptyIconUssClassName,
                    iconName = k_WarningIcon,
                    size = IconSize.Lg,
                    pickingMode = PickingMode.Ignore,
                };
                icon.AddToClassList(emptyIconUssClassName);
                block.Add(icon);
            }

            var title = new LocalizedTextElement(m_BudgetExhausted ? m_ExhaustedTitle : m_EmptyTitle)
            {
                name = emptyTitleUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            title.AddToClassList(emptyTitleUssClassName);
            block.Add(title);

            var descriptionText = m_BudgetExhausted ? m_ExhaustedDescription : m_EmptyDescription;
            if (!string.IsNullOrEmpty(descriptionText))
            {
                var description = new LocalizedTextElement(descriptionText)
                {
                    name = emptyDescriptionUssClassName,
                    pickingMode = PickingMode.Ignore,
                };
                description.AddToClassList(emptyDescriptionUssClassName);
                block.Add(description);
            }

            return block;
        }

        VisualElement BuildBanner(string text)
        {
            var banner = new VisualElement { name = bannerUssClassName, pickingMode = PickingMode.Ignore };
            banner.AddToClassList(bannerUssClassName);

            var icon = new Icon
            {
                name = bannerIconUssClassName,
                iconName = k_WarningIcon,
                size = IconSize.Sm,
                pickingMode = PickingMode.Ignore,
            };
            icon.AddToClassList(bannerIconUssClassName);
            banner.Add(icon);

            var label = new LocalizedTextElement(text)
            {
                name = bannerLabelUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            label.AddToClassList(bannerLabelUssClassName);
            banner.Add(label);

            return banner;
        }

        VisualElement BuildMessage(ChatMessage message, int index, ChatMessage lastUserMessage)
        {
            if (message.role == ChatRole.System)
            {
                if (m_RenderSystemMessage != null)
                    return m_RenderSystemMessage(message) ?? new VisualElement();
                return BuildBanner(message.content);
            }

            if (message.role == ChatRole.User)
                return BuildUserMessage(message);

            return BuildAssistantMessage(message, index, lastUserMessage);
        }

        VisualElement BuildUserMessage(ChatMessage message)
        {
            var bubble = new ChatMessageBubble
            {
                role = ChatMessageRole.User,
                content = message.content,
            };

            AddChips(bubble.sentWith, message.contextItems);
            AddChips(bubble.sentWith, message.attachments);

            var actions = m_RenderActions?.Invoke(new ChatMessageActionsContext(message, ChatRole.User));
            if (actions != null)
                bubble.actions.Add(actions);

            return bubble;
        }

        static void AddChips(VisualElement target, IEnumerable<string> labels)
        {
            if (labels == null)
                return;

            foreach (var label in labels)
            {
                var chip = new Chip { label = label };
                chip.AddToClassList(chipUssClassName);
                target.Add(chip);
            }
        }

        VisualElement BuildAssistantMessage(ChatMessage message, int index, ChatMessage lastUserMessage)
        {
            var isStreaming = IsStreaming(message, index);
            var isLast = index == m_Messages.Count - 1;

            var hasParts = message.parts != null;
            var bubble = new ChatMessageBubble
            {
                role = ChatMessageRole.Assistant,
                content = hasParts ? null : message.content,
            };

            var statusInGroup = hasParts && BuildParts(bubble, message, index, isStreaming);

            BuildFooter(bubble, message, index, lastUserMessage, isStreaming, isLast, statusInGroup);

            return bubble;
        }

        /// <summary>
        /// An explicit id decides on its own; only in its absence does the loading flag stream the last
        /// message.
        /// </summary>
        bool IsStreaming(ChatMessage message, int index)
        {
            if (!string.IsNullOrEmpty(m_StreamingMessageId))
                return message.id == m_StreamingMessageId;

            return m_IsLoading && message.role == ChatRole.Assistant && index == m_Messages.Count - 1;
        }

        /// <returns> Whether the live activity group is the turn's status line, which then carries the
        /// streaming figures in place of the streaming indicator. </returns>
        bool BuildParts(ChatMessageBubble bubble, ChatMessage message, int index, bool isStreaming)
        {
            // Pair every result onto its call first, so a call whose result arrived later in the same
            // message stops reporting itself as pending.
            var results = new Dictionary<string, object>();
            foreach (var part in message.parts)
            {
                if (part.kind == ChatMessagePartKind.ToolResult && !string.IsNullOrEmpty(part.toolCallId))
                    results[part.toolCallId] = part.result;
            }

            var groups = new List<VisualElement>();
            var timelines = new List<ChatActivityTimeline>();
            // With renderActivity, every run in order with its slot in groups, filled once the last
            // run is known.
            var runs = new List<(List<ChatActivityPart> parts, List<ChatMessagePart> source, int slot)>();
            // Whether prose or a standalone block has been written since the most recent activity
            // run closed. Once it has, that run is finished even if the turn is not.
            var textAfterLastTimeline = false;
            List<ChatActivityPart> activity = null;
            // The message parts the open run was folded from, each call followed by its result.
            List<ChatMessagePart> source = null;
            var toolContent = new List<VisualElement>();

            void FlushActivity()
            {
                if (activity == null || activity.Count == 0)
                {
                    activity = null;
                    source = null;
                    return;
                }

                if (m_RenderActivity != null)
                {
                    runs.Add((activity, source, groups.Count));
                    groups.Add(null);
                }
                else
                {
                    // The message's own stamp, so a list rebuilt mid-turn does not restart the count.
                    var timeline = new ChatActivityTimeline
                    {
                        parts = activity,
                        startTime = message.createdAt,
                        headerPlacement = m_ActivityHeaderPlacement,
                        foldRowsByKind = m_ActivityFoldRowsByKind,
                        pinEntries = m_ActivityPinEntries,
                        maxPinnedShare = m_ActivityMaxPinnedShare,
                    };
                    var selectionKey = SelectionOwner(message, index) + "/" + timelines.Count;
                    if (!m_PagerSelections.TryGetValue(selectionKey, out var selections))
                    {
                        selections = new Dictionary<string, int>();
                        m_PagerSelections.Add(selectionKey, selections);
                    }
                    timeline.pagerSelections = selections;
                    timelines.Add(timeline);
                    groups.Add(timeline);
                }
                textAfterLastTimeline = false;
                foreach (var element in toolContent)
                    groups.Add(element);
                toolContent.Clear();
                activity = null;
                source = null;
            }

            foreach (var part in message.parts)
            {
                // A standalone part is pulled out of the bubble before anything else looks at it,
                // so it is never also folded into an activity group. Tool results are exempt:
                // they are folded onto their call, and pulling one out alone would orphan it.
                if (part.kind != ChatMessagePartKind.ToolResult && m_StandalonePart != null &&
                    m_StandalonePart(part))
                {
                    // Flushes the open activity run exactly as a text part does, so the block
                    // lands in reading order rather than jumping above the run it followed.
                    FlushActivity();
                    textAfterLastTimeline = true;
                    if (m_RenderStandalonePart != null)
                    {
                        results.TryGetValue(part.toolCallId ?? string.Empty, out var standaloneResult);
                        var standalone = m_RenderStandalonePart(
                            new ChatStandalonePartContext(part, standaloneResult, isStreaming));
                        if (standalone != null)
                            groups.Add(standalone);
                    }
                    continue;
                }

                switch (part.kind)
                {
                    case ChatMessagePartKind.Text:
                    {
                        FlushActivity();
                        textAfterLastTimeline = true;
                        var text = new LocalizedTextElement(part.text)
                        {
                            name = textUssClassName,
                            enableRichText = true,
                        };
                        text.AddToClassList(textUssClassName);
                        groups.Add(text);
                        break;
                    }
                    case ChatMessagePartKind.Thinking:
                    {
                        activity ??= new List<ChatActivityPart>();
                        source ??= new List<ChatMessagePart>();
                        source.Add(part);
                        if (m_MergeThinking && activity.Count > 0 &&
                            activity[activity.Count - 1].kind == ChatActivityPartKind.Thinking)
                        {
                            var previous = activity[activity.Count - 1];
                            activity[activity.Count - 1] = new ChatActivityPart(
                                ChatActivityPartKind.Thinking,
                                previous.text + "\n\n" + part.text,
                                elapsedMs: CombineElapsed(previous.elapsedMs, part.elapsedMs));
                        }
                        else
                        {
                            activity.Add(new ChatActivityPart(ChatActivityPartKind.Thinking, part.text,
                                elapsedMs: part.elapsedMs));
                        }
                        break;
                    }
                    case ChatMessagePartKind.ToolCall:
                    {
                        activity ??= new List<ChatActivityPart>();
                        object callResult = null;
                        var hasResult = !string.IsNullOrEmpty(part.toolCallId) &&
                            results.TryGetValue(part.toolCallId, out callResult);
                        source ??= new List<ChatMessagePart>();
                        source.Add(part);
                        if (hasResult)
                            source.Add(ChatMessagePart.ToolResult(part.toolCallId, part.toolName, callResult));
                        activity.Add(new ChatActivityPart(ChatActivityPartKind.Tool, part.text, part.toolName,
                            part.pending && !hasResult, part.elapsedMs));

                        if (m_ToolRenderer != null)
                        {
                            results.TryGetValue(part.toolCallId ?? string.Empty, out var result);
                            var element = m_ToolRenderer.CreateToolContent(part, result);
                            if (element != null)
                            {
                                element.AddToClassList(toolContentUssClassName);
                                toolContent.Add(element);
                            }
                        }
                        break;
                    }
                    case ChatMessagePartKind.ToolResult:
                        // Already folded onto its call above.
                        break;
                }
            }

            FlushActivity();

            // Only the message's final activity run is still running; the earlier ones were closed by a
            // text part that has already been written out.
            for (var i = 0; i < timelines.Count; i++)
            {
                var isLastRun = i == timelines.Count - 1;
                timelines[i].isStreaming = isStreaming && isLastRun;
                // Only the final run can still be waiting on prose; the earlier ones were closed by
                // text that has already been written out.
                timelines[i].isFollowedByText = !isLastRun || textAfterLastTimeline;
            }

            // The group still producing activity: the last one, while the message streams and before
            // any text or standalone block follows it.
            ChatActivityTimeline live = null;
            if (isStreaming && timelines.Count > 0 && !textAfterLastTimeline)
                live = timelines[timelines.Count - 1];

            var statusInGroup = false;
            if (live != null)
            {
                // Only the live group follows the held state, and writes the reader's toggle back.
                live.open = m_ActivityOpen;
                live.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    // ChangeEvent bubbles: a bool control inside the group is not the group's header.
                    if (evt.target != live)
                        return;
                    OnLiveActivityToggled(evt.newValue);
                });

                statusInGroup = m_ActivityHeaderPlacement == ChatActivityHeaderPlacement.Bottom;
                if (statusInGroup)
                    live.stats = m_StreamingStats;
            }

            for (var i = 0; i < runs.Count; i++)
            {
                var isLastRun = i == runs.Count - 1;
                groups[runs[i].slot] = m_RenderActivity(new ChatActivityRenderContext(
                    message,
                    runs[i].parts,
                    runs[i].source,
                    i,
                    isStreaming && isLastRun,
                    !isLastRun || textAfterLastTimeline,
                    message.createdAt));
            }

            foreach (var group in groups)
            {
                if (group != null)
                    bubble.Add(group);
            }

            return statusInGroup;
        }

        // Whose pager selections these are: the message's id, or its place in the list when it has
        // none, so two anonymous messages never share one.
        static string SelectionOwner(ChatMessage message, int index) =>
            string.IsNullOrEmpty(message.id) ? "#" + index : "id:" + message.id;

        // Drops the pager selections of messages no longer in the conversation.
        void PrunePagerSelections()
        {
            if (m_PagerSelections.Count == 0)
                return;
            var ids = new HashSet<string>();
            for (var i = 0; i < m_Messages.Count; i++)
                ids.Add(SelectionOwner(m_Messages[i], i));
            var stale = new List<string>();
            foreach (var key in m_PagerSelections.Keys)
            {
                if (!ids.Contains(key.Substring(0, key.LastIndexOf('/'))))
                    stale.Add(key);
            }
            foreach (var key in stale)
                m_PagerSelections.Remove(key);
        }

        void OnLiveActivityToggled(bool value)
        {
            // Stored without a rebuild: the group already shows what the reader asked for.
            var changed = m_ActivityOpen != value;
            m_ActivityOpen = value;
            if (changed)
                NotifyPropertyChanged(in activityOpenProperty);
            activityOpenChanged?.Invoke(value);
        }

        static int CombineElapsed(int a, int b)
        {
            if (a < 0)
                return b;
            if (b < 0)
                return a;
            return a + b;
        }

        void BuildFooter(
            ChatMessageBubble bubble,
            ChatMessage message,
            int index,
            ChatMessage lastUserMessage,
            bool isStreaming,
            bool isLast,
            bool statusInGroup)
        {
            if (isStreaming)
            {
                // A live group whose header sits at its foot already says what the indicator would.
                if (!m_ShowStreamingIndicator || statusInGroup)
                    return;

                // The message's own stamp, not this element's construction, so a list rebuilt
                // mid-turn does not restart the elapsed count from zero.
                var indicator = new ChatStreamingIndicator
                {
                    stats = m_StreamingStats,
                    startTime = message.createdAt,
                };
                bubble.footer.Add(indicator);
                return;
            }

            if (isLast && m_ShowCompletedIndicator && message.durationMs >= 0)
            {
                bubble.footer.Add(new ChatStreamingIndicator
                {
                    completed = true,
                    durationMs = message.durationMs,
                });
            }

            var meta = m_RenderMessageMeta?.Invoke(new ChatMessageMetaContext(
                message, lastUserMessage, ResolveAnswerElapsed(message, lastUserMessage)));
            if (meta != null)
            {
                meta.AddToClassList(metaUssClassName);
                bubble.footer.Add(meta);
            }

            if (m_RenderActions != null)
            {
                var custom = m_RenderActions(new ChatMessageActionsContext(message, ChatRole.Assistant));
                if (custom != null)
                    bubble.actions.Add(custom);
                return;
            }

            var actions = new ChatMessageActions
            {
                content = PlainText(message),
                retryable = true,
            };
            actions.retryRequested += () => retryRequested?.Invoke(message);
            bubble.actions.Add(actions);
        }

        /// <summary>
        /// A duration the backend reported always wins; only in its absence is the gap between the
        /// prompt and the reply used.
        /// </summary>
        static int ResolveAnswerElapsed(ChatMessage message, ChatMessage precedingUserMessage)
        {
            if (message.durationMs >= 0)
                return message.durationMs;

            var started = precedingUserMessage?.createdAt;
            var finished = message.createdAt;
            if (started.HasValue && finished.HasValue && finished.Value > started.Value)
                return (int)(finished.Value - started.Value).TotalMilliseconds;

            return ChatMessage.unknownDuration;
        }

        /// <summary>
        /// The message's answer text, with the reasoning left out — this is what a copy action hands to
        /// the pasteboard.
        /// </summary>
        static string PlainText(ChatMessage message)
        {
            if (message.parts == null)
                return message.content;

            var sb = new StringBuilder();
            foreach (var part in message.parts)
            {
                if (part.kind != ChatMessagePartKind.Text)
                    continue;
                if (sb.Length > 0)
                    sb.Append("\n\n");
                sb.Append(part.text);
            }
            return sb.ToString();
        }
    }
}
