#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="MenuRadioGroup"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MenuRadioGroupTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(MenuRadioGroup))]
    class MenuRadioGroupSnapshotTests : UISnapshotTests<MenuRadioGroup>
    {
        protected override IEnumerable<Story> stories
        {
            get { yield return new Story("Default", ctx => MakeGroup()); }
        }


        static MenuRadioGroup MakeGroup(string value = "md")
        {
            var group = new MenuRadioGroup { value = value };
            group.Add(new MenuItem { key = "sm", label = "Small" });
            group.Add(new MenuItem { key = "md", label = "Medium" });
            group.Add(new MenuItem { key = "lg", label = "Large" });
            return group;
        }
    }
}
#endif
