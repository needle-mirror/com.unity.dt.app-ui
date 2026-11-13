using System.Collections.Generic;
using UnityEditor;
using Unity.AppUI.UI;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// The themes available in a project: App UI's own, plus any imported
    /// <see cref="AppUIThemeAsset"/>.
    /// </summary>
    /// <remarks>
    /// Discovery is by asset search rather than by inspecting loaded stylesheets. A
    /// <c>StyleSheet</c>'s selector tables are internal, so there is no supported way to ask one
    /// which <c>.appui--*</c> rules it defines; the imported theme assets are the reliable record.
    /// A theme written by hand in a <c>.uss</c> file will therefore not appear here — the dropdown
    /// keeps any value already set, so such a theme still survives being edited in the inspector.
    /// </remarks>
    static class ThemeRegistry
    {
        static List<string> s_Cached;
        static Dictionary<string, string> s_DisplayNames;

        static ThemeRegistry()
        {
            // The asset search is far too expensive to run on every inspector repaint, so the list
            // is cached and invalidated when the project changes.
            EditorApplication.projectChanged += Invalidate;
        }

        /// <summary>Discards the cached theme list, so the next query rescans the project.</summary>
        public static void Invalidate()
        {
            s_Cached = null;
            s_DisplayNames = null;
        }

        /// <summary>
        /// Every selectable theme name: App UI's six, then any custom ones, in a fresh list the
        /// caller may modify.
        /// </summary>
        public static List<string> GetThemes()
        {
            Rebuild();
            return new List<string>(s_Cached);
        }

        /// <summary>A human-readable label for a theme name.</summary>
        public static string GetDisplayName(string theme)
        {
            Rebuild();
            if (s_DisplayNames.TryGetValue(theme, out var name) && !string.IsNullOrEmpty(name))
                return name;
            return Titleize(theme);
        }

        static void Rebuild()
        {
            if (s_Cached != null)
                return;

            s_Cached = new List<string>(AppUIThemeUssWriter.builtInThemes);
            s_DisplayNames = new Dictionary<string, string>();

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(AppUIThemeAsset)}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(path);
                if (asset == null || string.IsNullOrEmpty(asset.themeName))
                    continue;

                if (!s_Cached.Contains(asset.themeName))
                    s_Cached.Add(asset.themeName);
                s_DisplayNames[asset.themeName] = asset.displayName;
            }
        }

        /// <summary>
        /// "editor-dark" to "Editor Dark". The general-purpose Capitalize helper only touches the
        /// first character, which leaves the hyphenated built-in themes reading as "Editor-dark".
        /// </summary>
        static string Titleize(string theme)
        {
            var parts = theme.Split('-');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            }

            return string.Join(" ", parts);
        }
    }
}
