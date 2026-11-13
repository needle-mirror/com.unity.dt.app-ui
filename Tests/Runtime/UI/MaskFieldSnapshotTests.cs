#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using MaskField = Unity.AppUI.UI.MaskField;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="MaskField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="MaskFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(MaskField))]
    class MaskFieldSnapshotTests : UISnapshotTests<MaskField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new MaskField(
                    Enum.GetValues(typeof(EnumMaskForTest)),
                    (int)(EnumMaskForTest.Value1 | EnumMaskForTest.Value2)));
            }
        }
    }
}
#endif
