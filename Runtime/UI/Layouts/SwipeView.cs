using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The strategy used when going to a specific item in the SwipeView.
    /// </summary>
    public enum GoToStrategy
    {
        /// <summary>
        /// The SwipeView will animate forward until it reaches the item.
        /// If the target item is before the current item, it will animate backward if the wrap property is set to false,
        /// otherwise it will animate forward until it reaches the item.
        /// </summary>
        Forward,
        /// <summary>
        /// The SwipeView will animate backward until it reaches the item.
        /// If the target item is after the current item, it will animate forward if the wrap property is set to false,
        /// otherwise it will animate backward until it reaches the item.
        /// </summary>
        Backward,
        /// <summary>
        /// The SwipeView will animate to the item using the shortest path, by taking into account the wrap property.
        /// </summary>
        ShortestPath,
        /// <summary>
        /// The SwipeView will animate to the item using the longest path, by taking into account the wrap property.
        /// </summary>
        LongestPath
    }

    /// <summary>
    /// A SwipeViewItem is an item that must be used as a child of a <see cref="SwipeView"/>.
    /// </summary>
    [UxmlElement]
    public partial class SwipeViewItem : BaseVisualElement
    {
        /// <summary>
        /// The main styling class of the SwipeViewItem. This is the class that is used in the USS file.
        /// </summary>
        public const string ussClassName = "appui-swipeview-item";

        /// <summary>
        /// The index of the item in the SwipeView.
        /// </summary>
        public int index { get; internal set; }

        /// <summary>
        /// The SwipeView that contains this item.
        /// </summary>
        public SwipeView view => parent as SwipeView;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SwipeViewItem()
        {
            AddToClassList(ussClassName);

            // Optimize for frequent transforms during swipe animations
            usageHints |= UsageHints.DynamicTransform;

            // Note: Positioning and sizing will be handled by the parent SwipeView
            // Items will be set to absolute positioning and sized based on visibleItemCount
        }

    }

    /// <summary>
    /// A container that allows users to swipe between multiple items with smooth animations and customizable
    /// behavior.
    /// </summary>
    /// <remarks>
    /// A SwipeView is a container that displays one or more children at a time and provides a UI to
    /// navigate between them. It is similar to a <see cref="ScrollView"/> but here children are
    /// snapped to the container's edges. See <see cref="PageView"/> for a similar container that
    /// includes a page indicator.
    ///
    /// The SwipeView is a versatile container component that enables users to navigate through content using
    /// swipe gestures or programmatic controls. It's perfect for creating image carousels, onboarding flows, or
    /// any interface where users need to browse through a sequence of items.
    ///
    /// Key features include:
    /// - Horizontal or vertical orientation
    /// - Smooth animations with customizable speed and easing
    /// - Support for both touch/mouse swipe gestures and keyboard navigation
    /// - Optional wrapping behavior
    /// - Configurable resistance and thresholds
    /// - Auto-play capability
    ///
    /// The SwipeView can be populated either through UXML by adding SwipeViewItem elements as children, or
    /// programmatically by setting a data source and providing bind/unbind callbacks.
    /// </remarks>
    /// <example>
    /// <para>Creating an image carousel with SwipeView</para>
    /// <code lang="xml"><![CDATA[
    /// <SwipeView class="image-carousel">
    ///     <SwipeViewItem>
    ///         <Image src="image1.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Image src="image2.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Image src="image3.png" />
    ///     </SwipeViewItem>
    /// </SwipeView>
    /// ]]></code>
    /// <para>Implementing a dynamic image gallery with data binding</para>
    /// <code lang="csharp"><![CDATA[
    /// public class Gallery : VisualElement
    /// {
    ///     private SwipeView swipeView;
    ///     private List<string> imageUrls;
    ///
    ///     public Gallery()
    ///     {
    ///         swipeView = new SwipeView
    ///         {
    ///             direction = Direction.Horizontal,
    ///             wrap = true,
    ///             autoPlayDuration = 3000,
    ///             snapAnimationSpeed = 0.5f
    ///         };
    ///
    ///         swipeView.sourceItems = imageUrls;
    ///         swipeView.bindItem = (item, index) => {
    ///             var image = new Image { image = LoadImage(imageUrls[index]) };
    ///             item.Add(image);
    ///         };
    ///
    ///         Add(swipeView);
    ///     }
    /// }
    /// ]]></code>
    /// <para>Creating an onboarding flow with vertical swiping</para>
    /// <code lang="xml"><![CDATA[
    /// <SwipeView direction="Vertical" class="onboarding">
    ///     <SwipeViewItem>
    ///         <Label text="Welcome" />
    ///         <Image src="welcome.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Label text="Features" />
    ///         <Image src="features.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Label text="Get Started" />
    ///         <Button text="Continue" />
    ///     </SwipeViewItem>
    /// </SwipeView>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class SwipeView : BaseVisualElement, INotifyValueChanged<int>
    {

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));

        internal static readonly BindingId wrapProperty = new BindingId(nameof(wrap));

        internal static readonly BindingId visibleItemCountProperty = new BindingId(nameof(visibleItemCount));

        internal static readonly BindingId skipAnimationThresholdProperty = new BindingId(nameof(skipAnimationThreshold));

        internal static readonly BindingId autoPlayDurationProperty = new BindingId(nameof(autoPlayDuration));

        internal static readonly BindingId swipeableProperty = new BindingId(nameof(swipeable));

        internal static readonly BindingId resistanceProperty = new BindingId(nameof(resistance));

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId canGoToNextProperty = new BindingId(nameof(canGoToNext));

        internal static readonly BindingId canGoToPreviousProperty = new BindingId(nameof(canGoToPrevious));

        internal static readonly BindingId currentItemProperty = new BindingId(nameof(currentItem));

        internal static readonly BindingId countProperty = new BindingId(nameof(count));

        internal static readonly BindingId sourceItemsProperty = new BindingId(nameof(sourceItems));

        internal static readonly BindingId bindItemProperty = new BindingId(nameof(bindItem));

        internal static readonly BindingId unbindItemProperty = new BindingId(nameof(unbindItem));

        internal static readonly BindingId snapAnimationSpeedProperty = new BindingId(nameof(snapAnimationSpeed));

        internal static readonly BindingId snapAnimationEasingProperty = new BindingId(nameof(snapAnimationEasing));

        internal static readonly BindingId startSwipeThresholdProperty = new BindingId(nameof(startSwipeThreshold));

        internal static readonly BindingId goToStrategyProperty = new BindingId(nameof(goToStrategy));


        /// <summary>
        /// The main styling class of the SwipeView. This is the class that is used in the USS file.
        /// </summary>
        public const string ussClassName = "appui-swipeview";

        /// <summary>
        /// The styling class applied to the container of the SwipeView.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The styling class applied to the SwipeView depending on its orientation.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The default duration of the auto play animation.
        /// </summary>
        public const int noAutoPlayDuration = -1;

        bool m_Wrap;

        float m_AnimationDirection;

        List<SwipeViewItem> m_StaticItems;

        Direction m_Direction;

        int m_Value = -1;

        ValueAnimation<float> m_Animation;

        int m_VisibleItemCount = k_DefaultVisibleItemCount;

        GoToStrategy m_GoToStrategy = k_DefaultGoToStrategy;

        IVisualElementScheduledItem m_PollHierarchyItem;

        IList m_SourceItems;

        readonly VisualElement m_Container;

        bool m_ForceDisableWrap;

        readonly Scrollable m_Scrollable;

        int m_AutoPlayDuration = noAutoPlayDuration;

        IVisualElementScheduledItem m_AutoPlayAnimation;

        bool m_Swipeable;

        Dir m_CurrentDirection;

        // Current drag offset for smooth dragging
        float m_CurrentDragOffset = 0f;

        /// <summary>
        /// The container of the SwipeView.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        const float k_DefaultSnapAnimationSpeed = 0.5f;

        float m_SnapAnimationSpeed = k_DefaultSnapAnimationSpeed;

        /// <summary>
        /// The speed of the animation when snapping to an item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float snapAnimationSpeed
        {
            get => m_SnapAnimationSpeed;
            set
            {
                var changed = !Mathf.Approximately(m_SnapAnimationSpeed, value);
                m_SnapAnimationSpeed = value;

                if (changed)
                    NotifyPropertyChanged(in snapAnimationSpeedProperty);
            }
        }

        static readonly Func<float, float> k_DefaultSnapAnimationEasing = Easing.OutCubic;

        Func<float, float> m_SnapAnimationEasing = k_DefaultSnapAnimationEasing;

        /// <summary>
        /// The easing of the animation when snapping to an item.
        /// </summary>
        [CreateProperty]
        public Func<float, float> snapAnimationEasing
        {
            get => m_SnapAnimationEasing;
            set
            {
                var changed = m_SnapAnimationEasing != value;
                m_SnapAnimationEasing = value;

                if (changed)
                    NotifyPropertyChanged(in snapAnimationEasingProperty);
            }
        }

        const float k_DefaultStartSwipeThreshold = 5f;

        /// <summary>
        /// The amount of pixels that must be swiped before the SwipeView begins to swipe.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float startSwipeThreshold
        {
            get => m_Scrollable.threshold;
            set
            {
                var changed = !Mathf.Approximately(m_Scrollable.threshold, value);
                m_Scrollable.threshold = value;

                if (changed)
                    NotifyPropertyChanged(in startSwipeThresholdProperty);
            }
        }

        const GoToStrategy k_DefaultGoToStrategy = GoToStrategy.ShortestPath;

        /// <summary>
        /// The strategy used when going to a specific item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public GoToStrategy goToStrategy
        {
            get => m_GoToStrategy;
            set
            {
                var changed = m_GoToStrategy != value;
                m_GoToStrategy = value;
                if (changed)
                    NotifyPropertyChanged(in goToStrategyProperty);
            }
        }

        const int k_DefaultVisibleItemCount = 1;

        /// <summary>
        /// The number of items that are visible at the same time.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int visibleItemCount
        {
            get => m_VisibleItemCount;
            set
            {
                var changed = m_VisibleItemCount != value;
                m_VisibleItemCount = value;
                SetValueWithoutNotify(this.value);

                if (changed)
                {
                    NotifyPropertyChanged(in visibleItemCountProperty);
                    NotifyPropertyChanged(in canGoToNextProperty);
                    NotifyPropertyChanged(in canGoToPreviousProperty);
                }
            }
        }

        const int k_DefaultAutoPlayDuration = noAutoPlayDuration;

        /// <summary>
        /// The number of milliseconds between each automatic swipe.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int autoPlayDuration
        {
            get => m_AutoPlayDuration;
            set
            {
                if (m_AutoPlayDuration == value)
                    return;

                m_AutoPlayDuration = value;
                if (m_AutoPlayDuration > 0)
                {
                    m_AutoPlayAnimation = schedule.Execute(() => GoToNext());
                    m_AutoPlayAnimation.Every(m_AutoPlayDuration);
                }
                else
                {
                    m_AutoPlayAnimation?.Pause();
                    m_AutoPlayAnimation = null;
                }

                NotifyPropertyChanged(in autoPlayDurationProperty);
            }
        }

        const Direction k_DefaultDirection = Direction.Horizontal;

        /// <summary>
        /// The orientation of the SwipeView.
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
                m_Scrollable.direction = value == Direction.Horizontal ? ScrollViewMode.Horizontal : ScrollViewMode.Vertical;
                AddToClassList(GetDirectionUssClassName(m_Direction));
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        Action<SwipeViewItem, int> m_BindItem;

        /// <summary>
        /// A method that is called when an item is bound to the SwipeView.
        /// </summary>
        [CreateProperty]
        public Action<SwipeViewItem, int> bindItem
        {
            get => m_BindItem;
            set
            {
                var changed = m_BindItem != value;
                m_BindItem = value;
                RefreshList();

                if (changed)
                    NotifyPropertyChanged(in bindItemProperty);
            }
        }

        Action<SwipeViewItem, int> m_UnbindItem;

        /// <summary>
        /// A method that is called when an item is unbound from the SwipeView.
        /// </summary>
        [CreateProperty]
        public Action<SwipeViewItem, int> unbindItem
        {
            get => m_UnbindItem;
            set
            {
                var changed = m_UnbindItem != value;
                m_UnbindItem = value;
                RefreshList();

                if (changed)
                    NotifyPropertyChanged(in unbindItemProperty);
            }
        }

        /// <summary>
        /// The source of items that are used to populate the SwipeView.
        /// </summary>
        [CreateProperty]
        public IList sourceItems
        {
            get => m_SourceItems;
            set
            {
                var changed = m_SourceItems != value;
                m_SourceItems = value;

                // Stop Polling the hierarchy as we provided a new set of items
                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;

                RefreshList();

                if (changed)
                {
                    NotifyPropertyChanged(in sourceItemsProperty);
                    NotifyPropertyChanged(in countProperty);
                    NotifyPropertyChanged(in canGoToNextProperty);
                    NotifyPropertyChanged(in canGoToPreviousProperty);
                }
            }
        }

        const bool k_DefaultWrap = false;

        /// <summary>
        /// This property determines whether or not the view wraps around when it reaches the start or end.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool wrap
        {
            get => m_Wrap;
            set
            {
                var changed = m_Wrap != value;
                m_Wrap = value;
                PositionAllItems();

                if (changed)
                {
                    NotifyPropertyChanged(in wrapProperty);
                    NotifyPropertyChanged(in canGoToNextProperty);
                    NotifyPropertyChanged(in canGoToPreviousProperty);
                }
            }
        }

        /// <summary>
        /// The total number of items.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public int count => items?.Count ?? 0;

        /// <summary>
        /// Determine if the SwipeView should wrap around.
        /// </summary>
        internal bool shouldWrap => count > visibleItemCount && wrap && !m_ForceDisableWrap;

        bool ShouldResist(Vector2 delta)
        {
            if (wrap)
                return false;

            var currentOffset = m_CurrentDragOffset;
            var itemSize = direction == Direction.Horizontal ? contentRect.width / m_VisibleItemCount : contentRect.height / m_VisibleItemCount;

            // Calculate how much we can drag in each direction from current value
            var maxRightDrag = value * itemSize;  // Can drag right to show earlier items (down to value 0)
            var maxLeftDrag = -(count - m_VisibleItemCount - value) * itemSize;  // Can drag left to show later items (up to max valid value)

            if (direction == Direction.Horizontal)
            {
                return (currentOffset >= maxRightDrag && delta.x >= 0) || (currentOffset <= maxLeftDrag && delta.x <= 0);
            }
            else
            {
                return (currentOffset >= maxRightDrag && delta.y >= 0) || (currentOffset <= maxLeftDrag && delta.y <= 0);
            }
        }

        /// <summary>
        /// The current item.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public SwipeViewItem currentItem => GetItem(value);

        IList items => m_SourceItems ?? m_StaticItems;

        SwipeViewItem GetItem(int index)
        {
            foreach (var child in Children())
            {
                if (child is SwipeViewItem item && item.index == index)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// The event that is called when the value of the SwipeView changes (i.e. when its being swiped or when it snaps to an item).
        /// </summary>
        public event Action<SwipeViewItem, float> beingSwiped;

        /// <summary>
        /// The value of the SwipeView (i.e. the index of the current item).
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int value
        {
            get => m_Value;
            set => SetValue(value);
        }

        void SetValue(int newValue, bool findAnimationDirection = true)
        {
            if (newValue < 0 || newValue > count - 1)
                return;

            if (findAnimationDirection)
                m_AnimationDirection = FindAnimationDirection(newValue);

            var previousValue = m_Value;
            SetValueWithoutNotify(newValue);
            if (previousValue != m_Value)
            {
                using var evt = ChangeEvent<int>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }

                NotifyPropertyChanged(in valueProperty);
                NotifyPropertyChanged(in canGoToNextProperty);
                NotifyPropertyChanged(in canGoToPreviousProperty);
                NotifyPropertyChanged(in currentItemProperty);
        }

        const float k_DefaultResistance = 1f;

        float m_Resistance = k_DefaultResistance;

        /// <summary>
        /// <para>The resistance of the SwipeView.</para>
        /// <para>
        /// By default, the SwipeView has a resistance of 1.
        /// </para>
        /// <para>
        /// If you set this property to more than 1, the SwipeView will
        /// be harder to swipe. If you set this property to less than 1, the SwipeView will be easier to swipe.
        /// </para>
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float resistance
        {
            get => m_Resistance;
            set
            {
                var changed = !Mathf.Approximately(m_Resistance, value);
                m_Resistance = value;

                if (changed)
                    NotifyPropertyChanged(in resistanceProperty);
            }
        }

        const bool k_DefaultSwipeable = true;

        /// <summary>
        /// <para>Whether or not the SwipeView is swipeable.</para>
        /// <para>
        /// By default, the SwipeView is swipeable. If you set this property to <see langword="false" />, you won't be
        /// able to interact with the SwipeView (except programmatically).
        /// </para>
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool swipeable
        {
            get => m_Swipeable;
            set
            {
                var changed = m_Swipeable != value;
                m_Swipeable = value;
                if (m_Swipeable)
                    this.AddManipulator(m_Scrollable);
                else
                    this.RemoveManipulator(m_Scrollable);

                if (changed)
                    NotifyPropertyChanged(in swipeableProperty);
            }
        }

        const int k_DefaultSkipAnimationThreshold = 2;

        int m_SkipAnimationThreshold = k_DefaultSkipAnimationThreshold;

        /// <summary>
        /// This property determines the threshold at which the animation will be skipped.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int skipAnimationThreshold
        {
            get => m_SkipAnimationThreshold;
            set
            {
                var changed = m_SkipAnimationThreshold != value;
                m_SkipAnimationThreshold = value;

                if (changed)
                    NotifyPropertyChanged(in skipAnimationThresholdProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SwipeView()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            m_Container = new VisualElement
            {
                name = containerUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_Container.AddToClassList(containerUssClassName);

            // Optimize for group transforms during animations
            m_Container.usageHints |= UsageHints.GroupTransform;

            hierarchy.Add(m_Container);

            m_PollHierarchyItem = schedule.Execute(PollHierarchy).Every(50L);

            m_Scrollable = new Scrollable(OnDrag, OnUp, OnDown);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_Container.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);

            direction = k_DefaultDirection;
            wrap = k_DefaultWrap;
            visibleItemCount = k_DefaultVisibleItemCount;
            autoPlayDuration = k_DefaultAutoPlayDuration;
            swipeable = k_DefaultSwipeable;
            resistance = k_DefaultResistance;
            skipAnimationThreshold = k_DefaultSkipAnimationThreshold;
            startSwipeThreshold = k_DefaultStartSwipeThreshold;
            snapAnimationSpeed = k_DefaultSnapAnimationSpeed;
            snapAnimationEasing = k_DefaultSnapAnimationEasing;

            this.RegisterContextChangedCallback<DirContext>(OnDirectionChanged);
        }

        void OnDirectionChanged(ContextChangedEvent<DirContext> evt)
        {
            m_CurrentDirection = evt.context?.dir ?? Dir.Ltr;
            schedule.Execute(() => {
                RefreshItemsSize();
                PositionAllItems();
            });
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            RefreshItemsSize();
            PositionAllItems();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var handled = false;
            if (evt.target == this)
            {
                if (direction == Direction.Horizontal)
                {
                    if (evt.keyCode == KeyCode.LeftArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.RightArrow)
                        handled = GoToNext();
                }
                else
                {
                    if (evt.keyCode == KeyCode.UpArrow)
                        handled = GoToPrevious();
                    else if (evt.keyCode == KeyCode.DownArrow)
                        handled = GoToNext();
                }
            }

            if (handled)
            {
                evt.StopPropagation();
            }
        }

        void OnDown(Scrollable draggable)
        {
            // Stop any ongoing animation
            if (m_Animation != null && !m_Animation.IsRecycled())
                m_Animation.Recycle();
        }

        void OnUp(Scrollable draggable)
        {
            m_ForceDisableWrap = true;
            var closestElement = GetClosestElement(out var d);
            var closestIndex = closestElement?.index ?? -1;

            if (closestIndex < 0)
            {
                m_ForceDisableWrap = false;
                return;
            }

            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            // If we're very close already, just snap without animation
            if (Mathf.Abs(d) < itemSize * 0.05f)
            {
                // Update value immediately and snap to final position
                UpdateValueAndSnap(closestIndex);
            }
            else
            {
                // Simple animation: animate current drag offset to center the closest element
                var targetOffset = m_CurrentDragOffset + d;
                StartSimpleSnapAnimation(closestIndex, targetOffset);
            }

            m_ForceDisableWrap = false;
        }

        /// <summary>
        /// Simple animation for manual drag release - just animate drag offset
        /// </summary>
        void StartSimpleSnapAnimation(int targetValue, float targetOffset)
        {
            // Recycle any previous animation
            if (m_Animation != null && !m_Animation.IsRecycled())
                m_Animation.Recycle();

            var startOffset = m_CurrentDragOffset;
            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            // Calculate duration based on distance
            var distance = Mathf.Abs(targetOffset - startOffset);
            var itemDistance = distance / itemSize;
            var durationInMilliseconds = (int)(itemDistance * snapAnimationSpeed * 1000f);

            if (durationInMilliseconds < 1)
                durationInMilliseconds = 1;

            // Simple animation from current offset to target offset
            m_Animation = experimental.animation.Start(startOffset, targetOffset, durationInMilliseconds, (_, currentOffset) =>
            {
                m_CurrentDragOffset = currentOffset;
                PositionAllItems(currentOffset);
            }).Ease(snapAnimationEasing).OnCompleted(() =>
            {
                // Update value when animation completes
                UpdateValueAndSnap(targetValue);
            }).KeepAlive();
        }

        /// <summary>
        /// Update value and snap to clean position
        /// </summary>
        void UpdateValueAndSnap(int newValue)
        {
            var previousValue = m_Value;

            // Update value and CSS classes
            var from = previousValue >= 0 ? GetItem(previousValue) : null;
            var to = GetItem(newValue);
            from?.RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            to?.AddToClassList(Styles.selectedUssClassName);

            // Reset drag offset and position cleanly
            m_CurrentDragOffset = 0f;
            PositionAllItems();

            // Fire change event
            if (previousValue != m_Value)
            {
                using var evt = ChangeEvent<int>.GetPooled(previousValue, m_Value);
                evt.target = this;
                SendEvent(evt);
            }

            NotifyPropertyChanged(in valueProperty);
            NotifyPropertyChanged(in canGoToNextProperty);
            NotifyPropertyChanged(in canGoToPreviousProperty);
            NotifyPropertyChanged(in currentItemProperty);
        }

        void OnDrag(Scrollable drag)
        {
            if (m_Animation != null && !m_Animation.IsRecycled())
                m_Animation.Recycle();

            var multiplier = ShouldResist(drag.deltaPos) ? 1f / resistance : 1f;
            var delta = direction == Direction.Horizontal ? drag.deltaPos.x : drag.deltaPos.y;

            m_AnimationDirection = Mathf.Approximately(0, delta) ? 0 : Mathf.Sign(-delta);

            // Update the current drag offset and position all items
            m_CurrentDragOffset += delta * multiplier;
            PositionAllItems(m_CurrentDragOffset);
        }

        float FindAnimationDirection(int newIndex)
        {
            if (newIndex < 0 || newIndex >= count)
                return 0;

            var currentIndex = value;
            if (currentIndex < 0 || currentIndex >= count)
                return 0f;

            if (currentIndex == newIndex)
                return 0f;

            if (m_GoToStrategy == GoToStrategy.Forward)
            {
                if (newIndex > currentIndex)
                    return 1f;
                if (newIndex < currentIndex && !shouldWrap)
                    return -1f; // Go backward if no wrap
                return 1f; // Wrap forward
            }
            else if (m_GoToStrategy == GoToStrategy.Backward)
            {
                if (newIndex < currentIndex)
                    return -1f;
                if (newIndex > currentIndex && !shouldWrap)
                    return 1f; // Go forward if no wrap
                return -1f; // Wrap backward
            }
            else if (m_GoToStrategy == GoToStrategy.ShortestPath)
            {
                if (!shouldWrap)
                {
                    // No wrapping - just go in the direct direction
                    return newIndex > currentIndex ? 1f : -1f;
                }
                else
                {
                    // With wrapping - calculate shortest path
                    var forwardDistance = newIndex > currentIndex
                        ? newIndex - currentIndex
                        : count - currentIndex + newIndex;
                    var backwardDistance = newIndex < currentIndex
                        ? currentIndex - newIndex
                        : currentIndex + count - newIndex;
                    return forwardDistance <= backwardDistance ? 1f : -1f;
                }
            }
            else // LongestPath
            {
                if (!shouldWrap)
                {
                    // No wrapping - just go in the direct direction
                    return newIndex > currentIndex ? 1f : -1f;
                }
                else
                {
                    // With wrapping - calculate longest path
                    var forwardDistance = newIndex > currentIndex
                        ? newIndex - currentIndex
                        : count - currentIndex + newIndex;
                    var backwardDistance = newIndex < currentIndex
                        ? currentIndex - newIndex
                        : currentIndex + count - newIndex;
                    return forwardDistance > backwardDistance ? 1f : -1f;
                }
            }
        }

        SwipeViewItem GetClosestElement(out float distance)
        {
            distance = 0f;
            if (items == null || count <= 0)
                return null;

            SwipeViewItem best = null;
            var bestDistance = float.MaxValue;
            var containerMin = direction == Direction.Horizontal ? paddingRect.x : paddingRect.y;

            for (var i = 0; i < childCount; i++)
            {
                var candidate = (SwipeViewItem)ElementAt(i);

                // Get the current translate value to determine actual position
                var translate = candidate.resolvedStyle.translate;
                var candidatePos = direction == Direction.Horizontal
                    ? translate.x
                    : translate.y;

                var d = containerMin - candidatePos;
                var candidateDistance = Mathf.Abs(d);
                if (candidateDistance < bestDistance)
                {
                    bestDistance = candidateDistance;
                    distance = d;
                    best = candidate;
                }
            }

            return best;
        }

        /// <summary>
        /// Position all items based on current value and drag offset
        /// </summary>
        void PositionAllItems(float dragOffset = 0f)
        {
            if (!contentRect.IsValid() || childCount == 0)
                return;

            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            for (var i = 0; i < childCount; i++)
            {
                var item = ElementAt(i) as SwipeViewItem;
                if (item == null) continue;

                // Base position relative to the current value
                var basePosition = (item.index - value) * itemSize + dragOffset;

                // Handle wrapping if enabled - ensure no blank spaces are visible
                if (shouldWrap && count > 0)
                {
                    var containerSize = direction == Direction.Horizontal ? contentRect.width : contentRect.height;
                    var totalItemsSize = count * itemSize;

                    // Define extended viewport (including buffer zones)
                    var viewportStart = -itemSize;
                    var viewportEnd = containerSize + itemSize;

                    // Wrap element to ensure it's positioned optimally to fill potential gaps
                    // Move items that are too far left to the right side
                    while (basePosition < viewportStart - totalItemsSize)
                        basePosition += totalItemsSize;

                    // Move items that are too far right to the left side
                    while (basePosition > viewportEnd + totalItemsSize)
                        basePosition -= totalItemsSize;

                    // If the item is outside the extended viewport, find the best wrapping position
                    if (basePosition < viewportStart || basePosition > viewportEnd)
                    {
                        // Find which wrap position puts the item closest to filling the viewport
                        var wrapPositions = new[]
                        {
                            basePosition,
                            basePosition + totalItemsSize,
                            basePosition - totalItemsSize
                        };

                        var bestPosition = basePosition;
                        var bestScore = float.MaxValue;

                        foreach (var pos in wrapPositions)
                        {
                            // Score based on how well this position helps fill the viewport
                            var score = 0f;

                            if (pos >= viewportStart && pos <= viewportEnd)
                            {
                                // Inside viewport - excellent
                                score = 0f;
                            }
                            else if (pos < viewportStart)
                            {
                                // Too far left
                                score = viewportStart - pos;
                            }
                            else
                            {
                                // Too far right
                                score = pos - viewportEnd;
                            }

                            if (score < bestScore)
                            {
                                bestScore = score;
                                bestPosition = pos;
                            }
                        }

                        basePosition = bestPosition;
                    }
                }

                // Use GPU-optimized translate instead of left/top
                if (direction == Direction.Horizontal)
                    item.style.translate = new Translate(basePosition, 0);
                else
                    item.style.translate = new Translate(0, basePosition);
            }

            InvokeSwipeEvents();
        }



        /// <summary>
        /// Sets the value without notifying the listeners.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void SetValueWithoutNotify(int newValue)
        {
            if (count == 0)
            {
                m_Value = -1;
                return;
            }

            if (newValue < 0 || newValue > count - 1)
                return;

            if (!shouldWrap)
            {
                newValue = Mathf.Clamp(newValue, 0, count - m_VisibleItemCount);
            }

            var oldValue = m_Value;
            var from = oldValue >= 0 ? GetItem(oldValue) : null;
            var to = GetItem(newValue);

            // Update value immediately - animation is just visual feedback
            from?.RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            to?.AddToClassList(Styles.selectedUssClassName);

            // Animate if there's a direction set, value changed, and we have valid elements to animate
            bool shouldAnimate = oldValue != newValue &&
                               !Mathf.Approximately(m_AnimationDirection, 0f) &&
                               paddingRect.IsValid() &&
                               contentRect.IsValid();

            if (shouldAnimate)
            {
                // Programmatic call - use full animation logic
                StartSwipeAnimation(oldValue, newValue);
            }
            else
            {
                // No animation needed - refresh sizes and position immediately (snap behavior)
                RefreshItemsSize();
                m_CurrentDragOffset = 0f;
                PositionAllItems();
            }
        }



        void StartSwipeAnimation(int oldValue, int newValue)
        {
            // Recycle any previous animation
            if (m_Animation != null && !m_Animation.IsRecycled())
                m_Animation.Recycle();

            // Calculate the actual animation distance considering wrap and direction
            var actualDistance = CalculateAnimationDistance(oldValue, newValue);

            // Skip animation if distance is too great
            if (actualDistance > skipAnimationThreshold)
            {
                // Debug.Log($"Skipping animation: distance {actualDistance} > threshold {skipAnimationThreshold}");
                m_CurrentDragOffset = 0f;
                PositionAllItems();
                return;
            }

            if (!contentRect.IsValid())
            {
                // Debug.Log("Skipping animation: contentRect not valid");
                m_CurrentDragOffset = 0f;
                PositionAllItems();
                return;
            }

            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            var startOffset = m_CurrentDragOffset;
            var targetOffset = 0f;

            // Only add visual offset for programmatic calls (not drag releases)
            // If m_CurrentDragOffset is near zero, this is likely a programmatic call
            if (Mathf.Abs(m_CurrentDragOffset) < itemSize * 0.1f)
            {
                // Programmatic call - calculate visual offset to maintain appearance
                var visualOffset = m_AnimationDirection * actualDistance * itemSize;
                startOffset = m_CurrentDragOffset + visualOffset;
            }
            // else: Manual drag release - use current drag offset as-is

            // Calculate duration based on actual visual distance to travel
            var visualDistance = Mathf.Abs(startOffset - targetOffset);
            var itemDistance = visualDistance / itemSize;
            var durationInMilliseconds = (int)(itemDistance * snapAnimationSpeed * 1000f);

            // Ensure minimum duration
            if (durationInMilliseconds < 1)
                durationInMilliseconds = 1;

            // Set the starting offset
            m_CurrentDragOffset = startOffset;
            PositionAllItems(startOffset);

            // Debug.Log($"Starting animation: {oldValue} → {newValue}, actualDistance: {actualDistance}, startOffset: {startOffset}, duration: {durationInMilliseconds}ms");

            // Start the animation
            m_Animation = experimental.animation.Start(startOffset, targetOffset, durationInMilliseconds, (_, currentOffset) =>
            {
                m_CurrentDragOffset = currentOffset;
                PositionAllItems(currentOffset);
            }).Ease(snapAnimationEasing).OnCompleted(() =>
            {
                // Ensure final position is exact
                m_CurrentDragOffset = 0f;
                PositionAllItems();
            }).KeepAlive();
        }

        /// <summary>
        /// Calculate the actual visual distance needed for animation, respecting m_AnimationDirection
        /// </summary>
        /// <param name="fromIndex">The index to animate from.</param>
        /// <param name="toIndex">The index to animate to.</param>
        float CalculateVisualDistance(int fromIndex, int toIndex)
        {
            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            // Basic visual distance (direct path)
            var directDistance = (fromIndex - toIndex) * itemSize;

            if (!shouldWrap)
            {
                // No wrapping - use direct distance
                return directDistance;
            }

            return m_AnimationDirection switch
            {
                // With wrapping, respect the animation direction
                // Forward animation
                // If direct path is already forward (negative/left direction), use it
                > 0 when directDistance <= 0 => directDistance,

                // Direct path is backward, so wrap forward
                > 0 => directDistance - (count * itemSize),

                // Backward animation
                // If direct path is already backward (positive/right direction), use it
                < 0 when directDistance >= 0 => directDistance,

                // Direct path is forward, so wrap backward
                < 0 => directDistance + (count * itemSize),
                _ => directDistance
            };
        }

        /// <summary>
        /// Calculate the actual animation distance considering wrap and animation direction
        /// </summary>
        int CalculateAnimationDistance(int oldValue, int newValue)
        {
            // Calculate visual distance that respects animation direction
            var visualDistance = CalculateVisualDistance(oldValue, newValue);

            var itemSize = direction == Direction.Horizontal
                ? contentRect.width / m_VisibleItemCount
                : contentRect.height / m_VisibleItemCount;

            // Convert visual distance to item distance
            return Mathf.RoundToInt(Mathf.Abs(visualDistance) / itemSize);
        }

        void PollHierarchy()
        {
            if (m_StaticItems == null && childCount > 0 && m_SourceItems == null)
            {
                m_PollHierarchyItem?.Pause();
                m_PollHierarchyItem = null;
                m_StaticItems = new List<SwipeViewItem>();
                foreach (var c in Children())
                {
                    m_StaticItems.Add((SwipeViewItem)c);
                }
                RefreshList();

                NotifyPropertyChanged(in sourceItemsProperty);
                NotifyPropertyChanged(in countProperty);
                NotifyPropertyChanged(in canGoToNextProperty);
                NotifyPropertyChanged(in canGoToPreviousProperty);
            }
        }

        /// <summary>
        /// Refresh size and reset transforms for all items.
        /// WARNING: This resets all translate values to (0,0) - do not call during animations!
        /// </summary>
        void RefreshItemsSize()
        {
            if (!contentRect.IsValid())
                return;

            foreach (var c in Children())
            {
                // Set absolute positioning for independent movement
                c.style.position = Position.Absolute;

                // Size based on visible item count
                if (direction == Direction.Horizontal)
                {
                    c.style.width = contentRect.width / m_VisibleItemCount;
                    c.style.height = contentRect.height; // Full height
                    c.style.top = 0; // Align to top
                }
                else
                {
                    c.style.height = contentRect.height / m_VisibleItemCount;
                    c.style.width = contentRect.width; // Full width
                    c.style.left = 0; // Align to left
                }

                // Reset translate to ensure clean positioning
                c.style.translate = new Translate(0, 0);
            }
        }

        void RefreshList()
        {
            for (var i = 0; i < childCount; i++)
            {
                var item = (SwipeViewItem)ElementAt(i);
                unbindItem?.Invoke(item, i);
                item.UnregisterCallback<GeometryChangedEvent>(OnItemGeometryChanged);
            }

            Clear();

            if (m_SourceItems != null)
            {
                for (var i = 0; i < m_SourceItems.Count; i++)
                {
                    var item = new SwipeViewItem { index = i };
                    ConfigureItem(item);
                    bindItem?.Invoke(item, i);
                    Add(item);
                }
            }
            else if (m_StaticItems != null)
            {
                for (var i = 0; i < m_StaticItems.Count; i++)
                {
                    var item = new SwipeViewItem { index = i };
                    ConfigureItem(item);
                    if (m_StaticItems[i].childCount > 0)
                        item.Add(m_StaticItems[i].ElementAt(0));
                    Add(item);
                }
            }

            if (childCount > 0)
                ElementAt(0).RegisterCallback<GeometryChangedEvent>(OnItemGeometryChanged);

            // Refresh sizing and positioning for all new items
            RefreshItemsSize();
            m_AnimationDirection = 0;
            m_CurrentDragOffset = 0f;
            if (childCount > 0)
                SetValueWithoutNotify(0);
            else
                m_Value = -1;
        }

        /// <summary>
        /// Configure a SwipeViewItem with proper styling for individual positioning
        /// </summary>
        void ConfigureItem(SwipeViewItem item)
        {
            // Force absolute positioning for independent movement
            item.style.position = Position.Absolute;

            // Optimize for frequent transforms during swipe animations
            item.usageHints |= UsageHints.DynamicTransform;

            // Ensure items don't interfere with layout
            item.style.flexGrow = 0;
            item.style.flexShrink = 0;
        }

        void InvokeSwipeEvents()
        {
            if (!paddingRect.IsValid() || beingSwiped == null)
                return;

            foreach (var item in Children())
            {
                var size = direction == Direction.Horizontal ? item.localBound.width : item.localBound.height;
                var localRect = this.WorldToLocal(item.worldBound);
                var normalizedDistance = direction == Direction.Horizontal ? localRect.x / size : localRect.y / size;
                beingSwiped?.Invoke((SwipeViewItem)item, normalizedDistance);
            }
        }

        void OnItemGeometryChanged(GeometryChangedEvent evt)
        {
            // Re-position items when geometry changes, but don't refresh sizes during animations
            if (m_Animation == null || m_Animation.IsRecycled())
            {
                RefreshItemsSize();
            }
            PositionAllItems(m_CurrentDragOffset);
        }

        void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            InvokeSwipeEvents();
        }

        /// <summary>
        /// Check if there is a next item or not.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public bool canGoToNext => shouldWrap || (value + 1 < childCount && value + 1 >= 0);

        /// <summary>
        /// Check if there is a previous item or not.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public bool canGoToPrevious => shouldWrap || (value - 1 < childCount && value - 1 >= 0);

        /// <summary>
        /// Go to item at index.
        /// </summary>
        /// <param name="index"> Index of the item to go to. </param>
        /// <returns> True if the operation was successful, false otherwise. </returns>
        public bool GoTo(int index)
        {
            if (index < 0 || index >= childCount)
                return false;

            SetValue(index);
            return true;
        }

        /// <summary>
        /// Snap to item at index.
        /// </summary>
        /// <param name="index"> Index of the item to snap to. </param>
        /// <returns> True if the operation was successful, false otherwise. </returns>
        public bool SnapTo(int index)
        {
            if (index < 0 || index >= childCount)
                return false;

            m_AnimationDirection = 0;
            SetValue(index, false);
            return true;
        }

        /// <summary>
        /// Go to next item.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool GoToNext()
        {
            if (!canGoToNext)
                return false;

            var nextIndex = shouldWrap
                ? (value + 1) % count
                : Mathf.Clamp(value + 1, 0, count - visibleItemCount);

            if (nextIndex == value)
                return false;

            m_AnimationDirection = 1f;
            SetValue(nextIndex, false);
            return true;
        }

        /// <summary>
        /// Go to previous item.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool GoToPrevious()
        {
            if (!canGoToPrevious)
                return false;

            var nextIndex = shouldWrap
                ? (value - 1 + count) % count
                : Mathf.Clamp(value - 1, 0, count - visibleItemCount);

            if (nextIndex == value)
                return false;

            m_AnimationDirection = -1f;
            SetValue(nextIndex, false);
            return true;
        }



    }
}
