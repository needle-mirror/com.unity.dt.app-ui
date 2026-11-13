#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="BreadcrumbSeparator"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BreadcrumbSeparatorTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(BreadcrumbSeparator))]
    class BreadcrumbSeparatorSnapshotTests : UISnapshotTests<BreadcrumbSeparator>
    {
    }
}
#endif
