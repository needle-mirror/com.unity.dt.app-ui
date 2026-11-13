#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="FeedbackTray"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="FeedbackTrayTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(FeedbackTray))]
    class FeedbackTraySnapshotTests : UISnapshotTests<FeedbackTray>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new FeedbackTray());
                yield return new Story("Good", ctx => new FeedbackTray { value = FeedbackValue.Good });
                yield return new Story("Poor", ctx => new FeedbackTray { value = FeedbackValue.Poor });
                yield return new Story("WithActions", ctx => new FeedbackTray
                {
                    canRefresh = true,
                    canCopy = true,
                });
                yield return new Story("Copied", ctx => new FeedbackTray { canCopy = true, copied = true });
            }
        }
    }
}
#endif
