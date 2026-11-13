using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DropZone))]
    class DropZoneTests : VisualElementTests<DropZone>
    {
        protected override string mainUssClassName => DropZone.ussClassName;

        // Handed over to DropZoneSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
