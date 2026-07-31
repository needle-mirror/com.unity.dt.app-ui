using System.Collections.Generic;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// The kind of content a documentation tree item points to.
    /// </summary>
    enum PageKind
    {
        /// <summary>The item is a grouping node without a page.</summary>
        None,

        /// <summary>The item renders a component page from the compile-time doc registry.</summary>
        Component,

        /// <summary>The item renders a markdown guide from the package documentation.</summary>
        Guide,
    }

    /// <summary>
    /// Runtime tree view item with built hierarchy
    /// </summary>
    class TreeViewItemModel
    {
        /// <summary>
        /// Unique identifier for this item
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display name of this item
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The name/slug for this item (used as navigation destination name)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Reference to the parent item (null for root items)
        /// </summary>
        public TreeViewItemModel Parent { get; private set; }

        /// <summary>
        /// The child items of this item
        /// </summary>
        public List<TreeViewItemModel> Children { get; } = new List<TreeViewItemModel>();

        /// <summary>
        /// Whether this item is a category (always expanded, has no dedicated page)
        /// </summary>
        public bool IsCategory { get; }

        /// <summary>
        /// Whether this item should be flagged as new
        /// </summary>
        public bool IsNew { get; }

        /// <summary>
        /// The kind of page this item points to.
        /// </summary>
        public PageKind PageKind { get; }

        /// <summary>
        /// The page reference: a doc registry page id for components, a markdown
        /// file name for guides, empty for grouping nodes.
        /// </summary>
        public string ContentRef { get; }

        /// <summary>
        /// Gets the depth level of this item in the tree hierarchy (0-based)
        /// </summary>
        public int Depth
        {
            get
            {
                if (Parent == null)
                    return 0;
                return Parent.Depth + 1;
            }
        }

        /// <summary>
        /// Gets the full path to this item as a breadcrumb string
        /// </summary>
        public string FullPath
        {
            get
            {
                if (Parent == null)
                    return DisplayName;
                return $"{Parent.FullPath} > {DisplayName}";
            }
        }

        /// <summary>
        /// Whether this item is browsable (i.e., has a page to display)
        /// </summary>
        public bool IsBrowsable => PageKind != PageKind.None && !string.IsNullOrEmpty(ContentRef);

        /// <summary>
        /// Create a new tree view item
        /// </summary>
        public TreeViewItemModel(
            string id,
            string displayName,
            bool isCategory = false,
            bool isNew = false,
            PageKind pageKind = PageKind.None,
            string contentRef = null)
        {
            Id = id;
            Name = id;
            DisplayName = displayName;
            IsCategory = isCategory;
            IsNew = isNew;
            PageKind = pageKind;
            ContentRef = contentRef;
        }

        /// <summary>
        /// Set the parent of this item
        /// </summary>
        public void SetParent(TreeViewItemModel parent)
        {
            Parent = parent;
            if (parent != null && !parent.Children.Contains(this))
            {
                parent.Children.Add(this);
            }
        }
    }

    /// <summary>
    /// Helpers to traverse the documentation tree
    /// </summary>
    static class TreeViewParser
    {
        /// <summary>
        /// Get a flat list of all items in the tree
        /// </summary>
        public static List<TreeViewItemModel> GetAllItems(List<TreeViewItemModel> rootItems)
        {
            var result = new List<TreeViewItemModel>();
            foreach (var rootItem in rootItems)
            {
                result.Add(rootItem);
                result.AddRange(GetAllChildrenRecursive(rootItem));
            }
            return result;
        }

        /// <summary>
        /// Helper method to get all children of an item recursively
        /// </summary>
        static List<TreeViewItemModel> GetAllChildrenRecursive(TreeViewItemModel item)
        {
            var result = new List<TreeViewItemModel>();
            foreach (var child in item.Children)
            {
                result.Add(child);
                result.AddRange(GetAllChildrenRecursive(child));
            }
            return result;
        }
    }
}
