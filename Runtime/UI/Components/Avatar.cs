using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The Avatar variant.
    /// </summary>
    public enum AvatarVariant
    {
        /// <summary>
        /// Display the Avatar component as a square.
        /// </summary>
        Square,
        
        /// <summary>
        /// Display the Avatar component as a rounded square.
        /// </summary>
        Rounded,
        
        /// <summary>
        /// Display the Avatar component as a circle.
        /// </summary>
        Circular,      
    }
    
    /// <summary>
    /// Avatar UI element.
    /// </summary>
    public class Avatar : VisualElement, ISizeableElement
    {
        /// <summary>
        /// The Avatar main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-avatar";

        /// <summary>
        /// The Avatar container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Avatar size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";
        
        /// <summary>
        /// The Avatar variant styling class.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";
        
        const Size k_DefaultSize = Size.M;
        
        const AvatarVariant k_DefaultVariant = AvatarVariant.Circular;

        Size m_Size = Size.M;

        readonly ExVisualElement m_Container;

        Color? m_BackgroundColor;

        Color? m_OutlineColor;

        float m_OutlineWidth;

        AvatarVariant m_Variant;

        /// <summary>
        /// The content container of the Avatar.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The Avatar size.
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
        /// The Avatar variant.
        /// </summary>
        public AvatarVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantUssClassName + m_Variant.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// The Avatar source image.
        /// </summary>
        public StyleBackground src
        {
            get => m_Container.style.backgroundImage;
            set => m_Container.style.backgroundImage = value;
        }

        /// <summary>
        /// The Avatar background color.
        /// </summary>
        public Color? backgroundColor
        {
            get => m_BackgroundColor;
            set
            {
                m_BackgroundColor = value;
                m_Container.style.backgroundColor = m_BackgroundColor ?? new StyleColor(StyleKeyword.Null);
            }
        }

        /// <summary>
        /// The Avatar outline width.
        /// </summary>
        public float outlineWidth
        {
            get => m_OutlineWidth;
            set
            {
                m_OutlineWidth = value;
                RefreshBorders();
            }
        }

        /// <summary>
        /// Refresh the Avatar borders.
        /// </summary>
        void RefreshBorders()
        {
            style.borderBottomWidth = m_OutlineColor.HasValue ? m_OutlineWidth : 0;
            style.borderLeftWidth = m_OutlineColor.HasValue ? m_OutlineWidth : 0;
            style.borderRightWidth = m_OutlineColor.HasValue ? m_OutlineWidth : 0;
            style.borderTopWidth = m_OutlineColor.HasValue ? m_OutlineWidth : 0;
        }

        /// <summary>
        /// The Avatar outline color.
        /// </summary>
        public Color? outlineColor
        {
            get => m_OutlineColor;
            set
            {
                m_OutlineColor = value;
                
                style.borderBottomColor = m_OutlineColor ?? new StyleColor(StyleKeyword.Null);
                style.borderLeftColor = m_OutlineColor ?? new StyleColor(StyleKeyword.Null);
                style.borderRightColor = m_OutlineColor ?? new StyleColor(StyleKeyword.Null);
                style.borderTopColor = m_OutlineColor ?? new StyleColor(StyleKeyword.Null);
                
                const float paddingValue = 1;
                
                style.paddingBottom = m_OutlineColor.HasValue ? paddingValue : 0;
                style.paddingLeft = m_OutlineColor.HasValue ? paddingValue : 0;
                style.paddingRight = m_OutlineColor.HasValue ? paddingValue : 0;
                style.paddingTop = m_OutlineColor.HasValue ? paddingValue : 0;
                
                RefreshBorders();
            }
        }

        /// <summary>
        /// The Avatar background image.
        /// </summary>
        public StyleBackground backgroundImage
        {
            get => m_Container.resolvedStyle.backgroundImage;
            set => m_Container.style.backgroundImage = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Avatar()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_Container = new ExVisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            size = k_DefaultSize;
            variant = k_DefaultVariant;
            backgroundColor = null;
            outlineColor = null;
            outlineWidth = 2;
            src = null;
            
            this.RegisterContextChangedCallback<AvatarVariantContext>(OnVariantContextChanged);
            this.RegisterContextChangedCallback<SizeContext>(OnSizeContextChanged);
        }
        
        void OnSizeContextChanged(ContextChangedEvent<SizeContext> evt)
        {
            if (evt.context != null)
                size = evt.context.size;
        }

        void OnVariantContextChanged(ContextChangedEvent<AvatarVariantContext> evt)
        {
            if (evt.context != null)
                variant = evt.context.variant;
        }

        /// <summary>
        /// Defines the UxmlFactory for the Avatar.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Avatar, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Avatar"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlColorAttributeDescription m_BackgroundColor = new UxmlColorAttributeDescription
            {
                name = "background-color",
                defaultValue = Color.gray
            };
            
            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = k_DefaultSize,
            };
            
            readonly UxmlEnumAttributeDescription<AvatarVariant> m_Variant = new UxmlEnumAttributeDescription<AvatarVariant>
            {
                name = "variant",
                defaultValue = k_DefaultVariant,
            };
            
            readonly UxmlStringAttributeDescription m_Src = new UxmlStringAttributeDescription
            {
                name = "src",
                defaultValue = null
            };
            
            readonly UxmlColorAttributeDescription m_OutlineColor = new UxmlColorAttributeDescription
            {
                name = "outline-color",
                defaultValue = Color.gray
            };
            
            readonly UxmlFloatAttributeDescription m_OutlineWidth = new UxmlFloatAttributeDescription
            {
                name = "outline-width",
                defaultValue = 2
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

                var element = (Avatar)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                var bgColor = Color.gray;
                if (m_BackgroundColor.TryGetValueFromBag(bag, cc, ref bgColor))
                    element.backgroundColor = bgColor;
                if (m_OutlineColor.TryGetValueFromBag(bag, cc, ref bgColor))
                    element.outlineColor = bgColor;
                string src = null;
                if (m_Src.TryGetValueFromBag(bag, cc, ref src))
                    element.src = new StyleBackground(Resources.Load<Texture2D>(src));
                element.outlineWidth = m_OutlineWidth.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
