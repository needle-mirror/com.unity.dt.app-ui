using System;
using System.Collections;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A vertical, virtualized, single-column collection of <see cref="ListViewItem"/> elements.
    /// </summary>
    /// <remarks>
    /// ListBox is the container that <see cref="ListViewItem"/> lives in. It is a single-column
    /// <see cref="GridView"/>, so it inherits the whole App UI collection contract: an
    /// <see cref="BaseGridView.itemsSource"/> data source, <see cref="BaseGridView.makeItem"/> and
    /// <see cref="BaseGridView.bindItem"/> recycling, row virtualization, a selection model
    /// (<see cref="BaseGridView.selectionType"/>) and keyboard navigation.
    ///
    /// This is deliberately App UI's own implementation rather than a wrapper around
    /// <see cref="UnityEngine.UIElements.ListView"/>, exactly as <see cref="BaseGridView"/> and
    /// <see cref="GridView"/> are. It keeps the App UI context system (theme, direction, density,
    /// <see cref="SizeContext"/>), the App UI styling vocabulary and the App UI selection semantics
    /// in one place, instead of straddling two collection frameworks.
    ///
    /// **Note:** like <see cref="GridView"/>, the row height is fixed and must agree with the height
    /// your items actually render at, because virtualization computes the scroll extent from it. Set
    /// <see cref="GridView.itemHeight"/>, or drive it from USS with `--unity-item-height`.
    ///
    /// If you do not provide <see cref="BaseGridView.makeItem"/> and <see cref="BaseGridView.bindItem"/>,
    /// ListBox builds a <see cref="ListViewItem"/> per row and puts the data item's string
    /// representation in its <see cref="ListViewItem.title"/>.
    /// </remarks>
    /// <example>
    /// <para>Basic usage — a list of strings with the default item and single selection.</para>
    /// <code lang="csharp"><![CDATA[
    /// var list = new ListBox
    /// {
    ///     itemsSource = new List<string> { "Materials", "Textures", "Prefabs" },
    ///     itemHeight = 40,
    ///     size = Size.Md,
    /// };
    /// list.selectionChanged += items => Debug.Log($"Selected: {string.Join(", ", items)}");
    /// ]]></code>
    /// <para>Custom items — supplying your own <see cref="ListViewItem"/> factory and binder.</para>
    /// <code lang="csharp"><![CDATA[
    /// var list = new ListBox
    /// {
    ///     itemsSource = assets,
    ///     itemHeight = 56,
    ///     selectionType = SelectionType.Multiple,
    ///     makeItem = () => new ListViewItem(),
    ///     bindItem = (element, index) =>
    ///     {
    ///         var asset = (Asset)assets[index];
    ///         var item = (ListViewItem)element;
    ///         item.title = asset.name;
    ///         item.subtitle = asset.path;
    ///         item.thumbnail = asset.preview;
    ///     },
    /// };
    /// ]]></code>
    /// <para>Using ListBox in a UXML template — declaring a ListBox in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:ListBox
    ///         size="Md"
    ///         item-height="40"
    ///         selection-type="Single"
    ///         allow-no-selection="true" />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class ListBox : GridView
    {
        internal static readonly BindingId sizeProperty = nameof(size);

        /// <summary>
        /// The ListBox's main USS class name.
        /// </summary>
        /// <remarks>
        /// This class is added on top of <see cref="BaseGridView.ussClassName"/>, so the shared grid
        /// rules keep applying.
        /// </remarks>
        public new const string ussClassName = "appui-list-box";

        /// <summary>
        /// The ListBox's size USS class name prefix.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        const Size k_DefaultSize = Size.Md;

        Size m_Size;

        /// <summary>
        /// Creates a <see cref="ListBox"/> with all default properties.
        /// </summary>
        public ListBox()
        {
            AddToClassList(ussClassName);

            columnCount = 1;
            makeItem = DefaultMakeItem;
            bindItem = DefaultBindItem;

            m_Size = (Size)(-1);
            size = k_DefaultSize;
        }

        /// <summary>
        /// Constructs a <see cref="ListBox"/>, with all required properties provided.
        /// </summary>
        /// <param name="itemsSource">The list of items to use as a data source.</param>
        /// <param name="makeItem">The factory method to call to create a display item. The method should return a
        /// VisualElement that can be bound to a data item.</param>
        /// <param name="bindItem">The method to call to bind a data item to a display item. The method
        /// receives as parameters the display item to bind, and the index of the data item to bind it to.</param>
        public ListBox(IList itemsSource, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem)
            : base(itemsSource, makeItem, bindItem)
        {
            AddToClassList(ussClassName);

            columnCount = 1;

            m_Size = (Size)(-1);
            size = k_DefaultSize;
        }

        /// <summary>
        /// The size of the ListBox's rows.
        /// </summary>
        /// <remarks>
        /// The size is published to the subtree through <see cref="SizeContext"/>, so every
        /// <see cref="ListViewItem"/> the ListBox realizes adopts it, the same way
        /// <see cref="AvatarGroup"/> sizes its avatars. Setting <see cref="ListViewItem.size"/>
        /// on an individual item after the fact still wins for that item.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                this.ProvideContext(new SizeContext(value));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        static VisualElement DefaultMakeItem() => new ListViewItem();

        void DefaultBindItem(VisualElement element, int index)
        {
            if (element is not ListViewItem item)
                return;

            var source = itemsSource;
            item.title = source != null && index >= 0 && index < source.Count
                ? source[index]?.ToString()
                : null;
        }
    }
}
