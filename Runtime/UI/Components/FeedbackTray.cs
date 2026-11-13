using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The rating carried by a <see cref="FeedbackTray"/>.
    /// </summary>
    public enum FeedbackValue
    {
        /// <summary>
        /// No rating has been given. This is the default.
        /// </summary>
        None,

        /// <summary>
        /// The response was rated positively (thumbs up).
        /// </summary>
        Good,

        /// <summary>
        /// The response was rated negatively (thumbs down).
        /// </summary>
        Poor,
    }

    /// <summary>
    /// A row of actions for rating and acting on a generated response.
    /// </summary>
    /// <remarks>
    /// FeedbackTray pairs a thumbs up / thumbs down rating with two optional actions, regenerate and
    /// copy. It is the row you put under an assistant message.
    ///
    /// The rating is a three-state value, see <see cref="FeedbackValue"/>. Pressing the active rating
    /// clears it, so a user who mis-clicks can undo without a separate control. The active rating is
    /// reported with the shared <c>is-selected</c> state class, the same one every other selectable
    /// App UI control uses.
    ///
    /// The two trailing actions are opt-in: <see cref="canRefresh"/> and <see cref="canCopy"/> reveal
    /// them, and they report their presses through <see cref="refreshRequested"/> and
    /// <see cref="copyRequested"/>. The tray does not copy anything itself — the host owns the
    /// clipboard — but it will reflect a completed copy by swapping the glyph to a checkmark while
    /// <see cref="copied"/> is set.
    /// </remarks>
    /// <example>
    /// <para>A tray under a chat message.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:FeedbackTray can-copy="true" can-refresh="true" />
    /// ]]></code>
    /// <para>Wiring the tray up in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var tray = new FeedbackTray { canCopy = true, canRefresh = true };
    ///
    /// tray.RegisterValueChangedCallback(evt => SubmitRating(messageId, evt.newValue));
    /// tray.refreshRequested += () => RegenerateResponse(messageId);
    /// tray.copyRequested += () =>
    /// {
    ///     GUIUtility.systemCopyBuffer = messageText;
    ///     tray.copied = true;
    ///     tray.schedule.Execute(() => tray.copied = false).ExecuteLater(2000);
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks", id = "feedback-tray", displayName = "Feedback Tray")]
    public partial class FeedbackTray : BaseVisualElement, INotifyValueChanged<FeedbackValue>
    {
        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId canRefreshProperty = new BindingId(nameof(canRefresh));

        internal static readonly BindingId canCopyProperty = new BindingId(nameof(canCopy));

        internal static readonly BindingId copiedProperty = new BindingId(nameof(copied));

        const string k_GoodIconName = "thumbs-up";

        const string k_PoorIconName = "thumbs-down";

        const string k_RefreshIconName = "arrows-clockwise";

        const string k_CopyIconName = "copy";

        const string k_CopiedIconName = "check";

        /// <summary>
        /// The FeedbackTray main styling class.
        /// </summary>
        public const string ussClassName = "appui-feedbacktray";

        /// <summary>
        /// The styling class shared by every button of the tray.
        /// </summary>
        public const string buttonUssClassName = ussClassName + "__button";

        /// <summary>
        /// The "good" rating button styling class.
        /// </summary>
        public const string goodUssClassName = ussClassName + "__good";

        /// <summary>
        /// The "poor" rating button styling class.
        /// </summary>
        public const string poorUssClassName = ussClassName + "__poor";

        /// <summary>
        /// The regenerate button styling class.
        /// </summary>
        public const string refreshUssClassName = ussClassName + "__refresh";

        /// <summary>
        /// The copy button styling class.
        /// </summary>
        public const string copyUssClassName = ussClassName + "__copy";

        readonly IconButton m_GoodButton;

        readonly IconButton m_PoorButton;

        readonly IconButton m_RefreshButton;

        readonly IconButton m_CopyButton;

        FeedbackValue m_Value;

        bool m_Copied;

        /// <summary>
        /// Raised when the regenerate button has been pressed.
        /// </summary>
        public event Action refreshRequested;

        /// <summary>
        /// Raised when the copy button has been pressed. The host owns the clipboard.
        /// </summary>
        public event Action copyRequested;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FeedbackTray()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_GoodButton = MakeButton(k_GoodIconName, goodUssClassName);
            m_GoodButton.clickable.clicked += () => Toggle(FeedbackValue.Good);

            m_PoorButton = MakeButton(k_PoorIconName, poorUssClassName);
            m_PoorButton.clickable.clicked += () => Toggle(FeedbackValue.Poor);

            m_RefreshButton = MakeButton(k_RefreshIconName, refreshUssClassName);
            m_RefreshButton.clickable.clicked += () => refreshRequested?.Invoke();

            m_CopyButton = MakeButton(k_CopyIconName, copyUssClassName);
            m_CopyButton.clickable.clicked += () => copyRequested?.Invoke();

            hierarchy.Add(m_GoodButton);
            hierarchy.Add(m_PoorButton);
            hierarchy.Add(m_RefreshButton);
            hierarchy.Add(m_CopyButton);

            value = FeedbackValue.None;
            canRefresh = false;
            canCopy = false;
            copied = false;
        }

        static IconButton MakeButton(string iconName, string ussClass)
        {
            var button = new IconButton(iconName) { quiet = true, size = Size.Sm, name = ussClass };
            button.AddToClassList(buttonUssClassName);
            button.AddToClassList(ussClass);
            return button;
        }

        /// <summary>
        /// The thumbs up button.
        /// </summary>
        public IconButton goodButton => m_GoodButton;

        /// <summary>
        /// The thumbs down button.
        /// </summary>
        public IconButton poorButton => m_PoorButton;

        /// <summary>
        /// The regenerate button. Hidden unless <see cref="canRefresh"/> is set.
        /// </summary>
        public IconButton refreshButton => m_RefreshButton;

        /// <summary>
        /// The copy button. Hidden unless <see cref="canCopy"/> is set.
        /// </summary>
        public IconButton copyButton => m_CopyButton;

        /// <summary>
        /// The current rating. Defaults to <see cref="FeedbackValue.None"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public FeedbackValue value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<FeedbackValue>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Whether the regenerate button is available.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool canRefresh
        {
            get => !m_RefreshButton.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = canRefresh != value;
                m_RefreshButton.EnableInClassList(Styles.hiddenUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in canRefreshProperty);
            }
        }

        /// <summary>
        /// Whether the copy button is available.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool canCopy
        {
            get => !m_CopyButton.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = canCopy != value;
                m_CopyButton.EnableInClassList(Styles.hiddenUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in canCopyProperty);
            }
        }

        /// <summary>
        /// Whether the copy button reports a completed copy. Swaps its glyph to a checkmark.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool copied
        {
            get => m_Copied;
            set
            {
                var changed = m_Copied != value;
                m_Copied = value;
                m_CopyButton.icon = value ? k_CopiedIconName : k_CopyIconName;
                m_CopyButton.EnableInClassList(Styles.selectedUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in copiedProperty);
            }
        }

        /// <summary>
        /// Set the rating without raising a <see cref="ChangeEvent{T}"/>.
        /// </summary>
        /// <param name="newValue"> The new rating. </param>
        public void SetValueWithoutNotify(FeedbackValue newValue)
        {
            m_Value = newValue;
            m_GoodButton.EnableInClassList(Styles.selectedUssClassName, newValue == FeedbackValue.Good);
            m_PoorButton.EnableInClassList(Styles.selectedUssClassName, newValue == FeedbackValue.Poor);
        }

        void Toggle(FeedbackValue next)
        {
            value = m_Value == next ? FeedbackValue.None : next;
        }
    }
}
