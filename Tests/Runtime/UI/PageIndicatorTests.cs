using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PageIndicator))]
    class PageIndicatorTests : VisualElementTests<PageIndicator>
    {

        // A PageIndicator with no pages lays out to nothing.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new PageIndicator { count = 5, value = 2 });
            }
        }

        protected override string mainUssClassName => PageIndicator.ussClassName;

        // Handed over to PageIndicatorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
