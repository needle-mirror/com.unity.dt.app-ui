using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A slim vertical audio level meter: a dB ruler, one or two thin level bars, an optional
    /// peak-hold line, and a draggable attenuation marker.
    /// </summary>
    /// <remarks>
    /// <para>
    /// VuMeter mirrors the meter beside a channel strip in Unity's Audio Mixer. It is presentational:
    /// feed it live <see cref="levels"/> and it paints them. It never reads an <c>AudioMixer</c> itself,
    /// so it is equally usable for a microphone preview, a VOIP indicator or a test signal.
    /// </para>
    ///
    /// <para>
    /// The level colour is anchored to <see cref="fader"/> rather than to a fixed dB: safe well below
    /// the marker, warning as the level approaches it, clipping once it passes. Move the fader and the
    /// bands move with it, which is what makes the meter readable at any attenuation.
    /// </para>
    ///
    /// <para>Rendering.</para>
    /// <para>
    /// Each channel bar is stroked and filled into its own mesh with <see cref="Painter2D"/>, the same
    /// path <see cref="CanvasEdge"/> and <see cref="AnimationCurveField"/> take, because the fill is a banded
    /// gradient that USS cannot express. A shader would be overkill for a few filled rectangles; those
    /// are reserved for full-surface fills like <see cref="ColorWheel"/>'s. The ruler, the marker chip
    /// and the readout are ordinary elements, styled from USS.
    /// </para>
    ///
    /// <para>Colours.</para>
    /// <para>
    /// Every colour the painter uses is read in C#, so none may be a literal. They come from custom
    /// style properties that <c>VuMeter.uss</c> binds to App UI tokens, and the C# fields start at
    /// <see cref="Color.clear"/> so an unresolved stylesheet is visible rather than plausible.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var meter = new VuMeter { levels = new List&lt;float&gt; { -12f, -18f }, fader = -6f };
    /// meter.faderChanged += db =&gt; mixer.SetFloat("Volume", db);
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:VuMeter min="-80" max="20" show-readout="true" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class VuMeter : VisualElement
    {
        internal static readonly BindingId levelsProperty = nameof(levels);

        internal static readonly BindingId minProperty = nameof(min);

        internal static readonly BindingId maxProperty = nameof(max);

        internal static readonly BindingId ticksProperty = nameof(ticks);

        internal static readonly BindingId faderProperty = nameof(fader);

        internal static readonly BindingId peakProperty = nameof(peak);

        internal static readonly BindingId showReadoutProperty = nameof(showReadout);

        /// <summary>
        /// The VuMeter main styling class.
        /// </summary>
        public const string ussClassName = "appui-vu-meter";

        /// <summary>
        /// The styling class applied when the numeric readout is hidden.
        /// </summary>
        public const string noReadoutUssClassName = ussClassName + "--no-readout";

        /// <summary> The row holding the meter group and the ruler. </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary> The meter group: bars, peak line and the attenuation marker. </summary>
        public const string meterUssClassName = ussClassName + "__meter";

        /// <summary> The container the per-channel bars live in. </summary>
        public const string barsUssClassName = ussClassName + "__bars";

        /// <summary> One channel bar. </summary>
        public const string barUssClassName = ussClassName + "__bar";

        /// <summary> The invisible lane that accepts the fader drag. </summary>
        public const string dragLaneUssClassName = ussClassName + "__drag-lane";

        /// <summary> The attenuation marker. </summary>
        public const string markerUssClassName = ussClassName + "__marker";

        /// <summary> The marker's arrow head. </summary>
        public const string markerArrowUssClassName = ussClassName + "__marker-arrow";

        /// <summary> The marker's numeric chip. </summary>
        public const string markerLabelUssClassName = ussClassName + "__marker-label";

        /// <summary> The dB ruler. </summary>
        public const string rulerUssClassName = ussClassName + "__ruler";

        /// <summary> One ruler row: a dash plus its label. </summary>
        public const string tickUssClassName = ussClassName + "__tick";

        /// <summary> A ruler row's dash. </summary>
        public const string tickDashUssClassName = ussClassName + "__tick-dash";

        /// <summary> A ruler row's label. </summary>
        public const string tickLabelUssClassName = ussClassName + "__tick-label";

        /// <summary> The numeric level readout under the meter. </summary>
        public const string readoutUssClassName = ussClassName + "__readout";

        /// <summary>
        /// How far below the fader the warning band starts, in dB.
        /// </summary>
        public const float warningRange = 12f;

        /// <summary>
        /// The default bottom of the scale, in dB.
        /// </summary>
        public const float defaultMin = -80f;

        /// <summary>
        /// The default top of the scale, in dB.
        /// </summary>
        public const float defaultMax = 20f;

        static readonly CustomStyleProperty<Color> k_SafeColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-safe-color");

        static readonly CustomStyleProperty<Color> k_WarningColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-warning-color");

        static readonly CustomStyleProperty<Color> k_ClipColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-clip-color");

        static readonly CustomStyleProperty<Color> k_TrackColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-track-color");

        static readonly CustomStyleProperty<Color> k_PeakColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-peak-color");

        static readonly CustomStyleProperty<Color> k_FaderColorProperty =
            new CustomStyleProperty<Color>("--vu-meter-fader-color");

        static readonly List<float> k_DefaultTicks = new List<float> { 20f, 0f, -20f, -40f, -60f, -80f };

        readonly List<float> m_Levels = new List<float>();

        readonly List<float> m_Ticks = new List<float>(k_DefaultTicks);

        readonly VisualElement m_Body;

        readonly VisualElement m_Meter;

        readonly VisualElement m_Bars;

        readonly VisualElement m_DragLane;

        readonly VisualElement m_Marker;

        readonly VisualElement m_MarkerArrow;

        readonly LocalizedTextElement m_MarkerLabel;

        readonly VisualElement m_Ruler;

        readonly LocalizedTextElement m_Readout;

        float m_Min = defaultMin;

        float m_Max = defaultMax;

        float m_Fader;

        Optional<float> m_Peak;

        bool m_ShowReadout = true;

        Color m_ResolvedSafeColor = Color.clear;

        Color m_ResolvedWarningColor = Color.clear;

        Color m_ResolvedClipColor = Color.clear;

        Color m_ResolvedTrackColor = Color.clear;

        Color m_ResolvedPeakColor = Color.clear;

        Color m_ResolvedFaderColor = Color.clear;

        /// <summary>
        /// Raised whenever the fader lands on a new value, whether set in code or dragged.
        /// </summary>
        public event Action<float> faderChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public VuMeter()
        {
            AddToClassList(ussClassName);

            m_Body = new VisualElement { name = "body", pickingMode = PickingMode.Ignore };
            m_Body.AddToClassList(bodyUssClassName);
            hierarchy.Add(m_Body);

            m_Meter = new VisualElement { name = "meter" };
            m_Meter.AddToClassList(meterUssClassName);
            m_Body.Add(m_Meter);

            m_Bars = new VisualElement { name = "bars", pickingMode = PickingMode.Ignore };
            m_Bars.AddToClassList(barsUssClassName);
            m_Meter.Add(m_Bars);

            m_DragLane = new VisualElement { name = "drag-lane" };
            m_DragLane.AddToClassList(dragLaneUssClassName);
            m_Meter.Add(m_DragLane);

            m_Marker = new VisualElement { name = "marker" };
            m_Marker.AddToClassList(markerUssClassName);
            m_Meter.Add(m_Marker);

            m_MarkerArrow = new VisualElement { name = "marker-arrow", pickingMode = PickingMode.Ignore };
            m_MarkerArrow.AddToClassList(markerArrowUssClassName);
            m_MarkerArrow.generateVisualContent += OnGenerateMarkerArrow;
            m_Marker.Add(m_MarkerArrow);

            m_MarkerLabel = new LocalizedTextElement { name = "marker-label", pickingMode = PickingMode.Ignore };
            m_MarkerLabel.AddToClassList(markerLabelUssClassName);
            m_Marker.Add(m_MarkerLabel);

            m_Ruler = new VisualElement { name = "ruler", pickingMode = PickingMode.Ignore };
            m_Ruler.AddToClassList(rulerUssClassName);
            m_Body.Add(m_Ruler);

            m_Readout = new LocalizedTextElement { name = "readout", pickingMode = PickingMode.Ignore };
            m_Readout.AddToClassList(readoutUssClassName);
            hierarchy.Add(m_Readout);

            // The marker and the lane beside it are both drag handles, exactly as on the web.
            var drag = new Draggable(null, OnDrag, null, OnDrag);
            drag.dragDirection = Draggable.DragDirection.Vertical;
            m_DragLane.AddManipulator(drag);
            var markerDrag = new Draggable(null, OnMarkerDrag, null, OnMarkerDrag);
            markerDrag.dragDirection = Draggable.DragDirection.Vertical;
            m_Marker.AddManipulator(markerDrag);

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            RefreshBars();
            RefreshTicks();
            RefreshFader();
            RefreshReadout();
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The per-channel levels, in dB. One bar each; an empty list still paints a single silent bar.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public List<float> levels
        {
            get => m_Levels;
            set
            {
                m_Levels.Clear();
                if (value != null)
                    m_Levels.AddRange(value);

                RefreshBars();
                RefreshReadout();
                NotifyPropertyChanged(in levelsProperty);
            }
        }

        /// <summary>
        /// The bottom of the scale, in dB.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float min
        {
            get => m_Min;
            set
            {
                var changed = !Mathf.Approximately(m_Min, value);
                m_Min = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in minProperty);
            }
        }

        /// <summary>
        /// The top of the scale, in dB.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float max
        {
            get => m_Max;
            set
            {
                var changed = !Mathf.Approximately(m_Max, value);
                m_Max = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in maxProperty);
            }
        }

        /// <summary>
        /// The dB values the ruler marks. Rebuilt in full whenever it is assigned.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public List<float> ticks
        {
            get => m_Ticks;
            set
            {
                m_Ticks.Clear();
                m_Ticks.AddRange(value ?? k_DefaultTicks);

                RefreshTicks();
                NotifyPropertyChanged(in ticksProperty);
            }
        }

        /// <summary>
        /// The attenuation fader, in dB. Always inside <see cref="min"/>..<see cref="max"/>, and the
        /// anchor the level colour bands are measured from.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float fader
        {
            get => m_Fader;
            set
            {
                var clamped = Mathf.Clamp(value, m_Min, m_Max);
                var changed = !Mathf.Approximately(m_Fader, clamped);
                m_Fader = clamped;
                RefreshFader();

                if (!changed)
                    return;

                NotifyPropertyChanged(in faderProperty);
                faderChanged?.Invoke(clamped);
            }
        }

        /// <summary>
        /// The peak-hold marker, in dB. Unset hides it.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> peak
        {
            get => m_Peak;
            set
            {
                var changed = !m_Peak.Equals(value);
                m_Peak = value;
                RepaintBars();

                if (changed)
                    NotifyPropertyChanged(in peakProperty);
            }
        }

        /// <summary>
        /// Whether the numeric level readout shows under the meter.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showReadout
        {
            get => m_ShowReadout;
            set
            {
                var changed = m_ShowReadout != value;
                m_ShowReadout = value;
                EnableInClassList(noReadoutUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in showReadoutProperty);
            }
        }

        /// <summary> The container the channel bars live in. </summary>
        public VisualElement barsElement => m_Bars;

        /// <summary> The dB ruler. </summary>
        public VisualElement rulerElement => m_Ruler;

        /// <summary> The attenuation marker. </summary>
        public VisualElement markerElement => m_Marker;

        /// <summary> The numeric level readout. </summary>
        public LocalizedTextElement readoutElement => m_Readout;

        /// <summary> The number of channel bars currently painted. </summary>
        public int barCount => m_Bars.childCount;

        /// <summary> The safe-level colour resolved from <c>--vu-meter-safe-color</c>. </summary>
        public Color resolvedSafeColor => m_ResolvedSafeColor;

        /// <summary> The approaching-clip colour resolved from <c>--vu-meter-warning-color</c>. </summary>
        public Color resolvedWarningColor => m_ResolvedWarningColor;

        /// <summary> The clipping colour resolved from <c>--vu-meter-clip-color</c>. </summary>
        public Color resolvedClipColor => m_ResolvedClipColor;

        /// <summary> The empty-track colour resolved from <c>--vu-meter-track-color</c>. </summary>
        public Color resolvedTrackColor => m_ResolvedTrackColor;

        /// <summary> The peak-hold colour resolved from <c>--vu-meter-peak-color</c>. </summary>
        public Color resolvedPeakColor => m_ResolvedPeakColor;

        /// <summary> The fader marker colour resolved from <c>--vu-meter-fader-color</c>. </summary>
        public Color resolvedFaderColor => m_ResolvedFaderColor;

        /// <summary>
        /// Where a dB value sits on the scale, as a <c>[0, 1]</c> fraction measured from
        /// <see cref="min"/>. Values outside the scale clamp to its ends.
        /// </summary>
        internal float Ratio(float db)
        {
            var span = m_Max - m_Min;
            if (Mathf.Abs(span) < Mathf.Epsilon)
                return 0f;

            return Mathf.Clamp01((db - m_Min) / span);
        }

        /// <summary>
        /// The dB value at a <c>[0, 1]</c> fraction of the scale. The inverse of <see cref="Ratio"/>.
        /// </summary>
        internal float DbAt(float ratio)
        {
            return m_Min + Mathf.Clamp01(ratio) * (m_Max - m_Min);
        }

        /// <summary>
        /// The band colour a level reads in, anchored to <see cref="fader"/>: clipping at or above the
        /// fader, warning within <see cref="warningRange"/> dB below it, safe under that.
        /// </summary>
        internal Color LevelColorAt(float db)
        {
            if (db >= m_Fader)
                return m_ResolvedClipColor;
            if (db >= m_Fader - warningRange)
                return m_ResolvedWarningColor;

            return m_ResolvedSafeColor;
        }

        void Refresh()
        {
            fader = m_Fader;
            RefreshTicks();
            RefreshFader();
            RepaintBars();
            RefreshReadout();
        }

        void RefreshBars()
        {
            var count = Mathf.Max(m_Levels.Count, 1);
            while (m_Bars.childCount > count)
                m_Bars.RemoveAt(m_Bars.childCount - 1);

            while (m_Bars.childCount < count)
            {
                var index = m_Bars.childCount;
                var bar = new VisualElement { name = "bar-" + index, pickingMode = PickingMode.Ignore };
                bar.AddToClassList(barUssClassName);
                bar.generateVisualContent += ctx => OnGenerateBar(bar, ctx);
                m_Bars.Add(bar);
            }

            RepaintBars();
        }

        void RepaintBars()
        {
            for (var i = 0; i < m_Bars.childCount; i++)
                m_Bars[i].MarkDirtyRepaint();
        }

        void RefreshTicks()
        {
            m_Ruler.Clear();

            foreach (var db in m_Ticks)
            {
                var row = new VisualElement { name = "tick-" + db, pickingMode = PickingMode.Ignore };
                row.AddToClassList(tickUssClassName);
                row.style.bottom = Length.Percent(Ratio(db) * 100f);

                var dash = new VisualElement { pickingMode = PickingMode.Ignore };
                dash.AddToClassList(tickDashUssClassName);
                row.Add(dash);

                var label = new LocalizedTextElement(Mathf.RoundToInt(db).ToString())
                {
                    pickingMode = PickingMode.Ignore
                };
                label.AddToClassList(tickLabelUssClassName);
                row.Add(label);

                m_Ruler.Add(row);
            }
        }

        void RefreshFader()
        {
            m_Marker.style.bottom = Length.Percent(Ratio(m_Fader) * 100f);
            m_MarkerLabel.text = Mathf.RoundToInt(m_Fader).ToString();
            RepaintBars();
        }

        void RefreshReadout()
        {
            var loudest = m_Min;
            for (var i = 0; i < m_Levels.Count; i++)
                loudest = Mathf.Max(loudest, m_Levels[i]);

            m_Readout.text = $"{Mathf.Max(loudest, m_Min):0.0} dB";
        }

        void OnDrag(Draggable dragger)
        {
            SetFaderFromLocalY(m_DragLane.localBound.y + dragger.localPosition.y);
        }

        void OnMarkerDrag(Draggable dragger)
        {
            SetFaderFromLocalY(m_Marker.localBound.y + dragger.localPosition.y);
        }

        void SetFaderFromLocalY(float y)
        {
            var height = m_Meter.localBound.height;
            if (height <= 0f)
                return;

            fader = DbAt(1f - y / height);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var custom = evt.customStyle;

            if (custom.TryGetValue(k_SafeColorProperty, out Color safe))
                m_ResolvedSafeColor = safe;
            if (custom.TryGetValue(k_WarningColorProperty, out Color warn))
                m_ResolvedWarningColor = warn;
            if (custom.TryGetValue(k_ClipColorProperty, out Color clip))
                m_ResolvedClipColor = clip;
            if (custom.TryGetValue(k_TrackColorProperty, out Color track))
                m_ResolvedTrackColor = track;
            if (custom.TryGetValue(k_PeakColorProperty, out Color pk))
                m_ResolvedPeakColor = pk;
            if (custom.TryGetValue(k_FaderColorProperty, out Color fc))
                m_ResolvedFaderColor = fc;

            RepaintBars();
            m_MarkerArrow.MarkDirtyRepaint();
        }

        void OnGenerateBar(VisualElement bar, MeshGenerationContext context)
        {
            var rect = bar.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            var painter = context.painter2D;

            // The unlit track, so a silent channel still reads as a channel.
            FillRect(painter, 0f, rect.height, rect.width, m_ResolvedTrackColor);

            var index = m_Bars.IndexOf(bar);
            var level = index >= 0 && index < m_Levels.Count ? m_Levels[index] : m_Min;
            var levelRatio = Ratio(level);
            if (levelRatio > 0f)
            {
                // The lit part is banded, not a single colour: safe below the warning threshold,
                // warning up to the fader, clipping above it. USS has no gradient, hence Painter2D.
                var warnRatio = Mathf.Min(Ratio(m_Fader - warningRange), levelRatio);
                var faderRatio = Mathf.Min(Ratio(m_Fader), levelRatio);

                FillBand(painter, rect, 0f, warnRatio, m_ResolvedSafeColor);
                FillBand(painter, rect, warnRatio, faderRatio, m_ResolvedWarningColor);
                FillBand(painter, rect, faderRatio, levelRatio, m_ResolvedClipColor);
            }

            if (!m_Peak.IsSet)
                return;

            var y = rect.height * (1f - Ratio(m_Peak.Value));
            painter.strokeColor = m_ResolvedPeakColor;
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, y));
            painter.LineTo(new Vector2(rect.width, y));
            painter.Stroke();
        }

        void OnGenerateMarkerArrow(MeshGenerationContext context)
        {
            var rect = m_MarkerArrow.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            // The marker sits to the right of the bars (arrow, then label), so the tip points left,
            // at the bars, with its base flush against the label.
            var painter = context.painter2D;
            painter.fillColor = m_ResolvedFaderColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.width, 0f));
            painter.LineTo(new Vector2(0f, rect.height * 0.5f));
            painter.LineTo(new Vector2(rect.width, rect.height));
            painter.ClosePath();
            painter.Fill();
        }

        static void FillBand(Painter2D painter, Rect rect, float fromRatio, float toRatio, Color color)
        {
            if (toRatio <= fromRatio)
                return;

            FillRect(painter, rect.height * (1f - toRatio), rect.height * (1f - fromRatio), rect.width, color);
        }

        static void FillRect(Painter2D painter, float top, float bottom, float width, Color color)
        {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, top));
            painter.LineTo(new Vector2(width, top));
            painter.LineTo(new Vector2(width, bottom));
            painter.LineTo(new Vector2(0f, bottom));
            painter.ClosePath();
            painter.Fill();
        }
    }
}
