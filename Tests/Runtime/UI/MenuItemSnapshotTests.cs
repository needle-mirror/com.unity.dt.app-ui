#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="MenuItem"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MenuItemTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(MenuItem))]
    class MenuItemSnapshotTests : UISnapshotTests<MenuItem>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new MenuItem { label = "Open", icon = "folder-open" });
                yield return new Story("Shortcut", ctx => new MenuItem { label = "Copy", icon = "copy", shortcut = "Ctrl+C" });
                yield return new Story("Checkbox", ctx => new MenuItem { label = "Word Wrap", selectable = true, value = true });
                yield return new Story("Radio", ctx => new MenuItem
                {
                    label = "Medium",
                    key = "md",
                    selectable = true,
                    indicator = MenuItemIndicator.Radio,
                    value = true
                });
                yield return new Story("Destructive", ctx => new MenuItem
                {
                    label = "Delete",
                    icon = "trash",
                    variant = MenuItemVariant.Destructive
                });
            }
        }
    }
}
#endif
