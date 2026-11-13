using System.Text.RegularExpressions;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Helpers to safely embed untrusted text inside UI Toolkit rich text.
    /// </summary>
    public static class RichTextUtils
    {
        // UI Toolkit accepts the closing marker with any casing and with whitespace inside the
        // angle brackets, e.g. `</ NoParse >`, so a literal string match is not enough.
        static readonly Regex k_NoParseClose = new Regex(@"<\s*/\s*noparse\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The zero-width space inserted to break an embedded <c>&lt;/noparse&gt;</c> marker.
        /// </summary>
        const string k_ZeroWidthSpace = "​";

        /// <summary>
        /// Neutralizes any <c>&lt;/noparse&gt;</c> marker in <paramref name="text"/> so it cannot
        /// terminate an enclosing noparse region.
        /// </summary>
        /// <remarks>
        /// UI Toolkit noparse regions do not nest and end at the first closing marker the parser
        /// sees, so an embedded marker would re-enable tag interpretation for the rest of the text.
        /// </remarks>
        /// <param name="text">The untrusted text.</param>
        /// <returns>The text with every closing noparse marker broken.</returns>
        public static string NeutralizeNoParse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return k_NoParseClose.Replace(text, m => "<" + k_ZeroWidthSpace + m.Value.Substring(1));
        }

        /// <summary>
        /// Wraps untrusted text in a <c>&lt;noparse&gt;</c> region so that any formatting tag it
        /// contains is displayed literally instead of being interpreted.
        /// </summary>
        /// <param name="text">The untrusted text.</param>
        /// <returns>Rich text that renders <paramref name="text"/> verbatim.</returns>
        public static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return "<noparse>" + NeutralizeNoParse(text) + "</noparse>";
        }
    }
}
