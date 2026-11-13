using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container for menu items organized in a vertical list.
    /// </summary>
    /// <remarks>
    /// The Menu component is a container that displays a vertical list of menu items, sections, and dividers.
    /// It provides a structured way to present actions, navigation options, or selectable items to users.
    ///
    /// Menus are commonly used in context menus, dropdown menus, navigation sidebars, and command palettes.
    /// They support keyboard navigation, nested sub-menus, and scrolling for long lists of items.
    ///
    /// The Menu component automatically handles focus management and keyboard navigation (Up/Down arrows)
    /// between menu items, making it fully keyboard accessible.
    ///
    /// Note: Menu has picking mode set to ignore by default, but is focusable to support keyboard navigation.
    /// </remarks>
    /// <example>
    /// <para>Basic Menu Structure &#8212; Creating a simple file menu with items and a divider.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuItem label="New File" icon="file" />
    ///     <ui:MenuItem label="Open" icon="folder-open" shortcut="Ctrl+O" />
    ///     <ui:MenuItem label="Save" icon="save" shortcut="Ctrl+S" />
    ///     <ui:MenuDivider />
    ///     <ui:MenuItem label="Exit" icon="exit" />
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Menu with Sections &#8212; Organizing menu items into titled sections.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuSection title="Edit">
    ///         <ui:MenuItem label="Undo" shortcut="Ctrl+Z" />
    ///         <ui:MenuItem label="Redo" shortcut="Ctrl+Y" />
    ///     </ui:MenuSection>
    ///     <ui:MenuDivider />
    ///     <ui:MenuSection title="Selection">
    ///         <ui:MenuItem label="Select All" shortcut="Ctrl+A" />
    ///         <ui:MenuItem label="Deselect" />
    ///     </ui:MenuSection>
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Creating a Menu programmatically &#8212; Building a menu with code and managing sub-menus.</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Add regular items
    /// menu.Add(new MenuItem { label = "Cut", icon = "scissors", shortcut = "Ctrl+X" });
    /// menu.Add(new MenuItem { label = "Copy", icon = "copy", shortcut = "Ctrl+C" });
    /// menu.Add(new MenuItem { label = "Paste", icon = "clipboard", shortcut = "Ctrl+V" });
    ///
    /// // Add a divider
    /// menu.Add(new MenuDivider());
    ///
    /// // Add a selectable item
    /// var item = new MenuItem { label = "Show Grid", selectable = true, value = true };
    /// menu.Add(item);
    ///
    /// // Close all sub-menus when needed
    /// menu.CloseSubMenus();
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups")]
    public partial class Menu : BaseVisualElement
    {
        /// <summary>
        /// The Menu main styling class.
        /// </summary>
        public const string ussClassName = "appui-menu";

        /// <summary>
        /// The Menu container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Menu selectable mode styling class.
        /// </summary>
        public const string selectableUssClassName = ussClassName + "--selectable";

        readonly ScrollView m_ScrollView;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Menu()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = true;

            m_ScrollView = new ScrollView
            {
                name = containerUssClassName,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            m_ScrollView.AddToClassList(containerUssClassName);
            hierarchy.Add(m_ScrollView);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        internal VisualElement firstMenuItem =>
            this.Query<MenuItem>().Enabled().First() as VisualElement ?? this.Query<PickerItem>().Enabled().First();

        internal VisualElement lastMenuItem =>
            this.Query<MenuItem>().Enabled().Last() as VisualElement ?? this.Query<PickerItem>().Enabled().Last();

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape || evt.target != this)
                return;

            evt.StopPropagation();

            // Focus on the surface itself: Up and End start from the last row, anything else
            // from the first, as the registry's walkRows does.
            // Through the same filtered rows as the walk, so a row that cannot take focus is never
            // handed focus; a Picker's menu has no MenuItem rows and keeps its PickerItem fallback.
            var rows = GetFocusableRows();
            var last = evt.keyCode is KeyCode.UpArrow or KeyCode.End;
            if (rows.Count > 0)
                rows[last ? rows.Count - 1 : 0].Focus();
            else if (last)
                lastMenuItem?.Focus();
            else
                firstMenuItem?.Focus();
        }

        /// <summary>
        /// The rows the arrow keys walk, in order: this menu's own <see cref="MenuItem"/>s that can
        /// take focus. A disabled, invisible or non-focusable row is skipped because it cannot be
        /// focused, a row under a <c>display: none</c> ancestor because it is not on screen, and a
        /// sub-menu's rows because they belong to that sub-menu.
        /// </summary>
        internal List<MenuItem> GetFocusableRows()
        {
            var rows = new List<MenuItem>();
            this.Query<MenuItem>().ForEach(item =>
            {
                if (item.canGrabFocus && item.enabledInHierarchy && IsDisplayed(item) &&
                    item.GetFirstAncestorOfType<Menu>() == this)
                    rows.Add(item);
            });
            return rows;
        }

        bool IsDisplayed(VisualElement element)
        {
            for (var e = element; e != null && e != this; e = e.hierarchy.parent)
            {
                if (e.resolvedStyle.display == DisplayStyle.None)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// The menu keyboard contract, mirroring the registry's <c>walkRows</c>: Down and Up step
        /// through the focusable rows and wrap, Home and End jump to the ends.
        /// </summary>
        /// <param name="current">The row focus is on.</param>
        /// <param name="keyCode">The key pressed.</param>
        /// <returns>True when the key belongs to the walk.</returns>
        internal bool WalkRows(MenuItem current, KeyCode keyCode)
        {
            if (keyCode is not (KeyCode.DownArrow or KeyCode.UpArrow or KeyCode.Home or KeyCode.End))
                return false;

            var rows = GetFocusableRows();
            if (rows.Count == 0)
                return true;

            var index = rows.IndexOf(current);
            int next;
            if (keyCode == KeyCode.Home)
                next = 0;
            else if (keyCode == KeyCode.End)
                next = rows.Count - 1;
            else if (index == -1)
                next = keyCode == KeyCode.DownArrow ? 0 : rows.Count - 1;
            else
                next = (index + (keyCode == KeyCode.DownArrow ? 1 : -1) + rows.Count) % rows.Count;

            rows[next].Focus();
            return true;
        }

        /// <summary>
        /// The content container of the Menu.
        /// </summary>
        public override VisualElement contentContainer => m_ScrollView.contentContainer;

        /// <summary>
        /// The parent MenuItem of this Menu (if this Menu is a sub-menu).
        /// </summary>
        public MenuItem parentItem { get; internal set; } = null;

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (panel != null)
            {
                var query = this.Query<MenuItem>().Where(item => item.selectable).Build();
                EnableInClassList(selectableUssClassName, query.ToList().Count > 0);
            }
        }

        /// <summary>
        /// Close all sub-menus.
        /// </summary>
        public void CloseSubMenus()
        {
            foreach (var child in Children())
            {
                switch (child)
                {
                    case MenuSection menuSection:
                    {
                        foreach (var menuItem in menuSection.GetChildren<MenuItem>(false))
                        {
                            if (menuItem.subMenu != null)
                                menuItem.CloseSubMenus(Vector2.negativeInfinity, menuItem.subMenu);
                        }

                        break;
                    }
                    case MenuItem menuItem:
                    {
                        if (menuItem.subMenu != null)
                            menuItem.CloseSubMenus(Vector2.negativeInfinity, menuItem.subMenu);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get all the MenuItems of this Menu (including sub-menus).
        /// </summary>
        /// <returns> The list of MenuItems. </returns>
        public IEnumerable<MenuItem> GetMenuItems()
        {
            return this.GetChildren<MenuItem>(true);
        }

    }
}
