using System;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The position of an element inside a Flex container.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum FlexPosition
    {
        /// <summary>
        /// The element is at the start of the container.
        /// </summary>
        /// <remarks>
        /// In a row with Left-to-Right layout, this is the left side.
        /// In a row with Right-to-Left layout, this is the right side.
        /// For a column, this is the top side.
        /// </remarks>
        Start,

        /// <summary>
        /// The element is at the end of the container.
        /// </summary>
        /// <remarks>
        /// In a row with Left-to-Right layout, this is the right side.
        /// In a row with Right-to-Left layout, this is the left side.
        /// For a column, this is the bottom side.
        /// </remarks>
        End
    }

    /// <summary>
    /// Item used inside an <see cref="Accordion"/> element.
    /// </summary>
    [UxmlElement]
    public partial class AccordionItem : BaseVisualElement, INotifyValueChanged<bool>
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId indicatorPositionProperty = nameof(indicatorPosition);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId trailingContentTemplateProperty = nameof(trailingContentTemplate);

        internal static readonly BindingId leadingContentTemplateProperty = nameof(leadingContentTemplate);

        const string k_IndicatorIconName = "caret-down";

        /// <summary>
        /// The AccordionItem main styling class.
        /// </summary>
        public const string ussClassName = "appui-accordionitem";

        /// <summary>
        /// The AccordionItem content parent styling class.
        /// </summary>
        public const string contentParentUssClassName = ussClassName + "__content-parent";

        /// <summary>
        /// The AccordionItem content styling class.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The AccordionItem header styling class.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The AccordionItem headertext styling class.
        /// </summary>
        public const string headerTextUssClassName = ussClassName + "__headertext";

        /// <summary>
        /// The AccordionItem leading container styling class.
        /// </summary>
        public const string leadingContainerUssClassName = ussClassName + "__leading-container";

        /// <summary>
        /// The AccordionItem trailing container styling class.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailing-container";

        /// <summary>
        /// The AccordionItem indicator styling class.
        /// </summary>
        public const string indicatorUssClassName = ussClassName + "__indicator";

        /// <summary>
        /// The AccordionItem heading styling class.
        /// </summary>
        public const string headingUssClassName = ussClassName + "__heading";

        /// <summary>
        /// The AccordionItem indicator position styling class.
        /// </summary>
        [EnumName("GetIndicatorPosUssClassName", typeof(FlexPosition))]
        public const string indicatorPosUssClassName = ussClassName + "--indicator-";

        readonly VisualElement m_ContentElement;

        readonly VisualElement m_ContentParentElement;

        readonly LocalizedTextElement m_HeaderTextElement;

        readonly VisualElement m_HeaderIndicatorElement;

        readonly Pressable m_Clickable;

        readonly ExVisualElement m_HeaderElement;

        VisualTreeAsset m_TrailingContentTemplate;

        VisualTreeAsset m_LeadingContentTemplate;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AccordionItem()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_HeaderTextElement = new LocalizedTextElement { name = headerTextUssClassName, pickingMode = PickingMode.Ignore };
            m_HeaderTextElement.AddToClassList(headerTextUssClassName);

            leadingContainer = new VisualElement { name = leadingContainerUssClassName, pickingMode = PickingMode.Ignore };
            leadingContainer.AddToClassList(leadingContainerUssClassName);

            trailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            trailingContainer.AddToClassList(trailingContainerUssClassName);

            m_HeaderIndicatorElement = new Icon { name = indicatorUssClassName, iconName = k_IndicatorIconName, pickingMode = PickingMode.Ignore };
            m_HeaderIndicatorElement.AddToClassList(indicatorUssClassName);

            m_HeaderElement = new ExVisualElement
            {
                name = headerUssClassName,
                pickingMode = PickingMode.Position,
                focusable = true,
                passMask = 0,
            };
            m_HeaderElement.AddToClassList(headerUssClassName);
            m_Clickable = new Pressable(OnClicked);
            m_HeaderElement.AddManipulator(m_Clickable);
            m_HeaderElement.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnFocus));
            m_HeaderElement.hierarchy.Add(leadingContainer);
            m_HeaderElement.hierarchy.Add(m_HeaderTextElement);
            m_HeaderElement.hierarchy.Add(trailingContainer);
            m_HeaderElement.hierarchy.Add(m_HeaderIndicatorElement);

            var headingElement = new VisualElement { pickingMode = PickingMode.Ignore };
            headingElement.AddToClassList(headingUssClassName);
            headingElement.hierarchy.Add(m_HeaderElement);

            m_ContentParentElement = new VisualElement
            {
                name = contentParentUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_ContentParentElement.AddToClassList(contentParentUssClassName);

            m_ContentElement = new VisualElement
            {
                name = contentUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_ContentElement.AddToClassList(contentUssClassName);
            m_ContentParentElement.hierarchy.Add(m_ContentElement);
            m_ContentElement.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);

            hierarchy.Add(headingElement);
            hierarchy.Add(m_ContentParentElement);

            AddToClassList(GetIndicatorPosUssClassName(FlexPosition.End));
            SetValueWithoutNotify(false);

            title = "Header";
            indicatorPosition = FlexPosition.End;
            leadingContentTemplate = null;
            trailingContentTemplate = null;
        }

        void OnContentGeometryChanged(GeometryChangedEvent evt)
        {
            if (value)
            {
                if (float.IsNaN(evt.newRect.height) || Mathf.Approximately(evt.newRect.height, m_ContentParentElement.resolvedStyle.height))
                    return;
                m_ContentParentElement.style.height = evt.newRect.height;
            }
        }

        void OnFocus(FocusInEvent evt)
        {
            m_HeaderElement.passMask = 0;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            m_HeaderElement.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

        /// <summary>
        /// The content container of the AccordionItem.
        /// </summary>
        public override VisualElement contentContainer => m_ContentElement;

        /// <summary>
        /// The header's trailing container of the AccordionItem.
        /// </summary>
        public VisualElement trailingContainer { get; }

        /// <summary>
        /// The header's leading container of the AccordionItem.
        /// </summary>
        public VisualElement leadingContainer { get; }

        /// <summary>
        /// The header's leading container template of the AccordionItem.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public VisualTreeAsset leadingContentTemplate
        {
            get => m_LeadingContentTemplate;
            set
            {
                var changed = m_LeadingContentTemplate != value;
                m_LeadingContentTemplate = value;
                leadingContainer.Clear();
                if (m_LeadingContentTemplate)
                    m_LeadingContentTemplate.CloneTree(leadingContainer);
                if (changed)
                    NotifyPropertyChanged(in leadingContentTemplateProperty);
            }
        }

        /// <summary>
        /// The header's trailing container template of the AccordionItem.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public VisualTreeAsset trailingContentTemplate
        {
            get => m_TrailingContentTemplate;
            set
            {
                var changed = m_TrailingContentTemplate != value;
                m_TrailingContentTemplate = value;
                trailingContainer.Clear();
                if (m_TrailingContentTemplate)
                    m_TrailingContentTemplate.CloneTree(trailingContainer);
                if (changed)
                    NotifyPropertyChanged(in trailingContentTemplateProperty);
            }
        }

        /// <summary>
        /// The position of the indicator.
        /// </summary>
        [Tooltip("The position of the indicator.")]
        [CreateProperty]
        [UxmlAttribute]
        public FlexPosition indicatorPosition
        {
            get => m_HeaderElement.hierarchy.IndexOf(m_HeaderIndicatorElement) == 0 ? FlexPosition.Start : FlexPosition.End;
            set
            {
                var previousValue = indicatorPosition;
                if (previousValue == value)
                    return;

                m_HeaderIndicatorElement.RemoveFromHierarchy();
                if (value == FlexPosition.Start)
                    m_HeaderElement.hierarchy.Insert(0, m_HeaderIndicatorElement);
                else
                    m_HeaderElement.hierarchy.Add(m_HeaderIndicatorElement);
                RemoveFromClassList(GetIndicatorPosUssClassName(previousValue));
                AddToClassList(GetIndicatorPosUssClassName(value));

                NotifyPropertyChanged(indicatorPositionProperty);
            }
        }

        /// <summary>
        /// The title of the AccordionItem.
        /// </summary>
        [Tooltip("The title of the AccordionItem.")]
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_HeaderTextElement.text;
            set
            {
                var previousValue = m_HeaderTextElement.text;

                m_HeaderTextElement.text = value;

                if (previousValue != value)
                    NotifyPropertyChanged(titleProperty);
            }
        }

        /// <summary>
        /// The value of the item, which represents its open state.
        /// </summary>
        [Tooltip("The value of the item, which represents its open state.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool value
        {
            get => ClassListContains(Styles.openUssClassName);
            set
            {
                var previousValue = ClassListContains(Styles.openUssClassName);
                if (previousValue == value)
                    return;
                using var evt = ChangeEvent<bool>.GetPooled(previousValue, value);
                using var itemEvt = AccordionItemValueChangedEvent.GetPooled();
                itemEvt.target = this;
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                SendEvent(itemEvt);

                NotifyPropertyChanged(valueProperty);
            }
        }

        /// <summary>
        /// Set the open state of the item without triggering any event.
        /// </summary>
        /// <param name="newValue">The new open state of the item.</param>
        public void SetValueWithoutNotify(bool newValue)
        {
            if (newValue)
            {
                m_ContentParentElement.style.height = m_ContentElement.resolvedStyle.height;
            }
            else
            {
                m_ContentParentElement.style.height = 0;
            }
            EnableInClassList(Styles.openUssClassName, newValue);
        }

        void OnClicked()
        {
            value = !value;
        }

    }

    /// <summary>
    /// A collapsible content container that allows users to expand and collapse sections of content.
    /// </summary>
    /// <remarks>
    /// The Accordion component is a versatile UI element that organizes content into collapsible sections. Each
    /// section consists of a header and content area that can be toggled open or closed. This pattern is
    /// particularly useful when you need to present multiple sections of content in a limited space.
    ///
    /// The component supports both single-expansion (exclusive) and multi-expansion modes, making it suitable for
    /// various use cases such as FAQs, settings panels, or navigation menus.
    ///
    /// Note: The Accordion component requires <see cref="AccordionItem"/> children to function properly. Each
    /// AccordionItem represents a collapsible section within the Accordion.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Accordion : BaseVisualElement
    {
        internal static readonly BindingId isExclusiveProperty = nameof(isExclusive);

        /// <summary>
        /// The Accordion main styling class.
        /// </summary>
        public const string ussClassName = "appui-accordion";

        bool m_IsExclusive;

        /// <summary>
        /// The behavior of the Accordion when multiple items are open.
        /// </summary>
        /// <remarks>
        /// If true, a maximum of one item can be open at a time.
        /// </remarks>
        [Tooltip("If true, a maximum of one item can be open at a time.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Accordion")]
        public bool isExclusive
        {
            get => m_IsExclusive;
            set
            {
                var previousValue = m_IsExclusive;
                m_IsExclusive = value;

                if (previousValue != value)
                    NotifyPropertyChanged(isExclusiveProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Accordion()
        {
            AddToClassList(ussClassName);

            RegisterCallback<AccordionItemValueChangedEvent>(OnAccordionItemValueChanged);

            pickingMode = PickingMode.Ignore;

            isExclusive = false;
        }

        void OnAccordionItemValueChanged(AccordionItemValueChangedEvent evt)
        {
            if (evt.target is AccordionItem item && item.parent == this)
            {
                if (isExclusive)
                {
                    foreach (var child in Children())
                    {
                        if (child != item && child is AccordionItem accordionItem)
                        {
                            accordionItem.SetValueWithoutNotify(false);
                        }
                    }
                }
                evt.StopPropagation();
            }
        }

    }
}
