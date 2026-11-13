#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="FieldLabel"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="FieldLabelTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(FieldLabel))]
    class FieldLabelSnapshotTests : UISnapshotTests<FieldLabel>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new FieldLabel("Label"));
                yield return new Story("Required", (ctx) => new FieldLabel("Label") { required = true });
            }
        }
    }
}
#endif
