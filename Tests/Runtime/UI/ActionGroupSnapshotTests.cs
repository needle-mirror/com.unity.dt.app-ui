#if APPUI_UITF_PINNED_PANEL
using System.Collections;
using System.Linq;
using System;
using NUnit.Framework;
using Object = UnityEngine.Object;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ActionGroup"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ActionGroupTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ActionGroup))]
    class ActionGroupSnapshotTests : UISnapshotTests<ActionGroup>
    {
    }
}
#endif
