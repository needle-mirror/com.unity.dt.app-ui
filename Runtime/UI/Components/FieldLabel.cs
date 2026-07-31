using UnityEngine.UIElements;
using Unity.Properties;

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
    [UxmlElement]
    public partial class FieldLabel : BaseVisualElement
    {

        internal static readonly BindingId labelProperty = new BindingId(nameof(label));

        internal static readonly BindingId requiredProperty = new BindingId(nameof(required));

        internal static readonly BindingId indicatorTypeProperty = new BindingId(nameof(indicatorType));

        internal static readonly BindingId requiredTextProperty = new BindingId(nameof(requiredText));

        internal static readonly BindingId labelOverflowProperty = new BindingId(nameof(labelOverflow));


        /// <summary>
        /// The FieldLabel main styling class.
        /// </summary>
        public const string ussClassName = "appui-field-label";

        /// <summary>
        /// The FieldLabel variant styling class.
        /// </summary>
        [EnumName("GetIndicatorTypeUssClassName", typeof(IndicatorType))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The FieldLabel required variant styling class.
        /// </summary>
        public const string requiredUssClassName = ussClassName + "--required";

        /// <summary>
        /// The FieldLabel label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The FieldLabel required label styling class.
        /// </summary>
        public const string requiredLabelUssClassName = ussClassName + "__required-label";

        /// <summary>
        /// The FieldLabel label overflow variant styling class.
        /// </summary>
        [EnumName("GetLabelOverflowUssClassName", typeof(TextOverflow))]
        public const string labelOverflowUssClassName = ussClassName + "--label-overflow-";

        readonly LocalizedTextElement m_LabelElement;

        readonly LocalizedTextElement m_RequiredLabelElement;

        IndicatorType m_IndicatorType = IndicatorType.Asterisk;

        string m_RequiredText;

        TextOverflow m_LabelOverflow;

        /// <summary>
        /// Whether the field is required.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool required
        {
            get => ClassListContains(requiredUssClassName);
            set
            {
                var changed = required != value;
                EnableInClassList(requiredUssClassName, value);
                m_RequiredLabelElement.text = m_IndicatorType switch
                {
                    IndicatorType.Asterisk => "*",
                    IndicatorType.Text => m_RequiredText,
                    _ => null
                };

                if (changed)
                    NotifyPropertyChanged(in requiredProperty);
            }
        }

        /// <summary>
        /// The type of indicator to display when a field is required.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IndicatorType indicatorType
        {
            get => m_IndicatorType;
            set
            {
                var changed = m_IndicatorType != value;
                RemoveFromClassList(GetIndicatorTypeUssClassName(m_IndicatorType));
                m_IndicatorType = value;
                AddToClassList(GetIndicatorTypeUssClassName(m_IndicatorType));

                if (changed)
                    NotifyPropertyChanged(in indicatorTypeProperty);
            }
        }

        /// <summary>
        /// The text to display next to the label when the field is required and the indicator type is <see cref="IndicatorType.Text"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string requiredText
        {
            get => m_RequiredText;
            set
            {
                var changed = m_RequiredText != value;
                m_RequiredText = value;
                if (m_IndicatorType == IndicatorType.Text)
                    m_RequiredLabelElement.text = m_RequiredText;

                if (changed)
                    NotifyPropertyChanged(in requiredTextProperty);
            }
        }

        /// <summary>
        /// The text to display in the label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                var changed = m_LabelElement.text != value;
                m_LabelElement.text = value;

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The text overflow mode.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TextOverflow labelOverflow
        {
            get => m_LabelOverflow;
            set
            {
                var changed = m_LabelOverflow != value;
                RemoveFromClassList(GetLabelOverflowUssClassName(m_LabelOverflow));
                m_LabelOverflow = value;
                AddToClassList(GetLabelOverflowUssClassName(m_LabelOverflow));

                if (changed)
                    NotifyPropertyChanged(in labelOverflowProperty);
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

    }
}
