using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    
    
    /// <summary>
    /// ActionGroup UI element.
    /// </summary>
    public class ActionGroup : VisualElement
    {
        const string k_MultiSelectWithOverflowMsg =
            "Having a single selection in an ActionGroup with overflow can lead to unexpected behavior.\n" +
            "Consider using SelectionType.None or SelectionType.Multiple instead, or try to avoid overflow.";
        
        /// <summary>
        /// The ActionGroup main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-actiongroup";

        /// <summary>
        /// The ActionGroup quiet mode styling class.
        /// </summary>
        public static readonly string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The ActionGroup compact mode styling class.
        /// </summary>
        public static readonly string compactUssClassName = ussClassName + "--compact";

        /// <summary>
        /// The ActionGroup vertical mode styling class.
        /// </summary>
        public static readonly string verticalUssClassName = ussClassName + "--vertical";

        /// <summary>
        /// The ActionGroup justified mode styling class.
        /// </summary>
        public static readonly string justifiedUssClassName = ussClassName + "--justified";
        
        /// <summary>
        /// The ActionGroup selectable mode styling class.
        /// </summary>
        public static readonly string selectableUssClassName = ussClassName + "--selectable";
        
        /// <summary>
        /// The ActionGroup container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";
        
        /// <summary>
        /// The ActionGroup More Button styling class.
        /// </summary>
        public static readonly string moreButtonUssClassName = ussClassName + "__more-button";
        
        /// <summary>
        /// Event sent when the selection changes.
        /// </summary>
        public event Action<IEnumerable<int>> selectionChanged;

        SelectionType m_SelectionType = k_DefaultSelectionType;

        readonly List<VisualElement> m_HandledChildren = new List<VisualElement>();

        readonly List<VisualElement> m_SelectedElements = new List<VisualElement>();

        readonly List<int> m_SelectedIndices = new List<int>();

        readonly List<int> m_SelectedIds = new List<int>();

        readonly VisualElement m_Container;

        readonly ActionButton m_MoreButton;

        int m_FirstIndexOutOfBound = -1;

        MenuBuilder m_MenuBuilder;

        Rect m_LastContainerLayout;

        Rect m_LastLayout;

        Dir m_CurrentDirection;
        
        Func<int, int> m_GetItemId;

        bool m_AllowNoSelection = true;

        const SelectionType k_DefaultSelectionType = SelectionType.None;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionGroup()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;
            
            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);
            
            m_MoreButton = new ActionButton { name = moreButtonUssClassName, icon = "dots-three", iconVariant = IconVariant.Bold };
            m_MoreButton.AddToClassList(ussClassName + "__item");
            m_MoreButton.AddToClassList("unity-last-child");
            m_MoreButton.AddToClassList(moreButtonUssClassName);
            m_MoreButton.clicked += OnMoreButtonClicked;
            hierarchy.Add(m_MoreButton);
            vertical = false;
            closeOnSelection = true;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<ActionTriggeredEvent>(OnActionTriggered);
            this.RegisterContextChangedCallback<DirContext>(OnDirectionChanged);
        }

        void OnDirectionChanged(ContextChangedEvent<DirContext> evt)
        {
            m_CurrentDirection = evt.context?.dir ?? Dir.Ltr;
            schedule.Execute(RefreshUI);
        }

        /// <summary>
        /// The content container of the ActionGroup.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The quiet state of the ActionGroup.
        /// </summary>
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set => EnableInClassList(quietUssClassName, value);
        }

        /// <summary>
        /// The compact state of the ActionGroup.
        /// </summary>
        public bool compact
        {
            get => ClassListContains(compactUssClassName);
            set => EnableInClassList(compactUssClassName, value);
        }

        /// <summary>
        /// The vertical state of the ActionGroup.
        /// </summary>
        public bool vertical
        {
            get => ClassListContains(verticalUssClassName);
            set
            {
                EnableInClassList(verticalUssClassName, value);
                m_MoreButton.icon = value ? "dots-three-vertical" : "dots-three";
            }
        }

        /// <summary>
        /// The justified state of the ActionGroup.
        /// </summary>
        public bool justified
        {
            get => ClassListContains(justifiedUssClassName);
            set => EnableInClassList(justifiedUssClassName, value);
        }

        /// <summary>
        /// The selection type of the ActionGroup.
        /// </summary>
        public SelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                m_SelectionType = value;
                EnableInClassList(selectableUssClassName, m_SelectionType != SelectionType.None);
                if (m_SelectionType == SelectionType.None)
                {
                    ClearSelection();
                }
                else if (m_SelectionType == SelectionType.Single && m_SelectedIndices.Count != 1)
                {
                    if (allowNoSelection)
                        ClearSelection();
                    else
                        SetSelection(new[] { m_SelectedIndices.Last() });
                }
            }
        }
        
        /// <summary>
        /// Whether the ActionGroup's menu popover should close when a selection is made.
        /// </summary>
        public bool closeOnSelection { get; set; }

        /// <summary>
        /// Callback used to get the ID of an item.
        /// </summary>
        public Func<int, int> getItemId
        {
            get => m_GetItemId;
            set
            {
                m_GetItemId = value;
                RefreshUI();
            }
        }
        
        /// <summary>
        /// The selected items.
        /// </summary>
        public IEnumerable<int> selectedIndices => m_SelectedIndices;
        
        /// <summary>
        /// The selected items.
        /// </summary>
        public IEnumerable<int> selectedIds => m_SelectedIds;

        /// <summary>
        /// Whether the ActionGroup allows no selection when in single or multi selection mode.
        /// </summary>
        public bool allowNoSelection
        {
            get => m_AllowNoSelection;
            set
            {
                m_AllowNoSelection = value;
                if (!m_AllowNoSelection && m_SelectedIndices.Count == 0)
                    SetSelection(new[] { 0 });
            }
        }
        
        /// <summary>
        /// Deselects any selected items.
        /// </summary>
        public void ClearSelection()
        {
            var notify = m_SelectedIds.Count > 0;
            ClearSelectionWithoutNotify();
            if (notify)
                NotifyOfSelectionChange();
        }

        /// <summary>
        /// Deselects any selected items without sending an event through the visual tree.
        /// </summary>
        public void ClearSelectionWithoutNotify()
        {
            m_SelectedIndices.Clear();
            m_SelectedIds.Clear();
            RefreshSelectionUI(closeOnSelection);
        }

        /// <summary>
        /// Sets a collection of selected items.
        /// </summary>
        /// <param name="indices">The collection of the indices of the items to be selected.</param>
        public void SetSelection(IEnumerable<int> indices)
        {
            switch (selectionType)
            {
                case SelectionType.None:
                    return;
                case SelectionType.Single:
                    if (indices != null)
                        indices = new[] { indices.Last() };
                    break;
                case SelectionType.Multiple:
                    break;
                default:
                    throw new InvalidOperationException("Invalid selection type");
            }

            SetSelectionInternal(indices, true);
        }

        /// <summary>
        /// Sets a collection of selected items without triggering a selection change callback.
        /// </summary>
        /// <param name="indices">The collection of items to be selected.</param>
        public void SetSelectionWithoutNotify(IEnumerable<int> indices)
        {
            switch (selectionType)
            {
                case SelectionType.None:
                    return;
                case SelectionType.Single:
                    if (indices != null)
                        indices = new[] { indices.Last() };
                    break;
                case SelectionType.Multiple:
                    break;
                default:
                    throw new InvalidOperationException("Invalid selection type");
            }

            SetSelectionInternal(indices, false);
        }
        
        void SetSelectionInternal(IEnumerable<int> indices, bool sendEvent)
        {
            indices ??= new int[] { };

            var newIndices = indices.ToList();
            newIndices.Sort();
            var newIds = newIndices.Select(GetIdFromIndex).ToList();
            newIds.Sort();
            var hasChanged = !EnumerableExtensions.SequenceEqual(newIds, m_SelectedIds);

            m_SelectedIndices.Clear();
            m_SelectedIds.Clear();
            m_SelectedIds.AddRange(newIds);
            m_SelectedIndices.AddRange(newIndices);
            RefreshSelectionUI(closeOnSelection);
            if (sendEvent && hasChanged)
                NotifyOfSelectionChange();
        }
        
        void NotifyOfSelectionChange()
        {
            selectionChanged?.Invoke(m_SelectedIndices);
        }

        void OnActionTriggered(ActionTriggeredEvent evt)
        {
            evt.StopPropagation();

            if (selectionType != SelectionType.Single && selectionType != SelectionType.Multiple)
                return;

            if (evt.target is VisualElement el && el != m_MoreButton)
            {
                var currentSelection = new List<int>(m_SelectedIndices);
                var index = el.parent.IndexOf(el);
                switch (selectionType)
                {
                    case SelectionType.Single:
                    {
                        if (!currentSelection.Contains(index))
                            SetSelection(new[] { index });
                        else if (allowNoSelection)
                            SetSelection(null);
                        break;
                    }
                    case SelectionType.Multiple:
                    {
                        if (!currentSelection.Contains(index))
                        {
                            currentSelection.Add(index);
                            SetSelection(currentSelection);
                        }
                        else
                        {
                            currentSelection.Remove(index);
                            if (currentSelection.Count > 0 || allowNoSelection)
                                SetSelection(currentSelection);
                        }
                        break;
                    }
                    case SelectionType.None:
                    default:
                        break;
                }
            }
        }

        int GetIdFromIndex(int index)
        {
            return m_GetItemId?.Invoke(index) ?? index;
        }
        
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            m_HandledChildren.Clear();
            m_HandledChildren.AddRange(Children());
            
            RefreshUI();
        }

        void RefreshSelectionUI(bool dismissPopover = true)
        {
            if (dismissPopover)
            {
                m_MenuBuilder?.Dismiss(DismissType.Action);
                m_MenuBuilder = null;
            }
            
            for (var i = 0; i < m_HandledChildren.Count; i++)
            {
                var child = m_HandledChildren[i];
                if (child is ISelectableElement selectableElement)
                    selectableElement.SetSelectedWithoutNotify(m_SelectedIndices.Contains(i));
            }
            
            if (selectionType == SelectionType.Single && m_FirstIndexOutOfBound >= 0)
                Debug.LogWarning(k_MultiSelectWithOverflowMsg); 
        }
        
        void RefreshUI()
        {
            if (!layout.IsValid() || (m_Container.layout == m_LastContainerLayout && layout == m_LastLayout))
                return;
            
            m_LastContainerLayout = m_Container.layout;
            m_LastLayout = layout;
            
            var size = vertical ? layout.height : layout.width;
            var outOfBounds = vertical ? size < m_Container.layout.height : size < m_Container.layout.width;

            var moreButtonSize = vertical ? 
                m_MoreButton.resolvedStyle.height + m_MoreButton.resolvedStyle.marginTop + m_MoreButton.resolvedStyle.marginBottom : 
                m_MoreButton.resolvedStyle.width + m_MoreButton.resolvedStyle.marginLeft + m_MoreButton.resolvedStyle.marginRight;
            
            var spaceUsed = outOfBounds ? moreButtonSize : 0;
            m_FirstIndexOutOfBound = -1;
            for (var i = 0; i < m_HandledChildren.Count; i++)
            {
                var child = m_HandledChildren[i];
                if (child == m_MoreButton)
                    continue;
                
                child.EnableInClassList("unity-first-child", i == 0);
                child.EnableInClassList(ussClassName + "__inbetween-item", i != 0 && i != m_HandledChildren.Count - 1);
                child.EnableInClassList("unity-last-child", i == m_HandledChildren.Count - 1);
                child.AddToClassList(ussClassName + "__item");

                if (outOfBounds)
                {
                    var childSize = vertical ? 
                        child.resolvedStyle.height + child.resolvedStyle.marginTop + child.resolvedStyle.marginBottom : 
                        child.resolvedStyle.width + child.resolvedStyle.marginLeft + child.resolvedStyle.marginRight;
                    var newSpaceUsed = spaceUsed + childSize;
                    if (spaceUsed <= size && newSpaceUsed > size)
                    {
                        // first item that doesn't fit
                        m_FirstIndexOutOfBound = i;
                    }
                    spaceUsed += childSize;
                    child.visible = spaceUsed <= (vertical ? layout.height : layout.width);
                }
                else
                {
                    child.visible = true;
                }
            }

            m_MoreButton.visible = m_FirstIndexOutOfBound >= 0;
            if (m_FirstIndexOutOfBound >= 0) // overflow detected
            {
                if (vertical)
                {
                    m_MoreButton.style.top = m_HandledChildren[m_FirstIndexOutOfBound].layout.y;
                }
                else
                {
                    m_MoreButton.style.left = m_CurrentDirection == Dir.Ltr ? 
                        m_HandledChildren[m_FirstIndexOutOfBound].layout.x : 
                        m_HandledChildren[m_FirstIndexOutOfBound].layout.xMax + (size - m_Container.layout.width) - moreButtonSize;
                }
                m_MoreButton.EnableInClassList("unity-first-child", m_FirstIndexOutOfBound == 0);
            }

            RefreshSelectionUI();
        }

        void OnMoreButtonClicked()
        {
            if (m_FirstIndexOutOfBound < 0)
                return;

            var ctx = this.GetContext();
            var horizontalPlacement = ctx.dir == Dir.Ltr ? PopoverPlacement.BottomStart : PopoverPlacement.BottomEnd;
            m_MenuBuilder?.Dismiss(DismissType.Consecutive);
            m_MenuBuilder = MenuBuilder.Build(m_MoreButton)
                .SetCloseOnSelection(closeOnSelection)
                .SetPlacement(vertical ? PopoverPlacement.EndBottom : horizontalPlacement);

            var selectable = selectionType != SelectionType.None;
            for (var i = m_FirstIndexOutOfBound; i < m_HandledChildren.Count; i++)
            {
                if (m_HandledChildren[i] is ActionButton button)
                {
                    m_MenuBuilder.AddAction(i, button.label, button.icon, null, OnMenuActionPressed);
                    var item = (MenuItem) m_MenuBuilder.currentMenu.ElementAt(m_MenuBuilder.currentMenu.childCount - 1);
                    item.selectable = selectable;
                    if (selectable)
                        item.SetValueWithoutNotify(m_SelectedIndices.Contains(i));
                }
            }
            
            m_MenuBuilder.Show();
        }

        void OnMenuActionPressed(ClickEvent evt)
        {
            if (
                evt.target is MenuItem {userData: int actionId and >= 0} && 
                actionId < m_HandledChildren.Count && 
                m_HandledChildren[actionId] is ActionButton btn)
            {
                btn.clickable?.InvokePressed(evt);
            }
        }
        
        /// <summary>
        /// Whether the element is disabled.
        /// </summary>
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }

        /// <summary>
        /// The UXML factory for the ActionGroup.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ActionGroup, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ActionGroup"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Compact = new UxmlBoolAttributeDescription
            {
                name = "compact",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Justified = new UxmlBoolAttributeDescription
            {
                name = "justified",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Quiet = new UxmlBoolAttributeDescription
            {
                name = "quiet",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_Vertical = new UxmlBoolAttributeDescription
            {
                name = "vertical",
                defaultValue = false
            };
            
            readonly UxmlEnumAttributeDescription<SelectionType> m_SelectionType = new UxmlEnumAttributeDescription<SelectionType>
            {
                name = "selection-type",
                defaultValue = k_DefaultSelectionType
            };
            
            readonly UxmlBoolAttributeDescription m_CloseOnSelection = new UxmlBoolAttributeDescription
            {
                name = "close-on-selection",
                defaultValue = true
            };
            
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };
            
            readonly UxmlBoolAttributeDescription m_AllowNoSelection = new UxmlBoolAttributeDescription
            {
                name = "allow-no-selection",
                defaultValue = true
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
                var el = (ActionGroup)ve;
                el.quiet = m_Quiet.GetValueFromBag(bag, cc);
                el.compact = m_Compact.GetValueFromBag(bag, cc);
                el.vertical = m_Vertical.GetValueFromBag(bag, cc);
                el.justified = m_Justified.GetValueFromBag(bag, cc);
                el.selectionType = m_SelectionType.GetValueFromBag(bag, cc);
                el.closeOnSelection = m_CloseOnSelection.GetValueFromBag(bag, cc);
                el.allowNoSelection = m_AllowNoSelection.GetValueFromBag(bag, cc);
                el.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
