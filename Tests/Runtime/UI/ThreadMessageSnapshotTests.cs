#if APPUI_UITF_PINNED_PANEL
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ThreadMessage"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ThreadMessageTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ThreadMessage))]
    class ThreadMessageSnapshotTests : UISnapshotTests<ThreadMessage>
    {
    }
}
#endif
