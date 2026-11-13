using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Resolves <c>--appui-*</c> tokens through the shipped theme stylesheets, so a test can ask what
    /// colour a token actually lands on in every theme rather than trusting that two different token
    /// names paint two different colours.
    /// </summary>
    /// <remarks>
    /// Two tokens with different names often resolve to the same value. A disabled indicator bound to
    /// a token that equals the fill behind it disappears, and the stylesheet still looks
    /// token-correct. Some tokens carry alpha (the Editor themes' disabled fills are 50% black or
    /// white), so <see cref="Composite"/> lays a colour over the one beneath it before two are
    /// compared.
    /// </remarks>
    static class UssTheme
    {
        // The theme stylesheets resolve their aliases against these shared ones, in the order the
        // generated .tss imports them.
        static readonly string[] k_SharedThemeFiles =
        {
            "App UI - Globals.uss",
            "App UI - Alias.uss",
            "App UI - Components.uss",
        };

        static readonly Regex k_Comment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
        static readonly Regex k_Rule = new Regex(@"([^{}]+)\{([^}]*)\}");
        static readonly Regex k_Declaration = new Regex(@"(--appui-[A-Za-z0-9_-]+)\s*:\s*([^;]+);");
        static readonly Regex k_VarOnly = new Regex(@"^var\(\s*(--appui-[A-Za-z0-9_-]+)\s*\)$");

        /// <summary>
        /// The surfaces a control can sit on.
        /// </summary>
        public static readonly string[] surfaces =
        {
            "--appui-alias-surface-default",
            "--appui-alias-surface-subtle",
            "--appui-alias-surface-sunken",
            "--appui-alias-surface-raised",
            "--appui-alias-surface-overlay",
            "--appui-alias-surface-card",
            "--appui-alias-surface-muted",
        };

        /// <summary>
        /// Removes block comments from a stylesheet.
        /// </summary>
        /// <param name="source"> The stylesheet source. </param>
        /// <returns> The source without comments. </returns>
        public static string StripComments(string source) => k_Comment.Replace(source, "");

        /// <summary>
        /// Every rule in a stylesheet as its selector list and its declaration block.
        /// </summary>
        /// <param name="uss"> The stylesheet source. </param>
        /// <returns> Each rule's selectors, whitespace-normalised, and its body. </returns>
        public static IEnumerable<(string[] selectors, string body)> Rules(string uss)
        {
            foreach (Match rule in k_Rule.Matches(StripComments(uss)))
            {
                var selectors = rule.Groups[1].Value.Split(',')
                    .Select(s => Regex.Replace(s.Trim(), @"\s+", " "))
                    .ToArray();
                yield return (selectors, rule.Groups[2].Value);
            }
        }

        /// <summary>
        /// The token the last rule naming <paramref name="selector"/> assigns to
        /// <paramref name="property"/>, or null when none does.
        /// </summary>
        /// <remarks>
        /// The whole selector has to match one entry of a comma list, so a descendant rule never
        /// stands in for it. Same-specificity rules cascade by source order, so the last one wins.
        /// </remarks>
        /// <param name="uss"> The stylesheet source. </param>
        /// <param name="selector"> The exact selector. </param>
        /// <param name="property"> The property, e.g. <c>background-color</c> or <c>--curve-color</c>. </param>
        /// <returns> The token name, e.g. <c>--appui-alias-text-disabled</c>. </returns>
        public static string DeclaredToken(string uss, string selector, string property)
        {
            var normalised = Regex.Replace(selector.Trim(), @"\s+", " ");
            var pattern = @"(?:^|;|\s)" + Regex.Escape(property)
                + @"\s*:\s*var\(\s*(--appui-[A-Za-z0-9_-]+)\s*\)";
            string token = null;
            foreach (var (selectors, body) in Rules(uss))
            {
                if (!selectors.Contains(normalised))
                    continue;
                var decl = Regex.Match(body, pattern);
                if (decl.Success)
                    token = decl.Groups[1].Value;
            }
            return token;
        }

        /// <summary>
        /// Every theme the package ships, in both its <c>:root</c> and its class-scoped form.
        /// A theme is whatever has a generated <c>.tss</c>.
        /// </summary>
        /// <returns> The theme stylesheet file names. </returns>
        public static List<string> ThemeFiles()
        {
            var themes = new List<string>();
            foreach (var tss in Directory.EnumerateFiles(ThemesDirectory(), "App UI - *.tss").OrderBy(p => p))
            {
                var name = Path.GetFileNameWithoutExtension(tss);
                foreach (var file in new[] { name + ".uss", name + ".class.uss" })
                {
                    if (File.Exists(Path.Combine(ThemesDirectory(), file)))
                        themes.Add(file);
                }
            }
            return themes;
        }

        /// <summary>
        /// Every <c>--appui-*</c> declaration a theme ends up with, shared files first.
        /// </summary>
        /// <param name="themeFile"> The theme stylesheet, e.g. <c>App UI - Dark.uss</c>. </param>
        /// <returns> Token name to raw value. </returns>
        public static Dictionary<string, string> LoadVars(string themeFile)
        {
            var vars = new Dictionary<string, string>();
            foreach (var file in k_SharedThemeFiles.Append(themeFile))
            {
                var source = StripComments(File.ReadAllText(Path.Combine(ThemesDirectory(), file)));
                foreach (Match m in k_Declaration.Matches(source))
                    vars[m.Groups[1].Value] = m.Groups[2].Value.Trim();
            }
            return vars;
        }

        /// <summary>
        /// Follows a token's <c>var()</c> chain to the colour it lands on.
        /// </summary>
        /// <param name="vars"> A theme's declarations, from <see cref="LoadVars"/>. </param>
        /// <param name="token"> The token name. </param>
        /// <param name="colour"> The resolved colour. </param>
        /// <returns> False when the chain breaks or ends on something that is not a colour. </returns>
        public static bool TryResolveColour(Dictionary<string, string> vars, string token, out Color colour)
        {
            colour = default;
            var value = $"var({token})";
            for (var depth = 0; depth < 32; depth++)
            {
                var m = k_VarOnly.Match(value);
                if (!m.Success)
                    return TryParseColour(value, out colour);
                if (!vars.TryGetValue(m.Groups[1].Value, out value))
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Parses <c>#rgb</c>, <c>#rrggbb</c>, <c>#rrggbbaa</c>, <c>rgb()</c> and <c>rgba()</c>.
        /// </summary>
        /// <param name="value"> The raw value. </param>
        /// <param name="colour"> The parsed colour. </param>
        /// <returns> False when the value is not one of those forms. </returns>
        public static bool TryParseColour(string value, out Color colour)
        {
            colour = default;
            value = value.Trim().ToLowerInvariant();
            if (value.StartsWith("#"))
                return ColorUtility.TryParseHtmlString(value, out colour);

            var m = Regex.Match(value, @"^rgba?\(([^)]*)\)$");
            if (!m.Success)
                return false;
            var parts = m.Groups[1].Value.Split(',')
                .Select(p => float.Parse(p.Trim(), CultureInfo.InvariantCulture))
                .ToArray();
            if (parts.Length < 3)
                return false;
            colour = new Color(parts[0] / 255f, parts[1] / 255f, parts[2] / 255f, parts.Length > 3 ? parts[3] : 1f);
            return true;
        }

        /// <summary>
        /// Lays <paramref name="top"/> over an opaque <paramref name="bottom"/>.
        /// </summary>
        /// <param name="top"> The colour painted on top, possibly translucent. </param>
        /// <param name="bottom"> The opaque colour beneath. </param>
        /// <returns> The opaque colour a viewer sees. </returns>
        public static Color Composite(Color top, Color bottom)
        {
            var a = top.a;
            return new Color(
                top.r * a + bottom.r * (1 - a),
                top.g * a + bottom.g * (1 - a),
                top.b * a + bottom.b * (1 - a),
                1f);
        }

        /// <summary>
        /// Whether two opaque colours are indistinguishable: no channel differs by more than
        /// <paramref name="tolerance"/> 8-bit steps.
        /// </summary>
        /// <remarks>
        /// Plain equality is too lenient once alpha is composited: <c>rgba(42, 42, 42, 0.5)</c>
        /// over <c>#282828</c> lands one step away and still reads as the same grey. This is
        /// not a contrast requirement, only "these are the same colour".
        /// </remarks>
        /// <param name="a"> The first colour. </param>
        /// <param name="b"> The second colour. </param>
        /// <param name="tolerance"> The largest per-channel difference still counted as equal. </param>
        /// <returns> True when the two cannot be told apart. </returns>
        public static bool SameColour(Color a, Color b, int tolerance = 3)
        {
            Color32 x = a, y = b;
            return Mathf.Abs(x.r - y.r) <= tolerance
                && Mathf.Abs(x.g - y.g) <= tolerance
                && Mathf.Abs(x.b - y.b) <= tolerance;
        }

        static string ThemesDirectory() => Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Themes");
    }
}
