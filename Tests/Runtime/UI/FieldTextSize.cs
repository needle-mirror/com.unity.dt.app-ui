using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Shared checks for a field's text size: the step its registry counterpart reads, and whether a
    /// line of that text still fits the control's box.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The registry's fields read <c>text-sm</c> and <c>text-base</c>, which <c>TEXT_TIERS</c> in
    /// <c>build-registry.ts</c> maps onto <c>--appui-font-sizes-100</c> and <c>-200</c>. A rendered
    /// check compares against a <see cref="Text"/> probe in the same panel, since <c>Text</c>'s
    /// <c>Sm</c> and <c>Base</c> sizes read those two steps, so the expectation follows the density
    /// rather than a hardcoded px. A literal <c>14px</c> would pass at normal density only, so every
    /// check runs at compact as well.
    /// </para>
    /// <para>
    /// The rendered size cannot tell <c>font-sizes-200</c> from <c>body-lg</c>, which resolves to
    /// the same step today. <see cref="AssertFontSize"/> reads the source for that.
    /// </para>
    /// </remarks>
    static class FieldTextSize
    {
        /// <summary>
        /// The densities that change the font-size steps. Comfort keeps the normal steps in taller
        /// boxes, so it adds nothing to either check.
        /// </summary>
        public static readonly string[] densities = { "normal", "compact" };

        /// <summary>
        /// A mounted panel with one probe per step the registry's fields read.
        /// </summary>
        public class Stage
        {
            /// <summary> The panel to add fields to. </summary>
            public Panel panel;

            /// <summary> Reads <c>--appui-font-sizes-100</c>, the registry's <c>text-sm</c>. </summary>
            public Text sm;

            /// <summary> Reads <c>--appui-font-sizes-200</c>, the registry's <c>text-base</c>. </summary>
            public Text @base;

            /// <summary> The density the panel is in. </summary>
            public string density;

            /// <summary>
            /// The resolved size of the probe for a registry text tier.
            /// </summary>
            /// <param name="tier"> <c>"sm"</c> or <c>"base"</c>. </param>
            /// <returns> The probe's font size, in px. </returns>
            public float Expected(string tier) => tier switch
            {
                "sm" => sm.resolvedStyle.fontSize,
                "base" => @base.resolvedStyle.fontSize,
                _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null),
            };
        }

        /// <summary>
        /// Clears the test root and mounts a panel at a density, with the two probes in it, then
        /// adds what <paramref name="build"/> returns and waits for styles to resolve.
        /// </summary>
        /// <param name="root"> The test root. </param>
        /// <param name="density"> The panel density. </param>
        /// <param name="build"> Adds the elements under test to the panel. </param>
        /// <param name="stage"> Receives the mounted stage. </param>
        /// <returns> A coroutine that ends once styles have resolved. </returns>
        public static IEnumerator Mount(VisualElement root, string density, Action<Panel> build, Action<Stage> stage)
        {
            root.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            root.Add(panel);
            var s = new Stage
            {
                panel = panel,
                density = density,
                sm = new Text("Ag") { size = TextSize.S },
                @base = new Text("Ag") { size = TextSize.M },
            };
            panel.Add(s.sm);
            panel.Add(s.@base);
            build(panel);
            yield return null;
            yield return null;

            // Compact's 10px text floor lifts font-sizes-100 to 12px, the same as -200, so the two
            // probes only differ at normal density. AssertFontSize still tells the steps apart by
            // reading the source.
            if (density == "compact")
                Assert.LessOrEqual(s.sm.resolvedStyle.fontSize, s.@base.resolvedStyle.fontSize,
                    $"{density}: the text-sm probe resolved larger than text-base.");
            else
                Assert.Less(s.sm.resolvedStyle.fontSize, s.@base.resolvedStyle.fontSize,
                    $"{density}: the probes did not resolve two distinct steps, so nothing below can be trusted.");
            stage(s);
        }

        /// <summary>
        /// Asserts a text element renders at the probe size for a registry text tier.
        /// </summary>
        /// <param name="stage"> The mounted stage. </param>
        /// <param name="text"> The element that draws the text. </param>
        /// <param name="tier"> <c>"sm"</c> or <c>"base"</c>. </param>
        /// <param name="what"> Names the element in the failure message. </param>
        public static void AssertTier(Stage stage, VisualElement text, string tier, string what)
        {
            Assert.IsNotNull(text, $"{what}: the text element was not found.");
            Assert.AreEqual(stage.Expected(tier), text.resolvedStyle.fontSize, 0.01f,
                $"{stage.density}: {what} must render at the registry's text-{tier}.");
        }

        /// <summary>
        /// Asserts one line of text at the element's font size fits the box that clips it, or at
        /// least overflows it no more than a <c>text-sm</c> line does.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The field containers are <c>overflow: hidden</c>, so a line taller than the box is cut off
        /// without any error.
        /// </para>
        /// <para>
        /// The measure is the font's whole line box, rounded up, not the ink. A <c>text-sm</c> line
        /// already overruns a compact <c>Sm</c> control by a pixel, and that is the geometry the
        /// fields shipped with before any of this, with the glyphs themselves inside the box. So the
        /// bar is the larger of the box and the <c>text-sm</c> probe's line: a field may be no worse
        /// off than that.
        /// </para>
        /// </remarks>
        /// <param name="stage"> The mounted stage. </param>
        /// <param name="text"> The element that draws the text. </param>
        /// <param name="box"> The control whose content box holds the line. </param>
        /// <param name="what"> Names the element in the failure message. </param>
        public static void AssertFits(Stage stage, TextElement text, VisualElement box, string what)
        {
            Assert.IsNotNull(text, $"{what}: the text element was not found.");
            var line = Line(text);
            var room = Math.Max(box.contentRect.height, Line(stage.sm));
            Assert.Greater(line, 0f, $"{stage.density}: {what} measured no line at all.");
            Assert.LessOrEqual(line, room + 0.01f,
                $"{stage.density}: a {line}px line of {what} does not fit the {box.contentRect.height}px "
                + $"the control leaves it, and overruns it more than a text-sm line ({Line(stage.sm)}px) does.");
        }

        /// <summary>
        /// Asserts the text an element shows fits the width its input leaves it, so a fixed-width
        /// host does not cut it off sideways.
        /// </summary>
        /// <param name="stage"> The mounted stage. </param>
        /// <param name="text"> The element that draws the text. </param>
        /// <param name="box"> The element whose content box holds the text. </param>
        /// <param name="what"> Names the element in the failure message. </param>
        public static void AssertFitsWidth(Stage stage, TextElement text, VisualElement box, string what)
        {
            Assert.IsNotNull(text, $"{what}: the text element was not found.");
            Assert.IsFalse(string.IsNullOrEmpty(text.text), $"{what}: shows no text to measure.");
            var width = text.MeasureTextSize(text.text, 0, VisualElement.MeasureMode.Undefined, 0,
                VisualElement.MeasureMode.Undefined).x;
            var room = box.contentRect.width;
            Assert.LessOrEqual(width, room + 0.01f,
                $"{stage.density}: \"{text.text}\" in {what} needs {width}px but the input leaves it {room}px.");
        }

        static float Line(TextElement text) =>
            text.MeasureTextSize("Ag", 0, VisualElement.MeasureMode.Undefined, 0,
                VisualElement.MeasureMode.Undefined).y;

        /// <summary>
        /// Asserts a rule sets its font size from one exact <c>--appui-font-sizes-*</c> step.
        /// </summary>
        /// <remarks>
        /// <c>body-lg</c> and <c>font-sizes-200</c> resolve to the same pixels, as do <c>body-md</c>
        /// and <c>font-sizes-100</c>, so no rendered check can tell them apart. The registry reads the
        /// numbered steps through <c>TEXT_TIERS</c>, and so does UITK.
        /// </remarks>
        /// <param name="fileName"> The stylesheet to read. </param>
        /// <param name="selector"> The exact selector that starts the rule. </param>
        /// <param name="step"> The step, e.g. <c>200</c>. </param>
        public static void AssertFontSize(string fileName, string selector, int step)
        {
            var block = UssSource.RuleBlock(UssSource.Read(fileName), selector);
            Assert.IsNotNull(block, $"Expected a {selector} rule in {fileName}.");
            StringAssert.Contains($"font-size: var(--appui-font-sizes-{step});", block,
                $"{selector} must read --appui-font-sizes-{step}, the step the registry's field reads.");
        }

        /// <summary>
        /// The size steps a field exposes, with their USS suffix.
        /// </summary>
        public static readonly (Size size, string suffix)[] sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };

        /// <summary>
        /// The registry's <c>textFieldVariants</c> text tier for a UITK size: <c>text-sm</c> at
        /// <c>sm</c>, <c>text-base</c> at <c>default</c> and <c>lg</c>.
        /// </summary>
        /// <param name="size"> The UITK size. </param>
        /// <returns> <c>"sm"</c> or <c>"base"</c>. </returns>
        public static string TextFieldTier(Size size) => size == Size.Sm ? "sm" : "base";

        /// <summary>
        /// The step that goes with a tier.
        /// </summary>
        /// <param name="tier"> <c>"sm"</c> or <c>"base"</c>. </param>
        /// <returns> 100 or 200. </returns>
        public static int Step(string tier) => tier == "sm" ? 100 : 200;
    }
}
