#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="MasonryGridView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MasonryGridViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(MasonryGridView))]
    class MasonryGridViewSnapshotTests : UISnapshotTests<MasonryGridView>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new MasonryGridView
                {
                    style =
                    {
                        height = 400,
                    },
                    makeItem = () => new Text(),
                    bindItem = (e, i) =>
                    {
                        var length = Mathf.Min(i * 10, Utils.loremIpsum.Length - 10);
                        var loremIpsumPart = Utils.loremIpsum.Substring(10, length);
                        ((Text)e).text = loremIpsumPart;
                    },
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });

                yield return new Story("ThreeColumns", _ => new MasonryGridView
                {
                    style =
                    {
                        height = 400,
                    },
                    columnCount = 3,
                    makeItem = () => new Text(),
                    bindItem = (e, i) =>
                    {
                        var length = Mathf.Min(i * 10, Utils.loremIpsum.Length - 10);
                        var loremIpsumPart = Utils.loremIpsum.Substring(10, length);
                        ((Text)e).text = loremIpsumPart;
                    },
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });
            }
        }
    }
}
#endif
