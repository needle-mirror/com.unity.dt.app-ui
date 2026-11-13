using UnityEngine;
#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using Unity.AppUI.TextMateLib;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Turns source code into UITK rich text using a TextMate grammar and theme.
    /// </summary>
    /// <remarks>
    /// The whole thing compiles away unless <c>APPUI_ENABLE_SYNTAX_HIGHLIGHTING</c> is defined, and
    /// <see cref="TryHighlight"/> then simply reports failure, which every caller already has to handle:
    /// grammars are resolved from asset GUIDs, which needs the editor, so a player build renders plain
    /// monospaced text either way.
    ///
    /// <see cref="CodeBlock"/> predates this helper and still carries its own copy of the same logic.
    /// Folding it onto this one is worth doing, but it is a change to a shipped component's rendering
    /// path and belongs in its own commit.
    /// </remarks>
#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
    static class CodeHighlighter
    {
        /// <summary>
        /// Highlights source code, or reports that it could not.
        /// </summary>
        /// <param name="grammar"> The grammar, or null. </param>
        /// <param name="theme"> The theme, or null. </param>
        /// <param name="source"> The source code. </param>
        /// <param name="richText"> The rich-text rendering, when this returns true. </param>
        /// <param name="foreground"> The theme's default foreground, when this returns true. </param>
        /// <param name="background"> The theme's default background, when this returns true. </param>
        /// <returns> Whether the source was highlighted. </returns>
        public static bool TryHighlight(
            TextMateGrammarAsset grammar,
            TextMateThemeAsset theme,
            string source,
            out string richText,
            out Color foreground,
            out Color background)
        {
            richText = null;
            foreground = default;
            background = default;

            if (!theme || !grammar)
                return false;

            using var highlighter = new TextMateSyntaxHighlighter(grammar, theme);
            foreground = Opaque(ColorExtensions.RawColorToColor(highlighter.defaultForeground));
            background = Opaque(ColorExtensions.RawColorToColor(highlighter.defaultBackground));

            try
            {
                richText = highlighter.Highlight(source ?? string.Empty);
            }
            catch (System.Exception e)
            {
                // Some grammars throw inside the native tokenizer on certain inputs. Degrading to plain
                // text is always correct here; letting it escape would break the style-update pass.
                Debug.LogWarning("[CodeHighlighter] Failed to highlight with grammar '" +
                    (grammar ? grammar.scopeName : "null") + "': " + e.Message);
                return false;
            }

            return richText != null;
        }

        static Color Opaque(Color color)
        {
            if (color.a <= 0f)
                color.a = 1f;
            return color;
        }
    }
#endif
}
