using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CurveEditor))]
    class CurveEditorTests : VisualElementTests<CurveEditor>
    {
        protected override string mainUssClassName => CurveEditor.ussClassName;

        // Handed over to CurveEditorSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // The editor is a picker surface, like ColorPicker: it is not authored from UXML.
        protected override bool uxmlConstructable => false;

        protected override IEnumerable<string> uxmlTestCases => null;

        static AnimationCurve Ramp() => AnimationCurve.Linear(0f, 0f, 1f, 1f);

        static AnimationCurve ThreeKeys()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f));
        }

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

        IEnumerator Mount(CurveEditor editor, float width = 320f, float height = 220f)
        {
            m_TestUI.rootVisualElement.Clear();
            editor.style.width = width;
            editor.graphElement.style.height = height;
            m_TestUI.rootVisualElement.Add(editor);
            for (var i = 0; i < 3; i++)
                yield return null;
        }

        // ------------------------------------------------------------------ shape

        [Test]
        public void DefaultValue_IsAnEaseInOut()
        {
            var editor = new CurveEditor();
            var curve = editor.value;
            Assert.IsNotNull(curve);
            Assert.AreEqual(2, curve.length);
            Assert.AreEqual(0f, curve.keys[0].value, 1e-4f);
            Assert.AreEqual(1f, curve.keys[1].value, 1e-4f);
            Assert.AreEqual(0f, curve.keys[0].outTangent, 1e-4f, "Ease In-Out starts flat.");
            Assert.AreEqual(0f, curve.keys[1].inTangent, 1e-4f, "Ease In-Out ends flat.");
        }

        [Test]
        public void Value_IsHandedOutAsAFreshCurve()
        {
            var editor = new CurveEditor();
            Assert.AreNotSame(editor.value, editor.value,
                "AnimationCurve is a reference type; handing out the live instance lets a consumer "
                + "mutate the editor's state behind its back.");
        }

        [Test]
        public void KeyMarkers_TrackTheKeyCount()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            Assert.AreEqual(3, editor.keyMarkers.Count);

            editor.value = Ramp();
            Assert.AreEqual(2, editor.keyMarkers.Count);
        }

        /// <summary>
        /// A key marker has to sit exactly on the point it edits. This is the single most
        /// load-bearing assertion in the file: a marker a few pixels off its key still looks
        /// correct in a screenshot and makes every drag feel wrong.
        /// </summary>
        [UnityTest]
        public IEnumerator KeyMarker_SitsExactlyOnItsKey()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            yield return Mount(editor);

            var plot = editor.plotRect;
            Assert.Greater(plot.width, 10f, "The plot needs real geometry for this to mean anything.");

            var curve = editor.value;
            for (var i = 0; i < curve.length; i++)
            {
                var expected = editor.DomainToPlot(curve.keys[i].time, curve.keys[i].value);
                var marker = editor.keyMarkers[i];
                Assert.AreEqual(expected.x, marker.style.left.value.value, 0.5f,
                    $"Key {i} marker is off in x.");
                Assert.AreEqual(expected.y, marker.style.top.value.value, 0.5f,
                    $"Key {i} marker is off in y.");
                Assert.AreEqual(-50f, marker.style.translate.value.x.value, 1e-3f);
                Assert.AreEqual(-50f, marker.style.translate.value.y.value, 1e-3f);
            }
        }

        // ------------------------------------------------------------------ mutation

        [Test]
        public void MoveSelectedKey_MovesTheKey()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SelectKey(1);

            editor.MoveSelectedKey(0.75f, 0.25f);

            Assert.AreEqual(0.75f, editor.value.keys[1].time, 1e-4f);
            Assert.AreEqual(0.25f, editor.value.keys[1].value, 1e-4f);
        }

        /// <summary>
        /// Endpoints anchor the domain. If they could slide in time the plot range would move
        /// under the pointer mid-drag, which reads as the curve fighting the cursor.
        /// </summary>
        [Test]
        public void MoveSelectedKey_PinsEndpointTimes_ButLetsThemMoveInValue()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SelectKey(0);

            editor.MoveSelectedKey(0.4f, 0.8f);

            Assert.AreEqual(0f, editor.value.keys[0].time, 1e-4f, "The first key keeps its time.");
            Assert.AreEqual(0.8f, editor.value.keys[0].value, 1e-4f);
        }

        [Test]
        public void MoveSelectedKey_ClampsAMiddleKeyBetweenItsNeighbours()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SelectKey(1);

            editor.MoveSelectedKey(5f, 0.5f);

            var t = editor.value.keys[1].time;
            Assert.Less(t, 1f, "A middle key must not cross the last key.");
            Assert.Greater(t, 0f);
            Assert.AreEqual(1, editor.selectedKeyIndex, "Clamping keeps the key at its index.");
        }

        [Test]
        public void AddKey_InsertsInTimeOrder_AndSelectsIt()
        {
            var editor = new CurveEditor { value = Ramp() };

            Assert.IsTrue(editor.AddKey(0.5f, 0.5f));
            Assert.AreEqual(3, editor.value.length);
            Assert.AreEqual(1, editor.selectedKeyIndex);
            Assert.AreEqual(0.5f, editor.value.keys[1].time, 1e-4f);
        }

        [Test]
        public void RemoveSelectedKey_KeepsAtLeastOneKey()
        {
            var editor = new CurveEditor { value = ThreeKeys() };

            editor.SelectKey(1);
            Assert.IsTrue(editor.RemoveSelectedKey());
            Assert.AreEqual(2, editor.value.length);

            editor.SelectKey(0);
            Assert.IsTrue(editor.RemoveSelectedKey());
            editor.SelectKey(0);
            Assert.IsFalse(editor.RemoveSelectedKey(), "The last key must not be removable.");
            Assert.AreEqual(1, editor.value.length);
        }

        // ------------------------------------------------------------------ tangents

        [Test]
        public void FlatMode_ZeroesBothTangents()
        {
            var editor = new CurveEditor { value = ThreeKeys() };

            editor.SetTangentMode(1, TangentMode.Flat);

            Assert.AreEqual(0f, editor.value.keys[1].inTangent, 1e-4f);
            Assert.AreEqual(0f, editor.value.keys[1].outTangent, 1e-4f);
            Assert.AreEqual(TangentMode.Flat, editor.GetTangentMode(1));
        }

        [Test]
        public void LinearMode_PointsAtTheNeighbours()
        {
            var editor = new CurveEditor { value = ThreeKeys() };

            editor.SetTangentMode(1, TangentMode.Linear);

            var keys = editor.value.keys;
            // (1 - 0) / (0.5 - 0) = 2 in, (0 - 1) / (1 - 0.5) = -2 out.
            Assert.AreEqual(2f, keys[1].inTangent, 1e-3f);
            Assert.AreEqual(-2f, keys[1].outTangent, 1e-3f);
        }

        /// <summary>
        /// The difference between Auto and Clamped Auto is exactly this: Clamped Auto flattens a
        /// local extremum so the interpolation cannot overshoot past it.
        /// </summary>
        [Test]
        public void ClampedAuto_FlattensALocalMaximum_WhereAutoOvershoots()
        {
            var asymmetric = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0.5f));

            var auto = new CurveEditor { value = asymmetric };
            auto.SetTangentMode(1, TangentMode.Auto);
            Assert.AreNotEqual(0f, auto.value.keys[1].outTangent,
                "Auto takes the slope across the neighbours, which is non-zero here.");

            var clamped = new CurveEditor { value = asymmetric };
            clamped.SetTangentMode(1, TangentMode.ClampedAuto);
            Assert.AreEqual(0f, clamped.value.keys[1].inTangent, 1e-4f);
            Assert.AreEqual(0f, clamped.value.keys[1].outTangent, 1e-4f);
        }

        [Test]
        public void BrokenMode_LetsTheTwoTangentsDiffer()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SetTangentMode(1, TangentMode.Broken);

            editor.SetTangent(1, CurveEditor.TangentSide.In, 3f);
            editor.SetTangent(1, CurveEditor.TangentSide.Out, -1f);

            Assert.AreEqual(3f, editor.value.keys[1].inTangent, 1e-3f);
            Assert.AreEqual(-1f, editor.value.keys[1].outTangent, 1e-3f);
        }

        [Test]
        public void FreeMode_KeepsTheTwoTangentsTogether()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SetTangentMode(1, TangentMode.Free);

            editor.SetTangent(1, CurveEditor.TangentSide.Out, 1.5f);

            Assert.AreEqual(1.5f, editor.value.keys[1].inTangent, 1e-3f,
                "A free (smooth) key moves both sides together.");
            Assert.AreEqual(1.5f, editor.value.keys[1].outTangent, 1e-3f);
        }

        /// <summary>
        /// A solved mode has to re-solve when the geometry it was solved from moves. Otherwise a
        /// key stays Flat in the mode dropdown while showing the tangents of where it used to be.
        /// </summary>
        [Test]
        public void SolvedModes_ReSolve_WhenANeighbourMoves()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            editor.SetTangentMode(1, TangentMode.Linear);
            Assert.AreEqual(2f, editor.value.keys[1].inTangent, 1e-3f);

            editor.SelectKey(0);
            editor.MoveSelectedKey(0f, 0.5f);

            // (1 - 0.5) / (0.5 - 0) = 1
            Assert.AreEqual(1f, editor.value.keys[1].inTangent, 1e-3f,
                "Linear tangents went stale after the neighbour moved.");
        }

        [UnityTest]
        public IEnumerator TangentHandles_AppearOnlyForTheSelectedKey()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            yield return Mount(editor);

            Assert.AreEqual(DisplayStyle.None, editor.inTangentHandle.resolvedStyle.display,
                "Nothing is selected, so there is nothing to show handles for.");

            editor.SelectKey(1);
            yield return null;

            Assert.AreEqual(DisplayStyle.Flex, editor.inTangentHandle.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, editor.outTangentHandle.resolvedStyle.display);
        }

        /// <summary>
        /// The handle is drawn at a fixed pixel radius along the tangent direction. If the drawing
        /// and the solving disagree about which way is positive, the handle sits mirrored and every
        /// drag pushes the tangent the wrong way — while still looking like a working handle.
        /// </summary>
        [UnityTest]
        public IEnumerator TangentHandle_SitsAlongTheTangent_OnTheCorrectSide()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            yield return Mount(editor);

            editor.SelectKey(1);
            editor.SetTangentMode(1, TangentMode.Free);
            editor.SetTangent(1, CurveEditor.TangentSide.Out, 1f);
            yield return null;

            var keyPt = editor.DomainToPlot(editor.value.keys[1].time, editor.value.keys[1].value);
            var outPt = new Vector2(editor.outTangentHandle.style.left.value.value,
                editor.outTangentHandle.style.top.value.value);
            var inPt = new Vector2(editor.inTangentHandle.style.left.value.value,
                editor.inTangentHandle.style.top.value.value);

            Assert.Greater(outPt.x, keyPt.x, "The out handle points forward in time.");
            Assert.Less(inPt.x, keyPt.x, "The in handle points backward in time.");
            Assert.Less(outPt.y, keyPt.y,
                "A positive slope goes up, and plot y grows downward — the out handle must be above the key.");
            Assert.Greater(inPt.y, keyPt.y);

            Assert.AreEqual(CurveEditor.handleRadius, Vector2.Distance(keyPt, outPt), 0.5f,
                "Handles sit at a fixed pixel radius so they stay grabbable at any zoom.");
        }

        // ------------------------------------------------------------------ presets

        [Test]
        public void Presets_AreExposed_AndEachOneApplies()
        {
            Assert.AreEqual(6, CurveEditor.presets.Count);

            var editor = new CurveEditor();
            for (var i = 0; i < CurveEditor.presets.Count; i++)
            {
                editor.ApplyPreset(i);
                Assert.AreEqual(CurveEditor.presets[i].curve.length, editor.value.length,
                    $"Preset '{CurveEditor.presets[i].id}' did not apply.");
            }

            editor.ApplyPreset(1); // Linear
            Assert.AreEqual(1f, editor.value.keys[0].outTangent, 1e-3f);
            Assert.AreEqual(1f, editor.value.keys[1].value, 1e-3f);
        }

        [Test]
        public void Presets_ArePerInstanceCopies_NotSharedState()
        {
            var a = new CurveEditor();
            a.ApplyPreset(1);
            a.SelectKey(1);
            a.MoveSelectedKey(1f, 0.25f);

            var b = new CurveEditor();
            b.ApplyPreset(1);

            Assert.AreEqual(1f, b.value.keys[1].value, 1e-4f,
                "Applying a preset must copy it; one editor mutated the shared preset curve.");
        }

        [UnityTest]
        public IEnumerator PresetButtons_ApplyTheirPreset()
        {
            var editor = new CurveEditor();
            yield return Mount(editor);

            var buttons = editor.presetsElement.Query(className: CurveEditor.presetUssClassName).ToList();
            Assert.AreEqual(CurveEditor.presets.Count, buttons.Count);

            var linear = buttons[1];
            using (var down = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       mousePosition = linear.worldBound.center,
                   }))
            {
                down.target = linear;
                linear.SendEvent(down);
            }
            using (var up = PointerUpEvent.GetPooled(new Event
                   {
                       type = EventType.MouseUp,
                       button = 0,
                       mousePosition = linear.worldBound.center,
                   }))
            {
                up.target = linear;
                linear.SendEvent(up);
            }
            yield return null;

            Assert.AreEqual(1f, editor.value.keys[1].value, 1e-3f);
            Assert.AreEqual(1f, editor.value.keys[0].outTangent, 1e-3f);
        }

        // ------------------------------------------------------------------ events

        [UnityTest]
        public IEnumerator Mutations_SendAChangeEvent()
        {
            var editor = new CurveEditor();
            yield return Mount(editor);

            var seen = 0;
            editor.RegisterCallback<ChangeEvent<AnimationCurve>>(_ => seen++);

            editor.SelectKey(0);
            editor.MoveSelectedKey(0f, 0.3f);

            Assert.AreEqual(1, seen);
            Assert.AreEqual(0.3f, editor.value.keys[0].value, 1e-4f);
        }

        [UnityTest]
        public IEnumerator FirstEditAfterLoading_ReportsTheLoadedValueAsPrevious()
        {
            var editor = new CurveEditor();
            yield return Mount(editor);

            editor.SetValueWithoutNotify(ThreeKeys());

            AnimationCurve previous = null;
            editor.RegisterCallback<ChangeEvent<AnimationCurve>>(e => previous = e.previousValue);

            editor.SelectKey(1);
            editor.MoveSelectedKey(0.5f, 0.25f);

            Assert.IsNotNull(previous,
                "The first edit after a load reported a null previousValue — the loaded curve is "
                + "the baseline the change is relative to, including on Show(..., initialValue).");
            Assert.AreEqual(3, previous.keys.Length);
            Assert.AreEqual(1f, previous.keys[1].value, 1e-4f,
                "previousValue must be the state before this edit, not after it.");
        }

        [UnityTest]
        public IEnumerator SetValueWithoutNotify_DoesNotSendAChangeEvent()
        {
            var editor = new CurveEditor();
            yield return Mount(editor);

            var seen = 0;
            editor.RegisterCallback<ChangeEvent<AnimationCurve>>(_ => seen++);
            editor.SetValueWithoutNotify(ThreeKeys());

            Assert.AreEqual(0, seen);
            Assert.AreEqual(3, editor.keyMarkers.Count, "The graph still has to refresh.");
        }

        // ------------------------------------------------------------------ pointer

        [UnityTest]
        public IEnumerator DraggingAKey_LandsOnThePointUnderThePointer()
        {
            var editor = new CurveEditor { value = ThreeKeys() };
            yield return Mount(editor);

            var graph = editor.graphElement;
            var marker = editor.keyMarkers[1];

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

            Assert.AreEqual(1, editor.selectedKeyIndex);

            // Aim at a point the middle key is actually allowed to reach.
            var target = editor.DomainToPlot(0.6f, 0.35f);
            var world = graph.LocalToWorld(target);
            using (var move = PointerMoveEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDrag,
                       button = 0,
                       mousePosition = world,
                   }))
            {
                move.target = graph;
                graph.SendEvent(move);
            }
            yield return null;

            Assert.AreEqual(0.6f, editor.value.keys[1].time, 0.02f,
                "The dragged key did not land on the time under the pointer.");
            Assert.AreEqual(0.35f, editor.value.keys[1].value, 0.02f,
                "The dragged key did not land on the value under the pointer.");
        }

        [UnityTest]
        public IEnumerator DoubleClickingTheGraph_AddsAKeyThere()
        {
            var editor = new CurveEditor { value = Ramp() };
            yield return Mount(editor);

            var graph = editor.graphElement;
            var world = graph.LocalToWorld(editor.DomainToPlot(0.5f, 0.7f));

            using (var down = PointerDownEvent.GetPooled(new Event
                   {
                       type = EventType.MouseDown,
                       button = 0,
                       clickCount = 2,
                       mousePosition = world,
                   }))
            {
                down.target = graph;
                graph.SendEvent(down);
            }
            yield return null;

            Assert.AreEqual(3, editor.value.length);
            Assert.AreEqual(0.5f, editor.value.keys[1].time, 0.02f);
            Assert.AreEqual(0.7f, editor.value.keys[1].value, 0.02f);
        }

        // ------------------------------------------------------------------ field seam

        [UnityTest]
        public IEnumerator ItIsACurveFieldPicker_AndDrivesTheFieldLive()
        {
            var field = new AnimationCurveField();
            var editor = new CurveEditor();
            field.customPicker = editor;
            m_TestUI.rootVisualElement.Clear();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var changing = 0;
            field.RegisterCallback<ChangingEvent<AnimationCurve>>(_ => changing++);

            field.value = ThreeKeys();
            field.OpenPicker();
            yield return null;

            Assert.AreEqual(3, editor.value.length,
                "The editor must open on the field's current curve, not on its own default.");

            editor.SelectKey(1);
            editor.MoveSelectedKey(0.5f, 0.4f);
            yield return null;

            Assert.AreEqual(1, changing, "Edits must reach the field as they happen.");
            Assert.AreEqual(0.4f, field.value.keys[1].value, 1e-3f);

            editor.Dismiss();
            yield return null;
        }

        // ------------------------------------------------------------------ USS source

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("CurveEditor.uss");
        }

        /// <summary>
        /// The stroke colours are read back into C# through custom properties, exactly as
        /// <see cref="AnimationCurveField"/> does it, so <c>generateVisualContent</c> never paints a
        /// literal. <c>--curve-width</c> stays unitless because a
        /// <c>CustomStyleProperty&lt;float&gt;</c> silently resolves a px length to 0.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void PaintedColors_ComeFromCustomPropertiesBoundToTokens()
        {
            var uss = UssSource.Read("CurveEditor.uss");
            var block = UssSource.RuleBlock(uss, "." + CurveEditor.graphUssClassName);
            Assert.IsNotNull(block, "Expected a graph rule.");

            StringAssert.Contains("--curve-color: var(--appui-", block);
            StringAssert.Contains("--grid-color: var(--appui-", block);
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(block, @"--curve-width:\s*[\d.]+px"),
                "--curve-width must stay unitless: a CustomStyleProperty<float> reads a px length as 0.");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(block, @"--grid-width:\s*[\d.]+px"),
                "--grid-width must stay unitless.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Uss_IsRegisteredInTheThemeEntryPoint()
        {
            var uss = UssSource.ReadStylesheet("App UI.uss");
            StringAssert.Contains("./Components/CurveEditor.uss", uss,
                "A component stylesheet nobody imports silently styles nothing.");
        }
    }
}
