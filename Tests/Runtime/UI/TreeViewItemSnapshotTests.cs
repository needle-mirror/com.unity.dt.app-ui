#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="TreeViewItem"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TreeViewItemTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(TreeViewItem))]
    class TreeViewItemSnapshotTests : UISnapshotTests<TreeViewItem>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TreeViewItem { label = "Item" });
                yield return new Story("WithIcon", (ctx) => new TreeViewItem { label = "Item", icon = "folder" });
                yield return new Story("CaretEnd", (ctx) => new TreeViewItem
                {
                    label = "Item",
                    showCaret = true,
                    caretPosition = CaretPosition.End
                });
            }
        }
    }
}
#endif
