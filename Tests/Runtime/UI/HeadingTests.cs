using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Heading))]
    class HeadingTests : VisualElementTests<Heading>
    {
        protected override string mainUssClassName => Heading.ussClassName;

        // Handed over to HeadingSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Heading { text = "Heading" });
                yield return new Story("Disabled", (ctx) =>
                {
                    var heading = new Heading { text = "Heading" };
                    heading.SetEnabled(false);
                    return heading;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Heading text=""Heading"" />",
            @"<appui:Heading text=""Heading"" enabled=""false"" />",
        };
    }
}
