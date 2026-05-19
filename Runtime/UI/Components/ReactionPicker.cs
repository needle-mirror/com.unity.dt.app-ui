using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ReactionPicker UI element. Displays an emoji grid for selecting reactions.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ReactionPicker : BaseVisualElement
    {
        /// <summary>
        /// The ReactionPicker main styling class.
        /// </summary>
        public const string ussClassName = "appui-reaction-picker";

        /// <summary>
        /// The ReactionPicker tabs styling class.
        /// </summary>
        public const string tabsUssClassName = "appui-reaction__tabs";

        /// <summary>
        /// The ReactionPicker search field styling class.
        /// </summary>
        public const string searchFieldUssClassName = "appui-reaction__search-field";

        /// <summary>
        /// The ReactionPicker grid styling class.
        /// </summary>
        public const string gridUssClassName = ussClassName + "__grid";

        readonly GridView m_Grid;
        readonly Tabs m_Tabs;
        readonly TextField m_SearchField;

        EmojiDatabase m_EmojiProvider;

        string m_Query;

        /// <summary>
        /// Event fired when an emoji is selected.
        /// </summary>
        public event Action<string> emojiSelected;

        /// <summary>
        /// The emoji provider used to populate the grid.
        /// </summary>
        public EmojiDatabase emojiProvider
        {
            get => m_EmojiProvider;
            set
            {
                m_EmojiProvider = value;
                Refresh();
            }
        }

        /// <summary>
        /// The content container of the ReactionPicker.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReactionPicker()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Tabs = new Tabs
            {
                name = tabsUssClassName
            };
            m_Tabs.AddToClassList(tabsUssClassName);
            hierarchy.Add(m_Tabs);

            m_SearchField = new TextField
            {
                name = searchFieldUssClassName,
                placeholder = "Search all emoji",
            };
            m_SearchField.AddToClassList(searchFieldUssClassName);
            m_SearchField.RegisterValueChangingCallback(OnSearch);
            hierarchy.Add(m_SearchField);

            m_Grid = new GridView
            {
                name = gridUssClassName,
                makeItem = MakeGridItem,
                destroyItem = DestroyGridItem,
                bindItem = BindItem,
                unbindItem = UnbindItem,
                selectionType = SelectionType.None,
                itemHeight = 32
            };
            m_Grid.AddToClassList(gridUssClassName);
            m_Grid.style.flexDirection = FlexDirection.Row;
            m_Grid.style.flexWrap = Wrap.Wrap;
            hierarchy.Add(m_Grid);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnSearch(ChangingEvent<string> evt)
        {
            m_Query = evt.newValue;
            Refresh();
        }

        void UnbindItem(VisualElement item, int index)
        {
            item.userData = null;
        }

        void BindItem(VisualElement item, int index)
        {
            var emojiData = (EmojiData)m_Grid.itemsSource[index];
            item.userData = emojiData.emoji;
            item.tooltip = emojiData.name;
            if (item is ActionButton button)
            {
                button.label = emojiData.emoji;
            }
        }

        static ObjectPool<ActionButton> s_ActionButtonPool;

        void DestroyItem(VisualElement item)
        {
            if (item is ActionButton button)
                button.clickable.clickedWithEventInfo -= OnEmojiButtonClicked;
        }

        ActionButton MakeItem()
        {
            var button = new ActionButton { quiet = true, size = Size.M };
            button.clickable.clickedWithEventInfo += OnEmojiButtonClicked;
            return button;
        }

        void OnEmojiButtonClicked(EventBase evt)
        {
            if (((VisualElement)evt.target).userData is string emoji)
                emojiSelected?.Invoke(emoji);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid() || !m_Grid.scrollView.contentViewport.layout.IsValid())
                return;

            // adapt the column count
            var columnCount = Mathf.FloorToInt(m_Grid.scrollView.contentViewport.layout.width / m_Grid.itemHeight); // m_Grid.itemHeight is the size of the buttons
            m_Grid.columnCount = Mathf.Max(1, columnCount);
        }

        /// <summary>
        /// Clears and rebuilds the emoji grid from the current <see cref="emojiProvider"/>.
        /// </summary>
        public void Refresh()
        {
            m_Grid.itemsSource = null;
            s_ActionButtonPool?.Clear();

            if (!m_EmojiProvider)
                return;

            var list = new List<EmojiData>(m_EmojiProvider.GetEmojis(m_Query));
            if (list.Count == 0)
                return;

            s_ActionButtonPool = new ObjectPool<ActionButton>(
                createFunc: MakeItem,
                actionOnDestroy: DestroyItem,
                collectionCheck: false,
                maxSize: list.Count,
                defaultCapacity: 100);
            m_Grid.itemsSource = list;
        }

        static VisualElement MakeGridItem()
        {
            return s_ActionButtonPool?.Get();
        }

        static void DestroyGridItem(VisualElement obj)
        {
            if (obj is ActionButton actionButton)
                s_ActionButtonPool?.Release(actionButton);
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the ReactionPicker.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ReactionPicker, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ReactionPicker"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }
#endif
    }
}
