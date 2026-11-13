#if APPUI_UITF_PINNED_PANEL
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="BottomNavBar"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BottomNavBarTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(BottomNavBar))]
    class BottomNavBarSnapshotTests : UISnapshotTests<BottomNavBar>
    {
    }
}
#endif
