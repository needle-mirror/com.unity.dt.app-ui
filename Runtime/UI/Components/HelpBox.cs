using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// HelpBox UI element.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class HelpBox : BaseVisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId messageProperty = nameof(message);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId iconNameProperty = nameof(iconName);

        internal static readonly BindingId sizeProperty = nameof(size);
#endif

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
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string message
        {
            get => m_MessageElement.text;
            set
            {
                var changed = m_MessageElement.text != value;
                m_MessageElement.text = value;

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in messageProperty);
#else
                _ = changed;
#endif
            }
        }

        /// <summary>
        /// The size of the HelpBox,
        /// which controls the size of the icon and the message text.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
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

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
#else
                _ = changed;
#endif
            }
        }

        /// <summary>
        /// The semantic variant which controls the color scheme of the HelpBox.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public AlertSemantic variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in variantProperty);
#else
                _ = changed;
#endif
            }
        }

        /// <summary>
        /// The name of the icon displayed in the HelpBox.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string iconName
        {
            get => m_IconName;
            set
            {
                var changed = m_IconName != value;
                m_IconName = value;
                m_IconElement.iconName = m_IconName;
                m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_IconName));

#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in iconNameProperty);
#else
                _ = changed;
#endif
            }
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the HelpBox.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<HelpBox, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="HelpBox"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Message = new UxmlStringAttributeDescription
            {
                name = "message",
                defaultValue = null
            };

            readonly UxmlEnumAttributeDescription<AlertSemantic> m_Variant = new UxmlEnumAttributeDescription<AlertSemantic>
            {
                name = "variant",
                defaultValue = AlertSemantic.Information
            };

            readonly UxmlStringAttributeDescription m_IconName = new UxmlStringAttributeDescription
            {
                name = "icon-name",
                defaultValue = "info"
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.S
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve">The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag">The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc">The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (HelpBox)ve;
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                element.iconName = m_IconName.GetValueFromBag(bag, cc);
                element.message = m_Message.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
