using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A link visual element.
    /// </summary>
    public class Link : LocalizedTextElement, IPressable
    {
        /// <summary>
        /// The Link's USS class name.
        /// </summary>
        public new static readonly string ussClassName = "appui-link";
        
        /// <summary>
        /// The Link's size USS class name.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";
        
        Pressable m_Clickable;

        TextSize m_Size;

        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// The size of the link.
        /// </summary>
        public TextSize size
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
        /// The URL of the link.
        /// </summary>
        public string url { get; set; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Link() : this(null)
        {}
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The text of the link. </param>
        public Link(string text) : this(text, null)
        {}
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The text of the link. </param>
        /// <param name="url"> The URL of the link. </param>
        public Link(string text, string url) : base(text)
        {
            AddToClassList(ussClassName);

            clickable = new Pressable();
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            
            this.url = url;
            size = TextSize.M;
            
            this.AddManipulator(new KeyboardFocusController());
        }

        /// <summary>
        /// Defines the UxmlFactory for the Link.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Link, UxmlTraits> { }

        /// <summary>
        /// Class containing the UXML traits for the Link.
        /// </summary>
        public new class UxmlTraits : LocalizedTextElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlEnumAttributeDescription<TextSize> m_Size = new UxmlEnumAttributeDescription<TextSize>
            {
                name = "size",
                defaultValue = TextSize.M,
            };
            
            readonly UxmlStringAttributeDescription m_Url = new UxmlStringAttributeDescription
            {
                name = "url",
                defaultValue = null,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (Link)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                var url = m_Url.GetValueFromBag(bag, cc);
                element.url = string.IsNullOrEmpty(url) ? element.url : url;
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
