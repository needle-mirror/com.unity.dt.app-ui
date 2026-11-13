#if APPUI_UITF_PINNED_PANEL
using NUnit.Framework;
using Unity.AppUI.Navigation;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="NavHost"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="NavHostTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(NavHost))]
    class NavHostSnapshotTests : UISnapshotTests<NavHost>
    {
        protected override bool hasDefaultStory => false;
    }
}
#endif
