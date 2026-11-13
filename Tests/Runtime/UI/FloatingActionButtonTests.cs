using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatingActionButton))]
    class FloatingActionButtonTests : VisualElementTests<FloatingActionButton>
    {
        protected override string mainUssClassName => FloatingActionButton.ussClassName;

        // Handed over to FloatingActionButtonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                // Default stays the inherited bare construction. Its reference images
                // predate this class overriding stories, and the snapshot key is the story
                // name, so giving it content would invalidate baselines it does not refresh.
                yield return new Story("Default", (ctx) => new FloatingActionButton());
                yield return new Story("Accent", (ctx) =>
                {
                    var fab = new FloatingActionButton { accent = true };
                    fab.Add(new Icon { iconName = "add" });
                    return fab;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:FloatingActionButton><appui:Icon icon-name=""add"" /></appui:FloatingActionButton>",
            @"<appui:FloatingActionButton accent=""true""><appui:Icon icon-name=""add"" /></appui:FloatingActionButton>",
        };
    }
}
