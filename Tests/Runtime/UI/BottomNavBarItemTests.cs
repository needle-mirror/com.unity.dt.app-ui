using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BottomNavBarItem))]
    class BottomNavBarItemTests : VisualElementTests<BottomNavBarItem>
    {
        protected override string mainUssClassName => BottomNavBarItem.ussClassName;

        // Handed over to BottomNavBarItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => false;
    }
}
