#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using System.Text;
using Unity.AppUI.TextMateLib;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Converts source code to UITK rich text using TextMate grammars and themes.
    /// Uses themed/encoded tokenization for optimal performance in static display.
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

        string[] m_ColorMap;

        string m_DefaultForegroundHex;

        uint m_DefaultForeground;

        uint m_DefaultBackground;

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
            registry.SetThemeFromJson(themeAsset.jsonContent);
            grammar = registry.LoadGrammar(grammarAsset.scopeName);
            m_ColorMap = registry.GetColorMap();
            // The native color map can hold empty strings for unassigned default colors.
            // Fall back so m_DefaultForegroundHex is never empty — an empty hex would emit a
            // broken "<color=>" tag and break UITK rich-text parsing.
            m_DefaultForegroundHex = m_ColorMap.Length > 1 && !string.IsNullOrEmpty(m_ColorMap[1])
                ? m_ColorMap[1]
                : "#FFFFFF";
            m_DefaultForeground = HexToRgba(m_DefaultForegroundHex);
            m_DefaultBackground = m_ColorMap.Length > 2 && !string.IsNullOrEmpty(m_ColorMap[2])
                ? HexToRgba(m_ColorMap[2])
                : 0x000000FF;
        }

        /// <summary>
        /// Gets the default foreground color in RGBA format (0xRRGGBBAA).
        /// </summary>
        public uint defaultForeground => m_DefaultForeground;

        /// <summary>
        /// Gets the default background color in RGBA format (0xRRGGBBAA).
        /// </summary>
        public uint defaultBackground => m_DefaultBackground;

        /// <summary>
        /// Highlights source code and returns UITK-compatible rich text.
        /// Uses batch themed tokenization for optimal performance.
        /// </summary>
        /// <param name="sourceCode">The source code to highlight.</param>
        /// <returns>Rich text string with UITK color and style tags.</returns>
        public string Highlight(string sourceCode)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(sourceCode))
                return string.Empty;

            var lines = sourceCode.Split('\n');
            var lineResults = grammar.TokenizeLines2(lines);
            var result = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                AppendHighlightedLine(result, lines[i], lineResults[i]);

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

            var tokenizeResult = grammar.TokenizeLine2(line, prevState);
            newState = tokenizeResult.StateStack;

            var sb = new StringBuilder();
            AppendHighlightedLine(sb, line, tokenizeResult);
            return sb.ToString();
        }

        void AppendHighlightedLine(StringBuilder sb, string line, TokenizeResult2 lineResult)
        {
            var tokens = lineResult.Tokens;
            for (int t = 0; t < tokens.Count; t++)
            {
                var token = tokens[t];
                int startIndex = token.StartIndex;
                int endIndex = t + 1 < tokens.Count ? tokens[t + 1].StartIndex : line.Length;

                // Guard against a negative start index from the native tokenizer (a bug or
                // malformed grammar) which would otherwise throw when indexing the line.
                if (startIndex < 0 || endIndex <= startIndex || startIndex >= line.Length)
                    continue;

                endIndex = Math.Min(endIndex, line.Length);

                int fgId = token.ForegroundId;
                // Fall back to the default when the slot is out of range or an empty string
                // (an unassigned native color), so we never emit a broken "<color=>" tag.
                var hexColor = fgId > 0 && fgId < m_ColorMap.Length && !string.IsNullOrEmpty(m_ColorMap[fgId])
                    ? m_ColorMap[fgId]
                    : m_DefaultForegroundHex;

                int fontStyle = token.FontStyle;
                bool bold = (fontStyle & 2) != 0;
                bool italic = (fontStyle & 1) != 0;

                if (bold)
                    sb.Append("<b>");
                if (italic)
                    sb.Append("<i>");

                sb.Append("<color=");
                sb.Append(hexColor);
                sb.Append('>');
                AppendEscaped(sb, line, startIndex, endIndex);
                sb.Append("</color>");

                if (italic)
                    sb.Append("</i>");
                if (bold)
                    sb.Append("</b>");
            }
        }

        // Appends line[startIndex..endIndex) to the builder, escaping '<' and '>' for UITK
        // rich text by inserting a zero-width space after each. Iterating the range directly
        // avoids the per-token Substring + Replace allocations that caused GC spikes when
        // displaying or scrolling highlighted code.
        static void AppendEscaped(StringBuilder sb, string line, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                char c = line[i];
                sb.Append(c);
                if (c == '<' || c == '>')
                    sb.Append('\u200B');
            }
        }

        static uint HexToRgba(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex[0] != '#')
                return 0xFFFFFFFF;

            int digits = hex.Length - 1; // hex digits following '#'
            if (digits != 6 && digits != 8)
                return 0xFFFFFFFF;

            // Parse the hex digits in place to avoid the Substring/concatenation allocations.
            uint value = 0;
            for (int i = 1; i < hex.Length; i++)
            {
                int d = HexDigit(hex[i]);
                if (d < 0)
                    return 0xFFFFFFFF;
                value = (value << 4) | (uint)d;
            }

            // #RRGGBB carries no alpha channel; default it to fully opaque (0xFF).
            if (digits == 6)
                value = (value << 8) | 0xFFu;

            return value;
        }

        static int HexDigit(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
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
