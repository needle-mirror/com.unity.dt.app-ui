#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="HelpText"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="HelpTextTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(HelpText))]
    class HelpTextSnapshotTests : UISnapshotTests<HelpText>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new HelpText("Pick the pipeline this project targets."));
            }
        }
    }
}
#endif
