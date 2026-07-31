using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The spacer spacing.
    /// </summary>
    public enum SpacerSpacing
    {
        /// <summary>
        /// No spacing.
        /// </summary>
        Null,

        /// <summary>
        /// Extra small spacing.
        /// </summary>
        XS,

        /// <summary>
        /// Small spacing.
        /// </summary>
        S,

        /// <summary>
        /// Medium spacing.
        /// </summary>
        M,

        /// <summary>
        /// Large spacing.
        /// </summary>
        L,

        /// <summary>
        /// Extra large spacing.
        /// </summary>
        XL,

        /// <summary>
        /// The spacer will expand to fill the remaining space using flex-grow.
        /// </summary>
        Expand
    }

    /// <summary>
    /// A layout component that creates consistent spacing between elements.
    /// </summary>
    /// <remarks>
    /// The Spacer component is a versatile layout utility that helps create whitespace and manage spacing between
    /// elements in your UI. It provides predefined spacing options and can also expand to fill available space,
    /// making it essential for creating consistent and responsive layouts.
    ///
    /// Spacers are non-interactive elements (they ignore pointer events) and can be used both vertically and
    /// horizontally depending on the parent container's layout direction.
    ///
    /// The Spacer component automatically handles its styling through USS classes, making it easy to maintain
    /// consistent spacing across your application.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in a horizontal layout. Creating equal spacing between buttons in a horizontal layout.</para>
    /// <code lang="xml"><![CDATA[
    /// <Box style="flex-direction: row;">
    ///     <Button text="Left" />
    ///     <Spacer spacing="M" />
    ///     <Button text="Center" />
    ///     <Spacer spacing="M" />
    ///     <Button text="Right" />
    /// </Box>
    /// ]]></code>
    /// <para>Using Spacer to push elements apart. Using an expanding spacer to push elements to opposite ends of a
    /// container.</para>
    /// <code lang="xml"><![CDATA[
    /// <Box style="flex-direction: row;">
    ///     <Button text="Left-aligned" />
    ///     <Spacer spacing="Expand" />
    ///     <Button text="Right-aligned" />
    /// </Box>
    /// ]]></code>
    /// <para>Vertical spacing in a column layout. Creating different spaces between form elements in a vertical layout.</para>
    /// <code lang="xml"><![CDATA[
    /// <Box style="flex-direction: column;">
    ///     <Label text="Header" />
    ///     <Spacer spacing="S" />
    ///     <TextField />
    ///     <Spacer spacing="L" />
    ///     <Button text="Submit" />
    /// </Box>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Spacer : BaseVisualElement
    {

        internal static readonly BindingId spacingProperty = nameof(spacing);


        const SpacerSpacing k_DefaultSpacing = SpacerSpacing.M;

        /// <summary>
        /// The spacer's main USS class name.
        /// </summary>
        public const string ussClassName = "appui-spacer";

        /// <summary>
        /// The spacer's spacing USS class name.
        /// </summary>
        [EnumName("GetSpacingUssClassName", typeof(SpacerSpacing))]
        public const string spacingUssClassName = ussClassName + "--spacing-";

        SpacerSpacing m_Spacing = k_DefaultSpacing;

        /// <summary>
        /// Main constructor.
        /// </summary>
        public Spacer()
        {
            pickingMode = PickingMode.Ignore;

            AddToClassList(ussClassName);

            spacing = k_DefaultSpacing;
        }

        /// <summary>
        /// The spacer's spacing.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public SpacerSpacing spacing
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
