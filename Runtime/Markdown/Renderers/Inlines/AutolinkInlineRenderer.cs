using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Unity.AppUI.Core;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="AutolinkInline"/> (e.g. <c>&lt;https://example.com&gt;</c>)
    /// as a UITK <c>&lt;a href="…"&gt;</c> tag with the URL as both label and target. URLs longer
    /// than <see cref="HrefUtils.maxHrefLength"/>, and destinations rejected by
    /// <see cref="HrefUtils.IsSafeHref"/>, are emitted as plain underlined text.
    /// </summary>
    public sealed class AutolinkInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, AutolinkInline>
    {
        const string k_Color = "#4FC1FF";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, AutolinkInline obj)
        {
            var url = obj.Url ?? string.Empty;
            var target = obj.IsEmail ? "mailto:" + url : url;
            var clickable = HrefUtils.IsSafeHref(target);
            var href = clickable ? HrefUtils.EscapeHref(target) : string.Empty;

            if (clickable)
                renderer.InlineBuilder.Append("<a href=\"").Append(href).Append("\">");

            renderer.InlineBuilder.Append("<color=").Append(k_Color).Append("><u>");
            renderer.InlineBuilder.Append(RichTextUtils.Escape(url));
            renderer.InlineBuilder.Append("</u></color>");

            if (clickable)
                renderer.InlineBuilder.Append("</a>");
        }
    }
}
