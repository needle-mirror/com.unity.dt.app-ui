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
    /// Snapshot stories for <see cref="LoadingDots"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="LoadingDotsTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(LoadingDots))]
    class LoadingDotsSnapshotTests : UISnapshotTests<LoadingDots>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new LoadingDots());
                yield return new Story("Static", (ctx) => new LoadingDots { animated = false });
            }
        }
    }
}
#endif
