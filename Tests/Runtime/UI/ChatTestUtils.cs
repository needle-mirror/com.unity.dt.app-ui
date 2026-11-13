using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// What every chat display component's tests need: a mounted panel to resolve styles against,
    /// and the ability to read the component's own USS source so the token contract can be pinned.
    /// </summary>
    /// <remarks>
    /// Reading the USS is the only way to defend the token bindings. A layout assertion passes just as
    /// happily against a hardcoded pixel value as against an alias that resolves to the same pixel
    /// today, so the binding itself has to be asserted at the source level.
    /// </remarks>
    static class ChatTestUtils
    {
        const string k_StylesDir = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/";

        /// <summary>
        /// Reads one component stylesheet from the package.
        /// </summary>
        public static string ReadUss(string fileName)
        {
            var path = k_StylesDir + fileName;
            var fullPath = Path.GetFullPath(path);
            return File.ReadAllText(fullPath);
        }

        /// <summary>
        /// The declarations of the first rule whose selector matches the pattern.
        /// </summary>
        /// <remarks>
        /// Matched multiline, so a pattern can anchor with <c>^</c> to mean "a rule whose selector
        /// starts the line". Without that, a bare class pattern also matches the same class appearing
        /// as the right-hand side of a descendant selector further up the file, and the assertion
        /// silently reads the wrong rule.
        /// </remarks>
        public static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}", RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, $"No rule matching {selectorPattern}.");
            return rule.Groups["body"].Value;
        }

        /// <summary>
        /// Asserts a stylesheet carries no literal colour, which is the failure the token contract
        /// exists to prevent.
        /// </summary>
        public static void AssertNoLiteralColours(string fileName)
        {
            var uss = ReadUss(fileName);
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), $"{fileName} contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), $"{fileName} contains a literal rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\bhsla?\("), $"{fileName} contains a literal hsl()/hsla() colour.");
        }

        /// <summary>
        /// Asserts a stylesheet carries no magic pixel value. Only zero and the token vars are allowed,
        /// so every dimension rescales with density.
        /// </summary>
        public static void AssertNoMagicPixels(string fileName)
        {
            var uss = ReadUss(fileName);
            var withoutComments = Regex.Replace(uss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var offender = Regex.Match(withoutComments, @"[:\s]\s*\d+(\.\d+)?px");
            Assert.IsFalse(offender.Success,
                $"{fileName} uses a literal pixel value instead of a token: '{offender.Value.Trim()}'.");
        }

        /// <summary>
        /// Mounts an element inside a themed Panel and pumps enough frames for styles to resolve.
        /// </summary>
        public static IEnumerator Mount(UIDocument document, VisualElement element, string density = "normal")
        {
#if !UNITY_EDITOR
            document.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            document.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            document.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            document.rootVisualElement.Add(panel);
            panel.Add(element);

            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
