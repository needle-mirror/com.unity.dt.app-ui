using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Reads component stylesheets off disk so a test can assert against the USS *source* rather
    /// than against resolved layout.
    /// </summary>
    /// <remarks>
    /// Two things force this. UITK only surfaces a custom property that a matched rule declares on
    /// the element, so a <c>:root</c>-level alias var cannot be read back through
    /// <c>CustomStyleResolvedEvent</c>. And a control that grabs a raw <c>--appui-sizing-NNN</c>
    /// step instead of the control-sizing alias resolves to identical pixels today, so no layout
    /// assertion can tell the two apart — only the source can.
    ///
    /// Every slice so far has rolled its own copy of these two helpers. This is the shared one.
    /// </remarks>
    static class UssSource
    {
        const string k_RelativeDir = "upm-packages/com.unity.dt.app-ui/PackageResources/Styles/Components";

        /// <summary>
        /// Reads a stylesheet out of the package's Components folder.
        /// </summary>
        /// <param name="fileName"> File name, e.g. <c>AnimationCurveField.uss</c>. </param>
        /// <returns> The file contents. Fails the test if the file cannot be located. </returns>
        public static string Read(string fileName)
        {
            var dir = new DirectoryInfo(Application.dataPath);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, k_RelativeDir, fileName);
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
        /// Reads a stylesheet out of the package's Styles folder, one level above Components.
        /// </summary>
        /// <remarks>
        /// Needed to assert a component stylesheet is actually <c>@import</c>ed by
        /// <c>App UI.uss</c>. A stylesheet nobody imports is never loaded, and the component then
        /// renders unstyled without any error.
        /// </remarks>
        /// <param name="fileName"> File name, e.g. <c>App UI.uss</c>. </param>
        /// <returns> The file contents. Fails the test if the file cannot be located. </returns>
        public static string ReadStylesheet(string fileName)
        {
            const string relativeDir = "upm-packages/com.unity.dt.app-ui/PackageResources/Styles";

            var dir = new DirectoryInfo(Application.dataPath);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, relativeDir, fileName);
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);
                dir = dir.Parent;
            }

            var packaged = Path.GetFullPath(
                Path.Combine("Packages/com.unity.dt.app-ui/PackageResources/Styles", fileName));
            if (File.Exists(packaged))
                return File.ReadAllText(packaged);

            Assert.Fail($"Could not locate {fileName} from {Application.dataPath}.");
            return null;
        }

        /// <summary>
        /// Locates the package's <c>PackageResources</c> folder on disk.
        /// </summary>
        /// <returns> The absolute folder path. Fails the test if it cannot be located. </returns>
        public static string PackageResourcesDirectory()
        {
            const string relativeDir = "upm-packages/com.unity.dt.app-ui/PackageResources";

            var dir = new DirectoryInfo(Application.dataPath);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, relativeDir);
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }

            var packaged = Path.GetFullPath("Packages/com.unity.dt.app-ui/PackageResources");
            if (Directory.Exists(packaged))
                return packaged;

            Assert.Fail($"Could not locate PackageResources from {Application.dataPath}.");
            return null;
        }

        /// <summary>
        /// Returns the declaration block of the rule whose selector list *starts a line* with
        /// <paramref name="selector"/>, or null when there is none.
        /// </summary>
        /// <remarks>
        /// Anchored to the start of a line on purpose. An unanchored <c>\.appui-thing\s*\{</c>
        /// also matches <c>.appui-other .appui-thing {</c> further up the file, so the assertion
        /// silently checks a descendant rule belonging to someone else and passes.
        /// </remarks>
        /// <param name="uss"> The stylesheet source. </param>
        /// <param name="selector"> The exact selector to find, including the leading dot. </param>
        /// <returns> The rule body, without braces. </returns>
        public static string RuleBlock(string uss, string selector)
        {
            var pattern = @"^\s*" + Regex.Escape(selector) + @"(?![\w-])[^{}]*\{([^}]*)\}";
            var match = Regex.Match(uss, pattern, RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// Asserts a control rule takes its height from the control-sizing alias for every size
        /// step, and never from a raw sizing step or a literal px.
        /// </summary>
        /// <param name="fileName"> The stylesheet to read. </param>
        /// <param name="baseSelector"> The component's main class, e.g. <c>.appui-animationcurvefield</c>. </param>
        public static void AssertHeightsComeFromControlSizingAliases(string fileName, string baseSelector)
        {
            var uss = Read(fileName);

            foreach (var suffix in new[] { "sm", "md", "lg" })
            {
                var selector = baseSelector + "--size-" + suffix;
                var block = RuleBlock(uss, selector);
                Assert.IsNotNull(block, $"Expected a {selector} rule in {fileName}.");

                StringAssert.Contains(
                    $"height: var(--appui-alias-control-sizing-{suffix}-height)", block,
                    $"{selector} must take its height from the control-sizing alias.");

                Assert.IsFalse(block.Contains("--appui-sizing-"),
                    $"{selector} reaches for a raw --appui-sizing-NNN step. Control heights must go "
                    + "through var(--appui-alias-control-sizing-*-height), otherwise a default "
                    + "control silently renders one size too small.");

                Assert.IsFalse(Regex.IsMatch(block, @"\d+px"),
                    $"{selector} hardcodes a pixel height.");
            }
        }

        /// <summary>
        /// Asserts a rule takes its left and right padding from one horizontal-inset token, and
        /// never from a raw sizing step or a literal px.
        /// </summary>
        /// <remarks>
        /// The three field insets all alias <c>--appui-sizing-200</c> today, and the old
        /// <c>--appui-alias-control-gap-md</c> resolves to a near neighbour, so a rendered width can
        /// tell neither which size variant a rule chose nor whether it reads the inset at all.
        /// Only the source can.
        /// </remarks>
        /// <param name="fileName"> The stylesheet to read. </param>
        /// <param name="selector"> The exact selector that starts the rule. </param>
        /// <param name="token"> The custom property both sides must read, e.g. <c>--appui-alias-field-padding-horizontal-md</c>. </param>
        public static void AssertHorizontalInset(string fileName, string selector, string token)
        {
            var block = RuleBlock(Read(fileName), selector);
            Assert.IsNotNull(block, $"Expected a {selector} rule in {fileName}.");

            StringAssert.Contains($"padding-left: var({token});", block,
                $"{selector} must take its leading inset from {token}.");
            StringAssert.Contains($"padding-right: var({token});", block,
                $"{selector} must take its trailing inset from {token}.");

            Assert.IsFalse(block.Contains("--appui-sizing-"),
                $"{selector} reaches for a raw --appui-sizing-NNN step for its inset.");
            Assert.IsFalse(Regex.IsMatch(block, @"\d+px"),
                $"{selector} hardcodes a pixel inset.");
        }

        /// <summary>
        /// Asserts a stylesheet contains no literal colors.
        /// </summary>
        /// <remarks>
        /// Fully transparent black is plain "no paint" rather than a design color, so it is
        /// allowed — the same carve-out TextField's own guard makes.
        /// </remarks>
        /// <param name="fileName"> The stylesheet to read. </param>
        public static void AssertNoLiteralColors(string fileName)
        {
            var uss = Read(fileName);
            var literals = Regex.Matches(uss, @"#[0-9a-fA-F]{3,8}\b|\brgba?\([^)]*\)|\bhsla?\([^)]*\)")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(v => !Regex.IsMatch(v, @"^rgba\(\s*0\s*,\s*0\s*,\s*0\s*,\s*0\s*\)$"))
                .ToList();

            Assert.AreEqual(0, literals.Count,
                $"{fileName} must not hardcode colors: " + string.Join(", ", literals));
        }
    }
}
