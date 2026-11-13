using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A responsive gallery that flows <see cref="Card"/> tiles into as many columns as the available
    /// width allows.
    /// </summary>
    /// <remarks>
    /// CardGrid is the UITK counterpart of a CSS `repeat(auto-fill, ...)` grid. UI Toolkit has no CSS
    /// grid and no `gap`, so the layout is computed instead: the grid is a wrapping flex row, and on
    /// every resize it works out how many columns of at least <see cref="minItemWidth"/> fit, then
    /// writes a width and the inter-tile margins onto each child. The result matches the web
    /// component's behaviour, including the ragged trailing row.
    ///
    /// <see cref="fill"/> picks between the two column strategies:
    /// with `true` (the default) the columns stretch to share the leftover width, so tiles only change
    /// size when the column count changes; with `false` every column is exactly
    /// <see cref="minItemWidth"/> wide and the row is left-packed, so tile size tracks
    /// <see cref="minItemWidth"/> continuously.
    ///
    /// The gap stays a token rather than a C# constant. `CardGrid.uss` sizes a hidden, absolutely
    /// positioned probe element from a density-aware `--appui-sizing-*` step, and the grid reads the
    /// gap back off that probe's resolved width. It therefore rescales with `Panel.density` on its own
    /// and never appears as a literal in code. (UITK cannot read a `px` length back through a float
    /// custom property, which is why the probe exists.)
    ///
    /// The grid is layout-direction aware. Under `.appui--rtl` the row itself flips to
    /// `row-reverse` in USS, so tile 0 sits against the right edge, and <see cref="Reflow"/> writes the
    /// inter-tile gap onto `margin-left` instead of `margin-right` so it stays on the trailing edge of
    /// each tile. The column arithmetic is direction-agnostic, so column count and tile widths are
    /// identical in both directions.
    ///
    /// **Note:** because CardGrid writes `width`, `margin-left`, `margin-right` and `margin-bottom`
    /// onto its children, do not set those four inline on a tile — they are overwritten on every
    /// re-flow. Everything else, including the tile height, is yours.
    /// </remarks>
    /// <example>
    /// <para>A template gallery in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:CardGrid min-item-width="240">
    ///     <appui:Card interactive="true">
    ///         <appui:CardHeader><appui:CardTitle text="2D Platformer" /></appui:CardHeader>
    ///     </appui:Card>
    ///     <appui:Card interactive="true">
    ///         <appui:CardHeader><appui:CardTitle text="First Person" /></appui:CardHeader>
    ///     </appui:Card>
    /// </appui:CardGrid>
    /// ]]></code>
    /// <para>Fixed-width columns, built in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var grid = new CardGrid { minItemWidth = 160, fill = false };
    /// foreach (var template in templates)
    /// {
    ///     var card = new Card { interactive = true };
    ///     var header = new CardHeader();
    ///     header.Add(new CardTitle { text = template.name });
    ///     card.Add(header);
    ///     grid.Add(card);
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class CardGrid : BaseVisualElement
    {
        internal static readonly BindingId minItemWidthProperty = nameof(minItemWidth);

        internal static readonly BindingId fillProperty = nameof(fill);

        /// <summary>
        /// The CardGrid main styling class.
        /// </summary>
        public const string ussClassName = "appui-card-grid";

        /// <summary>
        /// The styling class of the hidden element the grid measures its inter-tile gap from.
        /// </summary>
        public const string gapProbeUssClassName = ussClassName + "__gap-probe";

        const float k_DefaultMinItemWidth = 200f;

        readonly VisualElement m_GapProbe;

        readonly List<VisualElement> m_Tiles = new List<VisualElement>();

        float m_MinItemWidth = k_DefaultMinItemWidth;

        bool m_Fill = true;

        float m_Gap;

        int m_ColumnCount = 1;

        Dir m_LayoutDirection = Dir.Ltr;

        /// <summary>
        /// The per-item width, in pixels.
        /// </summary>
        /// <remarks>
        /// With <see cref="fill"/> this is the *minimum* width a tile may have before the grid drops a
        /// column; without it, it is the *exact* column width.
        /// </remarks>
        [Tooltip("The minimum width of a tile before the grid drops a column.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minItemWidth
        {
            get => m_MinItemWidth;
            set
            {
                var changed = !Mathf.Approximately(m_MinItemWidth, value);
                m_MinItemWidth = Mathf.Max(1f, value);
                Reflow();

                if (changed)
                    NotifyPropertyChanged(in minItemWidthProperty);
            }
        }

        /// <summary>
        /// Whether the columns stretch to fill the row.
        /// </summary>
        /// <remarks>
        /// `true` (the default) shares the leftover width between the columns, so the trailing edge is
        /// flush. `false` pins every column to <see cref="minItemWidth"/> and leaves the leftover width
        /// unused at the end of the row.
        /// </remarks>
        [Tooltip("Whether the columns stretch to share the leftover width.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool fill
        {
            get => m_Fill;
            set
            {
                var changed = m_Fill != value;
                m_Fill = value;
                Reflow();

                if (changed)
                    NotifyPropertyChanged(in fillProperty);
            }
        }

        /// <summary>
        /// The gap currently applied between tiles, in resolved pixels.
        /// </summary>
        /// <remarks>
        /// Read-only, and driven by the `--appui-card-grid-gap` custom property, so it changes with the
        /// panel density rather than with any C# setting.
        /// </remarks>
        public float resolvedGap => m_Gap;

        /// <summary>
        /// The number of columns the grid last flowed into. `1` until the grid has been laid out.
        /// </summary>
        public int resolvedColumnCount => m_ColumnCount;

        /// <summary>
        /// The layout direction the grid last flowed in.
        /// </summary>
        /// <remarks>
        /// Read-only, and driven by the nearest <see cref="DirContext"/> provider, usually the
        /// <see cref="Panel"/>. In <see cref="Dir.Rtl"/> the tiles flow from the right edge and the gap
        /// moves to each tile's left side.
        /// </remarks>
        public Dir resolvedLayoutDirection => m_LayoutDirection;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardGrid()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_GapProbe = new VisualElement { name = gapProbeUssClassName, pickingMode = PickingMode.Ignore };
            m_GapProbe.AddToClassList(gapProbeUssClassName);
            hierarchy.Add(m_GapProbe);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_GapProbe.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            this.RegisterContextChangedCallback<DirContext>(OnLayoutDirectionChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt) => Reflow();

        void OnLayoutDirectionChanged(ContextChangedEvent<DirContext> evt)
        {
            var newDirection = evt.context?.dir ?? Dir.Ltr;
            if (m_LayoutDirection == newDirection)
                return;

            m_LayoutDirection = newDirection;
            Reflow();
        }

        /// <summary>
        /// Recomputes the column count and re-applies the tile widths and margins.
        /// </summary>
        /// <remarks>
        /// This runs automatically whenever the grid resizes, its density changes or one of its
        /// properties is set. Call it directly only after mutating the children outside a layout pass.
        /// </remarks>
        public void Reflow()
        {
            var available = contentRect.width;
            var probeWidth = m_GapProbe.resolvedStyle.width;
            m_Gap = float.IsNaN(probeWidth) ? 0f : Mathf.Max(0f, probeWidth);

            m_Tiles.Clear();
            for (var i = 0; i < childCount; i++)
            {
                var child = this[i];
                if (child != m_GapProbe)
                    m_Tiles.Add(child);
            }

            if (float.IsNaN(available) || available <= 0f || m_Tiles.Count == 0)
                return;

            var gap = m_Gap;
            // auto-fill: the widest run of `columns` tiles of `minItemWidth` separated by `gap` that
            // still fits. Adding one gap to both sides of the division removes the trailing gap.
            var columns = Mathf.Max(1, Mathf.FloorToInt((available + gap) / (m_MinItemWidth + gap)));
            m_ColumnCount = columns;

            var itemWidth = m_Fill
                ? (available - gap * (columns - 1)) / columns
                : m_MinItemWidth;

            var lastRowStart = ((m_Tiles.Count - 1) / columns) * columns;

            var rtl = m_LayoutDirection == Dir.Rtl;

            for (var i = 0; i < m_Tiles.Count; i++)
            {
                var tile = m_Tiles[i];
                tile.style.flexGrow = 0;
                tile.style.flexShrink = 0;
                tile.style.width = itemWidth;
                // No trailing gap on the last column, or the row overflows and wraps a column early.
                var trailingGap = (i % columns == columns - 1) ? 0f : gap;
                // The row is `row-reverse` under `.appui--rtl`, so the trailing edge of a tile is its
                // left side. Both margins are always written, so nothing stale survives a flip.
                tile.style.marginLeft = rtl ? trailingGap : 0f;
                tile.style.marginRight = rtl ? 0f : trailingGap;
                tile.style.marginBottom = i < lastRowStart ? gap : 0f;
            }
        }
    }
}
