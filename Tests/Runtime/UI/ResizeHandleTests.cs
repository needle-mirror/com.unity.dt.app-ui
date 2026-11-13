using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ResizeHandle))]
    class ResizeHandleTests : VisualElementTests<ResizeHandle>
    {
        protected override string mainUssClassName => ResizeHandle.ussClassName;

        // Handed over to ResizeHandleSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
