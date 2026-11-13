#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CanvasNode"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CanvasNodeTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CanvasNode))]
    class CanvasNodeSnapshotTests : UISnapshotTests<CanvasNode>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new CanvasNode { title = "State" });
                yield return new Story("Ports", (ctx) => Graph());
                yield return new Story("Entry", (ctx) => new CanvasNode
                {
                    title = "Entry",
                    variant = CanvasNodeVariant.Entry,
                });
                yield return new Story("Selected", (ctx) => new CanvasNode
                {
                    title = "Selected",
                    selected = true,
                });
            }
        }


        static CanvasNode Graph() => new CanvasNode
        {
            title = "Idle",
            x = 40f,
            y = 24f,
            inputPorts = new List<CanvasPort>
            {
                new CanvasPort("in-a", "Base"),
                new CanvasPort("in-b", "Additive"),
            },
            outputPorts = new List<CanvasPort> { new CanvasPort("out", "Result") },
        };
    }
}
#endif
