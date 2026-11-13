#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System;
using EnumField = Unity.AppUI.UI.EnumField;
using NUnit.Framework;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="EnumField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="EnumFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(EnumField))]
    class EnumFieldSnapshotTests : UISnapshotTests<EnumField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new EnumField(EnumForTest.Value1));
            }
        }
    }
}
#endif
