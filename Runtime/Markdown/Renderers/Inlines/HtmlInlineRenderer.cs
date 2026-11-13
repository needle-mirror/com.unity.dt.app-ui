using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Unity.AppUI.Core;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="HtmlInline"/> as literal text.
    /// </summary>
    /// <remarks>
    /// Markdown handled by this renderer can be untrusted, and UITK rich-text applies the tags it
    /// understands (<c>&lt;b&gt;</c>, <c>&lt;color&gt;</c>, <c>&lt;size&gt;</c>, sprites). Emitting
    /// the tag verbatim would therefore let the markdown author restyle and spoof the surrounding
    /// UI, so the tag is wrapped in a <c>&lt;noparse&gt;</c> span and shown as text instead — the
    /// same treatment <see cref="LiteralInlineRenderer"/> applies to ordinary text.
    /// </remarks>
    public sealed class HtmlInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, HtmlInline>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, HtmlInline obj)
        {
            if (!string.IsNullOrEmpty(obj.Tag))
                renderer.InlineBuilder.Append(RichTextUtils.Escape(obj.Tag));
        }
    }
}
