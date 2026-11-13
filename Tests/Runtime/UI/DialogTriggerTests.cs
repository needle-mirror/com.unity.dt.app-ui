using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DialogTrigger))]
    class DialogTriggerTests : VisualElementTests<DialogTrigger>
    {
        // A DialogTrigger is an anchor and a handler, not a visual: at rest there is only the
        // child it wraps, and the dialog it shows is captured by the dialog fixtures.
        protected override bool hasDefaultStory => false;

        protected override string mainUssClassName => null;

        // Handed over to DialogTriggerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
