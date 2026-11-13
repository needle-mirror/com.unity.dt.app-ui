using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuTrigger))]
    class MenuTriggerTests : VisualElementTests<MenuTrigger>
    {
        // A MenuTrigger is an anchor and a handler, not a visual: at rest there is only the child
        // it wraps, and the menu it shows is captured by the Menu fixtures.
        protected override bool hasDefaultStory => false;

        protected override string mainUssClassName => null;

        // Handed over to MenuTriggerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
