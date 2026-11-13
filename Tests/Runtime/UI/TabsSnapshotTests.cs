#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Tabs"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TabsTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Tabs))]
    class TabsSnapshotTests : UISnapshotTests<Tabs>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                var sourceItems = new List<string>();
                for (var i = 0; i < 2; ++i)
                {
                    sourceItems.Add($"Tab {i}");
                }
                yield return new Story("Default", (ctx) => new Tabs
                {
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
                yield return new Story("Justified", (ctx) => new Tabs
                {
                    justified = true,
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
                yield return new Story("Vertical", (ctx) => new Tabs
                {
                    direction = Direction.Vertical,
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
            }
        }
    }
}
#endif
