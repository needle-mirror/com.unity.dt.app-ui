#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="Text"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="TextTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(Text))]
    class TextSnapshotTests : UISnapshotTests<Text>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Text("The quick brown fox"));
                yield return new Story("Sizes", (ctx) =>
                {
                    var root = new VisualElement();
                    foreach (TextSize size in System.Enum.GetValues(typeof(TextSize)))
                        root.Add(new Text(size.ToString()) { size = size });
                    return root;
                });
                yield return new Story("Variants", (ctx) =>
                {
                    var root = new VisualElement();
                    foreach (TextVariant variant in System.Enum.GetValues(typeof(TextVariant)))
                        root.Add(new Text(variant.ToString()) { variant = variant });
                    return root;
                });
                yield return new Story("Disabled", (ctx) =>
                {
                    // Disabled coverage from the UUM-108227 audit.
                    var text = new Text { text = "Text" };
                    text.SetEnabled(false);
                    return text;
                });
            }
        }
    }
}
#endif
