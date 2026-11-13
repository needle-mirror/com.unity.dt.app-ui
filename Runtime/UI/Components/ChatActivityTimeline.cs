using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// What one row of a <see cref="ChatActivityTimeline"/> reports.
    /// </summary>
    public enum ChatActivityPartKind
    {
        /// <summary>
        /// A stretch of the agent reasoning. Drawn with a clock.
        /// </summary>
        Thinking = 0,

        /// <summary>
        /// A tool call. Drawn with a spinner while it is pending, a wrench once it has returned.
        /// </summary>
        Tool
    }

    /// <summary>
    /// One entry in an agent's activity timeline.
    /// </summary>
    public readonly struct ChatActivityPart
    {
        /// <summary>
        /// What this entry reports.
        /// </summary>
        public ChatActivityPartKind kind { get; }

        /// <summary>
        /// The row's text: the reasoning for a <see cref="ChatActivityPartKind.Thinking"/> part, the
        /// tool's own header line for a <see cref="ChatActivityPartKind.Tool"/> one.
        /// </summary>
        public string text { get; }

        /// <summary>
        /// Whether a tool call is still waiting on its result. Ignored for a thinking part.
        /// </summary>
        public bool pending { get; }

        /// <summary>
        /// Wall-clock duration of the entry, in milliseconds. Negative means "not measured", which
        /// keeps it out of both the row badge and the header total.
        /// </summary>
        public int elapsedMs { get; }

        /// <summary>
        /// Constructs a part.
        /// </summary>
        /// <param name="kind"> What the entry reports. </param>
        /// <param name="text"> The row's text. </param>
        /// <param name="pending"> Whether a tool call is still running. </param>
        /// <param name="elapsedMs"> Duration in milliseconds, or a negative value if unmeasured. </param>
        public ChatActivityPart(ChatActivityPartKind kind, string text, bool pending = false, int elapsedMs = -1)
            : this(kind, text, null, pending, elapsedMs) { }

        /// <summary>
        /// Constructs a part that names its tool.
        /// </summary>
        /// <param name="kind"> What the entry reports. </param>
        /// <param name="text"> The row's text. </param>
        /// <param name="toolName"> The tool's name, for a <see cref="ChatActivityPartKind.Tool"/> part. </param>
        /// <param name="pending"> Whether a tool call is still running. </param>
        /// <param name="elapsedMs"> Duration in milliseconds, or a negative value if unmeasured. </param>
        public ChatActivityPart(ChatActivityPartKind kind, string text, string toolName, bool pending, int elapsedMs)
        {
            this.kind = kind;
            this.text = text;
            this.toolName = toolName;
            this.pending = pending;
            this.elapsedMs = elapsedMs;
        }

        /// <summary>
        /// The tool a <see cref="ChatActivityPartKind.Tool"/> part called, when known. A running
        /// timeline names its newest call in flight by it, and <see cref="ChatActivityTimeline.foldRowsByKind"/>
        /// folds calls of the same tool into one row. Null for a thinking part.
        /// </summary>
        public string toolName { get; }
    }

    /// <summary>
    /// Where a <see cref="ChatActivityTimeline"/> draws its header.
    /// </summary>
    public enum ChatActivityHeaderPlacement
    {
        /// <summary>
        /// A line above the rows.
        /// </summary>
        Top = 0,

        /// <summary>
        /// A chip at the foot of the group, with the rows opening upward above it.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// How a <see cref="ChatActivityTimeline"/> lays its rows out as pinned entries.
    /// </summary>
    public enum ChatActivityPinEntries
    {
        /// <summary>
        /// Rows are plain blocks that wrap and scroll with the content.
        /// </summary>
        None = 0,

        /// <summary>
        /// Rows are one-line entries; the ones above the row being read pin at the head of the
        /// scroll area and the ones below it at the foot.
        /// </summary>
        HeadAndFoot,

        /// <summary>
        /// Rows are one-line entries that pin at the foot only, for a log read top to bottom.
        /// </summary>
        Foot
    }

    /// <summary>
    /// The collapsed record of what an agent did before it answered.
    /// </summary>
    /// <remarks>
    /// Closed, the timeline is a single line: the label, an ellipsis while the turn is still streaming,
    /// and the total measured time. Opened, it lists every <see cref="ChatActivityPart"/> as a row with
    /// its own glyph, joined by a vertical connector. The connector is inset and drawn behind the rows
    /// rather than masked by an opaque icon background, so the timeline sits correctly on any surface.
    ///
    /// Closed, a finished timeline reads as a tally of what it did -- "3 thoughts, 2 tools" -- rather
    /// than repeating the label. While it runs it shows the label and a live elapsed count, and the
    /// label sweeps as a <see cref="Shimmer"/>. It is finished once the turn ends or, earlier, once
    /// <see cref="isFollowedByText"/> says the answer's own prose has begun.
    ///
    /// Pending tool calls reuse <see cref="CircularProgress"/> for their spinner rather than
    /// reimplementing one.
    /// </remarks>
    /// <example>
    /// <para>A finished turn with one thought and one tool call.</para>
    /// <code lang="csharp">
    /// var timeline = new ChatActivityTimeline
    /// {
    ///     parts = new[]
    ///     {
    ///         new ChatActivityPart(ChatActivityPartKind.Thinking, "The jump force lives on the controller.", elapsedMs: 800),
    ///         new ChatActivityPart(ChatActivityPartKind.Tool, "Read PlayerController.cs", elapsedMs: 4200),
    ///     },
    ///     open = true
    /// };
    /// parentElement.Add(timeline);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatActivityTimeline label="Running tool" is-streaming="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatActivityTimeline : BaseVisualElement
    {
        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId isStreamingProperty = nameof(isStreaming);

        internal static readonly BindingId isFollowedByTextProperty = nameof(isFollowedByText);

        internal static readonly BindingId startTimeProperty = nameof(startTime);

        internal static readonly BindingId openProperty = nameof(open);

        internal static readonly BindingId statsProperty = nameof(stats);

        internal static readonly BindingId headerPlacementProperty = nameof(headerPlacement);

        internal static readonly BindingId foldRowsByKindProperty = nameof(foldRowsByKind);

        internal static readonly BindingId pinEntriesProperty = nameof(pinEntries);

        internal static readonly BindingId maxPinnedShareProperty = nameof(maxPinnedShare);

        /// <summary>
        /// The ChatActivityTimeline main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-activity-timeline";

        // Glyphs the rows draw. They must stay in the required icon set (Icons - Minimal.uss), since
        // a theme without the full icon library resolves nothing else; ChatActivityTimelineTests checks it.
        internal static readonly string[] requiredIconNames = { k_ThinkingIconName, k_ToolIconName };

        internal const string k_ThinkingIconName = "clock";

        internal const string k_ToolIconName = "wrench";

        /// <summary>
        /// The styling class of the summary line.
        /// </summary>
        public const string summaryUssClassName = ussClassName + "__summary";

        /// <summary>
        /// The styling class of the summary label.
        /// </summary>
        public const string summaryLabelUssClassName = ussClassName + "__summary-label";

        /// <summary>
        /// The styling class of the total elapsed time in the summary.
        /// </summary>
        public const string summaryElapsedUssClassName = ussClassName + "__summary-elapsed";

        /// <summary>
        /// The styling class of the live figures in the summary.
        /// </summary>
        public const string summaryStatsUssClassName = ussClassName + "__summary-stats";

        /// <summary>
        /// The styling class of a timeline whose header sits at its foot.
        /// </summary>
        public const string headerBottomUssClassName = ussClassName + "--header-bottom";

        /// <summary>
        /// The styling class of a timeline whose rows are laid out as one-line entries.
        /// </summary>
        public const string entriesUssClassName = ussClassName + "--entries";

        /// <summary>
        /// The styling class of the summary caret.
        /// </summary>
        public const string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The styling class of the row container.
        /// </summary>
        public const string rowsUssClassName = ussClassName + "__rows";

        /// <summary>
        /// The styling class of one row.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The styling class marking the last row, which draws no connector.
        /// </summary>
        public const string lastRowUssClassName = rowUssClassName + "--last";

        /// <summary>
        /// The styling class of the vertical connector between rows.
        /// </summary>
        public const string connectorUssClassName = ussClassName + "__connector";

        /// <summary>
        /// The styling class of a row's glyph slot.
        /// </summary>
        public const string rowGlyphUssClassName = ussClassName + "__row-glyph";

        /// <summary>
        /// The styling class of a row's text.
        /// </summary>
        public const string rowTextUssClassName = ussClassName + "__row-text";

        /// <summary>
        /// The styling class of a row's elapsed badge.
        /// </summary>
        public const string rowElapsedUssClassName = ussClassName + "__row-elapsed";

        /// <summary>
        /// The styling class of an entry that pins at the head of the scroll area.
        /// </summary>
        public const string rowPinnedHeadUssClassName = rowUssClassName + "--pinned-head";

        /// <summary>
        /// The styling class of an entry that pins at the foot of the scroll area.
        /// </summary>
        public const string rowPinnedFootUssClassName = rowUssClassName + "--pinned-foot";

        /// <summary>
        /// The styling class of the pager a folded row uses to step through its parts.
        /// </summary>
        public const string rowPagerUssClassName = ussClassName + "__row-pager";

        /// <summary>
        /// The styling class of the pager's "2 of 3" label.
        /// </summary>
        public const string rowPagerLabelUssClassName = ussClassName + "__row-pager-label";

        /// <summary>
        /// Past this many rows a pinned list would be most of the view, so the entries stand down
        /// and scroll with their rows.
        /// </summary>
        public const int maxPinnedEntries = 6;

        /// <summary>
        /// The default summary label.
        /// </summary>
        public const string defaultLabel = "Thinking";

        /// <summary>
        /// How often the live elapsed readout updates while the activity runs, in milliseconds.
        /// Matches the registry's 500ms interval: a second-resolution readout needs nothing finer.
        /// </summary>
        internal const long tickIntervalMs = 500;

        readonly VisualElement m_Summary;

        readonly Shimmer m_SummaryLabel;

        readonly LocalizedTextElement m_SummaryElapsed;

        readonly Icon m_Caret;

        readonly VisualElement m_Rows;

        readonly Pressable m_Clickable;

        readonly List<ChatActivityPart> m_Parts = new List<ChatActivityPart>();

        string m_Label = defaultLabel;

        bool m_IsStreaming;

        bool m_Open;

        bool m_IsFollowedByText;

        DateTime? m_StartTime;

        IVisualElementScheduledItem m_TickItem;

        string m_Stats;

        ChatActivityHeaderPlacement m_HeaderPlacement = ChatActivityHeaderPlacement.Top;

        bool m_FoldRowsByKind;

        ChatActivityPinEntries m_PinEntries = ChatActivityPinEntries.None;

        float m_MaxPinnedShare = 1f;

        int m_PinBudget = maxPinnedEntries;

        readonly LocalizedTextElement m_SummaryStats;

        // The part a folded row's pager is holding, by row key. Absent means "the newest".
        Dictionary<string, int> m_PagerIndex = new Dictionary<string, int>();

        // The scroll area the pin budget is measured against, watched while attached.
        ScrollView m_PinScroller;

        // Set by a press on a foot-placed header, until the header's next layout.
        bool m_KeepHeaderInViewPending;

        /// <summary>
        /// The summary line's press handler. Toggles <see cref="open"/>.
        /// </summary>
        public Pressable clickable => m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatActivityTimeline()
        {
            AddToClassList(ussClassName);

            m_Summary = new VisualElement { name = summaryUssClassName, focusable = true };
            m_Summary.AddToClassList(summaryUssClassName);

            m_SummaryLabel = new Shimmer { name = summaryLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_SummaryLabel.AddToClassList(summaryLabelUssClassName);

            m_SummaryElapsed = new LocalizedTextElement { name = summaryElapsedUssClassName, pickingMode = PickingMode.Ignore };
            m_SummaryElapsed.AddToClassList(summaryElapsedUssClassName);

            m_SummaryStats = new LocalizedTextElement { name = summaryStatsUssClassName, pickingMode = PickingMode.Ignore };
            m_SummaryStats.AddToClassList(summaryStatsUssClassName);

            m_Caret = new Icon { name = caretUssClassName, size = IconSize.Xs, pickingMode = PickingMode.Ignore };
            m_Caret.AddToClassList(caretUssClassName);

            m_Summary.hierarchy.Add(m_SummaryLabel);
            m_Summary.hierarchy.Add(m_SummaryElapsed);
            m_Summary.hierarchy.Add(m_SummaryStats);
            m_Summary.hierarchy.Add(m_Caret);

            m_Rows = new VisualElement { name = rowsUssClassName, pickingMode = PickingMode.Ignore };
            m_Rows.AddToClassList(rowsUssClassName);

            hierarchy.Add(m_Summary);
            hierarchy.Add(m_Rows);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                UpdateTicker();
                WatchPinScroller();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                UnwatchPinScroller();
                m_TickItem?.Pause();
                m_TickItem = null;
            });

            RegisterCallback<GeometryChangedEvent>(_ => MeasurePinBudget());

            m_Clickable = new Pressable(OnSummaryPressed);
            m_Summary.RegisterCallback<GeometryChangedEvent>(OnSummaryGeometryChanged);
            m_Summary.AddManipulator(m_Clickable);

            open = false;
            Refresh();
        }

        /// <summary>
        /// The summary label. "Thinking" by default; pass something like "Running tool" when the turn
        /// is dominated by tool calls.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label;
            set
            {
                var changed = m_Label != value;
                m_Label = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Whether the turn is still running. False by default, which appends the terminal "Done" row.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isStreaming
        {
            get => m_IsStreaming;
            set
            {
                var changed = m_IsStreaming != value;
                m_IsStreaming = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in isStreamingProperty);
            }
        }

        /// <summary>
        /// Whether the rows are expanded. False by default: the timeline is a footnote, not the answer.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool open
        {
            get => m_Open;
            set
            {
                var changed = m_Open != value;
                m_Open = value;
                EnableInClassList(Styles.openUssClassName, m_Open);
                m_Rows.EnableInClassList(Styles.hiddenUssClassName, !m_Open);
                UpdateCaret();
                if (!m_Open)
                    m_PinBudget = maxPinnedEntries;
                ApplyPinClasses();

                if (changed)
                    NotifyPropertyChanged(in openProperty);
            }
        }

        /// <summary>
        /// Opens or closes the timeline the way the summary press does: it sets <see cref="open"/> and,
        /// when that changed it, sends a <see cref="ChangeEvent{T}"/> of bool. Setting
        /// <see cref="open"/> from code sends nothing, so only the reader's own toggles are reported.
        /// </summary>
        /// <param name="value"> The state asked for. </param>
        internal void RequestOpen(bool value)
        {
            var previous = m_Open;
            open = value;
            if (previous == m_Open)
                return;

            using var evt = ChangeEvent<bool>.GetPooled(previous, m_Open);
            evt.target = this;
            SendEvent(evt);
        }

        /// <summary>
        /// Live figures, for instance a token count, shown on the header. Null or empty shows nothing.
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
                RefreshSummary();

                if (changed)
                    NotifyPropertyChanged(in statsProperty);
            }
        }

        /// <summary>
        /// Where the header sits. <see cref="ChatActivityHeaderPlacement.Top"/> by default: a line
        /// above the rows. <see cref="ChatActivityHeaderPlacement.Bottom"/> draws it as a chip at the
        /// foot of the group with the rows opening upward above it, its caret pointing up while closed.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ChatActivityHeaderPlacement headerPlacement
        {
            get => m_HeaderPlacement;
            set
            {
                var changed = m_HeaderPlacement != value;
                m_HeaderPlacement = value;
                var atFoot = value == ChatActivityHeaderPlacement.Bottom;
                EnableInClassList(headerBottomUssClassName, atFoot);
                m_Summary.RemoveFromHierarchy();
                if (atFoot)
                    hierarchy.Add(m_Summary);
                else
                    hierarchy.Insert(0, m_Summary);
                UpdateCaret();

                if (changed)
                    NotifyPropertyChanged(in headerPlacementProperty);
            }
        }

        /// <summary>
        /// Folds every part of one kind into a single row: one row for the thinking and one per named
        /// tool, each showing its newest part with a pager back through the rest. The rows then sit
        /// in the order they last saw activity. False by default: one row per part, in order. A tool
        /// part with no <see cref="ChatActivityPart.toolName"/> is never folded.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool foldRowsByKind
        {
            get => m_FoldRowsByKind;
            set
            {
                var changed = m_FoldRowsByKind != value;
                m_FoldRowsByKind = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in foldRowsByKindProperty);
            }
        }

        /// <summary>
        /// Lays the rows out as one-line entries and marks the ones that pin at the head and foot of
        /// the scroll area. <see cref="ChatActivityPinEntries.None"/> by default. A group of more than
        /// <see cref="maxPinnedEntries"/> rows pins nothing.
        /// </summary>
        /// <remarks>
        /// UI Toolkit has no sticky positioning, so the pinned entries are marked with
        /// <see cref="rowPinnedHeadUssClassName"/> and <see cref="rowPinnedFootUssClassName"/> but still
        /// scroll with the content.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public ChatActivityPinEntries pinEntries
        {
            get => m_PinEntries;
            set
            {
                var changed = m_PinEntries != value;
                m_PinEntries = value;
                EnableInClassList(entriesUssClassName, value != ChatActivityPinEntries.None);
                MeasurePinBudget();
                ApplyPinClasses();

                if (changed)
                    NotifyPropertyChanged(in pinEntriesProperty);
            }
        }

        /// <summary>
        /// The share of the enclosing <see cref="ScrollView"/> the pinned entries may take at each end,
        /// from 0 to 1. Past it, the entries nearest the middle stop pinning. 1 by default, which
        /// leaves <see cref="maxPinnedEntries"/> as the only limit.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float maxPinnedShare
        {
            get => m_MaxPinnedShare;
            set
            {
                var clamped = float.IsNaN(value) ? 1f : Math.Clamp(value, 0f, 1f);
                var changed = Math.Abs(m_MaxPinnedShare - clamped) > 1e-6f;
                m_MaxPinnedShare = clamped;
                MeasurePinBudget();
                ApplyPinClasses();

                if (changed)
                    NotifyPropertyChanged(in maxPinnedShareProperty);
            }
        }

        /// <summary>
        /// How many entries may pin at each end, before the row count caps it.
        /// </summary>
        internal int pinBudget => m_PinBudget;

        /// <summary>
        /// The names of the tool calls still in flight, newest first. Empty once the activity is over.
        /// </summary>
        public IReadOnlyList<string> activeToolNames
        {
            get
            {
                var names = new List<string>();
                if (isFinalized)
                    return names;
                for (var i = m_Parts.Count - 1; i >= 0; i--)
                {
                    var part = m_Parts[i];
                    if (part.kind == ChatActivityPartKind.Tool && part.pending && !string.IsNullOrEmpty(part.toolName))
                        names.Add(part.toolName);
                }
                return names;
            }
        }

        /// <summary>
        /// How many entries fit in <paramref name="share"/> of a viewport, in whole entries.
        /// </summary>
        /// <param name="viewportHeight"> The scroll area's height. </param>
        /// <param name="entryHeight"> One entry's height. </param>
        /// <param name="share"> The share of the viewport the entries may take. </param>
        /// <returns> The entry budget, or <see cref="maxPinnedEntries"/> when nothing can be measured. </returns>
        internal static int ComputePinBudget(float viewportHeight, float entryHeight, float share)
        {
            if (float.IsNaN(viewportHeight) || float.IsNaN(entryHeight) || entryHeight <= 0f || viewportHeight <= 0f)
                return maxPinnedEntries;
            return Math.Max(0, (int)Math.Floor(viewportHeight * share / entryHeight));
        }

        void MeasurePinBudget()
        {
            var budget = maxPinnedEntries;
            if (m_Open && m_PinEntries != ChatActivityPinEntries.None && m_MaxPinnedShare < 1f && m_Rows.childCount > 0)
            {
                var scroller = GetFirstAncestorOfType<ScrollView>();
                if (scroller != null)
                    budget = ComputePinBudget(scroller.contentViewport.layout.height, m_Rows[0].layout.height, m_MaxPinnedShare);
            }

            if (budget == m_PinBudget)
                return;
            m_PinBudget = budget;
            ApplyPinClasses();
        }

        void ApplyPinClasses()
        {
            var count = m_Rows.childCount;
            var pins = m_PinEntries != ChatActivityPinEntries.None && count <= maxPinnedEntries;
            var budget = Math.Min(count, m_PinBudget);
            for (var i = 0; i < count; i++)
            {
                var row = m_Rows[i];
                row.EnableInClassList(rowPinnedHeadUssClassName,
                    pins && m_PinEntries == ChatActivityPinEntries.HeadAndFoot && i < budget);
                row.EnableInClassList(rowPinnedFootUssClassName, pins && i >= count - budget);
            }
        }

        void UpdateCaret()
        {
            m_Caret.iconName = m_Open
                ? "caret-down"
                : m_HeaderPlacement == ChatActivityHeaderPlacement.Bottom ? "caret-up" : "caret-right";
        }

        /// <summary>
        /// The entries the timeline lists. Assigning re-renders every row.
        /// </summary>
        /// <remarks>
        /// The setter copies the incoming sequence, so a caller mutating its own list afterwards does
        /// not silently desynchronise what is on screen. Null clears the timeline.
        /// </remarks>
        public IEnumerable<ChatActivityPart> parts
        {
            get => m_Parts;
            set
            {
                m_Parts.Clear();
                if (value != null)
                    m_Parts.AddRange(value);
                Refresh();
            }
        }

        /// <summary>
        /// The sum of every measured <see cref="ChatActivityPart.elapsedMs"/>, in milliseconds.
        /// Unmeasured parts contribute nothing.
        /// </summary>
        public int totalElapsedMs
        {
            get
            {
                var total = 0;
                foreach (var part in m_Parts)
                {
                    if (part.elapsedMs > 0)
                        total += part.elapsedMs;
                }
                return total;
            }
        }

        /// <summary>
        /// How many rows the opened timeline shows, including the terminal "Done" row.
        /// </summary>
        public int rowCount => m_Rows.childCount;

        /// <summary>
        /// A row duration badge: anything under a second collapses to "&lt;1s" rather than "0s".
        /// </summary>
        /// <param name="milliseconds"> The duration in milliseconds. </param>
        /// <returns> The badge text. </returns>
        internal static string FormatElapsed(int milliseconds)
        {
            if (milliseconds < 1000)
                return "<1s";

            return Math.Round(milliseconds / 1000d, MidpointRounding.AwayFromZero)
                .ToString("0", CultureInfo.InvariantCulture) + "s";
        }

        /// <summary>
        /// Whether the answer's own text has already started, while the turn is still streaming.
        /// </summary>
        /// <remarks>
        /// Once the model is writing prose, the activity above it is finished even though the turn
        /// is not. Without this the timeline keeps counting while the reader is already reading the
        /// answer.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool isFollowedByText
        {
            get => m_IsFollowedByText;
            set
            {
                var changed = m_IsFollowedByText != value;
                m_IsFollowedByText = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in isFollowedByTextProperty);
            }
        }

        /// <summary>
        /// When the activity started, so the running total survives the element being rebuilt.
        /// </summary>
        /// <remarks>
        /// Unset, the timeline can only add up the parts it was given, which is right for history.
        /// Set, it counts live from the stamp while the activity runs. Same reasoning, and the same
        /// wall-clock currency, as <see cref="ChatStreamingIndicator.startTime"/>.
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
        /// Whether the activity is over: the turn ended, or its prose has already begun.
        /// </summary>
        public bool isFinalized => m_IsFollowedByText || !m_IsStreaming;

        /// <summary>
        /// How many <see cref="ChatActivityPartKind.Thinking"/> parts the timeline holds.
        /// </summary>
        public int thoughtCount
        {
            get
            {
                var count = 0;
                foreach (var part in m_Parts)
                {
                    if (part.kind == ChatActivityPartKind.Thinking)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// How many tool parts the timeline holds.
        /// </summary>
        public int toolCount => m_Parts.Count - thoughtCount;

        /// <summary>
        /// The elapsed time the summary reports: measured from <see cref="startTime"/> while the
        /// activity is still running, and the sum of the parts once it is over.
        /// </summary>
        internal int effectiveElapsedMs
        {
            get
            {
                if (!isFinalized && m_StartTime.HasValue)
                    return (int)(DateTime.UtcNow - m_StartTime.Value).TotalMilliseconds;

                return totalElapsedMs;
            }
        }

        /// <summary>
        /// The collapsed header text: the active verb while the activity runs, and a tally of what
        /// it did once it is over.
        /// </summary>
        internal string SummaryText()
        {
            if (!isFinalized)
            {
                // A running group names the newest call in flight rather than the generic verb.
                var active = activeToolNames;
                var head = (active.Count > 0 ? active[0] : m_Label) + "…";
                // The call count stays on between calls so it does not flicker away; the thought
                // count is left to the finished tally to keep the line short. The elapsed time has
                // its own element.
                var calls = toolCount;
                return calls > 0 ? head + " " + calls + (calls == 1 ? " tool" : " tools") : head;
            }

            var thoughts = thoughtCount;
            var tools = toolCount;
            if (thoughts == 0 && tools == 0)
                return m_Label;

            var parts = new List<string>(2);
            if (thoughts > 0)
                parts.Add(thoughts + (thoughts == 1 ? " thought" : " thoughts"));
            if (tools > 0)
                parts.Add(tools + (tools == 1 ? " tool" : " tools"));
            return string.Join(", ", parts);
        }

        void UpdateTicker()
        {
            // Only a live, unfinished activity needs a clock; history is a fixed sum.
            if (!isFinalized && m_StartTime.HasValue && panel != null)
                m_TickItem ??= schedule.Execute(RefreshSummary).Every(tickIntervalMs);
            else if (m_TickItem != null)
            {
                m_TickItem.Pause();
                m_TickItem = null;
            }
        }

        void RefreshSummary()
        {
            m_SummaryLabel.text = SummaryText();
            // The header sweeps only while the activity is still running.
            m_SummaryLabel.active = !isFinalized;

            var total = effectiveElapsedMs;
            m_SummaryElapsed.text = total > 0 ? FormatElapsed(total) : null;
            m_SummaryElapsed.EnableInClassList(Styles.hiddenUssClassName, total <= 0);

            m_SummaryStats.text = m_Stats;
            m_SummaryStats.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Stats));
        }

        void Refresh()
        {
            RefreshSummary();
            UpdateTicker();

            RefreshRows();
        }

        void RefreshRows()
        {
            m_Rows.Clear();

            foreach (var bucket in BuildBuckets())
                m_Rows.Add(CreateRow(bucket));

            // Only the last row leaves its connector off, so the line never dangles past the timeline.
            for (var i = 0; i < m_Rows.childCount; i++)
                m_Rows[i].EnableInClassList(lastRowUssClassName, i == m_Rows.childCount - 1);

            ApplyPinClasses();
        }

        sealed class RowBucket
        {
            public string key;
            public readonly List<ChatActivityPart> parts = new List<ChatActivityPart>();
            public int last;
        }

        /// <summary>
        /// One bucket per row. Folding puts every part of a kind in one bucket, ordered by where it
        /// last saw activity, so whatever just happened is the row at the bottom.
        /// </summary>
        List<RowBucket> BuildBuckets()
        {
            var buckets = new List<RowBucket>(m_Parts.Count);
            var folded = new Dictionary<string, RowBucket>();
            for (var i = 0; i < m_Parts.Count; i++)
            {
                var part = m_Parts[i];
                string foldKey = null;
                if (m_FoldRowsByKind)
                {
                    if (part.kind == ChatActivityPartKind.Thinking)
                        foldKey = "thinking";
                    else if (!string.IsNullOrEmpty(part.toolName))
                        foldKey = "tool:" + part.toolName;
                }

                if (foldKey != null && folded.TryGetValue(foldKey, out var existing))
                {
                    existing.parts.Add(part);
                    existing.last = i;
                    continue;
                }

                var bucket = new RowBucket { key = foldKey ?? "part:" + i, last = i };
                bucket.parts.Add(part);
                buckets.Add(bucket);
                if (foldKey != null)
                    folded.Add(foldKey, bucket);
            }

            buckets.Sort((a, b) => a.last.CompareTo(b.last));
            return buckets;
        }

        VisualElement CreateRow(RowBucket bucket)
        {
            var first = bucket.parts[0];
            var count = bucket.parts.Count;
            var index = count - 1;
            if (m_PagerIndex.TryGetValue(bucket.key, out var held))
                index = Math.Clamp(held, 0, count - 1);
            var shown = bucket.parts[index];

            // Any call still out keeps the row working: calls of one tool can run at once, and the
            // newest may come back before an older one does.
            var pending = false;
            // Unmeasured unless a part supplied a duration; a folded row reads as the time its parts took.
            var elapsedMs = -1;
            foreach (var part in bucket.parts)
            {
                pending |= part.kind == ChatActivityPartKind.Tool && part.pending;
                if (part.elapsedMs >= 0)
                    elapsedMs = Math.Max(0, elapsedMs) + part.elapsedMs;
            }

            VisualElement content;
            if (first.kind == ChatActivityPartKind.Thinking)
                content = new Icon { iconName = k_ThinkingIconName, size = IconSize.Sm, pickingMode = PickingMode.Ignore };
            else if (pending)
                content = new CircularProgress
                {
                    variant = Progress.Variant.Indeterminate,
                    size = Size.Sm,
                    pickingMode = PickingMode.Ignore,
                };
            else
                content = new Icon { iconName = k_ToolIconName, size = IconSize.Sm, pickingMode = PickingMode.Ignore };

            var row = BuildRow(content, shown.text, elapsedMs);
            if (count > 1)
                row.hierarchy.Add(BuildPager(bucket.key,
                    first.kind == ChatActivityPartKind.Thinking ? "Thought" : "Call", index, count));
            return row;
        }

        /// <summary>
        /// Which part each folded row's pager holds, by row key. A host that rebuilds the timeline
        /// (as <see cref="ChatMessages"/> does while a turn streams) hands the same dictionary to the
        /// new instance, so a reader paging back is not thrown to the newest part on every update.
        /// </summary>
        internal Dictionary<string, int> pagerSelections
        {
            get => m_PagerIndex;
            set
            {
                m_PagerIndex = value ?? new Dictionary<string, int>();
                RefreshRows();
            }
        }

        void WatchPinScroller()
        {
            UnwatchPinScroller();
            m_PinScroller = GetFirstAncestorOfType<ScrollView>();
            // The viewport can resize without this element's own rect changing, and geometry events
            // do not travel down to descendants, so the viewport is watched directly.
            m_PinScroller?.contentViewport.RegisterCallback<GeometryChangedEvent>(OnPinViewportGeometryChanged);
        }

        void UnwatchPinScroller()
        {
            m_PinScroller?.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnPinViewportGeometryChanged);
            m_PinScroller = null;
        }

        void OnPinViewportGeometryChanged(GeometryChangedEvent evt) => MeasurePinBudget();

        void OnSummaryPressed()
        {
            RequestOpen(!open);

            // A header at the foot has the rows open above it, which moves everything already on
            // screen; keeping the header under the eye stops a tall group throwing the reader
            // elsewhere. Checked once the header has moved, which it always does when there are
            // rows to show or hide, and only acted on if it left the view.
            m_KeepHeaderInViewPending = m_HeaderPlacement == ChatActivityHeaderPlacement.Bottom &&
                panel != null && m_Rows.childCount > 0;
        }

        void OnSummaryGeometryChanged(GeometryChangedEvent evt)
        {
            if (!m_KeepHeaderInViewPending)
                return;
            m_KeepHeaderInViewPending = false;
            // Next tick: the scroll area's own range is settled in the same layout pass, possibly
            // after this event, and scrolling against the old range would clamp short.
            schedule.Execute(KeepHeaderInView);
        }

        void KeepHeaderInView()
        {
            var scroller = GetFirstAncestorOfType<ScrollView>();
            if (scroller == null)
                return;

            var view = scroller.contentViewport.worldBound;
            var header = m_Summary.worldBound;
            if (header.yMin >= view.yMin && header.yMax <= view.yMax)
                return;
            scroller.ScrollTo(m_Summary);
        }

        VisualElement BuildPager(string key, string noun, int index, int count)
        {
            var pager = new VisualElement { name = rowPagerUssClassName };
            pager.AddToClassList(rowPagerUssClassName);

            void Select(int next, string buttonName)
            {
                var hadFocus = focusController?.focusedElement is VisualElement focused &&
                    pager.Contains(focused);
                if (next == count - 1)
                    m_PagerIndex.Remove(key);
                else
                    m_PagerIndex[key] = next;
                RefreshRows();

                // The rows were rebuilt, so the pressed button is gone: hand focus to its replacement,
                // or to the other step once this one has reached the end.
                if (!hadFocus)
                    return;
                var newPager = m_Rows.Query(name: rowPagerUssClassName).Where(p => (string)p.userData == key).First();
                var target = newPager?.Q<IconButton>(buttonName);
                if (target != null && !target.enabledSelf)
                    target = newPager.Q<IconButton>(buttonName == "previous" ? "next" : "previous");
                target?.Focus();
            }

            pager.userData = key;

            var previous = new IconButton("caret-left", () => Select(index - 1, "previous"))
            {
                name = "previous",
                quiet = true,
                size = Size.Sm,
                tooltip = "Show previous " + noun.ToLowerInvariant(),
            };
            previous.SetEnabled(index > 0);

            var label = new LocalizedTextElement(noun + " " + (index + 1) + " of " + count)
            {
                name = rowPagerLabelUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            label.AddToClassList(rowPagerLabelUssClassName);

            var next = new IconButton("caret-right", () => Select(index + 1, "next"))
            {
                name = "next",
                quiet = true,
                size = Size.Sm,
                tooltip = "Show next " + noun.ToLowerInvariant(),
            };
            next.SetEnabled(index < count - 1);

            pager.Add(previous);
            pager.Add(label);
            pager.Add(next);
            return pager;
        }

        static VisualElement BuildRow(VisualElement glyphContent, string text, int elapsedMs)
        {
            var row = new VisualElement { name = rowUssClassName, pickingMode = PickingMode.Ignore };
            row.AddToClassList(rowUssClassName);

            var connector = new VisualElement { name = connectorUssClassName, pickingMode = PickingMode.Ignore };
            connector.AddToClassList(connectorUssClassName);

            var glyph = new VisualElement { name = rowGlyphUssClassName, pickingMode = PickingMode.Ignore };
            glyph.AddToClassList(rowGlyphUssClassName);
            glyph.Add(glyphContent);

            var body = new LocalizedTextElement
            {
                name = rowTextUssClassName,
                text = text,
                pickingMode = PickingMode.Ignore,
            };
            body.AddToClassList(rowTextUssClassName);

            row.hierarchy.Add(connector);
            row.hierarchy.Add(glyph);
            row.hierarchy.Add(body);

            if (elapsedMs > 0)
            {
                var badge = new LocalizedTextElement
                {
                    name = rowElapsedUssClassName,
                    text = FormatElapsed(elapsedMs),
                    pickingMode = PickingMode.Ignore,
                };
                badge.AddToClassList(rowElapsedUssClassName);
                row.hierarchy.Add(badge);
            }

            return row;
        }
    }
}
