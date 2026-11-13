using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldTests : VisualElementTests<ColorField>
    {
        protected override string mainUssClassName => ColorField.ussClassName;

        // Handed over to ColorFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        IEnumerator Mount(VisualElement element)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);
            yield return null;
            yield return null;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ColorField />",
            @"<appui:ColorField value=""#FF0000"" size=""Md"" invalid=""false"" swatch-only=""false"" inline-picker=""false"" />",
            @"<appui:ColorField value=""#FF0000"" swatch-size=""Md"" color-picker-type=""UnityEditor"" hdr=""true"" show-alpha=""false"" />",
        };
        [Test]
        public void ShowAlphaValue_IsOffByDefault()
        {
            var field = new ColorField();
            Assert.IsFalse(field.showAlphaValue);
            Assert.IsTrue(field.alphaField.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(field.ClassListContains(ColorField.showAlphaValueUssClassName));
        }

        /// <summary>
        /// A field with no alpha channel has no opacity to show, so hiding the channel hides the readout
        /// whatever the readout's own flag says.
        /// </summary>
        [Test]
        public void ShowAlphaValue_NeedsShowAlpha()
        {
            var field = new ColorField { showAlphaValue = true };
            Assert.IsTrue(field.ClassListContains(ColorField.showAlphaValueUssClassName));
            Assert.IsFalse(field.alphaField.ClassListContains(Styles.hiddenUssClassName));

            field.showAlpha = false;
            Assert.IsTrue(field.showAlphaValue, "The caller's own flag must survive the channel going away.");
            Assert.IsFalse(field.ClassListContains(ColorField.showAlphaValueUssClassName));
            Assert.IsTrue(field.alphaField.ClassListContains(Styles.hiddenUssClassName));

            field.showAlpha = true;
            Assert.IsFalse(field.alphaField.ClassListContains(Styles.hiddenUssClassName),
                "The readout must come back with the channel.");
        }

        [Test]
        public void TheReadoutIsTheValuesAlphaAsAWholePercent()
        {
            var field = new ColorField { showAlphaValue = true };

            field.value = new Color(1f, 0f, 0f, 1f);
            Assert.AreEqual(100, field.alphaField.value);

            field.value = new Color(1f, 0f, 0f, 0f);
            Assert.AreEqual(0, field.alphaField.value);

            field.value = new Color(1f, 0f, 0f, 0.5f);
            Assert.AreEqual(50, field.alphaField.value);
        }

        /// <summary>
        /// Editing the readout rewrites the colour's alpha and nothing else, so the swatch, the hex and
        /// the picker's alpha slider stay in step.
        /// </summary>
        [UnityTest]
        public IEnumerator EditingTheReadout_RewritesOnlyTheAlpha()
        {
            var field = new ColorField { showAlphaValue = true, value = new Color(0.2f, 0.4f, 0.6f, 1f) };
            yield return Mount(field);

            // Through the field's own value, which is the path a typed or scrubbed percent takes. An
            // event only reaches its callbacks through a panel, so this has to be mounted.
            field.alphaField.value = 25;
            yield return null;

            Assert.AreEqual(0.2f, field.value.r, 0.001f);
            Assert.AreEqual(0.4f, field.value.g, 0.001f);
            Assert.AreEqual(0.6f, field.value.b, 0.001f);
            Assert.AreEqual(0.25f, field.value.a, 0.005f);
        }

        /// <summary>
        /// Rounding to a whole percent throws away precision the picker's slider has, so a value the
        /// readout did not change must not be written back through it: editing red would otherwise
        /// quantize an alpha nobody touched.
        /// </summary>
        [UnityTest]
        public IEnumerator TheReadoutDoesNotQuantizeAnAlphaItDidNotEdit()
        {
            var field = new ColorField { showAlphaValue = true };
            field.value = new Color(1f, 1f, 1f, 0.333f);
            var before = field.value.a;
            yield return Mount(field);

            // The percent the readout already shows for 0.333 alpha. Writing it back would round the
            // alpha to 0.33 and lose precision the picker's slider has.
            field.alphaField.value = 33;
            yield return null;

            Assert.AreEqual(before, field.value.a, 0.0001f,
                "A readout value that matches the current percent must leave the alpha alone.");
        }


        [Test]
        public void Value_HdrColorAboveOne_DoesNotThrowAndKeepsValue()
        {
            var field = new ColorField { hdr = true };
            var hdrColor = new Color(2, 0, 0, 1);

            Assert.DoesNotThrow(() => field.value = hdrColor);
            Assert.AreEqual(hdrColor, field.value);
            Assert.DoesNotThrow(() => field.SetValueWithoutNotify(new Color(0, 3, 0, 1)));
        }

        // ---------------------------------------------------------------- insets

        const string k_FieldInset = "--appui-alias-field-padding-horizontal-md";

        // The registry's swatch-side inset is calc(field-md - sizing-50) (#1409). USS has no calc(),
        // so the swatch side reads the one sizing step that equals it at every density. The test
        // below holds that equality, so a token change that breaks it fails here instead of
        // silently moving the swatch.
        const string k_SwatchInset = "--appui-sizing-160";

        /// <summary>
        /// The registry's <c>color-field</c> has only the md height and reads the md field inset at
        /// every size, with the swatch edge 2px tighter than the text edge (#1409). The edges are
        /// logical, so RTL swaps them.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_IsTheFieldInset_WithATighterSwatchEdge()
        {
            var uss = UssSource.Read("ColorField.uss");

            var ltr = UssSource.RuleBlock(uss, ".appui-colorfield");
            StringAssert.Contains($"padding-left: var({k_SwatchInset});", ltr,
                "The swatch (leading) edge must be the tighter inset.");
            StringAssert.Contains($"padding-right: var({k_FieldInset});", ltr,
                "The text (trailing) edge must be the md field inset.");

            var rtl = UssSource.RuleBlock(uss, ".appui--rtl .appui-colorfield");
            Assert.IsNotNull(rtl, "Expected an RTL rule for the ColorField root.");
            StringAssert.Contains($"padding-left: var({k_FieldInset});", rtl,
                "Under RTL the text edge is on the left.");
            StringAssert.Contains($"padding-right: var({k_SwatchInset});", rtl,
                "Under RTL the swatch edge is on the right.");
        }

        /// <summary>
        /// Holds the equality the swatch inset rests on: <c>sizing-160 = field-md - sizing-50</c> in
        /// every density, with field-md still aliasing <c>sizing-200</c>.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void SwatchInset_EqualsTheFieldInsetMinusSizing50_InEveryDensity()
        {
            var alias = UssSource.ReadStylesheet("Themes/App UI - Alias.uss");
            StringAssert.Contains($"{k_FieldInset}: var(--appui-sizing-200);", alias,
                "field-padding-horizontal-md no longer aliases sizing-200; re-derive the swatch inset.");

            foreach (var file in new[]
                     {
                         "Themes/App UI - Globals.uss",
                         "Themes/App UI - Compact.uss",
                         "Themes/App UI - Compact.class.uss",
                         "Themes/App UI - Comfort.uss",
                         "Themes/App UI - Comfort.class.uss",
                     })
            {
                var uss = UssSource.ReadStylesheet(file);
                float Step(int n)
                {
                    var m = System.Text.RegularExpressions.Regex.Match(uss, $@"--appui-sizing-{n}:\s*([\d.]+)px");
                    Assert.IsTrue(m.Success, $"{file} does not define --appui-sizing-{n}.");
                    return float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                }

                Assert.AreEqual(Step(200) - Step(50), Step(160),
                    $"In {file}, sizing-160 no longer equals sizing-200 - sizing-50, so the swatch inset drifted.");
            }
        }

        IEnumerator MountIn(Dir dir, VisualElement element)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { layoutDirection = dir };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SwatchEdge_IsTheTighterOne_InBothDirections()
        {
            var ltr = new ColorField { value = Color.red };
            yield return MountIn(Dir.Ltr, ltr);
            Assert.Less(ltr.resolvedStyle.paddingLeft, ltr.resolvedStyle.paddingRight,
                "In LTR the swatch sits on the left, so the left inset is the tighter one.");

            var rtl = new ColorField { value = Color.red };
            yield return MountIn(Dir.Rtl, rtl);
            Assert.Greater(rtl.resolvedStyle.paddingLeft, rtl.resolvedStyle.paddingRight,
                "In RTL the swatch sits on the right, so the right inset is the tighter one.");
        }

        [UnityTest]
        public IEnumerator SwatchOnly_HasNoInset_InBothDirections()
        {
            foreach (var dir in new[] { Dir.Ltr, Dir.Rtl })
            {
                var field = new ColorField { value = Color.red, swatchOnly = true };
                yield return MountIn(dir, field);
                Assert.AreEqual(0f, field.resolvedStyle.paddingLeft, $"A swatch-only field has no inset ({dir}).");
                Assert.AreEqual(0f, field.resolvedStyle.paddingRight, $"A swatch-only field has no inset ({dir}).");
            }
        }

        /// <summary>
        /// The registry's <c>color-field</c> has one size, md, and reads <c>text-base</c>. UITK's
        /// <c>Md</c> and <c>Lg</c> take that step. <c>Sm</c> has no registry counterpart, and a
        /// <c>text-base</c> line does not fit the compact <c>Sm</c> box, so it steps down to
        /// <c>text-sm</c> as the registry's other <c>sm</c> fields do. The hex and the alpha readout
        /// move together.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, ColorField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new ColorField { size = size, showText = true, showAlpha = true, showAlphaValue = true };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var tier = FieldTextSize.TextFieldTier(size);
                    var hex = field.Q(className: ColorField.labelUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, hex, tier, $"a {size} ColorField's hex");
                    var alpha = field.Q(className: ColorField.alphaFieldUssClassName)
                        .Q(className: NumericalField<int>.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, alpha, tier, $"a {size} ColorField's alpha readout");
                    FieldTextSize.AssertFits(stage, hex, field, $"a {size} ColorField's hex");
                    FieldTextSize.AssertFits(stage, alpha, field, $"a {size} ColorField's alpha readout");
                }
            }
        }

        /// <summary>
        /// The alpha readout is a fixed-width slot on the field's rim, so a full <c>100</c> must still
        /// fit sideways at every size and density.
        /// </summary>
        [UnityTest]
        public IEnumerator AlphaReadout_FitsAFullValue_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, ColorField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new ColorField
                        {
                            size = size, value = Color.white, showAlpha = true, showAlphaValue = true,
                        };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var alpha = field.Q(className: ColorField.alphaFieldUssClassName)
                        .Q(className: NumericalField<int>.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertFitsWidth(stage, alpha, alpha.parent, $"a {size} ColorField's alpha readout");
                }
            }
        }

        /// <summary>
        /// The registry's readout is <c>w-16</c>, 16 × <c>--spacing</c>. USS has no <c>calc()</c>, so
        /// the readout takes the sizing step nearest it at Normal density, a token rather than a px.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void AlphaReadout_WidthComesFromASizingToken()
        {
            var block = UssSource.RuleBlock(UssSource.Read("ColorField.uss"), ".appui-colorfield__alpha-field");
            Assert.IsNotNull(block, "Expected a .appui-colorfield__alpha-field rule.");
            StringAssert.Contains("width: var(--appui-sizing-1350);", block,
                "The alpha readout must be as wide as the registry's w-16, through a sizing token.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            FieldTextSize.AssertFontSize("ColorField.uss", ".appui-colorfield__label", 200);
            FieldTextSize.AssertFontSize("ColorField.uss", ".appui-colorfield--size-sm .appui-colorfield__label", 100);
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("ColorField.uss"),
                "ColorField reads the numbered step the registry reads, never a body-* alias.");
        }
    }
}
