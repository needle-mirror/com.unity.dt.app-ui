#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Canvas"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CanvasTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Canvas))]
    class CanvasSnapshotTests : UISnapshotTests<Canvas>
    {
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
    }
}
#endif
