using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Spacer))]
    class SpacerTests : VisualElementTests<Spacer>
    {
        protected override string mainUssClassName => Spacer.ussClassName;

        // Handed over to SpacerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // A Spacer is flex space. It paints nothing, in any theme, density or direction, so a
        // capture of one is a file that can never fail.
        protected override bool hasDefaultStory => false;

        [Test]
        public void OptedOut_DeclaresNoStory()
        {
            CollectionAssert.IsEmpty(stories,
                "A Spacer paints nothing, so it must contribute no capture rather than a blank one.");
        }
    }
}
