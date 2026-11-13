#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="PageView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="PageViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(PageView))]
    class PageViewSnapshotTests : UISnapshotTests<PageView>
    {
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
    }
}
#endif
