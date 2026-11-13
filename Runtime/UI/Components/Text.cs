using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Sizing values for <see cref="Text"/> UI element.
    /// </summary>
    /// <remarks>
    /// The scale keeps the 2.3 member names (<see cref="XS"/> to <see cref="XXXL"/>) and mirrors the App UI
    /// design-system registry's `textVariants` size vocabulary (`xs`, `sm`, `base`, `lg`, `xl`, `2xl`, `3xl`):
    /// <see cref="M"/> is the registry's `base`. Every step resolves to an `--appui-font-sizes-*` token, so the
    /// rendered size follows the <see cref="Panel.density"/> in effect.
    /// </remarks>
    public enum TextSize
    {
        /// <summary>
        /// Extra-small — `--appui-font-sizes-75`.
        /// </summary>
        XS,

        /// <summary>
        /// Small — `--appui-font-sizes-100`.
        /// </summary>
        S,

        /// <summary>
        /// The default body size — `--appui-font-sizes-200`.
        /// </summary>
        M,

        /// <summary>
        /// Large — `--appui-font-sizes-400`.
        /// </summary>
        L,

        /// <summary>
        /// Extra-large — `--appui-font-sizes-500`.
        /// </summary>
        XL,

        /// <summary>
        /// Double extra-large — `--appui-font-sizes-700`.
        /// </summary>
        XXL,

        /// <summary>
        /// Triple extra-large — `--appui-font-sizes-900`.
        /// </summary>
        XXXL,
    }

    /// <summary>
    /// Emphasis values for <see cref="Text"/> UI element.
    /// </summary>
    /// <remarks>
    /// Mirrors the registry's `textVariants` variant axis. Each value binds to a semantic
    /// text-color alias, never to a raw palette step, so it re-resolves per theme.
    /// </remarks>
    public enum TextVariant
    {
        /// <summary>
        /// Body copy — `--appui-alias-text-primary`.
        /// </summary>
        Default,

        /// <summary>
        /// De-emphasized supporting copy — `--appui-alias-text-secondary`.
        /// </summary>
        Secondary,

        /// <summary>
        /// The quietest tier, for captions and hints — `--appui-alias-text-disabled`.
        /// </summary>
        Muted,
    }

    /// <summary>
    /// A versatile text component that displays text content with customizable size and styling options.
    /// </summary>
    /// <remarks>
    /// The Text component is a fundamental UI element used to display text content in your application. It
    /// provides a range of size options and emphasis variants to help maintain consistency and hierarchy in
    /// your text presentation.
    ///
    /// Text carries two orthogonal axes, both mirroring the App UI registry: a <see cref="size"/> drawn from
    /// <see cref="TextSize"/> (seven steps from <see cref="TextSize.XS"/> to <see cref="TextSize.XXXL"/>,
    /// defaulting to <see cref="TextSize.M"/>) and a <see cref="variant"/> drawn from
    /// <see cref="TextVariant"/> (<see cref="TextVariant.Default"/>, <see cref="TextVariant.Secondary"/> and
    /// <see cref="TextVariant.Muted"/>). Sizes resolve to `--appui-font-sizes-*` tokens and so rescale with
    /// the <see cref="Panel.density"/>; variants resolve to `--appui-alias-text-*` aliases and so re-resolve
    /// per theme.
    ///
    /// For titles and section headers, reach for <see cref="Heading"/> instead. The component is also
    /// localization-ready, inheriting from LocalizedTextElement.
    ///
    /// The Text component supports both UXML attributes and runtime property modification, making it flexible
    /// for both declarative and programmatic UI development.
    /// </remarks>
    /// <example>
    /// <para>Here's an example of creating a typical text hierarchy using different sizes and variants: Creating a
    /// text hierarchy in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns="UnityEngine.UIElements">
    ///     <Text text="Lead paragraph" size="XXL" />
    ///     <Text text="Supporting copy" size="XL" variant="Secondary" />
    ///     <Text text="Body text goes here at the default size." />
    ///     <Text text="Small caption text" size="XS" variant="Muted" />
    /// </UXML>
    /// ]]></code>
    /// <para>For runtime text manipulation: Creating and managing text elements via code.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Create text elements
    /// var container = new VisualElement();
    ///
    /// // Add a title
    /// var title = new Text("Welcome!") {
    ///     size = TextSize.XXL
    /// };
    /// container.Add(title);
    ///
    /// // Add a subtitle
    /// var subtitle = new Text("Please read the following instructions") {
    ///     size = TextSize.L,
    ///     variant = TextVariant.Secondary
    /// };
    /// container.Add(subtitle);
    ///
    /// // Add body text
    /// var body = new Text("Detailed instructions go here...");
    /// container.Add(body);
    ///
    /// // Add a quiet caption
    /// var caption = new Text("Last updated 5 minutes ago") {
    ///     size = TextSize.XS,
    ///     variant = TextVariant.Muted
    /// };
    /// container.Add(caption);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("typography")]
    public sealed partial class Text : LocalizedTextElement
    {

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId sizeProperty = nameof(size);


        /// <summary>
        /// The Text main styling class.
        /// </summary>
        public new const string ussClassName = "appui-text";

        /// <summary>
        /// The Text variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(TextVariant))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The Text size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(TextSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        TextSize m_Size = TextSize.M;

        TextVariant m_Variant = TextVariant.Default;

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
            AddToClassList(GetSizeUssClassName(m_Size));
            AddToClassList(GetVariantUssClassName(m_Variant));
        }

        /// <summary>
        /// The emphasis variant of the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TextVariant.Default"/>. Each variant binds to a semantic
        /// `--appui-alias-text-*` token rather than a literal color.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public TextVariant variant
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
        /// The size of the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TextSize.M"/>. Each step binds to an `--appui-font-sizes-*`
        /// token, so the rendered size follows the surrounding <see cref="Panel.density"/>.
        /// </remarks>
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
