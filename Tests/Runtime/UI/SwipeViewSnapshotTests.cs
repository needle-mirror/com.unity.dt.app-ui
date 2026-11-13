#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="SwipeView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SwipeViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(SwipeView))]
    class SwipeViewSnapshotTests : UISnapshotTests<SwipeView>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var view = new SwipeView { style = { width = 240, height = 120 } };
                    for (var i = 1; i <= 3; i++)
                    {
                        var item = new SwipeViewItem();
                        item.Add(new Text($"Item {i}"));
                        view.Add(item);
                    }
                    return view;
                });
            }
        }
    }
}
#endif
