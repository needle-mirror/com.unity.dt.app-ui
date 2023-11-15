using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Heading sizing.
    /// </summary>
    public enum HeadingSize
    {
        /// <summary>
        /// Double Extra-small
        /// </summary>
        XXS,

        /// <summary>
        /// Extra-small
        /// </summary>
        XS,

        /// <summary>
        /// Small
        /// </summary>
        S,

        /// <summary>
        /// Medium
        /// </summary>
        M,

        /// <summary>
        /// Large
        /// </summary>
        L,

        /// <summary>
        /// Extra-large
        /// </summary>
        XL,

        /// <summary>
        /// Double Extra-large
        /// </summary>
        XXL,
    }

    /// <summary>
    /// Heading UI element.
    /// </summary>
    public sealed class Heading : LocalizedTextElement
    {
        /// <summary>
        /// The Heading main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-heading";

        /// <summary>
        /// The Heading primary variant styling class.
        /// </summary>
        public static readonly string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Heading size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        HeadingSize m_Size = HeadingSize.M;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Heading()
            : this(string.Empty) { }

        /// <summary>
        /// Construct a Heading UI element with a provided text to display.
        /// </summary>
        /// <param name="text">The text that will be displayed.</param>
        public Heading(string text)
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Position; // in case we want a tooltip

            this.text = text;
            primary = true;
            size = HeadingSize.M;
        }

        /// <summary>
        /// The primary variant of the Heading.
        /// </summary>
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set => EnableInClassList(primaryUssClassName, value);
        }

        /// <summary>
        /// The size of the Heading.
        /// </summary>
        public HeadingSize size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
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
        /// Factory class to instantiate a <see cref="Heading"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Heading, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Heading"/>.
        /// </summary>
        public new class UxmlTraits : LocalizedTextElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Primary = new UxmlBoolAttributeDescription
            {
                name = "primary",
                defaultValue = true,
            };

            readonly UxmlEnumAttributeDescription<HeadingSize> m_Size = new UxmlEnumAttributeDescription<HeadingSize>
            {
                name = "size",
                defaultValue = HeadingSize.M,
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

                var element = (Heading)ve;
                element.primary = m_Primary.GetValueFromBag(bag, cc);
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
