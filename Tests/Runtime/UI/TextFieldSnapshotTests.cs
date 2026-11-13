#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TextField = Unity.AppUI.UI.TextField;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="TextField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TextFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(TextField))]
    class TextFieldSnapshotTests : UISnapshotTests<TextField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TextField());
                yield return new Story("Sizes", (ctx) =>
                {
                    var container = new VisualElement();
                    foreach (var size in new[] { Size.Sm, Size.Md, Size.Lg })
                        container.Add(new TextField { size = size, placeholder = "Enter a text..." });
                    return container;
                });
                yield return new Story("Suffix", (ctx) => new TextField("128") { suffix = "px" });
                yield return new Story("Adorned", (ctx) => new TextField
                {
                    leadingIconName = "magnifying-glass",
                    trailingIconName = "x",
                    placeholder = "Search...",
                });
                yield return new Story("Invalid", (ctx) => new TextField("nope")
                {
                    validateValue = v => !string.IsNullOrEmpty(v) && v.Contains("@"),
                });
            }
        }
    }
}
#endif
