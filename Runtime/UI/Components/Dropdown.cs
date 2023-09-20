using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Dropdown item UI element.
    /// </summary>
    public class DropdownItem : VisualElement
    {
        /// <summary>
        /// The Dropdown item main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-dropdown-item";
        
        /// <summary>
        /// The Dropdown item label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";
        
        /// <summary>
        /// The Dropdown item icon styling class.
        /// </summary>
        public static readonly string iconUssClassName = ussClassName + "__icon";
        
        /// <summary>
        /// The Dropdown item checkmark styling class.
        /// </summary>
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";
        
        /// <summary>
        /// The Dropdown item label.
        /// </summary>
        public LocalizedTextElement labelElement { get; }

        readonly Icon m_IconElement;
        
        readonly Icon m_CheckmarkElement;
        
        /// <summary>
        /// The Dropdown label text.
        /// </summary>
        public string label
        {
            get => labelElement.text;
            set => labelElement.text = value;
        }

        /// <summary>
        /// The Dropdown item icon.
        /// </summary>
        public string icon
        {
            get => m_IconElement.iconName;
            set
            {
                m_IconElement.iconName = value;
                m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DropdownItem()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            
            m_CheckmarkElement = new Icon
            {
                pickingMode = PickingMode.Ignore, 
                iconName = MenuItem.checkmarkIconName
            };
            m_CheckmarkElement.AddToClassList(checkmarkUssClassName);
            Add(m_CheckmarkElement);
            
            m_IconElement = new Icon
            {
                pickingMode = PickingMode.Ignore
            };
            m_IconElement.AddToClassList(iconUssClassName);
            Add(m_IconElement);

            labelElement = new LocalizedTextElement
            {
                name = labelUssClassName, 
                pickingMode = PickingMode.Ignore
            };
            labelElement.AddToClassList(labelUssClassName);
            Add(labelElement);

            icon = null;
            labelElement.text = null;
        }
    }
    
    /// <summary>
    /// Dropdown UI element.
    /// </summary>
    public class Dropdown : Picker<DropdownItem, DropdownItem>
    {
        /// <summary>
        /// The Dropdown main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-dropdown";
        
        Action<DropdownItem, IEnumerable<int>> m_CustomBindTitle;

        /// <summary>
        /// A method that will be called to bind the title.
        /// </summary>
        public new Action<DropdownItem, IEnumerable<int>> bindTitle
        {
            get => m_CustomBindTitle;
            set
            {
                m_CustomBindTitle = value;
                base.bindTitle = BindTitle;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dropdown()
            : this(null) { }

        /// <summary>
        /// Construct a Dropdown UI element with a provided dynamic collection of items.
        /// </summary>
        /// <param name="items">An items collection.</param>
        /// <param name="bindItemFunc">The binding function used to populate display data for each item.</param>
        /// <param name="bindTitleFunc">The binding function used to populate display data for the title.</param>
        /// <param name="defaultIndices">The selected index by default.</param>
        public Dropdown(
            IList items, 
            Action<DropdownItem, int> bindItemFunc = null, 
            Action<DropdownItem, IEnumerable<int>> bindTitleFunc = null, 
            int[] defaultIndices = null)
            : base(items, null, null, null, null, null, defaultIndices)
        {
            AddToClassList(ussClassName);

            makeItem = MakeItem;
            makeTitle = MakeTitle;
            bindItem = bindItemFunc;
            bindTitle = bindTitleFunc;
        }
        
        void BindTitle(DropdownItem item, IEnumerable<int> indices)
        {
            if (bindTitle != null)
            {
                bindTitle.Invoke(item, indices);
            }
            else if (bindItem != null && m_Value is {Count: 1})
            {
                bindItem.Invoke(item, m_Value[0]);
            }
            else
            {
                item.icon = null;
                if (m_Value == null || m_Value.Count == 0)
                    item.labelElement.text = defaultMessage;
                else
                    ApplyMultiSelectionMessage(item.labelElement, m_Value.Count);
            }
        }
        
        /// <summary>
        /// Apply the Picker localized multi selection message.
        /// </summary>
        /// <param name="element"> The <see cref="LocalizedTextElement"/> to apply the message to. </param>
        /// <param name="selectionCount"> The number of selected items. </param>
        protected virtual void ApplyMultiSelectionMessage(LocalizedTextElement element, int selectionCount)
        {
            element.variables = new object[]
            {
                new Dictionary<string, object>
                {
                    { "itemCount", selectionCount }
                }
            };
#if UNITY_LOCALIZATION_PRESENT
            element.text = "@AppUI:selectedItemsMessage";
#else
            element.text = $"{selectionCount} item{(selectionCount > 1 ? "s" : "")} selected";
#endif
        }
        
        static DropdownItem MakeTitle()
        {
            return new DropdownItem();
        }

        static DropdownItem MakeItem()
        {
            return new DropdownItem();
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="Dropdown"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Dropdown, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Dropdown"/>.
        /// </summary>
        public new class UxmlTraits : Picker<DropdownItem,DropdownItem>.UxmlTraits { }
    }
}
