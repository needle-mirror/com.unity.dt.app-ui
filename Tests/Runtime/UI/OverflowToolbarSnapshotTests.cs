#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="OverflowToolbar"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="OverflowToolbarTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(OverflowToolbar))]
    class OverflowToolbarSnapshotTests : UISnapshotTests<OverflowToolbar>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Roomy", (ctx) => Build(6, 600f));
                yield return new Story("Collapsed", (ctx) => Build(6, 160f));
            }
        }


        static OverflowToolbar Build(int items, float width)
        {
            var bar = new OverflowToolbar { style = { width = width } };
            AddItems(bar, items);
            return bar;
        }


        static List<VisualElement> AddItems(OverflowToolbar bar, int count, float itemWidth = 60f)
        {
            var items = new List<VisualElement>();
            for (var i = 0; i < count; i++)
            {
                var item = new VisualElement { name = "item" + i, style = { width = itemWidth, height = 20 } };
                bar.Add(item);
                items.Add(item);
            }
            return items;
        }
    }
}
#endif
