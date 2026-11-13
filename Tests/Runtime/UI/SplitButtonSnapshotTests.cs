#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="SplitButton"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SplitButtonTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(SplitButton))]
    class SplitButtonSnapshotTests : UISnapshotTests<SplitButton>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new SplitButton { label = "Play" });
                yield return new Story("WithIcon", (ctx) => new SplitButton { label = "Play", icon = "play" });
                yield return new Story("IconOnly", (ctx) => new SplitButton { icon = "play" });
                yield return new Story("Selected", (ctx) => new SplitButton { label = "Mute", selected = true });
                yield return new Story("Small", (ctx) => new SplitButton { label = "Play", size = Size.Sm });
            }
        }
    }
}
#endif
