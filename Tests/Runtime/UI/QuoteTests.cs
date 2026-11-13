using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Quote))]
    class QuoteTests : VisualElementTests<Quote>
    {
        protected override string mainUssClassName => Quote.ussClassName;

        // Handed over to QuoteSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
