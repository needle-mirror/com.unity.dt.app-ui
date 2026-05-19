using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="CodeInline"/> by wrapping its content in a colored,
    /// bold rich-text span. The body uses <c>&lt;noparse&gt;</c> so embedded <c>&lt;</c>/<c>&gt;</c>
    /// characters in the inline code are not interpreted as UITK rich-text tags. Note: a true
    /// monospace face cannot be applied per-span — UITK's <c>&lt;font&gt;</c> rich-text tag only
    /// resolves font assets under <c>Resources/Fonts &amp; Materials/</c>, and the package's
    /// RobotoMono lives in <c>PackageResources/Fonts/</c>. Block code uses the
    /// <c>--appui-code-font-family</c> token instead.
    /// </summary>
    public sealed class CodeInlineRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, CodeInline>
    {
        const string k_Color = "#CE9178";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, CodeInline obj)
        {
            renderer.InlineBuilder.Append("<color=").Append(k_Color).Append("><b><noparse>");
            if (!string.IsNullOrEmpty(obj.Content))
                renderer.InlineBuilder.Append(obj.Content);
            renderer.InlineBuilder.Append("</noparse></b></color>");
        }
    }
}
