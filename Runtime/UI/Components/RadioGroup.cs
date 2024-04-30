using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container for a set of <see cref="Radio"/> UI elements.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class RadioGroup : BaseVisualElement, IInputElement<int>
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        
        internal static readonly BindingId bindItemProperty = nameof(bindItem);
        
        internal static readonly BindingId sourceItemsProperty = nameof(sourceItems);
        
        internal static readonly BindingId valueProperty = nameof(value);
        
        internal static readonly BindingId invalidProperty = nameof(invalid);
        
        internal static readonly BindingId validateValueProperty = nameof(validateValue);
        
#endif
        
        /// <summary>
        /// The RadioGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-radiogroup";

        Action<Radio, int> m_BindItem;

        IList m_Items;

        int m_Value = -1;
        
        Func<int, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RadioGroup()
        {
            AddToClassList(ussClassName);
            RegisterCallback<ChangeEvent<bool>>(OnItemChosen);
        }

        /// <summary>
        /// Construct a RadioGroup UI element using a provided collection of items.
        /// </summary>
        /// <param name="items">A collection of items that will be displayed as Radio component.</param>
        /// <param name="bindItem">A function invoked to bind display data per item.</param>
        public RadioGroup(IList items, Action<Radio, int> bindItem = null)
            : this()
        {
            sourceItems = items;
            this.bindItem = bindItem;
        }

        /// <summary>
        /// The function invoked to bind display data per item.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Action<Radio, int> bindItem
        {
            get => m_BindItem;
            set
            {
                m_BindItem = value;
                if (sourceItems != null)
                    Refresh();
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in bindItemProperty);
#endif
            }
        }

        /// <summary>
        /// The RadioGroup content container.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The collection of items that will be displayed as Radio component.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public IList sourceItems
        {
            get => m_Items;
            set
            {
                m_Items = value;
                Refresh();
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in sourceItemsProperty);
#endif
            }
        }

        /// <summary>
        /// The selected item index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> if the value is out of range.</exception>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;
                if (value < -1 || value >= childCount)
                    throw new ArgumentOutOfRangeException(nameof(value));
                using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in valueProperty);
#endif
            }
        }
        
        /// <summary>
        /// The RadioGroup invalid state.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = invalid != value;
                EnableInClassList(Styles.invalidUssClassName, value);
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
#endif
            }
        }
        
        /// <summary>
        /// The RadioGroup validation function.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Func<int, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
#endif
            }
        }

        /// <summary>
        /// Set the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value.</param>
        public void SetValueWithoutNotify(int newValue)
        {
            for (var i = 0; i < childCount; i++)
            {
                if (ElementAt(i) is Radio r)
                    r.SetValueWithoutNotify(i == newValue);
            }

            m_Value = newValue;
            
            if (validateValue != null)
                invalid = !validateValue.Invoke(newValue);
        }

        void Refresh()
        {
            Clear();
            var newValue = -1;
            if (m_Items is { Count: > 0 })
            {
                for (var i = 0; i < m_Items.Count; i++)
                {
                    var item = new Radio();
                    if (bindItem != null)
                        bindItem.Invoke(item, i);
                    else
                        item.label = m_Items[i].ToString();
                    Add(item);
                }

                newValue = 0;
            }

            value = newValue;
            // if the value is the same as before, there won't be any refresh so we call SetValueWithoutNotify explicitly
            SetValueWithoutNotify(newValue);
        }

        void OnItemChosen(ChangeEvent<bool> evt)
        {
            if (evt.target is Radio radio && radio.parent == this && evt.newValue)
            {
                var newIndex = IndexOf(radio);
                value = newIndex;
            }
        }

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="RadioGroup"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<RadioGroup, UxmlTraits> { }
        
#endif
    }
}