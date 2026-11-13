#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="SVSquare"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="SVSquareTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(SVSquare))]
    class SVSquareSnapshotTests : UISnapshotTests<SVSquare>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => Sized());
                yield return new Story("Disabled", (ctx) =>
                {
                    var square = Sized();
                    square.SetEnabled(false);
                    return square;
                });
            }
        }


        static SVSquare Sized() => new SVSquare { style = { width = 160, height = 160 } };
    }
}
#endif
