using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Chip))]
    class ChipTests : VisualElementTests<Chip>
    {
        protected override string mainUssClassName => Chip.ussClassName;

        // Handed over to ChipSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Chip />",
            @"<appui:Chip label=""Chip"" variant=""Outlined"" delete-icon=""x"" deletable=""true"" />",
        };
    }
}
