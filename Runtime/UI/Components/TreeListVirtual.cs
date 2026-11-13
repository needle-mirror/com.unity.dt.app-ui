using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A tree whose rows are windowed: only the rows near the scrolled view exist as
    /// <see cref="TreeViewItem"/> elements, so a tree of any size costs a constant number of
    /// elements. A port of the web registry's <c>TreeListVirtual</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="TreeList"/> stays a composition container: its consumer builds every
    /// <see cref="TreeViewItem"/>, and its public surface (<see cref="TreeList.items"/>,
    /// <see cref="TreeList.selectedItems"/>, <see cref="TreeList.onReparent"/>) hands out those
    /// element references, so it cannot recycle rows without breaking that contract. This sibling
    /// takes the rows as data instead, flat and in display order (see
    /// <see cref="TreeListFlatten.Flatten{T}"/>), and recycles a small pool of items over them.
    /// Selection is therefore by row id rather than by element.
    /// </para>
    /// <para>
    /// Selection (with the same press-then-release rule as <see cref="TreeList"/> in
    /// <see cref="SelectionType.Multiple"/> mode) and keyboard navigation (arrows, Home / End,
    /// Shift-range, action+A, Escape, Delete) read the row list rather than the realised elements,
    /// so they reach rows that are not realised. Expansion is the caller's: a caret click or an
    /// arrow key reports it through <see cref="onExpandedChange"/> and the caller rebuilds
    /// <see cref="rows"/>. Rows are a uniform height, the height of the first realised row.
    /// Drag to reorder is not supported yet.
    /// </para>
    /// </remarks>
    [UxmlElement]
    public partial class TreeListVirtual : BaseVisualElement, ISizeableElement
    {
        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId selectionTypeProperty = nameof(selectionType);

        internal static readonly BindingId overscanProperty = nameof(overscan);

        /// <summary>
        /// The TreeListVirtual's main USS class name. The element also carries
        /// <see cref="TreeList.ussClassName"/>, so the rows pick up the tree's row styling.
        /// </summary>
        public const string ussClassName = "appui-tree-list-virtual";

        /// <summary>
        /// The USS class name of the scroll view.
        /// </summary>
        public const string scrollViewUssClassName = ussClassName + "__scroll-view";

        /// <summary>
        /// The USS class name of the element sized to the whole row list.
        /// </summary>
        public const string rowsContainerUssClassName = ussClassName + "__rows";

        /// <summary>
        /// The USS class name of a realised row.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The USS class name of a realised placeholder row.
        /// </summary>
        public const string placeholderUssClassName = ussClassName + "__row--placeholder";

        /// <summary>
        /// The USS class name of the hidden element whose width is the per-level indent.
        /// </summary>
        public const string indentProbeUssClassName = ussClassName + "__indent-probe";

        // The fewest rows realised beyond each edge, whatever the view's height.
        const int k_MinOverscan = 10;

        // Used only until the first row is measured.
        const float k_FallbackRowSize = 24f;

        const float k_PressDragDistance = 4f;

        readonly ScrollView m_ScrollView;

        readonly VisualElement m_RowsContainer;

        readonly VisualElement m_IndentProbe;

        readonly Dictionary<int, TreeViewItem> m_Realised = new Dictionary<int, TreeViewItem>();

        readonly Stack<TreeViewItem> m_Pool = new Stack<TreeViewItem>();

        readonly Dictionary<string, int> m_IndexById = new Dictionary<string, int>();

        readonly List<string> m_SelectedIds = new List<string>();

        readonly HashSet<string> m_SelectedSet = new HashSet<string>();

        IReadOnlyList<TreeListRow> m_Rows = Array.Empty<TreeListRow>();

        IReadOnlyList<string> m_PinnedIds = Array.Empty<string>();

        Size m_Size;

        SelectionType m_SelectionType = SelectionType.Single;

        int m_Overscan = -1;

        float m_RowSize;

        float m_Indent;

        string m_AnchorId;

        string m_PressId;

        string m_FocusedId;

        VisualElement m_PressWatchRoot;

        Vector2 m_PressOrigin;

        int m_PressPointerId = -1;

        bool m_PressDragged;

        /// <summary>
        /// Every visible row in display order, placeholders included. Assigning it rebinds the
        /// realised rows. The selection is kept by id.
        /// </summary>
        public IReadOnlyList<TreeListRow> rows
        {
            get => m_Rows;
            set
            {
                // Remember which item row each realised element draws, so it follows its row to the
                // row's new index rather than being rebound to whatever row now sits at the old one.
                var previous = new List<KeyValuePair<string, TreeViewItem>>();
                foreach (var pair in m_Realised)
                {
                    var old = pair.Key < m_Rows.Count ? m_Rows[pair.Key] : null;
                    previous.Add(new KeyValuePair<string, TreeViewItem>(
                        old != null && !old.placeholder ? old.id : null, pair.Value));
                }

                m_Rows = value ?? Array.Empty<TreeListRow>();
                m_IndexById.Clear();
                for (var i = 0; i < m_Rows.Count; i++)
                {
                    var row = m_Rows[i];
                    if (row != null && !row.placeholder && row.id != null)
                        m_IndexById[row.id] = i;
                }

                m_Realised.Clear();
                foreach (var pair in previous)
                {
                    var index = IndexOf(pair.Key);
                    if (index >= 0 && !m_Realised.ContainsKey(index))
                    {
                        m_Realised[index] = pair.Value;
                    }
                    else
                    {
                        pair.Value.style.display = DisplayStyle.None;
                        m_Pool.Push(pair.Value);
                    }
                }

                RefreshItems();
            }
        }

        /// <summary>
        /// Draws one row onto a recycled <see cref="TreeViewItem"/>: its label, icon and anything
        /// else that comes from the caller's item. The tree has already set the row's indentation,
        /// caret, expansion and selection. Called for placeholders too; check
        /// <see cref="TreeListRow.placeholder"/>. With no handler, the row shows its id.
        /// </summary>
        public Action<TreeViewItem, TreeListRow, int> bindRow { get; set; }

        /// <summary>
        /// Called with a row's id and its requested state when a caret click or an arrow key expands
        /// or collapses it. The caller updates its expansion state and assigns new <see cref="rows"/>.
        /// </summary>
        public Action<string, bool> onExpandedChange { get; set; }

        /// <summary>
        /// Called when Delete or Backspace is pressed with a non-empty selection, receiving the
        /// selected ids in priority order. There is no default behaviour.
        /// </summary>
        public Action<IReadOnlyList<string>> onDeleteSelection { get; set; }

        /// <summary>
        /// Raised when the selection changes, with the selected ids in priority order.
        /// </summary>
        public event Action<IReadOnlyList<string>> selectionChanged;

        /// <summary>
        /// The selected row ids in priority order. Index 0 is the lead, the row the user acted on
        /// last, which keyboard navigation moves from.
        /// </summary>
        public IReadOnlyList<string> selectedIds => m_SelectedIds;

        /// <summary>
        /// The lead selected row id, or null.
        /// </summary>
        public string selectedId => m_SelectedIds.Count > 0 ? m_SelectedIds[0] : null;

        /// <summary>
        /// Row ids kept realised even when scrolled out of view, a row being renamed for instance.
        /// The lead selected row and a row being pressed are always kept.
        /// </summary>
        public IReadOnlyList<string> pinnedIds
        {
            get => m_PinnedIds;
            set
            {
                m_PinnedIds = value ?? Array.Empty<string>();
                Refresh();
            }
        }

        /// <summary>
        /// Rows realised beyond each edge of the view. -1, the default, realises one view's worth,
        /// and never fewer than 10, so a scroll of up to a full view in one frame still lands on
        /// realised rows.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int overscan
        {
            get => m_Overscan;
            set
            {
                var changed = m_Overscan != value;
                m_Overscan = value;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in overscanProperty);
            }
        }

        /// <summary>
        /// The size of the rows. Cascades onto the rows the same way <see cref="TreeList.size"/> does.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(TreeList.GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(TreeList.GetSizeUssClassName(m_Size));
                // Rows change height with the size; measure again.
                m_RowSize = 0;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// How many rows the user can select at once. Defaults to <see cref="SelectionType.Single"/>.
        /// Narrowing it trims the current selection.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public SelectionType selectionType
        {
            get => m_SelectionType;
            set
            {
                var changed = m_SelectionType != value;
                m_SelectionType = value;
                if (m_SelectionType == SelectionType.None)
                    ApplySelection(Array.Empty<string>());
                else if (m_SelectionType == SelectionType.Single && m_SelectedIds.Count > 1)
                    ApplySelection(new[] { m_SelectedIds[0] });
                if (changed)
                    NotifyPropertyChanged(in selectionTypeProperty);
            }
        }

        /// <summary>
        /// The scroll view the rows are windowed against.
        /// </summary>
        public ScrollView scrollView => m_ScrollView;

        /// <summary>
        /// How many rows currently exist as elements.
        /// </summary>
        public int realisedRowCount => m_Realised.Count;

        /// <summary>
        /// The height every row is laid out at, measured from the first realised row.
        /// </summary>
        public float rowSize => m_RowSize > 0 ? m_RowSize : k_FallbackRowSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeListVirtual"/> class.
        /// </summary>
        public TreeListVirtual()
        {
            AddToClassList(TreeList.ussClassName);
            AddToClassList(ussClassName);

            focusable = true;
            tabIndex = 0;
            pickingMode = PickingMode.Ignore;

            m_ScrollView = new ScrollView(ScrollViewMode.Vertical) { name = scrollViewUssClassName };
            m_ScrollView.AddToClassList(scrollViewUssClassName);
            hierarchy.Add(m_ScrollView);

            m_RowsContainer = new VisualElement { name = rowsContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_RowsContainer.AddToClassList(rowsContainerUssClassName);
            m_ScrollView.Add(m_RowsContainer);

            // The indent is a sizing token, read as the resolved width of a hidden probe so it follows
            // the density classes.
            m_IndentProbe = new VisualElement { name = indentProbeUssClassName, pickingMode = PickingMode.Ignore };
            m_IndentProbe.AddToClassList(indentProbeUssClassName);
            m_IndentProbe.RegisterCallback<GeometryChangedEvent>(OnIndentProbeGeometryChanged);
            hierarchy.Add(m_IndentProbe);

            size = Size.Md;

            m_ScrollView.verticalScroller.valueChanged += _ => Refresh();
            m_ScrollView.RegisterCallback<GeometryChangedEvent>(_ => Refresh());
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<DetachFromPanelEvent>(_ => EndPress());
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary>
        /// The realised item drawing the row at <paramref name="index"/>, or null when that row is
        /// not realised.
        /// </summary>
        /// <param name="index">The row index.</param>
        /// <returns>The item, or null.</returns>
        public TreeViewItem GetRealisedItem(int index) =>
            m_Realised.TryGetValue(index, out var item) ? item : null;

        /// <summary>
        /// The index of the item row with this id, or -1. Placeholders are not indexed.
        /// </summary>
        /// <param name="id">The row id.</param>
        /// <returns>The index, or -1.</returns>
        public int IndexOf(string id) => id != null && m_IndexById.TryGetValue(id, out var i) ? i : -1;

        /// <summary>
        /// Scrolls the least distance that brings the row at <paramref name="index"/> fully into view.
        /// </summary>
        /// <param name="index">The row index.</param>
        public void ScrollToIndex(int index)
        {
            if (index < 0 || index >= m_Rows.Count)
                return;
            var view = m_ScrollView.contentViewport.layout.height;
            var top = index * rowSize;
            var offset = m_ScrollView.scrollOffset.y;
            if (top < offset)
                offset = top;
            else if (top + rowSize > offset + view)
                offset = top + rowSize - view;
            m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, Mathf.Max(0, offset));
            Refresh();
        }

        /// <summary>
        /// Whether the row at <paramref name="index"/> lies entirely inside the scrolled view.
        /// </summary>
        /// <param name="index">The row index.</param>
        /// <returns>True when the whole row is visible.</returns>
        public bool IsIndexVisible(int index)
        {
            var view = m_ScrollView.contentViewport.layout.height;
            if (index < 0 || index >= m_Rows.Count || float.IsNaN(view) || view <= 0)
                return false;
            var top = index * rowSize;
            var offset = m_ScrollView.scrollOffset.y;
            return top >= offset - 0.5f && top + rowSize <= offset + view + 0.5f;
        }

        // ── Selection ──────────────────────────────────────────────────────

        /// <summary>
        /// Applies a user selection gesture to the row with this id, the same way a click does.
        /// </summary>
        /// <param name="id">The row id.</param>
        /// <param name="shift">True when the range modifier is held (Multiple mode only).</param>
        /// <param name="toggle">True when the action key is held (Multiple mode only).</param>
        public void Select(string id, bool shift = false, bool toggle = false)
        {
            if (id == null || m_SelectionType == SelectionType.None || IndexOf(id) < 0)
                return;

            if (m_SelectionType != SelectionType.Multiple)
            {
                ApplySelection(new[] { id });
                m_AnchorId = id;
                return;
            }

            if (shift && m_AnchorId != null)
            {
                var a = IndexOf(m_AnchorId);
                var b = IndexOf(id);
                if (a >= 0)
                {
                    var range = new List<string> { id };
                    for (var i = Math.Min(a, b); i <= Math.Max(a, b); i++)
                    {
                        var row = m_Rows[i];
                        if (!row.placeholder && row.id != id)
                            range.Add(row.id);
                    }

                    if (toggle)
                        range.AddRange(m_SelectedIds);
                    ApplySelection(range);
                    return;
                }
            }

            if (toggle)
            {
                var current = new List<string>(m_SelectedIds);
                if (!current.Remove(id))
                    current.Insert(0, id);
                ApplySelection(current);
                m_AnchorId = id;
                return;
            }

            ApplySelection(new[] { id });
            m_AnchorId = id;
        }

        /// <summary>
        /// Replaces the selection with these ids, the first being the lead, and raises
        /// <see cref="selectionChanged"/> when it changes.
        /// </summary>
        /// <param name="ids">The ids to select.</param>
        public void SetSelection(IEnumerable<string> ids)
        {
            ApplySelection(ids ?? Array.Empty<string>());
            m_AnchorId = selectedId;
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void ClearSelection() => SetSelection(Array.Empty<string>());

        /// <summary>
        /// Selects every item row. Only valid in <see cref="SelectionType.Multiple"/> mode.
        /// </summary>
        public void SelectAll()
        {
            if (m_SelectionType != SelectionType.Multiple)
                return;
            var all = new List<string>();
            var lead = selectedId;
            if (lead != null)
                all.Add(lead);
            foreach (var row in m_Rows)
            {
                if (!row.placeholder && row.id != lead)
                    all.Add(row.id);
            }

            ApplySelection(all);
        }

        void ApplySelection(IEnumerable<string> ids)
        {
            var next = new List<string>();
            var seen = new HashSet<string>();
            foreach (var id in ids)
            {
                if (id != null && seen.Add(id))
                    next.Add(id);
                if (m_SelectionType == SelectionType.Single && next.Count == 1)
                    break;
            }

            if (m_SelectionType == SelectionType.None)
                next.Clear();

            var same = next.Count == m_SelectedIds.Count;
            for (var i = 0; same && i < next.Count; i++)
                same = next[i] == m_SelectedIds[i];
            if (same)
                return;

            m_SelectedIds.Clear();
            m_SelectedIds.AddRange(next);
            m_SelectedSet.Clear();
            m_SelectedSet.UnionWith(next);
            RefreshItems();
            selectionChanged?.Invoke(m_SelectedIds);
        }

        // ── Windowing ──────────────────────────────────────────────────────

        void OnIndentProbeGeometryChanged(GeometryChangedEvent evt)
        {
            var indent = evt.newRect.width;
            if (!float.IsNaN(indent) && Mathf.Abs(indent - m_Indent) > 0.01f)
            {
                m_Indent = indent;
                RefreshItems();
            }
        }

        void RefreshItems()
        {
            // Rebind every realised row: the row list, the selection or the indent changed. A row
            // list that shrank leaves realised indexes past its end; release those first.
            var stale = new List<int>();
            foreach (var index in m_Realised.Keys)
            {
                if (index >= m_Rows.Count)
                    stale.Add(index);
            }

            foreach (var index in stale)
                Release(index);
            foreach (var pair in m_Realised)
                Bind(pair.Value, pair.Key);
            Refresh();
        }

        int GetOverscan(float view)
        {
            if (m_Overscan >= 0)
                return m_Overscan;
            var viewportRows = float.IsNaN(view) ? 0 : Mathf.CeilToInt(view / rowSize);
            return Math.Max(k_MinOverscan, viewportRows);
        }

        void Refresh()
        {
            if (m_ScrollView == null)
                return;

            var count = m_Rows.Count;
            m_RowsContainer.style.height = count * rowSize;

            var view = m_ScrollView.contentViewport.layout.height;
            if (float.IsNaN(view))
                view = 0;
            var offset = m_ScrollView.scrollOffset.y;
            var extra = GetOverscan(view);
            var first = Math.Max(0, Mathf.FloorToInt(offset / rowSize) - extra);
            var last = Math.Min(count - 1, Mathf.CeilToInt((offset + view) / rowSize) + extra);

            var wanted = new HashSet<int>();
            for (var i = first; i <= last; i++)
                wanted.Add(i);
            // Kept realised out of view: the lead row is where keyboard navigation moves from and
            // a pressed row has to be there for its release, and the row holding focus (reached by
            // Tab, selected or not) would lose it if its element were recycled.
            AddPinned(wanted, selectedId);
            AddPinned(wanted, m_PressId);
            AddPinned(wanted, m_FocusedId);
            foreach (var id in m_PinnedIds)
                AddPinned(wanted, id);

            var release = new List<int>();
            foreach (var index in m_Realised.Keys)
            {
                if (!wanted.Contains(index))
                    release.Add(index);
            }

            foreach (var index in release)
                Release(index);

            foreach (var index in wanted)
            {
                if (m_Realised.ContainsKey(index))
                    continue;
                var item = m_Pool.Count > 0 ? m_Pool.Pop() : CreateItem();
                item.style.display = DisplayStyle.Flex;
                m_Realised[index] = item;
                Bind(item, index);
            }

            foreach (var pair in m_Realised)
                pair.Value.style.top = pair.Key * rowSize;
        }

        void Release(int index)
        {
            var item = m_Realised[index];
            m_Realised.Remove(index);
            item.style.display = DisplayStyle.None;
            m_Pool.Push(item);
        }

        void AddPinned(HashSet<int> wanted, string id)
        {
            var i = IndexOf(id);
            if (i >= 0)
                wanted.Add(i);
        }

        TreeViewItem CreateItem()
        {
            var item = new TreeViewItem();
            item.AddToClassList(rowUssClassName);
            item.RegisterCallback<GeometryChangedEvent>(OnRowGeometryChanged);
            m_RowsContainer.Add(item);
            return item;
        }

        void OnRowGeometryChanged(GeometryChangedEvent evt)
        {
            var height = evt.newRect.height;
            if (height > 0 && !float.IsNaN(height) && Mathf.Abs(height - m_RowSize) > 0.5f)
            {
                m_RowSize = height;
                Refresh();
            }
        }

        void Bind(TreeViewItem item, int index)
        {
            var row = m_Rows[index];
            item.userData = index;
            item.EnableInClassList(placeholderUssClassName, row.placeholder);
            item.showCaret = row.expandable;
            item.expanded = row.expanded;
            item.selected = !row.placeholder && m_SelectedSet.Contains(row.id);
            item.draggable = false;
            // Indent by inline margin on the caret gutter: the depth classes only reach level 3 and
            // nesting, which TreeList indents by, does not exist in a flat list.
            var gutter = item.Q(className: TreeViewItem.caretGutterUssClassName);
            if (gutter != null)
                gutter.style.marginLeft = row.level * m_Indent;
            if (bindRow != null)
                bindRow(item, row, index);
            else
                item.label = row.placeholder ? string.Empty : row.id;
        }

        // ── Focus ──────────────────────────────────────────────────────────

        void OnFocusIn(FocusInEvent evt)
        {
            var index = FindRow(evt.target as VisualElement, out _);
            var id = index >= 0 && !m_Rows[index].placeholder ? m_Rows[index].id : null;
            if (id == m_FocusedId)
                return;
            m_FocusedId = id;
            Refresh();
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            // Focus moving to nothing keeps the row, so focus can come back to it.
            if (evt.relatedTarget is VisualElement next && !Contains(next) && m_FocusedId != null)
            {
                m_FocusedId = null;
                Refresh();
            }
        }

        // ── Pointer ────────────────────────────────────────────────────────

        int FindRow(VisualElement target, out bool onCaret)
        {
            onCaret = false;
            for (var element = target; element != null && element != this; element = element.hierarchy.parent)
            {
                if (element.ClassListContains(TreeViewItem.caretButtonUssClassName))
                    onCaret = true;
                if (element is TreeViewItem item && item.userData is int index && m_Realised.TryGetValue(index, out var realised) && realised == item)
                    return index;
            }

            return -1;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;
            var index = FindRow(evt.target as VisualElement, out var onCaret);
            if (index < 0 || m_Rows[index].placeholder)
                return;
            var row = m_Rows[index];

            if (onCaret)
            {
                if (row.expandable)
                    onExpandedChange?.Invoke(row.id, !row.expanded);
                evt.StopPropagation();
                return;
            }

            EndPress();
            if (m_SelectionType != SelectionType.Multiple || evt.shiftKey || evt.actionKey)
            {
                Select(row.id, evt.shiftKey, evt.actionKey);
            }
            else
            {
                m_PressId = row.id;
                m_PressOrigin = evt.position;
                m_PressPointerId = evt.pointerId;
                m_PressDragged = false;
                // Watched on the panel root, as TreeList does: a press can leave the tree and come
                // back before its release, and a release or cancel outside has to end it.
                m_PressWatchRoot = panel?.visualTree;
                if (m_PressWatchRoot != null)
                {
                    m_PressWatchRoot.RegisterCallback<PointerMoveEvent>(OnPressMove, TrickleDown.TrickleDown);
                    m_PressWatchRoot.RegisterCallback<PointerUpEvent>(OnPressEnd);
                    m_PressWatchRoot.RegisterCallback<PointerCancelEvent>(OnPressCancel);
                }
            }

            Focus();
            evt.StopPropagation();
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

            var hadPress = m_PressId != null;
            m_PressId = null;
            m_PressPointerId = -1;
            m_PressDragged = false;
            if (hadPress)
                Refresh();
        }

        // Bubble phase on the root, so a release on a row reaches OnPointerUp first.
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

        void OnPressMove(PointerMoveEvent evt)
        {
            if (m_PressId != null && evt.pointerId == m_PressPointerId &&
                ((Vector2)evt.position - m_PressOrigin).sqrMagnitude >= k_PressDragDistance * k_PressDragDistance)
                m_PressDragged = true;
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_PressId == null || evt.pointerId != m_PressPointerId)
                return;
            var id = m_PressId;
            var dragged = m_PressDragged ||
                ((Vector2)evt.position - m_PressOrigin).sqrMagnitude >= k_PressDragDistance * k_PressDragDistance;
            // Resolve the row before ending the press: ending it unpins the row and may recycle it.
            var index = FindRow(evt.target as VisualElement, out _);
            EndPress();
            if (dragged || index < 0 || m_Rows[index].id != id)
                return;
            Select(id);
        }

        // ── Keyboard ───────────────────────────────────────────────────────

        int Step(int from, int direction)
        {
            for (var i = from + direction; i >= 0 && i < m_Rows.Count; i += direction)
            {
                if (!m_Rows[i].placeholder)
                    return i;
            }

            return -1;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            var multiple = m_SelectionType == SelectionType.Multiple;

            if (multiple && evt.actionKey && evt.keyCode == KeyCode.A)
            {
                SelectAll();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                if (onDeleteSelection != null && m_SelectedIds.Count > 0)
                {
                    onDeleteSelection.Invoke(new List<string>(m_SelectedIds));
                    evt.StopPropagation();
                }

                return;
            }

            if (multiple && evt.keyCode == KeyCode.Escape)
            {
                ApplySelection(Array.Empty<string>());
                m_AnchorId = null;
                evt.StopPropagation();
                return;
            }

            var firstIndex = Step(-1, 1);
            if (firstIndex < 0)
                return;
            var lastIndex = Step(m_Rows.Count, -1);
            var index = IndexOf(selectedId);
            var lead = index >= 0 ? m_Rows[index] : null;
            var extend = multiple && evt.shiftKey;

            void GoTo(int target)
            {
                if (target < 0)
                    return;
                Select(m_Rows[target].id, extend);
                ScrollToIndex(target);
            }

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    GoTo(index < 0 ? firstIndex : Math.Max(index, Step(index, 1)));
                    break;
                case KeyCode.UpArrow:
                    GoTo(index < 0 ? firstIndex : Math.Max(firstIndex, Step(index, -1)));
                    break;
                case KeyCode.RightArrow:
                    if (lead == null)
                        GoTo(firstIndex);
                    else if (lead.expandable && !lead.expanded)
                        onExpandedChange?.Invoke(lead.id, true);
                    else if (lead.expandable && index + 1 < m_Rows.Count
                             && !m_Rows[index + 1].placeholder && m_Rows[index + 1].parentId == lead.id)
                        GoTo(index + 1);
                    // Children still loading, or a leaf: stay rather than skip to the next sibling.
                    break;
                case KeyCode.LeftArrow:
                    if (lead == null)
                        GoTo(firstIndex);
                    else if (lead.expandable && lead.expanded)
                        onExpandedChange?.Invoke(lead.id, false);
                    else
                        GoTo(IndexOf(lead.parentId));
                    break;
                case KeyCode.Home:
                    GoTo(firstIndex);
                    break;
                case KeyCode.End:
                    GoTo(lastIndex);
                    break;
                default:
                    return;
            }

            evt.StopPropagation();
        }
    }
}
