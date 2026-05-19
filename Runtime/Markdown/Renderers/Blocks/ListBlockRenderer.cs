using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="ListBlock"/> as a column of items. The container carries
    /// either <see cref="orderedUssClassName"/> or <see cref="unorderedUssClassName"/> so callers
    /// can style the two list styles independently.
    /// </summary>
    public sealed class ListBlockRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, ListBlock>
    {
        /// <summary>The USS class applied to the list container.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__list";

        /// <summary>The USS modifier applied to ordered lists (<c>1.</c>, <c>2.</c>, …).</summary>
        public const string orderedUssClassName = ussClassName + "--ordered";

        /// <summary>The USS modifier applied to unordered lists (bullet glyph).</summary>
        public const string unorderedUssClassName = ussClassName + "--unordered";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, ListBlock obj)
        {
            var list = new VisualElement();
            list.AddToClassList(ussClassName);
            list.AddToClassList(obj.IsOrdered ? orderedUssClassName : unorderedUssClassName);
            list.pickingMode = PickingMode.Position;

            renderer.PushParent(list);
            renderer.WriteChildren(obj);
            renderer.PopParent();
        }
    }
}
