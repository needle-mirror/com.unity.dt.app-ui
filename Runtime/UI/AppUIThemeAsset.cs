using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A custom App UI theme, imported from a <c>.appuitheme.json</c> file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the main object of the imported asset, with the two compiled stylesheets stored
    /// alongside it as sub-assets. Making the theme — rather than the stylesheet — the main object
    /// keeps <c>AssetDatabase.FindAssets("t:AppUIThemeAsset")</c> a clean way to enumerate the
    /// custom themes in a project, which is what populates the theme dropdowns.
    /// </para>
    /// <para>
    /// The type lives in the runtime assembly even though only the editor ever creates one, so that
    /// player code can hold a serialized reference to a theme and apply it at run time. Were it
    /// editor-only, the usage below would not compile in a build.
    /// </para>
    /// <para>
    /// To use one, hold a reference to the theme — a serialized field assigned in the inspector —
    /// then add its <see cref="styleSheet"/> to a panel and set the theme name:
    /// </para>
    /// <code>
    /// [SerializeField] AppUIThemeAsset m_Theme;
    ///
    /// void Start()
    /// {
    ///     var panel = GetComponent&lt;UIDocument&gt;().rootVisualElement.Q&lt;Panel&gt;();
    ///     panel.styleSheets.Add(m_Theme.styleSheet);
    ///     panel.theme = m_Theme.themeName;
    /// }
    /// </code>
    /// <para>
    /// That layers the theme over whatever App UI theme is already loaded, so any token the custom
    /// theme does not define keeps its value from the base theme rather than resolving to nothing.
    /// Assigning <see cref="themeStyleSheet"/> to <c>PanelSettings.themeStyleSheet</c> instead
    /// replaces the theme file wholesale.
    /// </para>
    /// </remarks>
    public class AppUIThemeAsset : ScriptableObject
    {
        [SerializeField]
        string m_ThemeName;

        [SerializeField]
        string m_DisplayName;

        [SerializeField]
        string m_BaseTheme;

        [SerializeField]
        string m_ColorScheme;

        [SerializeField]
        StyleSheet m_StyleSheet;

        [SerializeField]
        ThemeStyleSheet m_ThemeStyleSheet;

        [SerializeField]
        List<string> m_TokenNames = new List<string>();

        [SerializeField]
        List<string> m_TokenValues = new List<string>();

        /// <summary>
        /// The theme's identifier, which is also the value to assign to <c>Panel.theme</c>. The
        /// generated stylesheet defines <c>.appui--&lt;themeName&gt;</c>.
        /// </summary>
        public string themeName => m_ThemeName;

        /// <summary>A human-readable name for menus and inspectors.</summary>
        public string displayName => string.IsNullOrEmpty(m_DisplayName) ? m_ThemeName : m_DisplayName;

        /// <summary>The App UI theme this one was derived from.</summary>
        public string baseTheme => m_BaseTheme;

        /// <summary>Whether the theme reads as <c>dark</c> or <c>light</c>.</summary>
        public string colorScheme => m_ColorScheme;

        /// <summary>
        /// The class-scoped stylesheet, for <c>element.styleSheets.Add(...)</c>. This is the
        /// recommended way to apply a custom theme.
        /// </summary>
        public StyleSheet styleSheet => m_StyleSheet;

        /// <summary>
        /// A standalone theme file, assignable to <c>PanelSettings.themeStyleSheet</c>. Null when
        /// the USS could not be compiled in this Unity version.
        /// </summary>
        public ThemeStyleSheet themeStyleSheet => m_ThemeStyleSheet;

        /// <summary>How many tokens the theme defines.</summary>
        public int tokenCount => m_TokenNames.Count;

        /// <summary>
        /// The theme's tokens: App UI tokens keyed without the <c>--appui-</c> prefix, and any other
        /// custom property the theme carries under its full <c>--name</c>.
        /// </summary>
        /// <remarks>
        /// Stored as two parallel lists because Unity cannot serialise a dictionary; rebuilt on
        /// demand rather than cached, since this is only for inspectors and tooling.
        /// </remarks>
        public IReadOnlyDictionary<string, string> tokens
        {
            get
            {
                var map = new Dictionary<string, string>(m_TokenNames.Count);
                for (var i = 0; i < m_TokenNames.Count && i < m_TokenValues.Count; i++)
                    map[m_TokenNames[i]] = m_TokenValues[i];
                return map;
            }
        }

        internal void SetMetadata(string themeName, string displayName, string baseTheme, string colorScheme)
        {
            m_ThemeName = themeName;
            m_DisplayName = displayName;
            m_BaseTheme = baseTheme;
            m_ColorScheme = colorScheme;
        }

        internal void SetTokens(IReadOnlyDictionary<string, string> tokens)
        {
            m_TokenNames.Clear();
            m_TokenValues.Clear();
            foreach (var pair in tokens)
            {
                m_TokenNames.Add(pair.Key);
                m_TokenValues.Add(pair.Value);
            }
        }

        internal void SetStyleSheets(StyleSheet styleSheet, ThemeStyleSheet themeStyleSheet)
        {
            m_StyleSheet = styleSheet;
            m_ThemeStyleSheet = themeStyleSheet;
        }
    }
}
