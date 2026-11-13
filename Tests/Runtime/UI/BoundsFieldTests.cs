using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsField))]
    class BoundsFieldTests : VisualElementTests<BoundsField>
    {
        protected override string mainUssClassName => BoundsField.ussClassName;

        // Handed over to BoundsFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:BoundsField />",
            @"<appui:BoundsField value=""0,0,100,100"" size=""Md"" invalid=""false"" />"
        };
    }
}
