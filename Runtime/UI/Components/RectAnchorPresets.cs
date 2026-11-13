using System;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// What a <see cref="RectAnchorPresets"/> reports when a preset is chosen.
    /// </summary>
    /// <remarks>
    /// The anchors are normalised fractions of the parent, y-up: <see cref="anchorMin"/> is the
    /// bottom-left corner and <see cref="anchorMax"/> the top-right, exactly as
    /// <see cref="RectTransform.anchorMin"/> and <see cref="RectTransform.anchorMax"/> define them.
    /// </remarks>
    public readonly struct RectAnchorPickEvent
    {
        /// <summary>
        /// The preset's bottom-left anchor, in 0..1 of the parent.
        /// </summary>
        public Vector2 anchorMin { get; }

        /// <summary>
        /// The preset's top-right anchor, in 0..1 of the parent.
        /// </summary>
        public Vector2 anchorMax { get; }

        /// <summary>
        /// Whether the pivot should move with the anchors, which is what Shift asks for.
        /// </summary>
        public bool setPivot { get; }

        /// <summary>
        /// Whether the position should move with the anchors, which is what Alt asks for.
        /// </summary>
        public bool setPosition { get; }

        /// <summary>
        /// Constructs a pick event.
        /// </summary>
        /// <param name="anchorMin"> The preset's bottom-left anchor. </param>
        /// <param name="anchorMax"> The preset's top-right anchor. </param>
        /// <param name="setPivot"> Whether the pivot should move with the anchors. </param>
        /// <param name="setPosition"> Whether the position should move with the anchors. </param>
        public RectAnchorPickEvent(Vector2 anchorMin, Vector2 anchorMax, bool setPivot, bool setPosition)
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.setPivot = setPivot;
            this.setPosition = setPosition;
        }
    }

    /// <summary>
    /// The glyph drawn inside one <see cref="RectAnchorPresets"/> cell.
    /// </summary>
    /// <remarks>
    /// A faint parent frame, a muted indicator showing what the element becomes under this preset, and
    /// four arrowheads whose tips sit exactly on the preset's anchor corners — converging into a
    /// pinwheel for a point anchor, splayed to the edges for a stretch.
    ///
    /// Drawn with <see cref="Painter2D"/> rather than assembled from child elements, because the
    /// arrowheads are triangles at 45° and nothing in USS draws one. Both colours are read from custom
    /// style properties, so the glyph follows the theme.
    /// </remarks>
    public class RectAnchorPresetGlyph : VisualElement
    {
        /// <summary>
        /// The glyph's main styling class.
        /// </summary>
        public const string ussClassName = "appui-rect-anchor-preset-glyph";

        /// <summary>
        /// The coordinate space the glyph is authored in, before it is scaled to the element's box.
        /// </summary>
        public const float viewBox = 20f;

        static readonly CustomStyleProperty<Color> k_GlyphColorProperty = new CustomStyleProperty<Color>("--rect-glyph-color");

        static readonly CustomStyleProperty<Color> k_AnchorColorProperty = new CustomStyleProperty<Color>("--rect-anchor");

        Color m_GlyphColor = Color.clear;

        Color m_AnchorColor = Color.clear;

        Vector2 m_AnchorMin;

        Vector2 m_AnchorMax;

        /// <summary>
        /// Constructs a glyph.
        /// </summary>
        public RectAnchorPresetGlyph()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            generateVisualContent = OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>
        /// The preset's bottom-left anchor, in 0..1 of the parent, y-up.
        /// </summary>
        public Vector2 anchorMin
        {
            get => m_AnchorMin;
            set
            {
                m_AnchorMin = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The preset's top-right anchor, in 0..1 of the parent, y-up.
        /// </summary>
        public Vector2 anchorMax
        {
            get => m_AnchorMax;
            set
            {
                m_AnchorMax = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The frame and indicator colour, resolved from <c>--rect-glyph-color</c>.
        /// </summary>
        public Color resolvedGlyphColor => m_GlyphColor;

        /// <summary>
        /// The arrowhead colour, resolved from <c>--rect-anchor</c>.
        /// </summary>
        public Color resolvedAnchorColor => m_AnchorColor;

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(k_GlyphColorProperty, out Color glyph))
                m_GlyphColor = glyph;
            if (evt.customStyle.TryGetValue(k_AnchorColorProperty, out Color anchor))
                m_AnchorColor = anchor;

            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var size = Mathf.Min(contentRect.width, contentRect.height);
            if (size <= 0f)
                return;

            var scale = size / viewBox;
            var painter = context.painter2D;

            const float pad = 3f;
            var span = viewBox - 2f * pad;
            float AX(float a) => (pad + a * span) * scale;
            // A y-up anchor drawn into a y-down box.
            float AY(float a) => (pad + (1f - a) * span) * scale;

            var stretchX = !Mathf.Approximately(m_AnchorMin.x, m_AnchorMax.x);
            var stretchY = !Mathf.Approximately(m_AnchorMin.y, m_AnchorMax.y);
            var px = (m_AnchorMin.x + m_AnchorMax.x) * 0.5f;
            var py = 1f - (m_AnchorMin.y + m_AnchorMax.y) * 0.5f;

            // The parent frame.
            var frame = m_GlyphColor;
            frame.a *= 0.35f;
            painter.strokeColor = frame;
            painter.lineWidth = scale;
            painter.BeginPath();
            StrokeRect(painter, scale, 1f);
            painter.Stroke();

            // The element indicator: what the element becomes under this preset.
            var indicator = m_GlyphColor;
            indicator.a *= 0.3f;
            painter.fillColor = indicator;
            painter.BeginPath();
            if (stretchX && stretchY)
                FillRect(painter, pad * scale, pad * scale, span * scale, span * scale);
            else if (stretchX)
                FillRect(painter, pad * scale, (py * viewBox - 1.25f) * scale, span * scale, 2.5f * scale);
            else if (stretchY)
                FillRect(painter, (px * viewBox - 1.25f) * scale, pad * scale, 2.5f * scale, span * scale);
            else
                FillRect(painter, (px * viewBox - 2f) * scale, (py * viewBox - 2f) * scale, 4f * scale, 4f * scale);
            painter.Fill();

            // The four arrowheads, tips on the anchor corners.
            painter.fillColor = m_AnchorColor;
            DrawArrow(painter, AX(m_AnchorMin.x), AY(m_AnchorMax.y), -1f, -1f, scale);
            DrawArrow(painter, AX(m_AnchorMax.x), AY(m_AnchorMax.y), 1f, -1f, scale);
            DrawArrow(painter, AX(m_AnchorMax.x), AY(m_AnchorMin.y), 1f, 1f, scale);
            DrawArrow(painter, AX(m_AnchorMin.x), AY(m_AnchorMin.y), -1f, 1f, scale);
        }

        static void StrokeRect(Painter2D painter, float scale, float inset)
        {
            var min = inset * scale;
            var max = (viewBox - inset) * scale;
            painter.MoveTo(new Vector2(min, min));
            painter.LineTo(new Vector2(max, min));
            painter.LineTo(new Vector2(max, max));
            painter.LineTo(new Vector2(min, max));
            painter.ClosePath();
        }

        static void FillRect(Painter2D painter, float x, float y, float w, float h)
        {
            painter.MoveTo(new Vector2(x, y));
            painter.LineTo(new Vector2(x + w, y));
            painter.LineTo(new Vector2(x + w, y + h));
            painter.LineTo(new Vector2(x, y + h));
            painter.ClosePath();
        }

        static void DrawArrow(Painter2D painter, float x, float y, float sx, float sy, float scale)
        {
            var dx = sx * Mathf.Sqrt(0.5f);
            var dy = sy * Mathf.Sqrt(0.5f);
            var pxu = -dy;
            var pyu = dx;
            // The base fans outward from the tip, which sits on the anchor corner.
            var bx = x + dx * 3f * scale;
            var by = y + dy * 3f * scale;

            painter.BeginPath();
            painter.MoveTo(new Vector2(x, y));
            painter.LineTo(new Vector2(bx + pxu * 1.7f * scale, by + pyu * 1.7f * scale));
            painter.LineTo(new Vector2(bx - pxu * 1.7f * scale, by - pyu * 1.7f * scale));
            painter.ClosePath();
            painter.Fill();
        }
    }

    /// <summary>
    /// The anchor preset picker from Unity's RectTransform inspector: a 4×4 grid of anchor
    /// configurations.
    /// </summary>
    /// <remarks>
    /// Columns run left / centre / right / stretch-horizontally, rows top / middle / bottom /
    /// stretch-vertically. Presentation only: choosing a preset raises <see cref="picked"/> with the
    /// anchors and the held modifiers, and the caller applies them to its own model. That split is
    /// deliberate — recomputing a RectTransform from new anchors is backend math, not UI.
    ///
    /// Hold Shift while picking to also move the pivot, Alt to also move the position. Those are
    /// reported, not applied.
    ///
    /// Anchors are normalised fractions of the parent, y-up, matching
    /// <see cref="RectTransform.anchorMin"/> and <see cref="RectTransform.anchorMax"/>.
    /// </remarks>
    /// <example>
    /// <para>Wiring the picker to a RectTransform.</para>
    /// <code lang="csharp">
    /// var presets = new RectAnchorPresets
    /// {
    ///     anchorMin = target.anchorMin,
    ///     anchorMax = target.anchorMax,
    /// };
    /// presets.picked += evt =>
    /// {
    ///     target.anchorMin = evt.anchorMin;
    ///     target.anchorMax = evt.anchorMax;
    ///     if (evt.setPivot)
    ///         target.pivot = (evt.anchorMin + evt.anchorMax) * 0.5f;
    /// };
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:RectAnchorPresets /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class RectAnchorPresets : BaseVisualElement
    {
        internal static readonly BindingId anchorMinProperty = nameof(anchorMin);

        internal static readonly BindingId anchorMaxProperty = nameof(anchorMax);

        /// <summary>
        /// The RectAnchorPresets main styling class.
        /// </summary>
        public const string ussClassName = "appui-rect-anchor-presets";

        /// <summary>
        /// The styling class of the 4×4 grid.
        /// </summary>
        public const string gridUssClassName = ussClassName + "__grid";

        /// <summary>
        /// The styling class of one row of the grid.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The styling class of one preset cell.
        /// </summary>
        public const string cellUssClassName = ussClassName + "__cell";

        /// <summary>
        /// The styling class of the modifier hints below the grid.
        /// </summary>
        public const string hintUssClassName = ussClassName + "__hint";

        /// <summary>
        /// The hint explaining what Shift does. Exposed so a caller can localise it.
        /// </summary>
        public const string shiftHint = "Shift: Also set pivot";

        /// <summary>
        /// The hint explaining what Alt does. Exposed so a caller can localise it.
        /// </summary>
        public const string altHint = "Alt: Also set position";

        /// <summary>
        /// The grid's column spans: left, centre, right, stretch.
        /// </summary>
        public static readonly Vector2[] columns =
        {
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };

        /// <summary>
        /// The grid's row spans, y-up: top, middle, bottom, stretch.
        /// </summary>
        public static readonly Vector2[] rows =
        {
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
        };

        static readonly string[] k_ColumnNames = { "left", "center", "right", "stretch-h" };

        static readonly string[] k_RowNames = { "top", "middle", "bottom", "stretch-v" };

        readonly VisualElement m_Grid;

        readonly VisualElement[] m_Cells = new VisualElement[16];

        Vector2 m_AnchorMin = new Vector2(float.NaN, float.NaN);

        Vector2 m_AnchorMax = new Vector2(float.NaN, float.NaN);

        /// <summary>
        /// Raised when a preset is chosen.
        /// </summary>
        public event Action<RectAnchorPickEvent> picked;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RectAnchorPresets()
        {
            AddToClassList(ussClassName);

            m_Grid = new VisualElement { name = gridUssClassName };
            m_Grid.AddToClassList(gridUssClassName);
            hierarchy.Add(m_Grid);

            for (var r = 0; r < rows.Length; r++)
            {
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList(rowUssClassName);
                m_Grid.hierarchy.Add(row);

                for (var c = 0; c < columns.Length; c++)
                {
                    var cx = columns[c];
                    var ry = rows[r];
                    var cell = new VisualElement
                    {
                        name = cellUssClassName + "-" + r + "-" + c,
                        focusable = true,
                        tooltip = "Anchor " + k_ColumnNames[c] + " / " + k_RowNames[r],
                    };
                    cell.AddToClassList(cellUssClassName);

                    var glyph = new RectAnchorPresetGlyph
                    {
                        anchorMin = new Vector2(cx.x, ry.x),
                        anchorMax = new Vector2(cx.y, ry.y),
                    };
                    cell.hierarchy.Add(glyph);

                    cell.RegisterCallback<ClickEvent>(evt => picked?.Invoke(new RectAnchorPickEvent(
                        new Vector2(cx.x, ry.x),
                        new Vector2(cx.y, ry.y),
                        evt.shiftKey,
                        evt.altKey)));

                    row.hierarchy.Add(cell);
                    m_Cells[r * columns.Length + c] = cell;
                }
            }

            var pivotHint = new LocalizedTextElement
            {
                text = shiftHint,
                pickingMode = PickingMode.Ignore,
            };
            pivotHint.AddToClassList(hintUssClassName);
            var positionHint = new LocalizedTextElement
            {
                text = altHint,
                pickingMode = PickingMode.Ignore,
            };
            positionHint.AddToClassList(hintUssClassName);
            hierarchy.Add(pivotHint);
            hierarchy.Add(positionHint);

            RefreshSelection();
        }

        /// <summary>
        /// The currently applied bottom-left anchor, used to highlight the matching preset. Unset —
        /// NaN — highlights nothing.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2 anchorMin
        {
            get => m_AnchorMin;
            set
            {
                var changed = !m_AnchorMin.Equals(value);
                m_AnchorMin = value;
                RefreshSelection();

                if (changed)
                    NotifyPropertyChanged(in anchorMinProperty);
            }
        }

        /// <summary>
        /// The currently applied top-right anchor, used to highlight the matching preset. Unset — NaN —
        /// highlights nothing.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Vector2 anchorMax
        {
            get => m_AnchorMax;
            set
            {
                var changed = !m_AnchorMax.Equals(value);
                m_AnchorMax = value;
                RefreshSelection();

                if (changed)
                    NotifyPropertyChanged(in anchorMaxProperty);
            }
        }

        /// <summary>
        /// One preset cell, indexed row-major from the top-left.
        /// </summary>
        /// <param name="row"> The row, 0..3, top to stretch. </param>
        /// <param name="column"> The column, 0..3, left to stretch. </param>
        /// <returns> The cell element. </returns>
        public VisualElement GetCell(int row, int column) => m_Cells[row * columns.Length + column];

        /// <summary>
        /// Whether one cell is the preset currently applied.
        /// </summary>
        /// <param name="row"> The row, 0..3. </param>
        /// <param name="column"> The column, 0..3. </param>
        /// <returns> Whether the cell matches <see cref="anchorMin"/> and <see cref="anchorMax"/>. </returns>
        public bool IsSelected(int row, int column) =>
            Matches(columns[column], rows[row]);

        bool Matches(Vector2 cx, Vector2 ry) =>
            m_AnchorMin.x == cx.x && m_AnchorMax.x == cx.y &&
            m_AnchorMin.y == ry.x && m_AnchorMax.y == ry.y;

        void RefreshSelection()
        {
            for (var r = 0; r < rows.Length; r++)
            {
                for (var c = 0; c < columns.Length; c++)
                    GetCell(r, c).EnableInClassList(Styles.selectedUssClassName, Matches(columns[c], rows[r]));
            }
        }
    }
}
