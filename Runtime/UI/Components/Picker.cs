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
    /// The type of selection for a Picker.
    /// </summary>
    public enum PickerSelectionType
    {
        /// <summary>
        /// The Picker allows only one item to be selected.
        /// </summary>
        Single,
        
        /// <summary>
        /// The Picker allows multiple items to be selected.
        /// </summary>
        Multiple
    }
    
    /// <summary>
    /// An Item from a Picker.
    /// </summary>
    public class PickerItem : VisualElement, IPressable
    {
        /// <summary>
        /// The main styling class for the PickerItem.
        /// </summary>
        public static readonly string ussClassName = "appui-picker-item";

        Pressable m_Clickable;

        /// <summary>
        /// Clickable Manipulator for this MenuItem.
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
        public PickerItem()
        {
            AddToClassList(ussClassName);
            
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Pressable(OnClick);
            
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.AddManipulator(new KeyboardFocusController());
        }
        
        void OnClick()
        {
            using var evt = ActionTriggeredEvent.GetPooled();
            evt.target = this;
            SendEvent(evt);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    if (parent.IndexOf(this) != parent.childCount - 1)
                        focusController.FocusNextInDirectionEx(VisualElementFocusChangeDirection.right);
                    handled = true;
                    break;
                case KeyCode.UpArrow:
                    if (parent.IndexOf(this) != 0)
                        focusController.FocusNextInDirectionEx(VisualElementFocusChangeDirection.left);
                    handled = true;
                    break;
                case KeyCode.RightArrow:
                case KeyCode.LeftArrow:
                    handled = true;
                    break;
            }

            if (handled)
            {
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }
    }
    
    /// <summary>
    /// Picker UI element.
    /// </summary>
    public abstract class Picker : ExVisualElement, INotifyValueChanged<IEnumerable<int>>, ISizeableElement, IPressable
    {
        /// <summary>
        /// The Picker main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-picker";

        /// <summary>
        /// The Picker title container styling class.
        /// </summary>
        public static readonly string titleContainerUssClassName = ussClassName + "__titlecontainer";

        /// <summary>
        /// The Picker title styling class.
        /// </summary>
        public static readonly string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The Picker trailing container styling class.
        /// </summary>
        public static readonly string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The Picker caret styling class.
        /// </summary>
        public static readonly string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The Picker size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Picker emphasized mode styling class.
        /// </summary>
        public static readonly string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Picker menu styling class.
        /// </summary>
        public static readonly string appuiPickerMenu = ussClassName + "__menu";

        protected readonly List<PickerItem> m_Items = new List<PickerItem>();

        int[] m_DefaultValue = null;

        Size m_Size;

        IList m_SourceItems;

        protected readonly List<int> m_Value = new List<int>();

        bool m_ValueSet;

        Pressable m_Clickable;

        MenuBuilder m_MenuBuilder;
        
        protected readonly VisualElement m_TitleContainer;

        PickerSelectionType m_SelectionType;

        string m_DefaultMessage =
#if UNITY_LOCALIZATION_PRESENT
            "@AppUI:dropdownSelectMessage";
#else 
            "Select";
#endif

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Picker()
            : this(null) { }

        /// <summary>
        /// Construct a Picker UI element with a provided dynamic collection of items.
        /// </summary>
        /// <param name="items">An items collection.</param>
        /// <param name="defaultIndices">The selected index by default.</param>
        public Picker(IList items, int[] defaultIndices = null)
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(OnClicked);
            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;

            m_TitleContainer = new VisualElement { name = titleContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_TitleContainer.AddToClassList(titleContainerUssClassName);

            var trailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            trailingContainer.AddToClassList(trailingContainerUssClassName);

            var caret = new Icon { name = caretUssClassName, iconName = "caret-down", pickingMode = PickingMode.Ignore };
            caret.AddToClassList(caretUssClassName);

            hierarchy.Add(m_TitleContainer);
            hierarchy.Add(trailingContainer);
            trailingContainer.hierarchy.Add(caret);

            defaultValue = defaultIndices;
            size = Size.M;
            sourceItems = items;
            
            if (defaultIndices is { Length: >0 })
                SetValueWithoutNotify(defaultIndices);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }
        
        /// <summary>
        /// Close the picker menu automatically when an item is selected.
        /// </summary>
        public bool closeOnSelection { get; set; } = true;

        /// <summary>
        /// Clickable Manipulator for this Picker.
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
        /// The Picker default message when no item is selected.
        /// </summary>
        public string defaultMessage
        {
            get => m_DefaultMessage;
            set
            {
                m_DefaultMessage = value;
                RefreshUI();
            }
        }

        /// <summary>
        /// The Picker default value. This is the value that will be selected if no value is set.
        /// </summary>
        public int[] defaultValue
        {
            get => m_DefaultValue;

            set
            {
                m_DefaultValue = value;
                if (!m_ValueSet)
                    SetValueWithoutNotify(m_DefaultValue);
            }
        }

        /// <summary>
        /// The source items collection.
        /// </summary>
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                if (m_SourceItems == value)
                    return;

                m_SourceItems = value;
                m_ValueSet = false;
                RefreshUI();
            }
        }

        /// <summary>
        /// The Picker size.
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
        /// The Picker emphasized mode.
        /// </summary>
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set => EnableInClassList(emphasizedUssClassName, value);
        }

        public PickerSelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                m_SelectionType = value;
                this.value = new int[] { };
            }
        }

        /// <summary>
        /// Set the Picker value without notifying any listeners.
        /// </summary>
        /// <param name="newValue"> The new value to set. </param>
        public void SetValueWithoutNotify(IEnumerable<int> newValue)
        {
            var values = newValue != null ? new List<int>(newValue) : new List<int>();
            foreach (var nv in values)
            {
                if (nv < 0 || nv >= sourceItems.Count)
                {
                    Debug.LogWarning($"<b>[App UI]</b> [Picker]: Trying to set a value out of range. " +
                        $"Value: {nv}, Range: [0, {sourceItems.Count - 1}]");
                    return;
                }
            }
            
            m_ValueSet = true;
            m_Value.Clear();
            m_Value.AddRange(values);
            RefreshTitleUI();

            var menu = m_MenuBuilder?.currentMenu;
            if (menu != null)
            {
                for (var i = 0; i < menu.childCount; i++)
                {
                    var item = menu[i];
                    var isSelected = m_Value.Contains(i);
                    item.EnableInClassList(Styles.selectedUssClassName, isSelected);
                }
            }
        }

        /// <summary>
        /// The Picker value. This is the index of the selected item.
        /// </summary>
        public IEnumerable<int> value
        {
            get => m_Value;

            set
            {
                if (EnumerableExtensions.SequenceEqual(m_Value, value))
                    return;

                using var evt = ChangeEvent<IEnumerable<int>>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }
        
        /// <summary>
        /// Refresh the Picker UI.
        /// </summary>
        public void Refresh()
        {
            RefreshUI();
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        void OnClicked()
        {
            m_MenuBuilder?.Dismiss(DismissType.Consecutive);
            if (sourceItems is not { Count: > 0 })
                return;

            m_MenuBuilder = MenuBuilder.Build(this, BuildPickerMenu());
            AddToClassList(Styles.openUssClassName);
            m_MenuBuilder.SetCloseOnSelection(closeOnSelection);
            m_MenuBuilder.dismissed += (_, _) =>
            {
                RemoveFromClassList(Styles.openUssClassName);
                m_MenuBuilder = null;
            };
            m_MenuBuilder.Show();
        }

        protected void RefreshUI()
        {
            RefreshListUI();
            RefreshTitleUI();
            // SetValueWithoutNotify(m_Value);
        }

        protected void RefreshListUI()
        {
            // clear items
            for (var i = 0; i < m_Items.Count; i++)
            {
                OnUnbindItem(m_Items[i], i);
                m_Items[i].clickable.clickedWithEventInfo -= OnItemClicked;
            }
            m_Items.Clear();

            // create menu items
            if (sourceItems != null)
            {
                for (var i = 0; i < sourceItems.Count; i++)
                {
                    var item = new PickerItem();
                    item.clickable.clickedWithEventInfo += OnItemClicked;
                    var content = OnRequestItemCreation(i);
                    item.Add(content);
                    m_Items.Add(item);
                }
            }
        }

        protected abstract void RefreshTitleUI();

        protected abstract VisualElement OnRequestItemCreation(int i);

        protected abstract void OnUnbindItem(VisualElement item, int index);

        void OnItemClicked(EventBase evt)
        {
            if (evt.target is VisualElement item)
            {
                var idx = item.parent.IndexOf(item);
                if (m_SelectionType == PickerSelectionType.Single)
                {
                    value = new[] {idx};
                }
                else
                {
                    var values = new List<int>(m_Value);
                    if (values.Contains(idx))
                        values.Remove(idx);
                    else
                        values.Add(idx);
                    value = values;
                }
            }
        }

        Menu BuildPickerMenu()
        {
            var menu = new Menu();

            menu.AddToClassList(appuiPickerMenu);
            menu.style.minWidth = paddingRect.width - 6;

            for (var i = 0; i < m_Items.Count; i++)
            {
                var isSelected = m_Value.Contains(i);
                m_Items[i].EnableInClassList(Styles.selectedUssClassName, isSelected);
                menu.Add(m_Items[i]);
            }

            return menu;
        }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Picker"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Emphasized = new UxmlBoolAttributeDescription
            {
                name = "emphasized",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };
            
            readonly UxmlBoolAttributeDescription m_CloseOnSelection = new UxmlBoolAttributeDescription
            {
                name = "close-on-selection",
                defaultValue = true,
            };
            
            readonly UxmlEnumAttributeDescription<PickerSelectionType> m_SelectionType = new UxmlEnumAttributeDescription<PickerSelectionType>
            {
                name = "selection-type",
                defaultValue = PickerSelectionType.Single,
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

                var el = (Picker)ve;
                el.size = m_Size.GetValueFromBag(bag, cc);
                el.emphasized = m_Emphasized.GetValueFromBag(bag, cc);
                el.closeOnSelection = m_CloseOnSelection.GetValueFromBag(bag, cc);
                el.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
                el.selectionType = m_SelectionType.GetValueFromBag(bag, cc);
            }
        }
    }

    public abstract class Picker<TItemType, TTitleType> : Picker
        where TItemType : VisualElement, new()
        where TTitleType : VisualElement, new()
    {
        Action<TItemType, int> m_BindItem;

        Func<TItemType> m_MakeItem;
        
        Func<TTitleType> m_MakeTitle;
        
        Action<TTitleType, IEnumerable<int>> m_BindTitle;
        
        public Func<TItemType> makeItem
        {
            get => m_MakeItem;
            set
            {
                m_MakeItem = value;
                RefreshListUI();
            }
        }
        
        public Action<TItemType, int> bindItem
        {
            get => m_BindItem;
            set
            {
                m_BindItem = value;
                RefreshListUI();
            }
        }
        
        public Action<TItemType, int> unbindItem
        {
            get;
            set;
        }
        
        public Func<TTitleType> makeTitle
        {
            get => m_MakeTitle;
            set
            {
                m_MakeTitle = value;
                RefreshTitleUI();
            }
        }
        
        public Action<TTitleType, IEnumerable<int>> bindTitle
        {
            get => m_BindTitle;
            set
            {
                m_BindTitle = value;
                RefreshTitleUI();
            }
        }

        public Picker(
            IList items, 
            Func<TItemType> makeItemFunc = null, 
            Func<TTitleType> makeTitleFunc = null, 
            Action<TItemType, int> bindItemFunc = null,
            Action<TTitleType, IEnumerable<int>> bindTitleFunc = null,
            Action<TItemType, int> unbindItemFunc = null,
            int[] defaultIndices = null)
            : base(items, defaultIndices)
        {
            unbindItem = unbindItemFunc;
            makeItem = makeItemFunc;
            makeTitle = makeTitleFunc;
            bindItem = bindItemFunc;
            bindTitle = bindTitleFunc;
        }

        protected override void RefreshTitleUI()
        {
            m_TitleContainer.Clear();
            var title = makeTitle?.Invoke() ?? new TTitleType();
            title.name = titleUssClassName;
            title.pickingMode = PickingMode.Ignore;
            title.AddToClassList(titleUssClassName);
            m_TitleContainer.hierarchy.Add(title);

            bindTitle?.Invoke(title, m_Value);
        }

        protected override VisualElement OnRequestItemCreation(int i)
        {
            var content = makeItem?.Invoke() ?? new TItemType();
            if (bindItem != null)
                bindItem.Invoke(content, i);
            else if (content is LocalizedTextElement text)
                text.text = sourceItems[i].ToString();
            
            return content;
        }

        protected override void OnUnbindItem(VisualElement item, int index)
        {
            if (item is TItemType typedItem)
                unbindItem?.Invoke(typedItem, index);
        }

        protected TItemType GetPickerItem(int index)
        {
            if (index < 0 || index >= sourceItems.Count)
                return null;
            
            return m_Items[index] as TItemType;
        }
        
        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Picker{T,TU}"/>.
        /// </summary>
        [Preserve]
        public new class UxmlTraits : Picker.UxmlTraits { }
    }
}
