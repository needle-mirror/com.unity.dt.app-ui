#if APPUI_UITF_PINNED_PANEL
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Spacer"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SpacerTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Spacer))]
    class SpacerSnapshotTests : UISnapshotTests<Spacer>
    {
        protected override bool hasDefaultStory => false;
    }
}
#endif
