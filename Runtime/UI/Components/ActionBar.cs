using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A contextual action bar that appears when items are selected in a collection view.
    /// </summary>
    /// <remarks>
    /// The ActionBar is a UI component that provides contextual actions for selected items in a collection view.
    /// It appears when one or more items are selected and offers a way to perform bulk operations on the selected
    /// items.
    ///
    /// The component consists of three main parts:
    ///
    /// 1. A checkbox for selecting/deselecting all items
    /// 2. A message indicating the number of selected items
    /// 3. An action group containing buttons for operations on selected items
    ///
    /// The ActionBar integrates seamlessly with Unity's CollectionView components and automatically updates its
    /// state based on selection changes.
    /// </remarks>
    /// <example>
    /// <para>Basic usage with ListView:</para>
    ///
    /// <para>Connecting an ActionBar with a ListView and adding action buttons.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ActionBar name="my-action-bar">
    ///     <ActionButton icon="edit" label="Edit" />
    ///     <ActionButton icon="delete" label="Delete" />
    /// </ActionBar>
    ///
    /// <ListView name="my-list-view" selection-type="Multiple" />
    ///
    /// <Script>
    /// var actionBar = root.Q<ActionBar>("my-action-bar");
    /// var listView = root.Q<ListView>("my-list-view");
    /// actionBar.collectionView = listView;
    /// </Script>
    /// ]]></code>
    /// <para>Customizing the selection message:</para>
    ///
    /// <para>Different ways to customize the selection message.</para>
    /// <code lang="csharp"><![CDATA[
    /// actionBar.message = "{itemCount} items selected - Choose an action below";
    ///
    /// // Using smart string for different counts
    /// actionBar.message = "{itemCount:plural:Select items|One item selected|{} items selected}";
    /// ]]></code>
    /// <para>Handling action button clicks:</para>
    ///
    /// <para>Adding an action button with click handling for selected items.</para>
    /// <code lang="csharp"><![CDATA[
    /// var deleteButton = new ActionButton("delete", () => {
    ///     var selectedItems = actionBar.selectedIndices.ToList();
    ///     // Handle deletion of selected items
    /// });
    /// actionBar.Add(deleteButton);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("actions")]
    public partial class ActionBar : BaseVisualElement
    {
        internal static readonly BindingId messageProperty = nameof(message);

        internal static readonly BindingId collectionViewProperty = nameof(collectionView);

#if UNITY_LOCALIZATION_PRESENT
        const string k_DefaultMessage = "@AppUI:selectedItemsMessage";
#else
        const string k_DefaultMessage = "{0} Selected item(s)";
#endif

        /// <summary>
        /// The ActionBar main styling class.
        /// </summary>
        public const string ussClassName = "appui-actionbar";

        /// <summary>
        /// The ActionBar action group styling class.
        /// </summary>
        public const string actionGroupUssClassName = ussClassName + "__actiongroup";

        /// <summary>
        /// The ActionBar checkbox styling class.
        /// </summary>
        public const string checkboxUssClassName = ussClassName + "__checkbox";

        /// <summary>
        /// The ActionBar label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        readonly ActionGroup m_ActionGroup;

        readonly Checkbox m_SelectAllCheckbox;

        BaseVerticalCollectionView m_CollectionView;

        readonly LocalizedTextElement m_Label;

        string m_Message;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionBar()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_SelectAllCheckbox = new Checkbox { name = checkboxUssClassName, emphasized = true };
            m_SelectAllCheckbox.AddToClassList(checkboxUssClassName);
            hierarchy.Add(m_SelectAllCheckbox);
            m_SelectAllCheckbox.RegisterValueChangedCallback(OnCheckboxValueChanged);

            m_Label = m_SelectAllCheckbox.Q<LocalizedTextElement>(Checkbox.labelUssClassName);
            m_Label.variables = new object[]
            {
                new Dictionary<string, object>
                {
                    {"itemCount", 0}
                }
            };

            m_ActionGroup = new ActionGroup { name = actionGroupUssClassName };
            m_ActionGroup.AddToClassList(actionGroupUssClassName);
            hierarchy.Add(m_ActionGroup);

            collectionView = null;
            message = k_DefaultMessage;
        }

        /// <summary>
        /// The collection view attached to this <see cref="ActionBar"/>.
        /// </summary>
        [Tooltip("The collection view attached to this ActionBar. " +
            "The collection view is used to get the selected indices and the items source.")]
        [CreateProperty]
        public BaseVerticalCollectionView collectionView
        {
            get => m_CollectionView;

            set
            {
                if (m_CollectionView != null)
                    m_CollectionView.selectedIndicesChanged -= OnSelectedIndicesChanged;
                m_CollectionView = value;
                if (m_CollectionView != null)
                    m_CollectionView.selectedIndicesChanged += OnSelectedIndicesChanged;

                RefreshUI();
            }
        }

        /// <summary>
        /// The list of selected indices from the Collection View.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public IEnumerable<int> selectedIndices => m_CollectionView?.selectedIndices ?? new List<int>();

        /// <summary>
        /// The items source from the Collection View.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public IList itemsSource => m_CollectionView?.itemsSource;

        /// <summary>
        /// Text used for item selection message.
        /// </summary>
        /// <remarks>
        /// We recommend to use a SmartString text in order to adjust the
        /// text based on the number of selected items.
        /// </remarks>
        /// <example>
        /// <code>
        /// {itemCount:plural:Nothing selected|One selected item|{} selected items}
        /// </code>
        /// </example>
        [Tooltip("Text used for item selection message.\n\n" +
            "We recommend to use a SmartString text in order to adjust the text based on the number of selected items.\n" +
            "Example: {itemCount:plural:Nothing selected|One selected item|{} selected items}")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Action Bar")]
        public string message
        {
            get => m_Message;
            set
            {
                var changed = m_Message != value;
                m_Message = value;
                RefreshUI();
                if (changed)
                    NotifyPropertyChanged(messageProperty);
            }
        }

        /// <summary>
        /// The content container of the <see cref="ActionBar"/>.
        /// </summary>
        public override VisualElement contentContainer => m_ActionGroup.contentContainer;

        void OnCheckboxValueChanged(ChangeEvent<CheckboxState> evt)
        {
            var val = evt.newValue;

            if (m_CollectionView == null)
                return;

            switch (val)
            {
                case CheckboxState.Unchecked:
                    m_CollectionView.ClearSelection();
                    break;
                case CheckboxState.Intermediate:
                    // do nothing
                    break;
                case CheckboxState.Checked:
                    var range = new int[itemsSource.Count];
                    for (var i = 0; i < itemsSource.Count; i++)
                    {
                        range[i] = i;
                    }
                    m_CollectionView.SetSelection(range);
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(val), val);
            }
        }

        void OnSelectedIndicesChanged(IEnumerable<int> _)
        {
            RefreshUI();
        }

        void RefreshUI()
        {
            var selectionCount = 0;
            foreach (var unused in selectedIndices)
            {
                selectionCount++;
            }
            var checkboxValue = CheckboxState.Unchecked;
            if (selectionCount > 0)
            {
                checkboxValue = selectionCount == itemsSource.Count
                    ? CheckboxState.Checked
                    : CheckboxState.Intermediate;
            }
            m_SelectAllCheckbox.SetValueWithoutNotify(checkboxValue);
            m_Label.variables = new object[]
            {
                new Dictionary<string, object>
                {
                    {"itemCount", selectionCount}
                }
            };
            m_SelectAllCheckbox.label = string.IsNullOrEmpty(m_Message) ? m_Message : string.Format(m_Message, selectionCount);
        }

    }
}
