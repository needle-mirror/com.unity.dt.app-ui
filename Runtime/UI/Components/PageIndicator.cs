using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A component that displays a series of dots to indicate and navigate between multiple pages or slides.
    /// </summary>
    /// <remarks>
    /// The PageIndicator is a UI component that displays a sequence of interactive dots, commonly used in
    /// carousels, slideshows, or any content that spans multiple pages. Each dot represents a page, with the
    /// current page highlighted.
    ///
    /// The component supports both horizontal and vertical layouts, keyboard navigation, and can be controlled
    /// programmatically or through user interaction.
    ///
    /// Note: The PageIndicator is designed to be used as a navigation aid and should be combined with the
    /// actual content container that displays the pages.
    /// </remarks>
    /// <example>
    /// <para>Basic Usage. Creating a basic horizontal page indicator with three dots and handling page changes.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <PageIndicator name="pageIndicator" count="3" direction="Horizontal" />
    /// </UXML>
    ///
    /// // C#
    /// pageIndicator.RegisterCallback<ChangeEvent<int>>((evt) => {
    ///     // Handle page change
    ///     var newPageIndex = evt.newValue;
    ///     UpdatePageContent(newPageIndex);
    /// });
    /// ]]></code>
    /// <para>Image Carousel Integration. Implementing an image carousel with PageIndicator for navigation.</para>
    /// <code lang="csharp"><![CDATA[
    /// public class ImageCarousel : VisualElement
    /// {
    ///     private PageIndicator m_PageIndicator;
    ///     private VisualElement m_ImageContainer;
    ///
    ///     public ImageCarousel()
    ///     {
    ///         // Setup image container
    ///         m_ImageContainer = new VisualElement();
    ///         Add(m_ImageContainer);
    ///
    ///         // Setup page indicator
    ///         m_PageIndicator = new PageIndicator();
    ///         m_PageIndicator.count = 5; // For 5 images
    ///         m_PageIndicator.RegisterCallback<ChangeEvent<int>>((evt) => {
    ///             ShowImage(evt.newValue);
    ///         });
    ///         Add(m_PageIndicator);
    ///     }
    ///
    ///     private void ShowImage(int index)
    ///     {
    ///         // Update image display logic
    ///     }
    /// }
    /// ]]></code>
    /// <para>Keyboard Navigation. Demonstrating keyboard navigation and programmatic page changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// // The PageIndicator supports keyboard navigation out of the box:
    /// // - Left/Right arrows for horizontal layout
    /// // - Up/Down arrows for vertical layout
    ///
    /// // You can also programmatically navigate:
    /// pageIndicator.GoToNext(); // Move to next page
    /// pageIndicator.GoToPrevious(); // Move to previous page
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class PageIndicator : BaseVisualElement, INotifyValueChanged<int>
    {

        internal static readonly BindingId directionProperty = nameof(direction);

        internal static readonly BindingId countProperty = nameof(count);

        internal static readonly BindingId valueProperty = nameof(value);


        /// <summary>
        /// The PageIndicator main styling class.
        /// </summary>
        public const string ussClassName = "appui-page-indicator";

        /// <summary>
        /// The PageIndicator direction styling class.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The PageIndicator dot styling class.
        /// </summary>
        public const string dotUssClassName = ussClassName + "__dot";

        /// <summary>
        /// The PageIndicator dot background styling class.
        /// </summary>
        public const string dotBackgroundUssClassName = ussClassName + "__dot-background";

        /// <summary>
        /// The PageIndicator dot content styling class.
        /// </summary>
        public const string dotContentUssClassName = ussClassName + "__dot-content";

        Direction m_Direction;

        int m_Value;

        readonly List<Pressable> m_Clickables = new List<Pressable>();

        readonly List<KeyboardFocusController> m_KeyboardFocusControllers = new List<KeyboardFocusController>();

        /// <summary>
        /// The number of dots.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int count
        {
            get => hierarchy.childCount;
            set
            {
                if (value == hierarchy.childCount)
                    return;

                BuildDots(value);

                NotifyPropertyChanged(in countProperty);
            }
        }

        /// <summary>
        /// The currently selected dot index.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int value
        {
            get => m_Value;

            set
            {
                if (value < 0 || value > hierarchy.childCount - 1)
                    return;

                var previousValue = m_Value;
                SetValueWithoutNotify(value);
                if (previousValue != m_Value)
                {
                    using var evt = ChangeEvent<int>.GetPooled(previousValue, m_Value);
                    evt.target = this;
                    SendEvent(evt);
                }

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The PageIndicator direction (horizontal or vertical).
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

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageIndicator()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;
            direction = Direction.Horizontal;

            RegisterCallback<KeyDownEvent>(OnDotKeyDown);
        }

        void OnDotKeyDown(KeyDownEvent evt)
        {
            var handled = false;

            if (evt.target is VisualElement dot && dot.hierarchy.parent == this)
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

        void OnDotClicked(EventBase evt)
        {
            if (evt.target is VisualElement dot && dot.hierarchy.parent == this)
            {
                value = hierarchy.IndexOf(dot);

                evt.StopPropagation();
            }
        }

        /// <summary>
        /// The content container of the PageIndicator. This is always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Set the value of the PageIndicator without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            if (m_Value >= 0 && m_Value < hierarchy.childCount)
                hierarchy.ElementAt(m_Value).RemoveFromClassList(Styles.selectedUssClassName);
            m_Value = newValue;
            if (m_Value >= 0 && m_Value < hierarchy.childCount)
                hierarchy.ElementAt(m_Value).AddToClassList(Styles.selectedUssClassName);
        }

        /// <summary>
        /// Go to the next dot.
        /// </summary>
        /// <returns> True if the dot has been changed. </returns>
        public bool GoToNext()
        {
            var nextIndex = Mathf.Min(Mathf.Max(0, m_Value + 1), hierarchy.childCount - 1);
            if (nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        /// <summary>
        /// Go to the previous dot.
        /// </summary>
        /// <returns> True if the dot has been changed. </returns>
        public bool GoToPrevious()
        {
            var nextIndex = Mathf.Min(Mathf.Max(0, m_Value - 1), hierarchy.childCount - 1);
            if (nextIndex == m_Value)
                return false;
            value = nextIndex;
            return true;
        }

        void BuildDots(int dotCount)
        {
            for (var i = hierarchy.childCount - 1; i >= 0; i--)
            {
                var dot = hierarchy.ElementAt(i);
                dot.RemoveManipulator(m_Clickables[i]);
                dot.RemoveManipulator(m_KeyboardFocusControllers[i]);
                hierarchy.Remove(dot);
                m_Clickables.RemoveAt(i);
            }
            hierarchy.Clear();
            m_Clickables.Clear();
            for (var i = 0; i < dotCount; i++)
            {
                var dot = new ExVisualElement { name = $"dot-{i}", focusable = true, tabIndex = 0, pickingMode = PickingMode.Position, passMask = 0 };
                dot.AddToClassList(dotUssClassName);
                var clickable = new Pressable(OnDotClicked);
                var keyboardFocus = new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn);
                m_Clickables.Add(clickable);
                m_KeyboardFocusControllers.Add(keyboardFocus);
                dot.AddManipulator(clickable);
                dot.AddManipulator(keyboardFocus);
                var dotBg = new VisualElement { name = dotBackgroundUssClassName, pickingMode = PickingMode.Ignore };
                dotBg.AddToClassList(dotBackgroundUssClassName);
                dot.Add(dotBg);
                var dotContent = new VisualElement { name = dotContentUssClassName, pickingMode = PickingMode.Ignore };
                dotContent.AddToClassList(dotContentUssClassName);
                dot.Add(dotContent);
                hierarchy.Add(dot);
            }
            SetValueWithoutNotify(value);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            ((ExVisualElement)evt.target).passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            ((ExVisualElement)evt.target).passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

    }
}
