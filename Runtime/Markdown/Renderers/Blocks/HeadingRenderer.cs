using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="HeadingBlock"/> as a <see cref="Heading"/> with the
    /// matching <see cref="HeadingSize"/> (H1→XXL, H2→XL, H3→L, H4→M, H5→S, H6→XS).
    /// </summary>
    public sealed class HeadingRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, HeadingBlock>
    {
        /// <summary>The USS class applied to the heading element.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__heading";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, HeadingBlock obj)
        {
            renderer.WriteChildren(obj.Inline);
            var text = renderer.TakeInline();

            var element = new Heading(text)
            {
                enableRichText = true,
                primary = true,
                size = LevelToSize(obj.Level),
            };
            element.AddToClassList(ussClassName);
            renderer.AddBlock(element);
        }

        static HeadingSize LevelToSize(int level)
        {
            switch (level)
            {
                case 1: return HeadingSize.XXL;
                case 2: return HeadingSize.XL;
                case 3: return HeadingSize.L;
                case 4: return HeadingSize.M;
                case 5: return HeadingSize.S;
                default: return HeadingSize.XS;
            }
        }
    }
}
