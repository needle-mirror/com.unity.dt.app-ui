using NUnit.Framework;
using Unity.AppUI.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Mask))]
    class MaskTests : VisualElementTests<Mask>
    {

        // Mask paints a hole into whatever box it is given; it has no intrinsic size.
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

        protected override string mainUssClassName => Mask.ussClassName;

        // Handed over to MaskSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;
    }
}
