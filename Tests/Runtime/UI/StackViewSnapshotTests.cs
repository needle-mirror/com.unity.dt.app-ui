#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="StackView"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="StackViewTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(StackView))]
    class StackViewSnapshotTests : UISnapshotTests<StackView>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var stackView = new StackView { style = { width = 240, height = 140 } };
                    stackView.Push(new Text("Top of the stack"), null, StackViewOperation.Immediate);
                    return stackView;
                });
            }
        }
    }
}
#endif
