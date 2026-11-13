#if APPUI_UITF_PINNED_PANEL
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CodeBlock"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CodeBlockTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CodeBlock))]
    class CodeBlockSnapshotTests : UISnapshotTests<CodeBlock>
    {
    }
}
#endif
