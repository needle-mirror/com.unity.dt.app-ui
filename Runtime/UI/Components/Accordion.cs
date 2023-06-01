using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Item used inside an <see cref="Accordion"/> element.
    /// </summary>
    public class AccordionItem : VisualElement, INotifyValueChanged<bool>
    {
        /// <summary>
        /// The AccordionItem main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-accordionitem";

        /// <summary>
        /// The AccordionItem content styling class.
        /// </summary>
        public static readonly string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The AccordionItem header styling class.
        /// </summary>
        public static readonly string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The AccordionItem headertext styling class.
        /// </summary>
        public static readonly string headerTextUssClassName = ussClassName + "__headertext";

        /// <summary>
        /// The AccordionItem indicator styling class.
        /// </summary>
        public static readonly string indicatorUssClassName = ussClassName + "__indicator";

        /// <summary>
        /// The AccordionItem heading styling class.
        /// </summary>
        public static readonly string headingUssClassName = ussClassName + "__heading";

        readonly VisualElement m_ContentElement;

        readonly LocalizedTextElement m_HeaderTextElement;

        readonly Pressable m_Clickable;

        readonly ExVisualElement m_HeaderElement;

        ValueAnimation<float> m_Anim;

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

            var headerIndicatorElement = new Icon { name = indicatorUssClassName, iconName = "caret-down", pickingMode = PickingMode.Ignore };
            headerIndicatorElement.AddToClassList(indicatorUssClassName);

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
            m_HeaderElement.hierarchy.Add(m_HeaderTextElement);
            m_HeaderElement.hierarchy.Add(headerIndicatorElement);

            var headingElement = new VisualElement { pickingMode = PickingMode.Ignore };
            headingElement.AddToClassList(headingUssClassName);
            headingElement.hierarchy.Add(m_HeaderElement);

            m_ContentElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };
            m_ContentElement.AddToClassList(contentUssClassName);

            hierarchy.Add(headingElement);
            hierarchy.Add(m_ContentElement);

            SetValueWithoutNotify(false);
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
        /// The title of the AccordionItem.
        /// </summary>
        public string title
        {
            get => m_HeaderTextElement.text;
            set => m_HeaderTextElement.text = value;
        }

        /// <summary>
        /// The value of the item, which represents its open state.
        /// </summary>
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
            }
        }

        /// <summary>
        /// Set the open state of the item without triggering any event.
        /// </summary>
        /// <param name="newValue">The new open state of the item.</param>
        public void SetValueWithoutNotify(bool newValue)
        {
            m_Anim?.Stop();
            m_Anim?.Recycle();
            m_Anim = null;
            
            if (newValue)
            {
                m_ContentElement.style.height = new StyleLength(StyleKeyword.Auto);
                m_ContentElement.style.maxHeight = 500;
                m_ContentElement.style.visibility = Visibility.Hidden;
                m_ContentElement.style.position = Position.Absolute;

                m_ContentElement.schedule.Execute(() =>
                {
                    var height = Mathf.Min(m_ContentElement.resolvedStyle.height, 500);
                    m_ContentElement.style.height = 0;
                    m_ContentElement.style.maxHeight = new StyleLength(StyleKeyword.Null);
                    m_ContentElement.style.visibility = new StyleEnum<Visibility>(StyleKeyword.Null);
                    m_ContentElement.style.position = new StyleEnum<Position>(StyleKeyword.Null);

                    m_Anim = m_ContentElement.experimental.animation.Start(0, height, 125, (element, f) =>
                    {
                        element.style.height = f;
                    }).Ease(Easing.OutQuad).KeepAlive();
                }).ExecuteLater(1L);
            }
            else
            {
                var startHeight = m_ContentElement.resolvedStyle.height;
                startHeight = float.IsNaN(startHeight) ? 0 : startHeight;
                m_Anim = m_ContentElement.experimental.animation.Start(startHeight, 0, 125, (element, f) =>
                {
                    element.style.height = f;
                }).Ease(Easing.OutQuad).KeepAlive();
            }
            EnableInClassList(Styles.openUssClassName, newValue);
        }

        void OnClicked()
        {
            value = !value;
        }

        /// <summary>
        /// Class to be able to use the <see cref="AccordionItem"/> in UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<AccordionItem, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="AccordionItem"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_Title = new UxmlStringAttributeDescription
            {
                name = "title",
                defaultValue = "Header",
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

                var element = (AccordionItem)ve;
                element.title = m_Title.GetValueFromBag(bag, cc);
                element.SetEnabled(!m_Disabled.GetValueFromBag(bag, cc));
            }
        }
    }

    /// <summary>
    /// Accordion UI element.
    /// </summary>
    public class Accordion : VisualElement
    {
        /// <summary>
        /// The Accordion main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-accordion";

        /// <summary>
        /// The behavior of the Accordion when multiple items are open.
        /// <para>
        /// If true, a maximum of one item can be open at a time.
        /// </para>
        /// </summary>
        public bool isExclusive { get; set; } = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Accordion()
        {
            AddToClassList(ussClassName);

            RegisterCallback<AccordionItemValueChangedEvent>(OnAccordionItemValueChanged);

            pickingMode = PickingMode.Ignore;
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

        /// <summary>
        /// The UXML factory for the Accordion.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Accordion, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Accordion"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            /// <summary>
            /// The behavior of the Accordion when multiple items are open.
            /// <para>
            /// If true, a maximum of one item can be open at a time.
            /// </para>
            /// </summary>
            readonly UxmlBoolAttributeDescription m_IsExclusive = new UxmlBoolAttributeDescription
            {
                name = "is-exclusive",
                defaultValue = false
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

                var element = (Accordion)ve;
                element.isExclusive = m_IsExclusive.GetValueFromBag(bag, cc);
            }
        }
    }
}
