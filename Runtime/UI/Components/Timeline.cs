using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The wheel and drag interaction scheme of a <see cref="Timeline"/>.
    /// </summary>
    /// <remarks>
    /// The vocabulary mirrors <see cref="CanvasControlScheme"/> exactly, so a screen holding both a
    /// <see cref="Canvas"/> and a <see cref="Timeline"/> can drive them from one user preference.
    /// The Timeline only ever pans and zooms its *time* axis: lane heights, ruler text and glyph
    /// sizes never scale.
    /// </remarks>
    public enum TimelineControlScheme
    {
        /// <summary>
        /// A bare wheel or two-finger swipe pans the time axis; Ctrl/Cmd + wheel zooms about the
        /// cursor. Middle-drag pans.
        /// </summary>
        Modern,

        /// <summary>
        /// The wheel always zooms about the cursor; Alt + drag pans. Middle-drag pans.
        /// </summary>
        Editor,
    }

    /// <summary>
    /// The shape of a <see cref="TimelineKeyframe"/> glyph.
    /// </summary>
    public enum TimelineKeyframeShape
    {
        /// <summary> A square rotated 45 degrees. The animation-editor default. </summary>
        Diamond,

        /// <summary> A circle. </summary>
        Circle,

        /// <summary> An axis-aligned square. </summary>
        Square,
    }

    /// <summary>
    /// The glyph size of a <see cref="TimelineKeyframe"/>.
    /// </summary>
    public enum TimelineKeySize
    {
        /// <summary> Defer: a key defers to the Timeline's <see cref="Timeline.keySize"/>, which defers to small for a muted key and regular otherwise. </summary>
        Auto,

        /// <summary> The small glyph a muted summary key draws by default. </summary>
        Small,

        /// <summary> The regular glyph. </summary>
        Regular,
    }

    /// <summary>
    /// Which edge of a <see cref="TimelineRuler"/> its ticks hang from.
    /// </summary>
    public enum TimelineTicksAt
    {
        /// <summary> The ruler's foot, against the lanes. The default. </summary>
        Bottom,

        /// <summary> The ruler's head, for a ruler flush with the top of the surface it sits in. </summary>
        Top,
    }

    /// <summary>
    /// How a <see cref="TimelineClip"/> draws its span.
    /// </summary>
    public enum TimelineClipVariant
    {
        /// <summary> A filled bar over the span, resizable by its edges. The default. </summary>
        Bar,

        /// <summary>
        /// Only a drag handle at the clip's start, for a track whose lane already shows the span
        /// (<see cref="TimelineTrack.laneStart"/> and <see cref="TimelineTrack.laneEnd"/>).
        /// </summary>
        Handle,

        /// <summary> Nothing drawn: the whole span is an invisible drag surface over the lane. </summary>
        Surface,
    }

    /// <summary>
    /// A closed time range on a <see cref="Timeline"/>'s axis.
    /// </summary>
    public readonly struct TimelineSelection : IEquatable<TimelineSelection>
    {
        /// <summary> The start of the range, in time units. </summary>
        public float start { get; }

        /// <summary> The end of the range, in time units. </summary>
        public float end { get; }

        /// <summary> The length of the range, in time units. Never negative. </summary>
        public float length => Mathf.Max(0f, end - start);

        /// <summary>
        /// Construct a range. The bounds are ordered, so <c>new TimelineSelection(6, 2)</c> and
        /// <c>new TimelineSelection(2, 6)</c> are the same range.
        /// </summary>
        /// <param name="start"> One bound of the range. </param>
        /// <param name="end"> The other bound of the range. </param>
        public TimelineSelection(float start, float end)
        {
            this.start = Mathf.Min(start, end);
            this.end = Mathf.Max(start, end);
        }

        /// <summary> Value equality. </summary>
        /// <param name="other"> The other range. </param>
        /// <returns> True when both bounds match. </returns>
        public bool Equals(TimelineSelection other) =>
            Mathf.Approximately(start, other.start) && Mathf.Approximately(end, other.end);

        /// <summary> Value equality. </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> True when the other object is an equal range. </returns>
        public override bool Equals(object obj) => obj is TimelineSelection o && Equals(o);

        /// <summary> Hash code. </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode() => HashCode.Combine(start, end);

        /// <summary> A readable representation. </summary>
        /// <returns> The range as a string. </returns>
        public override string ToString() =>
            $"[{start.ToString(CultureInfo.InvariantCulture)}, {end.ToString(CultureInfo.InvariantCulture)}]";
    }

    /// <summary>
    /// One tick the ruler and grid draw.
    /// </summary>
    internal readonly struct TimelineTick
    {
        /// <summary> Where the tick sits, in time units. </summary>
        public readonly float time;

        /// <summary> Whether it is a major, labelled tick. </summary>
        public readonly bool major;

        /// <summary> 1 for a minor or major tick, less for one on the fading fine level. </summary>
        public readonly float opacity;

        public TimelineTick(float time, bool major, float opacity)
        {
            this.time = time;
            this.major = major;
            this.opacity = opacity;
        }
    }

    /// <summary>
    /// The time-axis state a <see cref="Timeline"/> publishes to its parts.
    /// </summary>
    /// <remarks>
    /// Every part — the ruler, the grid, tracks, clips, keyframes, markers, the range band and the
    /// playhead — projects time onto pixels through this record rather than reaching for its parent.
    /// It is a record, so an unchanged axis raises no callback and nothing re-lays-out for free.
    /// This is the same shape <see cref="ListBox"/> uses to hand a <c>SizeContext</c> down to its
    /// rows.
    /// </remarks>
    /// <param name="owner"> The Timeline publishing the axis. </param>
    /// <param name="duration"> The total length in time units. </param>
    /// <param name="pxPerUnit"> The effective pixels per time unit, base multiplied by zoom. </param>
    /// <param name="origin"> The left inset before time zero, in pixels. </param>
    /// <param name="value"> The playhead position, in time units. </param>
    /// <param name="step"> The playhead snap increment. Zero means continuous. </param>
    /// <param name="minorTick"> The minor tick spacing, in time units. </param>
    /// <param name="majorTick"> The major (labelled) tick spacing, in time units. </param>
    /// <param name="trackHeight"> The default lane height, in pixels. </param>
    /// <param name="rulerHeight"> The ruler row height, in pixels. </param>
    /// <param name="disabled"> Whether interaction is off. </param>
    /// <param name="selection"> The current range selection, if any. </param>
    /// <param name="laneExtent"> The default lane end, in time units. Negative means the full duration. </param>
    /// <param name="keySize"> The default keyframe size for every track. </param>
    public record TimelineAxisContext(
        Timeline owner,
        float duration,
        float pxPerUnit,
        float origin,
        float value,
        float step,
        float minorTick,
        float majorTick,
        float trackHeight,
        float rulerHeight,
        bool disabled,
        TimelineSelection? selection,
        float laneExtent = -1f,
        TimelineKeySize keySize = TimelineKeySize.Auto) : IContext
    {
        /// <summary> The Timeline publishing the axis. </summary>
        public Timeline owner { get; } = owner;

        /// <summary> The total length in time units. </summary>
        public float duration { get; } = duration;

        /// <summary> The effective pixels per time unit, base multiplied by zoom. </summary>
        public float pxPerUnit { get; } = pxPerUnit;

        /// <summary> The left inset before time zero, in pixels. </summary>
        public float origin { get; } = origin;

        /// <summary> The playhead position, in time units. </summary>
        public float value { get; } = value;

        /// <summary> The playhead snap increment. Zero means continuous. </summary>
        public float step { get; } = step;

        /// <summary> The minor tick spacing, in time units. </summary>
        public float minorTick { get; } = minorTick;

        /// <summary> The major (labelled) tick spacing, in time units. </summary>
        public float majorTick { get; } = majorTick;

        /// <summary> The default lane height, in pixels. </summary>
        public float trackHeight { get; } = trackHeight;

        /// <summary> The ruler row height, in pixels. </summary>
        public float rulerHeight { get; } = rulerHeight;

        /// <summary> Whether interaction is off. </summary>
        public bool disabled { get; } = disabled;

        /// <summary> The current range selection, if any. </summary>
        public TimelineSelection? selection { get; } = selection;

        /// <summary> How far along the axis the lanes are drawn, in time units. Negative reads as the duration. </summary>
        public float laneExtent { get; } = laneExtent < 0f ? duration : laneExtent;

        /// <summary> The glyph size every key defers to. </summary>
        public TimelineKeySize keySize { get; } = keySize;

        /// <summary> Project a time onto an x coordinate inside the strip. </summary>
        /// <param name="t"> The time, in time units. </param>
        /// <returns> The x coordinate, in pixels, relative to the strip's left edge. </returns>
        public float TimeToX(float t) => Timeline.Project(t, origin, pxPerUnit);

        /// <summary> Project an x coordinate inside the strip back onto a time. </summary>
        /// <param name="x"> The x coordinate, in pixels, relative to the strip's left edge. </param>
        /// <returns> The time, in time units. </returns>
        public float XToTime(float x) => Timeline.Unproject(x, origin, pxPerUnit);
    }

    /// <summary>
    /// A horizontal time ruler with a draggable playhead and optional lanes of clips and keyframes.
    /// </summary>
    /// <remarks>
    /// Timeline is a compound component. On its own it draws a ruler, a frame grid and a playhead
    /// over an empty strip; the lanes come from the <see cref="TimelineTrack"/> and
    /// <see cref="TimelineTrackGroup"/> children you add to it. Inside a track,
    /// <see cref="TimelineClip"/> and <see cref="TimelineKeyframe"/> place themselves on the shared
    /// time axis. <see cref="TimelineMarker"/>s are added through <see cref="AddMarker"/> and float
    /// above every lane.
    ///
    /// **The time axis.** <see cref="pxPerUnit"/> is the base scale and <see cref="zoom"/> is a
    /// multiplier on top of it; <see cref="effectivePxPerUnit"/> is their clamped product, and every
    /// part projects time through <see cref="TimeToX"/>. A fixed <see cref="origin"/> inset sits
    /// before time zero and a fixed <see cref="endPadding"/> after the last frame, so the strip is
    /// <see cref="stripWidth"/> wide. Panning translates the strip inside a clipping viewport rather
    /// than scrolling it, which keeps a zoom pivoted exactly on the cursor even under a burst of
    /// wheel events.
    ///
    /// **Layout constants come from the static sizing ramp.** <see cref="origin"/>,
    /// <see cref="endPadding"/>, <see cref="defaultTrackHeight"/> and <see cref="defaultRulerHeight"/>
    /// are C# constants that also appear in `Timeline.uss`. They are bound to
    /// `--appui-static-sizing-*`, which no density class overrides, so the arithmetic and the
    /// rendering cannot drift apart under `.appui--compact`.
    ///
    /// **Interaction.** <see cref="controlScheme"/> mirrors <see cref="CanvasControlScheme"/>: under
    /// <see cref="TimelineControlScheme.Modern"/> a bare wheel pans and Ctrl/Cmd + wheel zooms, under
    /// <see cref="TimelineControlScheme.Editor"/> the wheel always zooms and Alt + drag pans.
    /// Middle-drag pans under either. Dragging the ruler seeks; Shift + dragging it draws a range
    /// selection. The playhead is focusable and answers the arrow keys, Home and End.
    /// </remarks>
    /// <example>
    /// <para>A dope sheet built in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:Timeline duration="60" label-width="120" px-per-unit="12">
    ///     <appui:TimelineTrack id="position" label="Position">
    ///         <appui:TimelineClip start="4" duration="12" label="Walk" />
    ///         <appui:TimelineKeyframe time="24" />
    ///     </appui:TimelineTrack>
    ///     <appui:TimelineTrackGroup id="cube" title="Cube">
    ///         <appui:TimelineTrack id="rotation" label="Rotation" />
    ///     </appui:TimelineTrackGroup>
    /// </appui:Timeline>
    /// ]]></code>
    /// <para>The same thing in C#, with a marker and a seek handler.</para>
    /// <code lang="csharp"><![CDATA[
    /// var timeline = new Timeline { duration = 60f, labelWidth = 120f };
    ///
    /// var track = new TimelineTrack { id = "position", label = "Position" };
    /// track.Add(new TimelineClip { start = 4f, duration = 12f, label = "Walk" });
    /// timeline.Add(track);
    ///
    /// timeline.AddMarker(new TimelineMarker { time = 30f, label = "Cut" });
    /// timeline.valueChanged += t => player.time = t;
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("data-display", isNew = true)]
    public partial class Timeline : BaseVisualElement
    {
        internal static readonly BindingId durationProperty = nameof(duration);
        internal static readonly BindingId valueProperty = nameof(value);
        internal static readonly BindingId pxPerUnitProperty = nameof(pxPerUnit);
        internal static readonly BindingId zoomProperty = nameof(zoom);
        internal static readonly BindingId stepProperty = nameof(step);
        internal static readonly BindingId minorTickProperty = nameof(minorTick);
        internal static readonly BindingId majorTickProperty = nameof(majorTick);
        internal static readonly BindingId trackHeightProperty = nameof(trackHeight);
        internal static readonly BindingId rulerHeightProperty = nameof(rulerHeight);
        internal static readonly BindingId labelWidthProperty = nameof(labelWidth);
        internal static readonly BindingId disabledProperty = nameof(disabled);
        internal static readonly BindingId controlSchemeProperty = nameof(controlScheme);
        internal static readonly BindingId showFrameLinesProperty = nameof(showFrameLines);
        internal static readonly BindingId infiniteProperty = nameof(infinite);
        internal static readonly BindingId minPxPerUnitProperty = nameof(minPxPerUnit);
        internal static readonly BindingId maxPxPerUnitProperty = nameof(maxPxPerUnit);
        internal static readonly BindingId fitWidthProperty = nameof(fitWidth);
        internal static readonly BindingId fitDurationProperty = nameof(fitDuration);
        internal static readonly BindingId adaptiveTicksProperty = nameof(adaptiveTicks);
        internal static readonly BindingId labelResizableProperty = nameof(labelResizable);
        internal static readonly BindingId minLabelWidthProperty = nameof(minLabelWidth);
        internal static readonly BindingId hoverLineProperty = nameof(hoverLine);
        internal static readonly BindingId timeSuffixProperty = nameof(timeSuffix);
        internal static readonly BindingId laneExtentProperty = nameof(laneExtent);
        internal static readonly BindingId keySizeProperty = nameof(keySize);
        internal static readonly BindingId minTickStepProperty = nameof(minTickStep);
        internal static readonly BindingId ticksAtProperty = nameof(ticksAt);

        /// <summary> The Timeline main styling class. </summary>
        public const string ussClassName = "appui-timeline";

        /// <summary> The styling class of the row holding the gutter and the viewport. </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary> The styling class of the left gutter that shows lane labels. </summary>
        public const string gutterUssClassName = ussClassName + "__gutter";

        /// <summary> The styling class of the gutter spacer that lines up with the ruler. </summary>
        public const string gutterSpacerUssClassName = ussClassName + "__gutter-spacer";

        /// <summary> The styling class of one gutter row, mirroring one <see cref="TimelineTrack"/>. </summary>
        public const string gutterRowUssClassName = ussClassName + "__gutter-row";

        /// <summary> The styling class of a gutter row belonging to a <see cref="TimelineTrackGroup"/> header. </summary>
        public const string gutterGroupUssClassName = ussClassName + "__gutter-group";

        /// <summary> The styling class of the gutter row label. </summary>
        public const string gutterLabelUssClassName = ussClassName + "__gutter-label";

        /// <summary> The styling class of a gutter group's expand chevron. </summary>
        public const string gutterChevronUssClassName = ussClassName + "__gutter-chevron";

        /// <summary> The styling class of the clipping viewport. </summary>
        public const string viewportUssClassName = ussClassName + "__viewport";

        /// <summary> The styling class of the pannable inner strip. </summary>
        public const string stripUssClassName = ussClassName + "__strip";

        /// <summary> The styling class of the container holding the lanes. </summary>
        public const string lanesUssClassName = ussClassName + "__lanes";

        /// <summary> The styling class of the layer holding the <see cref="TimelineMarker"/>s. </summary>
        public const string markersUssClassName = ussClassName + "__markers";

        /// <summary> The styling class applied while the Timeline is <see cref="disabled"/>. </summary>
        public new const string disabledUssClassName = ussClassName + "--disabled";

        /// <summary> The styling class of the overlay horizontal scrollbar. </summary>
        public const string scrollbarUssClassName = ussClassName + "__scrollbar";

        /// <summary> The gap the playhead keeps from the viewport edge when it pans into view, at most, in pixels. </summary>
        public const float revealMargin = 48f;

        /// <summary> The styling class of the gutter's resize sash. </summary>
        public const string sashUssClassName = ussClassName + "__sash";

        /// <summary> The styling class of the guide line drawn at the pointer. </summary>
        public const string hoverLineUssClassName = ussClassName + "__hover-line";

        /// <summary> The styling class of the time shown in the gutter's corner. </summary>
        public const string cornerTimeUssClassName = ussClassName + "__corner-time";

        /// <summary> The styling class of the fixed text after the corner time. </summary>
        public const string cornerSuffixUssClassName = ussClassName + "__corner-suffix";

        /// <summary> The narrowest the lanes get when the gutter is widened, in pixels. </summary>
        public const float minLanesWidth = 120f;

        /// <summary> The default <see cref="minLabelWidth"/>, in pixels. </summary>
        public const float defaultMinLabelWidth = 96f;

        /// <summary> How far one arrow key moves the gutter sash, in pixels. </summary>
        public const float sashKeyStep = 8f;

        /// <summary> The left inset before time zero, in pixels. Mirrors `--appui-static-sizing-200`. </summary>
        public const float origin = 16f;

        /// <summary> The padding after the last frame, in pixels. Mirrors `--appui-static-sizing-300`. </summary>
        public const float endPadding = 24f;

        /// <summary> The default lane height, in pixels. Mirrors `--appui-static-sizing-400`. </summary>
        public const float defaultTrackHeight = 32f;

        /// <summary> The default ruler row height, in pixels. Mirrors `--appui-static-sizing-300`. </summary>
        public const float defaultRulerHeight = 24f;

        /// <summary> The default lower bound on <see cref="effectivePxPerUnit"/>. </summary>
        public const float minEffectivePxPerUnit = 2f;

        /// <summary> The default upper bound on <see cref="effectivePxPerUnit"/>. </summary>
        public const float maxEffectivePxPerUnit = 80f;

        /// <summary>
        /// The smallest gap, in pixels, adaptive ticks will leave between two minor ticks.
        /// </summary>
        internal const float minAdaptiveMinorPx = 8f;

        /// <summary>
        /// The smallest gap, in pixels, adaptive ticks will leave between two labelled ticks. Wider
        /// than the minor budget because a label needs room the tick itself does not.
        /// </summary>
        internal const float minAdaptiveMajorPx = 64f;

        /// <summary>
        /// The most ticks the ruler and grid will draw, fine level included. A pathological
        /// minorTick would otherwise generate a runaway element count.
        /// </summary>
        internal const int maxTicks = 10000;

        /// <summary>
        /// The local custom property <c>Timeline.uss</c> republishes the theme's
        /// <c>--appui-timeline-tick-fine-opacity</c> under. UI Toolkit only surfaces a custom property
        /// a matched rule declares on the element, so the <c>:root</c> token cannot be read directly.
        /// </summary>
        static readonly CustomStyleProperty<float> k_TickFineOpacityProperty =
            new CustomStyleProperty<float>("--timeline-tick-fine-opacity");

        float m_TickFineOpacity;

        /// <summary> The default base scale, in pixels per time unit. </summary>
        public const float defaultPxPerUnit = 8f;

        const float k_ZoomSpeed = 0.0015f;

        const float k_PanSpeed = 2f;

        readonly VisualElement m_Body;
        readonly VisualElement m_Gutter;
        readonly VisualElement m_GutterSpacer;
        readonly VisualElement m_GutterRows;
        readonly VisualElement m_Viewport;
        readonly VisualElement m_Strip;
        readonly VisualElement m_Lanes;
        readonly VisualElement m_Markers;
        readonly TimelineGrid m_Grid;
        readonly TimelineRuler m_Ruler;
        readonly TimelineRange m_Range;
        readonly TimelinePlayhead m_Playhead;

        float m_Duration;
        float m_Value;
        float m_PxPerUnit = defaultPxPerUnit;
        float m_Zoom = 1f;

        float m_MinPxPerUnit = minEffectivePxPerUnit;

        float m_MaxPxPerUnit = maxEffectivePxPerUnit;

        bool m_FitWidth;

        float m_FitDuration;

        bool m_AdaptiveTicks;
        float m_Step = 1f;
        float m_MinorTick = 1f;
        float m_MajorTick = 5f;
        float m_TrackHeight = defaultTrackHeight;
        float m_RulerHeight = defaultRulerHeight;
        float m_LabelWidth;
        float m_Offset;
        bool m_Disabled;
        bool m_ShowFrameLines = true;
        bool m_Infinite;
        TimelineControlScheme m_ControlScheme = TimelineControlScheme.Modern;
        TimelineSelection? m_Selection;
        Func<float, string> m_FormatTime;

        int m_PanPointerId = PointerId.invalidPointerId;
        float m_PanStartX;
        float m_PanStartOffset;
        bool m_SpaceBarPressed;

        readonly Scroller m_Scrollbar;
        bool m_SyncingScrollbar;
        readonly VisualElement m_Sash;
        readonly VisualElement m_HoverLine;
        readonly LocalizedTextElement m_CornerTime;
        readonly LocalizedTextElement m_CornerSuffix;

        bool m_LabelResizable;
        float m_MinLabelWidth = defaultMinLabelWidth;
        bool m_HoverLineEnabled;
        Func<float, string> m_TimeLabel;
        string m_TimeSuffix;
        bool m_Hovering;
        float m_HoverX;
        float m_LaneExtent = -1f;
        TimelineKeySize m_KeySize = TimelineKeySize.Auto;
        float m_MinTickStep;
        TimelineTicksAt m_TicksAt = TimelineTicksAt.Bottom;
        readonly List<TimelineHandle> m_Handles = new List<TimelineHandle>();
        int m_SashPointerId = PointerId.invalidPointerId;
        float m_SashStartX;
        float m_SashStartWidth;

        /// <summary> Raised whenever the playhead moves. Carries the new time. </summary>
        public event Action<float> valueChanged;

        /// <summary>
        /// Raised whenever the gutter sash resizes the gutter, live during a drag and once per key
        /// press. Carries the new <see cref="labelWidth"/>.
        /// </summary>
        public event Action<float> labelWidthChanged;

        /// <summary> Raised whenever the range selection changes. Carries `null` when it is cleared. </summary>
        public event Action<TimelineSelection?> selectionChanged;

        /// <summary> Raised whenever the time axis changes: scale, zoom or pan. </summary>
        public event Action axisChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Timeline()
        {
            AddToClassList(ussClassName);
            focusable = false;

            m_Body = new VisualElement { name = bodyUssClassName, pickingMode = PickingMode.Ignore };
            m_Body.AddToClassList(bodyUssClassName);
            hierarchy.Add(m_Body);

            m_Gutter = new VisualElement { name = gutterUssClassName };
            m_Gutter.AddToClassList(gutterUssClassName);
            // labelWidth defaults to 0, and its setter early-returns on an unchanged value, so the
            // collapsed state has to be established here or the gutter would render at 0 width
            // while still laying out (and still drawing its right border).
            m_Gutter.AddToClassList(Styles.hiddenUssClassName);
            m_Gutter.style.width = 0f;
            m_Body.Add(m_Gutter);

            m_GutterSpacer = new VisualElement { name = gutterSpacerUssClassName, pickingMode = PickingMode.Ignore };
            m_GutterSpacer.AddToClassList(gutterSpacerUssClassName);
            m_Gutter.Add(m_GutterSpacer);

            m_CornerTime = new LocalizedTextElement { name = cornerTimeUssClassName, pickingMode = PickingMode.Ignore };
            m_CornerTime.AddToClassList(cornerTimeUssClassName);
            m_CornerTime.AddToClassList(Styles.hiddenUssClassName);
            m_GutterSpacer.Add(m_CornerTime);

            m_CornerSuffix = new LocalizedTextElement { name = cornerSuffixUssClassName, pickingMode = PickingMode.Ignore };
            m_CornerSuffix.AddToClassList(cornerSuffixUssClassName);
            m_CornerSuffix.AddToClassList(Styles.hiddenUssClassName);
            m_GutterSpacer.Add(m_CornerSuffix);

            m_GutterRows = new VisualElement { name = gutterUssClassName + "-rows" };
            m_Gutter.Add(m_GutterRows);

            // Last in the gutter, so it sits over the rows and the corner and takes the pointer
            // along the whole edge.
            m_Sash = new VisualElement { name = sashUssClassName, focusable = false };
            m_Sash.AddToClassList(sashUssClassName);
            m_Sash.AddToClassList(Styles.hiddenUssClassName);
            m_Gutter.Add(m_Sash);
            m_Sash.RegisterCallback<PointerDownEvent>(OnSashPointerDown);
            m_Sash.RegisterCallback<PointerMoveEvent>(OnSashPointerMove);
            m_Sash.RegisterCallback<PointerUpEvent>(evt => EndSashDrag(evt.pointerId));
            m_Sash.RegisterCallback<PointerCancelEvent>(evt => EndSashDrag(evt.pointerId));
            m_Sash.RegisterCallback<KeyDownEvent>(OnSashKeyDown);

            m_Viewport = new VisualElement { name = viewportUssClassName };
            m_Viewport.AddToClassList(viewportUssClassName);
            m_Body.Add(m_Viewport);

            m_Strip = new VisualElement { name = stripUssClassName };
            m_Strip.AddToClassList(stripUssClassName);
            m_Viewport.Add(m_Strip);

            m_Grid = new TimelineGrid();
            m_Strip.Add(m_Grid);

            m_Ruler = new TimelineRuler();
            m_Strip.Add(m_Ruler);

            m_Lanes = new VisualElement { name = lanesUssClassName, pickingMode = PickingMode.Ignore };
            m_Lanes.AddToClassList(lanesUssClassName);
            m_Strip.Add(m_Lanes);

            m_Markers = new VisualElement { name = markersUssClassName, pickingMode = PickingMode.Ignore };
            m_Markers.AddToClassList(markersUssClassName);
            m_Strip.Add(m_Markers);

            m_Range = new TimelineRange();
            m_Strip.Add(m_Range);

            m_Playhead = new TimelinePlayhead();
            m_Strip.Add(m_Playhead);

            // In the viewport rather than the strip: it follows the pointer, not the time axis.
            m_HoverLine = new VisualElement { name = hoverLineUssClassName, pickingMode = PickingMode.Ignore };
            m_HoverLine.AddToClassList(hoverLineUssClassName);
            m_HoverLine.AddToClassList(Styles.hiddenUssClassName);
            m_Viewport.Add(m_HoverLine);

            // An overlay on the viewport's bottom edge rather than a row of its own: a stock
            // Scroller, so ScrollView.uss draws it the way every other App UI scrollbar looks.
            m_Scrollbar = new Scroller(0f, 0f, OnScrollbarValueChanged, SliderDirection.Horizontal)
            {
                name = scrollbarUssClassName,
                focusable = true,
            };
            m_Scrollbar.AddToClassList(scrollbarUssClassName);
            m_Scrollbar.AddToClassList(Styles.hiddenUssClassName);
            // The bar itself takes focus and the keys, so the inner slider's own key handling
            // cannot move the pan a second time. A click on the track jumps there.
            m_Scrollbar.slider.focusable = false;
            m_Scrollbar.slider.pageSize = 0f;
            // The stock slider's track-click behaviour differs between Unity versions (pageSize 0
            // jumps on 6000.3 but not on trunk), so the jump is done here. Trickle-down on the
            // slider runs before its ClampedDragger, which still drives a drag from the thumb.
            m_Scrollbar.slider.RegisterCallback<PointerDownEvent>(OnScrollbarTrackPointerDown, TrickleDown.TrickleDown);
            m_Scrollbar.RegisterCallback<KeyDownEvent>(OnScrollbarKeyDown);
            // Adjust sizes the thumb from the track's laid-out width, which a just-shown bar does
            // not have yet; size it again once it does.
            m_Scrollbar.RegisterCallback<GeometryChangedEvent>(_ => RefreshScrollbar());
            m_Viewport.Add(m_Scrollbar);

            // Trickle-down, so a part that stops a move cannot hide it. A part dragging with the
            // pointer captured gets the moves alone, so it forwards them through TrackPointer.
            m_Viewport.RegisterCallback<PointerMoveEvent>(OnViewportHover, TrickleDown.TrickleDown);
            m_Viewport.RegisterCallback<PointerLeaveEvent>(OnViewportLeave);
            m_Body.RegisterCallback<GeometryChangedEvent>(_ => ApplyLabelWidth());

            m_Viewport.RegisterCallback<WheelEvent>(OnWheel);
            m_Viewport.RegisterCallback<PointerDownEvent>(OnViewportPointerDown);
            m_Viewport.RegisterCallback<PointerMoveEvent>(OnViewportPointerMove);
            m_Viewport.RegisterCallback<PointerUpEvent>(OnViewportPointerUp);
            m_Viewport.RegisterCallback<PointerCancelEvent>(OnViewportPointerCancel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<KeyUpEvent>(OnKeyUp);
            RegisterCallback<AttachToPanelEvent>(_ => RefreshAxis());
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            m_Lanes.RegisterCallback<GeometryChangedEvent>(_ => RefreshGutter());

            // A fitted axis is derived from the viewport width, so it has to be recomputed
            // whenever that width changes rather than only when a property is assigned.
            m_Viewport.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (m_FitWidth)
                {
                    RefreshAxis();
                    return;
                }

                // A wider viewport shrinks the pannable range under the current pan.
                offset = m_Offset;
                RefreshScrollbar();
            });

            RefreshAxis();
        }

        /// <summary>
        /// Lanes are added here, so `timeline.Add(track)` puts the track where it belongs.
        /// </summary>
        public override VisualElement contentContainer => m_Lanes;

        /// <summary> The clipping viewport that the strip pans inside. </summary>
        public VisualElement viewport => m_Viewport;

        /// <summary> The pannable inner strip that holds every part. </summary>
        public VisualElement strip => m_Strip;

        /// <summary> The left gutter that shows lane labels. </summary>
        public VisualElement gutter => m_Gutter;

        /// <summary> The built-in time ruler. </summary>
        public TimelineRuler ruler => m_Ruler;

        /// <summary> The built-in frame grid. </summary>
        public TimelineGrid grid => m_Grid;

        /// <summary> The built-in range selection band. </summary>
        public TimelineRange range => m_Range;

        /// <summary> The built-in playhead scrubber. </summary>
        public TimelinePlayhead playhead => m_Playhead;

        /// <summary> The number of lanes — <see cref="TimelineTrack"/>s and <see cref="TimelineTrackGroup"/>s. </summary>
        public int trackCount => m_Lanes.childCount;

        /// <summary>
        /// The total length of the timeline, in time units.
        /// </summary>
        /// <remarks>
        /// A non-finite value (NaN or infinity, say from a field that parsed <c>1e309</c>) has no
        /// extent to lay out, pan over or scroll through, so it reads as an empty axis, the way the
        /// registry treats it.
        /// </remarks>
        [Tooltip("The total length of the timeline, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float duration
        {
            get => m_Duration;
            set
            {
                var v = float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
                if (Mathf.Approximately(m_Duration, v))
                    return;
                m_Duration = v;
                this.value = m_Value;
                RefreshAxis();
                NotifyPropertyChanged(in durationProperty);
            }
        }

        /// <summary>
        /// The playhead position, in time units.
        /// </summary>
        /// <remarks>
        /// Assigning is clamped to <c>[0, duration]</c> and snapped to <see cref="step"/>, so what you
        /// read back is always a position the playhead can actually occupy.
        /// </remarks>
        [Tooltip("The playhead position, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float value
        {
            get => m_Value;
            set
            {
                var v = SnapTime(value);
                if (Mathf.Approximately(m_Value, v))
                    return;
                m_Value = v;
                RefreshAxis();
                valueChanged?.Invoke(v);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The base horizontal scale, in pixels per time unit. <see cref="zoom"/> multiplies it.
        /// </summary>
        [Tooltip("The base horizontal scale, in pixels per time unit.")]
        [CreateProperty]
        [UxmlAttribute]
        public float pxPerUnit
        {
            get => m_PxPerUnit;
            set
            {
                var v = Mathf.Max(0.0001f, value);
                if (Mathf.Approximately(m_PxPerUnit, v))
                    return;
                m_PxPerUnit = v;
                RefreshAxis();
                NotifyPropertyChanged(in pxPerUnitProperty);
            }
        }

        /// <summary>
        /// The zoom multiplier applied on top of <see cref="pxPerUnit"/>.
        /// </summary>
        /// <remarks>
        /// The product is clamped between <see cref="minEffectivePxPerUnit"/> and
        /// <see cref="maxEffectivePxPerUnit"/>, so a very large or very small zoom saturates
        /// rather than collapsing the axis. Read <see cref="effectivePxPerUnit"/> for the value the
        /// parts actually project through.
        /// </remarks>
        [Tooltip("The zoom multiplier applied on top of pxPerUnit.")]
        [CreateProperty]
        [UxmlAttribute]
        public float zoom
        {
            get => m_Zoom;
            set
            {
                var v = Mathf.Max(0.0001f, value);
                if (Mathf.Approximately(m_Zoom, v))
                    return;
                m_Zoom = v;
                RefreshAxis();
                NotifyPropertyChanged(in zoomProperty);
            }
        }

        /// <summary>
        /// The pixels per time unit every part actually projects through: <see cref="pxPerUnit"/>
        /// multiplied by <see cref="zoom"/>, clamped to the zoom range.
        /// </summary>
        public float effectivePxPerUnit =>
            Mathf.Clamp(EffectiveBasePxPerUnit() * m_Zoom, ResolvedMinPxPerUnit(), ResolvedMaxPxPerUnit());

        /// <summary>
        /// The full width of the strip: the origin inset, the projected duration and the end padding.
        /// </summary>
        public float stripWidth => origin + m_Duration * effectivePxPerUnit + endPadding;

        /// <summary>
        /// The pan offset, in pixels. The strip is translated by its negation inside the viewport.
        /// </summary>
        public float offset
        {
            get => m_Offset;
            set
            {
                var max = m_Infinite
                    ? float.MaxValue
                    : Mathf.Max(0f, stripWidth - m_Viewport.resolvedStyle.width);
                var v = Mathf.Clamp(value, 0f, max);
                if (Mathf.Approximately(m_Offset, v))
                    return;
                m_Offset = v;
                m_Strip.style.translate = new Translate(-v, 0f);
                RefreshScrollbar();
                // A wheel pan under a still pointer moves the time beneath it.
                RefreshHover();
                axisChanged?.Invoke();
            }
        }

        /// <summary>
        /// The playhead snap increment, in time units. Zero seeks continuously.
        /// </summary>
        [Tooltip("The playhead snap increment, in time units. Zero seeks continuously.")]
        [CreateProperty]
        [UxmlAttribute]
        public float step
        {
            get => m_Step;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_Step, v))
                    return;
                m_Step = v;
                RefreshAxis();
                NotifyPropertyChanged(in stepProperty);
            }
        }

        /// <summary>
        /// The spacing between minor ruler ticks, in time units.
        /// </summary>
        [Tooltip("The spacing between minor ruler ticks, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minorTick
        {
            get => m_MinorTick;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_MinorTick, v))
                    return;
                m_MinorTick = v;
                RefreshAxis();
                NotifyPropertyChanged(in minorTickProperty);
            }
        }

        /// <summary>
        /// The spacing between major, labelled ruler ticks, in time units.
        /// </summary>
        [Tooltip("The spacing between major, labelled ruler ticks, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float majorTick
        {
            get => m_MajorTick;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_MajorTick, v))
                    return;
                m_MajorTick = v;
                RefreshAxis();
                NotifyPropertyChanged(in majorTickProperty);
            }
        }

        /// <summary>
        /// The default lane height, in pixels. A <see cref="TimelineTrack"/> can override it.
        /// </summary>
        [Tooltip("The default lane height, in pixels.")]
        [CreateProperty]
        [UxmlAttribute]
        public float trackHeight
        {
            get => m_TrackHeight;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_TrackHeight, v))
                    return;
                m_TrackHeight = v;
                RefreshAxis();
                NotifyPropertyChanged(in trackHeightProperty);
            }
        }

        /// <summary>
        /// The ruler row height, in pixels.
        /// </summary>
        [Tooltip("The ruler row height, in pixels.")]
        [CreateProperty]
        [UxmlAttribute]
        public float rulerHeight
        {
            get => m_RulerHeight;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_RulerHeight, v))
                    return;
                m_RulerHeight = v;
                RefreshAxis();
                NotifyPropertyChanged(in rulerHeightProperty);
            }
        }

        /// <summary>
        /// The width of the left label gutter, in pixels. Zero, the default, hides the gutter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the width asked for. The gutter renders at <see cref="effectiveLabelWidth"/>,
        /// which keeps at least <see cref="minLanesWidth"/> pixels for the lanes, so a width stored
        /// from a wider panel cannot swallow the lanes in a narrower one. With
        /// <see cref="labelResizable"/> on, it is also never under <see cref="minLabelWidth"/>.
        /// </para>
        /// </remarks>
        [Tooltip("The width of the left label gutter, in pixels. Zero hides it.")]
        [CreateProperty]
        [UxmlAttribute]
        public float labelWidth
        {
            get => m_LabelWidth;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_LabelWidth, v))
                    return;
                m_LabelWidth = v;
                ApplyLabelWidth();
                RefreshGutter();
                NotifyPropertyChanged(in labelWidthProperty);
            }
        }

        /// <summary>
        /// Whether the gutter has a sash on its right edge that resizes it. False by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dragging the sash, or pressing the left and right arrows on it (by
        /// <see cref="sashKeyStep"/>) and Home (to <see cref="minLabelWidth"/>), assigns
        /// <see cref="labelWidth"/> and raises <see cref="labelWidthChanged"/>, so a host that stores
        /// the width can persist it from there. The sash answers the primary button only.
        /// </para>
        /// </remarks>
        [Tooltip("Whether the gutter has a sash on its right edge that resizes it.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool labelResizable
        {
            get => m_LabelResizable;
            set
            {
                if (m_LabelResizable == value)
                    return;
                m_LabelResizable = value;
                if (!value)
                    EndSashDrag(m_SashPointerId);
                ApplyLabelWidth();
                NotifyPropertyChanged(in labelResizableProperty);
            }
        }

        /// <summary>
        /// The narrowest the sash can drag the gutter to, in pixels. With
        /// <see cref="labelResizable"/> on, a narrower positive <see cref="labelWidth"/> also renders
        /// at this width. Defaults to <see cref="defaultMinLabelWidth"/>.
        /// </summary>
        [Tooltip("The narrowest the sash can drag the gutter to, in pixels.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minLabelWidth
        {
            get => m_MinLabelWidth;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_MinLabelWidth, v))
                    return;
                m_MinLabelWidth = v;
                ApplyLabelWidth();
                NotifyPropertyChanged(in minLabelWidthProperty);
            }
        }

        /// <summary>
        /// The widest the gutter may be: what leaves <see cref="minLanesWidth"/> for the lanes, never
        /// under <see cref="minLabelWidth"/>. Unbounded until the Timeline has been laid out.
        /// </summary>
        public float maxLabelWidth
        {
            get
            {
                var root = m_Body?.layout.width ?? float.NaN;
                return float.IsNaN(root) || root <= 0f
                    ? float.PositiveInfinity
                    : Mathf.Max(m_MinLabelWidth, root - minLanesWidth);
            }
        }

        /// <summary>
        /// The width the gutter actually renders at: <see cref="labelWidth"/>, raised to
        /// <see cref="minLabelWidth"/> when <see cref="labelResizable"/> is on, and capped at
        /// <see cref="maxLabelWidth"/>. Zero while the gutter is hidden.
        /// </summary>
        public float effectiveLabelWidth
        {
            get
            {
                if (m_LabelWidth <= 0f)
                    return 0f;
                var w = m_LabelWidth;
                if (m_LabelResizable)
                    w = Mathf.Max(w, m_MinLabelWidth);
                return Mathf.Min(w, maxLabelWidth);
            }
        }

        /// <summary> The sash on the gutter's right edge. Hidden unless <see cref="labelResizable"/>. </summary>
        public VisualElement sash => m_Sash;

        /// <summary>
        /// Whether a hairline is drawn through the ruler and the lanes at the pointer while it is
        /// over the timeline. False by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A guide to read a position against, for a timeline that draws no grid lines. Over the
        /// origin inset it holds at time zero and, on a finite axis, past the end it holds at the
        /// end.
        /// </para>
        /// </remarks>
        [Tooltip("Whether a hairline is drawn at the pointer while it is over the timeline.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool hoverLine
        {
            get => m_HoverLineEnabled;
            set
            {
                if (m_HoverLineEnabled == value)
                    return;
                m_HoverLineEnabled = value;
                if (!value)
                    m_HoverLine.AddToClassList(Styles.hiddenUssClassName);
                NotifyPropertyChanged(in hoverLineProperty);
            }
        }

        /// <summary> The hover guide line inside the viewport. </summary>
        public VisualElement hoverLineElement => m_HoverLine;

        /// <summary>
        /// The formatter for the time shown in the gutter's corner, above the labels. `null`, the
        /// default, shows no time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While the pointer is over the timeline the corner shows the pointer's time, snapped to
        /// <see cref="step"/> like the time a click there seeks to; otherwise the playhead's. The
        /// corner is the gutter's, so it needs a positive <see cref="labelWidth"/> to show.
        /// </para>
        /// </remarks>
        public Func<float, string> timeLabel
        {
            get => m_TimeLabel;
            set
            {
                m_TimeLabel = value;
                m_CornerTime.EnableInClassList(Styles.hiddenUssClassName, value == null);
                RefreshCornerTime();
            }
        }

        /// <summary>
        /// Fixed text drawn after the corner time, the length it is read against for instance
        /// (<c>/ 2.00s</c>). Shown only with a <see cref="timeLabel"/>.
        /// </summary>
        [Tooltip("Fixed text drawn after the corner time.")]
        [CreateProperty]
        [UxmlAttribute]
        public string timeSuffix
        {
            get => m_TimeSuffix;
            set
            {
                if (m_TimeSuffix == value)
                    return;
                m_TimeSuffix = value;
                m_CornerSuffix.text = value;
                RefreshCornerTime();
                NotifyPropertyChanged(in timeSuffixProperty);
            }
        }

        /// <summary> The time shown in the gutter's corner. </summary>
        public LocalizedTextElement cornerTime => m_CornerTime;

        void ApplyLabelWidth()
        {
            if (m_Gutter == null)
                return;
            var w = effectiveLabelWidth;
            m_Gutter.style.width = w;
            m_Gutter.EnableInClassList(Styles.hiddenUssClassName, w <= 0f);
            m_Sash.EnableInClassList(Styles.hiddenUssClassName, !m_LabelResizable);
            m_Sash.focusable = m_LabelResizable && !m_Disabled;
        }

        void CommitSashWidth(float width)
        {
            var next = Mathf.Round(Mathf.Min(maxLabelWidth, Mathf.Max(m_MinLabelWidth, width)));
            labelWidth = next;
            labelWidthChanged?.Invoke(next);
        }

        void OnSashPointerDown(PointerDownEvent evt)
        {
            if (!m_LabelResizable || m_Disabled || evt.button != (int)MouseButton.LeftMouse)
                return;
            m_SashPointerId = evt.pointerId;
            m_SashStartX = evt.position.x;
            m_SashStartWidth = effectiveLabelWidth;
            m_Sash.CapturePointer(evt.pointerId);
            m_Sash.AddToClassList(Styles.activeUssClassName);
            evt.StopPropagation();
        }

        void OnSashPointerMove(PointerMoveEvent evt)
        {
            if (m_SashPointerId != evt.pointerId)
                return;
            if (!m_LabelResizable || m_Disabled)
            {
                EndSashDrag(evt.pointerId);
                return;
            }
            CommitSashWidth(m_SashStartWidth + (evt.position.x - m_SashStartX));
            evt.StopPropagation();
        }

        void EndSashDrag(int pointerId)
        {
            if (m_SashPointerId == PointerId.invalidPointerId || m_SashPointerId != pointerId)
                return;
            if (m_Sash.HasPointerCapture(pointerId))
                m_Sash.ReleasePointer(pointerId);
            m_Sash.RemoveFromClassList(Styles.activeUssClassName);
            m_SashPointerId = PointerId.invalidPointerId;
        }

        void OnSashKeyDown(KeyDownEvent evt)
        {
            if (!m_LabelResizable || m_Disabled)
                return;
            float next;
            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                    next = effectiveLabelWidth - sashKeyStep;
                    break;
                case KeyCode.RightArrow:
                    next = effectiveLabelWidth + sashKeyStep;
                    break;
                case KeyCode.Home:
                    next = m_MinLabelWidth;
                    break;
                default:
                    return;
            }
            CommitSashWidth(next);
            evt.StopPropagation();
        }

        void OnViewportHover(PointerMoveEvent evt) => TrackPointer(evt.position);

        /// <summary>
        /// Move the hover guide and the corner time to a pointer position. The parts call this from
        /// their drag moves, which a pointer capture keeps from reaching the viewport.
        /// </summary>
        /// <param name="worldPosition"> The pointer, in panel coordinates. </param>
        internal void TrackPointer(Vector3 worldPosition)
        {
            // A captured drag keeps reporting once the pointer has left, and no leave event follows
            // on release, so leaving is decided here too.
            if (!m_Viewport.worldBound.Contains(worldPosition))
            {
                if (m_Hovering)
                    EndHover();
                return;
            }

            m_HoverX = m_Viewport.WorldToLocal(worldPosition).x;
            m_Hovering = true;
            RefreshHover();
        }

        /// <summary> Re-read the guide and the corner time at the last pointer position over the viewport. </summary>
        void RefreshHover()
        {
            if (!m_Hovering || m_HoverLine == null)
                return;
            var x = m_HoverX;
            var upper = m_Infinite ? float.PositiveInfinity : m_Duration;
            var t = Mathf.Clamp(ViewportXToTime(x), 0f, upper);

            if (m_HoverLineEnabled)
            {
                var hi = m_Infinite ? float.PositiveInfinity : TimeToX(m_Duration) - m_Offset;
                m_HoverLine.style.left = Mathf.Clamp(x, origin - m_Offset, hi);
                m_HoverLine.RemoveFromClassList(Styles.hiddenUssClassName);
            }

            if (m_TimeLabel != null)
                m_CornerTime.text = m_TimeLabel(SnapTime(t));
        }

        void OnViewportLeave(PointerLeaveEvent evt) => EndHover();

        void EndHover()
        {
            m_Hovering = false;
            m_HoverLine.AddToClassList(Styles.hiddenUssClassName);
            RefreshCornerTime();
        }

        /// <summary> Show the playhead's time in the corner, unless the pointer is over the timeline. </summary>
        void RefreshCornerTime()
        {
            if (m_CornerTime == null)
                return;
            var show = m_TimeLabel != null;
            m_CornerSuffix.EnableInClassList(Styles.hiddenUssClassName, !show || string.IsNullOrEmpty(m_TimeSuffix));
            if (show && !m_Hovering)
                m_CornerTime.text = m_TimeLabel(m_Value);
        }

        /// <summary>
        /// Whether pointer interaction and keyboard seeking are off.
        /// </summary>
        [Tooltip("Whether pointer interaction and keyboard seeking are off.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool disabled
        {
            get => m_Disabled;
            set
            {
                if (m_Disabled == value)
                    return;
                m_Disabled = value;
                EnableInClassList(disabledUssClassName, value);
                if (value)
                    EndSashDrag(m_SashPointerId);
                ApplyLabelWidth();
                RefreshAxis();
                NotifyPropertyChanged(in disabledProperty);
            }
        }

        /// <summary>
        /// The wheel and drag interaction scheme.
        /// </summary>
        [Tooltip("The wheel and drag interaction scheme.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineControlScheme controlScheme
        {
            get => m_ControlScheme;
            set
            {
                if (m_ControlScheme == value)
                    return;
                m_ControlScheme = value;
                NotifyPropertyChanged(in controlSchemeProperty);
            }
        }

        /// <summary>
        /// Whether the faint per-frame guide lines are drawn across the lanes.
        /// </summary>
        [Tooltip("Whether the faint per-frame guide lines are drawn across the lanes.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showFrameLines
        {
            get => m_ShowFrameLines;
            set
            {
                if (m_ShowFrameLines == value)
                    return;
                m_ShowFrameLines = value;
                m_Grid.EnableInClassList(Styles.hiddenUssClassName, !value);
                NotifyPropertyChanged(in showFrameLinesProperty);
            }
        }

        /// <summary>
        /// Whether the axis extends past <see cref="duration"/> into empty frames.
        /// </summary>
        [Tooltip("Whether the axis extends past the duration into empty frames.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool infinite
        {
            get => m_Infinite;
            set
            {
                if (m_Infinite == value)
                    return;
                m_Infinite = value;
                // The pan bound, the scrollbar and the ticks all depend on it.
                RefreshAxis();
                NotifyPropertyChanged(in infiniteProperty);
            }
        }

        /// <summary>
        /// The zoom-out floor on <see cref="effectivePxPerUnit"/>. Defaults to
        /// <see cref="minEffectivePxPerUnit"/>.
        /// </summary>
        /// <remarks>
        /// With <see cref="fitWidth"/> on, the floor instead becomes whatever fills the viewport
        /// with the whole <see cref="duration"/>, so the axis never zooms out past the point where
        /// the content stops filling the strip.
        /// </remarks>
        [Tooltip("The zoom-out floor on the effective pixels per time unit.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minPxPerUnit
        {
            get => m_MinPxPerUnit;
            set
            {
                if (Mathf.Approximately(m_MinPxPerUnit, value))
                    return;
                m_MinPxPerUnit = Mathf.Max(0.01f, value);
                RefreshAxis();
                NotifyPropertyChanged(in minPxPerUnitProperty);
            }
        }

        /// <summary>
        /// The zoom-in cap on <see cref="effectivePxPerUnit"/>. Defaults to
        /// <see cref="maxEffectivePxPerUnit"/>.
        /// </summary>
        [Tooltip("The zoom-in cap on the effective pixels per time unit.")]
        [CreateProperty]
        [UxmlAttribute]
        public float maxPxPerUnit
        {
            get => m_MaxPxPerUnit;
            set
            {
                if (Mathf.Approximately(m_MaxPxPerUnit, value))
                    return;
                m_MaxPxPerUnit = Mathf.Max(0.01f, value);
                RefreshAxis();
                NotifyPropertyChanged(in maxPxPerUnitProperty);
            }
        }

        /// <summary>
        /// Whether the base scale fills the measured viewport rather than being a fixed
        /// pixels-per-unit. False by default.
        /// </summary>
        /// <remarks>
        /// On, <see cref="pxPerUnit"/> is recomputed from the viewport width and
        /// <see cref="fitDuration"/>, so the content fills the strip at zoom 1 whatever the panel
        /// is sized to, and the zoom-out floor fills the width with the whole duration.
        /// </remarks>
        [Tooltip("Whether the base scale fills the measured viewport.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool fitWidth
        {
            get => m_FitWidth;
            set
            {
                if (m_FitWidth == value)
                    return;
                m_FitWidth = value;
                RefreshAxis();
                NotifyPropertyChanged(in fitWidthProperty);
            }
        }

        /// <summary>
        /// The span the fitted view fills. Zero, the default, falls back to
        /// <see cref="duration"/>.
        /// </summary>
        /// <remarks>
        /// For a strip that should open showing a window onto a longer timeline -- the first ten
        /// seconds of a two minute clip -- rather than the whole of it.
        /// </remarks>
        [Tooltip("The span the fitted view fills. Zero falls back to the duration.")]
        [CreateProperty]
        [UxmlAttribute]
        public float fitDuration
        {
            get => m_FitDuration;
            set
            {
                if (Mathf.Approximately(m_FitDuration, value))
                    return;
                m_FitDuration = Mathf.Max(0f, value);
                RefreshAxis();
                NotifyPropertyChanged(in fitDurationProperty);
            }
        }

        /// <summary>
        /// Whether the ruler and grid thin their ticks to a readable interval as the axis zooms.
        /// False by default; <see cref="infinite"/> implies it.
        /// </summary>
        /// <remarks>
        /// Off, the tick interval is whatever <see cref="minorTick"/> and <see cref="majorTick"/>
        /// say, which turns to mush once a zoomed-out axis puts those ticks a pixel apart. On, the
        /// interval is chosen from the current pixel density, stepping through 1, 2, 5, 10 and so
        /// on, so the spacing stays legible at any zoom.
        /// </remarks>
        [Tooltip("Whether the ruler and grid thin their ticks to a readable interval as the axis zooms.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool adaptiveTicks
        {
            get => m_AdaptiveTicks;
            set
            {
                if (m_AdaptiveTicks == value)
                    return;
                m_AdaptiveTicks = value;
                RefreshAxis();
                NotifyPropertyChanged(in adaptiveTicksProperty);
            }
        }

        /// <summary>
        /// The zoom-out floor actually applied: the configured <see cref="minPxPerUnit"/>, or the
        /// scale that fills the viewport with the whole duration when <see cref="fitWidth"/> is on.
        /// </summary>
        float ResolvedMinPxPerUnit()
        {
            if (!m_FitWidth)
                return m_MinPxPerUnit;

            var width = FittableViewportWidth();
            if (width <= 0f || m_Duration <= 0f)
                return m_MinPxPerUnit;

            return Mathf.Max(m_MinPxPerUnit, width / m_Duration);
        }

        /// <summary>
        /// The zoom-in cap, never below the floor. A caller that sets them the wrong way round gets
        /// a pinned axis rather than a clamp that inverts.
        /// </summary>
        float ResolvedMaxPxPerUnit() => Mathf.Max(ResolvedMinPxPerUnit(), m_MaxPxPerUnit);

        /// <summary>
        /// The base scale before zoom: <see cref="pxPerUnit"/>, or the scale that fills the
        /// measured viewport with <see cref="fitDuration"/> when <see cref="fitWidth"/> is on.
        /// </summary>
        float EffectiveBasePxPerUnit()
        {
            if (!m_FitWidth)
                return m_PxPerUnit;

            var width = FittableViewportWidth();
            var span = m_FitDuration > 0f ? m_FitDuration : m_Duration;
            if (width <= 0f || span <= 0f)
                return m_PxPerUnit;

            return width / span;
        }

        float ViewportWidth() => m_Viewport?.layout.width ?? 0f;

        /// <summary>
        /// The viewport width actually available to the projected duration, once the fixed
        /// <see cref="origin"/> and <see cref="endPadding"/> insets are taken out. Fitting against
        /// the raw viewport width instead leaves the strip those insets too wide, so a supposedly
        /// fitted axis still overflows and stays pannable.
        /// </summary>
        float FittableViewportWidth() => Mathf.Max(0f, ViewportWidth() - origin - endPadding);

        /// <summary>
        /// The tick spacing the parts should actually draw at the given pixel density.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With <see cref="adaptiveTicks"/> off, this is simply <see cref="minorTick"/> and
        /// <see cref="majorTick"/>. On -- and <see cref="infinite"/> implies it, since an axis with
        /// no end cannot have a fixed tick count -- the interval is chosen from the current pixel
        /// density so the spacing stays legible at any zoom. Without it, a zoomed-out axis puts the
        /// fixed ticks a pixel apart and the ruler turns to mush.
        /// </para>
        /// </remarks>
        /// <param name="pixelsPerUnit"> The effective pixels per time unit. </param>
        /// <param name="minor"> The minor tick spacing to draw. </param>
        /// <param name="major"> The major, labelled tick spacing to draw. </param>
        internal void ResolveTicks(float pixelsPerUnit, out float minor, out float major)
        {
            if (!m_AdaptiveTicks && !m_Infinite)
            {
                minor = m_MinorTick;
                major = m_MajorTick;
                return;
            }

            minor = NiceInterval(AdaptiveFloor(), pixelsPerUnit, minAdaptiveMinorPx);
            if (m_MinTickStep > 0f)
            {
                // The registry's label step: the nice interval that leaves room for a label, on the
                // minor ladder so every label sits on a tick.
                major = NiceInterval(m_MinTickStep, pixelsPerUnit, minAdaptiveMajorPx);
                major = Mathf.Ceil(major / minor - 1e-4f) * minor;
            }
            else
            {
                major = NiceInterval(m_MajorTick, pixelsPerUnit, minAdaptiveMajorPx);
            }

            // A major that fell to or below the minor would label every tick, so push it up a step.
            if (major <= minor)
                major = NiceInterval(minor * 2f, pixelsPerUnit, minAdaptiveMajorPx);
        }

        /// <summary>
        /// The smallest 1/2/5 x 10^n interval at or above <paramref name="baseInterval"/> that
        /// leaves at least <paramref name="minPx"/> pixels between ticks.
        /// </summary>
        /// <param name="baseInterval"> The configured interval, the floor for the search. </param>
        /// <param name="pixelsPerUnit"> The effective pixels per time unit. </param>
        /// <param name="minPx"> The minimum on-screen gap between ticks. </param>
        /// <returns> The interval to draw at. </returns>
        internal static float NiceInterval(float baseInterval, float pixelsPerUnit, float minPx)
        {
            var floor = baseInterval > 0f ? baseInterval : 1f;
            if (pixelsPerUnit <= 0f)
                return floor;

            // The raw interval that would exactly meet the pixel budget, then rounded up onto the
            // 1/2/5 ladder so the labels land on numbers a reader can hold in their head.
            var wanted = Mathf.Max(floor, minPx / pixelsPerUnit);
            var magnitude = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(wanted)));
            var normalized = wanted / magnitude;
            var stepped = normalized <= 1f ? 1f : normalized <= 2f ? 2f : normalized <= 5f ? 5f : 10f;
            var candidate = stepped * magnitude;

            // Stay a whole multiple of the configured interval, so an adaptive ruler still lands on
            // the beats the caller asked for rather than drifting off them.
            if (floor > 0f)
            {
                var multiples = Mathf.Max(1f, Mathf.Round(candidate / floor));
                candidate = multiples * floor;
            }

            return candidate;
        }

        /// <summary>
        /// The peak opacity of the next finer tick level, from the theme's
        /// <c>--appui-timeline-tick-fine-opacity</c>. Zero, App UI's value, generates no finer level.
        /// </summary>
        internal float tickFineOpacity
        {
            get => m_TickFineOpacity;
            set
            {
                if (Mathf.Approximately(m_TickFineOpacity, value))
                    return;
                m_TickFineOpacity = value;
                // The axis record does not carry the opacity, so republishing it would compare equal
                // and notify nobody. Redraw the two tick parts directly instead.
                m_Ruler?.RebuildTicks();
                m_Grid?.MarkDirtyRepaint();
            }
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            tickFineOpacity = evt.customStyle.TryGetValue(k_TickFineOpacityProperty, out float fine) ? fine : 0f;
        }

        /// <summary>
        /// The finer tick level to fade in below the minor tick at the given pixel density.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mirrors the registry: the level below the tick step fades in as its ticks spread out,
        /// fully in once they are <see cref="minAdaptiveMinorPx"/> apart, at up to
        /// <see cref="tickFineOpacity"/>. It exists only with adaptive ticks and a non-zero opacity.
        /// </para>
        /// <para>
        /// The fine step is floored at <see cref="minTickStep"/>, as in the registry, or at
        /// <see cref="minorTick"/> while that is unset: the same floor <see cref="ResolveTicks"/>
        /// keeps to. It must also be a whole multiple of the floor, so a fine tick still lands on a
        /// configured beat.
        /// </para>
        /// </remarks>
        /// <param name="pixelsPerUnit"> The effective pixels per time unit. </param>
        /// <param name="minor"> The resolved minor tick spacing. </param>
        /// <param name="fineStep"> The fine tick spacing. </param>
        /// <param name="opacity"> The fine ticks' opacity, the peak scaled by the fade. </param>
        /// <returns> Whether a fine level is drawn at all. </returns>
        internal bool ResolveFineTicks(float pixelsPerUnit, float minor, out float fineStep, out float opacity)
        {
            fineStep = 0f;
            opacity = 0f;
            if (m_TickFineOpacity <= 0f || (!m_AdaptiveTicks && !m_Infinite) || minor <= 0f || pixelsPerUnit <= 0f)
                return false;

            var step = NiceDivisorBelow(minor);
            var floor = AdaptiveFloor();
            var multiples = step / floor;
            if (step < floor - 1e-4f || Mathf.Abs(multiples - Mathf.Round(multiples)) > 1e-3f)
                return false;

            // 1 at the handoff, where the fine ticks are as far apart as a minor tick must be.
            var gap = step * pixelsPerUnit / minAdaptiveMinorPx;
            var alpha = Mathf.Clamp01((gap - 0.35f) / 0.65f);
            if (alpha <= 0f)
                return false;

            fineStep = step;
            opacity = Mathf.Clamp01(m_TickFineOpacity) * alpha;
            return true;
        }

        /// <summary>
        /// The registry's next "nice" divisor below a tick step: 10 to 5, 5 or 2 to 1, 1 to 0.5.
        /// </summary>
        /// <param name="step"> The tick step. </param>
        /// <returns> The finer step. </returns>
        internal static float NiceDivisorBelow(float step)
        {
            var pow = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(step)));
            return Mathf.Round(step / pow) > 1f ? pow : 0.5f * pow;
        }

        /// <summary>
        /// Every tick the ruler and grid draw: the minor ticks across the duration, majors flagged,
        /// then the fine level when <see cref="ResolveFineTicks"/> asks for one.
        /// </summary>
        /// <param name="axis"> The published axis. </param>
        /// <param name="ticks"> Cleared, then filled in drawing order. </param>
        internal static void CollectTicks(TimelineAxisContext axis, List<TimelineTick> ticks)
        {
            ticks.Clear();
            if (axis == null || axis.minorTick <= 0f)
                return;

            // Past maxTicks the step coarsens by a whole factor, so the ticks still cover the whole
            // duration rather than stopping partway or vanishing.
            var tickStep = axis.minorTick;
            var count = Mathf.FloorToInt(axis.duration / tickStep + 1e-4f);
            var coarsened = count > maxTicks;
            if (coarsened)
            {
                tickStep *= Mathf.Ceil((float)count / maxTicks);
                count = Mathf.FloorToInt(axis.duration / tickStep + 1e-4f);
            }

            for (var i = 0; i <= count; i++)
            {
                var t = i * tickStep;
                var isMajor = axis.majorTick > 0f &&
                    Mathf.Abs(t / axis.majorTick - Mathf.Round(t / axis.majorTick)) < 1e-4f;
                ticks.Add(new TimelineTick(t, isMajor, 1f));
            }

            if (coarsened || axis.owner == null ||
                !axis.owner.ResolveFineTicks(axis.pxPerUnit, axis.minorTick, out var fineStep, out var opacity))
                return;

            // Overlap is decided on integer step indices: every ratio-th fine tick is a minor tick.
            // Comparing float quotients lets rounding through on a fractional axis and builds those
            // ticks twice.
            var ratio = Mathf.RoundToInt(axis.minorTick / fineStep);
            if (ratio < 2 || Mathf.Abs(axis.minorTick / fineStep - ratio) > 1e-3f)
                return;

            // Count only the fine ticks actually added; the others are already minor ticks.
            var fineCount = Mathf.FloorToInt(axis.duration / fineStep + 1e-4f);
            var distinctFine = fineCount - fineCount / ratio;
            if (count + distinctFine > maxTicks)
                return;

            for (var i = 0; i <= fineCount; i++)
            {
                if (i % ratio == 0)
                    continue;
                ticks.Add(new TimelineTick(i * fineStep, false, opacity));
            }
        }

        /// <summary>
        /// The current range selection, or `null` when nothing is selected.
        /// </summary>
        public TimelineSelection? selection
        {
            get => m_Selection;
            set
            {
                if (Nullable.Equals(m_Selection, value))
                    return;
                m_Selection = value;
                RefreshAxis();
                selectionChanged?.Invoke(value);
            }
        }

        /// <summary> Whether a range is currently selected. </summary>
        public bool hasSelection => m_Selection.HasValue && m_Selection.Value.length > 0f;

        /// <summary>
        /// The formatter used for ruler tick labels and the playhead bubble.
        /// </summary>
        /// <remarks>
        /// Defaults to the invariant numeric representation, matching the registry's `String` default.
        /// Setting it to `null` restores that default.
        /// </remarks>
        public Func<float, string> formatTime
        {
            get => m_FormatTime ??= DefaultFormatTime;
            set
            {
                m_FormatTime = value;
                RefreshAxis();
            }
        }

        static string DefaultFormatTime(float t) => t.ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>
        /// The one projection of time onto pixels.
        /// </summary>
        /// <remarks>
        /// Both <see cref="TimeToX"/> and <see cref="TimelineAxisContext.TimeToX"/> route through
        /// this, so the number a consumer computes and the number a part renders at cannot drift.
        /// </remarks>
        /// <param name="t"> The time, in time units. </param>
        /// <param name="origin"> The left inset before time zero, in pixels. </param>
        /// <param name="pxPerUnit"> The effective pixels per time unit. </param>
        /// <returns> The x coordinate, in pixels, relative to the strip's left edge. </returns>
        internal static float Project(float t, float origin, float pxPerUnit) => origin + t * pxPerUnit;

        /// <summary> The exact inverse of <see cref="Project"/>. </summary>
        /// <param name="x"> The x coordinate, in pixels, relative to the strip's left edge. </param>
        /// <param name="origin"> The left inset before time zero, in pixels. </param>
        /// <param name="pxPerUnit"> The effective pixels per time unit. </param>
        /// <returns> The time, in time units. </returns>
        internal static float Unproject(float x, float origin, float pxPerUnit) =>
            pxPerUnit <= 0f ? 0f : (x - origin) / pxPerUnit;

        /// <summary>
        /// Project a time onto an x coordinate inside the strip.
        /// </summary>
        /// <param name="t"> The time, in time units. </param>
        /// <returns> The x coordinate, in pixels, relative to the strip's left edge. </returns>
        public float TimeToX(float t) => Project(t, origin, effectivePxPerUnit);

        /// <summary>
        /// Project an x coordinate inside the strip back onto a time. The exact inverse of
        /// <see cref="TimeToX"/>.
        /// </summary>
        /// <param name="x"> The x coordinate, in pixels, relative to the strip's left edge. </param>
        /// <returns> The time, in time units. </returns>
        public float XToTime(float x) => Unproject(x, origin, effectivePxPerUnit);

        /// <summary>
        /// Project an x coordinate inside the *viewport* onto a time, accounting for the pan offset.
        /// </summary>
        /// <param name="viewportX"> The x coordinate, in pixels, relative to the viewport's left edge. </param>
        /// <returns> The time, in time units. </returns>
        public float ViewportXToTime(float viewportX) => XToTime(viewportX + m_Offset);

        /// <summary>
        /// Clamp a time to <c>[0, duration]</c> and snap it to <see cref="step"/>.
        /// </summary>
        /// <param name="t"> The raw time. </param>
        /// <returns> A time the playhead can occupy. </returns>
        public float SnapTime(float t)
        {
            if (m_Step > 0f)
                t = Mathf.Round(t / m_Step) * m_Step;
            var upper = m_Infinite ? float.MaxValue : m_Duration;
            return Mathf.Clamp(t, 0f, upper);
        }

        /// <summary>
        /// Add a <see cref="TimelineMarker"/> to the marker layer, above every lane.
        /// </summary>
        /// <param name="marker"> The marker. </param>
        /// <exception cref="ArgumentNullException"> The marker can't be null. </exception>
        public void AddMarker(TimelineMarker marker)
        {
            if (marker == null)
                throw new ArgumentNullException(nameof(marker));
            m_Markers.Add(marker);
        }

        /// <summary> Remove every <see cref="TimelineMarker"/>. </summary>
        public void ClearMarkers() => m_Markers.Clear();

        /// <summary>
        /// Zoom about a point, keeping the time under that point fixed.
        /// </summary>
        /// <param name="viewportX"> The pivot, in pixels, relative to the viewport's left edge. </param>
        /// <param name="factor"> The multiplier to apply to <see cref="zoom"/>. </param>
        public void ZoomAt(float viewportX, float factor)
        {
            var timeUnderCursor = ViewportXToTime(viewportX);
            var before = effectivePxPerUnit;
            zoom = m_Zoom * factor;
            var after = effectivePxPerUnit;
            if (Mathf.Approximately(before, after))
                return;
            // Keep `timeUnderCursor` under the cursor: origin + t*ppx - offset == viewportX.
            offset = origin + timeUnderCursor * after - viewportX;
        }

        /// <summary>
        /// Re-publish the time axis to every part. Call it after mutating several properties at once
        /// if you need the parts refreshed before the next layout pass.
        /// </summary>
        public void RefreshAxis()
        {
            m_Strip.style.width = stripWidth;

            // The pan stays within the axis when the axis shrinks under it: a shorter duration or
            // a zoom out would otherwise leave the strip translated past the viewport.
            offset = m_Offset;
            RefreshScrollbar();
            m_Ruler.style.height = m_RulerHeight;
            m_GutterSpacer.style.height = m_RulerHeight;
            m_Grid.style.top = m_RulerHeight;

            var ppu = effectivePxPerUnit;
            ResolveTicks(ppu, out var minor, out var major);

            this.ProvideContext(new TimelineAxisContext(
                this, m_Duration, ppu, origin, m_Value, m_Step,
                minor, major, m_TrackHeight, m_RulerHeight, m_Disabled, m_Selection,
                resolvedLaneExtent, m_KeySize));

            axisChanged?.Invoke();
            RefreshGutter();
            RefreshCornerTime();
            RefreshHover();
        }

        /// <summary>
        /// Rebuild the gutter rows so each one lines up with the lane it names.
        /// </summary>
        /// <remarks>
        /// This runs automatically whenever the lanes change or the axis is refreshed. A group
        /// contributes a header row carrying its expand chevron, followed by one row per child track
        /// while it is expanded.
        /// </remarks>
        public void RefreshGutter()
        {
            m_GutterRows.Clear();
            if (m_LabelWidth <= 0f)
                return;

            foreach (var lane in m_Lanes.Children())
            {
                switch (lane)
                {
                    case TimelineTrackGroup group:
                        m_GutterRows.Add(BuildGroupRow(group));
                        if (group.expanded)
                        {
                            foreach (var child in group.Children())
                            {
                                if (child is TimelineTrack t)
                                    m_GutterRows.Add(BuildTrackRow(t, true));
                            }
                        }
                        break;
                    case TimelineTrack track:
                        m_GutterRows.Add(BuildTrackRow(track, false));
                        break;
                }
            }
        }

        VisualElement BuildTrackRow(TimelineTrack track, bool nested)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList(gutterRowUssClassName);
            row.EnableInClassList(gutterRowUssClassName + "--nested", nested);
            row.style.height = track.resolvedHeight;
            var label = new LocalizedTextElement(track.label) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(gutterLabelUssClassName);
            row.Add(label);
            return row;
        }

        VisualElement BuildGroupRow(TimelineTrackGroup group)
        {
            var row = new VisualElement();
            row.AddToClassList(gutterRowUssClassName);
            row.AddToClassList(gutterGroupUssClassName);
            row.style.height = group.headerHeight;

            var chevron = new Icon
            {
                iconName = group.expanded ? "caret-down" : "caret-right",
                pickingMode = PickingMode.Ignore,
            };
            chevron.AddToClassList(gutterChevronUssClassName);
            row.Add(chevron);

            var label = new LocalizedTextElement(group.title) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(gutterLabelUssClassName);
            row.Add(label);

            row.AddManipulator(new Pressable(() => group.expanded = !group.expanded));
            return row;
        }

        /// <summary>
        /// How far along the axis the track lanes are drawn, in time units. Negative, the default,
        /// draws them to <see cref="duration"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The length of the thing being edited, when the axis is kept addressable a little past it
        /// so content can be dragged out and the length grown. It never reaches past
        /// <see cref="duration"/>. A <see cref="TimelineTrack"/> can end its own lane earlier with
        /// <see cref="TimelineTrack.laneEnd"/>.
        /// </para>
        /// </remarks>
        [Tooltip("How far along the axis the track lanes are drawn. Negative draws them to the duration.")]
        [CreateProperty]
        [UxmlAttribute]
        public float laneExtent
        {
            get => m_LaneExtent;
            set
            {
                var v = float.IsFinite(value) ? value : -1f;
                if (Mathf.Approximately(m_LaneExtent, v))
                    return;
                m_LaneExtent = v;
                RefreshAxis();
                NotifyPropertyChanged(in laneExtentProperty);
            }
        }

        /// <summary> The lane extent actually drawn: <see cref="laneExtent"/>, or the duration, never past it. </summary>
        public float resolvedLaneExtent => m_LaneExtent < 0f ? m_Duration : Mathf.Min(m_Duration, m_LaneExtent);

        /// <summary>
        /// One glyph size for every key on the sheet. <see cref="TimelineKeySize.Auto"/>, the
        /// default, draws muted keys small and the rest regular. A key's own
        /// <see cref="TimelineKeyframe.size"/> still wins.
        /// </summary>
        [Tooltip("One glyph size for every key on the sheet. Auto draws muted keys small.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineKeySize keySize
        {
            get => m_KeySize;
            set
            {
                if (m_KeySize == value)
                    return;
                m_KeySize = value;
                RefreshAxis();
                NotifyPropertyChanged(in keySizeProperty);
            }
        }

        /// <summary>
        /// The finest tick interval an adaptive axis may pick, in time units. Zero, the default, falls
        /// back to <see cref="minorTick"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Active with <see cref="adaptiveTicks"/> or <see cref="infinite"/>. A seconds axis sets it
        /// to <c>0.1</c>, say, so zooming in subdivides a second into halves, fifths and tenths, each
        /// finer level taking over once its ticks are far enough apart. Without it the adaptive axis
        /// never goes finer than <see cref="minorTick"/>. The registry defaults it to <c>1</c>, which
        /// is <see cref="minorTick"/>'s default, so the two agree out of the box.
        /// </para>
        /// </remarks>
        [Tooltip("The finest tick interval an adaptive axis may pick. Zero falls back to minorTick.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minTickStep
        {
            get => m_MinTickStep;
            set
            {
                var v = float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
                if (Mathf.Approximately(m_MinTickStep, v))
                    return;
                m_MinTickStep = v;
                RefreshAxis();
                NotifyPropertyChanged(in minTickStepProperty);
            }
        }

        /// <summary> Which edge of the ruler the ticks hang from. <see cref="TimelineTicksAt.Bottom"/> by default. </summary>
        [Tooltip("Which edge of the ruler the ticks hang from.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineTicksAt ticksAt
        {
            get => m_TicksAt;
            set
            {
                if (m_TicksAt == value)
                    return;
                m_TicksAt = value;
                m_Ruler.EnableInClassList(TimelineRuler.ticksTopUssClassName, value == TimelineTicksAt.Top);
                NotifyPropertyChanged(in ticksAtProperty);
            }
        }

        /// <summary> The adaptive axis's finest interval: <see cref="minTickStep"/>, else <see cref="minorTick"/>, else 1. </summary>
        float AdaptiveFloor() => m_MinTickStep > 0f ? m_MinTickStep : m_MinorTick > 0f ? m_MinorTick : 1f;

        internal void RegisterHandle(TimelineHandle handle)
        {
            if (!m_Handles.Contains(handle))
                m_Handles.Add(handle);
        }

        internal void UnregisterHandle(TimelineHandle handle) => m_Handles.Remove(handle);

        /// <summary>
        /// Whether a <see cref="TimelineHandle"/> is under a point. The playhead is drawn above the
        /// lanes, so it asks before taking the pointer: a grip parked at the playhead's time must
        /// still be the thing that is grabbed.
        /// </summary>
        /// <param name="worldPosition"> The point, in panel coordinates. </param>
        /// <returns> True when a pickable handle contains it. </returns>
        internal bool HandleContains(Vector2 worldPosition)
        {
            foreach (var handle in m_Handles)
            {
                if (handle.panel != null && handle.pickingMode == PickingMode.Position &&
                    handle.resolvedStyle.display == DisplayStyle.Flex && handle.worldBound.Contains(worldPosition))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Pan so a time is in view, keeping a margin from the viewport edge. Does nothing when the
        /// time is already in view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The playhead calls this when the keyboard moves it, so a playhead sent to End is not left
        /// off-screen. Assigning <see cref="value"/> from code does not pan.
        /// </para>
        /// </remarks>
        /// <param name="t"> The time to bring into view, in time units. </param>
        public void RevealTime(float t)
        {
            var vp = ViewportWidth();
            if (float.IsNaN(vp) || vp <= 0f)
                return;
            var x = TimeToX(t);
            var margin = Mathf.Min(revealMargin, vp / 4f);
            if (x < m_Offset + margin)
                offset = x - margin;
            else if (x > m_Offset + vp - margin)
                offset = x - vp + margin;
        }

        /// <summary> The overlay horizontal scrollbar. Hidden while everything fits or the axis is infinite. </summary>
        public Scroller scrollbar => m_Scrollbar;

        float MaxOffset()
        {
            var vp = ViewportWidth();
            return float.IsNaN(vp) ? 0f : Mathf.Max(0f, stripWidth - vp);
        }

        void RefreshScrollbar()
        {
            if (m_Scrollbar == null)
                return;
            var vp = ViewportWidth();
            var max = MaxOffset();
            var show = !m_Infinite && !float.IsNaN(vp) && vp > 0f && max > 0.5f;
            m_Scrollbar.EnableInClassList(Styles.hiddenUssClassName, !show);
            if (!show)
            {
                m_Scrollbar.SetEnabled(!m_Disabled);
                return;
            }

            m_SyncingScrollbar = true;
            try
            {
                m_Scrollbar.lowValue = 0f;
                m_Scrollbar.highValue = max;
                m_Scrollbar.Adjust(Mathf.Clamp01(vp / stripWidth));
                m_Scrollbar.slider.SetValueWithoutNotify(Mathf.Clamp(m_Offset, 0f, max));
                // After Adjust, which enables the Scroller whenever its thumb is narrower than the track.
                m_Scrollbar.SetEnabled(!m_Disabled);
            }
            finally
            {
                m_SyncingScrollbar = false;
            }
        }

        void OnScrollbarValueChanged(float v)
        {
            if (m_SyncingScrollbar || m_Disabled)
                return;
            offset = v;
        }

        void OnScrollbarTrackPointerDown(PointerDownEvent evt)
        {
            if (m_Disabled || evt.button != (int)MouseButton.LeftMouse || evt.modifiers != EventModifiers.None)
                return;
            var dragger = m_Scrollbar.slider.Q(className: BaseSlider<float>.draggerUssClassName);
            var container = dragger?.parent;
            if (container == null || evt.target is VisualElement ve && dragger.Contains(ve))
                return;

            var track = container.layout.width;
            var thumb = dragger.layout.width;
            var travel = track - thumb;
            var max = MaxOffset();
            if (travel <= 0f || max <= 0f)
                return;

            // Centre the thumb under the pointer, as the stock jump does. A modified click (the
            // Editor scheme's Alt-drag pan) is left to the viewport, as the stock dragger leaves it.
            var local = container.WorldToLocal((Vector2)evt.position);
            offset = Mathf.Clamp01((local.x - thumb * 0.5f) / travel) * max;
        }

        void OnScrollbarKeyDown(KeyDownEvent evt)
        {
            if (m_Disabled)
                return;
            var stepPx = Mathf.Max(16f, ViewportWidth() / 10f);
            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                    offset = m_Offset - stepPx;
                    break;
                case KeyCode.RightArrow:
                    offset = m_Offset + stepPx;
                    break;
                case KeyCode.Home:
                    offset = 0f;
                    break;
                case KeyCode.End:
                    offset = MaxOffset();
                    break;
                default:
                    return;
            }
            evt.StopPropagation();
        }

        // ------------------------------------------------------------------ input

        void OnWheel(WheelEvent evt)
        {
            if (m_Disabled)
                return;

            var shouldZoom = m_ControlScheme switch
            {
                TimelineControlScheme.Modern => evt.ctrlKey || evt.commandKey,
                TimelineControlScheme.Editor => true,
                _ => evt.ctrlKey || evt.commandKey,
            };

            if (shouldZoom)
            {
                var factor = Mathf.Pow(2f, -evt.delta.y * k_ZoomSpeed * 100f);
                ZoomAt(m_Viewport.WorldToLocal(evt.mousePosition).x, factor);
            }
            else
            {
                // A two-finger swipe reports its horizontal intent on delta.x, but a bare wheel only
                // has delta.y — and on a single-axis wheel that y IS the pan intent for a horizontal
                // axis, so fall back to it.
                var dx = Mathf.Abs(evt.delta.x) > Mathf.Epsilon ? evt.delta.x : evt.delta.y;
                offset += dx * k_PanSpeed;
            }

            evt.StopPropagation();
        }

        bool panModifierPressed(bool altKey) => m_ControlScheme switch
        {
            TimelineControlScheme.Modern => m_SpaceBarPressed,
            TimelineControlScheme.Editor => altKey,
            _ => m_SpaceBarPressed,
        };

        void OnViewportPointerDown(PointerDownEvent evt)
        {
            if (m_Disabled)
                return;

            var isPan = evt.button == (int)MouseButton.MiddleMouse ||
                (evt.button == (int)MouseButton.LeftMouse && panModifierPressed(evt.altKey));
            if (!isPan)
                return;

            m_PanPointerId = evt.pointerId;
            m_PanStartX = evt.position.x;
            m_PanStartOffset = m_Offset;
            m_Viewport.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnViewportPointerMove(PointerMoveEvent evt)
        {
            TrackPointer(evt.position);
            if (m_PanPointerId != evt.pointerId)
                return;
            offset = m_PanStartOffset - (evt.position.x - m_PanStartX);
            evt.StopPropagation();
        }

        void OnViewportPointerUp(PointerUpEvent evt) => EndPan(evt.pointerId);

        void OnViewportPointerCancel(PointerCancelEvent evt) => EndPan(evt.pointerId);

        void EndPan(int pointerId)
        {
            if (m_PanPointerId != pointerId)
                return;
            if (m_Viewport.HasPointerCapture(pointerId))
                m_Viewport.ReleasePointer(pointerId);
            m_PanPointerId = PointerId.invalidPointerId;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
                m_SpaceBarPressed = true;
        }

        void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
                m_SpaceBarPressed = false;
        }
    }

    /// <summary>
    /// Base class for the parts that live on a <see cref="Timeline"/>'s time axis.
    /// </summary>
    /// <remarks>
    /// A part reads the axis from the <see cref="TimelineAxisContext"/> its Timeline provides rather
    /// than reaching for its parent, so it keeps working however deeply it is nested — inside a
    /// track, inside a group, inside another wrapper. <see cref="OnAxisChanged"/> is called once on
    /// attach and again on every axis change; override it and project through <see cref="axis"/>.
    /// </remarks>
    public abstract class TimelinePart : BaseVisualElement
    {
        Timeline m_Timeline;

        /// <summary>
        /// The current axis, or `null` while the part is not inside a <see cref="Timeline"/>.
        /// </summary>
        protected TimelineAxisContext axis { get; private set; }

        /// <summary> The Timeline this part belongs to, or `null`. </summary>
        public Timeline timeline => m_Timeline;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected TimelinePart()
        {
            this.RegisterContextChangedCallback<TimelineAxisContext>(OnContextChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            var next = GetFirstAncestorOfType<Timeline>();
            if (!ReferenceEquals(m_Timeline, next))
            {
                if (m_Timeline != null)
                    m_Timeline.axisChanged -= Refresh;
                m_Timeline = next;
                if (m_Timeline != null)
                    m_Timeline.axisChanged += Refresh;
            }
            Refresh();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (m_Timeline != null)
                m_Timeline.axisChanged -= Refresh;
            m_Timeline = null;
        }

        void OnContextChanged(ContextChangedEvent<TimelineAxisContext> evt)
        {
            axis = evt.context;
            OnAxisChanged();
        }

        void Refresh()
        {
            axis = this.GetContext<TimelineAxisContext>();
            OnAxisChanged();
        }

        /// <summary>
        /// Re-project this part onto the current <see cref="axis"/>.
        /// </summary>
        /// <remarks>
        /// Called on attach and on every axis change — a scale, zoom, pan, playhead, tick or
        /// selection change. Guard against a `null` <see cref="axis"/>.
        /// </remarks>
        protected abstract void OnAxisChanged();

        /// <summary> Project a time onto an x coordinate inside the strip. </summary>
        /// <param name="t"> The time, in time units. </param>
        /// <returns> The x coordinate in pixels, or 0 when there is no axis yet. </returns>
        protected float TimeToX(float t) => axis?.TimeToX(t) ?? 0f;
    }

    /// <summary>
    /// A lane inside a <see cref="Timeline"/>, holding <see cref="TimelineClip"/>s and
    /// <see cref="TimelineKeyframe"/>s.
    /// </summary>
    /// <remarks>
    /// The track's <see cref="label"/> is not drawn by the track itself — the Timeline's left gutter
    /// renders it, so labels stay put while the lanes pan. Set <see cref="color"/> to give every clip
    /// and keyframe in the lane a default fill; each of them can still override it.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <appui:TimelineTrack id="position" label="Position">
    ///     <appui:TimelineClip start="4" duration="12" label="Walk" />
    /// </appui:TimelineTrack>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TimelineTrack : TimelinePart
    {
        internal static readonly BindingId idProperty = nameof(id);
        internal static readonly BindingId labelProperty = nameof(label);
        internal static readonly BindingId heightProperty = nameof(height);
        internal static readonly BindingId colorProperty = nameof(color);

        /// <summary> The TimelineTrack main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__track";

        /// <summary> The styling class of the lane background that spans the duration. </summary>
        public const string backgroundUssClassName = ussClassName + "-background";

        internal static readonly BindingId laneProperty = nameof(lane);
        internal static readonly BindingId laneStartProperty = nameof(laneStart);
        internal static readonly BindingId laneEndProperty = nameof(laneEnd);

        readonly VisualElement m_Background;

        string m_Id;
        string m_Label;
        float m_Height = -1f;
        Optional<Color> m_Color;
        bool m_Lane = true;
        float m_LaneStart;
        float m_LaneEnd = -1f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineTrack()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Background = new VisualElement { name = backgroundUssClassName, pickingMode = PickingMode.Ignore };
            m_Background.AddToClassList(backgroundUssClassName);
            hierarchy.Add(m_Background);
        }

        /// <summary> A stable identifier for the lane. </summary>
        [Tooltip("A stable identifier for the lane.")]
        [CreateProperty]
        [UxmlAttribute]
        public string id
        {
            get => m_Id;
            set
            {
                if (m_Id == value)
                    return;
                m_Id = value;
                NotifyPropertyChanged(in idProperty);
            }
        }

        /// <summary> The text the Timeline's gutter shows for this lane. </summary>
        [Tooltip("The text the Timeline's gutter shows for this lane.")]
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label;
            set
            {
                if (m_Label == value)
                    return;
                m_Label = value;
                timeline?.RefreshGutter();
                NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The lane height in pixels. A negative value, the default, defers to the Timeline's
        /// <see cref="Timeline.trackHeight"/>.
        /// </summary>
        [Tooltip("The lane height in pixels. Negative defers to the Timeline's trackHeight.")]
        [CreateProperty]
        [UxmlAttribute]
        public float height
        {
            get => m_Height;
            set
            {
                if (Mathf.Approximately(m_Height, value))
                    return;
                m_Height = value;
                OnAxisChanged();
                timeline?.RefreshGutter();
                NotifyPropertyChanged(in heightProperty);
            }
        }

        /// <summary> The height this lane actually renders at, resolving the Timeline's default. </summary>
        public float resolvedHeight =>
            m_Height >= 0f ? m_Height : axis?.trackHeight ?? Timeline.defaultTrackHeight;

        /// <summary> The default fill for the clips and keyframes in this lane. </summary>
        [Tooltip("The default fill for the clips and keyframes in this lane.")]
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
                foreach (var child in Children())
                {
                    switch (child)
                    {
                        case TimelineClip clip: clip.RefreshFill(); break;
                        case TimelineKeyframe kf: kf.RefreshFill(); break;
                    }
                }
                NotifyPropertyChanged(in colorProperty);
            }
        }

        /// <summary>
        /// Whether the translucent lane is drawn behind the track. Turn it off for a heading row that
        /// holds no clips or keys, so only lanes with content read as lanes. True by default.
        /// </summary>
        [Tooltip("Whether the lane is drawn behind the track.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool lane
        {
            get => m_Lane;
            set
            {
                if (m_Lane == value)
                    return;
                m_Lane = value;
                OnAxisChanged();
                NotifyPropertyChanged(in laneProperty);
            }
        }

        /// <summary> Where the lane begins, in time units. Zero by default. </summary>
        [Tooltip("Where the lane begins, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float laneStart
        {
            get => m_LaneStart;
            set
            {
                var v = float.IsFinite(value) ? value : 0f;
                if (Mathf.Approximately(m_LaneStart, v))
                    return;
                m_LaneStart = v;
                OnAxisChanged();
                NotifyPropertyChanged(in laneStartProperty);
            }
        }

        /// <summary>
        /// Where the lane ends, in time units: the extent of what the track holds, a clip's span for
        /// instance. Negative, the default, ends it at the Timeline's <see cref="Timeline.laneExtent"/>.
        /// </summary>
        [Tooltip("Where the lane ends, in time units. Negative ends it at the Timeline's laneExtent.")]
        [CreateProperty]
        [UxmlAttribute]
        public float laneEnd
        {
            get => m_LaneEnd;
            set
            {
                var v = float.IsFinite(value) ? value : -1f;
                if (Mathf.Approximately(m_LaneEnd, v))
                    return;
                m_LaneEnd = v;
                OnAxisChanged();
                NotifyPropertyChanged(in laneEndProperty);
            }
        }

        /// <summary> The lane drawn behind the track. Style it, a rim on its ends for instance, through USS. </summary>
        public VisualElement laneElement => m_Background;

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            style.height = resolvedHeight;
            if (axis == null)
                return;
            var to = m_LaneEnd >= 0f ? m_LaneEnd : axis.laneExtent;
            m_Background.EnableInClassList(Styles.hiddenUssClassName, !m_Lane || to <= m_LaneStart);
            m_Background.style.left = axis.TimeToX(m_LaneStart);
            m_Background.style.width = Mathf.Max(0f, (to - m_LaneStart) * axis.pxPerUnit);
        }
    }

    /// <summary>
    /// A collapsible group of <see cref="TimelineTrack"/>s — every lane belonging to one object.
    /// </summary>
    /// <remarks>
    /// The group draws a muted header lane and, while <see cref="expanded"/>, its child tracks flush
    /// beneath it. The Timeline's gutter renders the title and the expand chevron. When collapsed the
    /// header shows a read-only summary of every keyframe time in the group, so a folded object still
    /// reads at a glance.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <appui:TimelineTrackGroup id="cube" title="Cube">
    ///     <appui:TimelineTrack id="position" label="Position" />
    ///     <appui:TimelineTrack id="rotation" label="Rotation" />
    /// </appui:TimelineTrackGroup>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TimelineTrackGroup : TimelinePart
    {
        internal static readonly BindingId idProperty = nameof(id);
        internal static readonly BindingId titleProperty = nameof(title);
        internal static readonly BindingId heightProperty = nameof(height);
        internal static readonly BindingId expandedProperty = nameof(expanded);

        /// <summary> The TimelineTrackGroup main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__group";

        /// <summary> The styling class of the group's summary header lane. </summary>
        public const string headerUssClassName = ussClassName + "-header";

        /// <summary> The styling class of one diamond in the collapsed summary. </summary>
        public const string summaryUssClassName = ussClassName + "-summary";

        /// <summary> The styling class of the container holding the child tracks. </summary>
        public const string tracksUssClassName = ussClassName + "-tracks";

        readonly VisualElement m_Header;
        readonly VisualElement m_Tracks;

        string m_Id;
        string m_Title;
        float m_Height = -1f;
        bool m_Expanded = true;

        /// <summary> Raised when the group is expanded or collapsed. </summary>
        public event Action<bool> expandedChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineTrackGroup()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore };
            m_Header.AddToClassList(headerUssClassName);
            hierarchy.Add(m_Header);

            m_Tracks = new VisualElement { name = tracksUssClassName, pickingMode = PickingMode.Ignore };
            m_Tracks.AddToClassList(tracksUssClassName);
            hierarchy.Add(m_Tracks);
        }

        /// <summary> Child tracks are added to the group's own lane container. </summary>
        public override VisualElement contentContainer => m_Tracks;

        /// <summary> The group's summary header lane. </summary>
        public VisualElement header => m_Header;

        /// <summary> A stable identifier for the group. </summary>
        [Tooltip("A stable identifier for the group.")]
        [CreateProperty]
        [UxmlAttribute]
        public string id
        {
            get => m_Id;
            set
            {
                if (m_Id == value)
                    return;
                m_Id = value;
                NotifyPropertyChanged(in idProperty);
            }
        }

        /// <summary> The text the Timeline's gutter shows for this group. </summary>
        [Tooltip("The text the Timeline's gutter shows for this group.")]
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title;
            set
            {
                if (m_Title == value)
                    return;
                m_Title = value;
                timeline?.RefreshGutter();
                NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// The header lane height in pixels. A negative value defers to the Timeline's
        /// <see cref="Timeline.trackHeight"/>.
        /// </summary>
        [Tooltip("The header lane height in pixels. Negative defers to the Timeline's trackHeight.")]
        [CreateProperty]
        [UxmlAttribute]
        public float height
        {
            get => m_Height;
            set
            {
                if (Mathf.Approximately(m_Height, value))
                    return;
                m_Height = value;
                OnAxisChanged();
                timeline?.RefreshGutter();
                NotifyPropertyChanged(in heightProperty);
            }
        }

        /// <summary> The height the header lane actually renders at. </summary>
        public float headerHeight =>
            m_Height >= 0f ? m_Height : axis?.trackHeight ?? Timeline.defaultTrackHeight;

        /// <summary> Whether the child tracks are laid out. Defaults to `true`. </summary>
        [Tooltip("Whether the child tracks are laid out.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool expanded
        {
            get => m_Expanded;
            set
            {
                if (m_Expanded == value)
                    return;
                m_Expanded = value;
                EnableInClassList(Styles.openUssClassName, value);
                m_Tracks.EnableInClassList(Styles.hiddenUssClassName, !value);
                OnAxisChanged();
                timeline?.RefreshGutter();
                expandedChanged?.Invoke(value);
                NotifyPropertyChanged(in expandedProperty);
            }
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            m_Header.style.height = headerHeight;
            EnableInClassList(Styles.openUssClassName, m_Expanded);
            m_Tracks.EnableInClassList(Styles.hiddenUssClassName, !m_Expanded);
            RebuildSummary();
        }

        void RebuildSummary()
        {
            m_Header.Clear();
            if (axis == null || m_Expanded)
                return;

            var seen = new HashSet<float>();
            CollectKeyframeTimes(m_Tracks, seen);
            foreach (var t in seen)
            {
                var diamond = new VisualElement { pickingMode = PickingMode.Ignore };
                diamond.AddToClassList(summaryUssClassName);
                diamond.style.left = axis.TimeToX(t);
                m_Header.Add(diamond);
            }
        }

        static void CollectKeyframeTimes(VisualElement root, HashSet<float> into)
        {
            foreach (var child in root.Children())
            {
                if (child is TimelineKeyframe kf)
                    into.Add(kf.time);
                CollectKeyframeTimes(child, into);
            }
        }
    }

    /// <summary>
    /// The fixed hue a frame clip takes.
    /// </summary>
    /// <remarks>
    /// Named by role rather than by hue in the tokens themselves: purple is the object track,
    /// amber the camera, green the signal. They are deliberately the same value in every theme
    /// mode, so a clip's identity does not flip between light and dark.
    /// </remarks>
    public enum TimelineFrameColor
    {
        /// <summary> The object-track hue. </summary>
        Purple,

        /// <summary> The camera-track hue. </summary>
        Amber,

        /// <summary> The signal-track hue. </summary>
        Green,
    }

    /// <summary>
    /// A range bar on a <see cref="TimelineTrack"/>.
    /// </summary>
    /// <remarks>
    /// Drag the body to move the clip and either edge to resize it. Both gestures snap to the
    /// Timeline's <see cref="Timeline.step"/> and are clamped inside <c>[0, duration]</c>, and a
    /// resize never takes the clip below <see cref="minDuration"/>. The geometry is committed once on
    /// release through <see cref="changed"/>, not on every pointer tick, so one drag is one edit;
    /// <see cref="dragMove"/> reports the live geometry in between for previews.
    /// </remarks>
    /// <example>
    /// <code lang="csharp"><![CDATA[
    /// var clip = new TimelineClip { start = 4f, duration = 12f, label = "Walk" };
    /// clip.changed += (start, duration) => Persist(start, duration);
    /// track.Add(clip);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TimelineClip : TimelinePart
    {
        internal static readonly BindingId startProperty = nameof(start);
        internal static readonly BindingId durationProperty = nameof(duration);
        internal static readonly BindingId labelProperty = nameof(label);
        internal static readonly BindingId colorProperty = nameof(color);
        internal static readonly BindingId draggableProperty = nameof(draggable);
        internal static readonly BindingId resizableProperty = nameof(resizable);
        internal static readonly BindingId minDurationProperty = nameof(minDuration);
        internal static readonly BindingId selectedProperty = nameof(selected);
        internal static readonly BindingId frameProperty = nameof(frame);
        internal static readonly BindingId frameColorProperty = nameof(frameColor);
        internal static readonly BindingId frameHeightProperty = nameof(frameHeight);
        internal static readonly BindingId variantProperty = nameof(variant);

        /// <summary> The TimelineClip main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__clip";

        /// <summary> The styling class of the clip's label. </summary>
        public const string labelUssClassName = ussClassName + "-label";

        /// <summary> The styling class of a clip resize edge. </summary>
        public const string edgeUssClassName = ussClassName + "-edge";

        /// <summary> The styling class applied while the clip is a placement frame. </summary>
        public const string frameUssClassName = ussClassName + "--frame";

        /// <summary> The styling class prefix carrying a frame clip's hue. </summary>
        [EnumName("GetFrameColorUssClassName", typeof(TimelineFrameColor))]
        public const string frameColorUssClassName = ussClassName + "--frame-";

        /// <summary> The styling class of a frame clip's translucent body. </summary>
        public const string fillUssClassName = ussClassName + "-fill";

        /// <summary> The styling class of a frame clip's drag handle strip. </summary>
        public const string dragHandleUssClassName = ussClassName + "-drag-handle";

        /// <summary> The styling class of the icon inside a handle-variant clip's drag handle. </summary>
        public const string dragHandleIconUssClassName = ussClassName + "-drag-handle-icon";

        /// <summary> The styling class prefix carrying the clip's <see cref="variant"/>. </summary>
        [EnumName("GetVariantUssClassName", typeof(TimelineClipVariant))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary> The tallest a frame clip's handle strip gets, in pixels. </summary>
        public const float maxFrameHandleHeight = 22f;

        /// <summary> The smallest duration a resize will leave, in time units. </summary>
        public const float defaultMinDuration = 0.5f;

        enum Op { None, Move, ResizeLeft, ResizeRight }

        readonly LocalizedTextElement m_Label;
        readonly VisualElement m_LeftEdge;
        readonly VisualElement m_RightEdge;

        float m_Start;
        float m_Duration;
        Optional<Color> m_Color;
        bool m_Draggable = true;
        bool m_Resizable = true;
        bool m_ResizableSet;
        TimelineClipVariant m_Variant = TimelineClipVariant.Bar;
        Icon m_DragHandleIcon;
        bool m_IconTinted;
        float m_MinDuration = defaultMinDuration;
        bool m_Selected;
        bool m_Frame;
        TimelineFrameColor m_FrameColor = TimelineFrameColor.Purple;
        float m_FrameHeight;
        readonly VisualElement m_Fill;
        readonly VisualElement m_DragHandle;

        Op m_Op = Op.None;
        int m_PointerId = PointerId.invalidPointerId;
        float m_PointerStartX;
        float m_DragStart;
        float m_DragDuration;
        float m_LastStart;
        float m_LastDuration;

        /// <summary> Raised once on release with the committed start and duration. </summary>
        public event Action<float, float> changed;

        /// <summary> Raised on every pointer move during a drag with the live start and duration. </summary>
        public event Action<float, float> dragMove;

        /// <summary> Raised when the clip body is clicked without dragging. </summary>
        public event Action clicked;

        /// <summary> Raised when the clip is double-clicked. </summary>
        public event Action doubleClicked;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineClip()
        {
            AddToClassList(ussClassName);
            focusable = true;

            // Behind everything, and only visible on a frame clip, where it carries the
            // translucent body the lane shows through.
            m_Fill = new VisualElement { name = fillUssClassName, pickingMode = PickingMode.Ignore };
            m_Fill.AddToClassList(fillUssClassName);
            m_Fill.AddToClassList(Styles.hiddenUssClassName);
            hierarchy.Add(m_Fill);

            m_DragHandle = new VisualElement { name = dragHandleUssClassName };
            m_DragHandle.AddToClassList(dragHandleUssClassName);
            m_DragHandle.AddToClassList(Styles.hiddenUssClassName);
            hierarchy.Add(m_DragHandle);

            // Shown by the handle variant only: the frame's handle strip stays a bare drag surface.
            m_DragHandleIcon = CreateDragHandleIcon();
            m_DragHandleIcon.AddToClassList(Styles.hiddenUssClassName);
            m_DragHandle.Add(m_DragHandleIcon);
            AddToClassList(GetVariantUssClassName(m_Variant));

            m_LeftEdge = new VisualElement { name = edgeUssClassName + "-left" };
            m_LeftEdge.AddToClassList(edgeUssClassName);
            m_LeftEdge.AddToClassList(edgeUssClassName + "--left");
            hierarchy.Add(m_LeftEdge);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            m_RightEdge = new VisualElement { name = edgeUssClassName + "-right" };
            m_RightEdge.AddToClassList(edgeUssClassName);
            m_RightEdge.AddToClassList(edgeUssClassName + "--right");
            hierarchy.Add(m_RightEdge);

            RegisterCallback<PointerDownEvent>(evt => BeginDrag(evt, Op.Move));
            m_DragHandle.RegisterCallback<PointerDownEvent>(evt => BeginDrag(evt, Op.Move));
            m_DragHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.clickCount == 2)
                    doubleClicked?.Invoke();
            });
            m_LeftEdge.RegisterCallback<PointerDownEvent>(evt => BeginDrag(evt, Op.ResizeLeft));
            m_RightEdge.RegisterCallback<PointerDownEvent>(evt => BeginDrag(evt, Op.ResizeRight));

            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_LeftEdge.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_RightEdge.RegisterCallback<PointerMoveEvent>(OnPointerMove);

            RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_LeftEdge.RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_RightEdge.RegisterCallback<PointerUpEvent>(OnPointerUp);
            // The handle captures the pointer it drags with, so its release reaches only it.
            m_DragHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        /// <summary> The clip's start on the time axis, in time units. </summary>
        [Tooltip("The clip's start on the time axis, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float start
        {
            get => m_Start;
            set
            {
                if (Mathf.Approximately(m_Start, value))
                    return;
                m_Start = value;
                OnAxisChanged();
                NotifyPropertyChanged(in startProperty);
            }
        }

        /// <summary> The clip's length, in time units. </summary>
        [Tooltip("The clip's length, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float duration
        {
            get => m_Duration;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_Duration, v))
                    return;
                m_Duration = v;
                OnAxisChanged();
                NotifyPropertyChanged(in durationProperty);
            }
        }

        /// <summary> The text drawn inside the bar. </summary>
        [Tooltip("The text drawn inside the bar.")]
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                if (m_Label.text == value)
                    return;
                m_Label.text = value;
                NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary> The bar fill. Unset falls back to the track colour, then to the stylesheet. </summary>
        [Tooltip("The bar fill. Unset falls back to the track colour.")]
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
                RefreshFill();
                NotifyPropertyChanged(in colorProperty);
            }
        }

        /// <summary> Whether dragging the body moves the clip. </summary>
        [Tooltip("Whether dragging the body moves the clip.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool draggable
        {
            get => m_Draggable;
            set
            {
                if (m_Draggable == value)
                    return;
                m_Draggable = value;
                NotifyPropertyChanged(in draggableProperty);
            }
        }

        /// <summary>
        /// Whether dragging an edge resizes the clip. Until assigned, it follows the
        /// <see cref="variant"/>: true for a <see cref="TimelineClipVariant.Bar"/>, false for the
        /// handle and surface variants, which draw no span to take hold of.
        /// </summary>
        [Tooltip("Whether dragging an edge resizes the clip.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool resizable
        {
            get => m_ResizableSet ? m_Resizable : m_Variant == TimelineClipVariant.Bar;
            set
            {
                var before = resizable;
                m_Resizable = value;
                m_ResizableSet = true;
                RefreshEdges();
                if (before != value)
                    NotifyPropertyChanged(in resizableProperty);
            }
        }

        /// <summary>
        /// How the clip draws its span. <see cref="TimelineClipVariant.Bar"/> by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a track whose lane already shows the span, a bar would say it twice:
        /// <see cref="TimelineClipVariant.Handle"/> draws only a drag handle at the clip's start, and
        /// <see cref="TimelineClipVariant.Surface"/> draws nothing, so the lane itself is what you take
        /// hold of to move the clip.
        /// </para>
        /// </remarks>
        [Tooltip("How the clip draws its span.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineClipVariant variant
        {
            get => m_Variant;
            set
            {
                if (m_Variant == value)
                    return;
                var wasResizable = resizable;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));
                RefreshEdges();
                RefreshFrame();
                RefreshFill();
                NotifyPropertyChanged(in variantProperty);
                if (wasResizable != resizable)
                    NotifyPropertyChanged(in resizableProperty);
            }
        }

        static Icon CreateDragHandleIcon()
        {
            var icon = new Icon { iconName = "crosshair", pickingMode = PickingMode.Ignore };
            icon.AddToClassList(dragHandleIconUssClassName);
            return icon;
        }

        void RefreshEdges()
        {
            var r = resizable;
            m_LeftEdge.EnableInClassList(Styles.hiddenUssClassName, !r);
            m_RightEdge.EnableInClassList(Styles.hiddenUssClassName, !r);
        }

        /// <summary> The smallest duration a resize will leave, in time units. </summary>
        [Tooltip("The smallest duration a resize will leave, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minDuration
        {
            get => m_MinDuration;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_MinDuration, v))
                    return;
                m_MinDuration = v;
                NotifyPropertyChanged(in minDurationProperty);
            }
        }

        /// <summary> Whether the clip draws its selected outline. </summary>
        [Tooltip("Whether the clip draws its selected outline.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected == value)
                    return;
                m_Selected = value;
                EnableInClassList(Styles.selectedUssClassName, value);
                NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary> Whether the clip renders as a translucent placement frame. </summary>
        /// <remarks>
        /// <para>
        /// A frame marks where something sits without claiming the lane the way a solid clip does:
        /// the body is translucent and pointer-transparent, so a click passes through to the lane
        /// behind it, and only the handle strip along the top drags the frame.
        /// </para>
        /// </remarks>
        [Tooltip("Whether the clip renders as a translucent placement frame.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool frame
        {
            get => m_Frame;
            set
            {
                if (m_Frame == value)
                    return;
                m_Frame = value;
                RefreshFrame();
                NotifyPropertyChanged(in frameProperty);
            }
        }

        /// <summary> The hue a frame clip takes. <see cref="TimelineFrameColor.Purple"/> by default. </summary>
        [Tooltip("The hue a frame clip takes.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineFrameColor frameColor
        {
            get => m_FrameColor;
            set
            {
                if (m_FrameColor == value)
                    return;
                RemoveFromClassList(GetFrameColorUssClassName(m_FrameColor));
                m_FrameColor = value;
                AddToClassList(GetFrameColorUssClassName(m_FrameColor));
                NotifyPropertyChanged(in frameColorProperty);
            }
        }

        /// <summary>
        /// An explicit height for a frame clip, in pixels, drawn from the lane top. Zero, the
        /// default, fills the lane.
        /// </summary>
        /// <remarks>
        /// The handle strip is the smaller of this and <see cref="maxFrameHandleHeight"/>, so a
        /// tall frame does not turn its whole body into a drag surface.
        /// </remarks>
        [Tooltip("An explicit height for a frame clip, in pixels. Zero fills the lane.")]
        [CreateProperty]
        [UxmlAttribute]
        public float frameHeight
        {
            get => m_FrameHeight;
            set
            {
                if (Mathf.Approximately(m_FrameHeight, value))
                    return;
                m_FrameHeight = Mathf.Max(0f, value);
                RefreshFrame();
                NotifyPropertyChanged(in frameHeightProperty);
            }
        }

        void RefreshFrame()
        {
            EnableInClassList(frameUssClassName, m_Frame);
            EnableInClassList(GetFrameColorUssClassName(m_FrameColor), m_Frame);
            var handleOnly = m_Variant == TimelineClipVariant.Handle;
            m_Fill.EnableInClassList(Styles.hiddenUssClassName, !m_Frame);
            m_DragHandle.EnableInClassList(Styles.hiddenUssClassName, !m_Frame && !handleOnly);
            m_DragHandleIcon.EnableInClassList(Styles.hiddenUssClassName, !handleOnly);

            // The body only stops taking pointers while it is a frame or a bare handle; a solid
            // clip, and a surface clip, are their own drag surface and must keep doing so.
            pickingMode = m_Frame || handleOnly ? PickingMode.Ignore : PickingMode.Position;

            if (m_Frame && m_FrameHeight > 0f)
            {
                style.bottom = StyleKeyword.Auto;
                style.height = m_FrameHeight;
                m_DragHandle.style.height = Mathf.Min(m_FrameHeight, maxFrameHandleHeight);
            }
            else
            {
                style.bottom = StyleKeyword.Null;
                style.height = StyleKeyword.Null;
                // A handle clip's grip fills the clip's own height, so it cannot reach into the next
                // lane; only a frame's strip has the fixed height.
                m_DragHandle.style.height = m_Frame ? new StyleLength(maxFrameHandleHeight) : new StyleLength(StyleKeyword.Null);
            }
        }

        internal void RefreshFill()
        {
            var resolved = m_Color.IsSet
                ? m_Color
                : (parent as TimelineTrack)?.color ?? Optional<Color>.none;
            // A handle or surface clip draws nothing over the span: the lane behind shows it. The
            // colour goes to the handle's icon instead.
            var bare = m_Variant != TimelineClipVariant.Bar;
            style.backgroundColor = resolved.IsSet && !bare
                ? new StyleColor(resolved.Value)
                : new StyleColor(StyleKeyword.Null);
            // Icon is an Image, which paints through tintColor rather than a background tint. An
            // assigned tintColor is inline for good, so the stylesheet tint (and its disabled rule)
            // is left alone unless there is a colour, and an icon that was tinted is replaced when
            // the colour goes, handing the tint back to the stylesheet.
            if (m_DragHandleIcon != null)
            {
                var tint = resolved.IsSet && bare;
                if (tint)
                {
                    m_DragHandleIcon.tintColor = resolved.Value;
                    m_IconTinted = true;
                }
                else if (m_IconTinted)
                {
                    var fresh = CreateDragHandleIcon();
                    fresh.EnableInClassList(Styles.hiddenUssClassName,
                        m_DragHandleIcon.ClassListContains(Styles.hiddenUssClassName));
                    m_DragHandle.Remove(m_DragHandleIcon);
                    m_DragHandleIcon = fresh;
                    m_DragHandle.Add(fresh);
                    m_IconTinted = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            RefreshFill();
            if (axis == null)
                return;
            style.left = axis.TimeToX(m_Start);
            style.width = Mathf.Max(4f, m_Duration * axis.pxPerUnit);
        }

        void BeginDrag(PointerDownEvent evt, Op op)
        {
            if (axis == null || axis.disabled)
                return;
            if (op == Op.Move && !m_Draggable)
                return;
            if (op != Op.Move && !m_Resizable)
                return;

            m_Op = op;
            m_PointerId = evt.pointerId;
            m_PointerStartX = evt.position.x;
            m_DragStart = m_Start;
            m_DragDuration = m_Duration;
            m_LastStart = m_Start;
            m_LastDuration = m_Duration;

            var target = (VisualElement)evt.currentTarget;
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            timeline?.TrackPointer(evt.position);
            if (m_Op == Op.None || m_PointerId != evt.pointerId || axis == null)
                return;

            var dt = (evt.position.x - m_PointerStartX) / axis.pxPerUnit;
            var stp = axis.step;
            var minD = stp > 0f ? Mathf.Max(m_MinDuration, stp) : m_MinDuration;
            var endT = m_DragStart + m_DragDuration;

            float ns, nd;
            switch (m_Op)
            {
                case Op.Move:
                    ns = Mathf.Clamp(Snap(m_DragStart + dt, stp), 0f, Mathf.Max(0f, axis.duration - m_DragDuration));
                    nd = m_DragDuration;
                    break;
                case Op.ResizeLeft:
                    ns = Mathf.Clamp(Snap(m_DragStart + dt, stp), 0f, endT - minD);
                    nd = endT - ns;
                    break;
                default:
                    var ne = Mathf.Clamp(Snap(endT + dt, stp), m_DragStart + minD, axis.duration);
                    ns = m_DragStart;
                    nd = ne - m_DragStart;
                    break;
            }

            m_LastStart = ns;
            m_LastDuration = nd;
            style.left = axis.TimeToX(ns);
            style.width = Mathf.Max(4f, nd * axis.pxPerUnit);
            dragMove?.Invoke(ns, nd);
            evt.StopPropagation();
        }

        static float Snap(float v, float step) => step > 0f ? Mathf.Round(v / step) * step : v;

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_Op == Op.None || m_PointerId != evt.pointerId)
            {
                if (m_Op == Op.None && ReferenceEquals(evt.currentTarget, this))
                    clicked?.Invoke();
                return;
            }

            var target = (VisualElement)evt.currentTarget;
            if (target.HasPointerCapture(evt.pointerId))
                target.ReleasePointer(evt.pointerId);

            var moved = !Mathf.Approximately(m_LastStart, m_DragStart) ||
                !Mathf.Approximately(m_LastDuration, m_DragDuration);
            m_Op = Op.None;
            m_PointerId = PointerId.invalidPointerId;

            if (moved)
            {
                start = m_LastStart;
                duration = m_LastDuration;
                changed?.Invoke(m_LastStart, m_LastDuration);
            }
            else
            {
                clicked?.Invoke();
                OnAxisChanged();
            }
            evt.StopPropagation();
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            m_Op = Op.None;
            m_PointerId = PointerId.invalidPointerId;
            OnAxisChanged();
        }
    }

    /// <summary>
    /// A point keyframe on a <see cref="TimelineTrack"/>.
    /// </summary>
    /// <remarks>
    /// The glyph is centred on its <see cref="time"/> — a diamond by default, matching the animation
    /// editors. Turn on <see cref="draggable"/> to let the user re-time it; the new time is committed
    /// once on release through <see cref="moved"/>, snapped to the Timeline's
    /// <see cref="Timeline.step"/>.
    /// </remarks>
    [UxmlElement]
    public partial class TimelineKeyframe : TimelinePart
    {
        internal static readonly BindingId idProperty = nameof(id);
        internal static readonly BindingId timeProperty = nameof(time);
        internal static readonly BindingId colorProperty = nameof(color);
        internal static readonly BindingId shapeProperty = nameof(shape);
        internal static readonly BindingId selectedProperty = nameof(selected);
        internal static readonly BindingId draggableProperty = nameof(draggable);
        internal static readonly BindingId mutedProperty = nameof(muted);
        internal static readonly BindingId sizeProperty = nameof(size);

        /// <summary> The styling class applied while the keyframe draws its small glyph. </summary>
        public const string smallUssClassName = ussClassName + "--size-small";

        TimelineKeySize m_Size = TimelineKeySize.Auto;

        [EnumName("GetShapeUssClassName", typeof(TimelineKeyframeShape))]
        const string k_ShapeUssClassNamePrefix = ussClassName + "--shape-";

        /// <summary> The TimelineKeyframe main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__keyframe";

        /// <summary> The styling class applied while the keyframe is muted. </summary>
        public const string mutedUssClassName = ussClassName + "--muted";

        /// <summary> The styling class of the glyph inside the keyframe's hit target. </summary>
        public const string glyphUssClassName = ussClassName + "-glyph";

        readonly VisualElement m_Glyph;

        bool m_Muted;

        string m_Id;
        float m_Time;
        Optional<Color> m_Color;
        TimelineKeyframeShape m_Shape = TimelineKeyframeShape.Diamond;
        bool m_Selected;
        bool m_Draggable;

        int m_PointerId = PointerId.invalidPointerId;
        float m_PointerStartX;
        float m_DragTime;
        float m_LastTime;

        /// <summary> Raised once on release with the committed time. </summary>
        public event Action<float> moved;

        /// <summary> Raised when the keyframe is clicked without dragging. </summary>
        public event Action clicked;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineKeyframe()
        {
            AddToClassList(ussClassName);
            focusable = true;

            m_Glyph = new VisualElement { name = glyphUssClassName, pickingMode = PickingMode.Ignore };
            m_Glyph.AddToClassList(glyphUssClassName);
            hierarchy.Add(m_Glyph);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            shape = TimelineKeyframeShape.Diamond;
        }

        /// <summary> A stable identifier for the keyframe. </summary>
        [Tooltip("A stable identifier for the keyframe.")]
        [CreateProperty]
        [UxmlAttribute]
        public string id
        {
            get => m_Id;
            set
            {
                if (m_Id == value)
                    return;
                m_Id = value;
                NotifyPropertyChanged(in idProperty);
            }
        }

        /// <summary> The keyframe's position on the time axis, in time units. </summary>
        [Tooltip("The keyframe's position on the time axis, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float time
        {
            get => m_Time;
            set
            {
                if (Mathf.Approximately(m_Time, value))
                    return;
                m_Time = value;
                OnAxisChanged();
                NotifyPropertyChanged(in timeProperty);
            }
        }

        /// <summary> The glyph fill. Unset falls back to the track colour, then to the stylesheet. </summary>
        [Tooltip("The glyph fill. Unset falls back to the track colour.")]
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
                RefreshFill();
                NotifyPropertyChanged(in colorProperty);
            }
        }

        /// <summary> The glyph shape. </summary>
        [Tooltip("The glyph shape.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineKeyframeShape shape
        {
            get => m_Shape;
            set
            {
                m_Glyph.RemoveFromClassList(GetShapeUssClassName(m_Shape));
                m_Shape = value;
                m_Glyph.AddToClassList(GetShapeUssClassName(m_Shape));
                NotifyPropertyChanged(in shapeProperty);
            }
        }

        /// <summary> Whether the keyframe draws its selected outline. </summary>
        [Tooltip("Whether the keyframe draws its selected outline.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected == value)
                    return;
                m_Selected = value;
                EnableInClassList(Styles.selectedUssClassName, value);
                NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary> Whether the keyframe is dimmed to a smaller, quieter marker. </summary>
        /// <remarks>
        /// For a key that exists but is not in play -- a muted track's, or one on a layer the
        /// editor is not currently soloing. It stays pickable: muted is about emphasis, not
        /// interactivity.
        /// </remarks>
        [Tooltip("Whether the keyframe is dimmed to a smaller, quieter marker.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool muted
        {
            get => m_Muted;
            set
            {
                if (m_Muted == value)
                    return;
                m_Muted = value;
                EnableInClassList(mutedUssClassName, value);
                RefreshSize();
                NotifyPropertyChanged(in mutedProperty);
            }
        }

        /// <summary> Whether dragging re-times the keyframe. </summary>
        [Tooltip("Whether dragging re-times the keyframe.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool draggable
        {
            get => m_Draggable;
            set
            {
                if (m_Draggable == value)
                    return;
                m_Draggable = value;
                NotifyPropertyChanged(in draggableProperty);
            }
        }

        /// <summary>
        /// The glyph size. <see cref="TimelineKeySize.Auto"/>, the default, defers to the Timeline's
        /// <see cref="Timeline.keySize"/>, then to small for a muted key and regular otherwise.
        /// </summary>
        [Tooltip("The glyph size. Auto defers to the Timeline's keySize, then to small when muted.")]
        [CreateProperty]
        [UxmlAttribute]
        public TimelineKeySize size
        {
            get => m_Size;
            set
            {
                if (m_Size == value)
                    return;
                m_Size = value;
                RefreshSize();
                NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary> The size the glyph actually draws at, after deferring to the Timeline and to <see cref="muted"/>. </summary>
        public TimelineKeySize resolvedSize
        {
            get
            {
                if (m_Size != TimelineKeySize.Auto)
                    return m_Size;
                var sheet = axis?.keySize ?? TimelineKeySize.Auto;
                if (sheet != TimelineKeySize.Auto)
                    return sheet;
                return m_Muted ? TimelineKeySize.Small : TimelineKeySize.Regular;
            }
        }

        void RefreshSize() => EnableInClassList(smallUssClassName, resolvedSize == TimelineKeySize.Small);

        internal void RefreshFill()
        {
            var resolved = m_Color.IsSet
                ? m_Color
                : (parent as TimelineTrack)?.color ?? Optional<Color>.none;
            m_Glyph.style.backgroundColor = resolved.IsSet
                ? new StyleColor(resolved.Value)
                : new StyleColor(StyleKeyword.Null);
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            RefreshFill();
            RefreshSize();
            if (axis == null)
                return;
            style.left = axis.TimeToX(m_Time);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!m_Draggable || axis == null || axis.disabled)
                return;
            m_PointerId = evt.pointerId;
            m_PointerStartX = evt.position.x;
            m_DragTime = m_Time;
            m_LastTime = m_Time;
            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            timeline?.TrackPointer(evt.position);
            if (m_PointerId != evt.pointerId || axis == null)
                return;
            var dt = (evt.position.x - m_PointerStartX) / axis.pxPerUnit;
            m_LastTime = axis.owner != null ? axis.owner.SnapTime(m_DragTime + dt) : m_DragTime + dt;
            style.left = axis.TimeToX(m_LastTime);
            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_PointerId != evt.pointerId)
            {
                clicked?.Invoke();
                return;
            }

            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
            m_PointerId = PointerId.invalidPointerId;

            if (!Mathf.Approximately(m_LastTime, m_DragTime))
            {
                time = m_LastTime;
                moved?.Invoke(m_LastTime);
            }
            else
            {
                clicked?.Invoke();
                OnAxisChanged();
            }
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// The time ruler at the top of a <see cref="Timeline"/>.
    /// </summary>
    /// <remarks>
    /// The ruler draws one tick per <see cref="Timeline.minorTick"/> across the duration and labels
    /// every <see cref="Timeline.majorTick"/>. Dragging it seeks the playhead; Shift + dragging draws
    /// a range selection into <see cref="Timeline.selection"/>. Set <see cref="seekOnClick"/> to
    /// `false` for a purely decorative ruler.
    ///
    /// A Timeline creates its own ruler — reach it through <see cref="Timeline.ruler"/> rather than
    /// adding a second one.
    /// </remarks>
    [UxmlElement]
    public partial class TimelineRuler : TimelinePart
    {
        internal static readonly BindingId seekOnClickProperty = nameof(seekOnClick);

        /// <summary> The TimelineRuler main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__ruler";

        /// <summary> The styling class of one ruler tick. </summary>
        public const string tickUssClassName = ussClassName + "-tick";

        /// <summary> The styling class of a major, labelled tick. </summary>
        public const string majorTickUssClassName = tickUssClassName + "--major";

        /// <summary> The styling class of a major tick's label. </summary>
        public const string tickLabelUssClassName = ussClassName + "-label";

        /// <summary> The styling class applied while the ticks hang from the ruler's top edge. </summary>
        public const string ticksTopUssClassName = ussClassName + "--ticks-top";

        readonly List<TimelineTick> m_Ticks = new List<TimelineTick>();

        bool m_SeekOnClick = true;
        bool m_Seeking;
        bool m_Selecting;
        float m_Anchor;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineRuler()
        {
            AddToClassList(ussClassName);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(_ => EndGesture());
        }

        /// <summary> Whether dragging the ruler seeks the playhead. </summary>
        [Tooltip("Whether dragging the ruler seeks the playhead.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool seekOnClick
        {
            get => m_SeekOnClick;
            set
            {
                if (m_SeekOnClick == value)
                    return;
                m_SeekOnClick = value;
                NotifyPropertyChanged(in seekOnClickProperty);
            }
        }

        /// <inheritdoc />
        protected override void OnAxisChanged() => RebuildTicks();

        /// <summary>
        /// Rebuild the tick elements from the current axis and the owner's fine tick level.
        /// </summary>
        internal void RebuildTicks()
        {
            Clear();
            Timeline.CollectTicks(axis, m_Ticks);

            var format = axis?.owner?.formatTime;
            foreach (var tickInfo in m_Ticks)
            {
                var t = tickInfo.time;
                var isMajor = tickInfo.major;
                var x = axis.TimeToX(t);

                var tick = new VisualElement { pickingMode = PickingMode.Ignore };
                tick.AddToClassList(tickUssClassName);
                tick.EnableInClassList(majorTickUssClassName, isMajor);
                tick.style.left = x;
                if (tickInfo.opacity < 1f)
                    tick.style.opacity = tickInfo.opacity;
                Add(tick);

                if (!isMajor)
                    continue;

                var label = new LocalizedTextElement(format != null ? format(t) : t.ToString(CultureInfo.InvariantCulture))
                {
                    pickingMode = PickingMode.Ignore,
                };
                label.AddToClassList(tickLabelUssClassName);
                label.style.left = x;
                Add(label);
            }
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!m_SeekOnClick || axis?.owner == null || axis.disabled || evt.button == (int)MouseButton.MiddleMouse)
                return;

            var owner = axis.owner;
            var t = owner.ViewportXToTime(owner.viewport.WorldToLocal(evt.position).x);

            if (evt.shiftKey)
            {
                m_Selecting = true;
                m_Anchor = Mathf.Clamp(t, 0f, axis.duration);
                owner.selection = new TimelineSelection(m_Anchor, m_Anchor);
            }
            else
            {
                m_Seeking = true;
                owner.value = t;
            }

            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            timeline?.TrackPointer(evt.position);
            if (axis?.owner == null || (!m_Seeking && !m_Selecting))
                return;

            var owner = axis.owner;
            var t = owner.ViewportXToTime(owner.viewport.WorldToLocal(evt.position).x);

            if (m_Seeking)
                owner.value = t;
            else
                owner.selection = new TimelineSelection(m_Anchor, Mathf.Clamp(t, 0f, axis.duration));

            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
            EndGesture();
        }

        void EndGesture()
        {
            m_Seeking = false;
            m_Selecting = false;
        }
    }

    /// <summary>
    /// The faint per-frame guide lines drawn across a <see cref="Timeline"/>'s lanes.
    /// </summary>
    /// <remarks>
    /// The grid is painted with <see cref="Painter2D"/> in one pass rather than as hundreds of
    /// elements, so a fully zoomed-out timeline costs the same as an empty one. Its two colours are
    /// custom style properties — `--grid-minor-color` and `--grid-major-color` — bound to App UI
    /// tokens in `Timeline.uss`, so the painted lines follow the theme like everything else.
    ///
    /// Minor lines are dropped below <see cref="minMinorPxPerUnit"/> effective pixels per unit, where
    /// they would alias into a solid wash.
    /// </remarks>
    [UxmlElement]
    public partial class TimelineGrid : TimelinePart
    {
        internal static readonly BindingId showMinorProperty = nameof(showMinor);
        internal static readonly BindingId showMajorProperty = nameof(showMajor);
        internal static readonly BindingId minMinorPxPerUnitProperty = nameof(minMinorPxPerUnit);

        /// <summary> The TimelineGrid main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__grid";

        static readonly CustomStyleProperty<Color> k_MinorColorProperty =
            new CustomStyleProperty<Color>("--grid-minor-color");

        static readonly CustomStyleProperty<Color> k_MajorColorProperty =
            new CustomStyleProperty<Color>("--grid-major-color");

        Color m_MinorColor = Color.clear;
        Color m_MajorColor = Color.clear;

        readonly List<TimelineTick> m_Ticks = new List<TimelineTick>();

        bool m_ShowMinor = true;
        bool m_ShowMajor = true;
        float m_MinMinorPxPerUnit = 6f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineGrid()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary> Whether minor-tick lines are drawn. </summary>
        [Tooltip("Whether minor-tick lines are drawn.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showMinor
        {
            get => m_ShowMinor;
            set
            {
                if (m_ShowMinor == value)
                    return;
                m_ShowMinor = value;
                MarkDirtyRepaint();
                NotifyPropertyChanged(in showMinorProperty);
            }
        }

        /// <summary> Whether major-tick lines are drawn. </summary>
        [Tooltip("Whether major-tick lines are drawn.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showMajor
        {
            get => m_ShowMajor;
            set
            {
                if (m_ShowMajor == value)
                    return;
                m_ShowMajor = value;
                MarkDirtyRepaint();
                NotifyPropertyChanged(in showMajorProperty);
            }
        }

        /// <summary> The effective pixels-per-unit below which minor lines are dropped. </summary>
        [Tooltip("The effective pixels-per-unit below which minor lines are dropped.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minMinorPxPerUnit
        {
            get => m_MinMinorPxPerUnit;
            set
            {
                if (Mathf.Approximately(m_MinMinorPxPerUnit, value))
                    return;
                m_MinMinorPxPerUnit = value;
                MarkDirtyRepaint();
                NotifyPropertyChanged(in minMinorPxPerUnitProperty);
            }
        }

        /// <inheritdoc />
        protected override void OnAxisChanged() => MarkDirtyRepaint();

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(k_MinorColorProperty, out Color minor))
                m_MinorColor = minor;
            if (evt.customStyle.TryGetValue(k_MajorColorProperty, out Color major))
                m_MajorColor = major;
            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (axis == null || axis.minorTick <= 0f)
                return;

            var height = contentRect.height;
            if (height <= 0f)
                return;

            var drawMinor = m_ShowMinor && axis.pxPerUnit >= m_MinMinorPxPerUnit;
            var drawMajor = m_ShowMajor && axis.pxPerUnit >= 2f;
            if (!drawMinor && !drawMajor)
                return;

            Timeline.CollectTicks(axis, m_Ticks);

            var painter = context.painter2D;
            painter.lineWidth = 1f;

            foreach (var tick in m_Ticks)
            {
                var t = tick.time;
                var isMajor = tick.major;

                if (isMajor ? !drawMajor : !drawMinor || t <= 0f)
                    continue;

                var color = isMajor ? m_MajorColor : m_MinorColor;
                color.a *= tick.opacity;
                painter.strokeColor = color;
                painter.BeginPath();
                painter.MoveTo(new Vector2(axis.TimeToX(t), 0f));
                painter.LineTo(new Vector2(axis.TimeToX(t), height));
                painter.Stroke();
            }
        }
    }

    /// <summary>
    /// A named marker — a flagged vertical line spanning a <see cref="Timeline"/>.
    /// </summary>
    /// <remarks>
    /// Add markers through <see cref="Timeline.AddMarker"/>, which puts them in the layer above every
    /// lane. A marker is inert chrome: it never takes the pointer, so a clip underneath it stays
    /// draggable.
    /// </remarks>
    [UxmlElement]
    public partial class TimelineMarker : TimelinePart
    {
        internal static readonly BindingId timeProperty = nameof(time);
        internal static readonly BindingId labelProperty = nameof(label);
        internal static readonly BindingId colorProperty = nameof(color);
        internal static readonly BindingId showGuideProperty = nameof(showGuide);

        /// <summary> The TimelineMarker main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__marker";

        /// <summary> The styling class of the marker's full-height guide line. </summary>
        public const string guideUssClassName = ussClassName + "-guide";

        /// <summary> The styling class of the marker's flag. </summary>
        public const string flagUssClassName = ussClassName + "-flag";

        /// <summary> The styling class of the marker's label. </summary>
        public const string labelUssClassName = ussClassName + "-label";

        readonly VisualElement m_Guide;
        readonly VisualElement m_Flag;
        readonly LocalizedTextElement m_Label;

        float m_Time;
        Optional<Color> m_Color;
        bool m_ShowGuide = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineMarker()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Guide = new VisualElement { name = guideUssClassName, pickingMode = PickingMode.Ignore };
            m_Guide.AddToClassList(guideUssClassName);
            hierarchy.Add(m_Guide);

            m_Flag = new VisualElement { name = flagUssClassName, pickingMode = PickingMode.Ignore };
            m_Flag.AddToClassList(flagUssClassName);
            hierarchy.Add(m_Flag);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);
        }

        /// <summary> The marker's position on the time axis, in time units. </summary>
        [Tooltip("The marker's position on the time axis, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float time
        {
            get => m_Time;
            set
            {
                if (Mathf.Approximately(m_Time, value))
                    return;
                m_Time = value;
                OnAxisChanged();
                NotifyPropertyChanged(in timeProperty);
            }
        }

        /// <summary> The short text shown beside the flag. </summary>
        [Tooltip("The short text shown beside the flag.")]
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                if (m_Label.text == value)
                    return;
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
                NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary> The marker colour. Unset falls back to the stylesheet's muted foreground. </summary>
        [Tooltip("The marker colour. Unset falls back to the muted foreground token.")]
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                if (m_Color.Equals(value))
                    return;
                m_Color = value;
                var c = value.IsSet ? new StyleColor(value.Value) : new StyleColor(StyleKeyword.Null);
                m_Guide.style.backgroundColor = c;
                m_Flag.style.backgroundColor = c;
                m_Label.style.color = c;
                NotifyPropertyChanged(in colorProperty);
            }
        }

        /// <summary> Whether the faint full-height guide line is drawn. </summary>
        [Tooltip("Whether the faint full-height guide line is drawn.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showGuide
        {
            get => m_ShowGuide;
            set
            {
                if (m_ShowGuide == value)
                    return;
                m_ShowGuide = value;
                m_Guide.EnableInClassList(Styles.hiddenUssClassName, !value);
                NotifyPropertyChanged(in showGuideProperty);
            }
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            if (axis == null)
                return;
            style.left = axis.TimeToX(m_Time);
        }
    }

    /// <summary>
    /// The range-selection band of a <see cref="Timeline"/>.
    /// </summary>
    /// <remarks>
    /// The band mirrors <see cref="Timeline.selection"/>, which is seeded either from code or by
    /// Shift + dragging the ruler. Its two edge handles sit over the ruler row so the lanes below stay
    /// clickable, and dragging one moves that bound alone.
    ///
    /// A Timeline creates its own band — reach it through <see cref="Timeline.range"/>.
    /// </remarks>
    [UxmlElement]
    public partial class TimelineRange : TimelinePart
    {
        /// <summary> The TimelineRange main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__range";

        /// <summary> The styling class of the translucent band. </summary>
        public const string bandUssClassName = ussClassName + "-band";

        /// <summary> The styling class of a band edge handle. </summary>
        public const string handleUssClassName = ussClassName + "-handle";

        readonly VisualElement m_Band;
        readonly VisualElement m_StartHandle;
        readonly VisualElement m_EndHandle;

        int m_PointerId = PointerId.invalidPointerId;
        bool m_DraggingStart;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineRange()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Band = new VisualElement { name = bandUssClassName, pickingMode = PickingMode.Ignore };
            m_Band.AddToClassList(bandUssClassName);
            hierarchy.Add(m_Band);

            m_StartHandle = new VisualElement { name = handleUssClassName + "-start" };
            m_StartHandle.AddToClassList(handleUssClassName);
            hierarchy.Add(m_StartHandle);

            m_EndHandle = new VisualElement { name = handleUssClassName + "-end" };
            m_EndHandle.AddToClassList(handleUssClassName);
            hierarchy.Add(m_EndHandle);

            m_StartHandle.RegisterCallback<PointerDownEvent>(evt => BeginHandleDrag(evt, true));
            m_EndHandle.RegisterCallback<PointerDownEvent>(evt => BeginHandleDrag(evt, false));
            m_StartHandle.RegisterCallback<PointerMoveEvent>(OnHandleMove);
            m_EndHandle.RegisterCallback<PointerMoveEvent>(OnHandleMove);
            m_StartHandle.RegisterCallback<PointerUpEvent>(OnHandleUp);
            m_EndHandle.RegisterCallback<PointerUpEvent>(OnHandleUp);
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            var sel = axis?.selection;
            var visible = axis != null && sel.HasValue && sel.Value.length > 0f;
            EnableInClassList(Styles.hiddenUssClassName, !visible);
            if (!visible)
                return;

            var left = axis.TimeToX(sel.Value.start);
            var width = sel.Value.length * axis.pxPerUnit;
            style.left = left;
            style.width = width;
            m_StartHandle.style.height = axis.rulerHeight;
            m_EndHandle.style.height = axis.rulerHeight;
        }

        void BeginHandleDrag(PointerDownEvent evt, bool isStart)
        {
            if (axis?.owner == null || axis.disabled)
                return;
            m_PointerId = evt.pointerId;
            m_DraggingStart = isStart;
            ((VisualElement)evt.currentTarget).CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnHandleMove(PointerMoveEvent evt)
        {
            timeline?.TrackPointer(evt.position);
            if (m_PointerId != evt.pointerId || axis?.owner == null || !axis.selection.HasValue)
                return;

            var owner = axis.owner;
            var t = Mathf.Clamp(owner.ViewportXToTime(owner.viewport.WorldToLocal(evt.position).x), 0f, axis.duration);
            var sel = axis.selection.Value;
            owner.selection = m_DraggingStart
                ? new TimelineSelection(t, sel.end)
                : new TimelineSelection(sel.start, t);
            evt.StopPropagation();
        }

        void OnHandleUp(PointerUpEvent evt)
        {
            var target = (VisualElement)evt.currentTarget;
            if (target.HasPointerCapture(evt.pointerId))
                target.ReleasePointer(evt.pointerId);
            m_PointerId = PointerId.invalidPointerId;
        }
    }

    /// <summary>
    /// The playhead scrubber of a <see cref="Timeline"/>.
    /// </summary>
    /// <remarks>
    /// A full-height line with a grab handle at the top, sitting at <see cref="Timeline.value"/>.
    /// Dragging it seeks; it is focusable and answers the left and right arrows by one
    /// <see cref="Timeline.step"/>, and Home and End by jumping to either end of the duration.
    /// Turn on <see cref="showLabel"/> for a bubble showing the formatted current time.
    ///
    /// A Timeline creates its own playhead — reach it through <see cref="Timeline.playhead"/>.
    /// </remarks>
    [UxmlElement]
    public partial class TimelinePlayhead : TimelinePart
    {
        internal static readonly BindingId showHandleProperty = nameof(showHandle);
        internal static readonly BindingId showLabelProperty = nameof(showLabel);

        /// <summary> The TimelinePlayhead main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__playhead";

        /// <summary> The styling class of the playhead's vertical line. </summary>
        public const string lineUssClassName = ussClassName + "-line";

        /// <summary> The styling class of the playhead's grab handle. </summary>
        public const string handleUssClassName = ussClassName + "-handle";

        /// <summary> The styling class of the playhead's time bubble. </summary>
        public const string labelUssClassName = ussClassName + "-label";

        readonly VisualElement m_Line;
        readonly VisualElement m_Handle;
        readonly LocalizedTextElement m_Label;

        bool m_ShowHandle = true;
        bool m_ShowLabel;
        int m_PointerId = PointerId.invalidPointerId;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelinePlayhead()
        {
            AddToClassList(ussClassName);
            focusable = true;

            m_Line = new VisualElement { name = lineUssClassName, pickingMode = PickingMode.Ignore };
            m_Line.AddToClassList(lineUssClassName);
            hierarchy.Add(m_Line);

            m_Handle = new VisualElement { name = handleUssClassName, pickingMode = PickingMode.Ignore };
            m_Handle.AddToClassList(handleUssClassName);
            hierarchy.Add(m_Handle);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            m_Label.AddToClassList(Styles.hiddenUssClassName);
            hierarchy.Add(m_Label);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary> Whether the grab handle is drawn at the top of the line. </summary>
        [Tooltip("Whether the grab handle is drawn at the top of the line.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showHandle
        {
            get => m_ShowHandle;
            set
            {
                if (m_ShowHandle == value)
                    return;
                m_ShowHandle = value;
                m_Handle.EnableInClassList(Styles.hiddenUssClassName, !value);
                NotifyPropertyChanged(in showHandleProperty);
            }
        }

        /// <summary> Whether the formatted current time is shown in a bubble. </summary>
        [Tooltip("Whether the formatted current time is shown in a bubble.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool showLabel
        {
            get => m_ShowLabel;
            set
            {
                if (m_ShowLabel == value)
                    return;
                m_ShowLabel = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, !value);
                OnAxisChanged();
                NotifyPropertyChanged(in showLabelProperty);
            }
        }

        /// <summary>
        /// The playhead is drawn above the lanes, so it gives way to a <see cref="TimelineHandle"/>
        /// under the same point: a grip parked at the playhead's time stays grabbable.
        /// </summary>
        /// <param name="localPoint"> The point, in this element's space. </param>
        /// <returns> Whether the playhead takes the point. </returns>
        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (!base.ContainsPoint(localPoint))
                return false;
            var owner = timeline;
            return owner == null || !owner.HandleContains(this.LocalToWorld(localPoint));
        }

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            if (axis == null)
                return;
            style.left = axis.TimeToX(axis.value);
            focusable = !axis.disabled;
            if (m_ShowLabel)
                m_Label.text = axis.owner?.formatTime(axis.value);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (axis?.owner == null || axis.disabled)
                return;
            m_PointerId = evt.pointerId;
            this.CapturePointer(evt.pointerId);
            Seek(evt.position.x);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            timeline?.TrackPointer(evt.position);
            if (m_PointerId != evt.pointerId)
                return;
            Seek(evt.position.x);
            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
            m_PointerId = PointerId.invalidPointerId;
        }

        void Seek(float worldX)
        {
            var owner = axis?.owner;
            if (owner == null)
                return;
            owner.value = owner.ViewportXToTime(owner.viewport.WorldToLocal(new Vector2(worldX, 0f)).x);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var owner = axis?.owner;
            if (owner == null || axis.disabled)
                return;

            var inc = axis.step > 0f ? axis.step : 1f;
            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                    owner.value = axis.value - inc;
                    break;
                case KeyCode.RightArrow:
                    owner.value = axis.value + inc;
                    break;
                case KeyCode.Home:
                    owner.value = 0f;
                    break;
                case KeyCode.End:
                    owner.value = axis.duration;
                    break;
                default:
                    return;
            }

            // Moved from the keyboard, the playhead pans the viewport along with it.
            owner.RevealTime(owner.value);
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// A grip on a boundary of a <see cref="Timeline"/>'s axis, the end of a range whose extent the
    /// lane already shows, for instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The handle has no look of its own beyond its focus ring: the host draws the edge, and this is
    /// the hit area astride it that drags it. Add it to a <see cref="TimelineTrack"/>. Dragging with
    /// the primary button reports the time under the pointer through <see cref="dragMove"/>, snapped
    /// to <see cref="Timeline.step"/> and clamped to <c>[min, max]</c>, and commits it once on
    /// release through <see cref="changed"/>. The boundary follows the pointer from where it was
    /// grabbed, and a press that never moves commits nothing. A lost pointer raises
    /// <see cref="dragCanceled"/>.
    /// </para>
    /// <para>
    /// It is focusable. The left and right arrows move it by <see cref="keyStep"/>, Home and End to
    /// either bound, and a keyboard move pans the Timeline to keep it in view. It sits above the
    /// playhead for picking, so a grip parked at the playhead's time is still the one grabbed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="csharp"><![CDATA[
    /// var end = new TimelineHandle { time = 48f };
    /// end.changed += t => clipLength = t;
    /// track.Add(end);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TimelineHandle : TimelinePart
    {
        internal static readonly BindingId timeProperty = nameof(time);
        internal static readonly BindingId minProperty = nameof(min);
        internal static readonly BindingId maxProperty = nameof(max);
        internal static readonly BindingId keyStepProperty = nameof(keyStep);

        /// <summary> The TimelineHandle main styling class. </summary>
        public const string ussClassName = Timeline.ussClassName + "__handle";

        float m_Time;
        float m_Min;
        float m_Max = -1f;
        float m_KeyStep;

        Timeline m_Owner;
        int m_PointerId = PointerId.invalidPointerId;
        bool m_Moved;
        float m_Grab;
        float m_DragTime;

        /// <summary> Raised on every pointer move during a drag with the time under the pointer, for a live preview. </summary>
        public event Action<float> dragMove;

        /// <summary> Raised with the committed time: once on a drag's release, and on every key that moves it. </summary>
        public event Action<float> changed;

        /// <summary> Raised when a drag is lost before release. Discard any preview held from <see cref="dragMove"/>. </summary>
        public event Action dragCanceled;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimelineHandle()
        {
            AddToClassList(ussClassName);
            focusable = true;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(_ => CancelDrag());
            RegisterCallback<PointerCaptureOutEvent>(_ => CancelDrag());
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                m_Owner = timeline;
                m_Owner?.RegisterHandle(this);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                m_Owner?.UnregisterHandle(this);
                m_Owner = null;
            });
        }

        /// <summary> Where the handle sits, in time units. </summary>
        [Tooltip("Where the handle sits, in time units.")]
        [CreateProperty]
        [UxmlAttribute]
        public float time
        {
            get => m_Time;
            set
            {
                var v = float.IsFinite(value) ? value : 0f;
                if (Mathf.Approximately(m_Time, v))
                    return;
                m_Time = v;
                OnAxisChanged();
                NotifyPropertyChanged(in timeProperty);
            }
        }

        /// <summary> The earliest time it can be dragged to. Zero by default. </summary>
        [Tooltip("The earliest time it can be dragged to.")]
        [CreateProperty]
        [UxmlAttribute]
        public float min
        {
            get => m_Min;
            set
            {
                if (Mathf.Approximately(m_Min, value))
                    return;
                m_Min = value;
                NotifyPropertyChanged(in minProperty);
            }
        }

        /// <summary> The latest time it can be dragged to. Negative, the default, is the Timeline's duration. </summary>
        [Tooltip("The latest time it can be dragged to. Negative is the Timeline's duration.")]
        [CreateProperty]
        [UxmlAttribute]
        public float max
        {
            get => m_Max;
            set
            {
                if (Mathf.Approximately(m_Max, value))
                    return;
                m_Max = value;
                NotifyPropertyChanged(in maxProperty);
            }
        }

        /// <summary>
        /// How far one arrow key moves it, in time units. Zero, the default, uses the Timeline's
        /// <see cref="Timeline.step"/>, else a hundredth of the range.
        /// </summary>
        [Tooltip("How far one arrow key moves it. Zero uses the Timeline's step, else a hundredth of the range.")]
        [CreateProperty]
        [UxmlAttribute]
        public float keyStep
        {
            get => m_KeyStep;
            set
            {
                var v = Mathf.Max(0f, value);
                if (Mathf.Approximately(m_KeyStep, v))
                    return;
                m_KeyStep = v;
                NotifyPropertyChanged(in keyStepProperty);
            }
        }

        /// <summary> The upper bound actually applied: <see cref="max"/>, or the Timeline's duration. </summary>
        public float resolvedMax => m_Max >= 0f ? m_Max : axis?.duration ?? 0f;

        float Clamp(float t) => Mathf.Clamp(t, m_Min, Mathf.Max(m_Min, resolvedMax));

        /// <inheritdoc />
        protected override void OnAxisChanged()
        {
            if (axis == null)
                return;
            // Mid-drag it holds the previewed time: a host that previews through the axis
            // (dragMove setting laneExtent, say) refreshes it, and must not snap it back.
            var dragging = m_PointerId != PointerId.invalidPointerId && m_Moved;
            style.left = axis.TimeToX(dragging ? m_DragTime : m_Time);
            focusable = !axis.disabled;
        }

        float PointerTime(Vector2 worldPosition)
        {
            var owner = axis?.owner;
            return owner == null ? m_Time : owner.ViewportXToTime(owner.viewport.WorldToLocal(worldPosition).x);
        }

        // The grab offset is added before the step snaps it, so the boundary still lands on a step.
        float At(Vector2 worldPosition)
        {
            var t = PointerTime(worldPosition) + m_Grab;
            var step = axis?.step ?? 0f;
            if (step > 0f)
                t = Mathf.Round(t / step) * step;
            return Clamp(t);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (axis?.owner == null || axis.disabled || evt.button != (int)MouseButton.LeftMouse)
                return;
            m_PointerId = evt.pointerId;
            m_Moved = false;
            m_Grab = m_Time - PointerTime(evt.position);
            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (m_PointerId != evt.pointerId)
                return;
            timeline?.TrackPointer(evt.position);
            m_Moved = true;
            var t = At(evt.position);
            m_DragTime = t;
            if (axis != null)
                style.left = axis.TimeToX(t);
            dragMove?.Invoke(t);
            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_PointerId != evt.pointerId)
                return;
            m_PointerId = PointerId.invalidPointerId;
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
            if (m_Moved)
            {
                var t = At(evt.position);
                time = t;
                changed?.Invoke(t);
            }
            evt.StopPropagation();
        }

        void CancelDrag()
        {
            if (m_PointerId == PointerId.invalidPointerId)
                return;
            m_PointerId = PointerId.invalidPointerId;
            OnAxisChanged();
            dragCanceled?.Invoke();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (axis?.owner == null || axis.disabled)
                return;
            var hi = Mathf.Max(m_Min, resolvedMax);
            var step = m_KeyStep > 0f ? m_KeyStep : axis.step > 0f ? axis.step : (hi - m_Min) / 100f;
            float next;
            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                    next = m_Time - step;
                    break;
                case KeyCode.RightArrow:
                    next = m_Time + step;
                    break;
                case KeyCode.Home:
                    next = m_Min;
                    break;
                case KeyCode.End:
                    next = hi;
                    break;
                default:
                    return;
            }

            next = Clamp(next);
            time = next;
            changed?.Invoke(next);
            // Moved from the keyboard, the grip pans the viewport along with it, as the playhead does.
            axis.owner.RevealTime(next);
            evt.StopPropagation();
        }
    }
}
