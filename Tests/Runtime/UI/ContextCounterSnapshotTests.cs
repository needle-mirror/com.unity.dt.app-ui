#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ContextCounter"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ContextCounterTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ContextCounter))]
    class ContextCounterSnapshotTests : UISnapshotTests<ContextCounter>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Partial", (ctx) => new ContextCounter { used = 120000, total = 200000 });
                yield return new Story("NearlyFull", (ctx) => new ContextCounter { used = 190000, total = 200000 });
            }
        }
    }
}
#endif
