using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ExVisualElement))]
    class ExVisualElementTests : VisualElementTests<ExVisualElement>
    {
        protected override string mainUssClassName => null;

        // Handed over to ExVisualElementSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // The base class every other component's extra render passes come from. On its own it has
        // no outline, no shadow and no content, so it paints nothing worth capturing; the passes
        // are covered by the components that configure them.
        protected override bool hasDefaultStory => false;

        [Test]
        public void OptedOut_DeclaresNoStory()
        {
            CollectionAssert.IsEmpty(stories,
                "A bare ExVisualElement paints nothing, so it must contribute no capture rather "
                + "than a blank one.");
        }
    }
}
