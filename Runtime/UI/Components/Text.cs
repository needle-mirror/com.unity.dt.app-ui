using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Sizing values for <see cref="Text"/> UI element.
    /// </summary>
    public enum TextSize
    {
        /// <summary>
        /// Extra-extra-small
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

        /// <summary>
        /// Triple Extra-large
        /// </summary>
        XXXL,
    }

    /// <summary>
    /// A versatile text component that displays text content with customizable size and styling options.
    /// </summary>
    /// <remarks>
    /// The Text component is a fundamental UI element used to display text content in your application. It
    /// provides a range of size options and styling variants to help maintain consistency and hierarchy in
    /// your text presentation.
    ///
    /// Text components come with eight predefined sizes (from XXS to XXXL) and support primary/secondary
    /// variants to establish visual hierarchy. The component is also localization-ready, inheriting from
    /// LocalizedTextElement.
    ///
    /// The Text component supports both UXML attributes and runtime property modification, making it flexible
    /// for both declarative and programmatic UI development.
    /// </remarks>
    /// <example>
    /// <para>Here's an example of creating a typical text hierarchy using different sizes and variants: Creating a
    /// text hierarchy in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns="UnityEngine.UIElements">
    ///     <Text text="Main Heading" size="XXL" />
    ///     <Text text="Subheading" size="XL" primary="false" />
    ///     <Text text="Body text goes here with a medium size." size="M" />
    ///     <Text text="Small caption text" size="XS" primary="false" />
    /// </UXML>
    /// ]]></code>
    /// <para>For runtime text manipulation: Creating and managing text elements via code.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Create text elements
    /// var container = new VisualElement();
    ///
    /// // Add a title
    /// var title = new Text("Welcome!") {
    ///     size = TextSize.XXL,
    ///     primary = true
    /// };
    /// container.Add(title);
    ///
    /// // Add a subtitle
    /// var subtitle = new Text("Please read the following instructions") {
    ///     size = TextSize.L,
    ///     primary = false
    /// };
    /// container.Add(subtitle);
    ///
    /// // Add body text
    /// var body = new Text("Detailed instructions go here...") {
    ///     size = TextSize.M
    /// };
    /// container.Add(body);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("typography")]
    public sealed partial class Text : LocalizedTextElement
    {

        internal static readonly BindingId primaryProperty = nameof(primary);

        internal static readonly BindingId sizeProperty = nameof(size);


        /// <summary>
        /// The Text main styling class.
        /// </summary>
        public new const string ussClassName = "appui-text";

        /// <summary>
        /// The Text primary variant styling class.
        /// </summary>
        public const string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Text size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(TextSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        TextSize m_Size = TextSize.M;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Text()
            : this(string.Empty) { }

        /// <summary>
        /// Construct a Text UI element and use the provided text as display text.
        /// </summary>
        /// <param name="text">The text that will be displayed.</param>
        public Text(string text)
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position; // tooltip support

            this.text = text;
            size = TextSize.M;
            primary = true;
        }

        /// <summary>
        /// The primary variant of the text.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set
            {
                var changed = ClassListContains(primaryUssClassName) != value;
                EnableInClassList(primaryUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in primaryProperty);
            }
        }

        /// <summary>
        /// The size of the text.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TextSize size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

    }
}
