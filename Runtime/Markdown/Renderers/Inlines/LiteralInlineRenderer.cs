using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="LiteralInline"/> by appending its text — wrapped in a
    /// UITK <c>&lt;noparse&gt;</c> span so any <c>&lt;</c>/<c>&gt;</c>-bearing characters in the
    /// source markdown are not interpreted as rich-text tags.
    /// </summary>
    public sealed class LiteralInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, LiteralInline>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, LiteralInline obj)
        {
            var slice = obj.Content;
            if (slice.IsEmpty || slice.Text == null)
                return;

            renderer.InlineBuilder.Append("<noparse>");
            for (var i = slice.Start; i <= slice.End; i++)
                renderer.InlineBuilder.Append(slice.Text[i]);
            renderer.InlineBuilder.Append("</noparse>");
        }
    }
}
