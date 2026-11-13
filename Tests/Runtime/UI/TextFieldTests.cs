using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextField))]
    class TextFieldTests : VisualElementTests<TextField>
    {
        protected override string mainUssClassName => TextField.ussClassName;

        // Handed over to TextFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new TextField());
                yield return new Story("Sizes", (ctx) =>
                {
                    var container = new VisualElement();
                    foreach (var size in new[] { Size.Sm, Size.Md, Size.Lg })
                        container.Add(new TextField { size = size, placeholder = "Enter a text..." });
                    return container;
                });
                yield return new Story("Suffix", (ctx) => new TextField("128") { suffix = "px" });
                yield return new Story("Adorned", (ctx) => new TextField
                {
                    leadingIconName = "magnifying-glass",
                    trailingIconName = "x",
                    placeholder = "Search...",
                });
                yield return new Story("Invalid", (ctx) => new TextField("nope")
                {
                    validateValue = v => !string.IsNullOrEmpty(v) && v.Contains("@"),
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TextField />",
            @"<appui:TextField size=""Sm"" />",
            @"<appui:TextField size=""Md"" />",
            @"<appui:TextField size=""Lg"" />",
            @"<appui:TextField placeholder=""Enter a text..."" suffix=""px"" />",
            @"<appui:TextField leading-icon-name=""info"" trailing-icon-name=""x"" />",
            @"<appui:TextField is-password=""true"" mask-char=""*"" max-length=""20"" />",
            @"<appui:TextField select-all-on-focus=""false"" is-read-only=""true"" />",
        };

        static UnityEngine.UIElements.TextField GetInputField(TextField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName);

        static LocalizedTextElement GetSuffix(TextField field) =>
            field.Q<LocalizedTextElement>(className: TextField.suffixUssClassName);

        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };

        // ---------------------------------------------------------------- size

        [Test]
        public void Size_DefaultsToMd()
        {
            var field = new TextField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(TextField.sizeUssClassName + "md"),
                "A freshly built TextField must carry the md size class.");
        }

        [Test]
        public void Size_AppliesTheMatchingUssClassAndRemovesTheStaleOne()
        {
            var field = new TextField();

            foreach (var (size, suffix) in k_Sizes)
            {
                field.size = size;
                Assert.AreEqual(size, field.size);
                Assert.IsTrue(field.ClassListContains(TextField.sizeUssClassName + suffix),
                    $"Expected the {suffix} size class after setting size to {size}.");

                foreach (var (_, other) in k_Sizes)
                {
                    if (other == suffix)
                        continue;
                    Assert.IsFalse(field.ClassListContains(TextField.sizeUssClassName + other),
                        $"Stale size class {other} survived a switch to {suffix}.");
                }
            }
        }

        /// <summary>
        /// Guards the control-sizing contract: heights must come from
        /// <c>--appui-alias-control-sizing-{sm,md,lg}-height</c> and never from a raw
        /// <c>--appui-sizing-NNN</c> step. Grabbing a raw step renders a control one size
        /// too small and nothing errors, because sm is sizing-400 and md is sizing-500,
        /// so no rendered-layout assertion would notice. The rule is therefore checked
        /// against the stylesheet source itself.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            var uss = ReadComponentStyleSheet("TextField.uss");

            foreach (var (_, suffix) in k_Sizes)
            {
                var selector = ".appui-textfield--size-" + suffix;
                var block = ReadRuleBlock(uss, selector);
                Assert.IsNotNull(block, $"Expected a {selector} rule in TextField.uss.");

                StringAssert.Contains(
                    $"height: var(--appui-alias-control-sizing-{suffix}-height)", block,
                    $"{selector} must take its height from the control-sizing alias.");

                Assert.IsFalse(block.Contains("--appui-sizing-"),
                    $"{selector} reaches for a raw --appui-sizing-NNN step. Control heights must "
                    + "go through var(--appui-alias-control-sizing-*-height), otherwise a default "
                    + "control silently renders one size too small.");
            }
        }

        /// <summary>
        /// The field's rest, hover, focus, invalid and disabled skins must come from the field
        /// and control aliases, never from a literal color.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void StateColors_ComeFromTokens_NotLiterals()
        {
            var uss = ReadComponentStyleSheet("TextField.uss");

            foreach (var state in new[] { "default", "hover", "focus", "active", "disabled" })
            {
                StringAssert.Contains($"var(--appui-alias-field-background-color-{state})", uss,
                    $"The {state} background must come from the field alias.");
                StringAssert.Contains($"var(--appui-alias-field-border-color-{state})", uss,
                    $"The {state} border must come from the field alias.");
            }

            StringAssert.Contains("var(--appui-alias-field-border-color-invalid)", uss);
            StringAssert.Contains("var(--appui-alias-control-outline-color)", uss);

            // Fully transparent is plain "no paint", not a design color, so it is allowed.
            var literals = Regex.Matches(uss, @"#[0-9a-fA-F]{3,8}\b|\brgba?\([^)]*\)|\bhsla?\([^)]*\)")
                .Select(m => m.Value)
                .Where(v => !Regex.IsMatch(v, @"^rgba\(\s*0\s*,\s*0\s*,\s*0\s*,\s*0\s*\)$"))
                .ToList();
            Assert.AreEqual(0, literals.Count,
                "TextField.uss must not hardcode colors: " + string.Join(", ", literals));
        }

        static string ReadComponentStyleSheet(string fileName)
        {
            const string relative = "upm-packages/com.unity.dt.app-ui/PackageResources/Styles/Components";
            var dir = new DirectoryInfo(Application.dataPath);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, relative, fileName);
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);
                dir = dir.Parent;
            }

            var packaged = Path.GetFullPath(
                Path.Combine("Packages/com.unity.dt.app-ui/PackageResources/Styles/Components", fileName));
            if (File.Exists(packaged))
                return File.ReadAllText(packaged);

            Assert.Fail($"Could not locate {fileName} from {Application.dataPath}.");
            return null;
        }

        /// <summary>
        /// Returns the declaration block of the first rule whose selector list contains
        /// <paramref name="selector"/> exactly, or null when there is none.
        /// </summary>
        static string ReadRuleBlock(string uss, string selector)
        {
            var pattern = @"(?<![\w-])" + Regex.Escape(selector) + @"(?![\w-])[^{}]*\{([^}]*)\}";
            var match = Regex.Match(uss, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        // -------------------------------------------------------------- suffix

        [Test]
        public void Suffix_IsEmptyAndHiddenByDefault()
        {
            var field = new TextField();
            var suffix = GetSuffix(field);
            Assert.IsNotNull(suffix, "Expected a suffix element in the TextField hierarchy.");
            Assert.IsTrue(string.IsNullOrEmpty(field.suffix));
            Assert.IsTrue(suffix.ClassListContains(Styles.hiddenUssClassName),
                "An empty suffix must be hidden so it costs no layout space.");
        }

        [Test]
        public void Suffix_SetAndCleared_TogglesVisibility()
        {
            var field = new TextField { suffix = "px" };
            var suffix = GetSuffix(field);

            Assert.AreEqual("px", field.suffix);
            Assert.AreEqual("px", suffix.text);
            Assert.IsFalse(suffix.ClassListContains(Styles.hiddenUssClassName));

            field.suffix = null;
            Assert.IsTrue(string.IsNullOrEmpty(field.suffix));
            Assert.IsTrue(suffix.ClassListContains(Styles.hiddenUssClassName));
        }

        [UnityTest]
        public IEnumerator Suffix_SitsBetweenTheInputAndTheTrailingContainer()
        {
            var field = new TextField { suffix = "px", trailingIconName = "x" };
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var suffix = GetSuffix(field);
            var inputContainer = field.Q(className: TextField.inputContainerUssClassName);
            var trailingContainer = field.Q(className: TextField.trailingContainerUssClassName);

            var inputIndex = field.hierarchy.IndexOf(inputContainer);
            var suffixIndex = field.hierarchy.IndexOf(suffix);
            var trailingIndex = field.hierarchy.IndexOf(trailingContainer);

            Assert.Greater(suffixIndex, inputIndex, "The suffix must follow the input.");
            Assert.Less(suffixIndex, trailingIndex, "The suffix must precede the trailing element.");
        }

        // ------------------------------------------------------ selectAllOnFocus

        [Test]
        public void SelectAllOnFocus_DefaultsToTrueAndReachesTheInputField()
        {
            var field = new TextField();
            Assert.IsTrue(field.selectAllOnFocus,
                "Focusing a field should select its value so typing replaces it.");
            Assert.IsTrue(GetInputField(field).textSelection.selectAllOnFocus,
                "The default must survive the RuntimeContextMenu wiring, which disables it.");
        }

        [Test]
        public void SelectAllOnFocus_Toggled_ReachesTheInputField()
        {
            var field = new TextField();
            var input = GetInputField(field);

            field.selectAllOnFocus = false;
            Assert.IsFalse(field.selectAllOnFocus);
            Assert.IsFalse(input.textSelection.selectAllOnFocus);

            field.selectAllOnFocus = true;
            Assert.IsTrue(field.selectAllOnFocus);
            Assert.IsTrue(input.textSelection.selectAllOnFocus);
        }

        // ------------------------------------------------------------ validation

        [Test]
        public void ValidateValue_DrivesTheInvalidState()
        {
            var field = new TextField("nope")
            {
                validateValue = v => v != null && v.Contains("@"),
            };
            Assert.IsTrue(field.invalid);
            Assert.IsTrue(field.ClassListContains(Styles.invalidUssClassName));

            field.value = "a@b.com";
            Assert.IsFalse(field.invalid);
            Assert.IsFalse(field.ClassListContains(Styles.invalidUssClassName));
        }

        // ------------------------------------------------------- adornments

        [Test]
        public void IconNames_ToggleTheirContainersVisibility()
        {
            var field = new TextField();
            var leading = field.Q(className: TextField.leadingContainerUssClassName);
            var trailing = field.Q(className: TextField.trailingContainerUssClassName);

            Assert.IsTrue(leading.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(trailing.ClassListContains(Styles.hiddenUssClassName));

            field.leadingIconName = "info";
            field.trailingIconName = "x";
            Assert.IsFalse(leading.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(trailing.ClassListContains(Styles.hiddenUssClassName));
            Assert.AreEqual("info", field.leadingIconName);
            Assert.AreEqual("x", field.trailingIconName);
        }

        // -------------------------------------------------------------- password

        [Test]
        public void IsPassword_SetWhileDetached_DoesNotThrow()
        {
            // Enabling UITK's isPasswordField forces multiline off, which walks text backing
            // that only exists once the field belongs to a panel. Setting isPassword before
            // attaching used to throw a NullReferenceException from deep inside UITK.
            TextField field = null;
            Assert.DoesNotThrow(() => field = new TextField { isPassword = true });
            Assert.IsTrue(field.isPassword);

            Assert.DoesNotThrow(() => field.isPassword = false);
            Assert.IsFalse(field.isPassword);
        }

        [Test]
        public void IsPassword_SetInObjectInitializerWithOtherProperties_DoesNotThrow()
        {
            // Mirrors how the VisualDoc demos build a password field.
            TextField field = null;
            Assert.DoesNotThrow(() => field = new TextField
            {
                placeholder = "Enter password",
                isPassword = true,
                leadingIconName = "gear",
            });
            Assert.IsTrue(field.isPassword);
        }

        [UnityTest]
        public IEnumerator IsPassword_SetWhileDetached_IsAppliedOnceAttached()
        {
            var field = new TextField { isPassword = true };
            var input = GetInputField(field);
            Assert.IsNotNull(input, "Expected to find the underlying UITK TextField.");
            Assert.IsFalse(input.isPasswordField, "UITK field should be left untouched while detached.");

            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            Assert.IsTrue(input.isPasswordField, "isPassword should reach the UITK field on attach.");
            Assert.IsTrue(field.isPassword);
        }

        [UnityTest]
        public IEnumerator IsPassword_ToggledWhileAttached_UpdatesInputField()
        {
            var field = new TextField();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var input = GetInputField(field);
            Assert.IsNotNull(input);
            Assert.IsFalse(input.isPasswordField);

            field.isPassword = true;
            Assert.IsTrue(input.isPasswordField);

            field.isPassword = false;
            Assert.IsFalse(input.isPasswordField);
        }

        // ------------------------------------------------------- insets and text

        /// <summary>
        /// The registry's <c>text-field</c> reads <c>--appui-alias-field-padding-horizontal-{sm,md,lg}</c>
        /// by size (#1395). The three aliases resolve to the same step today, so only the source can
        /// tell a size that reads the wrong variant.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_ComesFromTheFieldPaddingAlias_ForEverySize()
        {
            foreach (var (_, suffix) in k_Sizes)
                UssSource.AssertHorizontalInset("TextField.uss", ".appui-textfield--size-" + suffix,
                    "--appui-alias-field-padding-horizontal-" + suffix);

            var root = UssSource.RuleBlock(UssSource.Read("TextField.uss"), ".appui-textfield");
            StringAssert.DoesNotContain("control-gap-md", root,
                "The base rule must not keep the old control-gap inset; it would win wherever a size rule does not set one.");
        }

        /// <summary>
        /// The registry's <c>text-field</c> reads <c>text-sm</c> at <c>sm</c> and <c>text-base</c> at
        /// <c>default</c> and <c>lg</c>, for the value and the placeholder alike. Its suffix is
        /// <c>text-sm</c> at every size.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, TextField filled, TextField empty)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in k_Sizes)
                    {
                        var filled = new TextField("Ag") { size = size, suffix = "px" };
                        var empty = new TextField { size = size, placeholder = "Ag" };
                        panel.Add(filled);
                        panel.Add(empty);
                        fields.Add((size, filled, empty));
                    }
                }, s => stage = s);

                foreach (var (size, filled, empty) in fields)
                {
                    var tier = FieldTextSize.TextFieldTier(size);
                    var value = GetInputField(filled).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, value, tier, $"a {size} TextField's value");
                    FieldTextSize.AssertTier(stage, empty.Q(className: TextField.placeholderUssClassName), tier,
                        $"a {size} TextField's placeholder");
                    FieldTextSize.AssertTier(stage, GetSuffix(filled), "sm", $"a {size} TextField's suffix");
                    FieldTextSize.AssertFits(stage, value, filled, $"a {size} TextField's value");
                }
            }
        }

        /// <summary>
        /// <c>body-lg</c> and <c>body-md</c> resolve to the same pixels as <c>font-sizes-200</c> and
        /// <c>-100</c>, so only the source can tell which a rule reads.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in k_Sizes)
            {
                var step = FieldTextSize.Step(FieldTextSize.TextFieldTier(size));
                FieldTextSize.AssertFontSize("TextField.uss",
                    $".appui-textfield--size-{suffix} .appui-textfield__input", step);
            }
            FieldTextSize.AssertFontSize("TextField.uss", ".appui-textfield__suffix", 100);
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("TextField.uss"),
                "TextField reads the numbered steps the registry reads, never a body-* alias.");
        }
    }
}
