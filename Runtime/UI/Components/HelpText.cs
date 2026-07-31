using UnityEngine.UIElements;
using Unity.Properties;

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
    [UxmlElement]
    public partial class HelpText : LocalizedTextElement
    {

        internal static readonly BindingId variantProperty = new BindingId(nameof(variant));


        /// <summary>
        /// The HelpText main styling class.
        /// </summary>
        public new const string ussClassName = "appui-help-text";

        /// <summary>
        /// The HelpText variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(HelpTextVariant))]
        public const string variantUssClassName = ussClassName + "--";

        HelpTextVariant m_Variant;

        /// <summary>
        /// The variant of the <see cref="HelpText"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public HelpTextVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
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

    }
}
