using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Quote UI element.
    /// </summary>
    public class Quote : VisualElement
    {
        /// <summary>
        /// The Quote main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-quote";

        /// <summary>
        /// The Quote container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";

        readonly VisualElement m_Container;

        Color? m_InlineColor;

        /// <summary>
        /// The content container of the Quote.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The Quote outline color.
        /// </summary>
        public Color? color
        {
            get => m_InlineColor;
            set
            {
                m_InlineColor = value;
                style.borderLeftColor = m_InlineColor ?? new StyleColor(StyleKeyword.Null);
            }
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Quote()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            color = null;
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
        /// Defines the UxmlFactory for the Quote.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Quote, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Quote"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription
            {
                name = "color",
                defaultValue = Color.gray
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (Quote)ve;
                
                var color = Color.gray;
                if (m_Color.TryGetValueFromBag(bag, cc, ref color))
                    element.color = color;
                
                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
