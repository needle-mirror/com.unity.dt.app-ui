using Markdig.Renderers;
using Markdig.Syntax;
using AppUiCodeBlock = Unity.AppUI.UI.CodeBlock;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="CodeBlock"/> (fenced or indented) as an App UI
    /// <see cref="AppUiCodeBlock"/>. Syntax highlighting is driven entirely by USS — the
    /// CodeBlock element resolves <c>--codeblock-grammar</c> / <c>--codeblock-theme</c>
    /// from its own custom style based on the <c>language</c> we set, so this renderer just
    /// forwards the source and language and lets the design system do the rest.
    /// </summary>
    public sealed class CodeBlockRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, CodeBlock>
    {
        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, CodeBlock obj)
        {
            string info = null;
            if (obj is FencedCodeBlock fenced)
                info = fenced.Info;

            var source = obj.Lines.ToString() ?? string.Empty;
            renderer.AddBlock(new AppUiCodeBlock
            {
                language = info,
                code = source,
            });
        }
    }
}
