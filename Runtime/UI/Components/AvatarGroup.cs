using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The Avatar variant context.
    /// </summary>
    /// <param name="variant"> The Avatar variant.</param>
    public record AvatarVariantContext(AvatarVariant variant) : IContext
    {
        /// <summary>
        /// The Avatar variant.
        /// </summary>
        public AvatarVariant variant { get; } = variant;
    }
    
    /// <summary>
    /// The component size context.
    /// </summary>
    /// <param name="size"> The component size.</param>
    public record SizeContext(Size size) : IContext
    {
        /// <summary>
        /// The component size.
        /// </summary>
        public Size size { get; } = size;
    }

    /// <summary>
    /// The AvatarGroup spacing.
    /// </summary>
    public enum AvatarGroupSpacing
    {
        /// <summary>
        /// Small spacing.
        /// </summary>
        S,
        
        /// <summary>
        /// Medium spacing.
        /// </summary>
        M,
        
        /// <summary>
        /// Large spacing.
        /// </summary>
        L,
        
        /// <summary>
        /// No overlap between avatars.
        /// </summary>
        NoOverlap,
    }
    
    /// <summary>
    /// AvatarGroup UI element.
    /// </summary>
    public class AvatarGroup : VisualElement
    {
        /// <summary>
        /// The render surplus delegate.
        /// </summary>
        /// <param name="surplusCount">The number of surplus avatars.</param>
        /// <returns>The VisualElement to render.</returns>
        public delegate VisualElement RenderSurplusDelegate(int surplusCount);
        
        /// <summary>
        /// The AvatarGroup main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-avatar-group";
        
        /// <summary>
        /// The AvatarGroup surplus styling class.
        /// </summary>
        public static readonly string surplusUssClassName = ussClassName + "__surplus";
        
        /// <summary>
        /// The AvatarGroup spacing styling class.
        /// </summary>
        public static readonly string spacingUssClassName = ussClassName + "--spacing-";

        const int k_DefaultMax = 5;
        
        const AvatarGroupSpacing k_DefaultSpacing = AvatarGroupSpacing.M;
        
        const Size k_DefaultSize = Size.M;
        
        const AvatarVariant k_DefaultVariant = AvatarVariant.Circular;
        
        int? m_Total;

        AvatarVariant m_Variant;
        
        IList m_SourceItems;
        
        Action<Avatar, int> m_BindItem;

        Size m_Size;

        AvatarGroupSpacing m_Spacing;

        /// <summary>
        /// The AvatarGroup content container.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The maximum number of avatars to display before the overflow.
        /// </summary>
        public int max { get; set; } = k_DefaultMax;

        /// <summary>
        /// The custom render function for the surplus avatars.
        /// </summary>
        public RenderSurplusDelegate renderSurplus { get; set; } = GetDefaultSurplusElement;

        /// <summary>
        /// The spacing between avatars.
        /// </summary>
        public AvatarGroupSpacing spacing
        {
            get => m_Spacing;
            set
            {
                RemoveFromClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
                m_Spacing = value;
                AddToClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
                Refresh();
            }
        }

        /// <summary>
        /// The size of avatars.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                this.ProvideContext(new SizeContext(value));
            }
        }

        /// <summary>
        /// The AvatarGroup total count.
        /// </summary>
        public int total => m_Total ?? m_SourceItems?.Count ?? 0;

        /// <summary>
        /// The AvatarGroup variant.
        /// </summary>
        public AvatarVariant variant
        {
            get => m_Variant;
            set
            {
                m_Variant = value;
                this.ProvideContext(new AvatarVariantContext(value));
            }
        }

        /// <summary>
        /// Set a custom AvatarGroup total count instead of the child count.
        /// </summary>
        /// <param name="customTotal"> The custom total count.</param>
        /// <remarks>
        /// You can pass a null value to reset the custom total count and use the child count instead.
        /// </remarks>
        public void SetCustomTotal(int? customTotal)
        {
            m_Total = customTotal;
            Refresh();
        }

        /// <summary>
        /// The collection of items that will be displayed as Radio component.
        /// </summary>
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                m_SourceItems = value;
                Refresh();
            }
        }

        /// <summary>
        /// Method used to bind an item to a child Avatar.
        /// </summary>
        public Action<Avatar, int> bindItem
        {
            get => m_BindItem;
            set
            {
                m_BindItem = value;
                Refresh();
            }
        }
        
        /// <summary>
        /// Defines the AvatarGroup constructor.
        /// </summary>
        public AvatarGroup()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            variant = k_DefaultVariant;
            size = k_DefaultSize;
            spacing = k_DefaultSpacing;
        }

        /// <summary>
        /// Refresh the AvatarGroup.
        /// </summary>
        public void Refresh()
        {
            var currentTotal = total;
            Clear();
            
            if (m_SourceItems == null || currentTotal == 0)
                return;
            
            if (currentTotal > max)
            {
                var surplusCount = currentTotal - max;
                var surplus = renderSurplus?.Invoke(surplusCount);

                if (surplus != null)
                {
                    surplus.AddToClassList(surplusUssClassName);
                    Add(surplus);
                }
            }
            
            for (var i = Mathf.Min(currentTotal, max) - 1; i >= 0; i--)
            {
                var avatar = new Avatar();
                bindItem?.Invoke(avatar, i);
                
                Add(avatar);
            }
        }

        /// <summary>
        /// The default surplus VisualElement.
        /// </summary>
        /// <param name="surplus"> The number of surplus avatars.</param>
        /// <returns> The default surplus VisualElement.</returns>
        public static VisualElement GetDefaultSurplusElement(int surplus)
        {
            var avatar = new Avatar
            {
                backgroundColor = Color.gray
            };
            avatar.Add(new Text($"+{surplus}"));

            return avatar;
        }
        
        /// <summary>
        /// Whether the element is disabled.
        /// </summary>
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }

        /// <summary>
        /// Defines the UxmlFactory for the AvatarGroup.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AvatarGroup, UxmlTraits>
        {
            /// <summary>
            /// Describes the types of element that can appear as children of this element in a UXML file.
            /// </summary>
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield return new UxmlChildElementDescription(typeof(Avatar)); }
            }
        }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="AvatarGroup"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlIntAttributeDescription m_Max = new UxmlIntAttributeDescription
            {
                name = "max", 
                defaultValue = k_DefaultMax
            };
            
            readonly UxmlEnumAttributeDescription<AvatarGroupSpacing> m_Spacing = new UxmlEnumAttributeDescription<AvatarGroupSpacing>
            {
                name = "spacing",
                defaultValue = k_DefaultSpacing
            };
            
            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = k_DefaultSize
            };
            
            readonly UxmlIntAttributeDescription m_CustomTotal = new UxmlIntAttributeDescription
            {
                name = "total",
                defaultValue = -1
            };
            
            readonly UxmlEnumAttributeDescription<AvatarVariant> m_Variant = new UxmlEnumAttributeDescription<AvatarVariant>
            {
                name = "variant",
                defaultValue = k_DefaultVariant,
            };
            
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
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
                
                var element = (AvatarGroup)ve;
                
                element.max = m_Max.GetValueFromBag(bag, cc);
                element.spacing = m_Spacing.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.variant = m_Variant.GetValueFromBag(bag, cc);

                var total = -1;
                if (m_CustomTotal.TryGetValueFromBag(bag, cc, ref total))
                    element.SetCustomTotal(total);
                
                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
