using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// HelpBox UI element.
    /// </summary>
    [UxmlElement]
    public partial class HelpBox : BaseVisualElement
    {
        internal static readonly BindingId messageProperty = nameof(message);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId iconNameProperty = nameof(iconName);

        internal static readonly BindingId sizeProperty = nameof(size);

        /// <summary>
        /// The HelpBox main styling class.
        /// </summary>
        public const string ussClassName = "appui-helpbox";

        /// <summary>
        /// The HelpBox variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(AlertSemantic))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The HelpBox icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The HelpBox message styling class.
        /// </summary>
        public const string messageUssClassName = ussClassName + "__message";

        /// <summary>
        /// The HelpBox size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly Icon m_IconElement;
        readonly LocalizedTextElement m_MessageElement;
        Size m_Size;
        string m_IconName;

        AlertSemantic m_Variant;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HelpBox()
            : this(null) { }

        /// <summary>
        /// Constructor with a message and optional variant.
        /// </summary>
        /// <param name="msg">The message to display.</param>
        /// <param name="variant">The semantic variant controlling the color scheme.</param>
        /// <param name="icon">The icon name to display.</param>
        public HelpBox(string msg, AlertSemantic variant = AlertSemantic.Information, string icon = "info")
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_IconElement = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);

            m_MessageElement = new LocalizedTextElement
            {
                name = messageUssClassName,
                pickingMode = PickingMode.Ignore,
                enableRichText = true,
            };
            m_MessageElement.AddToClassList(messageUssClassName);

            hierarchy.Add(m_IconElement);
            hierarchy.Add(m_MessageElement);

            message = msg;
            this.variant = variant;
            iconName = icon;
            size = Size.S;
        }

        /// <summary>
        /// The message displayed inside the HelpBox.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string message
        {
            get => m_MessageElement.text;
            set
            {
                var changed = m_MessageElement.text != value;
                m_MessageElement.text = value;

                if (changed)
                    NotifyPropertyChanged(in messageProperty);
            }
        }

        /// <summary>
        /// The size of the HelpBox,
        /// which controls the size of the icon and the message text.
        /// </summary>
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
                m_IconElement.size = m_Size.ToIconSize();

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The semantic variant which controls the color scheme of the HelpBox.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AlertSemantic variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// The name of the icon displayed in the HelpBox.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string iconName
        {
            get => m_IconName;
            set
            {
                var changed = m_IconName != value;
                m_IconName = value;
                m_IconElement.iconName = m_IconName;
                m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_IconName));

                if (changed)
                    NotifyPropertyChanged(in iconNameProperty);
            }
        }

    }
}
