using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The "the agent is working" line shown above a streaming answer.
    /// </summary>
    /// <remarks>
    /// While streaming, the indicator shows the Unity mark filling itself in next to a present-tense
    /// verb, for instance "Thinking". Once <see cref="completed"/> is set it settles into the solid
    /// mark and the past tense with the duration, for instance "Thought for 6s".
    ///
    /// The animation is not this component's: it is the
    /// <see cref="UnityLogoVariant.Animated"/> variant of <see cref="UnityLogo"/>, which already owns
    /// a scheduled fill driver that starts on attach and stops on detach. This element only swaps the
    /// variant, so there is exactly one animation driver in the package rather than two.
    ///
    /// The verb pair is picked at random when the indicator is constructed, the same way the web
    /// implementation picks one per message, so a long conversation does not read as a stuck loop.
    /// <see cref="verbIndex"/> pins it when you need a deterministic rendering.
    ///
    /// <see cref="durationMs"/> is optional. Left at <see cref="unknownDuration"/>, the completed line
    /// reports how long the indicator itself was on screen; set it when the caller already knows the
    /// real duration of the turn.
    /// </remarks>
    /// <example>
    /// <para>A live indicator, with optional stats in parentheses.</para>
    /// <code lang="csharp">
    /// var indicator = new ChatStreamingIndicator { stats = "2 tools • 1.2k tokens" };
    /// parentElement.Add(indicator);
    /// </code>
    /// <para>Settling it when the turn is over.</para>
    /// <code lang="csharp">
    /// indicator.durationMs = 6400;
    /// indicator.completed = true;
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatStreamingIndicator /&gt;
    /// &lt;appui:ChatStreamingIndicator completed="true" duration-ms="90000" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatStreamingIndicator : BaseVisualElement
    {
        internal static readonly BindingId completedProperty = nameof(completed);

        internal static readonly BindingId durationMsProperty = nameof(durationMs);

        internal static readonly BindingId statsProperty = nameof(stats);

        internal static readonly BindingId verbIndexProperty = nameof(verbIndex);

        internal static readonly BindingId startTimeProperty = nameof(startTime);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId renderVerbProperty = nameof(renderVerb);

        /// <summary>
        /// The ChatStreamingIndicator main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-streaming-indicator";

        /// <summary>
        /// The styling class applied once the turn is over.
        /// </summary>
        public const string completedUssClassName = ussClassName + "--completed";

        /// <summary>
        /// The styling class of the Unity mark.
        /// </summary>
        public const string logoUssClassName = ussClassName + "__logo";

        /// <summary>
        /// The styling class of the verb label.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The styling class of the trailing stats.
        /// </summary>
        public const string statsUssClassName = ussClassName + "__stats";

        /// <summary>
        /// The styling class applied to a custom <see cref="icon"/>.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The styling class applied to the element <see cref="renderVerb"/> returns.
        /// </summary>
        public const string verbUssClassName = ussClassName + "__verb";

        /// <summary>
        /// The value of <see cref="durationMs"/> that means "nobody told me, measure it yourself".
        /// </summary>
        public const int unknownDuration = -1;

        /// <summary>
        /// The present and past tense verbs the indicator picks from, mirroring the web implementation's
        /// <c>VERBS</c> table.
        /// </summary>
        internal static readonly (string present, string past)[] verbs =
        {
            ("Thinking", "Thought"),
            ("Preparing", "Prepared"),
            ("Musing", "Mused"),
            ("Pondering", "Pondered"),
            ("Analyzing", "Analyzed"),
            ("Reasoning", "Reasoned"),
            ("Considering", "Considered"),
            ("Reflecting", "Reflected"),
        };

        readonly UnityLogo m_Logo;

        readonly Shimmer m_Label;

        readonly LocalizedTextElement m_StatsElement;

        readonly float m_OriginTime;

        DateTime? m_StartTime;

        bool m_Completed;

        int m_DurationMs = unknownDuration;

        string m_Stats;

        int m_VerbIndex;

        float m_FrozenElapsedMs = unknownDuration;

        VisualElement m_Icon;

        Func<string, VisualElement> m_RenderVerb;

        VisualElement m_RenderedVerb;

        string m_RenderedFor;

        /// <summary>
        /// The Unity mark the indicator animates.
        /// </summary>
        public UnityLogo logo => m_Logo;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatStreamingIndicator()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_OriginTime = TimeUtils.GetCurrentTime();

            m_Logo = new UnityLogo { name = logoUssClassName, size = IconSize.Sm };
            m_Logo.AddToClassList(logoUssClassName);

            m_Label = new Shimmer { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_StatsElement = new LocalizedTextElement { name = statsUssClassName, pickingMode = PickingMode.Ignore };
            m_StatsElement.AddToClassList(statsUssClassName);

            hierarchy.Add(m_Logo);
            hierarchy.Add(m_Label);
            hierarchy.Add(m_StatsElement);

            m_VerbIndex = UnityEngine.Random.Range(0, verbs.Length);

            Refresh();
        }

        /// <summary>
        /// Whether the turn is over. False by default.
        /// </summary>
        /// <remarks>
        /// Flipping this to true freezes the measured elapsed time, so a completed indicator does not
        /// keep counting up while it stays on screen.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool completed
        {
            get => m_Completed;
            set
            {
                var changed = m_Completed != value;
                if (value && !m_Completed)
                    m_FrozenElapsedMs = MeasuredElapsedMs();
                else if (!value)
                    m_FrozenElapsedMs = unknownDuration;

                m_Completed = value;
                EnableInClassList(completedUssClassName, m_Completed);
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in completedProperty);
            }
        }

        /// <summary>
        /// How long the turn took, in milliseconds. <see cref="unknownDuration"/> by default, which
        /// makes the indicator report its own on-screen time instead.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int durationMs
        {
            get => m_DurationMs;
            set
            {
                var changed = m_DurationMs != value;
                m_DurationMs = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in durationMsProperty);
            }
        }

        /// <summary>
        /// Extra detail shown in parentheses while streaming, for instance "2 tools • 1.2k tokens".
        /// Null or empty hides the trailing element entirely.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string stats
        {
            get => m_Stats;
            set
            {
                var changed = m_Stats != value;
                m_Stats = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in statsProperty);
            }
        }

        /// <summary>
        /// Which verb pair the indicator uses, as an index into <see cref="verbs"/>. Randomized at
        /// construction; set it to pin the wording.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int verbIndex
        {
            get => m_VerbIndex;
            set
            {
                var clamped = ((value % verbs.Length) + verbs.Length) % verbs.Length;
                var changed = m_VerbIndex != clamped;
                m_VerbIndex = clamped;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in verbIndexProperty);
            }
        }

        /// <summary>
        /// The line the indicator currently reads.
        /// </summary>
        public string label => m_Label.text;

        /// <summary>
        /// When the turn started, so the elapsed time survives the element being rebuilt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unset, the indicator measures from its own construction, which is right for an element
        /// that lives as long as the turn. A host that rebuilds its message list mid-turn destroys
        /// and recreates this element, and a construction-time origin would restart the count from
        /// zero every time; handing the message's own <see cref="ChatMessage.createdAt"/> in keeps
        /// it honest. This is the registry's <c>startTimeMs</c>, in the wall-clock currency the
        /// rest of the chat suite already speaks.
        /// </para>
        /// <para>
        /// It is deliberately a wall clock rather than the frame clock the fallback uses. A frame
        /// clock has no meaning across the rebuild this property exists to survive.
        /// </para>
        /// </remarks>
        [CreateProperty]
        public DateTime? startTime
        {
            get => m_StartTime;
            set
            {
                var changed = m_StartTime != value;
                m_StartTime = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in startTimeProperty);
            }
        }

        /// <summary>
        /// Milliseconds since the turn started: from <see cref="startTime"/> on the wall clock when
        /// one was given, otherwise from this element's own construction on the frame clock.
        /// </summary>
        float MeasuredElapsedMs()
        {
            if (m_StartTime.HasValue)
                return (float)(DateTime.UtcNow - m_StartTime.Value).TotalMilliseconds;

            return (TimeUtils.GetCurrentTime() - m_OriginTime) * 1000f;
        }

        /// <summary>
        /// The duration the completed line reports, in milliseconds: <see cref="durationMs"/> when it
        /// was supplied, otherwise the indicator's measured on-screen time.
        /// </summary>
        internal float effectiveDurationMs =>
            m_DurationMs >= 0
                ? m_DurationMs
                : m_FrozenElapsedMs >= 0
                    ? m_FrozenElapsedMs
                    : Mathf.Max(0f, MeasuredElapsedMs());

        /// <summary>
        /// A mark shown before the verb in place of the Unity <see cref="logo"/>, in both states. Null,
        /// the default, keeps the logo.
        /// </summary>
        /// <remarks>
        /// The registry's <c>icon</c>. While a custom mark is set the logo is hidden and held on its
        /// filled variant, so its animation driver does not run behind it.
        /// </remarks>
        [CreateProperty]
        public VisualElement icon
        {
            get => m_Icon;
            set
            {
                if (m_Icon == value)
                    return;

                if (m_Icon != null)
                {
                    m_Icon.RemoveFromClassList(iconUssClassName);
                    m_Icon.RemoveFromHierarchy();
                }

                m_Icon = value;

                if (m_Icon != null)
                {
                    m_Icon.AddToClassList(iconUssClassName);
                    hierarchy.Insert(0, m_Icon);
                }

                m_Logo.EnableInClassList(Styles.hiddenUssClassName, m_Icon != null);
                Refresh();
                NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// Renders the live verb ("Considering") as an element of the host's own, so it can give the verb
        /// a treatment of its own. Null, the default, keeps the verb under the sweep.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The registry's <c>renderVerb</c>. The rendered element replaces the sweep rather than
        /// wrapping it: both paint the text, and two would fight. It is used only while the turn runs;
        /// the completed line ("Considered for 6s") stays plain. A function that returns null renders
        /// no verb at all, rather than bringing the sweep back.
        /// </para>
        /// <para>
        /// The function builds an element, so it runs when the verb changes or the function is
        /// replaced, not on every refresh: a stats update does not rebuild the verb.
        /// </para>
        /// </remarks>
        [CreateProperty]
        public Func<string, VisualElement> renderVerb
        {
            get => m_RenderVerb;
            set
            {
                if (m_RenderVerb == value)
                    return;

                m_RenderVerb = value;
                ClearRenderedVerb();
                Refresh();
                NotifyPropertyChanged(in renderVerbProperty);
            }
        }

        void ClearRenderedVerb()
        {
            if (m_RenderedVerb != null)
            {
                m_RenderedVerb.RemoveFromClassList(verbUssClassName);
                m_RenderedVerb.RemoveFromHierarchy();
            }

            m_RenderedVerb = null;
            m_RenderedFor = null;
        }

        void RefreshRenderedVerb(string present)
        {
            if (m_Completed || m_RenderVerb == null)
            {
                ClearRenderedVerb();
                return;
            }

            // Keyed on the verb, not on the element: a renderer that returns null for this verb has
            // still rendered it, as nothing, and is not asked again until the verb changes.
            if (m_RenderedFor == present)
                return;

            ClearRenderedVerb();
            m_RenderedVerb = m_RenderVerb(present);
            m_RenderedFor = present;

            if (m_RenderedVerb != null)
            {
                m_RenderedVerb.AddToClassList(verbUssClassName);
                hierarchy.Insert(hierarchy.IndexOf(m_Label), m_RenderedVerb);
            }
        }

        void Refresh()
        {
            var verb = verbs[m_VerbIndex];

            m_Label.text = m_Completed
                ? verb.past + " for " + ChatFormat.Duration(effectiveDurationMs)
                : verb.present;

            // A custom mark stands in for the logo, so the hidden logo stays still.
            m_Logo.variant = m_Completed || m_Icon != null ? UnityLogoVariant.Filled : UnityLogoVariant.Animated;

            RefreshRenderedVerb(verb.present);
            // The renderer's presence decides, as in the registry: one that returns null renders
            // nothing in place of the sweep, it does not bring the sweep back.
            var rendered = !m_Completed && m_RenderVerb != null;
            m_Label.EnableInClassList(Styles.hiddenUssClassName, rendered);

            // The verb sweeps while the turn runs and settles once it is over. The stats
            // deliberately do not: they are figures to read, not a progress cue. A rendered verb
            // replaces the sweep, which then stops too.
            m_Label.active = !m_Completed && !rendered;

            var showStats = !m_Completed && !string.IsNullOrEmpty(m_Stats);
            m_StatsElement.text = showStats ? "(" + m_Stats + ")" : null;
            m_StatsElement.EnableInClassList(Styles.hiddenUssClassName, !showStats);
        }
    }
}
