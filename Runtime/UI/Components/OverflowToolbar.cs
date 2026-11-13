using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A horizontal strip of controls that collapses whatever does not fit into a trailing caret,
    /// whose menu holds the overflow.
    /// </summary>
    /// <remarks>
    /// Add any elements to an OverflowToolbar and it re-flows them on every resize: leading items win,
    /// trailing items collapse first, and an item is never split — it is either inline or in the menu.
    /// The caret appears only when something has actually collapsed. It lives OUTSIDE the measured
    /// strip and never shrinks, while the strip does, so the width the arithmetic measures against has
    /// the caret already subtracted from it. That is stronger than the registry's explicit
    /// reserve-then-drop pass: there is no window in which the bar can be one item too wide.
    ///
    /// The web component observes the container with a `ResizeObserver`; the UI Toolkit equivalent is
    /// <see cref="GeometryChangedEvent"/> on the bar, which is what drives <see cref="Reflow"/> here.
    /// Only the bar is observed, never the items — watching the items would feed collapse back into
    /// measurement and oscillate.
    ///
    /// Each item's natural outer width (its layout width plus its margins) is cached while it is
    /// inline. An item that has never been measured forces a fully expanded pass so it can be measured
    /// before the next collapse, exactly as the registry does. The bar therefore assumes stable items
    /// whose inline width does not depend on whether they are collapsed.
    ///
    /// Collapsed items stay children of the bar with `display: none`, so their order is never lost.
    /// Opening the caret moves that trailing run into a <see cref="Menu"/> shown through
    /// <see cref="MenuBuilder"/> — the same popup every other App UI menu uses — and dismissing it moves
    /// them straight back and re-flows.
    /// </remarks>
    /// <example>
    /// <para>A tool strip in UXML. Shrink the bar and the trailing buttons move into the caret menu.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:OverflowToolbar style="width: 240px;">
    ///     <appui:ActionButton icon="pen" />
    ///     <appui:ActionButton icon="copy" />
    ///     <appui:ActionButton icon="delete" />
    ///     <appui:ActionButton icon="magnifying-glass" />
    /// </appui:OverflowToolbar>
    /// ]]></code>
    /// <para>Built in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var bar = new OverflowToolbar { overflowLabel = "More tools" };
    /// foreach (var tool in tools)
    ///     bar.Add(new ActionButton { icon = tool.icon, clickable = { clicked = () => tool.Activate() } });
    ///
    /// Debug.Log($"{bar.visibleItemCount} of {bar.itemCount} tools fit inline");
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class OverflowToolbar : BaseVisualElement
    {
        internal static readonly BindingId overflowLabelProperty = nameof(overflowLabel);

        internal static readonly BindingId overflowIconProperty = nameof(overflowIcon);

        /// <summary>
        /// The OverflowToolbar main styling class.
        /// </summary>
        public const string ussClassName = "appui-overflow-toolbar";

        /// <summary>
        /// The styling class of the container the inline items live in.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The styling class applied to every item added to the bar.
        /// </summary>
        /// <remarks>
        /// The bar tags its children rather than styling them through a universal child selector, so an
        /// item keeps its inline sizing when it is re-parented into the overflow menu and back.
        /// </remarks>
        public const string itemUssClassName = ussClassName + "__item";

        /// <summary>
        /// The styling class of the trailing overflow caret.
        /// </summary>
        public const string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The styling class applied while at least one item has collapsed.
        /// </summary>
        public const string overflowingUssClassName = ussClassName + "--overflowing";

        /// <summary>
        /// The styling class applied to the <see cref="Menu"/> holding the collapsed items.
        /// </summary>
        public const string menuUssClassName = ussClassName + "__menu";

        readonly VisualElement m_Container;

        readonly ActionButton m_Caret;

        readonly Dictionary<VisualElement, float> m_Widths = new Dictionary<VisualElement, float>();

        readonly List<VisualElement> m_Collapsed = new List<VisualElement>();

        int m_VisibleCount;

        MenuBuilder m_MenuBuilder;

        string m_OverflowIcon = "caret-down";

        /// <inheritdoc />
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The accessible label and tooltip of the overflow caret.
        /// </summary>
        [Tooltip("The accessible label and tooltip of the overflow caret.")]
        [CreateProperty]
        [UxmlAttribute]
        public string overflowLabel
        {
            get => m_Caret.tooltip;
            set
            {
                var changed = m_Caret.tooltip != value;
                m_Caret.tooltip = value;

                if (changed)
                    NotifyPropertyChanged(in overflowLabelProperty);
            }
        }

        /// <summary>
        /// The name of the icon drawn in the overflow caret. Defaults to `caret-down`.
        /// </summary>
        [Tooltip("The name of the icon drawn in the overflow caret.")]
        [CreateProperty]
        [UxmlAttribute]
        public string overflowIcon
        {
            get => m_OverflowIcon;
            set
            {
                var changed = m_OverflowIcon != value;
                m_OverflowIcon = value;
                m_Caret.icon = value;

                if (changed)
                    NotifyPropertyChanged(in overflowIconProperty);
            }
        }

        /// <summary>
        /// The total number of items in the bar, inline and collapsed.
        /// </summary>
        public int itemCount => m_Container.childCount;

        /// <summary>
        /// The number of items currently shown inline. `itemCount` when nothing has collapsed.
        /// </summary>
        public int visibleItemCount => m_VisibleCount;

        /// <summary>
        /// Whether at least one item has collapsed into the caret menu.
        /// </summary>
        public bool overflowing => m_VisibleCount < m_Container.childCount;

        /// <summary>
        /// Whether the overflow menu is currently open.
        /// </summary>
        public bool menuOpen => m_MenuBuilder != null;

        /// <summary>
        /// Raised after a re-flow that changed how many items are inline.
        /// </summary>
        public event Action<int> visibleItemCountChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OverflowToolbar()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            m_Caret = new ActionButton(OpenOverflowMenu) { name = caretUssClassName, quiet = true };
            m_Caret.AddToClassList(caretUssClassName);
            hierarchy.Add(m_Caret);

            overflowIcon = m_OverflowIcon;
            overflowLabel = "More";

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            m_VisibleCount = 0;
            SetCaretVisible(false);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt) => Reflow();

        void OnDetachedFromPanel(DetachFromPanelEvent evt) => CloseOverflowMenu();

        void OnGeometryChanged(GeometryChangedEvent evt) => Reflow();

        /// <summary>
        /// Recomputes how many items fit inline and collapses the rest.
        /// </summary>
        /// <remarks>
        /// This runs automatically whenever the bar resizes or is attached. Call it directly only after
        /// mutating the items outside a layout pass. It is a no-op while the overflow menu is open,
        /// because the collapsed items are then parented to the menu and cannot be measured.
        /// </remarks>
        public void Reflow()
        {
            if (m_MenuBuilder != null)
                return;

            var count = m_Container.childCount;
            RefreshChildMarkers();

            if (count == 0)
            {
                SetVisibleCount(0);
                SetCaretVisible(false);
                return;
            }

            MeasureInlineItems();

            var available = m_Container.contentRect.width;
            // No measurable width yet (first layout pass, or a display:none ancestor): keep everything
            // inline rather than collapsing the whole bar into a caret.
            if (float.IsNaN(available) || available <= 0f)
            {
                ShowAll(count);
                return;
            }

            // An item we have never measured forces a fully expanded pass so it renders and gets a
            // width; the next pass collapses with real numbers.
            for (var i = 0; i < count; i++)
            {
                if (!m_Widths.ContainsKey(m_Container[i]))
                {
                    ShowAll(count);
                    return;
                }
            }

            var used = 0f;
            var fit = 0;
            for (var i = 0; i < count; i++)
            {
                var w = m_Widths[m_Container[i]];
                if (used + w > available)
                    break;
                used += w;
                fit++;
            }

            SetVisibleCount(fit);
            SetCaretVisible(fit < count);
        }

        void ShowAll(int count)
        {
            SetVisibleCount(count);
            SetCaretVisible(false);
        }

        void MeasureInlineItems()
        {
            for (var i = 0; i < m_Container.childCount; i++)
            {
                var child = m_Container[i];
                if (child.resolvedStyle.display == DisplayStyle.None)
                    continue;

                var width = OuterWidth(child);
                if (width > 0f)
                    m_Widths[child] = width;
            }
        }

        static float OuterWidth(VisualElement element)
        {
            var w = element.layout.width;
            if (float.IsNaN(w))
                return 0f;

            var left = element.resolvedStyle.marginLeft;
            var right = element.resolvedStyle.marginRight;
            return w + (float.IsNaN(left) ? 0f : left) + (float.IsNaN(right) ? 0f : right);
        }

        void SetVisibleCount(int value)
        {
            var count = m_Container.childCount;
            value = Mathf.Clamp(value, 0, count);

            for (var i = 0; i < count; i++)
                m_Container[i].EnableInClassList(Styles.hiddenUssClassName, i >= value);

            if (m_VisibleCount == value)
                return;

            m_VisibleCount = value;
            visibleItemCountChanged?.Invoke(value);
        }

        void SetCaretVisible(bool value)
        {
            m_Caret.EnableInClassList(Styles.hiddenUssClassName, !value);
            EnableInClassList(overflowingUssClassName, value);
        }

        void RefreshChildMarkers()
        {
            for (var i = 0; i < m_Container.childCount; i++)
            {
                var child = m_Container[i];
                child.AddToClassList(itemUssClassName);
                child.EnableInClassList(Styles.firstChildUssClassName, i == 0);
            }
        }

        /// <summary>
        /// Opens the overflow menu, moving the collapsed items into it.
        /// </summary>
        /// <remarks>
        /// Does nothing when nothing has collapsed or the menu is already open. The menu is a plain
        /// <see cref="Menu"/> shown through <see cref="MenuBuilder"/>, anchored to the caret.
        /// </remarks>
        public void OpenOverflowMenu()
        {
            if (m_MenuBuilder != null || !overflowing)
                return;

            var menu = new Menu();
            menu.AddToClassList(menuUssClassName);

            m_Collapsed.Clear();
            for (var i = m_Container.childCount - 1; i >= m_VisibleCount; i--)
            {
                var item = m_Container[i];
                m_Collapsed.Insert(0, item);
                m_Container.Remove(item);
            }

            foreach (var item in m_Collapsed)
            {
                item.RemoveFromClassList(Styles.hiddenUssClassName);
                menu.Add(item);
            }

            m_MenuBuilder = MenuBuilder.Build(m_Caret, menu);
            m_MenuBuilder.dismissed += OnMenuDismissed;
            m_MenuBuilder.Show();
        }

        /// <summary>
        /// Closes the overflow menu if it is open, returning the collapsed items to the bar.
        /// </summary>
        public void CloseOverflowMenu()
        {
            m_MenuBuilder?.Dismiss(DismissType.Manual);
        }

        void OnMenuDismissed(MenuBuilder builder, DismissType reason)
        {
            builder.dismissed -= OnMenuDismissed;
            m_MenuBuilder = null;

            foreach (var item in m_Collapsed)
            {
                item.RemoveFromHierarchy();
                m_Container.Add(item);
            }
            m_Collapsed.Clear();

            Reflow();
        }
    }
}
