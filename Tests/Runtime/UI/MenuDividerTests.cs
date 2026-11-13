using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuDivider))]
    class MenuDividerTests : VisualElementTests<MenuDivider>
    {
        protected override string mainUssClassName => Divider.ussClassName;

        // Handed over to MenuDividerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
