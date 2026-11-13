#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="LinearProgress"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="LinearProgressTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(LinearProgress))]
    class LinearProgressSnapshotTests : UISnapshotTests<LinearProgress>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new LinearProgress());
                yield return new Story("Determinate", ctx => new LinearProgress
                {
                    variant = Progress.Variant.Determinate,
                    bufferValue = 0.75f,
                    value = 0.5f,
                });
                yield return new Story("DeterminateNoRoundedCorners", ctx => new LinearProgress
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
