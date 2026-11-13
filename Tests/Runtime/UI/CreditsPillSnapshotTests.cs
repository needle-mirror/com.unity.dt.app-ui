#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CreditsPill"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CreditsPillTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CreditsPill))]
    class CreditsPillSnapshotTests : UISnapshotTests<CreditsPill>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Resolved", (ctx) => new CreditsPill { credits = 51200 });
                yield return new Story("Loading", (ctx) => new CreditsPill { loading = true });
                yield return new Story("Error", (ctx) => new CreditsPill { error = true });
            }
        }
    }
}
#endif
