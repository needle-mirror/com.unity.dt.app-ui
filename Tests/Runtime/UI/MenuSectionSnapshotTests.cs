#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="MenuSection"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MenuSectionTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(MenuSection))]
    class MenuSectionSnapshotTests : UISnapshotTests<MenuSection>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var section = new MenuSection { title = "Recent" };
                    section.Add(new MenuItem { label = "Scene.unity" });
                    section.Add(new MenuItem { label = "Player.prefab" });
                    return section;
                });
            }
        }
    }
}
#endif
