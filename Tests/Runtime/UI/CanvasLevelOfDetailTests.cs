#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CanvasCulling))]
    class CanvasLevelOfDetailTests : UIComponentTests<CanvasNode>
    {
        const float k_Tolerance = 0.01f;

        CanvasNode MountPortedNode(CanvasLevelOfDetail lod)
        {
            var node = new CanvasNode
            {
                title = "Node",
                x = 0f,
                y = 0f,
                lod = lod,
                inputPorts = new[] { new CanvasPort("in", "Input") },
                outputPorts = new[] { new CanvasPort("out", "Output"), new CanvasPort("out2", "Second") },
            };
            node.Add(new Label("Body"));
            return MountElement(node);
        }

        static VisualElement Body(CanvasNode node) => node.Q(CanvasNode.bodyUssClassName);

        static VisualElement Title(CanvasNode node) => node.Q(CanvasNode.titleUssClassName);

        static VisualElement PortLabel(CanvasNode node) => node.Q(className: CanvasNode.portLabelUssClassName);

        static VisualElement PortHit(CanvasNode node) => node.Q(className: CanvasNode.portHitUssClassName);

        [TestCase(1f, CanvasLevelOfDetail.Full)]
        [TestCase(0.5f, CanvasLevelOfDetail.Full)]
        [TestCase(0.49f, CanvasLevelOfDetail.Compact)]
        [TestCase(0.3f, CanvasLevelOfDetail.Compact)]
        [TestCase(0.29f, CanvasLevelOfDetail.Box)]
        public void LevelOfDetailForZoom_ShouldMatchRegistryThresholds(float zoom, CanvasLevelOfDetail expected)
        {
            Assert.AreEqual(expected, CanvasCulling.LevelOfDetailForZoom(zoom));
        }

        [Test]
        public void TryViewportToContent_ShouldGrowByMarginAndDivideByZoom()
        {
            Assert.IsTrue(CanvasCulling.TryViewportToContent(2f, new Vector2(100f, 50f), new Vector2(800f, 600f), 200f, out var r));
            Assert.AreEqual(-150f, r.xMin, k_Tolerance);
            Assert.AreEqual(-125f, r.yMin, k_Tolerance);
            Assert.AreEqual(450f, r.xMax, k_Tolerance);
            Assert.AreEqual(375f, r.yMax, k_Tolerance);
        }

        [Test]
        public void TryViewportToContent_WithoutSize_ShouldFail()
        {
            Assert.IsFalse(CanvasCulling.TryViewportToContent(1f, Vector2.zero, new Vector2(0f, 600f), 0f, out _));
            Assert.IsFalse(CanvasCulling.TryViewportToContent(0f, Vector2.zero, new Vector2(800f, 600f), 0f, out _));
        }

        [Test]
        public void CanvasNode_Full_ShouldShowEverything()
        {
            var node = MountPortedNode(CanvasLevelOfDetail.Full);
            Assert.AreEqual(DisplayStyle.Flex, Body(node).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, PortLabel(node).resolvedStyle.display);
            Assert.AreEqual(Visibility.Visible, Title(node).resolvedStyle.visibility);
        }

        [Test]
        public void CanvasNode_Compact_ShouldHideLabelsAndBodyButKeepTitleAndDots()
        {
            var node = MountPortedNode(CanvasLevelOfDetail.Compact);
            Assert.AreEqual(DisplayStyle.None, Body(node).resolvedStyle.display, "Compact drops the body.");
            Assert.AreEqual(DisplayStyle.None, PortLabel(node).resolvedStyle.display, "Compact drops port labels.");
            Assert.AreEqual(Visibility.Visible, Title(node).resolvedStyle.visibility, "Compact keeps the title.");
            Assert.AreEqual(DisplayStyle.Flex, PortHit(node).resolvedStyle.display, "Compact keeps the port dots.");
        }

        [Test]
        public void CanvasNode_Box_ShouldHideTitleAndPortsButKeepHeader()
        {
            var node = MountPortedNode(CanvasLevelOfDetail.Box);
            Assert.AreEqual(DisplayStyle.None, Body(node).resolvedStyle.display);
            Assert.AreEqual(Visibility.Hidden, Title(node).resolvedStyle.visibility, "Box hides the title.");
            Assert.AreEqual(DisplayStyle.None, PortHit(node).resolvedStyle.display, "Box hides the port targets.");
            Assert.AreEqual(CanvasNode.headerHeight, node.header.layout.height, k_Tolerance,
                "Box keeps the header's height so the header colour still shows.");
        }

        [TestCase(CanvasLevelOfDetail.Compact)]
        [TestCase(CanvasLevelOfDetail.Box)]
        public void CanvasNode_ReducedLod_ShouldKeepPortRowGeometry(CanvasLevelOfDetail lod)
        {
            var full = MountPortedNode(CanvasLevelOfDetail.Full);
            var fullRow = full.GetPortElement(CanvasPortSide.Output, 1).worldBound.y - full.worldBound.y;

            var reduced = MountPortedNode(lod);
            var reducedRow = reduced.GetPortElement(CanvasPortSide.Output, 1).worldBound.y - reduced.worldBound.y;

            Assert.AreEqual(fullRow, reducedRow, k_Tolerance, "Port rows must stay where edges anchor to them.");
            Assert.AreEqual(full.GetPortAnchor(CanvasPortSide.Output, 1).y,
                reduced.GetPortAnchor(CanvasPortSide.Output, 1).y, k_Tolerance);
        }

        [Test]
        public void CanvasNode_Lod_ShouldSwapClassBackToFull()
        {
            var node = MountPortedNode(CanvasLevelOfDetail.Box);
            node.lod = CanvasLevelOfDetail.Full;
            simulate.FrameUpdate();
            Assert.IsFalse(node.ClassListContains(CanvasNode.GetLodUssClassName(CanvasLevelOfDetail.Box)));
            Assert.AreEqual(Visibility.Visible, Title(node).resolvedStyle.visibility);
            Assert.AreEqual(DisplayStyle.Flex, Body(node).resolvedStyle.display);
        }

        [Test]
        public void CanvasEdge_Box_ShouldDropHitAreaAndArrow()
        {
            var edge = new CanvasEdge
            {
                from = new Vector2(0f, 0f),
                to = new Vector2(200f, 0f),
                curveStyle = CanvasCurveStyle.Straight,
                isDirectional = true,
            };
            var onLine = new Vector2(100f, 0f) - edge.bounds.min;

            Assert.IsTrue(edge.ContainsPoint(onLine), "A full edge is pickable on its line.");
            Assert.IsTrue(edge.drawsArrowHead, "A full directional edge draws its arrow.");

            edge.lod = CanvasLevelOfDetail.Compact;
            Assert.IsTrue(edge.ContainsPoint(onLine), "A compact edge is still pickable.");
            Assert.IsTrue(edge.drawsArrowHead, "A compact directional edge still draws its arrow.");

            edge.lod = CanvasLevelOfDetail.Box;
            Assert.IsFalse(edge.ContainsPoint(onLine), "A box edge has no hit area.");
            Assert.AreEqual(PickingMode.Ignore, edge.pickingMode);
            Assert.IsFalse(edge.drawsArrowHead, "A box edge draws no arrow.");

            edge.lod = CanvasLevelOfDetail.Full;
            Assert.IsTrue(edge.ContainsPoint(onLine));
            Assert.AreEqual(PickingMode.Position, edge.pickingMode);
        }

        [Test]
        public void IsVisible_Node_ShouldCullOffViewUnlessSelected()
        {
            var view = new Rect(0f, 0f, 500f, 500f);
            var inside = new CanvasNode { x = 100f, y = 100f };
            var outside = new CanvasNode { x = 1000f, y = 1000f };

            Assert.IsTrue(CanvasCulling.IsVisible(inside, view, 50f));
            Assert.IsFalse(CanvasCulling.IsVisible(outside, view, 50f));

            outside.selected = true;
            Assert.IsTrue(CanvasCulling.IsVisible(outside, view, 50f), "A selected node is pinned.");
        }

        [Test]
        public void IsVisible_UnmeasuredNode_ShouldUsePortGeometryOrStayKept()
        {
            var view = new Rect(0f, 0f, 500f, 500f);
            var ported = new CanvasNode
            {
                title = "Node",
                x = 100f,
                y = -60f,
                inputPorts = new[] { new CanvasPort("a"), new CanvasPort("b"), new CanvasPort("c") },
            };
            Assert.IsTrue(CanvasCulling.IsVisible(ported, view),
                "An unmeasured node whose port rows reach into view must be kept.");

            var bare = new CanvasNode { x = 100f, y = -60f };
            Assert.IsTrue(CanvasCulling.IsVisible(bare, view), "A node with no known height is kept.");

            var far = new CanvasNode { title = "Far", x = 100f, y = -500f };
            Assert.IsFalse(CanvasCulling.IsVisible(far, view), "A header far above the view is still culled.");
        }

        [Test]
        public void CanvasNode_BoxVariant_ShouldFillPortAreaWithHeaderColor()
        {
            var node = MountPortedNode(CanvasLevelOfDetail.Box);
            node.variant = CanvasNodeVariant.Entry;
            simulate.FrameUpdate();
            Assert.AreEqual(node.header.resolvedStyle.backgroundColor, node.resolvedStyle.backgroundColor,
                "A box draws one block of the header colour.");
        }

        [Test]
        public void IsVisible_Edge_ShouldKeepEdgeCrossingViewBetweenOffscreenNodes()
        {
            var view = new Rect(0f, 0f, 500f, 500f);
            var crossing = new CanvasEdge { from = new Vector2(-1000f, 250f), to = new Vector2(1500f, 250f) };
            var away = new CanvasEdge { from = new Vector2(-1000f, 2000f), to = new Vector2(1500f, 2000f) };

            Assert.IsTrue(CanvasCulling.IsVisible(crossing, view));
            Assert.IsFalse(CanvasCulling.IsVisible(away, view));

            away.selected = true;
            Assert.IsTrue(CanvasCulling.IsVisible(away, view), "A selected edge is pinned.");
        }

        [Test]
        public void Canvas_TryGetVisibleContentRect_ShouldTrackZoomAndScroll()
        {
            var canvas = new Unity.AppUI.UI.Canvas { style = { width = 400, height = 300 } };
            appUiPanel.Clear();
            appUiPanel.Add(canvas);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            canvas.zoom = 2f;
            canvas.scrollOffset = new Vector2(100f, 40f);

            Assert.IsTrue(canvas.TryGetVisibleContentRect(out var r, 0f));
            Assert.AreEqual(100f / 2f, r.xMin, k_Tolerance);
            Assert.AreEqual(40f / 2f, r.yMin, k_Tolerance);
            Assert.AreEqual(canvas.contentRect.width / 2f, r.width, k_Tolerance);
            Assert.AreEqual(canvas.contentRect.height / 2f, r.height, k_Tolerance);
        }
    }
}
#endif
