using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorToolbar))]
    class ColorToolbarTests : VisualElementTests<ColorToolbar>
    {
        protected override string mainUssClassName => ColorToolbar.ussClassName;

        // Handed over to ColorToolbarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => false;
    }
}
