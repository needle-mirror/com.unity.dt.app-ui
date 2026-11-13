#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="BoundsIntField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BoundsIntFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(BoundsIntField))]
    class BoundsIntFieldSnapshotTests : UISnapshotTests<BoundsIntField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new BoundsIntField());
            }
        }
    }
}
#endif
