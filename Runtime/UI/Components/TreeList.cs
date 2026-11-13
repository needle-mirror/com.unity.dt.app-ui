using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Where a dragged row lands relative to the row it was dropped on.
    /// </summary>
    public enum DropPosition
    {
        /// <summary>
        /// The dragged row becomes the target's previous sibling.
        /// </summary>
        Before,

        /// <summary>
        /// The dragged row becomes the target's next sibling.
        /// </summary>
        After,

        /// <summary>
        /// The dragged row becomes a child of the target.
        /// </summary>
        Inside,
    }

    /// <summary>
    /// What a <see cref="TreeList"/> drag carries.
    /// </summary>
    /// <remarks>
    /// UITK drags do not carry a MIME-typed data transfer the way the DOM does, so the web
    /// implementation's private <c>application/x-appui-tree-item</c> type is expressed here as a
    /// typed payload instead. A foreign drop target that is handed one of these can identify a tree
    /// row by its type — or by <see cref="mimeType"/> when it bridges to a string-keyed protocol —
    /// rather than guessing from a bare id string.
    /// </remarks>
    public sealed class TreeListDragPayload
    {
        /// <summary>
        /// The MIME type a tree-row drag publishes itself under, mirroring the web registry's
        /// <c>TREE_ITEM_MIME</c>.
        /// </summary>
        public string mimeType => TreeList.treeItemMimeType;

        /// <summary>
        /// The tree the drag started in.
        /// </summary>
        public TreeList source { get; }

        /// <summary>
        /// The rows being dragged, in selection priority order. Index 0 is the row the pointer
        /// grabbed.
        /// </summary>
        public IReadOnlyList<TreeViewItem> items { get; }

        internal TreeListDragPayload(TreeList source, IReadOnlyList<TreeViewItem> items)
        {
            this.source = source;
            this.items = items;
        }
    }

    /// <summary>
    /// A container that turns a hierarchy of <see cref="TreeViewItem"/> elements into a single
    /// navigable, selectable tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TreeList is a <b>composition</b> container, not a data-source collection view: you build the
    /// hierarchy by nesting <see cref="TreeViewItem"/> elements, exactly the way the App UI web
    /// registry composes <c>&lt;TreeList&gt;</c> and <c>&lt;TreeViewItem&gt;</c>. The container owns
    /// everything that is a property of the tree as a whole — selection, keyboard navigation, caret
    /// clicks and sizing — while each item owns its own label, caret and expansion state.
    /// </para>
    /// <para>
    /// This deliberately does <b>not</b> wrap <see cref="UnityEngine.UIElements.TreeView"/>. App UI
    /// answers this question the same way for every collection surface it ships: see
    /// <see cref="BaseGridView"/>, which is written from scratch on top of <c>BindableElement</c>
    /// rather than deriving from <c>BaseVerticalCollectionView</c>, and borrows only the
    /// <see cref="SelectionType"/> vocabulary from UI Toolkit. The built-in TreeView is a virtualised,
    /// data-bound view built around <c>TreeViewItemData</c> and its own item templates, which cannot
    /// express the registry's composition API or App UI's styling contract. Following the established
    /// pattern keeps the App UI collection family consistent. It is named TreeList, not TreeView, so
    /// it never shadows the UI Toolkit type in a file that imports both namespaces.
    /// </para>
    /// <para>
    /// Selection semantics follow <see cref="SelectionType"/>:
    /// <list type="bullet">
    /// <item><description><see cref="SelectionType.None"/> — the tree never selects anything.</description></item>
    /// <item><description><see cref="SelectionType.Single"/> — one item at a time; modifiers are ignored.</description></item>
    /// <item><description><see cref="SelectionType.Multiple"/> — the action key (Cmd on macOS, Ctrl elsewhere)
    /// toggles an item, Shift extends a range from the anchor over the <see cref="visibleItems"/> order,
    /// and action+Shift unions the range with the current selection. A modified press applies at
    /// once; a plain press changes nothing until the release, and only a release that ends a click
    /// (the pointer travelled less than 4px) selects the row. A press that becomes a drag leaves the
    /// selection as it was, so a group stays whole for a group drag.</description></item>
    /// </list>
    /// <see cref="selectedItems"/> is ordered by priority: index 0 is the <see cref="selectedItem"/>
    /// primary, the item the user interacted with last, which is also the row keyboard navigation
    /// moves from.
    /// </para>
    /// <para>
    /// Three behaviours are opt-in through a handler, and are completely inert until one is set, the
    /// same way the registry gates them:
    /// <list type="bullet">
    /// <item><description><see cref="onReparent"/> turns on drag-to-reorder. The tree resolves the
    /// drop target and its <see cref="DropPosition"/>, paints the indicators and refuses illegal
    /// drops, then hands the move to the handler — it never reparents anything itself.</description></item>
    /// <item><description><see cref="onDeleteSelection"/> handles Delete and Backspace. There is no
    /// default removal.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:TreeList size="Md" selection-type="Multiple">
    ///         <ui:TreeViewItem label="Assets" icon="folder" show-caret="true" expanded="true">
    ///             <ui:TreeViewItem label="Scenes" icon="folder" depth="1" />
    ///             <ui:TreeViewItem label="Scripts" icon="folder" depth="1" draggable="false" />
    ///         </ui:TreeViewItem>
    ///     </ui:TreeList>
    /// </UXML>
    /// ]]></code>
    /// <code><![CDATA[
    /// tree.onReparent = (dragged, target, position) => model.Move(dragged, target, position);
    /// tree.onDeleteSelection = selection => model.Delete(selection);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class TreeList : BaseVisualElement, ISizeableElement
    {
        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId selectionTypeProperty = nameof(selectionType);

        internal static readonly BindingId allowNoSelectionProperty = nameof(allowNoSelection);

        internal static readonly BindingId selectedItemProperty = nameof(selectedItem);

        internal static readonly BindingId selectedItemsProperty = nameof(selectedItems);

        internal static readonly BindingId caretIconProperty = nameof(caretIcon);

        internal static readonly BindingId keyboardNavigationProperty = nameof(keyboardNavigation);

        /// <summary>
        /// The TreeList's main USS class name.
        /// </summary>
        public const string ussClassName = "appui-tree-list";

        /// <summary>
        /// The TreeList's size USS class name. The container carries the size and the stylesheet
        /// cascades it onto every <see cref="TreeViewItem"/> row it contains.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The USS class name applied to the tree while a row drag is in progress.
        /// </summary>
        public const string draggingUssClassName = ussClassName + "--dragging";

        /// <summary>
        /// The MIME type a tree-row drag publishes itself under. See <see cref="TreeListDragPayload"/>.
        /// </summary>
        public const string treeItemMimeType = "application/x-appui-tree-item";

        /// <summary>
        /// The fraction of a row's height, measured from each edge, that reads as a
        /// <see cref="DropPosition.Before"/> or <see cref="DropPosition.After"/> drop rather than a
        /// <see cref="DropPosition.Inside"/> one. Matches the registry's 25% bands.
        /// </summary>
        const float k_EdgeBand = 0.25f;

        /// <summary>
        /// How far, in pixels, a press may travel and still release as a click rather than a drag.
        /// Matches the registry's <c>PRESS_DRAG_DISTANCE</c>.
        /// </summary>
        const float k_PressDragDistance = 4f;

        /// <summary>
        /// A plain press in <see cref="SelectionType.Multiple"/> mode waiting for its release.
        /// </summary>
        TreeViewItem m_PressItem;

        Vector2 m_PressOrigin;

        int m_PressPointerId = -1;

        bool m_PressDragged;

        VisualElement m_PressWatchRoot;

        Size m_Size;

        string m_CaretIcon;

        readonly Dragger m_Dragger;

        TreeViewItem m_DragCandidate;

        TreeViewItem m_DraggedItem;

        TreeViewItem m_DropTargetItem;

        DropPosition? m_DropPosition;

        SelectionType m_SelectionType = SelectionType.Single;

        bool m_AllowNoSelection = true;

        bool m_KeyboardNavigation = true;

        readonly List<TreeViewItem> m_SelectedItems = new List<TreeViewItem>();

        TreeViewItem m_Anchor;

        /// <summary>
        /// Event raised when <see cref="selectedItems"/> changes, with the new selection in priority
        /// order. Not raised by <see cref="SetSelectionWithoutNotify"/>, nor when a selection request
        /// resolves to the selection that was already in place.
        /// </summary>
        public event Action<IEnumerable<TreeViewItem>> selectionChanged;

        /// <summary>
        /// Handler called on a successful drop, with the dragged row, the row it was dropped on, and
        /// where it landed relative to that row.
        /// </summary>
        /// <remarks>
        /// Setting this is what turns drag-to-reorder on: with no handler the tree never starts a row
        /// drag at all, exactly like the registry, where the whole DnD surface is gated on
        /// <c>onReparent</c>. The tree deliberately does <b>not</b> move any element itself — reparenting
        /// is the owner's job, because the rows are usually a projection of an external model.
        /// Individual rows opt out of being picked up with <see cref="TreeViewItem.draggable"/>.
        /// </remarks>
        public Action<TreeViewItem, TreeViewItem, DropPosition> onReparent { get; set; }

        /// <summary>
        /// Handler called when Delete or Backspace is pressed with a non-empty selection, receiving
        /// that selection in priority order.
        /// </summary>
        /// <remarks>
        /// There is no default behaviour: the tree never removes a row on its own, because a row is
        /// usually a projection of an external model that has to be the one to delete. With no handler
        /// the keys are simply not handled.
        /// </remarks>
        public Action<IEnumerable<TreeViewItem>> onDeleteSelection { get; set; }

        /// <summary>
        /// The size of the rows in this TreeList.
        /// </summary>
        /// <remarks>
        /// The container carries the size class and <c>TreeList.uss</c> cascades the matching
        /// control-sizing alias onto the headers of the <see cref="TreeViewItem"/> elements inside it,
        /// so items added later pick the size up with no extra bookkeeping.
        /// </remarks>
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
        /// How many items the user can select at once. Defaults to <see cref="SelectionType.Single"/>.
        /// </summary>
        /// <remarks>
        /// Narrowing the selection type trims the current selection: <see cref="SelectionType.None"/>
        /// clears it and <see cref="SelectionType.Single"/> keeps only the primary.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public SelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                var changed = m_SelectionType != value;
                m_SelectionType = value;

                switch (m_SelectionType)
                {
                    case SelectionType.None:
                        ApplySelection(Array.Empty<TreeViewItem>(), true, force: true);
                        break;
                    case SelectionType.Single:
                        if (m_SelectedItems.Count > 1)
                            ApplySelection(new[] { m_SelectedItems[0] }, true, force: true);
                        break;
                }

                m_Anchor = null;

                if (changed)
                    NotifyPropertyChanged(in selectionTypeProperty);
            }
        }

        /// <summary>
        /// Whether the TreeList is allowed to have an empty selection. When false, a request that
        /// would empty the selection is ignored, and setting it to false selects the first visible
        /// item if nothing is selected yet.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool allowNoSelection
        {
            get => m_AllowNoSelection;
            set
            {
                var changed = m_AllowNoSelection != value;
                m_AllowNoSelection = value;

                if (!m_AllowNoSelection && m_SelectedItems.Count == 0 && m_SelectionType != SelectionType.None)
                {
                    var first = GetVisibleItems().FirstOrDefault();
                    if (first != null)
                        ApplySelection(new[] { first }, true, force: true);
                }

                if (changed)
                    NotifyPropertyChanged(in allowNoSelectionProperty);
            }
        }

        /// <summary>
        /// Whether the tree takes keyboard focus and navigates its rows with the arrow keys, Home / End,
        /// action+A, Escape and Delete. True by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Turn it off when the tree only provides selection and caret handling for rows another element
        /// lays out, a timeline's gutter say, and that element owns the keyboard: the tree then neither
        /// takes focus nor swallows keys bubbling up from the host. This is the UITK counterpart of the
        /// web registry's <c>TreeViewProvider</c>. Each expandable row then opens and closes itself on
        /// Right and Left (see <see cref="TreeViewItem.showCaret"/>).
        /// </para>
        /// <para>
        /// Leave <see cref="onReparent"/> unset for rows laid out as siblings: a drop on a descendant is
        /// told from element nesting, which flat rows do not have.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool keyboardNavigation
        {
            get => m_KeyboardNavigation;
            set
            {
                var changed = m_KeyboardNavigation != value;
                m_KeyboardNavigation = value;
                focusable = value;

                if (changed)
                    NotifyPropertyChanged(in keyboardNavigationProperty);
            }
        }

        /// <summary>
        /// Every <see cref="TreeViewItem"/> in this tree, in document order, whether or not it is
        /// currently reachable on screen.
        /// </summary>
        public IReadOnlyList<TreeViewItem> items => this.Query<TreeViewItem>().Build().ToList();

        /// <summary>
        /// The items the user can currently see, in top-to-bottom order: document order minus every
        /// item that sits inside a collapsed ancestor. This is the order Shift-range selection,
        /// <see cref="SelectAll"/> and arrow navigation work over.
        /// </summary>
        public IReadOnlyList<TreeViewItem> visibleItems => GetVisibleItems();

        List<TreeViewItem> GetVisibleItems() => this.Query<TreeViewItem>().Build().Where(IsVisible).ToList();

        /// <summary>
        /// The current selection, in priority order. Index 0 is the primary — see
        /// <see cref="selectedItem"/>.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public IReadOnlyList<TreeViewItem> selectedItems
        {
            get
            {
                PruneSelection();
                return m_SelectedItems.ToList();
            }
        }

        /// <summary>
        /// The primary selected item — the one the user interacted with last — or null when nothing
        /// is selected. Keyboard navigation moves relative to this item.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public TreeViewItem selectedItem
        {
            get
            {
                PruneSelection();
                return m_SelectedItems.Count > 0 ? m_SelectedItems[0] : null;
            }
        }

        /// <summary>
        /// Gets or sets the disclosure glyph every row in this tree draws, by icon name. Null or empty
        /// is the default caret.
        /// </summary>
        /// <remarks>
        /// The glyph rotates with the row's expansion either way, and the caret gutter keeps its width,
        /// so this is a glyph swap inside the existing row anatomy: a denser tree can ask for a
        /// 16-grid chevron in place of the 32-grid caret. The tree hands the name down as a context
        /// rather than walking its rows, so a row added later picks it up as well.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string caretIcon
        {
            get => m_CaretIcon;
            set
            {
                var changed = m_CaretIcon != value;
                m_CaretIcon = value;
                this.ProvideContext(new CaretIconContext(value));
                if (changed)
                    NotifyPropertyChanged(in caretIconProperty);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeList"/> class.
        /// </summary>
        public TreeList()
        {
            AddToClassList(ussClassName);

            focusable = true;
            tabIndex = 0;
            pickingMode = PickingMode.Ignore;

            size = Size.Md;
            caretIcon = null;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<DetachFromPanelEvent>(_ => EndPress());

            // Same plumbing BaseGridView uses for its own drags: a Dragger with a pixel threshold,
            // gated by acceptStartDrag so a press on a non-draggable row (or any press at all while
            // onReparent is unset) never turns into a drag.
            m_Dragger = new Dragger(OnDraggerStarted, OnDraggerMoved, OnDraggerEnded, OnDraggerCanceled)
            {
                acceptStartDrag = AcceptStartDrag
            };
            this.AddManipulator(m_Dragger);
        }

        // ── Drag to reorder ─────────────────────────────────────────────────

        /// <summary>
        /// The row currently being dragged, or null when no drag is in progress.
        /// </summary>
        public TreeViewItem draggedItem => m_DraggedItem;

        /// <summary>
        /// The row a drag is currently hovering as its drop target, or null.
        /// </summary>
        public TreeViewItem dropTargetItem => m_DropTargetItem;

        /// <summary>
        /// Where the dragged row would land relative to <see cref="dropTargetItem"/>, or null when
        /// there is no valid target under the pointer.
        /// </summary>
        public DropPosition? dropPosition => m_DropPosition;

        /// <summary>
        /// The payload of the drag in progress, or null. See <see cref="TreeListDragPayload"/>.
        /// </summary>
        public TreeListDragPayload dragPayload { get; private set; }

        /// <summary>
        /// The <see cref="Dragger"/> manipulator that turns a press on a row into a reorder drag.
        /// </summary>
        public Dragger dragger => m_Dragger;

        /// <summary>
        /// Whether <paramref name="target"/> can receive <paramref name="item"/>.
        /// </summary>
        /// <remarks>
        /// A row cannot be dropped onto itself, nor onto any of its own descendants — that would
        /// detach the subtree from the tree.
        /// </remarks>
        /// <param name="item">The row being dragged.</param>
        /// <param name="target">The candidate drop target.</param>
        /// <returns>True when the drop is legal.</returns>
        public bool CanDrop(TreeViewItem item, TreeViewItem target)
        {
            if (item == null || target == null || item == target)
                return false;

            var parent = target.hierarchy.parent;
            while (parent != null && parent != this)
            {
                if (parent == item)
                    return false;
                parent = parent.hierarchy.parent;
            }

            return Contains(item) && Contains(target);
        }

        /// <summary>
        /// The <see cref="DropPosition"/> a pointer at <paramref name="worldPosition"/> reads as over
        /// <paramref name="item"/>: the top and bottom quarters of the row are Before and After, the
        /// middle half is Inside.
        /// </summary>
        /// <param name="item">The row under the pointer.</param>
        /// <param name="worldPosition">The pointer position, in panel space.</param>
        /// <returns>The resolved drop position.</returns>
        public DropPosition GetDropPosition(TreeViewItem item, Vector2 worldPosition)
        {
            var rect = item.header.worldBound;
            if (rect.height <= 0f)
                return DropPosition.Inside;

            var rel = (worldPosition.y - rect.yMin) / rect.height;
            if (rel < k_EdgeBand)
                return DropPosition.Before;
            if (rel > 1f - k_EdgeBand)
                return DropPosition.After;
            return DropPosition.Inside;
        }

        /// <summary>
        /// The visible row whose header contains <paramref name="worldPosition"/>, or null.
        /// </summary>
        /// <param name="worldPosition">A position in panel space.</param>
        /// <returns>The row under that position.</returns>
        public TreeViewItem GetItemAtPosition(Vector2 worldPosition)
        {
            // Pick rather than a rect scan over visibleItems: a collapsed subtree's rows are clipped
            // to a zero-height viewport but still report a world rect, so a rect scan happily returns
            // a row that is not on screen and shadows the row that is.
            var picked = panel?.Pick(worldPosition);
            while (picked != null && picked != this)
            {
                if (picked is TreeViewItem item)
                    return item;
                picked = picked.hierarchy.parent;
            }

            return null;
        }

        /// <summary>
        /// Starts a reorder drag for <paramref name="item"/>. A no-op when <see cref="onReparent"/> is
        /// unset or the item is not <see cref="TreeViewItem.draggable"/>.
        /// </summary>
        /// <param name="item">The row to pick up.</param>
        public void BeginDrag(TreeViewItem item)
        {
            if (onReparent == null || item == null || !item.draggable || !Contains(item))
                return;

            CancelDrag();

            m_DraggedItem = item;
            m_DraggedItem.dragging = true;
            AddToClassList(draggingUssClassName);

            // A drag that starts on an already-selected row carries the whole selection, so a
            // multi-row move reads as one gesture; otherwise it carries just the grabbed row.
            var payloadItems = selectedItems.Contains(item)
                ? selectedItems.ToList()
                : new List<TreeViewItem> { item };
            dragPayload = new TreeListDragPayload(this, payloadItems);
        }

        /// <summary>
        /// Re-evaluates the drop target for a pointer at <paramref name="worldPosition"/>, updating the
        /// drop indicators.
        /// </summary>
        /// <param name="worldPosition">The pointer position, in panel space.</param>
        public void UpdateDrag(Vector2 worldPosition)
        {
            if (m_DraggedItem == null)
                return;

            var target = GetItemAtPosition(worldPosition);
            if (!CanDrop(m_DraggedItem, target))
            {
                SetDropTarget(null, null);
                return;
            }

            SetDropTarget(target, GetDropPosition(target, worldPosition));
        }

        /// <summary>
        /// Ends the drag in progress, invoking <see cref="onReparent"/> when it ends over a valid drop
        /// target.
        /// </summary>
        /// <returns>True when a drop was performed.</returns>
        public bool EndDrag()
        {
            if (m_DraggedItem == null)
                return false;

            var dragged = m_DraggedItem;
            var target = m_DropTargetItem;
            var position = m_DropPosition;

            ClearDrag();

            if (target == null || !position.HasValue || !CanDrop(dragged, target))
                return false;

            onReparent?.Invoke(dragged, target, position.Value);
            return true;
        }

        /// <summary>
        /// Abandons the drag in progress without dropping.
        /// </summary>
        public void CancelDrag()
        {
            if (m_DraggedItem == null)
                return;

            ClearDrag();
        }

        void ClearDrag()
        {
            SetDropTarget(null, null);
            if (m_DraggedItem != null)
                m_DraggedItem.dragging = false;
            m_DraggedItem = null;
            m_DragCandidate = null;
            dragPayload = null;
            RemoveFromClassList(draggingUssClassName);
        }

        void SetDropTarget(TreeViewItem target, DropPosition? position)
        {
            if (m_DropTargetItem != null && m_DropTargetItem != target)
                m_DropTargetItem.dropPosition = null;

            m_DropTargetItem = target;
            m_DropPosition = target != null ? position : null;

            if (m_DropTargetItem != null)
                m_DropTargetItem.dropPosition = m_DropPosition;
        }

        bool AcceptStartDrag(Vector2 worldPosition)
        {
            m_DragCandidate = null;

            if (onReparent == null)
                return false;

            var picked = panel?.Pick(worldPosition);
            var onCaret = false;
            TreeViewItem item = null;
            while (picked != null && picked != this)
            {
                if (picked.ClassListContains(TreeViewItem.caretButtonUssClassName))
                    onCaret = true;
                if (picked is TreeViewItem hit)
                {
                    item = hit;
                    break;
                }

                picked = picked.hierarchy.parent;
            }

            // A press on the caret is a disclosure, never the start of a move.
            if (item == null || !item.draggable || onCaret)
                return false;

            m_DragCandidate = item;
            return true;
        }

        void OnDraggerStarted(PointerMoveEvent evt)
        {
            // A drag started, so the press must not end in a selection.
            m_PressDragged = true;
            BeginDrag(m_DragCandidate);
            UpdateDrag(evt.position);
        }

        void OnDraggerMoved(PointerMoveEvent evt)
        {
            UpdateDrag(evt.position);
        }

        void OnDraggerEnded(PointerUpEvent evt)
        {
            UpdateDrag(evt.position);
            EndDrag();
        }

        void OnDraggerCanceled()
        {
            CancelDrag();
        }

        /// <summary>
        /// Applies a selection interaction for an item, resolving the modifiers against the current
        /// <see cref="selectionType"/>. This is the entry point pointer and keyboard interactions go
        /// through, and it is public so callers can reproduce a click from code.
        /// </summary>
        /// <param name="item">The item the user acted on.</param>
        /// <param name="shift">True when the range modifier is held. Ignored unless the tree is in
        /// <see cref="SelectionType.Multiple"/> mode and an anchor exists.</param>
        /// <param name="toggle">True when the action key (Cmd on macOS, Ctrl elsewhere) is held.
        /// Ignored unless the tree is in <see cref="SelectionType.Multiple"/> mode.</param>
        public void Select(TreeViewItem item, bool shift = false, bool toggle = false)
        {
            if (item == null || m_SelectionType == SelectionType.None)
                return;

            if (m_SelectionType != SelectionType.Multiple)
            {
                ApplySelection(new[] { item }, true);
                m_Anchor = item;
                return;
            }

            PruneSelection();
            var current = m_SelectedItems.ToList();

            if (shift && m_Anchor != null)
            {
                var order = GetVisibleItems();
                var a = order.IndexOf(m_Anchor);
                var b = order.IndexOf(item);
                if (a != -1 && b != -1)
                {
                    var lo = a <= b ? a : b;
                    var hi = a <= b ? b : a;
                    var range = new List<TreeViewItem> { item };
                    for (var i = lo; i <= hi; i++)
                    {
                        if (order[i] != item)
                            range.Add(order[i]);
                    }

                    // The action key turns a Shift-click into "add this range"; Shift alone replaces.
                    if (toggle)
                        range.AddRange(current);

                    // The anchor stays put so repeated Shift-clicks recompute from the same pivot.
                    ApplySelection(range, true);
                    return;
                }
            }

            if (toggle)
            {
                if (current.Contains(item))
                    current.Remove(item);
                else
                    current.Insert(0, item);
                ApplySelection(current, true);
                m_Anchor = item;
                return;
            }

            ApplySelection(new[] { item }, true);
            m_Anchor = item;
        }

        /// <summary>
        /// Selects a single item, replacing the current selection.
        /// </summary>
        /// <param name="item">The item to select, or null to clear the selection.</param>
        public void SetSelection(TreeViewItem item)
        {
            SetSelection(item == null ? Array.Empty<TreeViewItem>() : new[] { item });
        }

        /// <summary>
        /// Replaces the current selection, raising <see cref="selectionChanged"/> when it changes.
        /// </summary>
        /// <param name="itemsToSelect">The items to select, in priority order. In
        /// <see cref="SelectionType.Single"/> mode only the last one survives, matching how
        /// <see cref="BaseGridView"/> narrows a multi-index request.</param>
        public void SetSelection(IEnumerable<TreeViewItem> itemsToSelect)
        {
            SetSelectionInternal(itemsToSelect, true);
        }

        /// <summary>
        /// Replaces the current selection without raising <see cref="selectionChanged"/>.
        /// </summary>
        /// <param name="itemsToSelect">The items to select, in priority order.</param>
        public void SetSelectionWithoutNotify(IEnumerable<TreeViewItem> itemsToSelect)
        {
            SetSelectionInternal(itemsToSelect, false);
        }

        /// <summary>
        /// Clears the selection. Does nothing when <see cref="allowNoSelection"/> is false.
        /// </summary>
        public void ClearSelection()
        {
            SetSelectionInternal(Array.Empty<TreeViewItem>(), true);
        }

        /// <summary>
        /// Selects every item in <see cref="visibleItems"/>. Only valid in
        /// <see cref="SelectionType.Multiple"/> mode; a no-op otherwise.
        /// </summary>
        public void SelectAll()
        {
            if (m_SelectionType != SelectionType.Multiple)
                return;

            ApplySelection(GetVisibleItems(), true);
            m_Anchor = null;
        }

        /// <summary>
        /// Expands every item that has children.
        /// </summary>
        public void ExpandAll()
        {
            foreach (var item in items)
            {
                if (item.childCount > 0)
                    item.expanded = true;
            }
        }

        /// <summary>
        /// Collapses every item that has children.
        /// </summary>
        public void CollapseAll()
        {
            foreach (var item in items)
            {
                if (item.childCount > 0)
                    item.expanded = false;
            }
        }

        void SetSelectionInternal(IEnumerable<TreeViewItem> itemsToSelect, bool notify)
        {
            if (m_SelectionType == SelectionType.None)
                return;

            var list = itemsToSelect?.Where(i => i != null && Contains(i)).ToList() ?? new List<TreeViewItem>();

            if (m_SelectionType == SelectionType.Single && list.Count > 1)
                list = new List<TreeViewItem> { list[list.Count - 1] };

            if (list.Count == 0 && !m_AllowNoSelection)
                return;

            ApplySelection(list, notify);
            m_Anchor = list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// The single place the selection is written. Deduplicates, flips the
        /// <see cref="TreeViewItem.selected"/> flag on the items entering and leaving the selection,
        /// and raises <see cref="selectionChanged"/> only on a real change.
        /// </summary>
        void ApplySelection(IEnumerable<TreeViewItem> next, bool notify, bool force = false)
        {
            var list = new List<TreeViewItem>();
            foreach (var item in next)
            {
                if (item != null && !list.Contains(item))
                    list.Add(item);
            }

            if (list.Count == 0 && !m_AllowNoSelection && !force)
                return;

            if (list.Count == m_SelectedItems.Count && list.SequenceEqual(m_SelectedItems))
                return;

            foreach (var item in m_SelectedItems)
            {
                if (!list.Contains(item))
                    item.selected = false;
            }

            m_SelectedItems.Clear();
            m_SelectedItems.AddRange(list);

            foreach (var item in m_SelectedItems)
                item.selected = true;

            NotifyPropertyChanged(in selectedItemProperty);
            NotifyPropertyChanged(in selectedItemsProperty);

            if (notify)
                selectionChanged?.Invoke(m_SelectedItems.ToList());
        }

        /// <summary>
        /// Drops selected items that are no longer part of this tree, so a removed row cannot keep
        /// the selection alive.
        /// </summary>
        /// <remarks>
        /// The drop goes through <see cref="ApplySelection"/> rather than straight into the backing
        /// list: a detached row has to lose its <see cref="TreeViewItem.selected"/> flag, or it
        /// comes back styled as selected when the caller reuses it, and an
        /// <see cref="allowNoSelection"/> tree has to fall back to a visible row instead of
        /// reporting an empty selection. Listeners are told either way.
        /// </remarks>
        void PruneSelection()
        {
            var kept = new List<TreeViewItem>();
            foreach (var item in m_SelectedItems)
            {
                if (Contains(item))
                    kept.Add(item);
            }

            if (kept.Count == m_SelectedItems.Count)
                return;

            foreach (var item in m_SelectedItems)
            {
                if (!kept.Contains(item))
                    item.selected = false;
            }

            if (kept.Count == 0 && !m_AllowNoSelection && m_SelectionType != SelectionType.None)
            {
                var first = GetVisibleItems().FirstOrDefault();
                if (first != null)
                    kept.Add(first);
            }

            if (m_Anchor != null && !kept.Contains(m_Anchor))
                m_Anchor = kept.Count > 0 ? kept[0] : null;

            // Forced, so an empty result still lands when the tree has no row left to fall back to:
            // holding on to detached items would be worse than an empty selection.
            ApplySelection(kept, true, force: true);
        }

        /// <summary>
        /// True when no <see cref="TreeViewItem"/> ancestor between the item and this tree is
        /// collapsed.
        /// </summary>
        bool IsVisible(TreeViewItem item)
        {
            var parent = item.hierarchy.parent;
            while (parent != null && parent != this)
            {
                if (parent is TreeViewItem ancestor && !ancestor.expanded)
                    return false;
                parent = parent.hierarchy.parent;
            }

            return true;
        }

        /// <summary>
        /// Finds the <see cref="TreeViewItem"/> an event target belongs to, and whether the target is
        /// inside that item's caret button.
        /// </summary>
        TreeViewItem FindItem(VisualElement target, out bool onCaret)
        {
            onCaret = false;
            var caret = false;
            var element = target;
            while (element != null && element != this)
            {
                if (element.ClassListContains(TreeViewItem.caretButtonUssClassName))
                    caret = true;

                if (element is TreeViewItem item)
                {
                    onCaret = caret;
                    return item;
                }

                element = element.hierarchy.parent;
            }

            return null;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            var item = FindItem(evt.target as VisualElement, out var onCaret);
            if (item == null)
                return;

            if (onCaret)
            {
                // The caret is an expansion affordance, never a selection one.
                item.ToggleExpand();
                evt.StopPropagation();
                return;
            }

            EndPress();

            // Unity press/click/drag, as the registry does it. A modified press (Shift / action key)
            // applies at once, and so does any press outside Multiple mode. A plain multi-select
            // press changes nothing until the release, so pressing one row of a group to drag it
            // keeps the group whole, and dragging an unselected row does not first make it the
            // selection.
            if (m_SelectionType != SelectionType.Multiple || evt.shiftKey || evt.actionKey)
                Select(item, evt.shiftKey, evt.actionKey);
            else
                BeginPress(item, evt);

            Focus();
            evt.StopPropagation();
        }

        void BeginPress(TreeViewItem item, PointerDownEvent evt)
        {
            m_PressItem = item;
            m_PressOrigin = evt.position;
            m_PressPointerId = evt.pointerId;
            m_PressDragged = false;

            // The travel is watched on the panel root, not the row: a drag can leave the row before
            // it crosses the distance and come back to be released next to where it started. A
            // drag the consumer runs on pointer events is only visible here as that travel.
            m_PressWatchRoot = panel?.visualTree;
            if (m_PressWatchRoot != null)
            {
                m_PressWatchRoot.RegisterCallback<PointerMoveEvent>(OnPressMove, TrickleDown.TrickleDown);
                m_PressWatchRoot.RegisterCallback<PointerUpEvent>(OnPressEnd);
                m_PressWatchRoot.RegisterCallback<PointerCancelEvent>(OnPressCancel);
            }
        }

        void EndPress()
        {
            if (m_PressWatchRoot != null)
            {
                m_PressWatchRoot.UnregisterCallback<PointerMoveEvent>(OnPressMove, TrickleDown.TrickleDown);
                m_PressWatchRoot.UnregisterCallback<PointerUpEvent>(OnPressEnd);
                m_PressWatchRoot.UnregisterCallback<PointerCancelEvent>(OnPressCancel);
                m_PressWatchRoot = null;
            }

            m_PressItem = null;
            m_PressPointerId = -1;
            m_PressDragged = false;
        }

        void OnPressMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != m_PressPointerId)
                return;

            if (((Vector2)evt.position - m_PressOrigin).sqrMagnitude >= k_PressDragDistance * k_PressDragDistance)
                m_PressDragged = true;
        }

        // Bubble phase on the root, so a release on a row reaches OnPointerUp first; a release
        // anywhere else just ends the press.
        void OnPressEnd(PointerUpEvent evt)
        {
            if (evt.pointerId == m_PressPointerId)
                EndPress();
        }

        void OnPressCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId == m_PressPointerId)
                EndPress();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_PressItem == null || evt.pointerId != m_PressPointerId)
                return;

            var item = m_PressItem;
            var dragged = m_PressDragged ||
                ((Vector2)evt.position - m_PressOrigin).sqrMagnitude >= k_PressDragDistance * k_PressDragDistance;
            EndPress();

            // Only a click selects: the release has to land back on the pressed row. After a drag
            // the pointer is captured by the tree, so resolve the row by position, not by target.
            if (dragged || GetItemAtPosition(evt.position) != item)
                return;

            Select(item);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (!m_KeyboardNavigation)
                return;

            var multiple = m_SelectionType == SelectionType.Multiple;

            if (multiple && evt.actionKey && evt.keyCode == KeyCode.A)
            {
                SelectAll();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                // No default behaviour: with no handler the keys fall through untouched.
                var selection = selectedItems;
                if (onDeleteSelection != null && selection.Count > 0)
                {
                    onDeleteSelection.Invoke(selection.ToList());
                    evt.StopPropagation();
                }

                return;
            }

            if (multiple && evt.keyCode == KeyCode.Escape)
            {
                ApplySelection(Array.Empty<TreeViewItem>(), true, force: true);
                m_Anchor = null;
                evt.StopPropagation();
                return;
            }

            var order = GetVisibleItems();
            if (order.Count == 0)
                return;

            var lead = selectedItem;
            var index = lead != null ? order.IndexOf(lead) : -1;
            var extend = multiple && evt.shiftKey;

            void GoTo(TreeViewItem target)
            {
                if (target != null)
                    Select(target, extend);
            }

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    GoTo(index < 0 ? order[0] : order[Math.Min(index + 1, order.Count - 1)]);
                    break;
                case KeyCode.UpArrow:
                    GoTo(index < 0 ? order[0] : order[Math.Max(index - 1, 0)]);
                    break;
                case KeyCode.RightArrow:
                    if (lead == null)
                        GoTo(order[0]);
                    else if (lead.isExpandable && !lead.expanded)
                        lead.expanded = true;
                    else if (index >= 0 && index + 1 < order.Count)
                    {
                        // Step in only to a nested child: a caret row's children may be laid out
                        // elsewhere or not loaded, and the next row is then a sibling.
                        var next = order[index + 1];
                        if (!lead.isExpandable || lead.Contains(next))
                            GoTo(next);
                    }
                    break;
                case KeyCode.LeftArrow:
                    if (lead == null)
                        GoTo(order[0]);
                    else if (lead.isExpandable && lead.expanded)
                        lead.expanded = false;
                    else
                        GoTo(FindItem(lead.hierarchy.parent, out _));
                    break;
                case KeyCode.Home:
                    GoTo(order[0]);
                    break;
                case KeyCode.End:
                    GoTo(order[order.Count - 1]);
                    break;
                default:
                    return;
            }

            evt.StopPropagation();
        }
    }
}
