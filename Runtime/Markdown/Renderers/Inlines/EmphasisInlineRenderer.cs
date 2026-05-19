using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="EmphasisInline"/> by wrapping its children in
    /// UITK rich-text bold (<c>&lt;b&gt;</c>) or italic (<c>&lt;i&gt;</c>) tags.
    /// </summary>
    public sealed class EmphasisInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, EmphasisInline>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, EmphasisInline obj)
        {
            var tag = obj.DelimiterCount == 2 ? "b" : "i";
            renderer.InlineBuilder.Append('<').Append(tag).Append('>');
            renderer.WriteChildren(obj);
            renderer.InlineBuilder.Append("</").Append(tag).Append('>');
        }
    }
}
