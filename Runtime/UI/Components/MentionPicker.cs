using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// MentionPicker UI element. Displays a suggestion list for mentioning users or entities.
    /// </summary>
    public partial class MentionPicker : BaseVisualElement
    {
        /// <summary>
        /// The MentionPicker main styling class.
        /// </summary>
        public const string ussClassName = "appui-mention-picker";

        /// <summary>
        /// The MentionPicker list styling class.
        /// </summary>
        public const string listUssClassName = ussClassName + "__list";

        /// <summary>
        /// The MentionPicker "no results" element styling class.
        /// </summary>
        public const string noneElementUssClassName = ussClassName + "__none";

        readonly ListView m_List;

        IMentionProvider m_MentionProvider;

        string m_Query;

        /// <summary>
        /// Event fired when a mention suggestion is selected.
        /// </summary>
        public event Action<object> mentionSelected;

        /// <summary>
        /// The mention provider used to retrieve suggestions.
        /// </summary>
        public IMentionProvider mentionProvider
        {
            get => m_MentionProvider;
            set
            {
                m_MentionProvider = value;
                RefreshSuggestions();
            }
        }

        /// <summary>
        /// The query string used to filter mention suggestions.
        /// </summary>
        public string query
        {
            get => m_Query;
            set
            {
                var changed = m_Query != value;
                m_Query = value;
                RefreshSuggestions();
            }
        }

        /// <summary>
        /// The content container of the MentionPicker.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The count of suggestions for the current query.
        /// </summary>
        public int suggestionsCount => m_List.itemsSource?.Count ?? 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MentionPicker()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_List = new ListView
            {
                name = listUssClassName,
                makeItem = MakeItem,
#if UITK_MAKE_NONE_ELEMENT
                makeNoneElement = MakeNoneElement,
#endif
                bindItem = BindItem,
                selectionType =  SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            m_List.AddToClassList(listUssClassName);
            hierarchy.Add(m_List);

            query = null;
        }

        void BindItem(VisualElement item, int index)
        {
            var mention = m_List.itemsSource[index];
            m_MentionProvider.BindItem(item, mention);
            item.userData = mention;
        }

        VisualElement MakeNoneElement()
        {
            var label = new LocalizedTextElement($"No matches found for <b>@{m_Query}</b>");
            label.AddToClassList(noneElementUssClassName);
            return label;
        }

        VisualElement MakeItem()
        {
            var item = m_MentionProvider.MakeSuggestionItem();
            item.pickingMode = PickingMode.Position;
            item.AddManipulator(new Pressable(() =>
            {
                var mention = item.userData;
                mentionSelected?.Invoke(mention);
            }));
            return item;
        }

        /// <summary>
        /// Accepts the currently selected mention suggestion and fires the <see cref="mentionSelected"/> event.
        /// </summary>
        public void AcceptSelection()
        {
            if (m_List.itemsSource == null || m_List.selectedIndex < 0 || m_List.selectedIndex >= m_List.itemsSource.Count)
                return;
            mentionSelected?.Invoke(m_List.selectedItem);
        }

        /// <summary>
        /// Selects the next mention suggestion in the list.
        /// </summary>
        /// <returns>>True if the selection changed, false if it was already at the end of the list.</returns>
        public bool SelectNext()
        {
            if (m_List.itemsSource == null || m_List.itemsSource.Count == 0)
                return false;

            var nextIndex = Mathf.Clamp(m_List.selectedIndex + 1, 0, m_List.itemsSource.Count - 1);
            if (nextIndex == m_List.selectedIndex)
                return false;
            m_List.SetSelection(nextIndex);
            m_List.ScrollToItem(nextIndex);
            return true;
        }

        /// <summary>
        /// Selects the previous mention suggestion in the list.
        /// </summary>
        /// <returns>True if the selection changed, false if it was already at the beginning of the list.</returns>
        public bool SelectPrevious()
        {
            if (m_List.itemsSource == null || m_List.itemsSource.Count == 0)
                return false;

            var prevIndex = Mathf.Clamp(m_List.selectedIndex - 1, 0, m_List.itemsSource.Count - 1);
            if (prevIndex == m_List.selectedIndex)
                return false;
            m_List.SetSelection(prevIndex);
            m_List.ScrollToItem(prevIndex);
            return true;
        }

        void RefreshSuggestions()
        {
            if (m_MentionProvider == null || m_Query == null)
                return;

            m_List.itemsSource = m_MentionProvider.GetSuggestions(m_Query);
            m_List.RefreshItems();

            if (m_List.itemsSource.Count > 0)
                m_List.SetSelection(0);
            else
                m_List.ClearSelection();
        }
    }
}
