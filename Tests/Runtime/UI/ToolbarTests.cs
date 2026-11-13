using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toolbar))]
    class ToolbarTests : VisualElementTests<Toolbar>
    {
        protected override string mainUssClassName => Toolbar.ussClassName;

        // Handed over to ToolbarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
