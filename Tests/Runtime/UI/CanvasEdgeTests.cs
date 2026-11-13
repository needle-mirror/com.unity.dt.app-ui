using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CanvasEdge))]
    class CanvasEdgeTests : VisualElementTests<CanvasEdge>
    {
        const string k_Uss = "CanvasEdge.uss";

        protected override string mainUssClassName => CanvasEdge.ussClassName;

        // Handed over to CanvasEdgeSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly Vector2 k_From = new Vector2(40f, 60f);

        static readonly Vector2 k_To = new Vector2(240f, 160f);

        static CanvasEdge Edge(CanvasCurveStyle style = CanvasCurveStyle.Bezier) => new CanvasEdge
        {
            from = k_From,
            to = k_To,
            curveStyle = style,
            isDirectional = true,
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                foreach (CanvasCurveStyle style in System.Enum.GetValues(typeof(CanvasCurveStyle)))
                {
                    var captured = style;
                    yield return new Story(style.ToString(), (ctx) =>
                    {
                        var host = new VisualElement { style = { width = 320, height = 240 } };
                        host.Add(Edge(captured));
                        return host;
                    });
                }
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CanvasEdge />",
            @"<appui:CanvasEdge curve-style=""Cable"" is-directional=""true"" active=""true"" />",
            @"<appui:CanvasEdge from=""40,60"" to=""240,160"" curve-style=""Step"" />",
        };

        [Test]
        public void CurveStyle_SwapsTheClassInsteadOfAccumulating()
        {
            var edge = new CanvasEdge();
            Assert.IsTrue(edge.ClassListContains(CanvasEdge.GetCurveStyleUssClassName(CanvasCurveStyle.Bezier)),
                "Bezier is the registry's default curve style.");

            edge.curveStyle = CanvasCurveStyle.Step;
            Assert.IsTrue(edge.ClassListContains(CanvasEdge.GetCurveStyleUssClassName(CanvasCurveStyle.Step)));
            Assert.IsFalse(edge.ClassListContains(CanvasEdge.GetCurveStyleUssClassName(CanvasCurveStyle.Bezier)),
                "The previous curve style class must come off, not accumulate.");
        }

        [Test]
        public void Highlight_IsEitherActiveOrSelected()
        {
            var edge = new CanvasEdge();
            Assert.IsFalse(edge.highlighted);

            edge.active = true;
            Assert.IsTrue(edge.highlighted);
            Assert.IsTrue(edge.ClassListContains(CanvasEdge.activeUssClassName));

            edge.active = false;
            edge.selected = true;
            Assert.IsTrue(edge.highlighted);
            Assert.IsTrue(edge.ClassListContains(Styles.selectedUssClassName));

            edge.selected = false;
            Assert.IsFalse(edge.highlighted);
            Assert.IsFalse(edge.ClassListContains(Styles.selectedUssClassName));
        }

        /// <summary>
        /// The whole point of an edge is that it starts and ends exactly on the two port anchors it was
        /// given. Sampling the path is the only way to assert that against the geometry actually drawn.
        /// </summary>
        [Test]
        public void Path_StartsAndEndsOnTheGivenPoints()
        {
            foreach (CanvasCurveStyle style in System.Enum.GetValues(typeof(CanvasCurveStyle)))
            {
                var edge = Edge(style);
                var points = edge.SamplePath();
                Assert.Greater(points.Count, 2, $"{style} produced no path.");
                Assert.AreEqual(k_From.x, points[0].x, 0.001f, $"{style} does not start on `from`.");
                Assert.AreEqual(k_From.y, points[0].y, 0.001f, $"{style} does not start on `from`.");
                Assert.AreEqual(k_To.x, points[points.Count - 1].x, 0.001f, $"{style} does not end on `to`.");
                Assert.AreEqual(k_To.y, points[points.Count - 1].y, 0.001f, $"{style} does not end on `to`.");
            }
        }

        [Test]
        public void Straight_RunsDirectlyBetweenTheTwoPoints()
        {
            var edge = Edge(CanvasCurveStyle.Straight);
            Assert.AreEqual((k_From + k_To) * 0.5f, edge.midpoint);

            var expected = Mathf.Atan2(k_To.y - k_From.y, k_To.x - k_From.x) * Mathf.Rad2Deg;
            Assert.AreEqual(expected, edge.midpointAngle, 0.001f);
        }

        /// <summary>
        /// The registry's bezier control points are horizontally mirrored about the midpoint, so at
        /// t = 0.5 the curve passes exactly through the straight-line midpoint. If the control points
        /// stop mirroring, this moves.
        /// </summary>
        [Test]
        public void Bezier_PassesThroughTheStraightMidpointAndLeavesHorizontally()
        {
            var edge = Edge(CanvasCurveStyle.Bezier);
            Assert.AreEqual(((k_From + k_To) * 0.5f).x, edge.midpoint.x, 0.01f);
            Assert.AreEqual(((k_From + k_To) * 0.5f).y, edge.midpoint.y, 0.01f);

            var points = edge.SamplePath();
            Assert.AreEqual(k_From.y, points[1].y, 0.5f,
                "A bezier edge must leave its port horizontally, the way a port dot points.");
        }

        [Test]
        public void Step_TurnsThroughAVerticalMidSegment()
        {
            var edge = Edge(CanvasCurveStyle.Step);
            Assert.AreEqual((k_From.x + k_To.x) * 0.5f, edge.midpoint.x, 0.001f);
            Assert.AreEqual((k_From.y + k_To.y) * 0.5f, edge.midpoint.y, 0.001f);
            Assert.AreEqual(90f, edge.midpointAngle, 0.001f, "Descending, the arrow points down.");

            edge.to = new Vector2(k_To.x, k_From.y - 100f);
            Assert.AreEqual(-90f, edge.midpointAngle, 0.001f, "Ascending, the arrow points up.");
        }

        [Test]
        public void Cable_DroopsBelowTheStraightRun()
        {
            var straight = Edge(CanvasCurveStyle.Straight);
            var cable = Edge(CanvasCurveStyle.Cable);
            Assert.Greater(cable.midpoint.y, straight.midpoint.y,
                "A hung cable must sag below the straight line between its ends.");
            Assert.AreEqual(0f, straight.sag, 0.001f, "Only the cable style sags.");
            Assert.Greater(cable.sag, 0f);
        }

        [Test]
        public void Bounds_CoverBothEndpointsWithRoomForTheStroke()
        {
            var edge = Edge();
            var bounds = edge.bounds;
            Assert.IsTrue(bounds.Contains(k_From), "The edge's box must contain `from`.");
            Assert.IsTrue(bounds.Contains(k_To), "The edge's box must contain `to`.");
            Assert.AreEqual(Mathf.Min(k_From.x, k_To.x) - CanvasEdge.padding, bounds.xMin, 0.001f);
            Assert.AreEqual(Mathf.Min(k_From.y, k_To.y) - CanvasEdge.padding, bounds.yMin, 0.001f);

            Assert.AreEqual(bounds.xMin, edge.style.left.value.value, 0.001f);
            Assert.AreEqual(bounds.yMin, edge.style.top.value.value, 0.001f);
            Assert.AreEqual(bounds.width, edge.style.width.value.value, 0.001f);
        }

        /// <summary>
        /// The web draws a transparent 14px-wide path over the line so the edge is clickable. UITK has
        /// no such thing, so picking is answered by distance to the sampled path — which is the same
        /// idea without a second path to keep in sync.
        /// </summary>
        [Test]
        public void Picking_FollowsTheLineRatherThanTheBoundingBox()
        {
            var edge = Edge(CanvasCurveStyle.Straight);
            var onLine = (k_From + k_To) * 0.5f - edge.bounds.min;
            Assert.IsTrue(edge.ContainsPoint(onLine), "A point on the line must be pickable.");

            var corner = new Vector2(1f, edge.bounds.height - 1f);
            Assert.IsFalse(edge.ContainsPoint(corner),
                "A corner of the bounding box is nowhere near the line and must not be pickable.");
        }

        [UnityTest]
        public IEnumerator Colour_ResolvesFromTheStylesheetRatherThanALiteral()
        {
            var edge = Edge();
            var host = new VisualElement { style = { width = 400, height = 300 } };
            host.Add(edge);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            Assert.AreNotEqual(Color.clear, edge.resolvedColor,
                "The stroke colour must come from --edge-color. The C# field starts transparent on "
                + "purpose, so an unresolved custom property cannot pass for a real colour.");

            edge.active = true;
            yield return null;
            Assert.AreNotEqual(Color.clear, edge.resolvedHighlightColor);
            Assert.AreNotEqual(edge.resolvedColor, edge.resolvedHighlightColor,
                "A highlighted edge must read differently from an idle one.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Colours_BindToTokensThroughCustomProperties()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            var body = ChatTestUtils.RuleBody(uss, @"^\.appui-canvas-edge");
            StringAssert.Contains("--edge-color: var(--appui-base-500)", body);
            StringAssert.Contains("--edge-highlight-color: var(--appui-alias-actions-accent-background-color-default)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }
    }
}
