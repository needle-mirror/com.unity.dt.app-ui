#if APPUI_UITF_PINNED_PANEL
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Drawer"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="DrawerTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Drawer))]
    class DrawerSnapshotTests : UISnapshotTests<Drawer>
    {
    }
}
#endif
