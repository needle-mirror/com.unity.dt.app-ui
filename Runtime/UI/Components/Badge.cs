using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The variant of the Badge.
    /// </summary>
    public enum BadgeVariant
    {
        /// <summary>
        /// The default variant. The Badge will contain some text.
        /// </summary>
        Default,

        /// <summary>
        /// The dot variant. The Badge will be a small dot.
        /// </summary>
        Dot
    }

    /// <summary>
    /// The overlap type of the Badge.
    /// </summary>
    public enum BadgeOverlapType
    {
        /// <summary>
        /// The Badge overlap type is rectangular.
        /// </summary>
        Rectangular,
        
        /// <summary>
        /// The Badge overlap type is circular.
        /// </summary>
        Circular
    }
    
    /// <summary>
    /// A horizontal anchor.
    /// </summary>
    public enum HorizontalAnchor
    {
        /// <summary>
        /// The element is anchored at the left.
        /// </summary>
        Left,
        
        /// <summary>
        /// The element is anchored at the right.
        /// </summary>
        Right
    }
    
    /// <summary>
    /// A vertical anchor.
    /// </summary>
    public enum VerticalAnchor
    {
        /// <summary>
        /// The element is anchored at the top.
        /// </summary>
        Top,
        
        /// <summary>
        /// The element is anchored at the bottom.
        /// </summary>
        Bottom
    }
    
    /// <summary>
    /// Badge UI element.
    /// </summary>
    public class Badge : VisualElement
    {
        /// <summary>
        /// The Badge main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-badge";
        
        /// <summary>
        /// The Badge label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";
        
        /// <summary>
        /// The Badge badge styling class.
        /// </summary>
        public static readonly string badgeUssClassName = ussClassName + "__badge";
        
        /// <summary>
        /// The Badge Zero content styling class.
        /// </summary>
        public static readonly string zeroUssClassName = ussClassName + "--zero";
        
        /// <summary>
        /// The Badge variant styling class prefix.
        /// </summary>
        public static readonly string variantClassName = ussClassName + "--";
        
        /// <summary>
        /// The Badge overlap type styling class prefix.
        /// </summary>
        public static readonly string overlapUssClassName = ussClassName + "--overlap-";
        
        /// <summary>
        /// The Badge horizontal anchor styling class prefix.
        /// </summary>
        public static readonly string horizontalAnchorUssClassName = ussClassName + "--anchor-horizontal-";
        
        /// <summary>
        /// The Badge vertical anchor styling class prefix.
        /// </summary>
        public static readonly string verticalAnchorUssClassName = ussClassName + "--anchor-vertical-";
        
        Color? m_BackgroundColor;

        BadgeVariant m_Variant;

        BadgeOverlapType m_BadgeOverlapType;

        HorizontalAnchor m_HorizontalAnchor;

        VerticalAnchor m_VerticalAnchor;

        int m_Content;

        readonly Text m_LabelElement;

        bool m_ShowZero;

        int m_Max;

        Color? m_Color;

        readonly VisualElement m_BadgeElement;

        /// <summary>
        /// The content container of the Badge.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The background color of the Badge.
        /// </summary>
        public Color? backgroundColor
        {
            get => m_BackgroundColor;
            set
            {
                m_BackgroundColor = value;
                m_BadgeElement.style.backgroundColor = m_BackgroundColor ?? new StyleColor(StyleKeyword.Null);
            }
        }
        
        /// <summary>
        /// The content color of the Badge.
        /// </summary>
        public Color? color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                m_LabelElement.style.color = m_Color ?? new StyleColor(StyleKeyword.Null);
            }
        }
        
        /// <summary>
        /// The variant of the Badge.
        /// </summary>
        public BadgeVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantClassName + m_Variant.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// The overlap type of the Badge.
        /// </summary>
        public BadgeOverlapType overlapType
        {
            get => m_BadgeOverlapType;
            set
            {
                RemoveFromClassList(overlapUssClassName + m_BadgeOverlapType.ToString().ToLower());
                m_BadgeOverlapType = value;
                AddToClassList(overlapUssClassName + m_BadgeOverlapType.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// The horizontal anchor of the Badge.
        /// </summary>
        public HorizontalAnchor horizontalAnchor
        {
            get => m_HorizontalAnchor;
            set
            {
                RemoveFromClassList(horizontalAnchorUssClassName + m_HorizontalAnchor.ToString().ToLower());
                m_HorizontalAnchor = value;
                AddToClassList(horizontalAnchorUssClassName + m_HorizontalAnchor.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// The vertical anchor of the Badge.
        /// </summary>
        public VerticalAnchor verticalAnchor
        {
            get => m_VerticalAnchor;
            set
            {
                RemoveFromClassList(verticalAnchorUssClassName + m_VerticalAnchor.ToString().ToLower());
                m_VerticalAnchor = value;
                AddToClassList(verticalAnchorUssClassName + m_VerticalAnchor.ToString().ToLower());
            }
        }

        /// <summary>
        /// The text of the Badge.
        /// </summary>
        public int content
        {
            get => m_Content;
            set
            {
                m_Content = value;
                RefreshContent();
            }
        }

        /// <summary>
        /// The maximum value of the Badge.
        /// </summary>
        public int max
        {
            get => m_Max;
            set
            {
                m_Max = value;
                RefreshContent();
            }
        }

        /// <summary>
        /// Whether the Badge should show zero values.
        /// </summary>
        public bool showZero
        {
            get => m_ShowZero;
            set
            {
                m_ShowZero = value;
                RefreshContent();
            }
        }

        void RefreshContent()
        {
            m_LabelElement.text = m_Content > m_Max ? $"{m_Max}+" : m_Content.ToString();
            EnableInClassList(zeroUssClassName, !m_ShowZero && (m_Max == 0 || m_Content == 0));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Badge()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_BadgeElement = new VisualElement {name = badgeUssClassName, pickingMode = PickingMode.Ignore};
            m_BadgeElement.AddToClassList(badgeUssClassName);
            hierarchy.Add(m_BadgeElement);

            m_LabelElement = new Text { pickingMode = PickingMode.Ignore, name = labelUssClassName };
            m_LabelElement.AddToClassList(labelUssClassName);
            m_BadgeElement.Add(m_LabelElement);

            backgroundColor = null;
            color = null;
            variant = BadgeVariant.Default;
            overlapType = BadgeOverlapType.Rectangular;
            horizontalAnchor = HorizontalAnchor.Right;
            verticalAnchor = VerticalAnchor.Top;
            showZero = false;
            max = int.MaxValue;
            content = 0;
        }

        /// <summary>
        /// Defines the UxmlFactory for the Badge.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Badge, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Badge"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlColorAttributeDescription m_BackgroundColor = new UxmlColorAttributeDescription
            {
                name = "background-color",
                defaultValue = new Color(1, 0.3f, 0.3f)
            };
            
            readonly UxmlEnumAttributeDescription<BadgeVariant> m_Variant = new UxmlEnumAttributeDescription<BadgeVariant>
            {
                name = "variant",
                defaultValue = BadgeVariant.Default
            };
            
            readonly UxmlEnumAttributeDescription<BadgeOverlapType> m_OverlapType = new UxmlEnumAttributeDescription<BadgeOverlapType>
            {
                name = "overlap-type",
                defaultValue = BadgeOverlapType.Rectangular
            };
            
            readonly UxmlEnumAttributeDescription<HorizontalAnchor> m_HorizontalAnchor = new UxmlEnumAttributeDescription<HorizontalAnchor>
            {
                name = "horizontal-anchor",
                defaultValue = HorizontalAnchor.Right
            };
            
            readonly UxmlEnumAttributeDescription<VerticalAnchor> m_VerticalAnchor = new UxmlEnumAttributeDescription<VerticalAnchor>
            {
                name = "vertical-anchor",
                defaultValue = VerticalAnchor.Top
            };
            
            readonly UxmlIntAttributeDescription m_Content = new UxmlIntAttributeDescription
            {
                name = "content",
                defaultValue = 0
            };
            
            readonly UxmlIntAttributeDescription m_Max = new UxmlIntAttributeDescription
            {
                name = "max",
                defaultValue = int.MaxValue
            };
            
            readonly UxmlBoolAttributeDescription m_ShowZero = new UxmlBoolAttributeDescription
            {
                name = "show-zero",
                defaultValue = false
            };
            
            readonly UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription
            {
                name = "color",
                defaultValue = Color.white
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

                var el = (Badge)ve;
                
                var color = Color.clear;
                if (m_BackgroundColor.TryGetValueFromBag(bag, cc, ref color))
                    el.backgroundColor = color;
                
                if (m_Color.TryGetValueFromBag(bag, cc, ref color))
                    el.color = color;
                
                el.variant = m_Variant.GetValueFromBag(bag, cc);
                el.overlapType = m_OverlapType.GetValueFromBag(bag, cc);
                el.horizontalAnchor = m_HorizontalAnchor.GetValueFromBag(bag, cc);
                el.verticalAnchor = m_VerticalAnchor.GetValueFromBag(bag, cc);
                el.content = m_Content.GetValueFromBag(bag, cc);
                el.max = m_Max.GetValueFromBag(bag, cc);
                el.showZero = m_ShowZero.GetValueFromBag(bag, cc);
            }
        }
    }
}
