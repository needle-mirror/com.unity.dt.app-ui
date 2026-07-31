using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

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
    /// A versatile label component for form inputs with support for required fields, help text, and various
    /// layout options.
    /// </summary>
    /// <remarks>
    /// InputLabel is a specialized UI component designed to provide a consistent and accessible way to label form
    /// inputs. It combines a label with optional required field indicators and help text, making it ideal for form
    /// design and user input validation.
    ///
    /// The component supports both horizontal and vertical layouts, customizable label overflow behavior, and
    /// different styles of required field indicators to match your design system needs.
    ///
    /// Note: InputLabel is designed to work seamlessly with other form input components and follows accessibility
    /// best practices by maintaining proper label-input relationships.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class InputLabel : BaseVisualElement
    {

        internal static readonly BindingId labelProperty = new BindingId(nameof(label));

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));

        internal static readonly BindingId labelOverflowProperty = new BindingId(nameof(labelOverflow));

        internal static readonly BindingId inputAlignmentProperty = new BindingId(nameof(inputAlignment));

        internal static readonly BindingId requiredProperty = new BindingId(nameof(required));

        internal static readonly BindingId indicatorTypeProperty = new BindingId(nameof(indicatorType));

        internal static readonly BindingId requiredTextProperty = new BindingId(nameof(requiredText));

        internal static readonly BindingId helpMessageProperty = new BindingId(nameof(helpMessage));

        internal static readonly BindingId helpVariantProperty = new BindingId(nameof(helpVariant));

        internal static readonly BindingId draggableProperty = new BindingId(nameof(draggable));


        /// <summary>
        /// The InputLabel main styling class.
        /// </summary>
        public const string ussClassName = "appui-inputlabel";

        /// <summary>
        /// The InputLabel size styling class.
        /// </summary>
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The InputLabel direction styling class.
        /// </summary>
        [EnumName("GetOrientationUssClassName", typeof(Direction))]
        public const string orientationUssClassName = ussClassName + "--";

        /// <summary>
        /// The InputLabel input container styling class.
        /// </summary>
        public const string inputContainerUssClassName = ussClassName + "__input-container";

        /// <summary>
        /// The InputLabel container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The InputLabel label container styling class.
        /// </summary>
        public const string labelContainerUssClassName = ussClassName + "__label-container";

        /// <summary>
        /// The InputLabel field-label styling class.
        /// </summary>
        public const string fieldLabelUssClassName = ussClassName + "__field-label";

        /// <summary>
        /// The InputLabel help text styling class.
        /// </summary>
        public const string helpTextUssClassName = ussClassName + "__help-text";

        /// <summary>
        /// The InputLabel input alignment styling class.
        /// </summary>
        [EnumName("GetInputAlignmentUssClassName", typeof(Align))]
        public const string inputAlignmentUssClassName = ussClassName + "--input-alignment-";

        /// <summary>
        /// The InputLabel with help text styling class.
        /// </summary>
        public const string withHelpTextUssClassName = ussClassName + "--with-help-text";

        /// <summary>
        /// The InputLabel dragging styling class.
        /// </summary>
        public const string draggingUssClassName = ussClassName + "--dragging";

        readonly VisualElement m_LabelContainer;

        readonly FieldLabel m_FieldLabel;

        readonly VisualElement m_Container;

        readonly HelpText m_HelpText;

        Direction m_Direction = Direction.Horizontal;

        Align m_InputAlignment = Align.Stretch;

        bool m_Draggable;

        Draggable m_DraggableManipulator;

        /// <summary>
        /// The content container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The label value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_FieldLabel.label;
            set
            {
                var changed = m_FieldLabel.label != value;
                m_FieldLabel.label = value;
                m_FieldLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The orientation of the label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetOrientationUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetOrientationUssClassName(m_Direction));

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The text overflow mode.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TextOverflow labelOverflow
        {
            get => m_FieldLabel.labelOverflow;
            set
            {
                var changed = m_FieldLabel.labelOverflow != value;
                m_FieldLabel.labelOverflow = value;

                if (changed)
                    NotifyPropertyChanged(in labelOverflowProperty);
            }
        }

        /// <summary>
        /// The alignment of the input.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Align inputAlignment
        {
            get => m_InputAlignment;
            set
            {
                var changed = m_InputAlignment != value;
                RemoveFromClassList(GetInputAlignmentUssClassName(m_InputAlignment));
                m_InputAlignment = value;
                AddToClassList(GetInputAlignmentUssClassName(m_InputAlignment));

                if (changed)
                    NotifyPropertyChanged(in inputAlignmentProperty);
            }
        }

        /// <summary>
        /// Whether the input is required or not in the form. This will add an asterisk next to the label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool required
        {
            get => m_FieldLabel.required;
            set
            {
                var changed = m_FieldLabel.required != value;
                m_FieldLabel.required = value;
                EnableInClassList(Styles.requiredUssClassName,  m_FieldLabel.required);

                if (changed)
                    NotifyPropertyChanged(in requiredProperty);
            }
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IndicatorType indicatorType
        {
            get => m_FieldLabel.indicatorType;
            set
            {
                var changed = m_FieldLabel.indicatorType != value;
                m_FieldLabel.indicatorType = value;

                if (changed)
                    NotifyPropertyChanged(in indicatorTypeProperty);
            }
        }

        /// <summary>
        /// The requirement indicator to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string requiredText
        {
            get => m_FieldLabel.requiredText;
            set
            {
                var changed = m_FieldLabel.requiredText != value;
                m_FieldLabel.requiredText = value;

                if (changed)
                    NotifyPropertyChanged(in requiredTextProperty);
            }
        }

        /// <summary>
        /// The error message to display.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string helpMessage
        {
            get => m_HelpText.text;
            set
            {
                var changed = m_HelpText.text != value;
                m_HelpText.text = value;
                EnableInClassList(withHelpTextUssClassName, !string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in helpMessageProperty);
            }
        }

        /// <summary>
        /// The variant of the <see cref="HelpText"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public HelpTextVariant helpVariant
        {
            get => m_HelpText.variant;
            set
            {
                var changed = m_HelpText.variant != value;
                m_HelpText.variant = value;

                if (changed)
                    NotifyPropertyChanged(in helpVariantProperty);
            }
        }

        /// <summary>
        /// Whether the label is draggable to increment/decrement the value of child input fields.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool draggable
        {
            get => m_Draggable;
            set
            {
                var changed = m_Draggable != value;
                m_Draggable = value;
                EnableInClassList(draggingUssClassName, m_Draggable);

                if (m_Draggable)
                {
                    if (m_DraggableManipulator == null)
                    {
                        m_DraggableManipulator = new Draggable(null, OnDrag, OnDragEnd, OnDragStart)
                        {
                            dragDirection = Draggable.DragDirection.Horizontal
                        };
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Shift
                        });
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Alt
                        });
                        m_DraggableManipulator.activators.Add(new ManipulatorActivationFilter
                        {
                            button = MouseButton.LeftMouse,
                            modifiers = EventModifiers.Shift | EventModifiers.Alt
                        });
                        m_LabelContainer.AddManipulator(m_DraggableManipulator);
                    }
                }
                else
                {
                    if (m_DraggableManipulator != null)
                    {
                        m_LabelContainer.RemoveManipulator(m_DraggableManipulator);
                        m_DraggableManipulator = null;
                    }
                }

                if (changed)
                    NotifyPropertyChanged(in draggableProperty);
            }
        }

        void OnDragStart(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Started, Vector3.zero, s));
        }

        void OnDrag(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Dragging, d.deltaPos, s));
        }

        void OnDragEnd(Draggable d)
        {
            var s = d.shiftKey ? DeltaSpeed.Fast : d.altKey ? DeltaSpeed.Slow : DeltaSpeed.Normal;
            this.ProvideContext(new DragContext(DragPhase.Ended, Vector3.zero, s));
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
            focusable = true;
            delegatesFocus = true;
            pickingMode = PickingMode.Position;

            m_LabelContainer = new VisualElement { name = labelContainerUssClassName, pickingMode = PickingMode.Position };
            m_LabelContainer.AddToClassList(labelContainerUssClassName);
            hierarchy.Add(m_LabelContainer);

            m_FieldLabel = new FieldLabel(label) { name = fieldLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_FieldLabel.AddToClassList(fieldLabelUssClassName);
            m_LabelContainer.hierarchy.Add(m_FieldLabel);

            var cell = new HelpText { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(helpTextUssClassName);
            m_LabelContainer.hierarchy.Add(cell);

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
            draggable = false;
        }

    }
}
