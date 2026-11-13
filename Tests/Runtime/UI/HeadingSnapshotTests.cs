#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Heading"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="HeadingTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Heading))]
    class HeadingSnapshotTests : UISnapshotTests<Heading>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Heading { text = "Heading" });
                yield return new Story("Disabled", (ctx) =>
                {
                    var heading = new Heading { text = "Heading" };
                    heading.SetEnabled(false);
                    return heading;
                });
            }
        }
    }
}
#endif
