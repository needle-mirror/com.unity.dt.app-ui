using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="LineBreakInline"/>. Soft breaks emit a single newline,
    /// hard breaks (trailing two spaces or a backslash) emit two newlines so the surrounding
    /// rich-text element produces a visible blank line.
    /// </summary>
    public sealed class LineBreakInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, LineBreakInline>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, LineBreakInline obj)
        {
            renderer.InlineBuilder.Append(obj.IsHard ? "\n\n" : "\n");
        }
    }
}
