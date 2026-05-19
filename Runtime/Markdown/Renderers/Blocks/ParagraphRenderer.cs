using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="ParagraphBlock"/> as a single rich-text
    /// <see cref="LocalizedTextElement"/>; inline children flush their accumulated text
    /// into <see cref="UITKMarkdownRenderer.InlineBuilder"/>.
    /// </summary>
    public sealed class ParagraphRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, ParagraphBlock>
    {
        /// <summary>The USS class applied to the paragraph element.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__paragraph";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, ParagraphBlock obj)
        {
            renderer.WriteChildren(obj.Inline);

            var text = renderer.TakeInline();
            if (string.IsNullOrEmpty(text))
                return;

            var element = new LocalizedTextElement
            {
                text = text,
                enableRichText = true,
                pickingMode = PickingMode.Position,
            };
            element.AddToClassList(ussClassName);
            renderer.AddBlock(element);
        }
    }
}
