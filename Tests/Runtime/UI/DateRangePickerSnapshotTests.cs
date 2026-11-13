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
    /// Snapshot stories for <see cref="DateRangePicker"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="DateRangePickerTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(DateRangePicker))]
    class DateRangePickerSnapshotTests : UISnapshotTests<DateRangePicker>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DateRangePicker
                {
                    value = new DateRange(new Date(2020, 1, 1), new Date(2020, 1, 2))
                });
                yield return new Story("DifferentMonths", (ctx) => new DateRangePicker
                {
                    value = new DateRange(new Date(2020, 1, 1), new Date(2020, 2, 2))
                });
            }
        }
    }
}
#endif
