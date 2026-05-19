using Markdig.Renderers;
using Markdig.Syntax;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Text = Unity.AppUI.UI.Text;

namespace Unity.AppUI.Markdown.Renderers.Blocks
{
    /// <summary>
    /// Renders a CommonMark <see cref="ListItemBlock"/> as a horizontal row containing a bullet
    /// or number prefix and a content column. Nested lists work because the content column is
    /// pushed as the recursion target.
    /// </summary>
    public sealed class ListItemBlockRenderer : MarkdownObjectRenderer<UITKMarkdownRenderer, ListItemBlock>
    {
        /// <summary>The USS class applied to the list-item row.</summary>
        public const string ussClassName = MarkdownView.ussClassName + "__list-item";

        /// <summary>The USS class applied to the bullet/number prefix element.</summary>
        public const string bulletUssClassName = ussClassName + "__bullet";

        /// <summary>The USS class applied to the content column that holds the item's child blocks.</summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <inheritdoc />
        protected override void Write(UITKMarkdownRenderer renderer, ListItemBlock obj)
        {
            var row = new VisualElement();
            row.AddToClassList(ussClassName);
            row.pickingMode = PickingMode.Position;

            string bulletText;
            if (obj.Parent is ListBlock parent && parent.IsOrdered)
            {
                var start = 1;
                if (!string.IsNullOrEmpty(parent.OrderedStart))
                    int.TryParse(parent.OrderedStart, out start);
                var index = parent.IndexOf(obj);
                bulletText = (start + index).ToString() + ".";
            }
            else
            {
                bulletText = "•";
            }

            var bullet = new Text { text = bulletText };
            bullet.AddToClassList(bulletUssClassName);
            row.Add(bullet);

            var content = new VisualElement();
            content.AddToClassList(contentUssClassName);
            content.pickingMode = PickingMode.Position;
            row.Add(content);

            renderer.AddBlock(row);

            renderer.PushParentBare(content);
            renderer.WriteChildren(obj);
            renderer.PopParent();
        }
    }
}
