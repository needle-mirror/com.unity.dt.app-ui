using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="LinkInline"/>. Text links emit a UITK
    /// <c>&lt;a href="…"&gt;</c> rich-text tag (UITK clamps href + id values to 256 characters,
    /// so longer URLs are dropped from the href and rendered as plain underlined text).
    /// Image links emit their alt text and the URL as plain text — fetching remote bitmaps is
    /// out of scope for this renderer.
    /// </summary>
    public sealed class LinkInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, LinkInline>
    {
        const string k_Color = "#4FC1FF";

        /// <summary>
        /// UITK rich-text <c>href</c> values are limited to 256 characters; URLs longer than
        /// this are emitted as plain underlined text without a clickable href.
        /// </summary>
        public const int maxHrefLength = 256;

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, LinkInline obj)
        {
            var url = obj.GetDynamicUrl != null ? obj.GetDynamicUrl() : obj.Url;
            url = url ?? string.Empty;

            if (obj.IsImage)
            {
                renderer.InlineBuilder.Append('[');
                renderer.WriteChildren(obj);
                renderer.InlineBuilder.Append(']');
                if (!string.IsNullOrEmpty(url))
                    renderer.InlineBuilder.Append("(").Append(url).Append(")");
                return;
            }

            var href = EscapeHref(url);
            var clickable = !string.IsNullOrEmpty(href) && href.Length <= maxHrefLength;

            if (clickable)
                renderer.InlineBuilder.Append("<a href=\"").Append(href).Append("\">");

            renderer.InlineBuilder.Append("<color=").Append(k_Color).Append("><u>");
            renderer.WriteChildren(obj);
            renderer.InlineBuilder.Append("</u></color>");

            if (clickable)
                renderer.InlineBuilder.Append("</a>");
        }

        static string EscapeHref(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            return url.Replace("\"", "&quot;");
        }
    }
}
