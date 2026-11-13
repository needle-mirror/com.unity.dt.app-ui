using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FieldLabel))]
    class FieldLabelTests : VisualElementTests<FieldLabel>
    {

        // A textless FieldLabel lays out to nothing.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new FieldLabel("Label"));
                yield return new Story("Required", (ctx) => new FieldLabel("Label") { required = true });
            }
        }

        protected override string mainUssClassName => FieldLabel.ussClassName;

        // Handed over to FieldLabelSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
