using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Unity.AppUI.Core;

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

            // Escape rather than wrap by hand: a literal containing its own `</noparse>` would
            // otherwise close the span early and re-enable tag interpretation for the rest.
            renderer.InlineBuilder.Append(
                RichTextUtils.Escape(slice.Text.Substring(slice.Start, slice.End - slice.Start + 1)));
        }
    }
}
