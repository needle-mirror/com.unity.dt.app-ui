using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BottomNavBar))]
    class BottomNavBarTests : VisualElementTests<BottomNavBar>
    {
        protected override string mainUssClassName => BottomNavBar.ussClassName;

        // Handed over to BottomNavBarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => false;
    }
}
