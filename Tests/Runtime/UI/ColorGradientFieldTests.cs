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
    [TestOf(typeof(ColorGradientField))]
    class ColorGradientFieldTests : VisualElementTests<ColorGradientField>
    {
        protected override string mainUssClassName => ColorGradientField.ussClassName;

        // Handed over to ColorGradientFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };

        static Gradient RedToBlue()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

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

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ColorGradientField />",
            @"<appui:ColorGradientField size=""Sm"" />",
            @"<appui:ColorGradientField size=""Md"" />",
            @"<appui:ColorGradientField size=""Lg"" />",
            @"<appui:ColorGradientField hdr=""true"" />",
        };

        [Test]
        public void Size_DefaultsToMd()
        {
            var field = new ColorGradientField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(ColorGradientField.ussClassName + "--size-md"));
        }

        [Test]
        public void Size_EmitsItsClass_AndDropsTheStaleOne()
        {
            var field = new ColorGradientField();

            foreach (var (size, suffix) in k_Sizes)
            {
                field.size = size;
                Assert.IsTrue(field.ClassListContains(ColorGradientField.ussClassName + "--size-" + suffix));

                foreach (var (_, other) in k_Sizes)
                {
                    if (other == suffix)
                        continue;
                    Assert.IsFalse(field.ClassListContains(ColorGradientField.ussClassName + "--size-" + other),
                        $"Stale size class --size-{other} survived a switch to --size-{suffix}.");
                }
            }
        }

        /// <summary>
        /// The preview must be the shared <see cref="ColorSwatch"/>, not a second render path.
        /// If the field ever stops forwarding, the swatch keeps rendering the old gradient and
        /// nothing errors — this is the only thing that catches it.
        /// </summary>
        [Test]
        public void Value_IsForwardedToTheColorSwatch()
        {
            var field = new ColorGradientField();
            var swatch = field.Q<ColorSwatch>(className: ColorGradientField.swatchUssClassName);
            Assert.IsNotNull(swatch, "The preview must be a ColorSwatch carrying the swatch class.");

            var gradient = RedToBlue();
            field.value = gradient;
            Assert.AreSame(gradient, swatch.value);
        }

        // Events only dispatch through a panel, so anything asserting on one has to be attached.
        [UnityTest]
        public IEnumerator Value_SendsAChangeEvent()
        {
            var field = new ColorGradientField();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var seen = 0;
            field.RegisterCallback<ChangeEvent<Gradient>>(_ => seen++);

            field.value = RedToBlue();
            Assert.AreEqual(1, seen);
        }

        [Test]
        public void OpenPicker_RaisesPickRequested_EvenWithNoEditorAssigned()
        {
            var field = new ColorGradientField();
            var asked = 0;
            field.pickRequested += () => asked++;

            field.OpenPicker();
            Assert.AreEqual(1, asked,
                "With no GradientEditor ported yet the field must still be a well-behaved display "
                + "that reports the intent to edit.");
        }

        [UnityTest]
        public IEnumerator CustomPicker_ValueChanging_UpdatesTheFieldAndTheSwatch()
        {
            var picker = new RecordingPicker();
            var field = new ColorGradientField { customPicker = picker, hdr = true };
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var swatch = field.Q<ColorSwatch>(className: ColorGradientField.swatchUssClassName);
            var changing = 0;
            field.RegisterCallback<ChangingEvent<Gradient>>(_ => changing++);

            field.OpenPicker();
            Assert.AreEqual(1, picker.shown);
            Assert.IsTrue(picker.lastHdr, "hdr must be forwarded to the editor.");

            var gradient = RedToBlue();
            picker.Emit(gradient);
            Assert.AreSame(gradient, field.value);
            Assert.AreSame(gradient, swatch.value);
            Assert.AreEqual(1, changing);
        }

        class RecordingPicker : ICustomColorGradientFieldPicker
        {
            public int shown;
            public bool lastHdr;
            Action<Gradient> m_OnChanging;

            public void Show(ColorGradientField owner, Action<Gradient> onValueChanging, Gradient initialValue, bool hdr)
            {
                shown++;
                lastHdr = hdr;
                m_OnChanging = onValueChanging;
            }

            public void Emit(Gradient value) => m_OnChanging?.Invoke(value);
        }

        // ------------------------------------------------------------------ USS source

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            UssSource.AssertHeightsComeFromControlSizingAliases(
                "ColorGradientField.uss", "." + ColorGradientField.ussClassName);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("ColorGradientField.uss");
        }

        /// <summary>
        /// The swatch is a full-bleed preview inside the field, so it has to shed the ColorSwatch
        /// chip's fixed square sizing. Without the override it renders as a small square in a
        /// wide field and still looks plausible.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Swatch_FillsTheField()
        {
            var uss = UssSource.Read("ColorGradientField.uss");
            var block = UssSource.RuleBlock(uss, "." + ColorGradientField.swatchUssClassName);
            Assert.IsNotNull(block, "Expected a swatch rule in ColorGradientField.uss.");
            StringAssert.Contains("flex-grow: 1", block);
            StringAssert.Contains("width: auto", block);
            StringAssert.Contains("height: auto", block);
        }
    }
}
