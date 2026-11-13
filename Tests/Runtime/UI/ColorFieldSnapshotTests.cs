#if APPUI_UITF_PINNED_PANEL
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ColorField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ColorFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldSnapshotTests : UISnapshotTests<ColorField>
    {
    }
}
#endif
