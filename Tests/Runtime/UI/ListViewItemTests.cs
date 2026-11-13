using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ListViewItem))]
    class ListViewItemTests : VisualElementTests<ListViewItem>
    {
        protected override string mainUssClassName => ListViewItem.ussClassName;

        // Handed over to ListViewItemSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => false;
    }
}
