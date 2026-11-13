#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Mask"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MaskTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Mask))]
    class MaskSnapshotTests : UISnapshotTests<Mask>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                // maskRect is in pixels unless useNormalizedMaskRect says otherwise, so the
                // fractions below would carve a half-pixel hole out of a 200x120 mask and the
                // capture would be a solid rectangle.
                yield return new Story("Default", (ctx) => new Mask
                {
                    style = { width = 200, height = 120 },
                    useNormalizedMaskRect = true,
                    maskRect = new Rect(0.25f, 0.25f, 0.5f, 0.5f),
                    radius = 8f,
                });
            }
        }
    }
}
#endif
