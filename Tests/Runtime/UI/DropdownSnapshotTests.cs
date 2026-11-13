#if APPUI_UITF_PINNED_PANEL
using System.Collections;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Dropdown"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="DropdownTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Dropdown))]
    class DropdownSnapshotTests : UISnapshotTests<Dropdown>
    {
    }
}
#endif
