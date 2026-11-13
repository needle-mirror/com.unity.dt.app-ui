#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Breadcrumbs"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BreadcrumbsTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Breadcrumbs))]
    class BreadcrumbsSnapshotTests : UISnapshotTests<Breadcrumbs>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildTrail());
            }
        }


        static Breadcrumbs BuildTrail()
        {
            var breadcrumbs = new Breadcrumbs();
            breadcrumbs.Add(new BreadcrumbItem { text = "Home" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "Library" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "Assets" });
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(new BreadcrumbItem { text = "MyAsset", isCurrent = true });
            return breadcrumbs;
        }
    }
}
#endif
