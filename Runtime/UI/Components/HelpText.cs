using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The variant of the <see cref="HelpText"/>.
    /// </summary>
    public enum HelpTextVariant
    {
        /// <summary>
        /// The default variant.
        /// </summary>
        Default,
        
        /// <summary>
        /// The warning variant.
        /// </summary>
        Warning,
        
        /// <summary>
        /// The destructive variant.
        /// </summary>
        Destructive,
    }
    
    /// <summary>
    /// A help text.
    /// </summary>
    public class HelpText : LocalizedTextElement
    {
        /// <summary>
        /// The HelpText main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-help-text";
        
        /// <summary>
        /// The HelpText variant styling class.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        HelpTextVariant m_Variant;

        /// <summary>
        /// The variant of the <see cref="HelpText"/>.
        /// </summary>
        public HelpTextVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(variantUssClassName + m_Variant.ToString().ToLower());
                m_Variant = value;
                AddToClassList(variantUssClassName + m_Variant.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HelpText()
            : this(null) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The message to display. </param>
        public HelpText(string text)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);
            
            this.text = text;
            variant = HelpTextVariant.Default;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="HelpText"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<HelpText, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="HelpText"/>.
        /// </summary>
        public new class UxmlTraits : LocalizedTextElement.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<HelpTextVariant> m_Variant = new UxmlEnumAttributeDescription<HelpTextVariant>
            {
                name = "variant",
                defaultValue = HelpTextVariant.Default,
            };
            
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var helpText = (HelpText)ve;

                var variant = HelpTextVariant.Default;
                if (m_Variant.TryGetValueFromBag(bag, cc, ref variant))
                    helpText.variant = variant;
                
                var disabled = false;
                if (m_Disabled.TryGetValueFromBag(bag, cc, ref disabled))
                    helpText.SetEnabled(!disabled);
            }
        }
    }
}
