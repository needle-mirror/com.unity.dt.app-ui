#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CanvasEdge"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CanvasEdgeTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CanvasEdge))]
    class CanvasEdgeSnapshotTests : UISnapshotTests<CanvasEdge>
    {
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


        static CanvasEdge Edge(CanvasCurveStyle style = CanvasCurveStyle.Bezier) => new CanvasEdge
        {
            from = k_From,
            to = k_To,
            curveStyle = style,
            isDirectional = true,
        };


        static readonly Vector2 k_From = new Vector2(40f, 60f);


        static readonly Vector2 k_To = new Vector2(240f, 160f);
    }
}
#endif
