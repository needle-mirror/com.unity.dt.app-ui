using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A bottom navigation bar visual element.
    /// </summary>
    public class BottomNavBar : VisualElement
    {
        /// <summary>
        /// The BottomNavBar's USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-bottom-navbar";

        /// <summary>
        /// The content container of the BottomNavBar.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BottomNavBar()
        {
            AddToClassList(ussClassName);
            
            pickingMode = PickingMode.Ignore;
        }
    }
    
    /// <summary>
    /// A bottom navigation bar item visual element.
    /// </summary>
    public class BottomNavBarItem : VisualElement, IPressable
    {
        /// <summary>
        /// The BottomNavBarItem's USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-bottom-navbar-item";
        
        /// <summary>
        /// The BottomNavBarItem's icon USS class name.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";
        
        /// <summary>
        /// The BottomNavBarItem's label USS class name.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";

        Icon m_Icon;
        
        LocalizedTextElement m_Label;

        Pressable m_Clickable;

        IconVariant m_SelectedIconVariant = IconVariant.Regular;

        IconVariant m_IconVariant = IconVariant.Regular;

        /// <summary>
        /// The BottomNavBarItem's icon.
        /// </summary>
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                m_Icon.iconName = value;
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }
        
        /// <summary>
        /// The BottomNavBarItem's label.
        /// </summary>
        public string label
        {
            get => m_Label.text;
            set 
            {
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Whether the BottomNavBarItem is selected.
        /// </summary>
        public bool isSelected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                EnableInClassList(Styles.selectedUssClassName, value);
                m_Icon.variant = value ? m_SelectedIconVariant : m_IconVariant;
            }
        }
        
        /// <summary>
        /// The BottomNavBarItem's icon variant.
        /// </summary>
        public IconVariant iconVariant
        {
            get => m_IconVariant;
            set
            {
                m_IconVariant = value;
                
                if (!isSelected)
                    m_Icon.variant = value;
            }
        }
        
        /// <summary>
        /// The BottomNavBarItem's selected icon variant.
        /// </summary>
        public IconVariant selectedIconVariant
        {
            get => m_SelectedIconVariant;
            set
            {
                m_SelectedIconVariant = value;
                
                if (isSelected)
                    m_Icon.variant = value;
            }
        }
        
        /// <summary>
        /// Clickable Manipulator for this BottomNavBarItem.
        /// </summary>
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
        /// Default constructor.
        /// </summary>
        public BottomNavBarItem()
            : this(null, null, null)
        {
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="icon"> The BottomNavBarItem's icon. </param>
        /// <param name="label"> The BottomNavBarItem's label. </param>
        /// <param name="clickHandler"> The BottomNavBarItem's click handler. </param>
        public BottomNavBarItem(string icon, string label, Action clickHandler)
        {
            AddToClassList(ussClassName);
            
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Pressable(clickHandler);
            
            m_Icon = new Icon { iconName = icon, variant = IconVariant.Regular, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);
            
            m_Label = new LocalizedTextElement(label) { pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);
            
            this.label = label;
            this.icon = icon;
        }
    }
}
