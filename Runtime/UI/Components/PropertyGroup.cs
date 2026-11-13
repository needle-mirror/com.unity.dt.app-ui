using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How prominent a <see cref="PropertyGroup"/> is in an inspector.
    /// </summary>
    public enum PropertyGroupLevel
    {
        /// <summary>
        /// A top-level group: the unit an inspector is made of, one component on an object. Sections
        /// carry the larger type and sit <c>--appui-alias-inspector-section-gap</c> apart in a
        /// <see cref="PropertyGroupStack"/>.
        /// </summary>
        Section,

        /// <summary>
        /// A group nested inside a section, for controls that belong together more tightly than the
        /// section as a whole. The default.
        /// </summary>
        Sub,
    }

    /// <summary>
    /// A collapsible group of inspector rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The title sits on the text line with no caret, which is what keeps a column of groups reading as
    /// text rather than as a tree; a closed group that has something in it shows an ellipsis after its
    /// label instead. Rows inside are <c>--appui-alias-inspector-row-gap</c> apart, so a stack of
    /// <see cref="InputLabel"/> rows in <see cref="InputLabelRowLayout.Grid"/> keeps one rhythm across
    /// groups.
    /// </para>
    /// <para>
    /// An empty group is not a disclosure: it has nothing to expand, so its header is disabled and shows
    /// no ellipsis rather than claiming to be open. A group whose rows are laid out elsewhere sets
    /// <see cref="expandable"/> to keep its toggle. Use <see cref="PropertyGroupStack"/> for the column
    /// a panel stacks its sections in.
    /// </para>
    /// <para>
    /// This is not an <see cref="AccordionItem"/>: an accordion is a list of disclosures with carets,
    /// one of which is open, while a property group is a titled span of an inspector that happens to
    /// fold away.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>A section with a nested sub-group of rows.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:PropertyGroupStack>
    ///     <appui:PropertyGroup label="Transform" level="Section">
    ///         <appui:InputLabel label="Position" row-layout="Grid">
    ///             <appui:Vector3Field />
    ///         </appui:InputLabel>
    ///         <appui:PropertyGroup label="Advanced">
    ///             <appui:InputLabel label="Static" row-layout="Grid">
    ///                 <appui:Checkbox />
    ///             </appui:InputLabel>
    ///         </appui:PropertyGroup>
    ///     </appui:PropertyGroup>
    /// </appui:PropertyGroupStack>
    /// ]]></code>
    /// </example>
    [VisualDocPage("inputs")]
    [UxmlElement]
    public partial class PropertyGroup : BaseVisualElement
    {
        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId levelProperty = nameof(level);

        internal static readonly BindingId openProperty = nameof(open);

        internal static readonly BindingId expandableProperty = nameof(expandable);

        /// <summary>
        /// The PropertyGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-property-group";

        /// <summary>
        /// The styling class of the PropertyGroup's header row, which holds the toggle and the trailing
        /// slot.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the PropertyGroup's header toggle.
        /// </summary>
        public const string toggleUssClassName = ussClassName + "__toggle";

        /// <summary>
        /// The styling class of the PropertyGroup's label.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The styling class of the ellipsis a closed PropertyGroup shows after its label.
        /// </summary>
        public const string ellipsisUssClassName = ussClassName + "__ellipsis";

        /// <summary>
        /// The styling class of the PropertyGroup's trailing slot.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailing-container";

        /// <summary>
        /// The styling class of the PropertyGroup's content.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The PropertyGroup level styling class.
        /// </summary>
        [EnumName("GetLevelUssClassName", typeof(PropertyGroupLevel))]
        public const string levelUssClassName = ussClassName + "--level-";

        /// <summary>
        /// The PropertyGroup styling class applied while it is open.
        /// </summary>
        public const string openUssClassName = ussClassName + "--open";

        /// <summary>
        /// The PropertyGroup styling class applied while it has nothing to show.
        /// </summary>
        public const string emptyUssClassName = ussClassName + "--empty";

        /// <summary>
        /// The PropertyGroup styling class applied while it has trailing content beside its title.
        /// </summary>
        public const string withTrailingUssClassName = ussClassName + "--with-trailing";

        /// <summary>
        /// The PropertyGroup styling class applied while it has no rows of its own, whether or not it is
        /// <see cref="expandable"/>. Its content then takes no space below the header.
        /// </summary>
        public const string withoutRowsUssClassName = ussClassName + "--without-rows";

        readonly VisualElement m_Header;

        readonly VisualElement m_Toggle;

        readonly LocalizedTextElement m_Label;

        readonly LocalizedTextElement m_Ellipsis;

        readonly VisualElement m_TrailingContainer;

        readonly VisualElement m_Content;

        PropertyGroupLevel m_Level = PropertyGroupLevel.Sub;

        Pressable m_Clickable;

        bool m_Open = true;

        bool m_Expandable;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_Content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroup"/> class.
        /// </summary>
        public PropertyGroup()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore };
            m_Header.AddToClassList(headerUssClassName);

            m_Toggle = new VisualElement
            {
                name = toggleUssClassName,
                pickingMode = PickingMode.Position,
                focusable = true,
                tabIndex = 0
            };
            m_Toggle.AddToClassList(toggleUssClassName);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            // The ellipsis is the only thing that tells a closed group from an empty one, so it is a
            // real element rather than generated content: USS has no ::after.
            m_Ellipsis = new LocalizedTextElement("…") { name = ellipsisUssClassName, pickingMode = PickingMode.Ignore };
            m_Ellipsis.AddToClassList(ellipsisUssClassName);

            m_Toggle.hierarchy.Add(m_Label);
            m_Toggle.hierarchy.Add(m_Ellipsis);

            m_TrailingContainer = new VisualElement
            {
                name = trailingContainerUssClassName,
                pickingMode = PickingMode.Position
            };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);

            m_Header.hierarchy.Add(m_Toggle);
            m_Header.hierarchy.Add(m_TrailingContainer);

            m_Content = new VisualElement { name = contentUssClassName, pickingMode = PickingMode.Ignore };
            m_Content.AddToClassList(contentUssClassName);

            hierarchy.Add(m_Header);
            hierarchy.Add(m_Content);

            clickable = new Pressable(OnHeaderClicked);
            m_Toggle.AddManipulator(new KeyboardFocusController());

            label = null;
            level = PropertyGroupLevel.Sub;
            open = true;

            // Neither the content nor the trailing slot can report a child being added, so both are
            // re-read on their own layout passes, the way the rest of the package does it.
            m_Content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
            m_TrailingContainer.RegisterCallback<GeometryChangedEvent>(OnTrailingGeometryChanged);
        }

        /// <summary>
        /// The header row, holding the toggle and the trailing slot. Exposed for a host that wants to
        /// mark it as a drop target.
        /// </summary>
        public VisualElement headerElement => m_Header;

        /// <summary>
        /// The header toggle: the press target that folds the group. Exposed for a host's own
        /// hit-testing.
        /// </summary>
        public VisualElement toggleElement => m_Toggle;

        /// <summary>
        /// The slot at the end of the header line, outside the toggle. Content added here does not
        /// inherit the toggle's press target, so a control in it can be clicked without folding the
        /// group.
        /// </summary>
        /// <remarks>
        /// Call <see cref="RefreshTrailing"/> after adding to or removing from it if the state is needed
        /// before the next layout pass.
        /// </remarks>
        public VisualElement trailingContainer => m_TrailingContainer;

        /// <summary>
        /// Event raised when the group has been folded or unfolded, with the new state.
        /// </summary>
        public event Action<bool> openChanged;

        /// <summary>
        /// The Pressable manipulator of the header toggle.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == m_Toggle)
                    m_Toggle.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable != null)
                    m_Toggle.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// The group's title.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// How prominent the group is: a top-level <see cref="PropertyGroupLevel.Section"/> or a nested
        /// <see cref="PropertyGroupLevel.Sub"/> group.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public PropertyGroupLevel level
        {
            get => m_Level;
            set
            {
                var changed = m_Level != value;
                RemoveFromClassList(GetLevelUssClassName(m_Level));
                m_Level = value;
                AddToClassList(GetLevelUssClassName(m_Level));

                if (changed)
                    NotifyPropertyChanged(in levelProperty);
            }
        }

        /// <summary>
        /// Whether the group is showing its rows. Open by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool open
        {
            get => m_Open;
            set
            {
                var changed = m_Open != value;
                SetOpenWithoutNotify(value);

                if (changed)
                {
                    openChanged?.Invoke(m_Open);
                    NotifyPropertyChanged(in openProperty);
                }
            }
        }

        /// <summary>
        /// Whether the group has content laid out elsewhere, rows another element places (a timeline's
        /// gutter, say), so its header toggles and shows the closed-state ellipsis without children of
        /// its own. Without it a group toggles only while it has children. False by default.
        /// </summary>
        /// <remarks>
        /// The group only reports the state through <see cref="open"/> and <see cref="openChanged"/>;
        /// showing and hiding the rows it stands for is the host's job.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool expandable
        {
            get => m_Expandable;
            set
            {
                var changed = m_Expandable != value;
                m_Expandable = value;
                // With no children there is no layout pass of the content to re-read the state on.
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in expandableProperty);
            }
        }

        /// <summary>
        /// Whether the group has anything to show: children of its own, or <see cref="expandable"/>
        /// content laid out elsewhere.
        /// </summary>
        public bool hasContent => m_Expandable || m_Content.childCount > 0;

        /// <summary>
        /// Sets the open state without raising <see cref="openChanged"/>.
        /// </summary>
        /// <param name="newValue"> The new open state. </param>
        public void SetOpenWithoutNotify(bool newValue)
        {
            m_Open = newValue;
            EnableInClassList(openUssClassName, newValue);
            Refresh();
        }

        /// <summary>
        /// Folds the group if it is open, unfolds it otherwise.
        /// </summary>
        public void Toggle() => open = !open;

        /// <summary>
        /// Re-reads whether the group has content and whether it has a trailing slot.
        /// </summary>
        /// <remarks>
        /// This runs on every layout pass of either. Call it directly only after mutating them when the
        /// state is needed before the next pass.
        /// </remarks>
        public void RefreshTrailing() => Refresh();

        void OnHeaderClicked()
        {
            // An empty group has nothing to fold, and a press on it must not look like it did something.
            if (!hasContent)
                return;

            Toggle();
        }

        void OnContentGeometryChanged(GeometryChangedEvent evt) => Refresh();

        void OnTrailingGeometryChanged(GeometryChangedEvent evt) => Refresh();

        void Refresh()
        {
            // The rows are the caller's children, and the row gap is a leading margin on each of them,
            // so the first one has to be tagged for the stylesheet to zero it.
            for (var i = 0; i < m_Content.childCount; i++)
                m_Content[i].EnableInClassList(Styles.firstChildUssClassName, i == 0);

            var empty = !hasContent;
            EnableInClassList(emptyUssClassName, empty);
            EnableInClassList(withoutRowsUssClassName, m_Content.childCount == 0);
            EnableInClassList(withTrailingUssClassName, m_TrailingContainer.childCount > 0);
            m_Toggle.SetEnabled(!empty);
            // The ellipsis stands in for the rows that are folded away, so an empty group shows none.
            m_Ellipsis.EnableInClassList(Styles.hiddenUssClassName, m_Open || empty);
            m_Content.EnableInClassList(Styles.hiddenUssClassName, !m_Open);
        }
    }

    /// <summary>
    /// The column an inspector stacks its <see cref="PropertyGroup"/> sections in, spacing them by
    /// <c>--appui-alias-inspector-section-gap</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// UI Toolkit has no <c>gap</c>, so the spacing is a leading margin on every child with the first
    /// one tagged <see cref="Styles.firstChildUssClassName"/>. The tag is refreshed on every layout
    /// pass, so adding, inserting and removing sections all keep the rhythm right.
    /// </para>
    /// </remarks>
    [VisualDocPage("inputs")]
    [UxmlElement]
    public partial class PropertyGroupStack : BaseVisualElement
    {
        /// <summary>
        /// The PropertyGroupStack main styling class.
        /// </summary>
        public const string ussClassName = "appui-property-group-stack";

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGroupStack"/> class.
        /// </summary>
        public PropertyGroupStack()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt) => RefreshChildMarkers();

        void OnGeometryChanged(GeometryChangedEvent evt) => RefreshChildMarkers();

        /// <summary>
        /// Re-tags the first section of the stack so it renders without a leading gap.
        /// </summary>
        /// <remarks>
        /// This runs automatically on layout. Call it directly only if the children are mutated and the
        /// markers are needed before the next layout pass.
        /// </remarks>
        public void RefreshChildMarkers()
        {
            for (var i = 0; i < childCount; i++)
                this[i].EnableInClassList(Styles.firstChildUssClassName, i == 0);
        }
    }
}
