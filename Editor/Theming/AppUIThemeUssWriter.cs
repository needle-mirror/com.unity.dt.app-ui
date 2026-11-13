using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// Turns a theme's token map into the USS the runtime consumes.
    /// </summary>
    static class AppUIThemeUssWriter
    {
        /// <summary>The class prefix the App UI context providers switch on.</summary>
        public const string themeClassPrefix = "appui--";

        /// <summary>The themes App UI ships, which a custom theme may not shadow by accident.</summary>
        public static readonly string[] builtInThemes =
        {
            "dark", "light", "editor-dark", "editor-light", "cloud-dark", "cloud-light",
        };

        // A CSS identifier: letters, digits, hyphen and underscore, not starting with a digit, and
        // not a hyphen followed by a digit. The name becomes a `.appui--<name>` selector, so
        // anything else produces a stylesheet that silently fails to match.
        static readonly Regex k_ValidName = new Regex(@"^-?[A-Za-z_][A-Za-z0-9_-]*$", RegexOptions.Compiled);

        static readonly Regex k_ValidColor = new Regex(
            @"^(#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})|rgba?\([^)]*\)|hsla?\([^)]*\))$",
            RegexOptions.Compiled);

        // A token name becomes the suffix of a custom property, so it is written into the generated
        // stylesheet verbatim. Anything outside this grammar could close the theme's rule and open
        // another: a key of "accent-100: #000; } VisualElement { background-color" turns an import
        // into a stylesheet that restyles the whole application.
        static readonly Regex k_ValidTokenName = new Regex(
            @"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

        // A consumer's own custom property (`--grain-row-h`), written under its full name. The same
        // concern applies: nothing that could end the declaration.
        static readonly Regex k_ValidCustomProperty = new Regex(
            @"^--[A-Za-z_][A-Za-z0-9_-]*$", RegexOptions.Compiled);

        // The non-colour values a theme writes: a number or length (a count, a metric resolved by
        // the builder), or a bare reference such as `var(--appui-sizing-500)`, which is how a
        // density-relative metric reaches USS, a language without calc().
        static readonly Regex k_ValidLength = new Regex(
            @"^-?\d+(\.\d+)?(px|%)?$", RegexOptions.Compiled);

        static readonly Regex k_ValidReference = new Regex(
            @"^var\(--[A-Za-z_][A-Za-z0-9_-]*\)$", RegexOptions.Compiled);

        /// <summary>Whether <paramref name="name"/> can be used as a theme name.</summary>
        public static bool IsValidThemeName(string name)
        {
            return !string.IsNullOrEmpty(name) && k_ValidName.IsMatch(name);
        }

        /// <summary>Whether <paramref name="value"/> is a colour literal USS will accept.</summary>
        public static bool IsValidColor(string value)
        {
            return !string.IsNullOrEmpty(value) && k_ValidColor.IsMatch(value.Trim());
        }

        /// <summary>
        /// Whether <paramref name="value"/> is a value this writer puts into a stylesheet: a colour,
        /// a number or length, or a single <c>var()</c> reference.
        /// </summary>
        public static bool IsValidValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            var trimmed = value.Trim();
            return k_ValidColor.IsMatch(trimmed) || k_ValidLength.IsMatch(trimmed) ||
                k_ValidReference.IsMatch(trimmed);
        }

        /// <summary>
        /// Whether <paramref name="name"/> is a token name we are willing to write into a
        /// stylesheet: an App UI token's name without its <c>--appui-</c> prefix (lower-case
        /// alphanumeric words separated by single hyphens), or a consumer's own custom property
        /// under its full <c>--name</c>. See the note on <see cref="k_ValidTokenName"/> for why
        /// this is checked.
        /// </summary>
        public static bool IsValidTokenName(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                (k_ValidTokenName.IsMatch(name) || k_ValidCustomProperty.IsMatch(name));
        }

        /// <summary>The custom property a token name is written as.</summary>
        public static string PropertyNameOf(string name)
        {
            return name.StartsWith("--", System.StringComparison.Ordinal) ? name : "--appui-" + name;
        }

        /// <summary>Whether <paramref name="name"/> is one of App UI's own themes.</summary>
        public static bool IsBuiltInTheme(string name)
        {
            return System.Array.IndexOf(builtInThemes, name) >= 0;
        }

        /// <summary>
        /// Writes the theme's tokens as a single rule.
        /// </summary>
        /// <param name="themeName">Used to build the <c>.appui--&lt;name&gt;</c> selector.</param>
        /// <param name="tokens">
        /// App UI token names without the <c>--appui-</c> prefix, and consumer custom properties
        /// under their full <c>--name</c>.
        /// </param>
        /// <param name="rootScoped">
        /// When true the rule is scoped to <c>:root</c>, making the theme the unconditional default
        /// rather than one selectable option.
        /// </param>
        public static string WriteStyleSheet(
            string themeName,
            IReadOnlyDictionary<string, string> tokens,
            bool rootScoped = false)
        {
            var builder = new StringBuilder(2048);
            Line(builder, "/**");
            Line(builder, " * Do not edit directly, this file was auto-generated.");
            Line(builder, " * Generated from a .appuitheme.json by the App UI theme importer.");
            Line(builder, " */");
            Line(builder);
            Line(builder, (rootScoped ? ":root" : $".{themeClassPrefix}{themeName}") + " {");

            // Sorted so the generated sheet is stable: the token map comes from a JSON object, and
            // dictionary order is not something to hang a diff on.
            var names = new List<string>(tokens.Keys);
            names.Sort(System.StringComparer.Ordinal);

            foreach (var name in names)
            {
                var value = tokens[name];
                if (string.IsNullOrEmpty(value))
                    continue;

                // The importer already rejects these with an error the user can see. Refusing them
                // here as well means no caller can produce a stylesheet that escapes its own rule,
                // whatever route the tokens arrived by.
                if (!IsValidTokenName(name) || !IsValidValue(value))
                    continue;

                builder.Append("  ").Append(PropertyNameOf(name)).Append(": ").Append(value.Trim());
                Line(builder, ";");
            }

            Line(builder, "}");
            return builder.ToString();
        }

        /// <summary>
        /// Appends <paramref name="text"/> and a <c>\n</c>. <c>StringBuilder.AppendLine</c> would
        /// write <c>Environment.NewLine</c>, so the same theme would generate different bytes on
        /// Windows than on macOS and Linux.
        /// </summary>
        static void Line(StringBuilder builder, string text = "") => builder.Append(text).Append('\n');

        /// <summary>
        /// Writes the theme file assignable to <c>PanelSettings.themeStyleSheet</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The import order mirrors the per-theme assets App UI ships: the Unity default theme,
        /// then globals, aliases and component styles, and the theme's own tokens last so they win.
        /// The trailing empty rule is required — Unity's theme importer needs at least one rule to
        /// emit a valid <c>ThemeStyleSheet</c>.
        /// </para>
        /// <para>
        /// The tokens are written twice, under <c>:root</c> and under the theme's class. The
        /// <c>:root</c> copy is what makes this file work on its own: a freshly constructed
        /// <c>Panel</c> starts on the <c>appui--dark</c> class, and this file deliberately does not
        /// import App UI's dark theme — so with only a class-scoped rule, a project that assigned
        /// this file and nothing else would get the alias layer with no ramp underneath it and
        /// render unstyled. The class-scoped copy is kept alongside so that setting
        /// <c>Panel.theme</c> to this theme, or scoping it to a subtree with a
        /// <c>ContextProvider</c>, still works.
        /// </para>
        /// <para>
        /// The density sheets are imported for the same reason the package's own <c>App UI.tss</c>
        /// imports them: they carry the <c>.appui--compact</c> and <c>.appui--comfort</c> sizing and
        /// typography overrides. Density is orthogonal to colour, so leaving them out would let a
        /// custom theme silently disable density switching for any panel using it.
        /// </para>
        /// </remarks>
        public static string WriteThemeStyleSheet(
            string themeName,
            IReadOnlyDictionary<string, string> tokens)
        {
            const string styles = "/Packages/com.unity.dt.app-ui/PackageResources/Styles";
            var builder = new StringBuilder(4096);
            Line(builder, "@import url(\"unity-theme://default\");");
            Line(builder, $"@import url(\"{styles}/Themes/App UI - Globals.uss\");");
            Line(builder);
            Line(builder, $"@import url(\"{styles}/Themes/App UI - Alias.uss\");");
            Line(builder);
            Line(builder, $"@import url(\"{styles}/App UI.uss\");");
            Line(builder);
            Line(builder, $"@import url(\"{styles}/Themes/App UI - Normal.class.uss\");");
            Line(builder, $"@import url(\"{styles}/Themes/App UI - Compact.class.uss\");");
            Line(builder, $"@import url(\"{styles}/Themes/App UI - Comfort.class.uss\");");
            Line(builder);
            Line(builder, WriteStyleSheet(themeName, tokens, rootScoped: true));
            Line(builder, WriteStyleSheet(themeName, tokens));
            Line(builder);
            Line(builder, "VisualElement {}");
            return builder.ToString();
        }
    }
}
