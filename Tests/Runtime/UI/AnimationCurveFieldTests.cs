using System;
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
    [TestOf(typeof(AnimationCurveField))]
    class AnimationCurveFieldTests : VisualElementTests<AnimationCurveField>
    {
        protected override string mainUssClassName => AnimationCurveField.ussClassName;

        // Handed over to AnimationCurveFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };

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

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:AnimationCurveField />",
            @"<appui:AnimationCurveField size=""Sm"" />",
            @"<appui:AnimationCurveField size=""Md"" />",
            @"<appui:AnimationCurveField size=""Lg"" />",
        };

        [Test]
        public void Size_DefaultsToMd()
        {
            var field = new AnimationCurveField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(AnimationCurveField.ussClassName + "--size-md"));
        }

        [Test]
        public void Size_EmitsItsClass_AndDropsTheStaleOne()
        {
            var field = new AnimationCurveField();

            foreach (var (size, suffix) in k_Sizes)
            {
                field.size = size;
                Assert.IsTrue(field.ClassListContains(AnimationCurveField.ussClassName + "--size-" + suffix));

                foreach (var (_, other) in k_Sizes)
                {
                    if (other == suffix)
                        continue;
                    Assert.IsFalse(field.ClassListContains(AnimationCurveField.ussClassName + "--size-" + other),
                        $"Stale size class --size-{other} survived a switch to --size-{suffix}.");
                }
            }
        }

        [Test]
        public void Preview_ExistsAndCarriesItsClass()
        {
            var field = new AnimationCurveField();
            Assert.IsNotNull(field.previewElement);
            Assert.IsTrue(field.previewElement.ClassListContains(AnimationCurveField.previewUssClassName));
        }

        // Events only dispatch through a panel, so anything asserting on one has to be attached.
        [UnityTest]
        public IEnumerator Value_SendsAChangeEvent()
        {
            var field = new AnimationCurveField();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var seen = 0;
            field.RegisterCallback<ChangeEvent<AnimationCurve>>(_ => seen++);

            field.value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            Assert.AreEqual(1, seen);
        }

        [Test]
        public void NullAndEmptyCurves_DoNotThrowOnRepaint()
        {
            var field = new AnimationCurveField();
            m_TestUI.rootVisualElement.Add(field);
            Assert.DoesNotThrow(() => field.value = new AnimationCurve());
            Assert.DoesNotThrow(() => field.value = null);
        }

        [Test]
        public void OpenPicker_RaisesPickRequested_EvenWithNoEditorAssigned()
        {
            var field = new AnimationCurveField();
            var asked = 0;
            field.pickRequested += () => asked++;

            field.OpenPicker();
            Assert.AreEqual(1, asked,
                "With no CurveEditor ported yet the field must still be a well-behaved display "
                + "that reports the intent to edit.");
        }

        [UnityTest]
        public IEnumerator CustomPicker_ValueChanging_UpdatesTheField()
        {
            var picker = new RecordingPicker();
            var field = new AnimationCurveField { customPicker = picker };
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var changing = 0;
            field.RegisterCallback<ChangingEvent<AnimationCurve>>(_ => changing++);

            field.OpenPicker();
            Assert.AreEqual(1, picker.shown);

            var curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            picker.Emit(curve);
            Assert.AreSame(curve, field.value);
            Assert.AreEqual(1, changing);
        }

        class RecordingPicker : ICustomAnimationCurveFieldPicker
        {
            public int shown;
            Action<AnimationCurve> m_OnChanging;

            public void Show(AnimationCurveField owner, Action<AnimationCurve> onValueChanging, AnimationCurve initialValue)
            {
                shown++;
                m_OnChanging = onValueChanging;
            }

            public void Emit(AnimationCurve value) => m_OnChanging?.Invoke(value);
        }

        // ------------------------------------------------------------------ USS source

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            UssSource.AssertHeightsComeFromControlSizingAliases(
                "AnimationCurveField.uss", "." + AnimationCurveField.ussClassName);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("AnimationCurveField.uss");
        }

        /// <summary>
        /// The registry's <c>curve-field.tsx</c> strokes its thumbnail with a raw <c>#3ad16f</c>,
        /// which is a token-contract violation on the web side. UITK must not mirror it: the
        /// stroke comes from <c>--curve-color</c>, bound to a real semantic token so it tracks the
        /// theme.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void CurveStroke_ComesFromASemanticToken()
        {
            var uss = UssSource.Read("AnimationCurveField.uss");
            var block = UssSource.RuleBlock(uss, "." + AnimationCurveField.previewUssClassName);
            Assert.IsNotNull(block, "Expected a preview rule in AnimationCurveField.uss.");
            StringAssert.Contains("--curve-color: var(--appui-alias-semantic-positive)", block);
        }

        /// <summary>
        /// <c>--curve-width</c> has to stay unitless. A <c>CustomStyleProperty&lt;float&gt;</c>
        /// silently resolves a px length to 0, and a zero-width stroke draws nothing at all with
        /// no warning.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void CurveWidth_IsUnitless()
        {
            var uss = UssSource.Read("AnimationCurveField.uss");
            var block = UssSource.RuleBlock(uss, "." + AnimationCurveField.previewUssClassName);
            Assert.IsNotNull(block);
            StringAssert.Contains("--curve-width:", block);
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(block, @"--curve-width:\s*[\d.]+px"),
                "--curve-width must be unitless; CustomStyleProperty<float> resolves a px length "
                + "to 0 and the curve then draws nothing.");
        }
    }
}
