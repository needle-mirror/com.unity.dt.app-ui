#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="SplitView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SplitViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(SplitView))]
    class SplitViewSnapshotTests : UISnapshotTests<SplitView>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story($"Default-{m_Direction}", (ctx) =>
                {
                    var splitView = new SplitView(m_Direction) { style = { width = 320, height = 200 } };
                    splitView.AddPane(NamedPane("One"));
                    splitView.AddPane(NamedPane("Two"));
                    return splitView;
                });
            }
        }


        static Pane NamedPane(string title)
        {
            var pane = new Pane();
            pane.Add(new Text(title));
            return pane;
        }


        readonly Direction m_Direction;
    }
}
#endif
