#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Badge"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="BadgeTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Badge))]
    class BadgeSnapshotTests : UISnapshotTests<Badge>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new Badge
                {
                    style = { width = 100, height = 100 }
                });
                yield return new Story("Blue", ctx => new Badge
                {
                    backgroundColor = Color.blue,
                    variant = BadgeVariant.Dot,
                    overlapType = BadgeOverlapType.Rectangular,
                    verticalAnchor = VerticalAnchor.Top,
                    horizontalAnchor = HorizontalAnchor.Right,
                    style = { width = 100, height = 100 }
                });
            }
        }
    }
}
#endif
