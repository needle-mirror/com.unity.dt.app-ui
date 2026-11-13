using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(GradientEditor))]
    class GradientEditorTests : VisualElementTests<GradientEditor>
    {
        protected override string mainUssClassName => GradientEditor.ussClassName;

        // Handed over to GradientEditorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // The editor is a picker surface, like ColorPicker: it is not authored from UXML.
        protected override bool uxmlConstructable => false;

        protected override IEnumerable<string> uxmlTestCases => null;

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

        // Mounting matters: markers are positioned from resolved geometry and pointer events only
        // dispatch through a panel.
        IEnumerator Mount(GradientEditor editor, float width = 280f)
        {
            m_TestUI.rootVisualElement.Clear();
            editor.style.width = width;
            m_TestUI.rootVisualElement.Add(editor);
            for (var i = 0; i < 3; i++)
                yield return null;
        }

        // ------------------------------------------------------------------ shape

        [Test]
        public void DefaultValue_IsATwoStopOpaqueBlend()
        {
            var editor = new GradientEditor();
            Assert.IsNotNull(editor.value);
            Assert.AreEqual(GradientMode.Blend, editor.value.mode);
            Assert.AreEqual(2, editor.value.colorKeys.Length);
            Assert.AreEqual(2, editor.value.alphaKeys.Length);
            Assert.AreEqual(1f, editor.value.alphaKeys[0].alpha);
            Assert.AreEqual(1f, editor.value.alphaKeys[1].alpha);
        }

        /// <summary>
        /// The strip must be the shared <see cref="ColorSwatch"/>, the same render path
        /// <see cref="ColorGradientField"/>'s preview uses. A second path would drift on
        /// <see cref="GradientMode.Fixed"/> and on the transparency checkerboard.
        /// </summary>
        [Test]
        public void Strip_IsAColorSwatch_AndTracksTheValue()
        {
            var editor = new GradientEditor();
            var strip = editor.Q<ColorSwatch>(className: GradientEditor.stripUssClassName);
            Assert.IsNotNull(strip, "The preview strip must be a ColorSwatch.");

            editor.value = ThreeStops();
            Assert.IsNotNull(strip.value);
            Assert.AreEqual(3, strip.value.colorKeys.Length);
            Assert.AreEqual(Color.green.linear.gamma.r, strip.value.colorKeys[1].color.r, 1e-3f);
        }

        [Test]
        public void Markers_MatchTheKeyCounts_OnBothRails()
        {
            var editor = new GradientEditor { value = ThreeStops() };

            Assert.AreEqual(3, editor.colorMarkers.Count);
            Assert.AreEqual(2, editor.alphaMarkers.Count);

            editor.value = RedToBlue();
            Assert.AreEqual(2, editor.colorMarkers.Count);
            Assert.AreEqual(2, editor.alphaMarkers.Count);
        }

        /// <summary>
        /// The whole point of a stop bar: a marker sits exactly on the time it edits. Percent
        /// positioning plus a -50% translate is the only combination that survives a resize, and
        /// a marker off by a few pixels still looks plausible — so assert the numbers.
        /// </summary>
        [Test]
        public void Marker_SitsExactlyOnItsKeyTime()
        {
            var editor = new GradientEditor { value = ThreeStops() };

            var times = new[] { 0f, 0.5f, 1f };
            for (var i = 0; i < times.Length; i++)
            {
                var marker = editor.colorMarkers[i];
                Assert.AreEqual(LengthUnit.Percent, marker.style.left.value.unit,
                    "Markers must be positioned in percent so they track a resize.");
                Assert.AreEqual(times[i] * 100f, marker.style.left.value.value, 1e-3f);
                Assert.AreEqual(-50f, marker.style.translate.value.x.value, 1e-3f,
                    "A marker must be centred on its time, not left-aligned to it.");
            }
        }

        // ------------------------------------------------------------------ mutation

        [Test]
        public void SetSelectedKeyTime_MovesTheKeyAndItsMarker_Together()
        {
            var editor = new GradientEditor { value = ThreeStops() };
            editor.SelectKey(GradientEditor.KeyKind.Color, 1);

            editor.SetSelectedKeyTime(0.25f);

            Assert.AreEqual(0.25f, editor.value.colorKeys[1].time, 1e-4f);
            Assert.AreEqual(25f, editor.colorMarkers[editor.selectedKeyIndex].style.left.value.value, 1e-3f);
        }

        /// <summary>
        /// Stops are kept sorted, so dragging one past a neighbour reorders the list. The selection
        /// has to follow the stop it was on, not the index — otherwise a drag through a neighbour
        /// silently starts editing a different stop.
        /// </summary>
        [Test]
        public void SelectionFollowsTheStop_WhenAMoveReordersIt()
        {
            var editor = new GradientEditor { value = ThreeStops() };
            editor.SelectKey(GradientEditor.KeyKind.Color, 0);
            var picked = editor.value.colorKeys[0].color;

            editor.SetSelectedKeyTime(0.9f);

            Assert.AreEqual(1, editor.selectedKeyIndex, "The moved stop is now the middle one.");
            Assert.AreEqual(picked, editor.value.colorKeys[1].color,
                "The selection jumped to a different stop after the reorder.");
        }

        [Test]
        public void AddColorKey_SamplesTheGradientAtThatTime_AndSelectsIt()
        {
            var editor = new GradientEditor { value = RedToBlue() };

            Assert.IsTrue(editor.AddColorKey(0.5f));
            Assert.AreEqual(3, editor.value.colorKeys.Length);
            Assert.AreEqual(GradientEditor.KeyKind.Color, editor.selectedKeyKind);
            Assert.AreEqual(1, editor.selectedKeyIndex);

            // Inserting a stop must not change what the gradient looks like there.
            var sampled = editor.value.colorKeys[1].color;
            Assert.AreEqual(0.5f, sampled.r, 0.05f);
            Assert.AreEqual(0.5f, sampled.b, 0.05f);
        }

        [Test]
        public void AddKey_StopsAtUnitysEightKeyLimit()
        {
            var editor = new GradientEditor();

            for (var i = 0; i < 6; i++)
                Assert.IsTrue(editor.AddColorKey(0.1f + i * 0.1f), $"Add #{i} should fit under the limit.");

            Assert.AreEqual(GradientEditor.maxKeys, editor.value.colorKeys.Length);
            Assert.IsFalse(editor.AddColorKey(0.95f),
                "Unity's Gradient holds at most 8 keys; a 9th would be silently dropped by SetKeys.");
            Assert.AreEqual(GradientEditor.maxKeys, editor.value.colorKeys.Length);
        }

        /// <summary>
        /// <see cref="Gradient.SetKeys"/> pads a rail shorter than two back up to two without
        /// saying so, so deleting down to one stop would look like it worked and then hand back a
        /// gradient with a duplicated key.
        /// </summary>
        [Test]
        public void RemoveSelectedKey_StopsAtUnitysTwoKeyFloor()
        {
            var editor = new GradientEditor { value = ThreeStops() };

            editor.SelectKey(GradientEditor.KeyKind.Color, 1);
            Assert.IsTrue(editor.RemoveSelectedKey());
            Assert.AreEqual(GradientEditor.minKeys, editor.value.colorKeys.Length);

            editor.SelectKey(GradientEditor.KeyKind.Color, 0);
            Assert.IsFalse(editor.RemoveSelectedKey(),
                "A rail already down to two stops must refuse a deletion.");
            Assert.AreEqual(GradientEditor.minKeys, editor.value.colorKeys.Length);

            editor.SelectKey(GradientEditor.KeyKind.Alpha, 0);
            Assert.IsFalse(editor.RemoveSelectedKey());
            Assert.AreEqual(GradientEditor.minKeys, editor.value.alphaKeys.Length);
        }

        /// <summary>
        /// The inspector writes to a <see cref="ColorField"/> and to the alpha slider's track, both
        /// built on <see cref="ColorSwatch"/>, whose setter sends a bubbling
        /// <c>ChangeEvent&lt;Gradient&gt;</c>. Those must not reach a consumer as the editor's own
        /// value change.
        /// </summary>
        [UnityTest]
        public IEnumerator DescendantSwatchEvents_DoNotEscapeAsTheEditorsOwn()
        {
            var editor = new GradientEditor();
            yield return Mount(editor);

            var seen = 0;
            editor.RegisterCallback<ChangeEvent<Gradient>>(_ => seen++);

            // Selecting an alpha stop rebuilds the alpha slider track, which is a ColorSwatch.
            editor.SelectKey(GradientEditor.KeyKind.Alpha, 0);
            yield return null;

            Assert.AreEqual(0, seen, "Selecting a stop is not a value change.");
        }

        [Test]
        public void Mode_ReachesTheValueAndTheStrip()
        {
            var editor = new GradientEditor();
            var strip = editor.Q<ColorSwatch>(className: GradientEditor.stripUssClassName);

            editor.mode = GradientMode.Fixed;

            Assert.AreEqual(GradientMode.Fixed, editor.value.mode);
            Assert.AreEqual(GradientMode.Fixed, strip.value.mode,
                "Fixed mode is rendered by the ColorSwatch shader; the strip has to be told.");
        }

        // ------------------------------------------------------------------ events

        [UnityTest]
        public IEnumerator Mutations_SendAChangeEvent_WithAFreshGradientInstance()
        {
            var editor = new GradientEditor();
            yield return Mount(editor);

            var seen = 0;
            Gradient last = null;
            editor.RegisterCallback<ChangeEvent<Gradient>>(e =>
            {
                seen++;
                last = e.newValue;
            });

            editor.SelectKey(GradientEditor.KeyKind.Color, 0);
            editor.SetSelectedKeyTime(0.2f);

            Assert.AreEqual(1, seen);
            Assert.IsNotNull(last);
            Assert.AreNotSame(editor.value, last,
                "The editor must hand out a fresh Gradient per read, so a consumer holding the "
                + "previous value never sees it mutate under them.");
            Assert.AreEqual(0.2f, last.colorKeys[0].time, 1e-4f);
        }

        [UnityTest]
        public IEnumerator FirstEditAfterLoading_ReportsTheLoadedValueAsPrevious()
        {
            var editor = new GradientEditor();
            yield return Mount(editor);

            editor.SetValueWithoutNotify(ThreeStops());

            Gradient previous = null;
            editor.RegisterCallback<ChangeEvent<Gradient>>(e => previous = e.previousValue);

            editor.SelectKey(GradientEditor.KeyKind.Color, 0);
            editor.SetSelectedKeyTime(0.2f);

            Assert.IsNotNull(previous,
                "The first edit after a load reported a null previousValue — the loaded gradient "
                + "is the baseline the change is relative to, including on Show(..., initialValue).");
            Assert.AreEqual(3, previous.colorKeys.Length);
            Assert.AreEqual(0f, previous.colorKeys[0].time, 1e-4f,
                "previousValue must be the state before this edit, not after it.");
        }

        [UnityTest]
        public IEnumerator SetValueWithoutNotify_DoesNotSendAChangeEvent()
        {
            var editor = new GradientEditor();
            yield return Mount(editor);

            var seen = 0;
            editor.RegisterCallback<ChangeEvent<Gradient>>(_ => seen++);
            editor.SetValueWithoutNotify(ThreeStops());

            Assert.AreEqual(0, seen);
            Assert.AreEqual(3, editor.colorMarkers.Count, "The rails still have to refresh.");
        }

        // ------------------------------------------------------------------ pointer

        /// <summary>
        /// The drag has to land on the value under the cursor. Percent markers and a pixel-space
        /// drag are two different coordinate systems, and a drag that is consistently a few percent
        /// off is invisible in a screenshot.
        /// </summary>
        [UnityTest]
        public IEnumerator DraggingAColorMarker_LandsOnTheTimeUnderThePointer()
        {
            var editor = new GradientEditor { value = RedToBlue() };
            yield return Mount(editor);

            var rail = editor.colorRailElement;
            var railRect = rail.worldBound;
            Assert.Greater(railRect.width, 10f, "The rail needs real geometry for this to mean anything.");

            var marker = editor.colorMarkers[0];
            using (var down = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = marker.worldBound.center,
                   }))
            {
                down.target = marker;
                marker.SendEvent(down);
            }
            yield return null;

            Assert.AreEqual(0, editor.selectedKeyIndex);
            Assert.AreEqual(GradientEditor.KeyKind.Color, editor.selectedKeyKind);

            var targetX = railRect.xMin + railRect.width * 0.4f;
            using (var move = PointerMoveEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDrag,
                       button = 0,
                       mousePosition = new Vector2(targetX, railRect.center.y),
                   }))
            {
                move.target = rail;
                rail.SendEvent(move);
            }
            yield return null;

            Assert.AreEqual(0.4f, editor.value.colorKeys[editor.selectedKeyIndex].time, 0.01f,
                "The dragged stop did not land on the time under the pointer.");
        }

        [UnityTest]
        public IEnumerator DoubleClickingAnEmptyRail_AddsAStopThere()
        {
            var editor = new GradientEditor { value = RedToBlue() };
            yield return Mount(editor);

            var rail = editor.alphaRailElement;
            var rect = rail.worldBound;

            using (var down = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       clickCount = 2,
                       mousePosition = new Vector2(rect.xMin + rect.width * 0.6f, rect.center.y),
                   }))
            {
                down.target = rail;
                rail.SendEvent(down);
            }
            yield return null;

            Assert.AreEqual(3, editor.value.alphaKeys.Length);
            Assert.AreEqual(GradientEditor.KeyKind.Alpha, editor.selectedKeyKind);
            Assert.AreEqual(0.6f, editor.value.alphaKeys[editor.selectedKeyIndex].time, 0.01f);
        }

        // ------------------------------------------------------------------ field seam

        [UnityTest]
        public IEnumerator ItIsAGradientFieldPicker_AndDrivesTheFieldLive()
        {
            var field = new ColorGradientField();
            var editor = new GradientEditor();
            field.customPicker = editor;
            m_TestUI.rootVisualElement.Clear();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var changing = 0;
            field.RegisterCallback<ChangingEvent<Gradient>>(_ => changing++);

            field.value = ThreeStops();
            field.OpenPicker();
            yield return null;

            Assert.AreEqual(3, editor.value.colorKeys.Length,
                "The editor must open on the field's current gradient, not on its own default.");

            editor.SelectKey(GradientEditor.KeyKind.Color, 0);
            editor.SetSelectedKeyTime(0.1f);
            yield return null;

            Assert.AreEqual(1, changing, "Edits must reach the field as they happen.");
            Assert.AreEqual(0.1f, field.value.colorKeys[0].time, 1e-4f);

            editor.Dismiss();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hdr_IsForwardedFromTheFieldToTheColorField()
        {
            var field = new ColorGradientField { hdr = true };
            var editor = new GradientEditor();
            field.customPicker = editor;
            m_TestUI.rootVisualElement.Clear();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            field.OpenPicker();
            yield return null;

            Assert.IsTrue(editor.hdr);
            Assert.IsTrue(editor.colorElement.hdr, "HDR has to reach the ColorField that edits the stop.");
            Assert.IsTrue(editor.ClassListContains(GradientEditor.hdrUssClassName));

            editor.Dismiss();
            yield return null;
        }

        // ------------------------------------------------------------------ USS source

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("GradientEditor.uss");
        }

        /// <summary>
        /// Drag maths reads the rail's content rect while markers are positioned against its
        /// padding box. Padding on a rail makes the two disagree by exactly the padding, which is
        /// the classic "handle lands slightly off" bug.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Rails_CarryNoPaddingOrBorder()
        {
            var uss = UssSource.Read("GradientEditor.uss");
            foreach (var selector in new[]
                     {
                         "." + GradientEditor.alphaRailUssClassName,
                         "." + GradientEditor.colorRailUssClassName,
                     })
            {
                var block = UssSource.RuleBlock(uss, selector);
                Assert.IsNotNull(block, $"Expected a {selector} rule.");
                StringAssert.Contains("padding: 0", block,
                    $"{selector} must have no padding, or a drag lands off by the padding.");
                StringAssert.Contains("border-width: 0", block);
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_IsRegisteredInTheThemeEntryPoint()
        {
            var uss = UssSource.ReadStylesheet("App UI.uss");
            StringAssert.Contains("./Components/GradientEditor.uss", uss,
                "A component stylesheet nobody imports silently styles nothing.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void StripHeight_ComesFromTheControlSizingAlias()
        {
            var uss = UssSource.Read("GradientEditor.uss");
            var block = UssSource.RuleBlock(uss, "." + GradientEditor.stripUssClassName);
            Assert.IsNotNull(block, "Expected a strip rule.");
            StringAssert.Contains("height: var(--appui-alias-control-sizing-md-height)", block,
                "The strip is a control-height surface; a magic px there stops responding to density.");
        }
    }
}
