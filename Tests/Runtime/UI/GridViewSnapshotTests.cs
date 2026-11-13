#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using Unity.Properties;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="GridView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="GridViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(GridView))]
    class GridViewSnapshotTests : UISnapshotTests<GridView>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new GridView
                {
                    style =
                    {
                        height = 400,
                    },
                    makeItem = () => new Text(),
                    bindItem = (e, i) => ((Text)e).text = $"Item {i}",
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });

                yield return new Story("ThreeColumns", _ => new GridView
                {
                    style =
                    {
                        height = 400,
                    },
                    columnCount = 3,
                    makeItem = () => new Text(),
                    bindItem = (e, i) => ((Text)e).text = $"Item {i}",
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });
            }
        }
    }
}
#endif
