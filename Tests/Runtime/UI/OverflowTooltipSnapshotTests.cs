#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="OverflowTooltip"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="OverflowTooltipTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(OverflowTooltip))]
    class OverflowTooltipSnapshotTests : UISnapshotTests<OverflowTooltip>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new OverflowTooltip { text = "Fits" });
                yield return new Story("Truncated", ctx =>
                {
                    var host = new VisualElement { style = { width = 120 } };
                    host.Add(new OverflowTooltip { text = k_Long });
                    return host;
                });
            }
        }


        const string k_Long = "A very long asset name that will certainly not fit in a narrow column";
    }
}
#endif
