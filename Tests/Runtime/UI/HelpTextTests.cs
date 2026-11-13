using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(HelpText))]
    class HelpTextTests : VisualElementTests<HelpText>
    {

        // A textless HelpText lays out to nothing.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new HelpText("Pick the pipeline this project targets."));
            }
        }

        protected override string mainUssClassName => HelpText.ussClassName;

        // Handed over to HelpTextSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
