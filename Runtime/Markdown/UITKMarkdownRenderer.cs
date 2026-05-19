using System.Collections.Generic;
using System.Text;
using Markdig.Renderers;
using Markdig.Syntax;
using UnityEngine.UIElements;

namespace Unity.AppUI.Markdown
{
    /// <summary>
    /// A Markdig <see cref="RendererBase"/> that emits App UI UITK <see cref="VisualElement"/>s
    /// instead of HTML. Block renderers append elements to <see cref="ParentStack"/>; inline
    /// renderers append to <see cref="InlineBuilder"/> so the enclosing block can flush the
    /// accumulated rich text into a single text-bearing element.
    /// </summary>
    public sealed class UITKMarkdownRenderer : RendererBase
    {
        /// <summary>
        /// The root <see cref="VisualElement"/> that block-level markdown nodes are appended to.
        /// </summary>
        public VisualElement Root { get; }

        internal Stack<VisualElement> ParentStack { get; } = new Stack<VisualElement>();

        internal StringBuilder InlineBuilder { get; } = new StringBuilder();

        /// <summary>
        /// Creates a new renderer that emits UITK <see cref="VisualElement"/>s into <paramref name="root"/>.
        /// </summary>
        /// <param name="root">The container that receives the rendered block-level elements.</param>
        public UITKMarkdownRenderer(VisualElement root)
        {
            Root = root;
            ParentStack.Push(root);

            ObjectRenderers.Add(new Renderers.Blocks.ParagraphRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.HeadingRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.CodeBlockRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.QuoteBlockRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.ListBlockRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.ListItemBlockRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.ThematicBreakRenderer());
            ObjectRenderers.Add(new Renderers.Blocks.HtmlBlockRenderer());

            ObjectRenderers.Add(new Renderers.Inlines.LiteralInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.EmphasisInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.CodeInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.LinkInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.AutolinkInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.LineBreakInlineRenderer());
            ObjectRenderers.Add(new Renderers.Inlines.HtmlInlineRenderer());
        }

        /// <summary>
        /// Renders <paramref name="markdownObject"/> into <see cref="Root"/> by dispatching it
        /// (and all of its descendants) through the registered object renderers.
        /// </summary>
        /// <param name="markdownObject">The Markdig syntax tree node to render.</param>
        /// <returns>The <see cref="Root"/> element, populated in place.</returns>
        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return Root;
        }

        internal void PushParent(VisualElement element)
        {
            ParentStack.Peek().Add(element);
            ParentStack.Push(element);
        }

        internal void PushParentBare(VisualElement element)
        {
            ParentStack.Push(element);
        }

        internal void PopParent()
        {
            ParentStack.Pop();
        }

        internal void AddBlock(VisualElement element)
        {
            ParentStack.Peek().Add(element);
        }

        internal string TakeInline()
        {
            var s = InlineBuilder.ToString();
            InlineBuilder.Clear();
            return s;
        }
    }
}
