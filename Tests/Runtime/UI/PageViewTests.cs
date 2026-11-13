using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PageView))]
    class PageViewTests : VisualElementTests<PageView>
    {

        // An empty PageView lays out to nothing, and it has no intrinsic size either.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var view = new PageView { style = { width = 240, height = 140 } };
                    for (var i = 1; i <= 3; i++)
                    {
                        var item = new SwipeViewItem();
                        item.Add(new Text($"Page {i}"));
                        view.Add(item);
                    }
                    return view;
                });
            }
        }

        protected override string mainUssClassName => PageView.ussClassName;

        // Handed over to PageViewSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
