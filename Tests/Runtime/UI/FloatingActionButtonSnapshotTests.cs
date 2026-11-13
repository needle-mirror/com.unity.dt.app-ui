#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="FloatingActionButton"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="FloatingActionButtonTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(FloatingActionButton))]
    class FloatingActionButtonSnapshotTests : UISnapshotTests<FloatingActionButton>
    {
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
    }
}
#endif
