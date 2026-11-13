using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The shape a <see cref="CanvasEdge"/> takes between its two endpoints.
    /// </summary>
    public enum CanvasCurveStyle
    {
        /// <summary>
        /// A direct line.
        /// </summary>
        Straight = 0,

        /// <summary>
        /// A cubic curve that leaves and arrives horizontally, the way a port dot points.
        /// </summary>
        Bezier,

        /// <summary>
        /// An orthogonal route through a vertical mid-segment, with rounded corners.
        /// </summary>
        SmoothStep,

        /// <summary>
        /// An orthogonal route through a vertical mid-segment, with square corners.
        /// </summary>
        Step,

        /// <summary>
        /// A cord hung between the two ends, drooping under its own weight.
        /// </summary>
        Cable
    }

    /// <summary>
    /// A connection line between two points in a <see cref="Canvas"/>, drawn with
    /// <see cref="Painter2D"/>.
    /// </summary>
    /// <remarks>
    /// Pair it with <see cref="CanvasNode"/>: <see cref="CanvasNode.GetPortAnchor"/> gives the two
    /// points, and the edge positions itself over the bounding box that covers them, so
    /// <see cref="from"/> and <see cref="to"/> are in the same content-space units as a node's
    /// <see cref="CanvasNode.x"/> and <see cref="CanvasNode.y"/>. Add edges before nodes, so nodes
    /// paint on top.
    ///
    /// Picking follows the line, not the box. The web draws a second, transparent, fat path to make a
    /// thin line clickable; here <see cref="ContainsPoint"/> answers with the distance from the pointer
    /// to the sampled path, so there is no second path to keep in sync with the first.
    ///
    /// Every colour the painter uses is read from a custom style property, because a colour written in
    /// C# is a colour the theme cannot reach. <c>--edge-color</c> and its siblings are declared in
    /// <c>CanvasEdge.uss</c> and bound to App UI tokens there.
    /// </remarks>
    /// <example>
    /// <para>An edge between two nodes' ports.</para>
    /// <code lang="csharp">
    /// var edge = new CanvasEdge
    /// {
    ///     from = source.GetPortAnchor(CanvasPortSide.Output, 0),
    ///     to = target.GetPortAnchor(CanvasPortSide.Input, 0),
    ///     curveStyle = CanvasCurveStyle.Bezier,
    ///     isDirectional = true,
    /// };
    /// canvas.Add(edge);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:CanvasEdge curve-style="SmoothStep" is-directional="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class CanvasEdge : BaseVisualElement, ISelectableElement
    {
        internal static readonly BindingId fromProperty = nameof(from);

        internal static readonly BindingId toProperty = nameof(to);

        internal static readonly BindingId isDirectionalProperty = nameof(isDirectional);

        internal static readonly BindingId curveStyleProperty = nameof(curveStyle);

        internal static readonly BindingId strokeWidthProperty = nameof(strokeWidth);

        internal static readonly BindingId activeProperty = nameof(active);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId lodProperty = nameof(lod);

        /// <summary>
        /// The CanvasEdge main styling class.
        /// </summary>
        public const string ussClassName = "appui-canvas-edge";

        /// <summary>
        /// The CanvasEdge curve style styling class.
        /// </summary>
        [EnumName("GetCurveStyleUssClassName", typeof(CanvasCurveStyle))]
        public const string curveStyleUssClassName = ussClassName + "--";

        /// <summary>
        /// The styling class of an active edge.
        /// </summary>
        public const string activeUssClassName = ussClassName + "--active";

        /// <summary>
        /// The styling class of a directional edge.
        /// </summary>
        public const string directionalUssClassName = ussClassName + "--directional";

        /// <summary>
        /// The margin left around the path's bounding box, in pixels, so a thick stroke and the
        /// arrowhead are not clipped by the element's own box.
        /// </summary>
        public const float padding = 16f;

        /// <summary>
        /// The default stroke width, in pixels.
        /// </summary>
        public const float defaultStrokeWidth = 2f;

        /// <summary>
        /// Half the width of the band around the line that answers a pick, in pixels.
        /// </summary>
        /// <remarks>
        /// A hairline is impossible to hit exactly, so the edge accepts a pointer within this distance
        /// of the path. It is the equivalent of the web's transparent fat hit-path.
        /// </remarks>
        public const float hitTolerance = 7f;

        const int k_CurveSegments = 48;

        static readonly CustomStyleProperty<Color> k_ColorProperty = new CustomStyleProperty<Color>("--edge-color");

        static readonly CustomStyleProperty<Color> k_HighlightColorProperty = new CustomStyleProperty<Color>("--edge-highlight-color");

        static readonly CustomStyleProperty<Color> k_CableColorProperty = new CustomStyleProperty<Color>("--edge-cable-color");

        static readonly CustomStyleProperty<Color> k_CableOutlineColorProperty = new CustomStyleProperty<Color>("--edge-cable-outline-color");

        static readonly CustomStyleProperty<Color> k_CableOutlineHighlightColorProperty = new CustomStyleProperty<Color>("--edge-cable-outline-highlight-color");

        readonly List<Vector2> m_Samples = new List<Vector2>();

        Vector2 m_From;

        Vector2 m_To = new Vector2(100f, 0f);

        bool m_IsDirectional;

        CanvasCurveStyle m_CurveStyle;

        float m_StrokeWidth = defaultStrokeWidth;

        bool m_Active;

        bool m_Selected;

        CanvasLevelOfDetail m_Lod;

        Optional<Color> m_Color;

        Color m_ResolvedColor = Color.clear;

        Color m_ResolvedHighlightColor = Color.clear;

        Color m_ResolvedCableColor = Color.clear;

        Color m_ResolvedCableOutlineColor = Color.clear;

        Color m_ResolvedCableOutlineHighlightColor = Color.clear;

        /// <summary>
        /// Raised when the edge is pressed.
        /// </summary>
        public event Action selectRequested;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CanvasEdge()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            generateVisualContent = OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RegisterCallback<PointerDownEvent>(OnPointerDown);

            curveStyle = CanvasCurveStyle.Bezier;
            isDirectional = false;
            active = false;
            selected = false;
            lod = CanvasLevelOfDetail.Full;
            Refresh();
        }

        /// <summary>
        /// The edge's start point, in the canvas' content space.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2 from
        {
            get => m_From;
            set
            {
                var changed = m_From != value;
                m_From = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in fromProperty);
            }
        }

        /// <summary>
        /// The edge's end point, in the canvas' content space.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2 to
        {
            get => m_To;
            set
            {
                var changed = m_To != value;
                m_To = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in toProperty);
            }
        }

        /// <summary>
        /// Whether an arrowhead is drawn at the path's midpoint. False by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isDirectional
        {
            get => m_IsDirectional;
            set
            {
                var changed = m_IsDirectional != value;
                m_IsDirectional = value;
                EnableInClassList(directionalUssClassName, value);
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in isDirectionalProperty);
            }
        }

        /// <summary>
        /// The path's shape. <see cref="CanvasCurveStyle.Bezier"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public CanvasCurveStyle curveStyle
        {
            get => m_CurveStyle;
            set
            {
                var changed = m_CurveStyle != value;
                RemoveFromClassList(GetCurveStyleUssClassName(m_CurveStyle));
                m_CurveStyle = value;
                AddToClassList(GetCurveStyleUssClassName(m_CurveStyle));
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in curveStyleProperty);
            }
        }

        /// <summary>
        /// The stroke width in pixels. <see cref="defaultStrokeWidth"/> by default.
        /// </summary>
        /// <remarks>
        /// A highlighted edge draws one pixel thicker, so it reads as emphasised at any width.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public float strokeWidth
        {
            get => m_StrokeWidth;
            set
            {
                var changed = !Mathf.Approximately(m_StrokeWidth, value);
                m_StrokeWidth = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in strokeWidthProperty);
            }
        }

        /// <summary>
        /// Whether the edge is an active path, for instance the branch a blend tree is currently taking.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool active
        {
            get => m_Active;
            set
            {
                var changed = m_Active != value;
                m_Active = value;
                EnableInClassList(activeUssClassName, value);
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in activeProperty);
            }
        }

        /// <summary>
        /// Whether the edge is selected. Drawn the same way as <see cref="active"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => m_Selected;
            set
            {
                var changed = m_Selected != value;
                SetSelectedWithoutNotify(value);

                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// How much of the edge to draw. <see cref="CanvasLevelOfDetail.Full"/> by default.
        /// </summary>
        /// <remarks>
        /// At <see cref="CanvasLevelOfDetail.Box"/>, a far zoom-out, the edge draws its line only: no
        /// hit area and no arrow, which are too small to use at that scale.
        /// <see cref="CanvasLevelOfDetail.Compact"/> draws like <see cref="CanvasLevelOfDetail.Full"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public CanvasLevelOfDetail lod
        {
            get => m_Lod;
            set
            {
                var changed = m_Lod != value;
                m_Lod = value;
                pickingMode = value == CanvasLevelOfDetail.Box ? PickingMode.Ignore : PickingMode.Position;
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in lodProperty);
            }
        }

        /// <summary>
        /// Whether the arrowhead is drawn: the edge is directional and not at
        /// <see cref="CanvasLevelOfDetail.Box"/>.
        /// </summary>
        internal bool drawsArrowHead => m_IsDirectional && m_Lod != CanvasLevelOfDetail.Box;

        /// <summary>
        /// Sets the selected state without notifying.
        /// </summary>
        /// <param name="newValue"> The new selected state. </param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            m_Selected = newValue;
            EnableInClassList(Styles.selectedUssClassName, newValue);
            MarkDirtyRepaint();
        }

        /// <summary>
        /// An explicit stroke colour, overriding the stylesheet's.
        /// </summary>
        /// <remarks>
        /// Unset by default, which is the normal case: the colour then comes from <c>--edge-color</c>
        /// and follows the theme. Set it only when the colour carries data, such as a connection type.
        /// </remarks>
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// Whether the edge is drawn emphasised, which is true when it is either active or selected.
        /// </summary>
        public bool highlighted => m_Active || m_Selected;

        /// <summary>
        /// The stroke colour resolved from the stylesheet.
        /// </summary>
        public Color resolvedColor => m_ResolvedColor;

        /// <summary>
        /// The emphasised stroke colour resolved from the stylesheet.
        /// </summary>
        public Color resolvedHighlightColor => m_ResolvedHighlightColor;

        /// <summary>
        /// How far a <see cref="CanvasCurveStyle.Cable"/> edge droops at its middle, in pixels. Zero for
        /// every other curve style.
        /// </summary>
        public float sag
        {
            get
            {
                if (m_CurveStyle != CanvasCurveStyle.Cable)
                    return 0f;

                var dx = Mathf.Abs(m_To.x - m_From.x);
                var dy = Mathf.Abs(m_To.y - m_From.y);
                return Mathf.Max(14f, Mathf.Min(dx * 0.45f, 140f) - dy * 0.4f);
            }
        }

        /// <summary>
        /// The element's box in the canvas' content space: the path's bounding box grown by
        /// <see cref="padding"/>.
        /// </summary>
        public Rect bounds
        {
            get
            {
                var minX = Mathf.Min(m_From.x, m_To.x) - padding;
                var minY = Mathf.Min(m_From.y, m_To.y) - padding;
                var w = Mathf.Abs(m_To.x - m_From.x) + padding * 2f;
                var h = Mathf.Abs(m_To.y - m_From.y) + padding * 2f + sag * 0.75f;
                return new Rect(minX, minY, w, h);
            }
        }

        /// <summary>
        /// Where the arrowhead sits, in the canvas' content space.
        /// </summary>
        public Vector2 midpoint => Midpoint(out _);

        /// <summary>
        /// The direction the arrowhead points, in degrees clockwise from the positive X axis.
        /// </summary>
        public float midpointAngle
        {
            get
            {
                Midpoint(out var angle);
                return angle;
            }
        }

        /// <summary>
        /// The path, sampled into a polyline in the canvas' content space.
        /// </summary>
        /// <returns> The sampled points, starting at <see cref="from"/> and ending at <see cref="to"/>. </returns>
        /// <remarks>
        /// The same samples drive both painting and picking, so the line a user sees is exactly the line
        /// they can click.
        /// </remarks>
        public IReadOnlyList<Vector2> SamplePath()
        {
            BuildSamples();
            return m_Samples;
        }

        /// <summary>
        /// Whether a point in the element's local space is close enough to the line to count as a hit.
        /// </summary>
        /// <param name="localPoint"> The point, in the element's local coordinates. </param>
        /// <returns> Whether the point is within <see cref="hitTolerance"/> of the path. </returns>
        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (m_Lod == CanvasLevelOfDetail.Box)
                return false;

            BuildSamples();
            var origin = bounds.min;
            var p = localPoint + origin;
            var tolerance = hitTolerance + m_StrokeWidth * 0.5f;

            for (var i = 1; i < m_Samples.Count; i++)
            {
                if (DistanceToSegment(p, m_Samples[i - 1], m_Samples[i]) <= tolerance)
                    return true;
            }

            return false;
        }

        void Refresh()
        {
            var b = bounds;
            style.position = Position.Absolute;
            style.left = b.xMin;
            style.top = b.yMin;
            style.width = b.width;
            style.height = b.height;
            m_Samples.Clear();
            MarkDirtyRepaint();
        }

        Vector2 Midpoint(out float angle)
        {
            var dx = m_To.x - m_From.x;

            switch (m_CurveStyle)
            {
                case CanvasCurveStyle.Straight:
                {
                    angle = Mathf.Atan2(m_To.y - m_From.y, m_To.x - m_From.x) * Mathf.Rad2Deg;
                    return (m_From + m_To) * 0.5f;
                }
                case CanvasCurveStyle.Bezier:
                {
                    GetBezierControls(out var c1, out var c2);
                    return CubicAt(m_From, c1, c2, m_To, 0.5f, out angle);
                }
                case CanvasCurveStyle.Cable:
                {
                    GetCableControls(out var c1, out var c2);
                    return CubicAt(m_From, c1, c2, m_To, 0.5f, out angle);
                }
                default:
                {
                    // Step and SmoothStep route horizontal, vertical, horizontal, and the midpoint
                    // falls on the vertical run. When the ends are level that run collapses and the
                    // midpoint lies on a horizontal line, so the caret follows dx instead.
                    var dy = m_To.y - m_From.y;
                    if (Mathf.Abs(dy) <= 0.5f)
                        angle = dx < 0f ? 180f : 0f;
                    else
                        angle = dy > 0f ? 90f : -90f;
                    return new Vector2(m_From.x + dx * 0.5f, (m_From.y + m_To.y) * 0.5f);
                }
            }
        }

        void GetBezierControls(out Vector2 c1, out Vector2 c2)
        {
            var dx = m_To.x - m_From.x;
            c1 = new Vector2(m_From.x + dx * 0.5f, m_From.y);
            c2 = new Vector2(m_To.x - dx * 0.5f, m_To.y);
        }

        void GetCableControls(out Vector2 c1, out Vector2 c2)
        {
            var dx = m_To.x - m_From.x;
            var s = sag;
            c1 = new Vector2(m_From.x + dx * 0.25f, m_From.y + s);
            c2 = new Vector2(m_To.x - dx * 0.25f, m_To.y + s);
        }

        static Vector2 CubicAt(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, out float angle)
        {
            var mt = 1f - t;
            var x = mt * mt * mt * p0.x + 3f * mt * mt * t * p1.x + 3f * mt * t * t * p2.x + t * t * t * p3.x;
            var y = mt * mt * mt * p0.y + 3f * mt * mt * t * p1.y + 3f * mt * t * t * p2.y + t * t * t * p3.y;
            var dx = 3f * mt * mt * (p1.x - p0.x) + 6f * mt * t * (p2.x - p1.x) + 3f * t * t * (p3.x - p2.x);
            var dy = 3f * mt * mt * (p1.y - p0.y) + 6f * mt * t * (p2.y - p1.y) + 3f * t * t * (p3.y - p2.y);
            angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            return new Vector2(x, y);
        }

        void BuildSamples()
        {
            if (m_Samples.Count > 0)
                return;

            var dx = m_To.x - m_From.x;

            switch (m_CurveStyle)
            {
                case CanvasCurveStyle.Straight:
                    m_Samples.Add(m_From);
                    m_Samples.Add((m_From + m_To) * 0.5f);
                    m_Samples.Add(m_To);
                    break;

                case CanvasCurveStyle.Bezier:
                {
                    GetBezierControls(out var c1, out var c2);
                    SampleCubic(c1, c2);
                    break;
                }

                case CanvasCurveStyle.Cable:
                {
                    GetCableControls(out var c1, out var c2);
                    SampleCubic(c1, c2);
                    break;
                }

                case CanvasCurveStyle.Step:
                {
                    var mx = m_From.x + dx * 0.5f;
                    m_Samples.Add(m_From);
                    m_Samples.Add(new Vector2(mx, m_From.y));
                    m_Samples.Add(new Vector2(mx, m_To.y));
                    m_Samples.Add(m_To);
                    break;
                }

                default:
                {
                    // SmoothStep: the same orthogonal route, with the two corners rounded by a radius
                    // that shrinks on short runs so the corners never overlap.
                    var mx = m_From.x + dx * 0.5f;
                    var r = Mathf.Min(12f, Mathf.Abs(m_To.y - m_From.y) * 0.5f, Mathf.Abs(dx) * 0.5f);
                    var sy = m_To.y > m_From.y ? 1f : -1f;
                    var sx = dx > 0f ? 1f : -1f;

                    m_Samples.Add(m_From);
                    m_Samples.Add(new Vector2(mx - r * sx, m_From.y));
                    SampleQuadratic(
                        new Vector2(mx - r * sx, m_From.y),
                        new Vector2(mx, m_From.y),
                        new Vector2(mx, m_From.y + r * sy));
                    m_Samples.Add(new Vector2(mx, m_To.y - r * sy));
                    SampleQuadratic(
                        new Vector2(mx, m_To.y - r * sy),
                        new Vector2(mx, m_To.y),
                        new Vector2(mx + r * sx, m_To.y));
                    m_Samples.Add(m_To);
                    break;
                }
            }
        }

        void SampleCubic(Vector2 c1, Vector2 c2)
        {
            for (var i = 0; i <= k_CurveSegments; i++)
                m_Samples.Add(CubicAt(m_From, c1, c2, m_To, i / (float)k_CurveSegments, out _));
        }

        void SampleQuadratic(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            const int segments = 8;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var mt = 1f - t;
                m_Samples.Add(new Vector2(
                    mt * mt * p0.x + 2f * mt * t * p1.x + t * t * p2.x,
                    mt * mt * p0.y + 2f * mt * t * p1.y + t * t * p2.y));
            }
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var lengthSq = ab.sqrMagnitude;
            if (lengthSq < Mathf.Epsilon)
                return Vector2.Distance(p, a);

            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
            return Vector2.Distance(p, a + ab * t);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            evt.StopPropagation();
            selectRequested?.Invoke();
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var custom = evt.customStyle;

            if (custom.TryGetValue(k_ColorProperty, out Color c))
                m_ResolvedColor = c;
            if (custom.TryGetValue(k_HighlightColorProperty, out Color hc))
                m_ResolvedHighlightColor = hc;
            if (custom.TryGetValue(k_CableColorProperty, out Color cc))
                m_ResolvedCableColor = cc;
            if (custom.TryGetValue(k_CableOutlineColorProperty, out Color coc))
                m_ResolvedCableOutlineColor = coc;
            if (custom.TryGetValue(k_CableOutlineHighlightColorProperty, out Color cohc))
                m_ResolvedCableOutlineHighlightColor = cohc;

            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            BuildSamples();
            if (m_Samples.Count < 2)
                return;

            var painter = context.painter2D;
            var origin = bounds.min;
            var stroke = m_Color.IsSet
                ? m_Color.Value
                : highlighted ? m_ResolvedHighlightColor : m_ResolvedColor;

            if (m_CurveStyle == CanvasCurveStyle.Cable)
            {
                // A patch cable is a pale cord over a darker outline, so it reads as a physical object
                // rather than a drawn line. The outline inverts when highlighted, so a coloured cable
                // still stands out.
                var cordWidth = highlighted ? m_StrokeWidth + 3f : m_StrokeWidth + 2f;
                var outline = highlighted ? m_ResolvedCableOutlineHighlightColor : m_ResolvedCableOutlineColor;
                var cord = m_Color.IsSet
                    ? m_Color.Value
                    : highlighted ? m_ResolvedHighlightColor : m_ResolvedCableColor;

                StrokePath(painter, origin, outline, cordWidth + 2.5f);
                StrokePath(painter, origin, cord, cordWidth);
                stroke = cord;
            }
            else
            {
                StrokePath(painter, origin, stroke, highlighted ? m_StrokeWidth + 1f : m_StrokeWidth);
            }

            if (drawsArrowHead)
                DrawArrowHead(painter, origin, stroke);
        }

        void StrokePath(Painter2D painter, Vector2 origin, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.BeginPath();
            painter.MoveTo(m_Samples[0] - origin);
            for (var i = 1; i < m_Samples.Count; i++)
                painter.LineTo(m_Samples[i] - origin);
            painter.Stroke();
        }

        void DrawArrowHead(Painter2D painter, Vector2 origin, Color color)
        {
            var tip = Midpoint(out var angle) - origin;
            var rad = angle * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);

            Vector2 Rotate(float x, float y) => tip + new Vector2(x * cos - y * sin, x * sin + y * cos);

            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(Rotate(-5f, -5f));
            painter.LineTo(Rotate(6f, 0f));
            painter.LineTo(Rotate(-5f, 5f));
            painter.ClosePath();
            painter.Fill();
        }
    }
}
