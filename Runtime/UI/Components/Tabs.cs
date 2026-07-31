using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Direction of a UI container.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum Direction
    {
        /// <summary>
        /// Container's items are stacked horizontally.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Container's items are stacked vertically.
        /// </summary>
        Vertical
    }

    /// <summary>
    /// A navigation component that allows users to switch between different views or sections.
    /// </summary>
    /// <remarks>
    /// Tabs make it easy to explore and switch between different views or functional aspects of an app. They
    /// enable content organization at a high level, such as switching between views, data sets, or functional
    /// aspects of an app.
    ///
    /// Tabs organize content across different screens, data sets, and other interactions. They consist of a list
    /// of tab items that can contain text labels and/or icons. When a tab is selected, it displays an indicator to
    /// show which tab is active.
    ///
    /// Note: Tabs should be used at the top level of navigation, not for subordinate or lower-level views.
    ///
    /// The component supports both horizontal and vertical orientations, different sizes, and can be configured
    /// with various styling options like emphasized or justified layouts.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Tabs : BaseVisualElement, INotifyValueChanged<int>
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId directionProperty = nameof(direction);

        internal static readonly BindingId emphasizedProperty = nameof(emphasized);

        internal static readonly BindingId justifiedProperty = nameof(justified);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId itemsProperty = nameof(items);

        internal static readonly BindingId sourceItemsProperty = nameof(sourceItems);

        internal static readonly BindingId bindItemProperty = nameof(bindItem);

        internal static readonly BindingId unbindItemProperty = nameof(unbindItem);


        /// <summary>
        /// The Tabs main styling class.
        /// </summary>
        public const string ussClassName = "appui-tabs";

        /// <summary>
        /// The Tabs size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Tabs direction styling class.
        /// </summary>
        [EnumName("GetOrientationUssClassName", typeof(Direction))]
        public const string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The Tabs emphasized mode styling class.
        /// </summary>
        public const string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Tabs justified mode styling class.
        /// </summary>
        public const string justifiedUssClassName = ussClassName + "--justified";

        /// <summary>
        /// The Tabs container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Tabs ScrollView styling class.
        /// </summary>
        public const string scrollViewUssClassName = ussClassName + "__scroll-view";

        /// <summary>
        /// The Tabs indicator styling class.
        /// </summary>
        public const string indicatorUssClassName = ussClassName + "__indicator";

        /// <summary>
        /// The Tabs animated indicator styling class.
        /// </summary>
        public const string animatedIndicatorUssClassName = indicatorUssClassName + "--animated";

        readonly VisualElement m_Indicator;

        readonly List<TabItem> m_Items = new List<TabItem>();

        readonly ScrollView m_ScrollView;

        readonly VisualElement m_LambdaContainer;

        readonly VisualElement m_Container;

        Action<TabItem, int> m_BindItem;

        Action<TabItem, int> m_UnbindItem;

        int m_DefaultValue;

        Direction m_Direction;

        Size m_Size;

        IList m_SourceItems;

        int m_Value;

        IVisualElementScheduledItem m_ScheduledRefreshIndicator;

        IVisualElementScheduledItem m_PollHierarchyItem;

        List<TabItem> m_StaticItems;

        readonly EventCallback<ITransitionEvent> m_TransitionEndAction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Tabs()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_ScrollView = new ScrollView
            {
                name = scrollViewUssClassName,
                nestedInteractionKind = ScrollView.NestedInteractionKind.StopScrolling,
                mode = ScrollViewMode.Horizontal,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_ScrollView.AddToClassList(scrollViewUssClassName);

            m_Container = new VisualElement
            {
                name = containerUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_Container.AddToClassList(containerUssClassName);
            m_ScrollView.Add(m_Container);

            m_Indicator = new VisualElement
            {
                name = indicatorUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_Indicator.AddToClassList(indicatorUssClassName);

            m_LambdaContainer = new VisualElement
            {
                name = "lambda-container",
                pickingMode = PickingMode.Ignore,
            };
            hierarchy.Add(m_LambdaContainer);

            hierarchy.Add(m_ScrollView);
            hierarchy.Add(m_Indicator);

            size = Size.M;
            emphasized = false;
            justified = false;
            direction = Direction.Horizontal;
            value = -1;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            this.RegisterContextChangedCallback<DirContext>(OnDirectionChanged);
            m_PollHierarchyItem = schedule.Execute(PollHierarchy).Every(50L);
            m_ScrollView.verticalScroller.valueChanged += OnVerticalScrollerChanged;
            m_ScrollView.horizontalScroller.valueChanged += OnHorizontalScrollerChanged;
            RegisterCallback<ActionTriggeredEvent>(OnItemClicked);
            m_TransitionEndAction = OnIndicatorTransitionEnd;
        }

        void OnIndicatorTransitionEnd(ITransitionEvent evt)
        {
            m_Indicator.UnregisterCallback<TransitionEndEvent>(m_TransitionEndAction);
            m_Indicator.UnregisterCallback<TransitionCancelEvent>(m_TransitionEndAction);
            m_Indicator.RemoveFromClassList(animatedIndicatorUssClassName);

            // make sure the indicator is in the right position,
            // that would happen if the geometry has changed just after clicking on a tab
            RefreshIndicator();
        }

        void OnDirectionChanged(ContextChangedEvent<DirContext> evt)
        {
            RefreshVisuals();
        }

        /// <summary>
        /// The size of the Tabs.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The direction of the Tabs. Horizontal or Vertical.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetOrientationUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetOrientationUssClassName(m_Direction));
                m_ScrollView.mode = m_Direction switch
                {
                    Direction.Vertical => ScrollViewMode.Vertical,
                    _ => ScrollViewMode.Horizontal
                };
                SetValueWithoutNotify(m_Value);

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The current list of items used to populate the Tabs.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public IList items => m_SourceItems ?? m_StaticItems;

        /// <summary>
        /// The emphasized mode of the Tabs.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set
            {
                var changed = emphasized != value;
                EnableInClassList(emphasizedUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in emphasizedProperty);
            }
        }

        /// <summary>
        /// The justified mode of the Tabs.
        /// </summary>
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
        /// Method to bind the TabItem.
        /// </summary>
        [CreateProperty]
        public Action<TabItem, int> bindItem
        {
            get => m_BindItem;

            set
            {
                var changed = m_BindItem != value;
                m_BindItem = value;
                RefreshItems();

                if (changed)
                    NotifyPropertyChanged(in bindItemProperty);
            }
        }

        /// <summary>
        /// Method to unbind the TabItem.
        /// </summary>
        [CreateProperty]
        public Action<TabItem, int> unbindItem
        {
            get => m_UnbindItem;
            set
            {
                var changed = m_UnbindItem != value;
                m_UnbindItem = value;
                RefreshItems();

                if (changed)
                    NotifyPropertyChanged(in unbindItemProperty);
            }
        }

        /// <summary>
        /// Collection of items used to populate the Tabs.
        /// </summary>
        [CreateProperty]
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                if (m_SourceItems == value)
                    return;

                m_SourceItems = value;

                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;
                RefreshItems();

                NotifyPropertyChanged(in sourceItemsProperty);
                NotifyPropertyChanged(in itemsProperty);
            }
        }

        /// <summary>
        /// The virtual content container of the Tabs.
        /// </summary>
        public override VisualElement contentContainer => m_LambdaContainer;

        /// <summary>
        /// The item container of the Tabs.
        /// </summary>
        public VisualElement itemContainer => m_Container;

        /// <summary>
        /// Set the value of the Tabs without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value.</param>
        /// <exception cref="ValueOutOfRangeException"> Throws if the value is out of range.</exception>
        public void SetValueWithoutNotify(int newValue)
        {
            SetValueWithoutNotifyInternal(newValue);
        }

        void SetValueWithoutNotifyInternal(int newValue, bool scroll = true, bool animateIndicator = false)
        {
            var previousValue = m_Value;
            m_Value = IsValid(newValue) ? newValue : previousValue;

            // refresh selection visually
            if (previousValue >= 0 && previousValue < m_Items.Count && previousValue != m_Value)
                m_Items[previousValue].selected = false;

            RefreshVisuals(scroll, animateIndicator);
        }

        void RefreshVisuals(bool scroll = true, bool animateIndicator = false)
        {
            if (panel == null || !paddingRect.IsValid())
                return;

            if (m_Value >= 0 && m_Value < m_Items.Count)
            {
                m_Items[m_Value].selected = true;
                if (scroll)
                    m_ScrollView.ScrollTo(m_Items[m_Value]);
                m_ScheduledRefreshIndicator?.Pause();
                m_Indicator.EnableInClassList(animatedIndicatorUssClassName, animateIndicator);
                m_Indicator.RegisterCallback<TransitionEndEvent>(m_TransitionEndAction);
                m_Indicator.RegisterCallback<TransitionCancelEvent>(m_TransitionEndAction);
                m_ScheduledRefreshIndicator = schedule.Execute(RefreshIndicator);
            }
            else
            {
                m_ScheduledRefreshIndicator?.Pause();
                m_Indicator.RemoveFromClassList(animatedIndicatorUssClassName);
                if (direction == Direction.Horizontal)
                {
                    m_Indicator.style.left = 0;
                    m_Indicator.style.width = 0;
                    m_Indicator.style.top = StyleKeyword.Null;
                    m_Indicator.style.height = StyleKeyword.Null;
                }
                else
                {
                    m_Indicator.style.top = 0;
                    m_Indicator.style.height = 0;
                    m_Indicator.style.left = StyleKeyword.Null;
                    m_Indicator.style.width = StyleKeyword.Null;
                }
            }
        }

        void RefreshIndicator()
        {
            var item = m_Items.Count > m_Value && m_Value >= 0 ? m_Items[m_Value] : null;
            if (item == null)
            {
                m_Indicator.style.left = 0;
                m_Indicator.style.width = 0;
                m_Indicator.style.top = 0;
                m_Indicator.style.height = 0;
                return;
            }
            switch (direction)
            {
                case Direction.Horizontal:
                    var x = item.layout.x - m_ScrollView.scrollOffset.x;
                    if (!Mathf.Approximately(x, m_Indicator.resolvedStyle.left))
                        m_Indicator.style.left = x;
                    if (!Mathf.Approximately(item.layout.width, m_Indicator.resolvedStyle.width))
                        m_Indicator.style.width = item.layout.width;
                    m_Indicator.style.height = StyleKeyword.Null;
                    m_Indicator.style.top = StyleKeyword.Null;
                    break;
                case Direction.Vertical:
                    var y = item.layout.y - m_ScrollView.scrollOffset.y;
                    if (!Mathf.Approximately(y, m_Indicator.resolvedStyle.top))
                        m_Indicator.style.top = y;
                    if (!Mathf.Approximately(item.layout.height, m_Indicator.resolvedStyle.scale.value.y))
                        m_Indicator.style.height = item.layout.height;
                    m_Indicator.style.left = StyleKeyword.Null;
                    m_Indicator.style.width = StyleKeyword.Null;
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(direction), direction);
            }
        }

        /// <summary>
        /// The value of the Tabs. This is the index of the selected TabItem.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int value
        {
            get => m_Value;
            set
            {
                if (value == m_Value || !IsValid(value))
                    return;

                var previousValue = m_Value;
                SetValueWithoutNotifyInternal(value, true, true);

                using var evt = ChangeEvent<int>.GetPooled(previousValue, value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        bool IsValid(int v)
        {
            if (v == -1)
                return true;
            return v >= 0 && v < m_Items.Count && m_Items[v].enabledSelf;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;

            if (direction == Direction.Horizontal)
            {
                if (evt.keyCode == KeyCode.LeftArrow)
                    handled = GoToPrevious();
                else if (evt.keyCode == KeyCode.RightArrow) handled = GoToNext();
            }
            else
            {
                if (evt.keyCode == KeyCode.UpArrow)
                    handled = GoToPrevious();
                else if (evt.keyCode == KeyCode.DownArrow) handled = GoToNext();
            }

            if (handled)
            {

                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Go to the next TabItem.
        /// </summary>
        /// <returns> True if the next TabItem is selected, false otherwise.</returns>
        public bool GoToNext()
        {
            var nextIndex = m_Value + 1;
            while (nextIndex < m_Items.Count && !m_Items[nextIndex].enabledSelf)
                nextIndex++;
            if (nextIndex >= m_Items.Count || nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        /// <summary>
        /// Go to the previous TabItem.
        /// </summary>
        /// <returns> True if the previous TabItem is selected, false otherwise.</returns>
        public bool GoToPrevious()
        {
            // From the deselected state (value == -1) scan back from the last item, mirroring
            // GoToNext which scans forward from the first, so Left/Up can reselect a tab too.
            var nextIndex = (m_Value == -1 ? m_Items.Count : m_Value) - 1;
            while (nextIndex >= 0 && !m_Items[nextIndex].enabledSelf)
                nextIndex--;
            if (nextIndex < 0 || nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        void OnHorizontalScrollerChanged(float offset)
        {
            if (direction == Direction.Horizontal)
                SetValueWithoutNotifyInternal(value, false);
        }

        void OnVerticalScrollerChanged(float offset)
        {
            if (direction == Direction.Vertical)
                SetValueWithoutNotifyInternal(value, false);
        }

        void PollHierarchy()
        {
            if (m_StaticItems == null && childCount > 0 && m_SourceItems == null)
            {
                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;
                m_StaticItems = new List<TabItem>();
                foreach (var c in Children())
                {
                    m_StaticItems.Add((TabItem)c);
                }

                NotifyPropertyChanged(in itemsProperty);

                RefreshItems();
            }
        }

        void RefreshItems()
        {
            for (var i = 0; i < itemContainer.childCount; i++)
            {
                var item = (TabItem)itemContainer.ElementAt(i);
                unbindItem?.Invoke(item, i);
                item.UnregisterCallback<GeometryChangedEvent>(OnItemGeometryChanged);
            }

            itemContainer.Clear();
            m_Items.Clear();

            if (m_SourceItems != null)
            {
                for (var i = 0; i < m_SourceItems.Count; i++)
                {
                    var item = new TabItem();
                    bindItem?.Invoke(item, i);
                    item.RegisterCallback<GeometryChangedEvent>(OnItemGeometryChanged);
                    itemContainer.Add(item);
                    m_Items.Add(item);
                }
            }
            else if (m_StaticItems != null)
            {
                foreach (var item in m_StaticItems)
                {
                    item.RegisterCallback<GeometryChangedEvent>(OnItemGeometryChanged);
                    itemContainer.Add(item);
                    m_Items.Add(item);
                }
            }

            if (itemContainer.childCount > 0)
            {
                // find the next valid item
                var newValue = 0;
                while (newValue < m_Items.Count && m_Items[newValue].enabledSelf == false)
                    newValue++;
                if (newValue < m_Items.Count)
                    SetValueWithoutNotifyInternal(newValue);
                else
                    SetValueWithoutNotifyInternal(-1);
            }
            else
            {
                SetValueWithoutNotifyInternal(-1);
            }
        }

        void OnItemGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_Indicator.ClassListContains(animatedIndicatorUssClassName))
                return;

            if (evt.target is TabItem { selected: true })
                SetValueWithoutNotify(m_Value);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            SetValueWithoutNotify(m_Value);
        }

        void OnItemClicked(ActionTriggeredEvent evt)
        {
            if (evt.target is TabItem item)
            {
                var newValue = item.parent.IndexOf(item);
                if (value != newValue)
                    value = item.parent.IndexOf(item);
                else
                    RefreshVisuals(true, true);
                evt.StopPropagation();
            }
        }

    }
}
