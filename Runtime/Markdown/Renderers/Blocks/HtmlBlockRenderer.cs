using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders raw HTML blocks as escaped, monospaced text. We deliberately don't try to interpret
    /// arbitrary HTML inside a UITK panel — that surface is too large to safely support.
    /// </summary>
    public sealed class HtmlBlockRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, HtmlBlock>
    {
        /// <summary>The USS class applied to the raw-HTML text element.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__html-block";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, HtmlBlock obj)
        {
            var raw = obj.Lines.ToString() ?? string.Empty;
            var element = new LocalizedTextElement
            {
                text = raw,
                enableRichText = false,
                pickingMode = PickingMode.Position,
            };
            element.AddToClassList(ussClassName);
            renderer.AddBlock(element);
        }
    }
}
