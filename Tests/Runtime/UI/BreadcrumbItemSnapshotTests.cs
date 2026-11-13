#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="BreadcrumbItem"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BreadcrumbItemTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(BreadcrumbItem))]
    class BreadcrumbItemSnapshotTests : UISnapshotTests<BreadcrumbItem>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new BreadcrumbItem { text = "Library" });
                yield return new Story("Current", (ctx) => new BreadcrumbItem { text = "MyAsset", isCurrent = true });
            }
        }
    }
}
#endif
