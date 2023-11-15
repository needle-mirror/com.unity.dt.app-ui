using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The text overflow mode.
    /// </summary>
    public enum TextOverflow
    {
        /// <summary>
        /// The text will be truncated with an ellipsis.
        /// </summary>
        Ellipsis,
        
        /// <summary>
        /// The text won't be truncated.
        /// </summary>
        Normal,
    }

    /// <summary>
    /// InputLabel UI element.
    /// </summary>
    public class InputLabel : VisualElement
    {
        /// <summary>
        /// The InputLabel main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-inputlabel";

        /// <summary>
        /// The InputLabel size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The InputLabel direction styling class.
        /// </summary>
        public static readonly string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The InputLabel input container styling class.
        /// </summary>
        public static readonly string inputContainerUssClassName = ussClassName + "__input-container";
        
        /// <summary>
        /// The InputLabel container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";
        
        /// <summary>
        /// The InputLabel label container styling class.
        /// </summary>
        public static readonly string labelContainerUssClassName = ussClassName + "__label-container";
        
        /// <summary>
        /// The InputLabel field-label styling class.
        /// </summary>
        public static readonly string fieldLabelUssClassName = ussClassName + "__field-label";
        
        /// <summary>
        /// The InputLabel help text styling class.
        /// </summary>
        public static readonly string helpTextUssClassName = ussClassName + "__help-text";

        /// <summary>
        /// The InputLabel input alignment styling class.
        /// </summary>
        public static readonly string inputAlignmentUssClassName = ussClassName + "--input-alignment-";
        
        /// <summary>
        /// The InputLabel with help text styling class.
        /// </summary>
        public static readonly string withHelpTextUssClassName = ussClassName + "--with-help-text";
        
        readonly FieldLabel m_FieldLabel;

        readonly VisualElement m_Container;
        
        readonly HelpText m_HelpText;
        
        Direction m_Direction = Direction.Horizontal;
        
        Align m_InputAlignment = Align.Stretch;
        
        /// <summary>
        /// The content container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The label value.
        /// </summary>
        public string label
        {
            get => m_FieldLabel.label;
            set
            {
                m_FieldLabel.label = value;
                m_FieldLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// The orientation of the label.
        /// </summary>
        public Direction direction
        {
            get => m_Direction;
            set
            {
                RemoveFromClassList(orientationUssClassName + m_Direction.ToString().ToLower());
                m_Direction = value;
                AddToClassList(orientationUssClassName + m_Direction.ToString().ToLower());
            }
        }

        /// <summary>
        /// The text overflow mode.
        /// </summary>
        public TextOverflow labelOverflow
        {
            get => m_FieldLabel.labelOverflow;
            set => m_FieldLabel.labelOverflow = value;
        }

        /// <summary>
        /// The alignment of the input.
        /// </summary>
        public Align inputAlignment
        {
            get => m_InputAlignment;
            set
            {
                RemoveFromClassList(inputAlignmentUssClassName + m_InputAlignment.ToString().ToLower());
                m_InputAlignment = value;
                AddToClassList(inputAlignmentUssClassName + m_InputAlignment.ToString().ToLower());
            }
        }

        /// <summary>
        /// Whether the input is required or not in the form. This will add an asterisk next to the label.
        /// </summary>
        public bool required
        {
            get => m_FieldLabel.required;
            set
            {
                m_FieldLabel.required = value;
                EnableInClassList(Styles.requiredUssClassName,  m_FieldLabel.required);
            }
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        public IndicatorType indicatorType
        {
            get => m_FieldLabel.indicatorType;
            set => m_FieldLabel.indicatorType = value;
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        public string requiredText
        {
            get => m_FieldLabel.requiredText;
            set => m_FieldLabel.requiredText = value;
        }

        /// <summary>
        /// The error message to display.
        /// </summary>
        public string helpMessage
        {
            get => m_HelpText.text;
            set
            {
                m_HelpText.text = value;
                EnableInClassList(withHelpTextUssClassName, !string.IsNullOrEmpty(value));
            }
        }
        
        /// <summary>
        /// The variant of the <see cref="HelpText"/>.
        /// </summary>
        public HelpTextVariant helpVariant
        {
            get => m_HelpText.variant;
            set => m_HelpText.variant = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InputLabel()
            : this(null)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label"> The label value. </param>
        public InputLabel(string label)
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            
            var labelContainer = new VisualElement { name = labelContainerUssClassName, pickingMode = PickingMode.Ignore };
            labelContainer.AddToClassList(labelContainerUssClassName);
            hierarchy.Add(labelContainer);

            m_FieldLabel = new FieldLabel(label) { name = fieldLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_FieldLabel.AddToClassList(fieldLabelUssClassName);
            labelContainer.hierarchy.Add(m_FieldLabel);
            
            var cell = new HelpText { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(helpTextUssClassName);
            labelContainer.hierarchy.Add(cell);
            
            var inputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            inputContainer.AddToClassList(inputContainerUssClassName);
            hierarchy.Add(inputContainer);
            
            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            inputContainer.hierarchy.Add(m_Container);
            
            m_HelpText = new HelpText
            {
                name = helpTextUssClassName, 
                pickingMode = PickingMode.Ignore
            };
            m_HelpText.AddToClassList(helpTextUssClassName);
            inputContainer.hierarchy.Add(m_HelpText);

            this.label = label;
            direction = Direction.Horizontal;
            inputAlignment = Align.Stretch;
            labelOverflow = TextOverflow.Ellipsis;
            requiredText = "(Required)";
            indicatorType = IndicatorType.Asterisk;
            required = false;
            helpMessage = null;
            helpVariant = HelpTextVariant.Destructive;
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
        /// Factory class to instantiate a <see cref="InputLabel"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<InputLabel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="InputLabel"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            readonly UxmlEnumAttributeDescription<TextSize> m_Size = new UxmlEnumAttributeDescription<TextSize>
            {
                name = "size",
                defaultValue = TextSize.S,
            };

            readonly UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = "label",
                defaultValue = null,
            };

            readonly UxmlEnumAttributeDescription<Direction> m_Orientation = new UxmlEnumAttributeDescription<Direction>
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
            };

            readonly UxmlEnumAttributeDescription<TextOverflow> m_LabelOverflow = new UxmlEnumAttributeDescription<TextOverflow>
            {
                name = "label-overflow",
                defaultValue = TextOverflow.Ellipsis,
            };

            readonly UxmlEnumAttributeDescription<Align> m_InputAlignment = new UxmlEnumAttributeDescription<Align>
            {
                name = "input-alignment",
                defaultValue = Align.Stretch,
            };
            
            readonly UxmlBoolAttributeDescription m_Required = new UxmlBoolAttributeDescription
            {
                name = "required",
                defaultValue = false,
            };
            
            readonly UxmlStringAttributeDescription m_HelpMessage = new UxmlStringAttributeDescription
            {
                name = "help-message",
                defaultValue = null,
            };
            
            readonly UxmlEnumAttributeDescription<HelpTextVariant> m_HelpVariant = new UxmlEnumAttributeDescription<HelpTextVariant>
            {
                name = "help-variant",
                defaultValue = HelpTextVariant.Destructive,
            };
            
            readonly UxmlStringAttributeDescription m_RequiredText = new UxmlStringAttributeDescription
            {
                name = "required-text",
                defaultValue = "(Required)",
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

                var element = (InputLabel)ve;
                
                var direction = Direction.Horizontal;
                if (m_Orientation.TryGetValueFromBag(bag, cc, ref direction))
                    element.direction = direction;
                
                var inputAlignment = Align.Stretch;
                if (m_InputAlignment.TryGetValueFromBag(bag, cc, ref inputAlignment))
                    element.inputAlignment = inputAlignment;
                
                var labelOverflow = TextOverflow.Ellipsis;
                if (m_LabelOverflow.TryGetValueFromBag(bag, cc, ref labelOverflow))
                    element.labelOverflow = labelOverflow;
                
                var label = string.Empty;
                if (m_Label.TryGetValueFromBag(bag, cc, ref label))
                    element.label = label;
                
                var disabled = false;
                if (m_Disabled.TryGetValueFromBag(bag, cc, ref disabled))
                    element.disabled = disabled;
                
                var required = false;
                if (m_Required.TryGetValueFromBag(bag, cc, ref required))
                    element.required = required;
                
                var helpMessage = string.Empty;
                if (m_HelpMessage.TryGetValueFromBag(bag, cc, ref helpMessage))
                    element.helpMessage = helpMessage;
                
                var helpVariant = HelpTextVariant.Destructive;
                if (m_HelpVariant.TryGetValueFromBag(bag, cc, ref helpVariant))
                    element.helpVariant = helpVariant;
                
                var requiredText = string.Empty;
                if (m_RequiredText.TryGetValueFromBag(bag, cc, ref requiredText))
                    element.requiredText = requiredText;
            }
        }
    }
}
