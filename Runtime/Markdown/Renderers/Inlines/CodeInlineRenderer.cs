using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Unity.AppUI.Core;

namespace Unity.AppUI.Markdown.Renderers.Inlines
{
    /// <summary>
    /// Renders a CommonMark <see cref="CodeInline"/> by wrapping its content in a colored,
    /// bold rich-text span. The body is escaped with <see cref="RichTextUtils.Escape"/> so embedded
    /// <c>&lt;</c>/<c>&gt;</c> characters in the inline code are not interpreted as UITK rich-text
    /// tags. Note: a true monospace face cannot be applied per-span — UITK's
    /// <c>&lt;font&gt;</c> rich-text tag only
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
            // Escape rather than hand-wrap in <noparse>: a code span carrying its own
            // `</noparse>` would close the span early and re-enable tag interpretation for
            // everything after it. Only the surrounding color/bold markup is trusted.
            renderer.InlineBuilder.Append("<color=").Append(k_Color).Append("><b>");
            if (!string.IsNullOrEmpty(obj.Content))
                renderer.InlineBuilder.Append(RichTextUtils.Escape(obj.Content));
            renderer.InlineBuilder.Append("</b></color>");
        }
    }
}
