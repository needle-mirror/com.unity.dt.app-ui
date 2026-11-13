using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Stepper))]
    class StepperTests : VisualElementTests<Stepper>
    {
        protected override string mainUssClassName => Stepper.ussClassName;

        // Handed over to StepperSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
