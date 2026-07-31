using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container component that organizes and manages a collection of action buttons.
    /// </summary>
    /// <remarks>
    /// An Action Group is a container component that organizes multiple action buttons into a cohesive unit. It
    /// provides various layout options and selection capabilities, making it ideal for toolbars, command bars, and
    /// button groups.
    ///
    /// The component automatically handles overflow by showing a 'more' button when there isn't enough space to
    /// display all actions, ensuring good responsive behavior.
    ///
    /// Key features:
    /// - Horizontal or vertical layout orientation
    /// - Single or multiple selection modes
    /// - Compact and justified layout options
    /// - Quiet visual variant
    /// - Automatic overflow handling with a 'more' menu
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("actions")]
    public partial class ActionGroup : BaseVisualElement
    {

        internal static readonly BindingId quietProperty = nameof(quiet);

        internal static readonly BindingId compactProperty = nameof(compact);

        internal static readonly BindingId directionProperty = nameof(direction);

        internal static readonly BindingId justifiedProperty = nameof(justified);

        internal static readonly BindingId selectionTypeProperty = nameof(selectionType);

        internal static readonly BindingId closeOnSelectionProperty = nameof(closeOnSelection);

        internal static readonly BindingId allowNoSelectionProperty = nameof(allowNoSelection);

        /// <summary>
        /// The ActionGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-actiongroup";

        /// <summary>
        /// The ActionGroup quiet mode styling class.
        /// </summary>
        public const string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The ActionGroup compact mode styling class.
        /// </summary>
        public const string compactUssClassName = ussClassName + "--compact";

        /// <summary>
        /// The ActionGroup vertical mode styling class.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string verticalUssClassName = ussClassName + "--";

        /// <summary>
        /// The ActionGroup justified mode styling class.
        /// </summary>
        public const string justifiedUssClassName = ussClassName + "--justified";

        /// <summary>
        /// The ActionGroup selectable mode styling class.
        /// </summary>
        public const string selectableUssClassName = ussClassName + "--selectable";

        /// <summary>
        /// The ActionGroup container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The ActionGroup More Button styling class.
        /// </summary>
        public const string moreButtonUssClassName = ussClassName + "__more-button";

        /// <summary>
        /// Event sent when the selection changes.
        /// </summary>
        public event Action<IEnumerable<int>> selectionChanged;

        SelectionType m_SelectionType = k_DefaultSelectionType;

        readonly List<VisualElement> m_HandledChildren = new List<VisualElement>();

        readonly List<int> m_SelectedIndices = new List<int>();

        readonly List<int> m_SelectedIds = new List<int>();

        readonly VisualElement m_Container;

        readonly ActionButton m_MoreButton;

        int m_FirstIndexOutOfBound = -1;

        MenuBuilder m_MenuBuilder;

        Rect m_LastContainerLayout;

        Rect m_LastLayout;

        int m_LastChildCount = 0;

        Dir m_CurrentLayoutDirection;

        Func<int, int> m_GetItemId;

        bool m_AllowNoSelection = true;

        bool m_CloseOnSelection;

        Direction m_Direction;

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

            m_MoreButton = new ActionButton
            {
                name = moreButtonUssClassName,
                icon = "dots-three",
                iconVariant = IconVariant.Bold,
            };
            m_MoreButton.EnableDynamicTransform(true);
            m_MoreButton.AddToClassList(ussClassName + "__item");
            m_MoreButton.AddToClassList("unity-last-child");
            m_MoreButton.AddToClassList(moreButtonUssClassName);
            m_MoreButton.clicked += OnMoreButtonClicked;
            hierarchy.Add(m_MoreButton);
            direction = Direction.Horizontal;
            closeOnSelection = true;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_MoreButton.RegisterCallback<GeometryChangedEvent>(OnMoreButtonGeometryChanged);
            RegisterCallback<ActionTriggeredEvent>(OnActionTriggered);
            this.RegisterContextChangedCallback<DirContext>(OnDirectionChanged);
        }

        void OnDirectionChanged(ContextChangedEvent<DirContext> evt)
        {
            m_CurrentLayoutDirection = evt.context?.dir ?? Dir.Ltr;
            schedule.Execute(RefreshUI);
        }

        /// <summary>
        /// The content container of the ActionGroup.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The quiet state of the ActionGroup.
        /// </summary>
        [Tooltip("The quiet state of the ActionGroup. A quiet ActionGroup has no background and no border.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Action Group")]
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set
            {
                var changed = quiet != value;
                EnableInClassList(quietUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in quietProperty);
            }
        }

        /// <summary>
        /// The compact state of the ActionGroup.
        /// </summary>
        [Tooltip("The compact state of the ActionGroup. A compact ActionGroup doesn't have any gap between its items.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool compact
        {
            get => ClassListContains(compactUssClassName);
            set
            {
                var changed = compact != value;
                EnableInClassList(compactUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in compactProperty);
            }
        }

        /// <summary>
        /// The orientation of the ActionGroup.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetDirectionUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetDirectionUssClassName(m_Direction));
                m_MoreButton.icon = m_Direction switch
                {
                    Direction.Horizontal => "dots-three",
                    Direction.Vertical => "dots-three-vertical",
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The justified state of the ActionGroup.
        /// </summary>
        [Tooltip("The justified state of the ActionGroup. A justified ActionGroup has its items stretched to fill the available space.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool justified
        {
            get => ClassListContains(justifiedUssClassName);
            set
            {
                var changed = justified != value;
                EnableInClassList(justifiedUssClassName, value);
                if (changed)
                    NotifyPropertyChanged(in justifiedProperty);
            }
        }

        /// <summary>
        /// The selection type of the ActionGroup.
        /// </summary>
        [Tooltip("The selection type of the ActionGroup. " +
            "A selection type of None means that no item can be selected. " +
            "A selection type of Single means that only one item can be selected at a time. " +
            "A selection type of Multiple means that multiple items can be selected at a time.")]
        [CreateProperty]
        [UxmlAttribute]
        public SelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                var changed = m_SelectionType != value;
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
                if (changed)
                    NotifyPropertyChanged(in selectionTypeProperty);
            }
        }

        /// <summary>
        /// Whether the ActionGroup's menu popover should close when a selection is made.
        /// </summary>
        [Tooltip("Whether the ActionGroup's menu popover should close when a selection is made.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool closeOnSelection
        {
            get => m_CloseOnSelection;
            set
            {
                var changed = m_CloseOnSelection != value;
                m_CloseOnSelection = value;
                if (changed)
                    NotifyPropertyChanged(in closeOnSelectionProperty);
            }
        }

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
        [Tooltip("Whether the ActionGroup allows no selection when in single or multi selection mode.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool allowNoSelection
        {
            get => m_AllowNoSelection;
            set
            {
                var changed = m_AllowNoSelection != value;
                m_AllowNoSelection = value;
                if (!m_AllowNoSelection && m_SelectedIndices.Count == 0)
                    SetSelection(new[] { 0 });
                if (changed)
                    NotifyPropertyChanged(in allowNoSelectionProperty);
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

        void OnMoreButtonGeometryChanged(GeometryChangedEvent evt)
        {
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
        }

        void RefreshUI()
        {
            var actionGroupLayout = layout;
            var containerLayout = m_Container.layout;
            var groupChildCount = m_HandledChildren.Count;

            if (!actionGroupLayout.IsValid() || (
                    containerLayout == m_LastContainerLayout
                    && actionGroupLayout == m_LastLayout
                    && groupChildCount == m_LastChildCount))
                return;

            m_LastChildCount = groupChildCount;
            m_LastContainerLayout = containerLayout;
            m_LastLayout = actionGroupLayout;
            var moreButtonStyle = m_MoreButton.resolvedStyle;
            var moreButtonLayout = m_MoreButton.layout;

            float size;
            float containerSize;
            float moreButtonSize;
            Func<VisualElement, float> getChildSize;

            switch (m_Direction)
            {
                case Direction.Horizontal:
                    size = actionGroupLayout.width;
                    containerSize = containerLayout.width;
                    moreButtonSize = moreButtonStyle.width + (m_CurrentLayoutDirection == Dir.Ltr
                        ? moreButtonStyle.marginLeft
                        : moreButtonStyle.marginRight);
                    getChildSize = GetElementFullWidth;
                    break;
                case Direction.Vertical:
                    size = actionGroupLayout.height;
                    containerSize = containerLayout.height;
                    moreButtonSize = moreButtonStyle.height + moreButtonStyle.marginTop;
                    getChildSize = GetElementFullHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var outOfBounds = size < containerSize;
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
                    var childSize = getChildSize.Invoke(child);
                    var newSpaceUsed = spaceUsed + childSize;
                    if (spaceUsed <= size && newSpaceUsed > size)
                    {
                        // first item that doesn't fit
                        m_FirstIndexOutOfBound = i;
                    }
                    spaceUsed += childSize;
                    child.visible = spaceUsed <= size;
                }
                else
                {
                    child.visible = true;
                }
            }

            m_MoreButton.visible = m_FirstIndexOutOfBound >= 0;
            if (m_FirstIndexOutOfBound >= 0) // overflow detected
            {
                var firstChildOutOfBoundLayout = m_HandledChildren[m_FirstIndexOutOfBound].layout;
                var left = m_Direction switch
                {
                    Direction.Horizontal when m_CurrentLayoutDirection == Dir.Ltr => firstChildOutOfBoundLayout.x,
                    Direction.Horizontal => firstChildOutOfBoundLayout.xMax + (size - containerSize) - moreButtonSize,
                    Direction.Vertical => containerLayout.x,
                    _ => 0
                };
                var top = m_Direction switch
                {
                    Direction.Horizontal => containerLayout.y,
                    Direction.Vertical => firstChildOutOfBoundLayout.y,
                    _ => 0
                };
                if (!Mathf.Approximately(moreButtonLayout.x, left))
                    m_MoreButton.style.left = left;
                if (!Mathf.Approximately(moreButtonLayout.y, top))
                    m_MoreButton.style.top = top;
                m_MoreButton.EnableInClassList("unity-first-child", m_FirstIndexOutOfBound == 0);
            }

            RefreshSelectionUI();
        }

        static float GetElementFullWidth(VisualElement ve)
        {
            var style = ve.resolvedStyle;
            return style.width + style.marginLeft + style.marginRight;
        }

        static float GetElementFullHeight(VisualElement ve)
        {
            var style = ve.resolvedStyle;
            return style.height + style.marginTop + style.marginBottom;
        }

        void OnMoreButtonClicked()
        {
            if (m_FirstIndexOutOfBound < 0)
                return;

            var dir = this.GetContext<DirContext>()?.dir ?? Dir.Ltr;
            var horizontalPlacement = dir == Dir.Ltr ? PopoverPlacement.BottomStart : PopoverPlacement.BottomEnd;
            var placement = m_Direction switch
            {
                Direction.Horizontal => horizontalPlacement,
                Direction.Vertical => PopoverPlacement.EndBottom,
                _ => throw new ArgumentOutOfRangeException()
            };
            m_MenuBuilder?.Dismiss(DismissType.Consecutive);
            m_MenuBuilder = MenuBuilder.Build(m_MoreButton)
                .SetCloseOnSelection(closeOnSelection)
                .SetPlacement(placement);

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

        void OnMenuActionPressed(EventBase evt)
        {
            if (
                evt.target is MenuItem {userData: int actionId and >= 0} &&
                actionId < m_HandledChildren.Count &&
                m_HandledChildren[actionId] is ActionButton btn)
            {
                btn.clickable?.InvokePressed(evt);
            }
        }

    }
}
