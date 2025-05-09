using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A list view item visual element.
    /// </summary>
    public class ListViewItem : VisualElement
    {
        /// <summary>
        /// The ListViewItem's USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-listview-item";
        
        /// <summary>
        /// The ListViewItem's loading USS class name.
        /// </summary>
        public static readonly string loadingUssClassName = ussClassName + "--loading";
        
        /// <summary>
        /// The ListViewItem's size USS class name.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";
        
        /// <summary>
        /// The ListViewItem's title USS class name.
        /// </summary>
        public static readonly string titleUssClassName = ussClassName + "__title";
        
        /// <summary>
        /// The ListViewItem's subtitle USS class name.
        /// </summary>
        public static readonly string subtitleUssClassName = ussClassName + "__subtitle";
        
        /// <summary>
        /// The ListViewItem's thumbnail USS class name.
        /// </summary>
        public static readonly string thumbnailUssClassName = ussClassName + "__thumbnail";
        
        /// <summary>
        /// The ListViewItem's container USS class name.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";
        
        /// <summary>
        /// The ListViewItem's leading container USS class name.
        /// </summary>
        public static readonly string leadingContainerUssClassName = ussClassName + "__leading-container";
        
        /// <summary>
        /// The ListViewItem's trailing container USS class name.
        /// </summary>
        public static readonly string trailingContainerUssClassName = ussClassName + "__trailing-container";

        /// <summary>
        /// The ListViewItem's options button USS class name.
        /// </summary>
        public static readonly string optionsButtonUssClassName = ussClassName + "__options-button";

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
            set => m_Thumbnail.image = value;
        }

        /// <summary>
        /// The ListViewItem's size.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
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
            size = Size.M;
        }
    }
}
