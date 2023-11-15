using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The type of indicator to display when a field is required.
    /// </summary>
    public enum IndicatorType
    {
        /// <summary>
        /// No indicator.
        /// </summary>
        None,
        
        /// <summary>
        /// An asterisk.
        /// </summary>
        Asterisk,
        
        /// <summary>
        /// A localized "Required" text.
        /// </summary>
        Text,
    }
    
    /// <summary>
    /// A label for a field.
    /// </summary>
    public class FieldLabel : VisualElement
    {
        /// <summary>
        /// The FieldLabel main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-field-label";
        
        /// <summary>
        /// The FieldLabel required variant styling class.
        /// </summary>
        public static readonly string requiredUssClassName = ussClassName + "--required";
        
        /// <summary>
        /// The FieldLabel label styling class.
        /// </summary>
        public static readonly string labelUssClassName = ussClassName + "__label";
        
        /// <summary>
        /// The FieldLabel required label styling class.
        /// </summary>
        public static readonly string requiredLabelUssClassName = ussClassName + "__required-label";
        
        /// <summary>
        /// The FieldLabel label overflow variant styling class.
        /// </summary>
        public static readonly string labelOverflowUssClassName = ussClassName + "--label-overflow-";
        
        readonly LocalizedTextElement m_LabelElement;
        
        readonly LocalizedTextElement m_RequiredLabelElement;

        IndicatorType m_IndicatorType = IndicatorType.Asterisk;

        string m_RequiredText;

        TextOverflow m_LabelOverflow;

        /// <summary>
        /// Whether the field is required.
        /// </summary>
        public bool required
        {
            get => ClassListContains(requiredUssClassName);
            set
            {
                EnableInClassList(requiredUssClassName, value);
                m_RequiredLabelElement.text = m_IndicatorType switch
                {
                    IndicatorType.Asterisk => "*",
                    IndicatorType.Text => m_RequiredText,
                    _ => null
                };
            }
        }

        /// <summary>
        /// The type of indicator to display when a field is required.
        /// </summary>
        public IndicatorType indicatorType
        {
            get => m_IndicatorType;
            set
            {
                RemoveFromClassList(ussClassName + "--" + m_IndicatorType.ToString().ToLower());
                m_IndicatorType = value;
                AddToClassList(ussClassName + "--" + m_IndicatorType.ToString().ToLower());
            }
        }

        /// <summary>
        /// The text to display next to the label when the field is required and the indicator type is <see cref="IndicatorType.Text"/>.
        /// </summary>
        public string requiredText
        {
            get => m_RequiredText;
            set
            {
                m_RequiredText = value;
                if (m_IndicatorType == IndicatorType.Text)
                    m_RequiredLabelElement.text = m_RequiredText;
            }
        }

        /// <summary>
        /// The text to display in the label.
        /// </summary>
        public string label
        {
            get => m_LabelElement.text;
            set => m_LabelElement.text = value;
        }
        
        /// <summary>
        /// The text overflow mode.
        /// </summary>
        public TextOverflow labelOverflow
        {
            get => m_LabelOverflow;
            set
            {
                RemoveFromClassList(labelOverflowUssClassName + m_LabelOverflow.ToString().ToLower());
                m_LabelOverflow = value;
                AddToClassList(labelOverflowUssClassName + m_LabelOverflow.ToString().ToLower());
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FieldLabel() 
            : this(null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The text to display in the label. </param>
        public FieldLabel(string text)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            m_LabelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_LabelElement.AddToClassList(labelUssClassName);
            hierarchy.Add(m_LabelElement);
            
            m_RequiredLabelElement = new LocalizedTextElement
            {
                name = requiredLabelUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_RequiredLabelElement.AddToClassList(requiredLabelUssClassName);
            hierarchy.Add(m_RequiredLabelElement);
            
            label = text;
            required = false;
            indicatorType = IndicatorType.Asterisk;
            requiredText = "(Required)";
            labelOverflow = TextOverflow.Ellipsis;
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
        /// Factory class to instantiate a <see cref="FieldLabel"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<FieldLabel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="FieldLabel"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Required = new UxmlBoolAttributeDescription
            {
                name = "required",
                defaultValue = false
            };
            
            readonly UxmlEnumAttributeDescription<IndicatorType> m_IndicatorType = new UxmlEnumAttributeDescription<IndicatorType>
            {
                name = "indicator-type",
                defaultValue = IndicatorType.None
            };
            
            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = string.Empty
            };
            
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false
            };
            
            readonly UxmlStringAttributeDescription m_RequiredText = new UxmlStringAttributeDescription
            {
                name = "required-text",
                defaultValue = "(Required)"
            };
            
            readonly UxmlEnumAttributeDescription<TextOverflow> m_LabelOverflow = new UxmlEnumAttributeDescription<TextOverflow>
            {
                name = "label-overflow",
                defaultValue = TextOverflow.Ellipsis
            };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var fieldLabel = (FieldLabel)ve;

                var required = false;
                if (m_Required.TryGetValueFromBag(bag, cc, ref required))
                    fieldLabel.required = required;
                
                var indicatorType = IndicatorType.None;
                if (m_IndicatorType.TryGetValueFromBag(bag, cc, ref indicatorType))
                    fieldLabel.indicatorType = indicatorType;
                
                var label = string.Empty;
                if (m_Label.TryGetValueFromBag(bag, cc, ref label))
                    fieldLabel.label = label;

                var disabled = false;
                if (m_Disabled.TryGetValueFromBag(bag, cc, ref disabled))
                    fieldLabel.disabled = disabled;
                
                var requiredText = string.Empty;
                if (m_RequiredText.TryGetValueFromBag(bag, cc, ref requiredText))
                    fieldLabel.requiredText = requiredText;
                
                var labelOverflow = TextOverflow.Ellipsis;
                if (m_LabelOverflow.TryGetValueFromBag(bag, cc, ref labelOverflow))
                    fieldLabel.labelOverflow = labelOverflow;
            }
        }
    }
}
