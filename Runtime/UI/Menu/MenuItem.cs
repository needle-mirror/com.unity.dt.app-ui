using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// An interactive item within a menu that triggers actions or displays sub-menus.
    /// </summary>
    /// <remarks>
    /// MenuItem is an interactive element designed to be used within <see cref="Menu"/> components. Each item can
    /// display a label, icon, keyboard shortcut, and optional checkmark for selection state.
    ///
    /// Menu items can be configured in several ways: as standard action items that trigger events when clicked, as
    /// selectable items with checkmarks to indicate state, or as parent items that open sub-menus when activated.
    ///
    /// The component supports full keyboard navigation with Up/Down arrow keys to move between items, and Left/Right
    /// arrow keys to navigate into or out of sub-menus (respecting text direction).
    ///
    /// When a MenuItem has a sub-menu attached, a small caret icon appears automatically as a visual indicator. The
    /// sub-menu opens on click or hover, and can be navigated with keyboard controls.
    /// </remarks>
    /// <example>
    /// <para>Basic Menu Items &#8212; Standard menu items with labels, icons, and shortcuts.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuItem label="New" icon="file" shortcut="Ctrl+N" />
    ///     <ui:MenuItem label="Open" icon="folder-open" shortcut="Ctrl+O" />
    ///     <ui:MenuItem label="Recent Files" icon="clock" />
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Selectable Menu Items &#8212; Toggle menu items for editor preferences.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuItem label="Word Wrap" selectable="true" value="true" />
    ///     <ui:MenuItem label="Show Line Numbers" selectable="true" value="false" />
    ///     <ui:MenuItem label="Show Minimap" selectable="true" value="true" />
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Menu with Sub-menus &#8212; Creating nested menus for hierarchical actions.</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Create a parent item with sub-menu
    /// var viewItem = new MenuItem { label = "View", icon = "eye" };
    /// var viewSubMenu = new Menu();
    /// viewSubMenu.Add(new MenuItem { label = "Zoom In", shortcut = "Ctrl++" });
    /// viewSubMenu.Add(new MenuItem { label = "Zoom Out", shortcut = "Ctrl+-" });
    /// viewSubMenu.Add(new MenuDivider());
    /// viewSubMenu.Add(new MenuItem { label = "Reset Zoom", shortcut = "Ctrl+0" });
    /// viewItem.subMenu = viewSubMenu;
    ///
    /// menu.Add(viewItem);
    /// ]]></code>
    /// <para>Handling Menu Item Actions &#8212; Responding to menu item interactions and state changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Create item and handle selection
    /// var saveItem = new MenuItem { label = "Save", icon = "save", shortcut = "Ctrl+S" };
    /// saveItem.RegisterCallback<ActionTriggeredEvent>(evt => {
    ///     SaveDocument();
    ///     Debug.Log("Document saved");
    /// });
    ///
    /// // Create toggleable item and handle state changes
    /// var gridItem = new MenuItem { label = "Show Grid", selectable = true };
    /// gridItem.RegisterValueChangedCallback(evt => {
    ///     SetGridVisible(evt.newValue);
    ///     Debug.Log($"Grid visibility: {evt.newValue}");
    /// });
    ///
    /// menu.Add(saveItem);
    /// menu.Add(gridItem);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups", id = "menu-item", displayName = "Menu Item")]
    public partial class MenuItem : BaseVisualElement, INotifyValueChanged<bool>, IPressable
    {
        enum FocusStrategy
        {
            None,
            Item,
        }


        internal static readonly BindingId labelProperty = new BindingId(nameof(label));

        internal static readonly BindingId shortcutProperty = new BindingId(nameof(shortcut));

        internal static readonly BindingId iconProperty = new BindingId(nameof(icon));

        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        internal static readonly BindingId selectableProperty = new BindingId(nameof(selectable));

        internal static readonly BindingId activeProperty = new BindingId(nameof(active));

        internal static readonly BindingId subMenuProperty = new BindingId(nameof(subMenu));

        internal static readonly BindingId hasSubMenuProperty = new BindingId(nameof(hasSubMenu));

        internal static readonly BindingId clickableProperty = new BindingId(nameof(clickable));


        const int k_DefaultOpenSubMenuDelay = 300;

        static readonly Stack<Menu> k_SubMenuStack = new Stack<Menu>();

        internal const string checkmarkIconName = "check";

        const string k_SubMenuIconName = "sub-menu-indicator";

        /// <summary>
        /// The MenuItem main styling class.
        /// </summary>
        public const string ussClassName = "appui-menuitem";

        /// <summary>
        /// The MenuItem label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The MenuItem shortcut styling class.
        /// </summary>
        public const string shortcutUssClassName = ussClassName + "__shortcut";

        /// <summary>
        /// The MenuItem icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The MenuItem checkmark styling class.
        /// </summary>
        public const string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The MenuItem submenu mode styling class.
        /// </summary>
        public const string subMenuItemUssClassname = ussClassName + "--submenu";

        /// <summary>
        /// The MenuItem submenu icon styling class.
        /// </summary>
        public const string subMenuIconUssClassname = ussClassName + "__submenu-icon";

        /// <summary>
        /// The MenuItem selectable mode styling class.
        /// </summary>
        public const string selectableUssClassname = ussClassName + "--selectable";

        /// <summary>
        /// The MenuItem active styling class.
        /// </summary>
        public const string activeUssClassname = ussClassName + "--active";

        /// <summary>
        /// The content container of the MenuItem.
        /// </summary>
        public override VisualElement contentContainer => m_SubMenuContainer;

        /// <summary>
        /// The event raised when the item's submenu is opened.
        /// </summary>
        public event Action subMenuOpened;

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_Label;

        readonly LocalizedTextElement m_Shortcut;

        bool m_Selected;

        readonly VisualElement m_SubMenuContainer;

        Menu m_SubMenu;

        IVisualElementScheduledItem m_ScheduledItem;

        Pressable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuItem()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Pressable(OnClick);

            var checkmark = new Icon { name = checkmarkUssClassName, iconName = checkmarkIconName, pickingMode = PickingMode.Ignore };
            checkmark.AddToClassList(checkmarkUssClassName);
            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            m_Shortcut = new LocalizedTextElement { name = shortcutUssClassName, pickingMode = PickingMode.Ignore };
            m_Shortcut.AddToClassList(shortcutUssClassName);
            var subMenuIcon = new Icon { name = subMenuIconUssClassname, iconName = k_SubMenuIconName, pickingMode = PickingMode.Ignore };
            subMenuIcon.AddToClassList(subMenuIconUssClassname);
            hierarchy.Add(checkmark);
            hierarchy.Add(m_Icon);
            hierarchy.Add(m_Label);
            hierarchy.Add(m_Shortcut);
            hierarchy.Add(subMenuIcon);

            this.AddManipulator(new KeyboardFocusController(OnFocusIn, OnFocusIn, OnFocusOut));

            m_SubMenuContainer = new VisualElement { style = { display = DisplayStyle.None } };
            hierarchy.Add(m_SubMenuContainer);

            selectable = false;
            active = false;
            icon = null;
            label = null;
            subMenu = null;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<PointerOverEvent>(OnEntered);
            RegisterCallback<PointerOutEvent>(OnLeft);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnFocusIn(FocusInEvent evt)
        {
            if (GetFirstAncestorOfType<ScrollView>() is {} scrollView)
                scrollView.ScrollTo(this);

            if (subMenu != null)
                ScheduleOpenSubMenu(k_DefaultOpenSubMenuDelay, FocusStrategy.None);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target != this)
                return;

            var dir = this.GetContext<DirContext>()?.dir ?? Dir.Ltr;
            var menu = GetFirstAncestorOfType<Menu>();

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    evt.StopPropagation();
                    if (menu.lastMenuItem != this)
                        focusController.FocusNextInDirectionEx(this, VisualElementFocusChangeDirection.right);
                    break;
                case KeyCode.UpArrow:
                    evt.StopPropagation();
                    if (menu.firstMenuItem != this)
                        focusController.FocusNextInDirectionEx(this, VisualElementFocusChangeDirection.left);
                    break;
                case KeyCode.RightArrow when dir is Dir.Ltr:
                case KeyCode.LeftArrow when dir is Dir.Rtl:
                    evt.StopPropagation();
                    if (hasSubMenu)
                        clickable?.SimulateSingleClickInternal(evt);
                    break;
                case KeyCode.LeftArrow when dir is Dir.Ltr:
                case KeyCode.RightArrow when dir is Dir.Rtl:
                    evt.StopPropagation();
                    if (menu is { parentItem: { } item })
                        item.Focus();
                    break;
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_SubMenuContainer.childCount > 0)
            {
                subMenu = (Menu)m_SubMenuContainer.ElementAt(0);
                subMenu.parentItem = this;
                m_SubMenuContainer.Remove(subMenu);
            }
        }

        void OnEntered(PointerOverEvent evt)
        {
            var menu = GetFirstAncestorOfType<Menu>();
            if (menu != null)
            {
                foreach (var menuItem in menu.GetMenuItems())
                {
                    if (menuItem.subMenu != null && menuItem != this)
                        CloseSubMenus(evt.localPosition, menuItem.subMenu);
                }
            }

            if (UnityEngine.Device.Application.isMobilePlatform)
                return;

            if (subMenu != null)
                ScheduleOpenSubMenu(k_DefaultOpenSubMenuDelay, FocusStrategy.None);
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            if (subMenu != null && evt.relatedTarget is VisualElement el && el.GetFirstAncestorOfType<Menu>() != subMenu)
            {
                m_ScheduledItem?.Pause();
                CloseSubMenus(Vector2.negativeInfinity, subMenu);
            }
        }

        void OnLeft(PointerOutEvent evt)
        {
            if (UnityEngine.Device.Application.isMobilePlatform)
                return;

            if (subMenu != null)
            {
                m_ScheduledItem?.Pause();
                CloseSubMenus(evt.localPosition, subMenu);
            }
        }

        void ScheduleOpenSubMenu(int delayMs, FocusStrategy strategy)
        {
            m_ScheduledItem?.Pause();

            if (!enabledInHierarchy || !enabledSelf)
                return;

            m_ScheduledItem = schedule.Execute(() => OpenSubMenu(strategy));
            if (delayMs > 0)
                m_ScheduledItem.ExecuteLater(delayMs);
        }

        void OpenSubMenu(FocusStrategy strategy)
        {
            m_ScheduledItem?.Pause();
            if (subMenu.parent != null)
            {
                FocusItemOrMenu(subMenu, strategy);
                return;
            }

            var popover = GetFirstAncestorOfType<Popover.PopoverVisualElement>();
            if (popover == null)
            {
                Debug.LogWarning("MenuItem.OpenSubMenu: No PopoverVisualElement ancestor found. " +
                    "Use MenuTrigger or MenuBuilder.Build() to display menus with submenus.");
                return;
            }

            var popoverElement = MenuBuilder.CreateMenuPopoverVisualElement(subMenu).popoverElement;
            popover.popoverElement.parent.hierarchy.Add(popoverElement);
            popoverElement.visible = false;
            popoverElement.style.opacity = 0.00001f;
            popover.schedule.Execute(() =>
            {
                var dir = this.GetContext<DirContext>()?.dir ?? Dir.Ltr;
                var pos = AnchorPopupUtils
                    .ComputePosition(
                        popoverElement,
                        this,
                        GetFirstAncestorOfType<Panel>(),
                        new PositionOptions(dir == Dir.Ltr ? PopoverPlacement.EndTop : PopoverPlacement.StartTop,
                            -4,
                            -8));
                if (!Mathf.Approximately(popoverElement.resolvedStyle.left, pos.left))
                    popoverElement.style.left = pos.left;
                if (!Mathf.Approximately(popoverElement.resolvedStyle.top, pos.top))
                    popoverElement.style.top = pos.top;
                if (!Mathf.Approximately(popoverElement.resolvedStyle.marginLeft, pos.marginLeft))
                    popoverElement.style.marginLeft = pos.marginLeft;
                if (!Mathf.Approximately(popoverElement.resolvedStyle.marginTop, pos.marginTop))
                    popoverElement.style.marginTop = pos.marginTop;

                popoverElement.visible = true;
                popoverElement.style.opacity = 1f;
                popoverElement.schedule.Execute(() =>
                {
                    FocusItemOrMenu(subMenu, strategy);
                    subMenuOpened?.Invoke();
                });
            });

            k_SubMenuStack.Push(subMenu);
        }

        static void FocusItemOrMenu(Menu menu, FocusStrategy strategy)
        {
            switch (strategy)
            {
                case FocusStrategy.None:
                    return;
                case FocusStrategy.Item when menu.firstMenuItem != null:
                    menu.firstMenuItem.Focus();
                    break;
                default:
                    menu.Focus();
                    break;
            }
        }

        internal void CloseSubMenus(Vector2 localMousePosition, Menu targetMenu)
        {
            if (targetMenu?.parent == null)
                return;

            while (k_SubMenuStack.TryPeek(out var stackedMenu))
            {
                var popoverElement = stackedMenu.parent.parent;
                var mousePosition = popoverElement.WorldToLocal(this.LocalToWorld(localMousePosition));
                if (popoverElement.ContainsPoint(mousePosition))
                    break;

                popoverElement.parent.hierarchy.Remove(popoverElement);
                stackedMenu.parent.hierarchy.Remove(stackedMenu);

                k_SubMenuStack.Pop();
                if (stackedMenu == targetMenu)
                    break;
            }
        }

        /// <summary>
        /// Clickable Manipulator for this MenuItem.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = m_Clickable != value;
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// The label text value.
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
        /// The shortcut text value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string shortcut
        {
            get => m_Shortcut.text;
            set
            {
                var changed = m_Shortcut.text != value;
                m_Shortcut.text = value;

                if (changed)
                    NotifyPropertyChanged(in shortcutProperty);
            }
        }

        /// <summary>
        /// The icon to display next to the label.
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
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The selected state of the item.
        /// </summary>
        /// <remarks>You should set the item as <see cref="selectable"/> first to see any result.</remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool value
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                if (m_Selected == value)
                    return;
                using var evt = ChangeEvent<bool>.GetPooled(m_Selected, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                if (selectable)
                    SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// <para>Enable or disable the selectable mode of the item.</para>
        /// <para>
        /// A selectable item is an item with a small checkmark as leading UI element.
        /// </para>
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selectable
        {
            get => ClassListContains(selectableUssClassname);
            set
            {
                var changed = selectable != value;
                EnableInClassList(selectableUssClassname, value);

                if (changed)
                    NotifyPropertyChanged(in selectableProperty);
            }
        }

        /// <summary>
        /// Enable or disable the active mode of the item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool active
        {
            get => ClassListContains(activeUssClassname);
            set
            {
                var changed = active != value;
                EnableInClassList(activeUssClassname, value);

                if (changed)
                    NotifyPropertyChanged(in activeProperty);
            }
        }

        /// <summary>
        /// <para>Sub Menu linked to this item.</para>
        /// <para>
        /// An item with a submenu mode enabled has a small caret as trailing UI element which defines that a sub menu
        /// will appear if you trigger the item's action.
        /// </para>
        /// </summary>
        [CreateProperty]
        public Menu subMenu
        {
            get => m_SubMenu;
            set
            {
                var changed = m_SubMenu != value;
                m_SubMenu = value;
                EnableInClassList(subMenuItemUssClassname, m_SubMenu != null);

                if (changed)
                {
                    NotifyPropertyChanged(in subMenuProperty);
                    NotifyPropertyChanged(in hasSubMenuProperty);
                }
            }
        }

        /// <summary>
        /// Whether the item has a sub menu.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public bool hasSubMenu => subMenu != null;

        /// <summary>
        /// <para>Set the selected state of this item.</para>
        /// <para>
        /// See <see cref="value"/> and <see cref="selectable"/> properties for more info.
        /// </para>
        /// </summary>
        /// <param name="newValue">The new selected state.</param>
        public void SetValueWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
            m_Selected = newValue;
        }

        void OnClick(EventBase e)
        {
            if (selectable)
                value = !value;

            if (subMenu != null)
            {
                var fromKeyboard = e is KeyDownEvent or KeyUpEvent;
                ScheduleOpenSubMenu(0, fromKeyboard ? FocusStrategy.Item : FocusStrategy.None);
            }
            else
            {
                using var evt = ActionTriggeredEvent.GetPooled();
                evt.target = this;
                SendEvent(evt);
            }
        }

    }
}
