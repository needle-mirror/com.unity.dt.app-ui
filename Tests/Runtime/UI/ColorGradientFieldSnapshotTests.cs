#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using System;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ColorGradientField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ColorGradientFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ColorGradientField))]
    class ColorGradientFieldSnapshotTests : UISnapshotTests<ColorGradientField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new ColorGradientField());
                yield return new Story("Assigned", ctx => new ColorGradientField { value = RedToBlue() });
                yield return new Story("Sizes", ctx =>
                {
                    var container = new VisualElement();
                    foreach (var (size, _) in k_Sizes)
                        container.Add(new ColorGradientField { size = size, value = RedToBlue() });
                    return container;
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


        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };
    }
}
#endif
