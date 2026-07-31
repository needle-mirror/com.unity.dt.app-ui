using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A visual element that helps highlight and distinguish quoted or featured content from regular text.
    /// </summary>
    /// <remarks>
    /// The Quote component is a specialized visual container that creates a distinctive block for showcasing
    /// quoted content, testimonials, or any text that needs to be visually separated from the surrounding content.
    /// It uses a vertical border line that can be customized in color to create visual hierarchy and improve
    /// content readability.
    ///
    /// This component follows a minimalist design approach, using subtle visual cues rather than heavy quotation
    /// marks or backgrounds, making it suitable for both formal and casual user interfaces.
    ///
    /// The Quote component is particularly useful in documentation, testimonials, blog posts, or any context where
    /// you need to highlight specific content while maintaining the overall flow of the interface.
    /// </remarks>
    /// <example>
    /// <para>Basic usage of the Quote component: A simple quote with attribution</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Quote>
    ///     <ui:Label text="The best way to predict the future is to invent it."/>
    ///     <ui:Label text="- Alan Kay" class="quote-attribution"/>
    /// </ui:Quote>
    /// ]]></code>
    /// <para>Creating a Quote programmatically with custom content: Creating a quote with custom styling and nested
    /// elements</para>
    /// <code lang="csharp"><![CDATA[
    /// var quote = new Quote();
    /// quote.color = Color.blue;
    ///
    /// var content = new Label();
    /// content.text = "Innovation distinguishes between a leader and a follower.";
    /// quote.Add(content);
    ///
    /// var attribution = new Label();
    /// attribution.text = "- Steve Jobs";
    /// attribution.AddToClassList("quote-attribution");
    /// quote.Add(attribution);
    /// ]]></code>
    /// <para>Using Quote in a complex layout: Integrating a quote within a larger content structure</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:VerticalLayout>
    ///     <ui:Label text="Key Insights:" class="heading"/>
    ///     <ui:Quote color="#4CAF50">
    ///         <ui:VerticalLayout spacing="8">
    ///             <ui:Label text="First, solve the problem. Then, write the code." />
    ///             <ui:Label text="- John Johnson" class="quote-attribution"/>
    ///         </ui:VerticalLayout>
    ///     </ui:Quote>
    /// </ui:VerticalLayout>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Quote : BaseVisualElement
    {

        internal static readonly BindingId colorProperty = nameof(color);


        /// <summary>
        /// The Quote main styling class.
        /// </summary>
        public const string ussClassName = "appui-quote";

        /// <summary>
        /// The Quote container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        readonly VisualElement m_Container;

        Optional<Color> m_InlineColor;

        /// <summary>
        /// The content container of the Quote.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The Quote outline color.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_InlineColor;
            set
            {
                m_InlineColor = value;
                var borderColor = m_InlineColor.IsSet ? m_InlineColor.Value : new StyleColor(StyleKeyword.Null);
                var previousBorderColor = resolvedStyle.borderLeftColor;
                style.borderLeftColor = borderColor;
                style.borderRightColor = borderColor;

                if (borderColor != previousBorderColor)
                    NotifyPropertyChanged(in colorProperty);
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

            color = Optional<Color>.none;
        }

    }
}
