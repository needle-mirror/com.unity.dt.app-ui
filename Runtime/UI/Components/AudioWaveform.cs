using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A presentational waveform envelope with an optional playhead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AudioWaveform is a pure display element: the caller owns the peak data, the playback state and
    /// any polling or animation. It draws one mirrored vertical bar per entry in <see cref="peaks"/>
    /// around a faint centre baseline, and — when <see cref="progress"/> is set — a playhead line at
    /// that fraction of the width.
    /// </para>
    ///
    /// <para>
    /// Use it to preview an audio clip in an inspector, a browser row or a timeline. It knows nothing
    /// about <c>AudioClip</c>, so it works just as well for a precomputed envelope shipped alongside
    /// an asset as for peaks sampled live.
    /// </para>
    ///
    /// <para>Rendering.</para>
    /// <para>
    /// The waveform is stroked straight into the element's mesh with <see cref="Painter2D"/>, the same
    /// path <see cref="CanvasEdge"/> and <see cref="AnimationCurveField"/> take. It needs neither a shader nor a
    /// render texture: those are only warranted for a full-surface fill such as
    /// <see cref="ColorWheel"/>'s. A stroked envelope is exactly what Painter2D is for.
    /// </para>
    ///
    /// <para>Colours.</para>
    /// <para>
    /// Everything the painter draws is read in C#, so no colour may be a literal. All three come from
    /// custom style properties that <c>AudioWaveform.uss</c> binds to App UI tokens —
    /// <c>--waveform-color</c>, <c>--waveform-baseline-color</c> and <c>--waveform-playhead-color</c> —
    /// the same pattern as <see cref="Progress"/>'s <c>--progress-color</c>. The C# fields start at
    /// <see cref="Color.clear"/> so a stylesheet that fails to resolve is visible rather than plausible.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var waveform = new AudioWaveform
    /// {
    ///     peaks = new List&lt;float&gt; { 0.1f, 0.4f, 0.9f, 0.6f, 0.2f },
    ///     progress = new Optional&lt;float&gt;(0.35f),
    /// };
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:AudioWaveform progress="0.35" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class AudioWaveform : VisualElement
    {
        internal static readonly BindingId peaksProperty = nameof(peaks);

        internal static readonly BindingId progressProperty = nameof(progress);

        /// <summary>
        /// The AudioWaveform main styling class.
        /// </summary>
        public const string ussClassName = "appui-audio-waveform";

        /// <summary>
        /// The width of a waveform bar and of the playhead, in points.
        /// </summary>
        /// <remarks>
        /// A hairline, matching the registry's non-scaling 1px stroke. It is a stroke width in code
        /// rather than a layout dimension, so it is not a token-backed length.
        /// </remarks>
        public const float strokeWidth = 1f;

        static readonly CustomStyleProperty<Color> k_WaveformColorProperty =
            new CustomStyleProperty<Color>("--waveform-color");

        static readonly CustomStyleProperty<Color> k_BaselineColorProperty =
            new CustomStyleProperty<Color>("--waveform-baseline-color");

        static readonly CustomStyleProperty<Color> k_PlayheadColorProperty =
            new CustomStyleProperty<Color>("--waveform-playhead-color");

        readonly List<float> m_Peaks = new List<float>();

        readonly List<Vector3> m_Bars = new List<Vector3>();

        Optional<float> m_Progress;

        Color m_ResolvedWaveformColor = Color.clear;

        Color m_ResolvedBaselineColor = Color.clear;

        Color m_ResolvedPlayheadColor = Color.clear;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AudioWaveform()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            generateVisualContent = OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>
        /// The normalized peak amplitudes, in the <c>[0, 1]</c> range, one mirrored vertical bar each.
        /// </summary>
        /// <remarks>
        /// Never null. Assigning null empties the list rather than replacing it, so a caller can hold
        /// on to the collection. Values outside <c>[0, 1]</c> are clamped when drawn, and a peak of
        /// zero or less draws no bar at all, so silence reads as the bare baseline.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public List<float> peaks
        {
            get => m_Peaks;
            set
            {
                m_Peaks.Clear();
                if (value != null)
                    m_Peaks.AddRange(value);

                MarkDirtyRepaint();
                NotifyPropertyChanged(in peaksProperty);
            }
        }

        /// <summary>
        /// The playhead position, as a <c>[0, 1]</c> fraction of the clip. Unset hides the playhead.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> progress
        {
            get => m_Progress;
            set
            {
                var changed = !m_Progress.Equals(value);
                m_Progress = value;
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in progressProperty);
            }
        }

        /// <summary>
        /// The bar colour resolved from <c>--waveform-color</c>.
        /// </summary>
        public Color resolvedWaveformColor => m_ResolvedWaveformColor;

        /// <summary>
        /// The centre baseline colour resolved from <c>--waveform-baseline-color</c>.
        /// </summary>
        public Color resolvedBaselineColor => m_ResolvedBaselineColor;

        /// <summary>
        /// The playhead colour resolved from <c>--waveform-playhead-color</c>.
        /// </summary>
        public Color resolvedPlayheadColor => m_ResolvedPlayheadColor;

        /// <summary>
        /// The bars built by the last <see cref="BuildBars"/> pass: <c>x</c> is the bar's centre,
        /// <c>y</c> its top and <c>z</c> its bottom.
        /// </summary>
        internal IReadOnlyList<Vector3> bars => m_Bars;

        /// <summary>
        /// Lays the mirrored bars out inside a rect. Peaks are clamped to <c>[0, 1]</c> and silent
        /// entries are skipped.
        /// </summary>
        internal void BuildBars(Rect rect)
        {
            m_Bars.Clear();

            var count = Mathf.Max(m_Peaks.Count, 1);
            var step = rect.width / count;
            var mid = rect.height * 0.5f;

            for (var i = 0; i < m_Peaks.Count; i++)
            {
                var p = Mathf.Clamp01(m_Peaks[i]);
                if (p <= 0f)
                    continue;

                var half = p * mid;
                m_Bars.Add(new Vector3((i + 0.5f) * step, mid - half, mid + half));
            }
        }

        /// <summary>
        /// The playhead's horizontal position for a given width. Progress is clamped to <c>[0, 1]</c>,
        /// and an unset progress reports zero — the playhead is simply not drawn.
        /// </summary>
        internal float PlayheadX(float width)
        {
            return m_Progress.IsSet ? Mathf.Clamp01(m_Progress.Value) * width : 0f;
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var custom = evt.customStyle;

            if (custom.TryGetValue(k_WaveformColorProperty, out Color c))
                m_ResolvedWaveformColor = c;
            if (custom.TryGetValue(k_BaselineColorProperty, out Color bc))
                m_ResolvedBaselineColor = bc;
            if (custom.TryGetValue(k_PlayheadColorProperty, out Color pc))
                m_ResolvedPlayheadColor = pc;

            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            var painter = context.painter2D;
            painter.lineWidth = strokeWidth;
            painter.lineCap = LineCap.Butt;

            // The centre baseline, so silence still reads as a track rather than an empty box.
            var mid = rect.height * 0.5f;
            painter.strokeColor = m_ResolvedBaselineColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, mid));
            painter.LineTo(new Vector2(rect.width, mid));
            painter.Stroke();

            BuildBars(rect);
            if (m_Bars.Count > 0)
            {
                painter.strokeColor = m_ResolvedWaveformColor;
                painter.BeginPath();
                foreach (var bar in m_Bars)
                {
                    painter.MoveTo(new Vector2(bar.x, bar.y));
                    painter.LineTo(new Vector2(bar.x, bar.z));
                }
                painter.Stroke();
            }

            if (!m_Progress.IsSet)
                return;

            var x = PlayheadX(rect.width);
            painter.strokeColor = m_ResolvedPlayheadColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, 0f));
            painter.LineTo(new Vector2(x, rect.height));
            painter.Stroke();
        }
    }
}
