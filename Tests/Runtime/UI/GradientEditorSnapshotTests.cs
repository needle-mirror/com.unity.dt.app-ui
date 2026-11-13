#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="GradientEditor"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="GradientEditorTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(GradientEditor))]
    class GradientEditorSnapshotTests : UISnapshotTests<GradientEditor>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new GradientEditor());
                yield return new Story("Assigned", ctx => new GradientEditor { value = ThreeStops() });
                yield return new Story("AlphaSelected", ctx =>
                {
                    var editor = new GradientEditor { value = RedToBlue() };
                    editor.SelectKey(GradientEditor.KeyKind.Alpha, 1);
                    return editor;
                });
            }
        }


        static Gradient RedToBlue()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }


        static Gradient ThreeStops()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.black, 0f),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.white, 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
#endif
