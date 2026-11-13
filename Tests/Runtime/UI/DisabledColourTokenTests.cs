using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards the rule that a disabled state paints the role's <c>*-disabled</c> colour tokens and
    /// never dims with opacity (registry #1419).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>SetEnabled(false)</c> puts an element and its whole subtree in <c>:disabled</c>, so an
    /// <c>opacity: 0.5</c> on a disabled rule applies again at every level that matches it and
    /// multiplies. AnimationCurveField and MenuItem did both at once, tokens and opacity, so they rendered
    /// dimmer than any theme meant.
    /// </para>
    /// <para>
    /// ColorSlider keeps its opacity on purpose: it paints a colour value no token can stand in
    /// for, and the registry keeps opacity on it too. ColorWheel, SVSquare and Timeline do the same
    /// in their own stylesheets, which this fixture does not read.
    /// </para>
    /// </remarks>
    [TestFixture]
    class DisabledColourTokenTests
    {
        const string k_FieldBackground = "--appui-alias-field-background-color-disabled";
        const string k_FieldBorder = "--appui-alias-field-border-color-disabled";
        const string k_Text = "--appui-alias-text-disabled";
        const string k_ControlBorder = "--appui-alias-control-border-color-disabled";
        const string k_ActionsBackground = "--appui-alias-actions-background-color-disabled";
        const string k_ActionsBorder = "--appui-alias-actions-border-color-disabled";
        const string k_ActionsForeground = "--appui-alias-actions-foreground-color-disabled";
        const string k_SelectedBackground = "--appui-alias-actions-selected-background-color-disabled";

        const string k_SliderControl = ".appui-slider__slider-control:disabled";
        const string k_Thumb = k_SliderControl + " > .appui-slider__thumbs-container > .appui-thumb > .appui-thumb__content";
        const string k_ColorSliderControl = ".appui-color-slider > " + k_SliderControl;

        /// <summary>
        /// The stylesheets this slice moved off opacity.
        /// </summary>
        static readonly string[] k_Stylesheets =
        {
            "AnimationCurveField.uss",
            "ColorGradientField.uss",
            "Menu.uss",
            "Chip.uss",
            "Slider.uss",
            "MonthPicker.uss",
            "YearPicker.uss",
        };

        /// <summary>
        /// (stylesheet, selector, property, token) for every disabled paint this slice owns.
        /// </summary>
        static readonly object[][] k_DisabledPaints =
        {
            new object[] { "AnimationCurveField.uss", ".appui-animationcurvefield:disabled", "border-color", k_FieldBorder },
            new object[] { "AnimationCurveField.uss", ".appui-animationcurvefield:disabled", "background-color", k_FieldBackground },
            new object[] { "AnimationCurveField.uss", ".appui-animationcurvefield:disabled > .appui-animationcurvefield__preview", "--curve-color", k_Text },

            new object[] { "ColorGradientField.uss", ".appui-colorgradientfield:disabled", "border-color", k_FieldBorder },
            new object[] { "ColorGradientField.uss", ".appui-colorgradientfield:disabled", "background-color", k_FieldBackground },

            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__label", "color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__icon", "--unity-image-tint-color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__checkmark", "--unity-image-tint-color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__submenu-icon", "--unity-image-tint-color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__shortcut", "color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem:disabled > .appui-menuitem__radio", "background-color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem--selectable.appui-menuitem.is-selected:disabled", "background-color", k_SelectedBackground },
            // A selected row's label is four classes deep, so a plain disabled rule loses to it.
            new object[] { "Menu.uss", ".appui-menuitem--selectable.appui-menuitem.is-selected:disabled > .appui-menuitem__label", "color", k_Text },
            new object[] { "Menu.uss", ".appui-menuitem--indicator-radio.appui-menuitem--selectable.is-selected:disabled > .appui-menuitem__radio", "background-color", k_Text },

            new object[] { "Chip.uss", ".appui-chip:disabled", "border-color", k_ActionsBorder },
            new object[] { "Chip.uss", ".appui-chip.appui-chip--filled:disabled", "background-color", k_ActionsBackground },
            new object[] { "Chip.uss", ".appui-chip:disabled > .appui-chip__label", "color", k_ActionsForeground },
            new object[] { "Chip.uss", ".appui-chip:disabled > .appui-chip__delete-button > .appui-chip__delete-icon.appui-icon", "--unity-image-tint-color", k_ActionsForeground },

            new object[] { "Slider.uss", k_SliderControl + " > .appui-slider__track", "background-color", k_FieldBackground },
            // Five parts deep so it outranks the invalid progress fill.
            new object[] { "Slider.uss", ".appui-slider > " + k_SliderControl + " > .appui-slider__progress-container > .appui-slider__progress", "background-color", k_Text },
            new object[] { "Slider.uss", k_Thumb, "background-color", k_Text },
            new object[] { "Slider.uss", k_Thumb, "border-color", k_ControlBorder },
            new object[] { "Slider.uss", k_SliderControl + " > .appui-slider__mark-container > .appui-slider__mark", "background-color", k_ControlBorder },
            new object[] { "Slider.uss", k_SliderControl + " > .appui-slider__mark-container > .appui-slider__mark > .appui-slider__mark-label", "color", k_Text },

            new object[] { "MonthPicker.uss", ".appui-text.appui-date-picker-pane__month-button:disabled", "color", k_Text },
            new object[] { "YearPicker.uss", ".appui-text.appui-date-picker-pane__year-button:disabled", "color", k_Text },
        };

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void DisabledRules_NeverDimWithOpacity([ValueSource(nameof(k_Stylesheets))] string stylesheet)
        {
            var offenders = new List<string>();
            foreach (var (selectors, body) in UssTheme.Rules(UssSource.Read(stylesheet)))
            {
                if (!Regex.IsMatch(body, @"(?:^|;|\s)opacity\s*:"))
                    continue;
                // The ColorSlider carve-out is the one disabled opacity allowed here.
                offenders.AddRange(selectors.Where(s => s.Contains(":disabled") && !s.StartsWith(".appui-color-slider")));
            }

            Assert.IsEmpty(offenders,
                $"{stylesheet} dims a disabled state with opacity. A disabled subtree matches the rule at "
                + "every level, so the opacity compounds; paint the role's *-disabled colour tokens instead:\n"
                + string.Join("\n", offenders));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        [TestCaseSource(nameof(k_DisabledPaints))]
        public void DisabledState_PaintsTheRoleToken(string stylesheet, string selector, string property, string token)
        {
            var declared = UssTheme.DeclaredToken(UssSource.Read(stylesheet), selector, property);
            Assert.AreEqual(token, declared,
                $"{stylesheet}: '{selector}' should paint {property} with {token}.");
        }

        /// <summary>
        /// ColorSlider shares Slider's parts, so Slider.uss's disabled rules reach it too. Its track
        /// and thumb hold a colour value, so it keeps the registry's opacity and its own thumb rim.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void ColorSlider_KeepsItsOpacityAndRim_WhenDisabled()
        {
            var uss = UssSource.Read("Slider.uss");
            var block = UssSource.RuleBlock(uss, k_ColorSliderControl);
            Assert.IsNotNull(block, $"Slider.uss has no '{k_ColorSliderControl}' rule, so a disabled ColorSlider no longer looks disabled.");
            StringAssert.IsMatch(@"opacity\s*:\s*0\.5", block);

            var rim = UssTheme.DeclaredToken(uss, ".appui-color-slider > " + k_Thumb, "border-color");
            var enabledRim = UssTheme.DeclaredToken(UssSource.Read("ColorSlider.uss"),
                ".appui-color-slider > .appui-slider__slider-control > .appui-slider__thumbs-container > .appui-thumb > .appui-thumb__content",
                "border-color");
            Assert.IsNotNull(enabledRim, "ColorSlider.uss no longer declares its thumb rim; update this test.");
            Assert.AreEqual(enabledRim, rim,
                "A disabled ColorSlider's thumb rim must stay its own, or the slider's disabled rim "
                + "paints over it under the opacity and dims it twice.");

            // A translucent colour value shows through to the thumb's and the track's own fill, so
            // repainting either one changes the colour on display.
            var thumbFill = UssTheme.DeclaredToken(uss, ".appui-color-slider > " + k_Thumb, "background-color");
            var enabledThumbFill = UssTheme.DeclaredToken(UssSource.Read("Thumb.uss"), ".appui-thumb__content", "background-color");
            Assert.IsNotNull(enabledThumbFill, "Thumb.uss no longer declares the thumb fill; update this test.");
            Assert.AreEqual(enabledThumbFill, thumbFill,
                "A disabled ColorSlider's thumb backdrop must stay its enabled fill.");

            var trackFill = UssTheme.DeclaredToken(uss, ".appui-color-slider > " + k_SliderControl + " > .appui-slider__track", "background-color");
            var enabledTrackFill = UssTheme.DeclaredToken(uss, ".appui-slider__track", "background-color");
            Assert.IsNotNull(enabledTrackFill, "Slider.uss no longer declares the track fill; update this test.");
            Assert.AreEqual(enabledTrackFill, trackFill,
                "A disabled ColorSlider's track backdrop must stay its enabled fill.");
        }

        const string k_SliderTrack = k_SliderControl + " > .appui-slider__track";
        const string k_SliderProgress = ".appui-slider > " + k_SliderControl + " > .appui-slider__progress-container > .appui-slider__progress";
        const string k_SelectedRow = ".appui-menuitem--selectable.appui-menuitem.is-selected:disabled";

        /// <summary>
        /// (what is painted, what it is painted on, what the pair is) for every disabled indicator
        /// that a component paints over its own disabled fill. Each side is (stylesheet, selector,
        /// property) and its token is read from the stylesheet, so a token swap in the USS reaches
        /// this check.
        /// </summary>
        static readonly ((string uss, string selector, string property) top, (string uss, string selector, string property) bottom, string what)[] k_Overlays =
        {
            (("Slider.uss", k_SliderProgress, "background-color"), ("Slider.uss", k_SliderTrack, "background-color"), "Slider range on the disabled track"),
            (("Slider.uss", k_Thumb, "background-color"), ("Slider.uss", k_SliderTrack, "background-color"), "Slider thumb on the disabled track"),
            (("Slider.uss", k_Thumb, "border-color"), ("Slider.uss", k_SliderProgress, "background-color"), "Slider thumb rim against the range it sits on"),
            (("AnimationCurveField.uss", ".appui-animationcurvefield:disabled > .appui-animationcurvefield__preview", "--curve-color"), ("AnimationCurveField.uss", ".appui-animationcurvefield:disabled", "background-color"), "AnimationCurveField stroke on the disabled field"),
            (("Chip.uss", ".appui-chip:disabled > .appui-chip__label", "color"), ("Chip.uss", ".appui-chip.appui-chip--filled:disabled", "background-color"), "Chip label on the disabled filled chip"),
            (("Menu.uss", k_SelectedRow + " > .appui-menuitem__label", "color"), ("Menu.uss", k_SelectedRow, "background-color"), "MenuItem label on a disabled selected row"),
        };

        static string Token((string uss, string selector, string property) paint)
        {
            var token = UssTheme.DeclaredToken(UssSource.Read(paint.uss), paint.selector, paint.property);
            Assert.IsNotNull(token, $"{paint.uss} declares no token {paint.property} for '{paint.selector}'.");
            return token;
        }

        /// <summary>
        /// A disabled token that lands on the colour beneath it makes the indicator vanish, and the
        /// stylesheet still reads token-correct. Resolve both sides in every theme, compositing the
        /// translucent Editor-theme fills over the page, and compare what a viewer would see.
        /// </summary>
        /// <remarks>
        /// The disabled slider track itself, <c>field-background-color-disabled</c>, does equal some
        /// surfaces (Cloud Dark and Cloud Light default, Dark raised and muted, Light subtle, raised,
        /// card and muted). That is the registry's own token choice and a token gap, not an
        /// indicator painted on its own fill, so it is not asserted here.
        /// </remarks>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void DisabledIndicators_StayDistinctFromTheFillBeneathThem_InEveryTheme()
        {
            var themes = UssTheme.ThemeFiles();
            Assert.GreaterOrEqual(themes.Count, 6, "Expected the Dark, Light, Cloud and Editor theme stylesheets.");

            var overlays = k_Overlays.Select(o => (top: Token(o.top), bottom: Token(o.bottom), o.what)).ToList();
            var failures = new List<string>();
            foreach (var theme in themes)
            {
                var vars = UssTheme.LoadVars(theme);
                Assert.IsTrue(UssTheme.TryResolveColour(vars, "--appui-alias-surface-default", out var page),
                    $"--appui-alias-surface-default does not resolve in {theme}.");

                foreach (var (top, bottom, what) in overlays)
                {
                    Assert.IsTrue(UssTheme.TryResolveColour(vars, top, out var topColour), $"{top} does not resolve in {theme}.");
                    Assert.IsTrue(UssTheme.TryResolveColour(vars, bottom, out var bottomColour), $"{bottom} does not resolve in {theme}.");
                    var under = UssTheme.Composite(bottomColour, page);
                    var over = UssTheme.Composite(topColour, under);
                    if (UssTheme.SameColour(over, under))
                        failures.Add($"{theme}: {what}: {top} lands on #{ColorUtility.ToHtmlStringRGB(over)}, the same as {bottom}");
                }

                // Disabled text has no fill of its own, so it lands straight on the surface.
                foreach (var surface in UssTheme.surfaces)
                {
                    if (!UssTheme.TryResolveColour(vars, surface, out var surfaceColour))
                        continue;
                    UssTheme.TryResolveColour(vars, k_Text, out var text);
                    var under = UssTheme.Composite(surfaceColour, page);
                    if (UssTheme.SameColour(UssTheme.Composite(text, under), under))
                        failures.Add($"{theme}: disabled text vanishes on {surface}");
                }
            }

            Assert.IsEmpty(failures,
                "A disabled indicator resolves to the colour beneath it, so it disappears:\n" + string.Join("\n", failures));
        }

        UIDocument m_TestUI;

        [SetUp]
        public void SetUp()
        {
            m_TestUI = Utils.ConstructTestUI();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);
            m_TestUI = null;
        }

        static readonly string[] k_Components = { "AnimationCurveField", "ColorGradientField", "MenuItem", "Chip", "Slider" };

        static VisualElement Create(string component)
        {
            switch (component)
            {
                case "AnimationCurveField":
                    return new AnimationCurveField { value = AnimationCurve.Linear(0f, 0f, 1f, 1f), style = { width = 120 } };
                case "ColorGradientField":
                    return new ColorGradientField { style = { width = 120 } };
                case "MenuItem":
                    return new MenuItem { label = "Open", icon = "folder-open", shortcut = "Ctrl+O" };
                case "Chip":
                    return new Chip { label = "Filter", deletable = true };
                case "Slider":
                    return new SliderFloat { lowValue = 0f, highValue = 1f, value = 0.5f, style = { width = 160 } };
                default:
                    Assert.Fail($"Unknown component {component}.");
                    return null;
            }
        }

        /// <summary>
        /// The runtime half of <see cref="DisabledRules_NeverDimWithOpacity"/>: once mounted under
        /// the real theme, disabling the component leaves every element's opacity where it was,
        /// whichever stylesheet a stray opacity would come from.
        /// </summary>
        [UnityTest]
        public IEnumerator Disabling_ChangesNoElementsOpacity([ValueSource(nameof(k_Components))] string component)
        {
            var enabled = Create(component);
            var disabled = Create(component);
            var host = new VisualElement();
            host.Add(enabled);
            host.Add(disabled);
            disabled.SetEnabled(false);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            var enabledTree = enabled.Query<VisualElement>().ToList();
            var disabledTree = disabled.Query<VisualElement>().ToList();
            Assert.AreEqual(enabledTree.Count, disabledTree.Count, "The two copies built different trees.");

            var dimmed = new List<string>();
            for (var i = 0; i < enabledTree.Count; i++)
            {
                var before = enabledTree[i].resolvedStyle.opacity;
                var after = disabledTree[i].resolvedStyle.opacity;
                // Not Mathf.Approximately: on 6000.7 it reports 0 and 0 as different.
                if (Mathf.Abs(before - after) > 1e-4f)
                    dimmed.Add($"{string.Join(" ", disabledTree[i].GetClasses())}: {before} -> {after}");
            }

            Assert.IsTrue(disabledTree.All(e => !e.enabledInHierarchy), "The copy did not disable.");
            Assert.IsEmpty(dimmed, "Disabling dimmed these elements with opacity:\n" + string.Join("\n", dimmed));
        }

        static readonly FieldInfo k_CurveColorField =
            typeof(AnimationCurveField).GetField("m_CurveColor", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// The curve is drawn with Painter2D from <c>--curve-color</c>, which AnimationCurveField reads back
        /// in C#. A disabled field has to hand it the disabled text colour, and a USS rule alone
        /// cannot show that the value actually reaches the painter.
        /// </summary>
        [UnityTest]
        public IEnumerator AnimationCurveField_DrawsTheDisabledStroke_WhenDisabled()
        {
            Assert.IsNotNull(k_CurveColorField, "AnimationCurveField.m_CurveColor was renamed; update this test.");

            var enabled = new AnimationCurveField { value = AnimationCurve.Linear(0f, 0f, 1f, 1f), style = { width = 120 } };
            var disabled = new AnimationCurveField { value = AnimationCurve.Linear(0f, 0f, 1f, 1f), style = { width = 120 } };
            var host = new VisualElement();
            host.Add(enabled);
            host.Add(disabled);
            disabled.SetEnabled(false);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            var enabledStroke = (Color)k_CurveColorField.GetValue(enabled);
            var disabledStroke = (Color)k_CurveColorField.GetValue(disabled);

            Assert.AreNotEqual(Color.white, enabledStroke,
                "The enabled stroke is still AnimationCurveField's white fallback, so --curve-color never reached it.");
            Assert.AreNotEqual(enabledStroke, disabledStroke,
                "A disabled AnimationCurveField still draws the enabled stroke colour.");
        }
    }
}
