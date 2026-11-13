using System;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Helpers used by the inline link renderers to decide whether a markdown link destination may
    /// become a clickable UITK <c>&lt;a href="…"&gt;</c> target.
    /// </summary>
    /// <remarks>
    /// Markdown rendered by App UI can come from untrusted sources (chat, assistant or mention
    /// payloads, fetched documentation), so link destinations are validated against an allow-list of
    /// schemes rather than a block-list of known-bad ones. This mirrors the JavaScript reference
    /// implementation in <c>packages/com-unity-app-ui/src/utils/safeExternalUrl.mjs</c>.
    /// </remarks>
    public static class HrefUtils
    {
        /// <summary>
        /// UITK rich-text <c>href</c> values are limited to 256 characters; URLs longer than
        /// this are emitted as plain underlined text without a clickable href.
        /// </summary>
        public const int maxHrefLength = 256;

        static readonly string[] k_AllowedSchemes = { "http://", "https://", "mailto:" };

        /// <summary>
        /// Returns whether <paramref name="url"/> is a link destination that may be made clickable.
        /// </summary>
        /// <remarks>
        /// Allowed are absolute <c>http</c>, <c>https</c> and <c>mailto</c> destinations, plus
        /// site-relative paths starting with a single <c>/</c>. Everything else — including
        /// protocol-relative <c>//host</c> forms, any destination containing a backslash (which
        /// browsers normalise to <c>/</c>, so <c>\\host</c> and <c>/\host</c> resolve off-origin
        /// too), and schemes such as <c>javascript:</c>, <c>data:</c>, <c>vbscript:</c>,
        /// <c>file:</c> and <c>ftp:</c> — is rejected.
        /// </remarks>
        /// <param name="url">The raw link destination from the markdown source.</param>
        /// <returns><see langword="true"/> when the destination is safe to link to.</returns>
        public static bool IsSafeHref(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            var trimmed = url.Trim();
            if (trimmed.Length == 0 || trimmed.Length > maxHrefLength)
                return false;

            // Reject control characters (including the tabs and newlines browsers strip out of
            // URLs, used to smuggle a scheme past a prefix check) and backslashes.
            //
            // A backslash never carries meaning in a web destination: browsers normalise it to `/`
            // while parsing a special-scheme URL, so `\\host`, `\\/host` and `/\\host` all resolve
            // off-origin exactly as `//host` does. Rejecting the character outright covers every
            // such combination instead of enumerating the positions it can appear in.
            foreach (var c in trimmed)
            {
                if (char.IsControl(c) || c == '\\')
                    return false;
            }

            // Site-relative, but not protocol-relative `//host`.
            if (trimmed[0] == '/')
                return trimmed.Length == 1 || trimmed[1] != '/';

            foreach (var scheme in k_AllowedSchemes)
            {
                if (trimmed.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // A destination with no scheme separator at all (`example.com/page`, `#anchor`) is a
            // relative reference and cannot select a dangerous handler.
            var colon = trimmed.IndexOf(':');
            if (colon < 0)
                return true;

            var slash = trimmed.IndexOf('/');
            return slash >= 0 && slash < colon;
        }

        /// <summary>
        /// Escapes a link destination so it cannot terminate the <c>href</c> attribute it is
        /// written into.
        /// </summary>
        /// <param name="url">The link destination.</param>
        /// <returns>The escaped destination.</returns>
        public static string EscapeHref(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            return url.Trim().Replace("\"", "&quot;");
        }
    }
}
