using NUnit.Framework;
using Unity.AppUI.Navigation;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(NavHost))]
    class NavHostTests : VisualElementTests<NavHost>
    {
        // A NavHost paints nothing itself: it is an empty viewport until a navigation graph
        // swaps a destination into it, and a graph asset is not something a capture can carry.
        protected override bool hasDefaultStory => false;

        protected override string mainUssClassName => NavHost.ussClassName;

        // Handed over to NavHostSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string uxmlNamespaceName => "nav";
    }
}
