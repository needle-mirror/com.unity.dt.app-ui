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
    /// Snapshot stories for <see cref="ClampedContent"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ClampedContentTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ClampedContent))]
    class ClampedContentSnapshotTests : UISnapshotTests<ClampedContent>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Overflowing", (ctx) => Build(100f, 400f));
                yield return new Story("Fitting", (ctx) => Build(200f, 60f));
            }
        }


        static ClampedContent Build(float maxHeight, float contentHeight)
        {
            var clamped = new ClampedContent { maxHeight = maxHeight, style = { width = 320 } };
            clamped.Add(new VisualElement { style = { height = contentHeight } });
            return clamped;
        }
    }
}
#endif
