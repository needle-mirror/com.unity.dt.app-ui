using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    record CollapsedItemContext : IContext { }

    /// <summary>
    /// The disclosure glyph every <see cref="TreeViewItem"/> under a <see cref="TreeList"/> draws.
    /// Provided by the tree so a row added later picks it up too.
    /// </summary>
    class CaretIconContext : IContext
    {
        public CaretIconContext(string iconName)
        {
            this.iconName = iconName;
        }

        /// <summary>
        /// The icon name, or null for the default caret.
        /// </summary>
        public string iconName { get; }
    }

    /// <summary>
    /// Which side of the row the expand/collapse caret sits on.
    /// </summary>
    public enum CaretPosition
    {
        /// <summary>
        /// The caret sits in the leading gutter, before the icon and the label. This is the default
        /// and matches nested rows column-aligning their icons.
        /// </summary>
        Start,

        /// <summary>
        /// The caret sits after the label, at the trailing edge of the row. Used by the big
        /// top-level scope/section rows.
        /// </summary>
        End,
    }

    /// <summary>
    /// A tree view item that can be used directly in a <see cref="ScrollView"/> or any vertical container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A row is composed of, in order: the caret gutter (always reserved, even on a leaf, so icons and
    /// labels line up in a column), an optional <see cref="icon"/>, the label, and an optional
    /// <see cref="trailingContainer"/> pinned to the right edge of the row.
    /// </para>
    /// <para>
    /// The trailing strip overlays the row. While it is visible — on hover, or permanently when
    /// <see cref="trailingVisible"/> is true — the label reserves exactly the strip's measured width so
    /// text stops before the actions instead of running under them. When the row becomes too narrow to
    /// keep a readable label next to the actions, the strip collapses to a single overflow button that
    /// reopens the very same trailing content in a <see cref="Popover"/>. Both thresholds are measured
    /// from <see cref="GeometryChangedEvent"/>, and the minimum readable label width comes from a
    /// token-driven probe element so it rescales with density.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:TreeViewItem label="Assets" icon="folder" show-caret="true" expanded="true">
    ///         <ui:TreeViewItem label="Scenes" depth="1" caret-position="End" />
    ///     </ui:TreeViewItem>
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TreeViewItem : VisualElement
    {
        internal static readonly BindingId dropIndicatorProperty = nameof(dropIndicator);

        internal static readonly BindingId showCaretProperty = nameof(showCaret);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId expandedProperty = nameof(expanded);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId depthProperty = nameof(depth);

        internal static readonly BindingId draggableProperty = nameof(draggable);

        internal static readonly BindingId trailingVisibleProperty = nameof(trailingVisible);

        internal static readonly BindingId caretPositionProperty = nameof(caretPosition);

        internal static readonly BindingId labelBadgeProperty = nameof(labelBadge);

        internal static readonly BindingId labelBadgeLabelProperty = nameof(labelBadgeLabel);

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string ussClassName = "appui-tree-view-item";

        /// <summary>
        /// The USS class name for the header of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string headerUssClassName = "appui-tree-view-item__header";

        /// <summary>
        /// The USS class name for the content viewport of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string contentViewportUssClassName = "appui-tree-view-item__content-viewport";

        /// <summary>
        /// The USS class name for the content container of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string contentContainerUssClassName = "appui-tree-view-item__content-container";

        /// <summary>
        /// The USS class name for the title label of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string titleLabelUssClassName = "appui-tree-view-item__title-label";

        /// <summary>
        /// The USS class name for the badge that follows the label of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string labelBadgeUssClassName = "appui-tree-view-item__label-badge";

        /// <summary>
        /// The USS class name for the leading icon of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string iconUssClassName = "appui-tree-view-item__icon";

        /// <summary>
        /// The USS class name for the always-reserved leading caret gutter of the
        /// <see cref="TreeViewItem"/>.
        /// </summary>
        public const string caretGutterUssClassName = "appui-tree-view-item__caret-gutter";

        /// <summary>
        /// The USS class name for the caret button of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string caretButtonUssClassName = "appui-tree-view-item__caret-button";

        /// <summary>
        /// The USS class name for the caret icon of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string caretIconUssClassName = "appui-tree-view-item__caret-icon";

        /// <summary>
        /// The USS class name for the trailing container of the <see cref="TreeViewItem"/>.
        /// </summary>
        public const string trailingContainerUssClassName = "appui-tree-view-item__trailing-container";

        /// <summary>
        /// The USS class name for the overflow button the trailing strip collapses to on a narrow row.
        /// </summary>
        public const string overflowButtonUssClassName = "appui-tree-view-item__overflow-button";

        /// <summary>
        /// The USS class name for the popover content hosting the trailing strip while it is collapsed.
        /// </summary>
        public const string overflowPopoverUssClassName = "appui-tree-view-item__overflow-popover";

        /// <summary>
        /// The USS class name for the hidden probe carrying the minimum readable label width.
        /// </summary>
        public const string minLabelProbeUssClassName = "appui-tree-view-item__min-label-probe";

        /// <summary>
        /// The USS class name for the drop indicator shown above the row.
        /// </summary>
        public const string dropIndicatorBeforeUssClassName = "appui-tree-view-item__drop-indicator-before";

        /// <summary>
        /// The USS class name for the drop indicator shown below the row.
        /// </summary>
        public const string dropIndicatorAfterUssClassName = "appui-tree-view-item__drop-indicator-after";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it has a caret.
        /// </summary>
        public const string withCaretVariantUssClassName = "appui-tree-view-item--with-caret";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it has a leading icon.
        /// </summary>
        public const string withIconVariantUssClassName = "appui-tree-view-item--with-icon";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it has a label badge.
        /// </summary>
        public const string withLabelBadgeVariantUssClassName = "appui-tree-view-item--with-label-badge";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it has trailing content.
        /// </summary>
        public const string withTrailingVariantUssClassName = "appui-tree-view-item--with-trailing";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> while its trailing content is showing.
        /// </summary>
        public const string trailingShownVariantUssClassName = "appui-tree-view-item--trailing-shown";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> while its trailing strip is collapsed
        /// into the overflow button.
        /// </summary>
        public const string trailingCollapsedVariantUssClassName = "appui-tree-view-item--trailing-collapsed";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is expanded.
        /// </summary>
        public const string expandedVariantUssClassName = "appui-tree-view-item--expanded";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is interactable (has a clickable header).
        /// </summary>
        public const string interactableVariantUssClassName = "appui-tree-view-item--interactable";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> when it is selected.
        /// </summary>
        public const string selectedVariantUssClassName = "appui-tree-view-item--selected";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> while it is the row being dragged.
        /// </summary>
        public const string draggingVariantUssClassName = "appui-tree-view-item--dragging";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> based on its caret position.
        /// </summary>
        [EnumName("GetCaretPositionUssClassName", typeof(CaretPosition))]
        public const string caretPositionUssClassName = "appui-tree-view-item--caret-";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> based on the active drop position.
        /// </summary>
        [EnumName("GetDropPositionUssClassName", typeof(DropPosition))]
        public const string dropPositionUssClassName = "appui-tree-view-item--drop-";

        /// <summary>
        /// The USS class name for the <see cref="TreeViewItem"/> based on its depth.
        /// </summary>
        public const string depthVariantUssClassName = "appui-tree-view-item--depth-";

        const string k_DefaultCaretIconName = "caret-down";

        readonly VisualElement m_ContentContainer;

        readonly VisualElement m_ContentViewport;

        readonly VisualElement m_HeaderContainer;

        readonly VisualElement m_CaretGutter;

        readonly VisualElement m_CaretButton;

        readonly Icon m_CaretIcon;

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_LabelElement;

        readonly Icon m_LabelBadge;

        readonly VisualElement m_TrailingContainer;

        readonly IconButton m_OverflowButton;

        readonly VisualElement m_MinLabelProbe;

        readonly VisualElement m_DropIndicatorBefore;

        readonly VisualElement m_DropIndicatorAfter;

        readonly KeyboardFocusController m_KeyboardFocusController = new ();

        Pressable m_Clickable;

        Pressable m_CaretClickable;

        float m_ContentHeight;

        int m_Depth;

        CaretPosition m_CaretPosition;

        DropPosition? m_DropPosition;

        Optional<DropPosition> m_DropIndicator;

        DropPosition? m_AppliedDropIndicator;

        bool m_Draggable = true;

        bool m_TrailingVisible;

        bool m_Hovered;

        bool m_TrailingCollapsed;

        float m_TrailingWidth;

        float m_OverflowWidth;

        float m_MinLabelWidth;

        Popover m_OverflowPopover;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_ContentContainer;

        /// <summary>
        /// Gets or sets whether the caret is shown for this <see cref="TreeViewItem"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showCaret
        {
            get => ClassListContains(withCaretVariantUssClassName);
            set
            {
                var changed = showCaret != value;
                if (value)
                    AddToClassList(withCaretVariantUssClassName);
                else
                    RemoveFromClassList(withCaretVariantUssClassName);
                if (changed)
                    NotifyPropertyChanged(in showCaretProperty);
            }
        }

        /// <summary>
        /// Gets or sets the label of the <see cref="TreeViewItem"/>.
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
        /// The label element itself. Exposed so callers can style a distinguished row by adding their
        /// own USS class to it — the UITK equivalent of the registry's <c>labelClassName</c>.
        /// </summary>
        public LocalizedTextElement labelElement => m_LabelElement;

        /// <summary>
        /// Gets or sets the name of a small glyph that belongs to the name — a scene's startup star, a
        /// dirty marker. Null or empty hides it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The badge sits immediately after the label text rather than at the row's end, so it follows
        /// the name's length; <see cref="trailingContainer"/> is the slot that pins to the edge. The
        /// label ellipsizes before the badge does, and while the trailing strip is showing the badge
        /// stops short of it exactly as the label does.
        /// </para>
        /// <para>
        /// This takes an icon name rather than an element so it is expressible in UXML. Use
        /// <see cref="labelBadgeElement"/> to set a variant or a sprite directly.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string labelBadge
        {
            get => m_LabelBadge.iconName;
            set
            {
                var changed = m_LabelBadge.iconName != value;
                m_LabelBadge.iconName = value;
                var hasBadge = !string.IsNullOrEmpty(value);
                EnableInClassList(withLabelBadgeVariantUssClassName, hasBadge);
                m_LabelBadge.EnableInClassList(Styles.hiddenUssClassName, !hasBadge);
                // The reserve that keeps text clear of the trailing strip moves between the label and
                // the badge, whichever is last on the name line.
                UpdateTrailingLayout();
                if (changed)
                    NotifyPropertyChanged(in labelBadgeProperty);
            }
        }

        /// <summary>
        /// Gets or sets what <see cref="labelBadge"/> means, in words, as the badge's tooltip.
        /// </summary>
        /// <remarks>
        /// A glyph on its own says nothing to someone who has not seen it before, and the badge is too
        /// small to label in place. Text rather than an element: the tooltip is a string, and a caller
        /// should not have to build one.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string labelBadgeLabel
        {
            get => m_LabelBadge.tooltip;
            set
            {
                var changed = m_LabelBadge.tooltip != value;
                m_LabelBadge.tooltip = value;
                // A tooltip only arrives on an element the pointer can hit.
                m_LabelBadge.pickingMode = string.IsNullOrEmpty(value)
                    ? PickingMode.Ignore
                    : PickingMode.Position;
                if (changed)
                    NotifyPropertyChanged(in labelBadgeLabelProperty);
            }
        }

        /// <summary>
        /// The badge element itself, for callers that need to set a variant or a sprite directly.
        /// </summary>
        public Icon labelBadgeElement => m_LabelBadge;

        /// <summary>
        /// The caret glyph element. The tree sets its icon for every row through
        /// <see cref="TreeList.caretIcon"/>; this is the per-row escape hatch.
        /// </summary>
        public Icon caretIconElement => m_CaretIcon;

        /// <summary>
        /// Gets or sets the name of the leading icon shown between the caret gutter and the label.
        /// Null or empty hides the icon slot entirely.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                var changed = m_Icon.iconName != value;
                m_Icon.iconName = value;
                var hasIcon = !string.IsNullOrEmpty(value);
                EnableInClassList(withIconVariantUssClassName, hasIcon);
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, !hasIcon);
                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The leading icon element, for callers that need to set a variant or a sprite directly.
        /// </summary>
        public Icon iconElement => m_Icon;

        /// <summary>
        /// Gets or sets whether the <see cref="TreeViewItem"/> is expanded or collapsed.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool expanded
        {
            get => ClassListContains(expandedVariantUssClassName);
            set
            {
                var changed = expanded != value;
                if (value)
                    AddToClassList(expandedVariantUssClassName);
                else
                    RemoveFromClassList(expandedVariantUssClassName);
                UpdateViewportHeight();
                PropagateCollapse();
                if (changed)
                    NotifyPropertyChanged(in expandedProperty);
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="TreeViewItem"/> is in selected state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => ClassListContains(selectedVariantUssClassName);
            set
            {
                var changed = selected != value;
                if (value)
                    AddToClassList(selectedVariantUssClassName);
                else
                    RemoveFromClassList(selectedVariantUssClassName);
                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// Gets or sets the depth of the <see cref="TreeViewItem"/> in the tree view hierarchy.
        /// This is used to determine the indentation level of the item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int depth
        {
            get => m_Depth;
            set
            {
                var changed = m_Depth != value;
                RemoveFromClassList(MemoryUtils.Concatenate(depthVariantUssClassName, m_Depth.ToString()));
                m_Depth = value;
                AddToClassList(MemoryUtils.Concatenate(depthVariantUssClassName, m_Depth.ToString()));
                if (changed)
                    NotifyPropertyChanged(in depthProperty);
            }
        }

        /// <summary>
        /// Which side of the row the expand/collapse caret sits on. Defaults to
        /// <see cref="CaretPosition.Start"/>, matching the registry.
        /// </summary>
        /// <remarks>
        /// The leading caret gutter is reserved on every row, caret or not, so icons and labels stay
        /// column-aligned; <see cref="CaretPosition.End"/> keeps that empty gutter and moves the caret
        /// itself after the label.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public CaretPosition caretPosition
        {
            get => m_CaretPosition;
            set
            {
                var changed = m_CaretPosition != value;
                RemoveFromClassList(GetCaretPositionUssClassName(m_CaretPosition));
                m_CaretPosition = value;
                AddToClassList(GetCaretPositionUssClassName(m_CaretPosition));
                PlaceCaret();
                if (changed)
                    NotifyPropertyChanged(in caretPositionProperty);
            }
        }

        /// <summary>
        /// Whether this specific item can be picked up by a drag. Only meaningful when the owning
        /// <see cref="TreeList"/> has a <see cref="TreeList.onReparent"/> handler; a non-draggable item
        /// is still a valid drop target.
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
                if (changed)
                    NotifyPropertyChanged(in draggableProperty);
            }
        }

        /// <summary>
        /// The container for content pinned to the right of the row — visibility toggles, a count
        /// badge, and so on. Add your own elements to it; the row handles the reveal, the label reserve
        /// and the narrow-row overflow on its own.
        /// </summary>
        public VisualElement trailingContainer => m_TrailingContainer;

        /// <summary>
        /// Tells the row that some trailing control is permanently visible rather than revealed on
        /// hover — an active lock or hidden-eye toggle, for instance — so the label keeps reserving room
        /// for it even when the row is not hovered.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool trailingVisible
        {
            get => m_TrailingVisible;
            set
            {
                var changed = m_TrailingVisible != value;
                m_TrailingVisible = value;
                UpdateTrailingLayout();
                if (changed)
                    NotifyPropertyChanged(in trailingVisibleProperty);
            }
        }

        /// <summary>
        /// True while the trailing strip has collapsed into the overflow button because the row is too
        /// narrow to show a readable label next to it.
        /// </summary>
        public bool trailingCollapsed => m_TrailingCollapsed;

        /// <summary>
        /// The button the trailing strip collapses to on a narrow row.
        /// </summary>
        public IconButton overflowButton => m_OverflowButton;

        /// <summary>
        /// The active drop position for this row while a drag hovers it, or null when it is not the
        /// current drop target. Set by the owning <see cref="TreeList"/>.
        /// </summary>
        public DropPosition? dropPosition
        {
            get => m_DropPosition;
            internal set
            {
                if (m_DropPosition == value)
                    return;

                m_DropPosition = value;
                RefreshDropIndicator();
            }
        }

        /// <summary>
        /// Draw this row's drop indicator from the outside, for a drag the tree is not running
        /// itself.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A pointer-driven drag produces none of the events the tree's own drag handling watches
        /// for, so a tree whose rows are targets for one has no way to reach the built-in
        /// indicators. Feeding the position in keeps such a drop looking exactly like a native one
        /// instead of growing a second, divergent set of highlight styles.
        /// </para>
        /// <para>
        /// Ignored while the tree's own drag is targeting this row, so the two can never contradict
        /// each other. Unset draws nothing, so a tree that never sees an outside drag is
        /// unaffected.
        /// </para>
        /// </remarks>
        [CreateProperty]
        public Optional<DropPosition> dropIndicator
        {
            get => m_DropIndicator;
            set
            {
                if (m_DropIndicator == value)
                    return;

                m_DropIndicator = value;
                RefreshDropIndicator();
                NotifyPropertyChanged(in dropIndicatorProperty);
            }
        }

        /// <summary>
        /// The indicator actually showing: the tree's own drop target wins, then the host-driven
        /// <see cref="dropIndicator"/>.
        /// </summary>
        DropPosition? effectiveDropIndicator =>
            m_DropPosition ?? (m_DropIndicator.IsSet ? m_DropIndicator.Value : null);

        void RefreshDropIndicator()
        {
            var next = effectiveDropIndicator;
            if (m_AppliedDropIndicator == next)
                return;

            if (m_AppliedDropIndicator.HasValue)
                RemoveFromClassList(GetDropPositionUssClassName(m_AppliedDropIndicator.Value));
            m_AppliedDropIndicator = next;
            if (m_AppliedDropIndicator.HasValue)
                AddToClassList(GetDropPositionUssClassName(m_AppliedDropIndicator.Value));
        }

        /// <summary>
        /// True while this row is the one being dragged. Set by the owning <see cref="TreeList"/>.
        /// </summary>
        public bool dragging
        {
            get => ClassListContains(draggingVariantUssClassName);
            internal set => EnableInClassList(draggingVariantUssClassName, value);
        }

        /// <summary>
        /// The row header, the element that carries the row's height, hover and selected styling.
        /// </summary>
        internal VisualElement header => m_HeaderContainer;

        /// <summary>
        /// Gets or sets the clickable behavior for the header of the <see cref="TreeViewItem"/>.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                m_HeaderContainer.RemoveManipulator(m_KeyboardFocusController);
                if (m_Clickable != null && m_Clickable.target == m_HeaderContainer)
                    m_HeaderContainer.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable != null)
                {
                    m_HeaderContainer.AddManipulator(m_Clickable);
                    m_HeaderContainer.AddManipulator(m_KeyboardFocusController);
                }
                EnableInClassList(interactableVariantUssClassName, m_Clickable != null);
            }
        }

        /// <summary>
        /// Gets or sets the clickable behavior for the caret button of the <see cref="TreeViewItem"/>.
        /// </summary>
        public Pressable caretClickable
        {
            get => m_CaretClickable;
            set
            {
                if (m_CaretClickable != null && m_CaretClickable.target == m_CaretButton)
                    m_CaretButton.RemoveManipulator(m_CaretClickable);
                m_CaretClickable = value;
                if (m_CaretClickable != null)
                    m_CaretButton.AddManipulator(m_CaretClickable);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewItem"/> class.
        /// </summary>
        public TreeViewItem()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_HeaderContainer = new VisualElement
            {
                pickingMode = PickingMode.Position,
                focusable = true,
                name = headerUssClassName
            };
            m_HeaderContainer.AddToClassList(headerUssClassName);
            m_ContentViewport = new VisualElement { pickingMode = PickingMode.Ignore, name = contentViewportUssClassName };
            m_ContentViewport.AddToClassList(contentViewportUssClassName);
            m_ContentContainer = new VisualElement {pickingMode = PickingMode.Ignore, name = contentContainerUssClassName};
            m_ContentContainer.AddToClassList(contentContainerUssClassName);
            m_LabelElement = new LocalizedTextElement { pickingMode = PickingMode.Ignore, name = titleLabelUssClassName };
            m_LabelElement.AddToClassList(titleLabelUssClassName);
            m_Icon = new Icon { pickingMode = PickingMode.Ignore, name = iconUssClassName };
            m_Icon.AddToClassList(iconUssClassName);
            m_CaretButton = new VisualElement { pickingMode = PickingMode.Position, name = caretButtonUssClassName };
            m_CaretButton.AddToClassList(caretButtonUssClassName);
            m_CaretIcon = new Icon
            {
                pickingMode = PickingMode.Ignore,
                name = caretIconUssClassName,
                iconName = k_DefaultCaretIconName,
                primary = true,
                size = IconSize.Sm
            };
            m_CaretIcon.AddToClassList(caretIconUssClassName);

            m_CaretButton.Add(m_CaretIcon);

            m_CaretGutter = new VisualElement { pickingMode = PickingMode.Ignore, name = caretGutterUssClassName };
            m_CaretGutter.AddToClassList(caretGutterUssClassName);

            m_LabelBadge = new Icon
            {
                pickingMode = PickingMode.Ignore,
                name = labelBadgeUssClassName,
                iconName = null,
                primary = true,
                size = IconSize.Xs
            };
            m_LabelBadge.AddToClassList(labelBadgeUssClassName);

            m_HeaderContainer.Add(m_CaretGutter);
            m_HeaderContainer.Add(m_Icon);
            m_HeaderContainer.Add(m_LabelElement);
            m_HeaderContainer.Add(m_LabelBadge);

            m_TrailingContainer = new VisualElement { pickingMode = PickingMode.Position, name = trailingContainerUssClassName };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);
            m_HeaderContainer.Add(m_TrailingContainer);

            m_OverflowButton = new IconButton { icon = "dots-three-vertical", quiet = true, name = overflowButtonUssClassName };
            m_OverflowButton.AddToClassList(overflowButtonUssClassName);
            m_OverflowButton.clickable.clicked += OpenOverflowPopover;
            m_HeaderContainer.Add(m_OverflowButton);

            m_MinLabelProbe = new VisualElement { pickingMode = PickingMode.Ignore, name = minLabelProbeUssClassName };
            m_MinLabelProbe.AddToClassList(minLabelProbeUssClassName);
            m_HeaderContainer.Add(m_MinLabelProbe);

            m_DropIndicatorBefore = new VisualElement { pickingMode = PickingMode.Ignore, name = dropIndicatorBeforeUssClassName };
            m_DropIndicatorBefore.AddToClassList(dropIndicatorBeforeUssClassName);
            m_DropIndicatorAfter = new VisualElement { pickingMode = PickingMode.Ignore, name = dropIndicatorAfterUssClassName };
            m_DropIndicatorAfter.AddToClassList(dropIndicatorAfterUssClassName);

            m_ContentViewport.Add(m_ContentContainer);

            hierarchy.Add(m_HeaderContainer);
            hierarchy.Add(m_DropIndicatorBefore);
            hierarchy.Add(m_ContentViewport);
            hierarchy.Add(m_DropIndicatorAfter);

            showCaret = false;
            expanded = false;
            selected = false;
            depth = 0;
            label = string.Empty;
            icon = null;
            labelBadge = null;
            labelBadgeLabel = null;
            caretPosition = CaretPosition.Start;
            trailingVisible = false;

            m_ContentContainer.RegisterCallback<GeometryChangedEvent>(OnContentContainerGeometryChanged);
            m_HeaderContainer.RegisterCallback<GeometryChangedEvent>(OnHeaderGeometryChanged);
            m_TrailingContainer.RegisterCallback<GeometryChangedEvent>(OnTrailingGeometryChanged);
            m_OverflowButton.RegisterCallback<GeometryChangedEvent>(OnOverflowGeometryChanged);
            m_MinLabelProbe.RegisterCallback<GeometryChangedEvent>(OnMinLabelProbeGeometryChanged);
            m_HeaderContainer.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            m_HeaderContainer.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            this.RegisterContextChangedCallback<CollapsedItemContext>(OnCollapsedItemContextChanged);
            this.RegisterContextChangedCallback<CaretIconContext>(OnCaretIconContextChanged);

            UpdateTrailingLayout();
        }

        /// <summary>
        /// Whether the row reports expansion: it has nested rows, or it shows a caret for children laid
        /// out elsewhere or not loaded yet.
        /// </summary>
        internal bool isExpandable => showCaret || childCount > 0;

        void OnKeyDown(KeyDownEvent evt)
        {
            // Only keys on this row's own header: a nested row's keys bubble through here too.
            if (evt.target != m_HeaderContainer || !isExpandable || HasKeyboardRoot())
                return;

            // No tree root navigates these rows, so the row opens and closes itself, the way the
            // registry's rows do under a TreeViewProvider.
            if (evt.keyCode == KeyCode.RightArrow && !expanded)
                expanded = true;
            else if (evt.keyCode == KeyCode.LeftArrow && expanded)
                expanded = false;
            else
                return;

            evt.StopPropagation();
        }

        bool HasKeyboardRoot()
        {
            for (var p = hierarchy.parent; p != null; p = p.hierarchy.parent)
            {
                if (p is TreeList tree && tree.keyboardNavigation)
                    return true;
                if (p is TreeListVirtual)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Toggles the expanded state of the <see cref="TreeViewItem"/>.
        /// </summary>
        public void ToggleExpand()
        {
            expanded = !expanded;
        }

        // ── Caret placement ─────────────────────────────────────────────────────

        void PlaceCaret()
        {
            // The caret button always exists; only its slot in the header row moves. The leading
            // gutter is a separate, always-reserved box in USS, so an end caret still leaves the
            // icons of every row aligned in the same column.
            m_CaretButton.RemoveFromHierarchy();
            if (m_CaretPosition == CaretPosition.Start)
                m_CaretGutter.Add(m_CaretButton);
            else
                m_HeaderContainer.Insert(m_HeaderContainer.IndexOf(m_LabelElement) + 1, m_CaretButton);
        }

        // ── Trailing strip ──────────────────────────────────────────────────────

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_Hovered = true;
            UpdateTrailingLayout();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            m_Hovered = false;
            UpdateTrailingLayout();
        }

        void OnHeaderGeometryChanged(GeometryChangedEvent evt) => UpdateTrailingLayout();

        void OnTrailingGeometryChanged(GeometryChangedEvent evt)
        {
            // Keep the last non-zero width: while the strip is collapsed it is hidden and measures 0,
            // and the row still needs the width it *would* take to know when it can un-collapse.
            var w = evt.newRect.width;
            if (w > 0 && !Mathf.Approximately(w, m_TrailingWidth))
            {
                m_TrailingWidth = w;
                UpdateTrailingLayout();
            }
        }

        void OnOverflowGeometryChanged(GeometryChangedEvent evt)
        {
            var w = evt.newRect.width;
            if (w > 0 && !Mathf.Approximately(w, m_OverflowWidth))
            {
                m_OverflowWidth = w;
                UpdateTrailingLayout();
            }
        }

        void OnMinLabelProbeGeometryChanged(GeometryChangedEvent evt)
        {
            var w = evt.newRect.width;
            if (!Mathf.Approximately(w, m_MinLabelWidth))
            {
                m_MinLabelWidth = w;
                UpdateTrailingLayout();
            }
        }

        /// <summary>
        /// Re-evaluates whether the trailing strip is shown, whether it has to collapse into the
        /// overflow button, and how much room the label reserves for it.
        /// </summary>
        /// <remarks>
        /// Call this after adding to or removing from <see cref="trailingContainer"/> from code; the
        /// row cannot observe child additions on its own.
        /// </remarks>
        public void RefreshTrailing() => UpdateTrailingLayout();

        void UpdateTrailingLayout()
        {
            var hasTrailing = m_TrailingContainer != null && m_TrailingContainer.childCount > 0;
            EnableInClassList(withTrailingVariantUssClassName, hasTrailing);

            if (!hasTrailing)
            {
                m_TrailingCollapsed = false;
                EnableInClassList(trailingCollapsedVariantUssClassName, false);
                EnableInClassList(trailingShownVariantUssClassName, false);
                m_LabelElement.style.marginRight = 0f;
                m_LabelBadge.style.marginRight = 0f;
                return;
            }

            var headerWidth = m_HeaderContainer.resolvedStyle.width;
            var collapsed = m_TrailingWidth > 0f
                && headerWidth > 0f
                && headerWidth - m_TrailingWidth < m_MinLabelWidth;

            if (collapsed != m_TrailingCollapsed)
            {
                m_TrailingCollapsed = collapsed;
                EnableInClassList(trailingCollapsedVariantUssClassName, collapsed);
                if (!collapsed)
                    CloseOverflowPopover();
            }

            // Collapsed rows always show the overflow button, so the label reserve applies whether or
            // not the row is hovered.
            var shown = m_TrailingVisible || m_Hovered || collapsed;
            EnableInClassList(trailingShownVariantUssClassName, shown);

            // The strip overlays the header, so the name line stops short of it by exactly the measured
            // width instead of running underneath. The reserve goes on whichever of the two is last on
            // that line: put it on the label while a badge follows and the margin would push the badge
            // under the strip, which is the one thing the reserve exists to prevent.
            var reserve = shown ? (collapsed ? m_OverflowWidth : m_TrailingWidth) : 0f;
            var hasBadge = !string.IsNullOrEmpty(m_LabelBadge.iconName);
            m_LabelElement.style.marginRight = hasBadge ? 0f : reserve;
            m_LabelBadge.style.marginRight = hasBadge ? reserve : 0f;
        }

        void OpenOverflowPopover()
        {
            if (m_OverflowPopover != null || m_TrailingContainer.childCount == 0)
                return;

            var content = new VisualElement { name = overflowPopoverUssClassName };
            content.AddToClassList(overflowPopoverUssClassName);

            // Reparent the very same controls rather than cloning them, so their state and their
            // callbacks survive the trip into the popover.
            while (m_TrailingContainer.childCount > 0)
            {
                var child = m_TrailingContainer[0];
                child.RemoveFromHierarchy();
                content.Add(child);
            }

            m_OverflowPopover = Popover.Build(m_OverflowButton, content)
                .SetPlacement(PopoverPlacement.BottomEnd)
                .SetOutsideClickDismiss(true);
            m_OverflowPopover.dismissed += (_, _) => ReclaimOverflowContent(content);
            m_OverflowPopover.Show();
        }

        void CloseOverflowPopover()
        {
            m_OverflowPopover?.Dismiss(DismissType.Manual);
        }

        void ReclaimOverflowContent(VisualElement content)
        {
            while (content.childCount > 0)
            {
                var child = content[0];
                child.RemoveFromHierarchy();
                m_TrailingContainer.Add(child);
            }

            m_OverflowPopover = null;
            UpdateTrailingLayout();
        }

        // ── Expansion plumbing ──────────────────────────────────────────────────

        void OnContentContainerGeometryChanged(GeometryChangedEvent evt)
        {
            if (float.IsNaN(evt.newRect.height) || Mathf.Approximately(evt.newRect.height, m_ContentHeight))
                return;

            m_ContentHeight = evt.newRect.height;
            UpdateViewportHeight();
        }

        void UpdateViewportHeight()
        {
            m_ContentViewport.style.height = expanded ? m_ContentHeight : 0;
        }

        void PropagateCollapse()
        {
            if (!expanded)
            {
                this.ProvideContext(new CollapsedItemContext());
                schedule.Execute(() => this.ProvideContext<CollapsedItemContext>(null));
            }
        }

        void OnCaretIconContextChanged(ContextChangedEvent<CaretIconContext> evt)
        {
            // No context, or a tree that cleared its override, both mean the default caret.
            var iconName = evt.context?.iconName;
            m_CaretIcon.iconName = string.IsNullOrEmpty(iconName) ? k_DefaultCaretIconName : iconName;
        }

        void OnCollapsedItemContextChanged(ContextChangedEvent<CollapsedItemContext> evt)
        {
            if (evt.context == null)
                return;

            if (expanded)
                expanded = false;
        }

    }
}
