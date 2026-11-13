#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="RadioGroup"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="RadioGroupTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(RadioGroup))]
    class RadioGroupSnapshotTests : UISnapshotTests<RadioGroup>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var group = new RadioGroup { value = "urp" };
                    group.Add(new Radio { key = "urp", label = "Universal" });
                    group.Add(new Radio { key = "hdrp", label = "High Definition" });
                    group.Add(new Radio { key = "builtin", label = "Built-in" });
                    return group;
                });
            }
        }
    }
}
#endif
