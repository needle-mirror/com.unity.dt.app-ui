using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A navigation component that enables easy switching between top-level destinations.
    /// </summary>
    /// <remarks>
    /// The Bottom Navigation Bar provides quick navigation between top-level destinations in your app. It
    /// displays 3-5 destinations at the bottom of the screen, each represented by an icon and an optional text
    /// label.
    ///
    /// Bottom navigation bars are typically used in mobile applications where quick switching between main
    /// features is important. They should be used for important destinations that need to be accessed
    /// frequently.
    ///
    /// Note: Bottom navigation should be used only for top-level destinations that need to be accessible from
    /// anywhere in the app. For other navigation patterns, consider using tabs, drawers, or other navigation
    /// components.
    ///
    /// The Bottom Navigation Bar consists of individual BottomNavBarItem elements, each representing a
    /// destination in your application. The BottomNavBar itself is a simple container, while BottomNavBarItem
    /// provides the actual functionality with properties for icons, labels, selection states, and click
    /// handling.
    ///
    /// ## Anatomy
    /// Basic Bottom Navigation Bar:
    /// ```xml
    /// &lt;appui:BottomNavBar&gt;
    ///     &lt;appui:BottomNavBarItem icon="home" label="Home" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="search" label="Search" selected="true" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="favorite" label="Favorites" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="user" label="Profile" /&gt;
    /// &lt;/appui:BottomNavBar&gt;
    /// ```
    ///
    /// Icon-Only Bottom Navigation:
    /// ```xml
    /// &lt;appui:BottomNavBar&gt;
    ///     &lt;appui:BottomNavBarItem icon="dashboard" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="analytics" selected="true" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="notifications" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="settings" /&gt;
    /// &lt;/appui:BottomNavBar&gt;
    /// ```
    ///
    /// With Badge Indicators:
    /// ```xml
    /// &lt;appui:BottomNavBar&gt;
    ///     &lt;appui:BottomNavBarItem icon="home" label="Home" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="mail" label="Messages" badge-content="3" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="notifications" label="Alerts" show-badge="true" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="user" label="Profile" selected="true" /&gt;
    /// &lt;/appui:BottomNavBar&gt;
    /// ```
    ///
    /// Five Item Navigation:
    /// ```xml
    /// &lt;appui:BottomNavBar&gt;
    ///     &lt;appui:BottomNavBarItem icon="home" label="Home" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="explore" label="Explore" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="add" label="Create" selected="true" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="favorite" label="Saved" /&gt;
    ///     &lt;appui:BottomNavBarItem icon="user" label="Profile" /&gt;
    /// &lt;/appui:BottomNavBar&gt;
    /// ```
    /// </remarks>
    /// <example>
    /// <para>Basic Bottom Navigation Bar with three items</para>
    /// <code lang="xml"><![CDATA[
    /// <BottomNavBar>
    ///     <BottomNavBarItem icon="home" label="Home" />
    ///     <BottomNavBarItem icon="search" label="Search" />
    ///     <BottomNavBarItem icon="settings" label="Settings" />
    /// </BottomNavBar>
    /// ]]></code>
    /// <para>Creating a Bottom Navigation Bar programmatically</para>
    /// <code lang="csharp"><![CDATA[
    /// var bottomNav = new BottomNavBar();
    ///
    /// var homeItem = new BottomNavBarItem("home", "Home", () => {
    ///     Debug.Log("Home clicked");
    /// });
    ///
    /// var searchItem = new BottomNavBarItem("search", "Search", () => {
    ///     Debug.Log("Search clicked");
    /// });
    ///
    /// var settingsItem = new BottomNavBarItem("settings", "Settings", () => {
    ///     Debug.Log("Settings clicked");
    /// });
    ///
    /// bottomNav.Add(homeItem);
    /// bottomNav.Add(searchItem);
    /// bottomNav.Add(settingsItem);
    /// ]]></code>
    /// <para>Bottom Navigation Bar integrated with Navigation System</para>
    /// <code lang="csharp"><![CDATA[
    /// public class MainNavController : INavVisualController
    /// {
    ///     public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    ///     {
    ///         var homeItem = new BottomNavBarItem("home", "Home", () =>
    ///             navController.Navigate("home"));
    ///         var searchItem = new BottomNavBarItem("search", "Search", () =>
    ///             navController.Navigate("search"));
    ///
    ///         bottomNavBar.Add(homeItem);
    ///         bottomNavBar.Add(searchItem);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("nav-components", id = "bottom-navigation-bar", displayName = "Bottom Navigation Bar")]
    public partial class BottomNavBar : BaseVisualElement
    {
        /// <summary>
        /// The BottomNavBar's USS class name.
        /// </summary>
        public const string ussClassName = "appui-bottom-navbar";

        /// <summary>
        /// The content container of the BottomNavBar.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BottomNavBar()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
        }
    }

    /// <summary>
    /// A single destination within a BottomNavBar, providing an icon, label, selection state, and click handling.
    /// </summary>
    [UxmlElement]
    public partial class BottomNavBarItem : BaseVisualElement, IPressable
    {
        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId isSelectedProperty = nameof(isSelected);

        internal static readonly BindingId iconVariantProperty = nameof(iconVariant);

        internal static readonly BindingId selectedIconVariantProperty = nameof(selectedIconVariant);

        internal static readonly BindingId clickableProperty = nameof(clickable);
        /// <summary>
        /// The BottomNavBarItem's USS class name.
        /// </summary>
        public const string ussClassName = "appui-bottom-navbar-item";

        /// <summary>
        /// The BottomNavBarItem's icon USS class name.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The BottomNavBarItem's label USS class name.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Icon m_Icon;

        LocalizedTextElement m_Label;

        Pressable m_Clickable;

        IconVariant m_SelectedIconVariant = IconVariant.Regular;

        IconVariant m_IconVariant = IconVariant.Regular;

        /// <summary>
        /// The BottomNavBarItem's icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Bottom Navigation Bar")]
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
        /// The BottomNavBarItem's label.
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
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Whether the BottomNavBarItem is selected.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isSelected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = isSelected != value;
                EnableInClassList(Styles.selectedUssClassName, value);
                m_Icon.variant = value ? m_SelectedIconVariant : m_IconVariant;
                if (changed)
                    NotifyPropertyChanged(in isSelectedProperty);
            }
        }

        /// <summary>
        /// The BottomNavBarItem's icon variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant iconVariant
        {
            get => m_IconVariant;
            set
            {
                var changed = m_IconVariant != value;
                m_IconVariant = value;

                if (!isSelected)
                    m_Icon.variant = value;

                if (changed)
                    NotifyPropertyChanged(in iconVariantProperty);
            }
        }

        /// <summary>
        /// The BottomNavBarItem's selected icon variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant selectedIconVariant
        {
            get => m_SelectedIconVariant;
            set
            {
                var changed = m_SelectedIconVariant != value;
                m_SelectedIconVariant = value;

                if (isSelected)
                    m_Icon.variant = value;

                if (changed)
                    NotifyPropertyChanged(in selectedIconVariantProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this BottomNavBarItem.
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
        /// Default constructor.
        /// </summary>
        public BottomNavBarItem()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="icon"> The BottomNavBarItem's icon. </param>
        /// <param name="label"> The BottomNavBarItem's label. </param>
        /// <param name="clickHandler"> The BottomNavBarItem's click handler. </param>
        public BottomNavBarItem(string icon, string label, Action clickHandler)
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Pressable(clickHandler);

            m_Icon = new Icon { iconName = icon, variant = IconVariant.Regular, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);

            m_Label = new LocalizedTextElement(label) { pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            this.label = label;
            this.icon = icon;
        }
    }
}
