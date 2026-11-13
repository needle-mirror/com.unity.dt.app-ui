using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How much of a <see cref="CanvasNode"/> or <see cref="CanvasEdge"/> to draw, picked from the
    /// canvas zoom with <see cref="CanvasCulling.LevelOfDetailForZoom"/>.
    /// </summary>
    public enum CanvasLevelOfDetail
    {
        /// <summary>
        /// Everything: title, labelled ports and body.
        /// </summary>
        Full,

        /// <summary>
        /// A node's title and port dots, without port labels or body.
        /// </summary>
        Compact,

        /// <summary>
        /// A node's header colour over its port area and nothing else. An edge draws its line only,
        /// with no hit area and no arrow.
        /// </summary>
        Box,
    }

    /// <summary>
    /// Level-of-detail and viewport-culling helpers for content placed in a <see cref="Canvas"/>.
    /// </summary>
    /// <remarks>
    /// Mirrors the registry's <c>canvas-lod</c> and <c>canvas-culling</c> modules. The canvas never
    /// applies them on its own: a consumer reads <see cref="Canvas.zoom"/> to pick a level for its
    /// nodes and edges, and keeps only the content that overlaps the rect
    /// <see cref="Canvas.TryGetVisibleContentRect"/> returns.
    /// </remarks>
    public static class CanvasCulling
    {
        /// <summary>
        /// The default zoom below which content draws at <see cref="CanvasLevelOfDetail.Compact"/>.
        /// Under half scale a node's port labels fall below a readable size.
        /// </summary>
        public const float compactZoomThreshold = 0.5f;

        /// <summary>
        /// The default zoom below which content draws at <see cref="CanvasLevelOfDetail.Box"/>.
        /// Under about a third of full scale a node's title is no longer readable.
        /// </summary>
        public const float boxZoomThreshold = 0.3f;

        /// <summary>
        /// The default screen margin, in pixels, that culling keeps around the viewport, so content
        /// panned or zoomed into view is already there instead of appearing a frame late.
        /// </summary>
        public const float defaultMargin = 200f;

        /// <summary>
        /// The level of detail to draw at for a zoom, where 1 is 100%.
        /// </summary>
        /// <param name="zoom"> The canvas zoom. </param>
        /// <param name="compactThreshold"> Below this zoom, content draws compact. </param>
        /// <param name="boxThreshold"> Below this zoom, content draws as a box. Lower than <paramref name="compactThreshold"/>. </param>
        /// <returns> The level of detail. </returns>
        public static CanvasLevelOfDetail LevelOfDetailForZoom(
            float zoom,
            float compactThreshold = compactZoomThreshold,
            float boxThreshold = boxZoomThreshold)
        {
            if (zoom < boxThreshold)
                return CanvasLevelOfDetail.Box;
            if (zoom < compactThreshold)
                return CanvasLevelOfDetail.Compact;
            return CanvasLevelOfDetail.Full;
        }

        /// <summary>
        /// The content-space box a viewport shows, grown by <paramref name="margin"/> screen pixels on
        /// every side.
        /// </summary>
        /// <param name="zoom"> The canvas zoom. </param>
        /// <param name="offset"> Where the content origin sits on screen, in pixels from the viewport's top-left corner. </param>
        /// <param name="size"> The viewport's on-screen size, in pixels. </param>
        /// <param name="margin"> The screen margin to keep around the viewport. </param>
        /// <param name="contentRect"> The visible content-space box. </param>
        /// <returns> False while the viewport has no size or the zoom is not positive, when nothing can be said about what is visible. </returns>
        public static bool TryViewportToContent(
            float zoom, Vector2 offset, Vector2 size, float margin, out Rect contentRect)
        {
            contentRect = default;
            if (!(size.x > 0f) || !(size.y > 0f) || !(zoom > 0f))
                return false;

            var minX = (-offset.x - margin) / zoom;
            var minY = (-offset.y - margin) / zoom;
            var maxX = (size.x - offset.x + margin) / zoom;
            var maxY = (size.y - offset.y + margin) / zoom;
            contentRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        /// <summary>
        /// Whether a node should be kept for a visible content box: it overlaps the box, or it is
        /// selected, which keeps it whatever its position.
        /// </summary>
        /// <param name="node"> The node. </param>
        /// <param name="view"> The visible content box. </param>
        /// <param name="height"> The node's content-space height; its laid-out height when 0 or less, or, before
        /// the first layout, the height of its header and port rows. </param>
        /// <returns> Whether to keep the node. </returns>
        public static bool IsVisible(CanvasNode node, Rect view, float height = 0f)
        {
            if (node == null)
                return false;
            if (node.selected)
                return true;
            var x = node.x.IsSet ? node.x.Value : 0f;
            var y = node.y.IsSet ? node.y.Value : 0f;
            var h = height > 0f ? height : node.layout.height;
            if (float.IsNaN(h) || h <= 0f)
            {
                // Not laid out yet: estimate from the header and port rows, whose geometry is fixed.
                // A node with neither is kept, so an unmeasured node is never culled for good.
                h = node.GetPortAreaBottom();
                if (h <= 0f)
                    return true;
            }
            return Overlaps(new Rect(x, y, node.width, h), view);
        }

        /// <summary>
        /// Whether an edge should be kept for a visible content box. An edge is kept by its own bounds,
        /// so an edge crossing the view between two off-screen nodes stays while its nodes do not. A
        /// selected or active edge is always kept.
        /// </summary>
        /// <param name="edge"> The edge. </param>
        /// <param name="view"> The visible content box. </param>
        /// <returns> Whether to keep the edge. </returns>
        public static bool IsVisible(CanvasEdge edge, Rect view)
        {
            if (edge == null)
                return false;
            return edge.highlighted || Overlaps(edge.bounds, view);
        }

        // Inclusive on the edges: a box touching the view counts as visible.
        static bool Overlaps(Rect a, Rect b) =>
            a.xMin <= b.xMax && a.xMax >= b.xMin && a.yMin <= b.yMax && a.yMax >= b.yMin;
    }
}
