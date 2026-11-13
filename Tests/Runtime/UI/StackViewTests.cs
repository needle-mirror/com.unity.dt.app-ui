using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(StackView))]
    class StackViewTests : VisualElementTests<StackView>
    {

        // An empty StackView lays out to nothing. The pushed item is the component.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var stackView = new StackView { style = { width = 240, height = 140 } };
                    stackView.Push(new Text("Top of the stack"), null, StackViewOperation.Immediate);
                    return stackView;
                });
            }
        }

        protected override string mainUssClassName => StackView.ussClassName;

        // Handed over to StackViewSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
