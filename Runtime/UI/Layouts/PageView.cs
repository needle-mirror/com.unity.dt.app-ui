using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container component that displays a single child at a time with built-in navigation.
    /// </summary>
    /// <remarks>
    /// The PageView component is a container that displays one child element at a time and provides smooth
    /// navigation between them. It combines a <see cref="SwipeView"/> for touch/mouse interaction with a
    /// <see cref="PageIndicator"/> for visual feedback and navigation control. It is similar to a
    /// <see cref="ScrollView"/>, but here children are snapped to the container's edges.
    ///
    /// It's ideal for creating carousels, slideshows, onboarding flows, or any interface where content needs to
    /// be displayed one page at a time.
    ///
    /// Key features:
    /// - Supports both horizontal and vertical orientations
    /// - Touch and mouse swipe gestures
    /// - Keyboard navigation
    /// - Auto-play capability
    /// - Configurable animation speed and behavior
    /// - Optional wrapping navigation
    /// - Built-in page indicator
    /// </remarks>
    /// <example>
    /// <para>**Basic Usage** — Creating a simple horizontal page view with three pages.</para>
    /// <code lang="xml"><![CDATA[
    /// <PageView style="width: 100%; height: 300px;">
    ///     <SwipeViewItem>
    ///         <Label text="Page 1" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Label text="Page 2" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Label text="Page 3" />
    ///     </SwipeViewItem>
    /// </PageView>
    /// ]]></code>
    /// <para>**Image Carousel** — Creating an auto-playing image carousel with wrap-around navigation.</para>
    /// <code lang="xml"><![CDATA[
    /// <PageView class="carousel" auto-play-duration="5000" wrap="true">
    ///     <SwipeViewItem>
    ///         <Image src="image1.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Image src="image2.png" />
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Image src="image3.png" />
    ///     </SwipeViewItem>
    /// </PageView>
    /// ]]></code>
    /// <para>**Onboarding Flow** — Creating an onboarding flow with multiple pages.</para>
    /// <code lang="xml"><![CDATA[
    /// <PageView direction="Horizontal" class="onboarding">
    ///     <SwipeViewItem>
    ///         <Box class="onboarding-page">
    ///             <Image src="welcome.png" />
    ///             <Label text="Welcome!" class="title" />
    ///             <Label text="Swipe to learn more" class="subtitle" />
    ///         </Box>
    ///     </SwipeViewItem>
    ///     <SwipeViewItem>
    ///         <Box class="onboarding-page">
    ///             <Image src="feature1.png" />
    ///             <Label text="Feature 1" class="title" />
    ///             <Label text="Description of feature 1" class="subtitle" />
    ///         </Box>
    ///     </SwipeViewItem>
    /// </PageView>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class PageView : BaseVisualElement
    {

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));

        internal static readonly BindingId animationSpeedProperty = new BindingId(nameof(snapAnimationSpeed));

        internal static readonly BindingId skipAnimationThresholdProperty = new BindingId(nameof(skipAnimationThreshold));

        internal static readonly BindingId wrapProperty = new BindingId(nameof(wrap));

        internal static readonly BindingId visibilityCountProperty = new BindingId(nameof(visibilityCount));

        internal static readonly BindingId autoPlayDurationProperty = new BindingId(nameof(autoPlayDuration));

        /// <summary>
        /// The main styling class of the PageView. This is the class that is used in the USS file.
        /// </summary>
        public const string ussClassName = "appui-pageview";

        /// <summary>
        /// The styling class applied to the SwipeView.
        /// </summary>
        public const string swipeViewUssClassName = ussClassName + "__swipeview";

        /// <summary>
        /// The styling class applied to the PageIndicator.
        /// </summary>
        public const string pageIndicatorUssClassName = ussClassName + "__page-indicator";

        /// <summary>
        /// The styling class applied to the PageView depending on its direction.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string variantUssClassName = ussClassName + "--";

        readonly SwipeView m_SwipeView;

        readonly PageIndicator m_PageIndicator;

        Dir m_CurrentDirection;

        /// <summary>
        /// The content container of the PageView.
        /// </summary>
        public override VisualElement contentContainer => m_SwipeView.contentContainer;

        /// <summary>
        /// The speed of the animation when snapping to a page.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float snapAnimationSpeed
        {
            get => m_SwipeView.snapAnimationSpeed;
            set
            {
                var changed = !Mathf.Approximately(m_SwipeView.snapAnimationSpeed, value);
                m_SwipeView.snapAnimationSpeed = value;

                if (changed)
                    NotifyPropertyChanged(in animationSpeedProperty);
            }
        }

        /// <summary>
        /// A limit number of pages to keep animating the transition between pages.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int skipAnimationThreshold
        {
            get => m_SwipeView.skipAnimationThreshold;
            set
            {
                var changed = m_SwipeView.skipAnimationThreshold != value;
                m_SwipeView.skipAnimationThreshold = value;

                if (changed)
                    NotifyPropertyChanged(in skipAnimationThresholdProperty);
            }
        }

        /// <summary>
        /// Whether the PageView should wrap around when reaching the end of the list.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool wrap
        {
            get => m_SwipeView.wrap;
            set
            {
                var changed = m_SwipeView.wrap != value;
                m_SwipeView.wrap = value;

                if (changed)
                    NotifyPropertyChanged(in wrapProperty);
            }
        }

        /// <summary>
        /// The number of milliseconds between each automatic swipe.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int autoPlayDuration
        {
            get => m_SwipeView.autoPlayDuration;
            set
            {
                var changed = m_SwipeView.autoPlayDuration != value;
                m_SwipeView.autoPlayDuration = value;

                if (changed)
                    NotifyPropertyChanged(in autoPlayDurationProperty);
            }
        }

        /// <summary>
        /// The orientation of the PageView.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_SwipeView.direction;
            set
            {
                var changed = m_SwipeView.direction != value;
                RemoveFromClassList(GetDirectionUssClassName(m_SwipeView.direction));
                m_SwipeView.direction = value;
                m_PageIndicator.direction = value;
                AddToClassList(GetDirectionUssClassName(m_SwipeView.direction));

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The number of pages that are visible at the same time.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int visibilityCount
        {
            get => m_SwipeView.visibleItemCount;
            set
            {
                var changed = m_SwipeView.visibleItemCount != value;
                m_SwipeView.visibleItemCount = value;

                if (changed)
                    NotifyPropertyChanged(in visibilityCountProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageView()
        {
            AddToClassList(ussClassName);

            m_SwipeView = new SwipeView { name = swipeViewUssClassName };
            m_SwipeView.AddToClassList(swipeViewUssClassName);

            m_PageIndicator = new PageIndicator { name = pageIndicatorUssClassName };
            m_PageIndicator.AddToClassList(pageIndicatorUssClassName);

            hierarchy.Add(m_SwipeView);
            hierarchy.Add(m_PageIndicator);

            m_SwipeView.RegisterValueChangedCallback(OnSwipeValueChanged);
            m_PageIndicator.RegisterValueChangedCallback(OnPageIndicatorValueChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            direction = Direction.Horizontal;
            snapAnimationSpeed = 0.5f;
            skipAnimationThreshold = 2;
            wrap = false;
            visibilityCount = 1;
            autoPlayDuration = SwipeView.noAutoPlayDuration;

            this.RegisterContextChangedCallback<DirContext>(OnDirContextChanged);
        }

        void OnDirContextChanged(ContextChangedEvent<DirContext> evt)
        {
            m_CurrentDirection = evt.context?.dir ?? Dir.Ltr;
            m_PageIndicator.count = m_SwipeView.childCount;
            var newValue = m_PageIndicator.count > 0 ? 0 : -1;
            m_SwipeView.value = newValue;
        }

        void OnPageIndicatorValueChanged(ChangeEvent<int> evt)
        {
            m_SwipeView.SetValueWithoutNotify(evt.newValue);
        }

        void OnSwipeValueChanged(ChangeEvent<int> evt)
        {
            m_PageIndicator.count = m_SwipeView.childCount;
            m_PageIndicator.SetValueWithoutNotify(evt.newValue);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            m_PageIndicator.count = m_SwipeView.childCount;
            m_PageIndicator.SetValueWithoutNotify(m_SwipeView.value);
            m_SwipeView.SetValueWithoutNotify(m_SwipeView.value);
        }

    }
}
