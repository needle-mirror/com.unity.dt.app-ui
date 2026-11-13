#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using FloatField = Unity.AppUI.UI.FloatField;
using IntField = Unity.AppUI.UI.IntField;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="NumericPropertyFloat"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="NumericPropertyFloatTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(NumericPropertyFloat))]
    class NumericPropertyFloatSnapshotTests : UISnapshotTests<NumericPropertyFloat>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new NumericPropertyFloat
                {
                    label = "Opacity",
                    value = 0.5f,
                    lowValue = 0f,
                    highValue = 1f
                });
            }
        }
    }
}
#endif
