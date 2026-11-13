using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Unity.AppUI.Core;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="LinkInline"/>. Text links emit a UITK
    /// <c>&lt;a href="…"&gt;</c> rich-text tag (UITK clamps href + id values to 256 characters,
    /// so longer URLs are dropped from the href and rendered as plain underlined text).
    /// Image links emit their alt text and the URL as plain text — fetching remote bitmaps is
    /// out of scope for this renderer.
    /// </summary>
    /// <remarks>
    /// Only destinations accepted by <see cref="HrefUtils.IsSafeHref"/> become clickable; anything
    /// else (<c>javascript:</c>, <c>data:</c>, <c>file:</c>, custom app schemes, …) renders as
    /// plain non-clickable text.
    /// </remarks>
    public sealed class LinkInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, LinkInline>
    {
        const string k_Color = "#4FC1FF";

        /// <summary>
        /// UITK rich-text <c>href</c> values are limited to 256 characters; URLs longer than
        /// this are emitted as plain underlined text without a clickable href.
        /// </summary>
        public const int maxHrefLength = HrefUtils.maxHrefLength;

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
                    renderer.InlineBuilder.Append("(").Append(RichTextUtils.Escape(url)).Append(")");
                return;
            }

            var clickable = HrefUtils.IsSafeHref(url);
            var href = clickable ? HrefUtils.EscapeHref(url) : string.Empty;

            if (clickable)
                renderer.InlineBuilder.Append("<a href=\"").Append(href).Append("\">");

            renderer.InlineBuilder.Append("<color=").Append(k_Color).Append("><u>");
            renderer.WriteChildren(obj);
            renderer.InlineBuilder.Append("</u></color>");

            if (clickable)
                renderer.InlineBuilder.Append("</a>");
        }
    }
}
