using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards that an emphasized checked or indeterminate <see cref="Checkbox"/> paints its box and
    /// mark from the checked-box component tokens, as the registry's checkbox does, rather than
    /// from raw accent ramp steps.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>--appui-checkbox__box-checked-{background,border,foreground}-color</c> live in
    /// <c>App UI - Components.uss</c> and a theme can override them. A box that reads
    /// <c>--appui-accent-100</c> instead looks right in the themes where the accent alias points at
    /// that step and wrong in Cloud Light and Editor Light, where it does not.
    /// </para>
    /// <para>
    /// UITK's non-emphasized checked box keeps its neutral field look. The registry has no such
    /// variant; it is a deliberate UITK-only divergence recorded in <c>UITK-PARITY.md</c>.
    /// </para>
    /// </remarks>
    [TestFixture]
    [TestOf(typeof(Checkbox))]
    class CheckboxCheckedTokenTests
    {
        const string k_Stylesheet = "Checkbox.uss";

        const string k_CheckedBackground = "--appui-checkbox__box-checked-background-color";
        const string k_CheckedBorder = "--appui-checkbox__box-checked-border-color";
        const string k_CheckedForeground = "--appui-checkbox__box-checked-foreground-color";

        const string k_Box = " > .appui-checkbox__box";
        const string k_EmphasizedChecked = ".appui-checkbox--emphasized.is-checked" + k_Box;
        const string k_EmphasizedIntermediate = ".appui-checkbox--emphasized.is-intermediate" + k_Box;

        static readonly string[] k_EmphasizedBoxes = { k_EmphasizedChecked, k_EmphasizedIntermediate };

        [Test]
        public void EmphasizedCheckedBox_ReadsTheCheckedBoxTokens([ValueSource(nameof(k_EmphasizedBoxes))] string selector)
        {
            var uss = UssSource.Read(k_Stylesheet);
            Assert.AreEqual(k_CheckedBackground, UssTheme.DeclaredToken(uss, selector, "background-color"),
                $"{selector} should fill from the checked-box token.");
            Assert.AreEqual(k_CheckedBorder, UssTheme.DeclaredToken(uss, selector, "border-color"),
                $"{selector} should draw its rim from the checked-box token.");
        }

        [Test]
        public void EmphasizedMark_ReadsTheCheckedForegroundToken()
        {
            var uss = UssSource.Read(k_Stylesheet);
            Assert.AreEqual(k_CheckedForeground,
                UssTheme.DeclaredToken(uss, ".appui-checkbox--emphasized .appui-icon", "--unity-image-tint-color"),
                "The emphasized check and dash should tint from the checked-box foreground token.");
        }

        /// <summary>
        /// The invalid mark sits on the destructive fill, not the checked-box fill, so it must not
        /// follow the checked-box foreground token.
        /// </summary>
        [Test]
        public void InvalidMark_KeepsTheAccentForegroundTint()
        {
            var uss = UssSource.Read(k_Stylesheet);
            foreach (var selector in new[] { ".is-invalid .appui-checkbox .appui-icon", ".is-invalid.appui-checkbox .appui-icon" })
            {
                Assert.AreEqual("--appui-alias-actions-accent-foreground-color-default",
                    UssTheme.DeclaredToken(uss, selector, "--unity-image-tint-color"),
                    $"{selector} should keep the accent foreground tint over its destructive fill.");
            }
            Assert.AreEqual("--appui-destructive-100",
                UssTheme.DeclaredToken(uss, ".appui-checkbox.is-invalid" + k_Box, "background-color"));
        }

        /// <summary>
        /// Disabled boxes paint the field <c>*-disabled</c> tokens, and nothing in the stylesheet
        /// reaches for opacity except the hidden native input.
        /// </summary>
        [Test]
        public void DisabledBox_ReadsTheDisabledFieldTokens_AndNeverOpacity()
        {
            var uss = UssSource.Read(k_Stylesheet);
            foreach (var selector in new[] { ".appui-checkbox:disabled" + k_Box, ".appui-checkbox.is-invalid:disabled" + k_Box })
            {
                Assert.AreEqual("--appui-alias-field-background-color-disabled",
                    UssTheme.DeclaredToken(uss, selector, "background-color"), selector);
                Assert.AreEqual("--appui-alias-field-border-color-disabled",
                    UssTheme.DeclaredToken(uss, selector, "border-color"), selector);
            }

            var dimmed = UssTheme.Rules(uss)
                .Where(r => Regex.IsMatch(r.body, @"(?:^|;|\s)opacity\s*:"))
                .SelectMany(r => r.selectors)
                .Where(s => s != ".appui-checkbox__input")
                .ToList();
            Assert.IsEmpty(dimmed, "These Checkbox rules dim with opacity:\n" + string.Join("\n", dimmed));
        }

        /// <summary>
        /// The neutral (non-emphasized) checked box is UITK-only and keeps the field fill: no rule
        /// paints a plain checked box with the checked-box tokens.
        /// </summary>
        [Test]
        public void NeutralCheckedBox_KeepsTheFieldFill()
        {
            var uss = UssSource.Read(k_Stylesheet);
            foreach (var state in new[] { ".is-checked", ".is-intermediate" })
            {
                Assert.IsNull(UssTheme.DeclaredToken(uss, ".appui-checkbox" + state + k_Box, "background-color"),
                    $"A non-emphasized {state} box should keep the field fill.");
            }
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

        static readonly string[] k_Themes = { "dark", "light", "editor-dark", "editor-light", "cloud-dark", "cloud-light" };

        static readonly CheckboxState[] k_CheckedStates = { CheckboxState.Checked, CheckboxState.Intermediate };

        /// <summary>
        /// The runtime half: mounted under each shipped theme, an emphasized checked box resolves
        /// to whatever that theme gives the checked-box tokens, so a theme override flows through.
        /// </summary>
        [UnityTest]
        public IEnumerator EmphasizedCheckedBox_ResolvesToTheThemesCheckedBoxTokens(
            [ValueSource(nameof(k_Themes))] string theme,
            [ValueSource(nameof(k_CheckedStates))] CheckboxState state)
        {
            var checkbox = new Checkbox { label = "Accept", emphasized = true, value = state };
            yield return Mount(theme, checkbox);

            var vars = PanelVars(theme);
            var box = checkbox.Q(Checkbox.boxUssClassName);
            Assert.IsNotNull(box, "The checkbox box was not found.");

            AssertColour(vars, k_CheckedBackground, box.resolvedStyle.backgroundColor, $"{theme} {state} box fill");
            AssertColour(vars, k_CheckedBorder, box.resolvedStyle.borderTopColor, $"{theme} {state} box rim");

            var markName = state == CheckboxState.Checked ? Checkbox.checkmarkUssClassName : Checkbox.partialCheckmarkUssClassName;
            var mark = checkbox.Q<Icon>(markName);
            Assert.IsNotNull(mark, "The checkbox mark was not found.");
            Assert.AreEqual(DisplayStyle.Flex, mark.resolvedStyle.display, "The mark for this state should be shown.");
            AssertColour(vars, k_CheckedForeground, mark.tintColor, $"{theme} {state} mark tint");
        }

        IEnumerator Mount(string theme, VisualElement element)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = theme, density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);

            for (var i = 0; i < 4; i++)
                yield return null;
        }

        /// <summary>
        /// The vars a panel themed <paramref name="theme"/> sees: the <c>:root</c> declarations the
        /// root <c>App UI.tss</c> carries (the palette, the shared files, then Dark), overlaid by
        /// the theme's class.
        /// </summary>
        static Dictionary<string, string> PanelVars(string theme)
        {
            var vars = new Dictionary<string, string>();
            Overlay(vars, "App UI - Palette.uss");
            foreach (var kv in UssTheme.LoadVars("App UI - Dark.uss"))
                vars[kv.Key] = kv.Value;
            var name = string.Join(" ", theme.Split('-').Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
            Overlay(vars, $"App UI - {name}.class.uss");
            return vars;
        }

        static void Overlay(Dictionary<string, string> vars, string themeFile)
        {
            var path = Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Themes", themeFile);
            Assert.IsTrue(File.Exists(path), $"Missing theme stylesheet {path}.");
            var source = UssTheme.StripComments(File.ReadAllText(path));
            foreach (Match m in Regex.Matches(source, @"(--appui-[A-Za-z0-9_-]+)\s*:\s*([^;]+);"))
                vars[m.Groups[1].Value] = m.Groups[2].Value.Trim();
        }

        static void AssertColour(Dictionary<string, string> vars, string token, Color actual, string what)
        {
            Assert.IsTrue(UssTheme.TryResolveColour(vars, token, out var expected), $"{token} does not resolve ({what}).");
            Assert.IsTrue(UssTheme.SameColour(expected, actual) && Mathf.Abs(expected.a - actual.a) <= 0.02f,
                $"{what}: expected {token} = {(Color32)expected}, got {(Color32)actual}.");
        }
    }
}
