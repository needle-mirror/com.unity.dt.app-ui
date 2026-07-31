using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    class TableOfContents : VisualElement
    {
        public override VisualElement contentContainer => m_RootItem;

        readonly TreeViewItem m_RootItem;

        public TableOfContents()
        {
            m_RootItem = new TreeViewItem
            {
                label = "Quick Nav",
                expanded = true,
                depth = 0,
            };
            m_RootItem.AddToClassList("is-category");
            hierarchy.Add(m_RootItem);;
        }
    }
}
