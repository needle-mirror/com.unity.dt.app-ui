using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Canvas))]
    class CanvasTests : VisualElementTests<Canvas>
    {

        // A Canvas is a viewport: no intrinsic size, and nothing to show without content.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var canvas = new Canvas { style = { width = 320, height = 200 } };
                    canvas.Add(new Unity.AppUI.UI.Button { title = "Node" });
                    return canvas;
                });
            }
        }

        protected override string mainUssClassName => Canvas.ussClassName;

        // Handed over to CanvasSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            "<appui:Canvas/>",
            @"<Unity.AppUI.UI.Canvas enabled=""true"" frame-container=""0,0,100,100"" scroll-speed=""2.1"" min-zoom=""0.11"" max-zoom=""99"" zoom-speed=""0.0751"" zoom-multiplier=""2.1"" pan-multiplier=""3.1"" scroll-direction=""Inverse"" zoom=""1.1"" frame-margin=""12.2"" use-space-bar=""false"" control-scheme=""Editor"" primary-manipulator=""Pan"" style=""width: 277px; height: 257px;"" />",
        };
    }
}
