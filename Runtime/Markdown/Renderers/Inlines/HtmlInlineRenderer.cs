using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="HtmlInline"/> by emitting its tag verbatim. UITK rich-text
    /// will silently ignore tags it does not understand (e.g. <c>&lt;span&gt;</c>) and apply ones it
    /// does (e.g. <c>&lt;b&gt;</c>, <c>&lt;color&gt;</c>) — so this is the most permissive,
    /// least-disruptive behavior for embedded HTML.
    /// </summary>
    public sealed class HtmlInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, HtmlInline>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, HtmlInline obj)
        {
            if (!string.IsNullOrEmpty(obj.Tag))
                renderer.InlineBuilder.Append(obj.Tag);
        }
    }
}
