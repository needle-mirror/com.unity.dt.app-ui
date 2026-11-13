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
    /// Snapshot stories for <see cref="Skeleton"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SkeletonTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Skeleton))]
    class SkeletonSnapshotTests : UISnapshotTests<Skeleton>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Skeleton { style = { width = 160 } });
                yield return new Story("Static", (ctx) => new Skeleton { animated = false, style = { width = 160 } });
                yield return new Story("Stack", (ctx) =>
                {
                    var col = new VisualElement { style = { width = 200 } };
                    col.Add(new Skeleton { style = { height = 12, marginBottom = 6 } });
                    col.Add(new Skeleton { style = { height = 12, marginBottom = 6, width = Length.Percent(80) } });
                    col.Add(new Skeleton { style = { height = 12, width = Length.Percent(60) } });
                    return col;
                });
            }
        }
    }
}
#endif
