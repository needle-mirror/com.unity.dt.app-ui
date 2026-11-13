#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ColorWheel"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ColorWheelTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ColorWheel))]
    class ColorWheelSnapshotTests : UISnapshotTests<ColorWheel>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => Sized());
                yield return new Story("Disabled", (ctx) =>
                {
                    var wheel = Sized();
                    wheel.SetEnabled(false);
                    return wheel;
                });
            }
        }


        static ColorWheel Sized() => new ColorWheel { style = { width = 160, height = 160 } };
    }
}
#endif
