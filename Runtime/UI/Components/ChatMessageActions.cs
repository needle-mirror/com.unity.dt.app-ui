using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Which way a user rated an answer.
    /// </summary>
    public enum FeedbackSentiment
    {
        /// <summary>
        /// No rating yet. This is where a fresh action row starts.
        /// </summary>
        None = 0,

        /// <summary>
        /// The answer was good.
        /// </summary>
        Positive,

        /// <summary>
        /// The answer was bad.
        /// </summary>
        Negative
    }

    /// <summary>
    /// The hover-revealed action row under a chat message.
    /// </summary>
    /// <remarks>
    /// Copy, thumbs up, thumbs down and, when the caller offers it, retry. Copy writes
    /// <see cref="content"/> to the system clipboard and flips its glyph to a check for
    /// <see cref="copiedFeedbackMs"/>, so the user gets confirmation without a toast.
    ///
    /// The rating is local visual state: the row remembers which way it was pressed and reports it
    /// through <see cref="feedbackGiven"/>, but the caller decides what that means and can pin it by
    /// assigning <see cref="sentiment"/> when it loads a past conversation.
    ///
    /// Every control is an <see cref="IconButton"/>, so the row inherits the package's focus ring,
    /// hover tint and sizing rather than restating them.
    /// </remarks>
    /// <example>
    /// <para>An action row with retry.</para>
    /// <code lang="csharp">
    /// var actions = new ChatMessageActions { content = answerText, retryable = true };
    /// actions.retryRequested += Regenerate;
    /// actions.feedbackGiven += s => Report(s);
    /// parentElement.Add(actions);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatMessageActions content="Hello" retryable="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatMessageActions : BaseVisualElement
    {
        internal static readonly BindingId contentProperty = nameof(content);

        internal static readonly BindingId retryableProperty = nameof(retryable);

        internal static readonly BindingId sentimentProperty = nameof(sentiment);

        /// <summary>
        /// The ChatMessageActions main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-message-actions";

        /// <summary>
        /// The styling class of the copy control.
        /// </summary>
        public const string copyButtonUssClassName = ussClassName + "__copy-button";

        /// <summary>
        /// The styling class of the thumbs up control.
        /// </summary>
        public const string thumbsUpButtonUssClassName = ussClassName + "__thumbs-up-button";

        /// <summary>
        /// The styling class of the thumbs down control.
        /// </summary>
        public const string thumbsDownButtonUssClassName = ussClassName + "__thumbs-down-button";

        /// <summary>
        /// The styling class of the retry control.
        /// </summary>
        public const string retryButtonUssClassName = ussClassName + "__retry-button";

        /// <summary>
        /// The styling class applied to the copy control while it is confirming.
        /// </summary>
        public const string copiedUssClassName = ussClassName + "__copy-button--copied";

        /// <summary>
        /// How long the copy control stays on its confirmation glyph, in milliseconds.
        /// </summary>
        public const long copiedFeedbackMs = 2000;

        readonly IconButton m_CopyButton;

        readonly IconButton m_ThumbsUpButton;

        readonly IconButton m_ThumbsDownButton;

        readonly IconButton m_RetryButton;

        IVisualElementScheduledItem m_CopiedReset;

        bool m_Retryable;

        string m_Content;

        FeedbackSentiment m_Sentiment;

        /// <summary>
        /// Raised when the user asks for the answer to be regenerated.
        /// </summary>
        public event Action retryRequested;

        /// <summary>
        /// Raised when the user rates the answer.
        /// </summary>
        public event Action<FeedbackSentiment> feedbackGiven;

        /// <summary>
        /// Raised when <see cref="content"/> has been written to the clipboard.
        /// </summary>
        public event Action contentCopied;

        /// <summary>
        /// The copy control.
        /// </summary>
        public IconButton copyButton => m_CopyButton;

        /// <summary>
        /// The thumbs up control.
        /// </summary>
        public IconButton thumbsUpButton => m_ThumbsUpButton;

        /// <summary>
        /// The thumbs down control.
        /// </summary>
        public IconButton thumbsDownButton => m_ThumbsDownButton;

        /// <summary>
        /// The retry control.
        /// </summary>
        public IconButton retryButton => m_RetryButton;

        /// <summary>
        /// Whether the copy control is currently showing its confirmation glyph.
        /// </summary>
        public bool copied => m_CopyButton.ClassListContains(copiedUssClassName);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatMessageActions()
        {
            AddToClassList(ussClassName);

            m_CopyButton = CreateButton(copyButtonUssClassName, "copy", "Copy");
            m_CopyButton.clicked += Copy;

            m_ThumbsUpButton = CreateButton(thumbsUpButtonUssClassName, "thumbs-up", "Good response");
            m_ThumbsUpButton.clicked += () => sentiment = FeedbackSentiment.Positive;

            m_ThumbsDownButton = CreateButton(thumbsDownButtonUssClassName, "thumbs-down", "Bad response");
            m_ThumbsDownButton.clicked += () => sentiment = FeedbackSentiment.Negative;

            m_RetryButton = CreateButton(retryButtonUssClassName, "arrow-clockwise", "Retry");
            m_RetryButton.clicked += () => retryRequested?.Invoke();

            hierarchy.Add(m_CopyButton);
            hierarchy.Add(m_ThumbsUpButton);
            hierarchy.Add(m_ThumbsDownButton);
            hierarchy.Add(m_RetryButton);

            retryable = false;
            sentiment = FeedbackSentiment.None;

            RegisterCallback<DetachFromPanelEvent>(_ => CancelCopiedReset());
        }

        /// <summary>
        /// The plain text the copy control writes to the clipboard.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string content
        {
            get => m_Content;
            set
            {
                var changed = m_Content != value;
                m_Content = value;

                if (changed)
                    NotifyPropertyChanged(in contentProperty);
            }
        }

        /// <summary>
        /// Whether the retry control is offered. False by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool retryable
        {
            get => m_Retryable;
            set
            {
                var changed = m_Retryable != value;
                m_Retryable = value;
                m_RetryButton.EnableInClassList(Styles.hiddenUssClassName, !m_Retryable);

                if (changed)
                    NotifyPropertyChanged(in retryableProperty);
            }
        }

        /// <summary>
        /// How the answer is currently rated. <see cref="FeedbackSentiment.None"/> by default.
        /// </summary>
        /// <remarks>
        /// Setting this marks the matching control as selected and raises
        /// <see cref="feedbackGiven"/>, so a rating made in code and a rating made by clicking travel
        /// the same path. Re-setting the same value is a no-op and raises nothing.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public FeedbackSentiment sentiment
        {
            get => m_Sentiment;
            set
            {
                var changed = m_Sentiment != value;
                m_Sentiment = value;
                m_ThumbsUpButton.EnableInClassList(Styles.selectedUssClassName, m_Sentiment == FeedbackSentiment.Positive);
                m_ThumbsDownButton.EnableInClassList(Styles.selectedUssClassName, m_Sentiment == FeedbackSentiment.Negative);

                if (!changed)
                    return;

                NotifyPropertyChanged(in sentimentProperty);
                feedbackGiven?.Invoke(m_Sentiment);
            }
        }

        /// <summary>
        /// Writes <see cref="content"/> to the system clipboard and shows the confirmation glyph.
        /// </summary>
        public void Copy()
        {
            GUIUtility.systemCopyBuffer = m_Content ?? string.Empty;

            m_CopyButton.AddToClassList(copiedUssClassName);
            m_CopyButton.icon = "check";
            m_CopyButton.tooltip = "Copied";

            CancelCopiedReset();
            m_CopiedReset = schedule.Execute(ResetCopied).StartingIn(copiedFeedbackMs);

            contentCopied?.Invoke();
        }

        /// <summary>
        /// Restores the copy control to its resting glyph.
        /// </summary>
        internal void ResetCopied()
        {
            m_CopyButton.RemoveFromClassList(copiedUssClassName);
            m_CopyButton.icon = "copy";
            m_CopyButton.tooltip = "Copy";
        }

        void CancelCopiedReset()
        {
            if (m_CopiedReset == null)
                return;

            m_CopiedReset.Pause();
            m_CopiedReset = null;
        }

        static IconButton CreateButton(string className, string icon, string label)
        {
            var button = new IconButton
            {
                name = className,
                icon = icon,
                quiet = true,
                size = Size.Sm,
                tooltip = label,
            };
            button.AddToClassList(className);
            return button;
        }
    }
}
