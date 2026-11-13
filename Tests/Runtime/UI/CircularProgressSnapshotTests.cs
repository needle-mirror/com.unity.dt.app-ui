#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CircularProgress"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CircularProgressTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CircularProgress))]
    class CircularProgressSnapshotTests : UISnapshotTests<CircularProgress>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new CircularProgress());
                yield return new Story("Determinate", ctx => new CircularProgress
                {
                    variant = Progress.Variant.Determinate,
                    bufferValue = 0.75f,
                    value = 0.5f,
                });
                yield return new Story("DeterminateNoRoundedCorners", ctx => new CircularProgress
                {
                    variant = Progress.Variant.Determinate,
                    bufferValue = 0.75f,
                    value = 0.5f,
                    roundedProgressCorners = false,
                });
            }
        }
    }
}
#endif
