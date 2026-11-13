using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(NavigationRail))]
    class NavigationRailTests : VisualElementTests<NavigationRail>
    {
        protected override string mainUssClassName => NavigationRail.ussClassName;

        // Handed over to NavigationRailSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

    }

    [TestFixture]
    [TestOf(typeof(NavigationRailItem))]
    class NavigationRailItemTests : VisualElementTests<NavigationRailItem>
    {
        // Handed over to NavigationRailItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => NavigationRailItem.ussClassName;

    }
}
