#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Kbd"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="KbdTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Kbd))]
    class KbdSnapshotTests : UISnapshotTests<Kbd>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Kbd { text = "K" });
                yield return new Story("Combo", (ctx) => new Kbd { keys = new List<string> { "Ctrl", "K" } });
                yield return new Story("Row", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    row.Add(new Kbd { text = "Esc" });
                    row.Add(new Kbd { keys = new List<string> { "Shift", "Tab" } });
                    return row;
                });
            }
        }
    }
}
#endif
