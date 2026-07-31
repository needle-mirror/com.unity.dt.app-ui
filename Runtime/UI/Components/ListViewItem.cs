using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A versatile list item component that displays content with a title, subtitle, thumbnail, and optional
    /// actions.
    /// </summary>
    /// <remarks>
    /// ListViewItem is a flexible component designed to display content in a list format. It provides a
    /// consistent layout structure with support for leading (left) and trailing (right) content areas, making it
    /// perfect for displaying items in lists, menus, or selection interfaces.
    ///
    /// The component supports various content elements including:
    /// - A thumbnail image
    /// - A primary title
    /// - A secondary subtitle
    /// - Leading and trailing containers for custom content
    /// - A built-in options button
    ///
    /// **Note:** ListViewItem comes with different size variants and loading states to accommodate various use
    /// cases and provide better user feedback during data loading.
    /// </remarks>
    /// <example>
    /// <para>Basic usage with title and subtitle — Creating a standard list item with title and subtitle.</para>
    /// <code lang="csharp"><![CDATA[
    /// var item = new ListViewItem
    /// {
    ///     title = "Project Assets",
    ///     subtitle = "Contains 42 files",
    ///     size = Size.M
    /// };
    /// ]]></code>
    /// <para>Advanced usage with custom content and loading state — Creating a rich list item with custom content,
    /// loading state, and event handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var item = new ListViewItem();
    ///
    /// // Add custom badge to trailing container
    /// var badge = new Badge { label = "Updated" };
    /// item.trailingContainer.Add(badge);
    ///
    /// // Configure item properties
    /// item.title = "Documentation";
    /// item.subtitle = "Last updated: Today";
    /// item.thumbnail = documentIcon;
    ///
    /// // Handle options button click
    /// item.optionsButton.clicked += () => ShowOptionsMenu();
    ///
    /// // Show loading state
    /// item.isLoading = true;
    /// await LoadDataAsync();
    /// item.isLoading = false;
    /// ]]></code>
    /// <para>Using ListViewItem in a UXML template — Declaring a ListViewItem in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:ListViewItem
    ///         title="Settings"
    ///         subtitle="Configure application preferences"
    ///         size="L"
    ///     />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [VisualDocPage("layouts")]
    public partial class ListViewItem : BaseVisualElement
    {
        /// <summary>
        /// The ListViewItem's USS class name.
        /// </summary>
        public const string ussClassName = "appui-listview-item";

        /// <summary>
        /// The ListViewItem's loading USS class name.
        /// </summary>
        public const string loadingUssClassName = ussClassName + "--loading";

        /// <summary>
        /// The ListViewItem's size USS class name.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ListViewItem's title USS class name.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The ListViewItem's subtitle USS class name.
        /// </summary>
        public const string subtitleUssClassName = ussClassName + "__subtitle";

        /// <summary>
        /// The ListViewItem's thumbnail USS class name.
        /// </summary>
        public const string thumbnailUssClassName = ussClassName + "__thumbnail";

        /// <summary>
        /// The ListViewItem's container USS class name.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The ListViewItem's leading container USS class name.
        /// </summary>
        public const string leadingContainerUssClassName = ussClassName + "__leading-container";

        /// <summary>
        /// The ListViewItem's trailing container USS class name.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailing-container";

        /// <summary>
        /// The ListViewItem's options button USS class name.
        /// </summary>
        public const string optionsButtonUssClassName = ussClassName + "__options-button";

        VisualElement m_Container;

        LocalizedTextElement m_Title;

        LocalizedTextElement m_Subtitle;

        Image m_Thumbnail;

        Size m_Size;

        /// <summary>
        /// Content container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The ListViewItem's title.
        /// </summary>
        public string title
        {
            get => m_Title.text;
            set => m_Title.text = value;
        }

        /// <summary>
        /// The ListViewItem's subtitle.
        /// </summary>
        public string subtitle
        {
            get => m_Subtitle.text;
            set => m_Subtitle.text = value;
        }

        /// <summary>
        /// The ListViewItem's thumbnail.
        /// </summary>
        public Texture thumbnail
        {
            get => m_Thumbnail.image;
            set
            {
                m_Thumbnail.image = value;
                m_Thumbnail.EnableInClassList(Styles.hiddenUssClassName, !value);
            }
        }

        /// <summary>
        /// The ListViewItem's size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
            }
        }

        /// <summary>
        /// The ListViewItem's loading state.
        /// </summary>
        public bool isLoading
        {
            get => ClassListContains(loadingUssClassName);
            set
            {
                EnableInClassList(loadingUssClassName, value);

                if (!value)
                {
                    pickingMode = PickingMode.Position;
                    focusable = true;
                    tabIndex = 0;
                }
                else
                {
                    pickingMode = PickingMode.Ignore;
                    focusable = false;
                    tabIndex = -1;
                }
            }
        }

        /// <summary>
        /// The ListViewItem's leading container.
        /// </summary>
        public VisualElement leadingContainer { get; }

        /// <summary>
        /// The ListViewItem's trailing container.
        /// </summary>
        public VisualElement trailingContainer { get; }

        /// <summary>
        /// The ListViewItem's options button.
        /// </summary>
        public IconButton optionsButton { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ListViewItem()
        {
            AddToClassList(ussClassName);

            leadingContainer = new VisualElement {pickingMode = PickingMode.Ignore};
            leadingContainer.AddToClassList(leadingContainerUssClassName);
            hierarchy.Add(leadingContainer);

            m_Thumbnail = new Image { pickingMode = PickingMode.Ignore };
            m_Thumbnail.AddToClassList(thumbnailUssClassName);
            leadingContainer.Add(m_Thumbnail);

            m_Container = new VisualElement { pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            m_Title = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);
            m_Container.Add(m_Title);

            m_Subtitle = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Subtitle.AddToClassList(subtitleUssClassName);
            m_Container.Add(m_Subtitle);

            trailingContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            trailingContainer.AddToClassList(trailingContainerUssClassName);
            hierarchy.Add(trailingContainer);

            optionsButton = new IconButton {icon = "info", quiet = true };
            optionsButton.AddToClassList(optionsButtonUssClassName);
            trailingContainer.Add(optionsButton);

            isLoading = false;
            thumbnail = null;
            size = Size.M;
        }
    }
}
