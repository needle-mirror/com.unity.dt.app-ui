using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="QuoteBlock"/> as a <see cref="Quote"/>; child blocks
    /// recurse into the <see cref="Quote"/>'s content container.
    /// </summary>
    public sealed class QuoteBlockRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, QuoteBlock>
    {
        /// <summary>The USS class applied to the quote element.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__quote";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, QuoteBlock obj)
        {
            var quote = new Quote();
            quote.AddToClassList(ussClassName);
            renderer.AddBlock(quote);

            var contentContainer = quote.contentContainer ?? (VisualElement)quote;
            renderer.PushParentBare(contentContainer);
            renderer.WriteChildren(obj);
            renderer.PopParent();
        }
    }
}
