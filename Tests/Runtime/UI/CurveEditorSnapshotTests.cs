#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="CurveEditor"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="CurveEditorTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(CurveEditor))]
    class CurveEditorSnapshotTests : UISnapshotTests<CurveEditor>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new CurveEditor());
                yield return new Story("Assigned", ctx => new CurveEditor { value = ThreeKeys() });
                yield return new Story("KeySelected", ctx =>
                {
                    var editor = new CurveEditor { value = ThreeKeys() };
                    editor.SelectKey(1);
                    return editor;
                });
            }
        }


        static AnimationCurve ThreeKeys()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f));
        }
    }
}
#endif
