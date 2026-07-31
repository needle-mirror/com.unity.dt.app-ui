using System;
using UnityEngine;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// VisualDocNavArgument is a navigation argument that holds a reference to a TreeViewItemModel.
    /// </summary>
    /// <param name="name"> The name of the navigation argument. </param>
    /// <param name="treeViewItem"> The value of the navigation argument. </param>
    [Serializable]
    record VisualDocNavArgument(string name, TreeViewItemModel treeViewItem) : Navigation.Argument(name, null)
    {
        TreeViewItemModel m_TreeViewItem = treeViewItem;

        /// <summary>
        /// The value of the navigation argument.
        /// </summary>
        public TreeViewItemModel treeViewItem => m_TreeViewItem;
    }
}
