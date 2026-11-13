using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DrawerHeader))]
    class DrawerHeaderTests : VisualElementTests<DrawerHeader>
    {
        protected override string mainUssClassName => DrawerHeader.ussClassName;

        // Handed over to DrawerHeaderSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => false;
    }
}
