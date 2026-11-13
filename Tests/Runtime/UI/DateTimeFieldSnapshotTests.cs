#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="DateTimeField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="DateTimeFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(DateTimeField))]
    class DateTimeFieldSnapshotTests : UISnapshotTests<DateTimeField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DateTimeField
                {
                    value = new DateTime(2020, 1, 1, 9, 30, 0)
                });
                yield return new Story("12-hour with seconds", (ctx) => new DateTimeField
                {
                    hour12 = true,
                    showSeconds = true,
                    value = new DateTime(2020, 1, 1, 14, 5, 30)
                });
            }
        }
    }
}
#endif
