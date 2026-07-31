using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    record CollapsedItemContext : IContext { }

    /// <summary>
    /// A tree view item that can be used directly in a <see cref="ScrollView"/> or any vertical container.
    /// </summary>
    [UxmlElement]
    public partial class TreeViewItem : VisualElement
    {
        internal static readonly BindingId showCaretProperty = nameof(showCaret);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId expandedProperty = nameof(expanded);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId depthProperty = nameof(depth);

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string ussClassName = "appui-tree-view-item";

        /// <summary>
        /// The USS class name for the header of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string headerUssClassName = "appui-tree-view-item__header";

        /// <summary>
        /// The USS class name for the content viewport of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string contentViewportUssClassName = "appui-tree-view-item__content-viewport";

        /// <summary>
        /// The USS class name for the content container of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string contentContainerUssClassName = "appui-tree-view-item__content-container";

        /// <summary>
        /// The USS class name for the title label of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string titleLabelUssClassName = "appui-tree-view-item__title-label";

        /// <summary>
        /// The USS class name for the caret button of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string caretButtonUssClassName = "appui-tree-view-item__caret-button";

        /// <summary>
        /// The USS class name for the caret icon of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string caretIconUssClassName = "appui-tree-view-item__caret-icon";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it has a caret.
        /// </summary>
        public const string withCaretVariantUssClassName = "appui-tree-view-item--with-caret";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is expanded.
        /// </summary>
        public const string expandedVariantUssClassName = "appui-tree-view-item--expanded";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is interactable (has a clickable header).
        /// </summary>
        public const string interactableVariantUssClassName = "appui-tree-view-item--interactable";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is selected.
        /// </summary>
        public const string selectedVariantUssClassName = "appui-tree-view-item--selected";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> based on its depth.
        /// </summary>
        public const string depthVariantUssClassName = "appui-tree-view-item--depth-";

        readonly VisualElement m_ContentContainer;

        readonly VisualElement m_ContentViewport;

        readonly VisualElement m_HeaderContainer;

        readonly VisualElement m_CaretButton;

        readonly VisualElement m_CaretIcon;

        readonly LocalizedTextElement m_LabelElement;

        readonly VisualElement m_LabelContainerElement;

        readonly KeyboardFocusController m_KeyboardFocusController = new ();

        Pressable m_Clickable;

        Pressable m_CaretClickable;

        float m_ContentHeight;

        int m_Depth;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_ContentContainer;

        /// <summary>
        /// Gets or sets whether the caret is shown for this <see cref="TreeViewItem"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showCaret
        {
            get => ClassListContains(withCaretVariantUssClassName);
            set
            {
                var changed = showCaret != value;
                if (value)
                    AddToClassList(withCaretVariantUssClassName);
                else
                    RemoveFromClassList(withCaretVariantUssClassName);
                if (changed)
                    NotifyPropertyChanged(in showCaretProperty);
            }
        }

        /// <summary>
        /// Gets or sets the label of the <see cref="TreeViewItem"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                var changed = m_LabelElement.text != value;
                m_LabelElement.text = value;
                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="TreeViewItem"/> is expanded or collapsed.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool expanded
        {
            get => ClassListContains(expandedVariantUssClassName);
            set
            {
                var changed = expanded != value;
                if (value)
                    AddToClassList(expandedVariantUssClassName);
                else
                    RemoveFromClassList(expandedVariantUssClassName);
                UpdateViewportHeight();
                PropagateCollapse();
                if (changed)
                    NotifyPropertyChanged(in expandedProperty);
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="TreeViewItem"/> is in selected state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => ClassListContains(selectedVariantUssClassName);
            set
            {
                var changed = selected != value;
                if (value)
                    AddToClassList(selectedVariantUssClassName);
                else
                    RemoveFromClassList(selectedVariantUssClassName);
                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// Gets or sets the depth of the <see cref="TreeViewItem"/> in the tree view hierarchy.
        /// This is used to determine the indentation level of the item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int depth
        {
            get => m_Depth;
            set
            {
                var changed = m_Depth != value;
                RemoveFromClassList(MemoryUtils.Concatenate(depthVariantUssClassName, m_Depth.ToString()));
                m_Depth = value;
                AddToClassList(MemoryUtils.Concatenate(depthVariantUssClassName, m_Depth.ToString()));
                if (changed)
                    NotifyPropertyChanged(in depthProperty);
            }
        }

        /// <summary>
        /// Gets or sets the clickable behavior for the header of the <see cref="TreeViewItem"/>.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                m_HeaderContainer.RemoveManipulator(m_KeyboardFocusController);
                if (m_Clickable != null && m_Clickable.target == m_HeaderContainer)
                    m_HeaderContainer.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable != null)
                {
                    m_HeaderContainer.AddManipulator(m_Clickable);
                    m_HeaderContainer.AddManipulator(m_KeyboardFocusController);
                }
                EnableInClassList(interactableVariantUssClassName, m_Clickable != null);
            }
        }

        /// <summary>
        /// Gets or sets the clickable behavior for the caret button of the <see cref="TreeViewItem"/>.
        /// </summary>
        public Pressable caretClickable
        {
            get => m_CaretClickable;
            set
            {
                if (m_CaretClickable != null && m_CaretClickable.target == m_CaretButton)
                    m_CaretButton.RemoveManipulator(m_CaretClickable);
                m_CaretClickable = value;
                if (m_CaretClickable != null)
                    m_CaretButton.AddManipulator(m_CaretClickable);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewItem"/> class.
        /// </summary>
        public TreeViewItem()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_HeaderContainer = new VisualElement
            {
                pickingMode = PickingMode.Position,
                focusable = true,
                name = headerUssClassName
            };
            m_HeaderContainer.AddToClassList(headerUssClassName);
            m_ContentViewport = new VisualElement { pickingMode = PickingMode.Ignore, name = contentViewportUssClassName };
            m_ContentViewport.AddToClassList(contentViewportUssClassName);
            m_ContentContainer = new VisualElement {pickingMode = PickingMode.Ignore, name = contentContainerUssClassName};
            m_ContentContainer.AddToClassList(contentContainerUssClassName);
            m_LabelElement = new LocalizedTextElement { pickingMode = PickingMode.Ignore, name = titleLabelUssClassName };
            m_LabelElement.AddToClassList(titleLabelUssClassName);
            m_CaretButton = new VisualElement { pickingMode = PickingMode.Position, name = caretButtonUssClassName };
            m_CaretButton.AddToClassList(caretButtonUssClassName);
            m_CaretIcon = new VisualElement { pickingMode = PickingMode.Ignore, name = caretIconUssClassName };
            m_CaretIcon.AddToClassList(caretIconUssClassName);

            m_CaretButton.Add(m_CaretIcon);

            m_HeaderContainer.Add(m_LabelElement);
            m_HeaderContainer.Add(m_CaretButton);

            m_ContentViewport.Add(m_ContentContainer);

            hierarchy.Add(m_HeaderContainer);
            hierarchy.Add(m_ContentViewport);

            showCaret = false;
            expanded = false;
            selected = false;
            depth = 0;
            label = string.Empty;

            m_ContentContainer.RegisterCallback<GeometryChangedEvent>(OnContentContainerGeometryChanged);
            this.RegisterContextChangedCallback<CollapsedItemContext>(OnCollapsedItemContextChanged);
        }

        /// <summary>
        /// Toggles the expanded state of the <see cref="TreeViewItem"/>.
        /// </summary>
        public void ToggleExpand()
        {
            expanded = !expanded;
        }

        void OnContentContainerGeometryChanged(GeometryChangedEvent evt)
        {
            if (float.IsNaN(evt.newRect.height) || Mathf.Approximately(evt.newRect.height, m_ContentHeight))
                return;

            m_ContentHeight = evt.newRect.height;
            UpdateViewportHeight();
        }

        void UpdateViewportHeight()
        {
            m_ContentViewport.style.height = expanded ? m_ContentHeight : 0;
        }

        void PropagateCollapse()
        {
            if (!expanded)
            {
                this.ProvideContext(new CollapsedItemContext());
                schedule.Execute(() => this.ProvideContext<CollapsedItemContext>(null));
            }
        }

        void OnCollapsedItemContextChanged(ContextChangedEvent<CollapsedItemContext> evt)
        {
            if (evt.context == null)
                return;

            if (expanded)
                expanded = false;
        }

    }
}
