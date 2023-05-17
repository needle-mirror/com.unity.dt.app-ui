using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Sizing values for <see cref="Icon"/> elements.
    /// </summary>
    public enum IconSize
    {
        /// <summary>
        /// Extra extra small
        /// </summary>
        XXS,
        
        /// <summary>
        /// Extra small
        /// </summary>
        XS,
        
        /// <summary>
        /// Small
        /// </summary>
        S,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L
    }

    /// <summary>
    /// Variant values for <see cref="Icon"/> elements.
    /// </summary>
    public enum IconVariant
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 1,
        
        /// <summary>
        /// Bold
        /// </summary>
        Bold,
        
        /// <summary>
        /// DuoTone
        /// </summary>
        DuoTone,
        
        /// <summary>
        /// Light
        /// </summary>
        Light,
        
        /// <summary>
        /// Fill
        /// </summary>
        Fill,
        
        /// <summary>
        /// Thin
        /// </summary>
        Thin
    }

    /// <summary>
    /// Icon UI component.
    /// </summary>
    public class Icon : Image
    {
        /// <summary>
        /// The Icon main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-icon";

        /// <summary>
        /// The Icon primary variant styling class.
        /// </summary>
        public static readonly string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Icon size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        string m_IconName;

        IconSize m_Size;

        IconVariant m_Variant = IconVariant.Regular;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Icon()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            iconName = "info";
            size = IconSize.M;
            primary = true;
            scaleMode = ScaleMode.ScaleToFit;
        }

        /// <summary>
        /// The primary variant of the Icon.
        /// </summary>
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set => EnableInClassList(primaryUssClassName, value);
        }

        /// <summary>
        /// The size of the Icon.
        /// </summary>
        public IconSize size
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
        /// The name of the Icon.
        /// </summary>
        public string iconName
        {
            get => m_IconName;
            set
            {
                RemoveFromClassList(ussClassName + "--" + m_IconName + "--" + m_Variant.ToString().ToLower());
                RemoveFromClassList(ussClassName + "--" + m_IconName);
                m_IconName = value;
                AddToClassList(ussClassName + "--" + m_IconName + "--" + m_Variant.ToString().ToLower());
                AddToClassList(ussClassName + "--" + m_IconName);
            }
        }

        /// <summary>
        /// The variant of the Icon.
        /// </summary>
        public IconVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(ussClassName + "--" + m_IconName + "--" + m_Variant.ToString().ToLower());
                RemoveFromClassList(ussClassName + "--" + m_IconName);
                m_Variant = value;
                AddToClassList(ussClassName + "--" + m_IconName + "--" + m_Variant.ToString().ToLower());
                AddToClassList(ussClassName + "--" + m_IconName);
            } 
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Icon"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Icon, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Icon"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_IconName = new UxmlStringAttributeDescription
            {
                name = "icon-name",
                defaultValue = "info",
            };

            readonly UxmlBoolAttributeDescription m_Primary = new UxmlBoolAttributeDescription
            {
                name = "primary",
                defaultValue = true,
            };
            
            readonly UxmlEnumAttributeDescription<IconVariant> m_Variant = new UxmlEnumAttributeDescription<IconVariant>
            {
                name = "variant",
                defaultValue = IconVariant.Regular,
            };

            readonly UxmlEnumAttributeDescription<IconSize> m_Size = new UxmlEnumAttributeDescription<IconSize>
            {
                name = "size",
                defaultValue = IconSize.M,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);

                var element = (Icon)ve;
                element.primary = m_Primary.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.iconName = m_IconName.GetValueFromBag(bag, cc);
                element.variant = m_Variant.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }
}
