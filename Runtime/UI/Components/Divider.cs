using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A visual separator that creates a thematic break between different content sections.
    /// </summary>
    /// <remarks>
    /// The Divider component is a thin line that groups content in lists and layouts. Dividers can separate
    /// content into clear groups and establish hierarchy, creating visual rhythm and improving content
    /// readability.
    ///
    /// Dividers can be oriented horizontally or vertically, and their appearance can be customized through size
    /// and spacing properties to match your design requirements.
    ///
    /// Note: Dividers are non-interactive elements with picking mode set to ignore by default.
    /// </remarks>
    /// <example>
    /// <para>Basic horizontal divider — simple horizontal divider with default properties.</para>
    /// <code lang="xml"><![CDATA[
    /// <Divider />
    /// ]]></code>
    /// <para>Custom styled divider — customized divider with specific size and spacing.</para>
    /// <code lang="xml"><![CDATA[
    /// <Divider direction="Horizontal" size="L" spacing="XL" />
    /// ]]></code>
    /// <para>Using dividers in a layout — example of using dividers in a complex layout.</para>
    /// <code lang="xml"><![CDATA[
    /// <Box>
    ///     <Text text="Section 1" />
    ///     <Divider spacing="M" />
    ///     <Text text="Section 2" />
    ///     <Box style="flex-direction: row;">
    ///         <Box>
    ///             <Text text="Left Content" />
    ///         </Box>
    ///         <Divider direction="Vertical" size="S" spacing="S" />
    ///         <Box>
    ///             <Text text="Right Content" />
    ///         </Box>
    ///     </Box>
    ///     <Divider spacing="M" />
    ///     <Text text="Section 3" />
    /// </Box>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Divider : BaseVisualElement
    {

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId spacingProperty = new BindingId(nameof(spacing));


        /// <summary>
        /// The Divider main styling class.
        /// </summary>
        public const string ussClassName = "appui-divider";

        /// <summary>
        /// The Divider size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Divider spacing styling class.
        /// </summary>
        [EnumName("GetSpacingUssClassName", typeof(Spacing))]
        public const string spacingUssClassName = ussClassName + "--spacing-";

        /// <summary>
        /// The Divider vertical mode styling class.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string verticalUssClassName = ussClassName + "--";

        /// <summary>
        /// The Divider content styling class.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The content container of the Divider. This is always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        Size m_Size;

        Spacing m_Spacing;

        Direction m_Direction;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Divider()
        {
            AddToClassList(ussClassName);

            var content = new VisualElement { name = contentUssClassName, pickingMode = PickingMode.Ignore };
            content.AddToClassList(contentUssClassName);
            hierarchy.Add(content);

            pickingMode = PickingMode.Ignore;

            size = Size.M;
            spacing = Spacing.M;
            direction = Direction.Horizontal;
        }

        /// <summary>
        /// The orientation of the Divider.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetDirectionUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetDirectionUssClassName(m_Direction));

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// The size of the Divider.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
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

        /// <summary>
        /// The spacing of the Divider.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Spacing spacing
        {
            get => m_Spacing;
            set
            {
                var changed = m_Spacing != value;
                RemoveFromClassList(GetSpacingUssClassName(m_Spacing));
                m_Spacing = value;
                AddToClassList(GetSpacingUssClassName(m_Spacing));

                if (changed)
                    NotifyPropertyChanged(in spacingProperty);
            }
        }

    }
}
