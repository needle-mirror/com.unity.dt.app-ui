#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="SelectionRect"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SelectionRectTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(SelectionRect))]
    class SelectionRectSnapshotTests : UISnapshotTests<SelectionRect>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Marquee", (ctx) =>
                {
                    var host = new UnityEngine.UIElements.VisualElement
                    {
                        style = { width = 240, height = 160 },
                    };
                    host.Add(new SelectionRect { rect = new Rect(20f, 20f, 160f, 90f) });
                    return host;
                });
            }
        }
    }
}
#endif
