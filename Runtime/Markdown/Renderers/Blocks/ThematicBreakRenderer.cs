using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="ThematicBreakBlock"/> (<c>---</c>, <c>***</c>, <c>___</c>)
    /// as a horizontal <see cref="Divider"/>.
    /// </summary>
    public sealed class ThematicBreakRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, ThematicBreakBlock>
    {
        /// <summary>The USS class applied to the divider element.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__thematic-break";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, ThematicBreakBlock obj)
        {
            var divider = new Divider { direction = Direction.Horizontal };
            divider.AddToClassList(ussClassName);
            renderer.AddBlock(divider);
        }
    }
}
