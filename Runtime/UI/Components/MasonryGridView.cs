using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A GridView that arranges items in a masonry layout pattern, similar to Pinterest's layout.
    /// </summary>
    /// <remarks>
    /// The MasonryGridView is a specialized grid layout component that arranges items in a vertical masonry
    /// pattern. Unlike traditional grid layouts where items must conform to a fixed grid, the masonry layout
    /// allows items of varying heights while maintaining aligned columns.
    ///
    /// This layout is particularly useful for displaying content of varying heights, such as images, cards, or
    /// content blocks, in a visually appealing and space-efficient manner.
    ///
    /// Key features include:
    /// - Dynamic column arrangement
    /// - Support for items with varying heights
    /// - Efficient item recycling for optimal performance
    /// - Built-in selection management
    /// - Smooth scrolling with viewport item management
    /// </remarks>
    /// <example>
    /// <para>Basic setup of a MasonryGridView with custom items:</para>
    ///
    /// <para>Creating and configuring a MasonryGridView</para>
    /// <code lang="csharp"><![CDATA[
    /// var masonryGrid = new MasonryGridView() {
    ///     columnCount = 3,
    ///     selectionType = SelectionType.Single,
    ///     pack = true
    /// };
    ///
    /// // Setup item source
    /// masonryGrid.itemsSource = items;
    ///
    /// // Define how items are created
    /// masonryGrid.makeItem = () => new CustomItemElement();
    ///
    /// // Define how items are bound
    /// masonryGrid.bindItem = (element, index) => {
    ///     var item = (CustomItemElement)element;
    ///     item.SetData(itemsSource[index]);
    /// };
    /// ]]></code>
    /// <para>Using MasonryGridView in UXML:</para>
    ///
    /// <para>UXML definition of a MasonryGridView</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements">
    ///     <ui:MasonryGridView
    ///         column-count="3"
    ///         selection-type="Multiple"
    ///         pack="true"
    ///         style="height: 100%;" />
    /// </ui:UXML>
    /// ]]></code>
    /// <para>Handling selection changes:</para>
    ///
    /// <para>Setting up selection event handlers</para>
    /// <code lang="csharp"><![CDATA[
    /// masonryGrid.selectionChanged += (selectedItems) => {
    ///     foreach (var item in selectedItems) {
    ///         Debug.Log($"Selected item: {item}");
    ///     }
    /// };
    ///
    /// masonryGrid.itemsChosen += (chosenItems) => {
    ///     // Handle double-click or Enter key on selected items
    ///     foreach (var item in chosenItems) {
    ///         Debug.Log($"Chosen item: {item}");
    ///     }
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class MasonryGridView : BaseGridView
    {

        internal static readonly BindingId packProperty = new BindingId(nameof(pack));

        /// <summary>
        /// The USS class name of a <see cref="MasonryGridView"/>.
        /// </summary>
        public static readonly string masonryGridViewUssClassName = ussClassName + "--masonry";

        /// <summary>
        /// The column container USS class name of a <see cref="MasonryGridView"/>.
        /// </summary>
        public static readonly string columnContainerUssClassName = ussClassName + "__row";

        /// <summary>
        /// The columns USS class name of a <see cref="MasonryGridView"/>.
        /// </summary>
        public static readonly string columnUssClassName = ussClassName + "__column";

        readonly ObjectPool<VisualElement> m_ItemPool;

        readonly VisualElement m_ColumnContainer;

        VisualElement[] m_Columns;

        SortedDictionary<int, VisualElement>[] m_ItemsByColumn;

        IVisualElementScheduledItem m_PackTask;

        IVisualElementScheduledItem m_PostPackTask;

        IVisualElementScheduledItem m_PostRefreshTask;

        bool m_Pack;

        /// <summary>
        /// Whether to pack the items (the grid will try to take the minimum space possible
        /// by distributing the items in the columns).
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool pack
        {
            get => m_Pack;
            set
            {
                if (m_Pack != value)
                {
                    m_Pack = value;
                    Refresh();

                    NotifyPropertyChanged(in packProperty);
                }
            }
        }

        /// <summary>
        /// Constructs a new <see cref="MasonryGridView"/>.
        /// </summary>
        public MasonryGridView()
        {
            AddToClassList(masonryGridViewUssClassName);
            m_ColumnContainer = new VisualElement
            {
                name = columnContainerUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_ColumnContainer.AddToClassList(columnContainerUssClassName);
            scrollView.Add(m_ColumnContainer);

            m_ItemPool = new ObjectPool<VisualElement>(CreatePooledItem, OnTakeFromPool,
                OnReturnedToPool, OnDestroyPoolObject);
        }

        /// <inheritdoc cref="BaseGridView.Refresh"/>
        public override void Refresh()
        {
            base.Refresh();

            // clear everything
            m_PackTask?.Pause();
            m_PostPackTask?.Pause();
            m_PostRefreshTask?.Pause();
            if (m_Columns != null)
            {
                foreach (var column in m_Columns)
                {
                    column.Clear();
                    column.RemoveFromHierarchy();
                }
            }
            m_ItemsByColumn = new SortedDictionary<int, VisualElement>[columnCount];
            m_Columns = new VisualElement[columnCount];
            for (var c = 0; c < columnCount; ++c)
            {
                m_ItemsByColumn[c] = new SortedDictionary<int, VisualElement>();
                m_Columns[c] = new VisualElement
                {
                    name = columnUssClassName,
                    pickingMode = PickingMode.Ignore,
                };
                m_Columns[c].AddToClassList(columnUssClassName);
                m_Columns[c].style.width = new StyleLength(Length.Percent(100f / columnCount));
                m_Columns[c].EnableInClassList(firstColumnUssClassName, c == 0);
                m_Columns[c].EnableInClassList(lastColumnUssClassName, c == columnCount - 1);
                m_ColumnContainer.Add(m_Columns[c]);
            }
            m_ItemPool.Clear();

            // if there is no way to bind data, exit
            if (!HasValidDataAndBindings())
                return;

            // allocate items
            scrollView.Add(m_ColumnContainer);
            for (var i = 0; i < itemsSource.Count; i++)
            {
                var item = m_ItemPool.Get();
                item.userData = i;
                bindItem?.Invoke(item[0], i);
                var columnIndex = i % columnCount;
                m_ItemsByColumn[columnIndex].Add(i, item);
                m_Columns[columnIndex].Add(item);
            }

            ApplySelectedState();

            m_PostRefreshTask = schedule.Execute(PostRefresh);
        }

        void PostRefresh()
        {
            if (pack)
            {
                m_PackTask = schedule.Execute(Pack);
            }
            else
            {
                var scrollOffset = scrollView.verticalScroller.value;
                scrollOffset = float.IsNaN(scrollOffset) ? 0 : scrollOffset;
                OnScroll(scrollOffset);
            }
        }

        void Pack()
        {
            m_PostPackTask?.Pause();
            var items = new List<VisualElement>();
            for (var i = 0; i < itemsSource.Count; i++)
            {
                var columnIndex = i % columnCount;
                var item = m_ItemsByColumn[columnIndex][i];
                items.Add(item);
            }

            foreach (var dict in m_ItemsByColumn)
            {
                dict.Clear();
            }

            var columnHeights = new float[columnCount];
            for (var index = 0; index < itemsSource.Count; index++)
            {
                var smallerColumnIndex = 0;
                var smallerColumnHeight = float.MaxValue;
                for (var c = 0; c < columnCount; ++c)
                {
                    if (columnHeights[c] < smallerColumnHeight)
                    {
                        smallerColumnHeight = columnHeights[c];
                        smallerColumnIndex = c;
                    }
                }
                var item = items[index];
                m_ItemsByColumn[smallerColumnIndex].Add(index, item);
                item.RemoveFromHierarchy();
                m_Columns[smallerColumnIndex].Add(item);
                columnHeights[smallerColumnIndex] += item.layout.height;
            }

            m_PostPackTask = schedule.Execute(PostPack);
        }

        void PostPack()
        {
            var scrollOffset = scrollView.verticalScroller.value;
            scrollOffset = float.IsNaN(scrollOffset) ? 0 : scrollOffset;
            OnScroll(scrollOffset);
        }

        /// <inheritdoc cref="BaseGridView.ScrollToItem"/>
        public override void ScrollToItem(int index)
        {
            foreach (var items in m_ItemsByColumn)
            {
                if (items.TryGetValue(index, out var item))
                {
                    scrollView.ScrollTo(item);
                    return;
                }
            }
        }

        /// <inheritdoc cref="BaseGridView.GetVisualElementInternal"/>
        protected override VisualElement GetVisualElementInternal(int index)
        {
            var columnIndex = index % columnCount;
            return m_ItemsByColumn[columnIndex][index];
        }

        /// <inheritdoc cref="BaseGridView.OnCustomStyleResolved"/>
        protected override void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            // do nothing
        }

        /// <inheritdoc cref="BaseGridView.GetIndexByWorldPosition"/>
        public override int GetIndexByWorldPosition(Vector2 worldPosition)
        {
            var localPosition = scrollView.contentContainer.WorldToLocal(worldPosition);
            var viewportPosition = scrollView.contentViewport.WorldToLocal(worldPosition);
            if (viewportPosition.y <= 0 || viewportPosition.y >= scrollView.contentViewport.layout.height)
                return -1;
            if (viewportPosition.x <= 0 || viewportPosition.x >= scrollView.contentViewport.layout.width)
                return -1;

            var columnIndex = Mathf.FloorToInt(viewportPosition.x / itemWidth);
            var index = BinarySearch(m_ItemsByColumn[columnIndex], localPosition.y);
            return index;
        }

        static int BinarySearch(SortedDictionary<int, VisualElement> items, float y)
        {
            var keys = new List<int>(items.Keys);
            var min = 0;
            var max = keys.Count - 1;
            while (min <= max)
            {
                var mid = (min + max) / 2;
                var midKey = keys[mid];
                var midItem = items[midKey];
                var midLayout = midItem.layout;
                if (y >= midLayout.yMin && y <= midLayout.yMax)
                    return midKey;
                if (midLayout.yMin < y)
                    min = mid + 1;
                else
                    max = mid - 1;
            }
            return -1;
        }

        /// <inheritdoc cref="BaseGridView.OnScroll"/>
        protected override void OnScroll(float offset)
        {
            for (var c = 0; c < columnCount; ++c)
            {
                var column = m_Columns[c];
                for (var i = 0; i < column.childCount; ++i)
                {
                    var item = column[i];
                    var itemY = item.resolvedStyle.top;
                    var itemHeight = item.resolvedStyle.height;
                    var itemBottom = itemY + itemHeight;
                    var scrollViewHeight = scrollView.resolvedStyle.height;
                    var scrollViewBottom = offset + scrollViewHeight;
                    if (itemY < scrollViewBottom && itemBottom > offset)
                    {
                        if (!item.visible)
                            item.visible = true;
                    }
                    else
                    {
                        if (item.visible)
                            item.visible = false;
                    }
                }
            }
        }

        /// <inheritdoc cref="BaseGridView.OnContainerHeightChanged"/>
        protected override void OnContainerHeightChanged(float height)
        {
            OnScroll(scrollView.verticalScroller.value);
        }

        /// <inheritdoc cref="BaseGridView.ApplySelectedState"/>
        protected override void ApplySelectedState()
        {
            if (!HasValidDataAndBindings() || m_Columns == null)
                return;

            foreach (var column in m_Columns)
            {
                foreach (var item in column.Children())
                {
                    var index = (int)item.userData;
                    var id = GetIdFromIndex(index);
                    var selected = IsSelectedId(id);
                    item.EnableInClassList(Styles.selectedUssClassName, selected);
                }
            }
        }

        static VisualElement CreatePooledItem()
        {
            var item = new VisualElement();
            item.AddToClassList(itemUssClassName);
            return item;
        }

        void OnTakeFromPool(VisualElement item)
        {
            // Add the item in the visual tree but keep it hidden
            item.Add(makeItem.Invoke());
        }

        void OnReturnedToPool(VisualElement item)
        {
            if (item.childCount > 0)
                unbindItem?.Invoke(item[0], (int)item.userData);
            item.userData = null;
            item.Clear();
            item.RemoveFromHierarchy();
        }

        static void OnDestroyPoolObject(VisualElement item)
        {
            // Same behavior as returning to pool
            item.userData = null;
            item.Clear();
            item.RemoveFromHierarchy();
        }

    }
}
