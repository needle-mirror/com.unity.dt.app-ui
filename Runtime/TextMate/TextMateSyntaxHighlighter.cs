#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using System.Text;
using Unity.AppUI.TextMateLib;
using UnityEngine;
using FontStyle = Unity.AppUI.TextMateLib.FontStyle;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Converts source code to UITK rich text using TextMate grammars and themes.
    /// </summary>
    public class TextMateSyntaxHighlighter : IDisposable
    {
        /// <summary>
        /// The TextMate registry used by this highlighter.
        /// </summary>
        public Registry registry { get; }

        /// <summary>
        /// The TextMate grammar used for tokenization.
        /// </summary>
        public Grammar grammar { get; }

        /// <summary>
        /// The TextMate theme used for colors and styles.
        /// </summary>
        public Theme theme { get; }

        bool m_Disposed;

        /// <summary>
        /// Creates a new syntax highlighter with the specified grammar and theme.
        /// </summary>
        /// <param name="grammarAsset">The TextMate grammar asset for tokenization.</param>
        /// <param name="themeAsset">The TextMate theme asset for colors and styles.</param>
        /// <exception cref="ArgumentNullException">Thrown when grammarAsset or themeAsset is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when grammar or theme fails to load.</exception>
        public TextMateSyntaxHighlighter(TextMateGrammarAsset grammarAsset, TextMateThemeAsset themeAsset)
        {
            if (grammarAsset == null)
                throw new ArgumentNullException(nameof(grammarAsset));
            if (themeAsset == null)
                throw new ArgumentNullException(nameof(themeAsset));

            registry = new Registry();
            registry.AddGrammarFromJson(grammarAsset.jsonContent);
            grammar = registry.LoadGrammar(grammarAsset.scopeName);
            theme = Theme.LoadFromJson(themeAsset.jsonContent);
        }

        /// <summary>
        /// Highlights source code and returns UITK-compatible rich text.
        /// </summary>
        /// <param name="sourceCode">The source code to highlight.</param>
        /// <returns>Rich text string with UITK color and style tags.</returns>
        public string Highlight(string sourceCode)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(sourceCode))
                return string.Empty;

            var lines = sourceCode.Split('\n');
            var result = new StringBuilder();
            var state = IntPtr.Zero;

            for (int i = 0; i < lines.Length; i++)
            {
                var richLine = HighlightLine(lines[i], state, out state);
                result.Append(richLine);

                if (i < lines.Length - 1)
                    result.Append('\n');
            }

            return result.ToString();
        }

        /// <summary>
        /// Highlights a single line of code.
        /// </summary>
        /// <param name="line">The line to highlight.</param>
        /// <param name="prevState">The state from the previous line (use IntPtr.Zero for the first line).</param>
        /// <param name="newState">The state at the end of this line, to be passed to the next line.</param>
        /// <returns>Rich text string for the line.</returns>
        public string HighlightLine(string line, IntPtr prevState, out IntPtr newState)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(line))
            {
                newState = prevState;
                return string.Empty;
            }

            var tokenizeResult = grammar.TokenizeLine(line, prevState);
            newState = tokenizeResult.StateStack;

            var sb = new StringBuilder();

            foreach (var token in tokenizeResult.Tokens)
            {
                sb.Append(TokenToRichText(token, line, theme));
            }

            return sb.ToString();
        }

        static string TokenToRichText(Token token, string line, Theme theme)
        {
            var text = token.GetValue(line);

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Build scope path from token scopes (space-separated)
            var scopePath = string.Join(" ", token.Scopes);

            var defaultFg = theme.GetDefaultForeground();
            var fgColor = theme.GetForeground(scopePath, defaultFg);
            var fontStyle = theme.GetFontStyle(scopePath, FontStyle.None);

            var sb = new StringBuilder();

            // Open style tags
            if ((fontStyle & FontStyle.Bold) != 0)
                sb.Append("<b>");
            if ((fontStyle & FontStyle.Italic) != 0)
                sb.Append("<i>");

            // Color tag with escaped text
            sb.Append("<color=");
            sb.Append(ColorExtensions.RawColorToHex(fgColor));
            sb.Append('>');
            sb.Append(EscapeRichText(text));
            sb.Append("</color>");

            // Close style tags (reverse order)
            if ((fontStyle & FontStyle.Italic) != 0)
                sb.Append("</i>");
            if ((fontStyle & FontStyle.Bold) != 0)
                sb.Append("</b>");

            return sb.ToString();
        }

        /// <summary>
        /// Escapes special characters in text to prevent rich text tag injection.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>Escaped text safe for rich text.</returns>
        static string EscapeRichText(string text)
        {
            // Escape < and > to prevent tag injection
            return text.Replace("<", "<\u200B").Replace(">", ">\u200B");
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(TextMateSyntaxHighlighter));
        }

        /// <summary>
        /// Releases the native resources used by the highlighter.
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed)
            {
                grammar?.Dispose();
                theme?.Dispose();
                registry?.Dispose();
                m_Disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~TextMateSyntaxHighlighter()
        {
            Dispose();
        }
    }
}
#endif
