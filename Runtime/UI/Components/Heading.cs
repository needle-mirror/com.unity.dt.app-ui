using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

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
    /// Display headings with various size options for content hierarchy.
    /// </summary>
    /// <remarks>
    /// The Heading component is used to create hierarchical text elements in your UI. It supports different
    /// size variations to establish visual hierarchy and improve content organization. Headings are essential
    /// for creating scannable content and maintaining a clear document structure.
    ///
    /// Headings come with seven different size options, from XXS to XXL, allowing for flexible typography
    /// hierarchy. By default, headings use the primary text color and medium (M) size.
    ///
    /// Note: Headings should be used in a hierarchical order to maintain proper document structure and
    /// accessibility. Don't skip heading levels, and use them to create meaningful content sections.
    /// </remarks>
    /// <example>
    /// <para>Here's an example of creating a typical page hierarchy using different heading sizes — creating a page
    /// hierarchy with headings.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    ///   <Heading text="Welcome to Our App" size="XXL" />
    ///   <Heading text="Getting Started" size="XL" />
    ///   <Heading text="Quick Setup" size="L" primary="false" />
    ///   <Heading text="Prerequisites" size="M" />
    ///   <Heading text="Additional Notes" size="S" primary="false" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating headings programmatically — creating and customizing headings in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var container = new VisualElement();
    ///
    /// // Create main title
    /// var mainTitle = new Heading("Documentation") {
    ///     size = HeadingSize.XXL
    /// };
    ///
    /// // Create section title
    /// var sectionTitle = new Heading("API Reference") {
    ///     size = HeadingSize.L,
    ///     primary = false
    /// };
    ///
    /// container.Add(mainTitle);
    /// container.Add(sectionTitle);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("typography")]
    public sealed partial class Heading : LocalizedTextElement
    {

        internal static readonly BindingId primaryProperty = new BindingId(nameof(primary));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));


        /// <summary>
        /// The Heading main styling class.
        /// </summary>
        public new const string ussClassName = "appui-heading";

        /// <summary>
        /// The Heading primary variant styling class.
        /// </summary>
        public const string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The Heading size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(HeadingSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

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
        /// The size of the Heading.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public HeadingSize size
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
