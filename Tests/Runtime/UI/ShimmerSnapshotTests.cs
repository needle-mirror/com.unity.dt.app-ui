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
    /// Snapshot stories for <see cref="Shimmer"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ShimmerTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Shimmer))]
    class ShimmerSnapshotTests : UISnapshotTests<Shimmer>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Shimmer("Thinking"));
                yield return new Story("Static", (ctx) => new Shimmer("Thinking") { active = false });
            }
        }
    }
}
#endif
