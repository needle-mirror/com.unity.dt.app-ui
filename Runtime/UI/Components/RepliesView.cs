using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A simple replies container backed by a <see cref="ScrollView"/>. Drop-in
    /// replacement for the subset of <see cref="ListView"/> used by
    /// <see cref="Thread"/>: supports makeItem / bindItem / unbindItem /
    /// destroyItem, an itemsSource, and a manual <see cref="RefreshItems"/>.
    /// Items are realized eagerly — there is no virtualization — which keeps
    /// the layout stable inside popovers where dynamic-height ListView
    /// constantly relayouts itself.
    /// </summary>
    public class RepliesView : ScrollView
    {
        Func<VisualElement> m_MakeItem;
        Action<VisualElement, int> m_BindItem;
        Action<VisualElement, int> m_UnbindItem;
        Action<VisualElement> m_DestroyItem;
        IList m_ItemsSource;
        readonly List<VisualElement> m_Items = new();

        /// <summary>
        /// Initializes a new instance of <see cref="RepliesView"/> with vertical scrolling
        /// and no horizontal scrollbar.
        /// </summary>
        public RepliesView()
        {
            mode = ScrollViewMode.Vertical;
            horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        /// <summary>
        /// Callback invoked to create a new item visual element when the list needs one.
        /// </summary>
        public Func<VisualElement> makeItem
        {
            get => m_MakeItem;
            set => m_MakeItem = value;
        }

        /// <summary>
        /// Callback invoked to bind data from <see cref="itemsSource"/> at the given index
        /// to the supplied visual element.
        /// </summary>
        public Action<VisualElement, int> bindItem
        {
            get => m_BindItem;
            set => m_BindItem = value;
        }

        /// <summary>
        /// Callback invoked to release any bindings on a visual element before it is
        /// destroyed or recycled.
        /// </summary>
        public Action<VisualElement, int> unbindItem
        {
            get => m_UnbindItem;
            set => m_UnbindItem = value;
        }

        /// <summary>
        /// Callback invoked to dispose of a visual element that is no longer needed.
        /// </summary>
        public Action<VisualElement> destroyItem
        {
            get => m_DestroyItem;
            set => m_DestroyItem = value;
        }

        /// <summary>
        /// Accepted for API compatibility with <see cref="ListView"/> but has no effect;
        /// <see cref="RepliesView"/> always realizes items eagerly.
        /// </summary>
        public CollectionVirtualizationMethod virtualizationMethod { get; set; }

        /// <summary>
        /// Accepted for API compatibility with <see cref="ListView"/> but has no effect;
        /// <see cref="RepliesView"/> does not support item selection.
        /// </summary>
        public SelectionType selectionType { get; set; }

        /// <summary>
        /// The data source that drives the list. Call <see cref="RefreshItems"/> after
        /// changing the source or its contents to update the view.
        /// </summary>
        public IList itemsSource
        {
            get => m_ItemsSource;
            set => m_ItemsSource = value;
        }

        /// <summary>
        /// Destroys all current item elements and recreates them from <see cref="itemsSource"/>
        /// using <see cref="makeItem"/> and <see cref="bindItem"/>.
        /// </summary>
        public void RefreshItems()
        {
            for (var i = m_Items.Count - 1; i >= 0; i--)
            {
                var el = m_Items[i];
                m_UnbindItem?.Invoke(el, i);
                m_DestroyItem?.Invoke(el);
                el.RemoveFromHierarchy();
            }
            m_Items.Clear();

            if (m_ItemsSource == null || m_MakeItem == null)
                return;

            for (var i = 0; i < m_ItemsSource.Count; i++)
            {
                var el = m_MakeItem();
                Add(el);
                m_BindItem?.Invoke(el, i);
                m_Items.Add(el);
            }
        }

        /// <summary>
        /// Returns the root visual element for the item at <paramref name="index"/>,
        /// or <c>null</c> if the index is out of range.
        /// </summary>
        /// <param name="index">Zero-based index into <see cref="itemsSource"/>.</param>
        /// <returns>The realized <see cref="VisualElement"/> for that index, or <c>null</c>.</returns>
        public VisualElement GetRootElementForIndex(int index)
        {
            if (index < 0 || index >= m_Items.Count)
                return null;
            return m_Items[index];
        }
    }
}
