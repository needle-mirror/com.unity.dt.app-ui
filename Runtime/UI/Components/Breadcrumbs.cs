using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Breadcrumbs visual element.
    /// </summary>
    public class Breadcrumbs : VisualElement
    {
        /// <summary>
        /// The Breadcrumbs' USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-breadcrumbs";
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public Breadcrumbs()
        {
            AddToClassList(ussClassName);
        }
        
        /// <summary>
        /// UXML Factory for Breadcrumbs.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Breadcrumbs, UxmlTraits> { }

        /// <summary>
        /// UXML Traits for Breadcrumbs.
        /// </summary>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            
        }
    }

    /// <summary>
    /// BreadcrumbItem visual element.
    /// </summary>
    public class BreadcrumbItem : Link
    {
        /// <summary>
        /// The BreadcrumbItem's USS class name.
        /// </summary>
        public new static readonly string ussClassName = "appui-breadcrumb-item";
        
        /// <summary>
        /// The BreadcrumbItem's active USS class name.
        /// </summary>
        public static readonly string currentUssClassName = ussClassName + "--current";

        /// <summary>
        /// Whether the BreadcrumbItem is the current item.
        /// </summary>
        public bool isCurrent
        {
            get => ClassListContains(currentUssClassName);
            set => EnableInClassList(currentUssClassName, value);
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BreadcrumbItem()
        {
            AddToClassList(ussClassName);
        }
        
        /// <summary>
        /// UXML Factory for BreadcrumbItem.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<BreadcrumbItem, UxmlTraits> { }

        /// <summary>
        /// UXML Traits for BreadcrumbItem.
        /// </summary>
        public new class UxmlTraits : Link.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_IsCurrent = new UxmlBoolAttributeDescription
            {
                name = "is-current",
                defaultValue = false
            };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var item = (BreadcrumbItem)ve;
                item.isCurrent = m_IsCurrent.GetValueFromBag(bag, cc);
            }
        }
    }

    /// <summary>
    /// BreadcrumbSeparator visual element.
    /// </summary>
    public class BreadcrumbSeparator : TextElement
    {
        /// <summary>
        /// The BreadcrumbSeparator's USS class name.
        /// </summary>
        public new static readonly string ussClassName = "appui-breadcrumb-separator";
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BreadcrumbSeparator()
        {
            AddToClassList(ussClassName);
            
            text = "/";
        }
        
        /// <summary>
        /// UXML Factory for BreadcrumbSeparator.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<BreadcrumbSeparator, UxmlTraits> { }
        
        /// <summary>
        /// UXML Traits for BreadcrumbSeparator.
        /// </summary>
        public new class UxmlTraits : TextElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var separator = (BreadcrumbSeparator)ve;
                
                if (string.IsNullOrEmpty(separator.text))
                    separator.text = "/";
            }
        }
    }
}
