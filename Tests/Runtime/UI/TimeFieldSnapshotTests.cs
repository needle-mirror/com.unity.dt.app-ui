#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="TimeField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TimeFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(TimeField))]
    class TimeFieldSnapshotTests : UISnapshotTests<TimeField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TimeField
                {
                    value = new TimeSpan(9, 30, 0)
                });
                yield return new Story("12-hour with seconds", (ctx) => new TimeField
                {
                    hour12 = true,
                    showSeconds = true,
                    value = new TimeSpan(14, 5, 30)
                });
            }
        }
    }
}
#endif
