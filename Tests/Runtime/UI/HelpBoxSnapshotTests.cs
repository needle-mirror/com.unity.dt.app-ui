#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="HelpBox"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="HelpBoxTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(HelpBox))]
    class HelpBoxSnapshotTests : UISnapshotTests<HelpBox>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new HelpBox());
                yield return new Story("Information", _ => new HelpBox("Informational message", AlertSemantic.Information, "info"));
                yield return new Story("Warning", _ => new HelpBox("Warning message", AlertSemantic.Warning, "warning"));
                yield return new Story("Error", _ => new HelpBox("Error message", AlertSemantic.Error, "x-circle"));
                yield return new Story("Destructive", _ => new HelpBox("Destructive message", AlertSemantic.Destructive, "x-circle"));
                yield return new Story("Confirmation", _ => new HelpBox("Confirmation message", AlertSemantic.Confirmation, "check-circle"));
                yield return new Story("NoIcon", _ => new HelpBox("No icon", AlertSemantic.Default, null));
            }
        }
    }
}
#endif
