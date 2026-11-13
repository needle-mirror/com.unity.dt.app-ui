#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Menu"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MenuTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Menu))]
    class MenuSnapshotTests : UISnapshotTests<Menu>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var menu = new Menu();
                    menu.Add(new MenuItem { label = "Open" });
                    menu.Add(new MenuItem { label = "Duplicate" });
                    menu.Add(new MenuDivider());
                    menu.Add(new MenuItem { label = "Delete" });
                    return menu;
                });
            }
        }
    }
}
#endif
