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
    /// Snapshot stories for <see cref="AnimationCurveField"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="AnimationCurveFieldTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(AnimationCurveField))]
    class AnimationCurveFieldSnapshotTests : UISnapshotTests<AnimationCurveField>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new AnimationCurveField());
                yield return new Story("EaseInOut", ctx => new AnimationCurveField
                {
                    value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                });
                yield return new Story("Sizes", ctx =>
                {
                    var container = new VisualElement();
                    foreach (var (size, _) in k_Sizes)
                        container.Add(new AnimationCurveField
                        {
                            size = size,
                            value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                        });
                    return container;
                });
                yield return new Story("OutOfBandValues", ctx => new AnimationCurveField
                {
                    value = AnimationCurve.Linear(0f, 0f, 1f, 90f),
                });
            }
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
