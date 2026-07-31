using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.AppUI.Navigation;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.AppUI.VisualDoc
{
    [UxmlElement]
    partial class VisualDocView : VisualElement
    {
        const string k_Uxml = "Packages/com.unity.dt.app-ui/Runtime/VisualDoc/VisualDocView.uxml";

        const string badgeUssClassName = "badge";

        const string categoryVariantUssClassName = "is-category";

        readonly Drawer m_NavDrawer;

        readonly AppBar m_AppBar;

        readonly NavHost m_NavHost;

        readonly VisualElement m_SideBar;

        readonly VisualElement m_Main;

        readonly List<TreeViewItemModel> m_TreeViewData;

        float m_PreviousWidth = 0;

        readonly SearchBar m_SearchBar;

        readonly IconButton m_ToggleDarkModeButton;

        readonly VisualElement m_SearchResultView;

        public override VisualElement contentContainer => m_NavHost?.contentContainer ?? this;

        public VisualDocView()
        {
#if UNITY_EDITOR
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml);
#else
            var template = Resources.Load<VisualTreeAsset>(System.IO.Path.GetFileNameWithoutExtension(k_Uxml));
#endif
            template.CloneTree(this);
            m_TreeViewData = DocTree.BuildTree();

            m_AppBar = this.Q<AppBar>("app-bar");
            m_NavDrawer = this.Q<Drawer>("nav-drawer");
            m_Main = this.Q<VisualElement>("main");
            m_SideBar = this.Q<VisualElement>("sidebar");
            m_NavHost = this.Q<NavHost>("content");
            m_SearchBar = new SearchBar();
            m_ToggleDarkModeButton = new IconButton(null, OnDarkModeToggle) { quiet = true };
            m_SearchResultView = this.Q<VisualElement>("search-results");

            m_SearchBar.RegisterCallback<FocusInEvent>(OnSearchFocusedIn);
            m_SearchBar.RegisterCallback<FocusOutEvent>(OnSearchFocusedOut);
            m_AppBar.title = "App UI Visual Documentation";
            m_AppBar.backButtonTriggered += OnBackButtonClicked;
            m_AppBar.drawerButtonTriggered += OnDrawerButtonClicked;
            m_AppBar.AddAction(m_SearchBar);
            m_AppBar.AddAction(m_ToggleDarkModeButton);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttached);

            m_NavHost.enteredDestination += OnEnter;
            m_NavHost.exitedDestination += OnExit;

            PopulateSideBar(m_NavDrawer);
            PopulateSideBar(m_SideBar);

            m_NavHost.navController.SetGraph(VisualDocNavGraph.instance);
        }

        void OnAttached(AttachToPanelEvent evt)
        {
            m_ToggleDarkModeButton.icon = isDarkMode ? "moon" : "sun-dim";
        }

        bool isDarkMode
        {
            get
            {
                var appUIPanel = GetFirstAncestorOfType<Panel>();
                if (appUIPanel == null)
                    return false;

                return appUIPanel.theme == "dark";
            }
        }

        void OnDarkModeToggle()
        {
            var appUIPanel = GetFirstAncestorOfType<Panel>();
            if (appUIPanel == null)
                return;

            var theme = appUIPanel.theme;
            var newTheme = theme == "dark" ? "light" : "dark";
            appUIPanel.theme = newTheme;
            m_ToggleDarkModeButton.icon = newTheme == "dark" ? "moon" : "sun-dim";
        }

        void OnSearchFocusedIn(FocusInEvent evt)
        {
            m_SearchResultView.style.left = isMobile ? 0 : m_SearchBar.worldBound.x;
            m_SearchResultView.style.top = m_SearchBar.worldBound.y;
            m_SearchResultView.style.width = isMobile ? new Length(100, LengthUnit.Percent) : m_SearchBar.worldBound.width;
            m_SearchResultView.visible = true;
        }

        void OnSearchFocusedOut(FocusOutEvent evt)
        {
            m_SearchResultView.visible = false;
        }

        void PopulateSideBar(VisualElement sidebarElement)
        {
            var scrollView = new ScrollView();
            Populate(scrollView, m_TreeViewData, 0);
            sidebarElement.Add(scrollView);
        }

        void Populate(VisualElement container, List<TreeViewItemModel> items, int depth)
        {
            foreach (var item in items)
            {
                var hasChildren = item.Children.Count > 0;
                var treeViewItem = new TreeViewItem
                {
                    name = item.Name,
                    userData = item,
                    label = item.DisplayName,
                    showCaret = hasChildren && !item.IsCategory,
                    clickable = item.IsBrowsable ? new Pressable(OnTreeViewItemClicked) : (hasChildren && !item.IsCategory) ? new Pressable(OnTreeViewItemCaretClicked) : null,
                    caretClickable = hasChildren && item.IsBrowsable ? new Pressable(OnTreeViewItemCaretClicked) : null,
                    expanded = item.IsCategory,
                    depth = depth,
                };
                treeViewItem.EnableInClassList(categoryVariantUssClassName, item.IsCategory);
                if (item.IsNew)
                {
                    var badge = new LocalizedTextElement("New");
                    badge.AddToClassList(badgeUssClassName);
                    var titleElement = treeViewItem.Q(TreeViewItem.titleLabelUssClassName);
                    var index = titleElement.hierarchy.parent.IndexOf(titleElement);
                    titleElement.hierarchy.parent.Insert(index + 1, badge);
                }
                Populate(treeViewItem.contentContainer, item.Children, depth + 1);
                container.Add(treeViewItem);
            }
        }

        void OnTreeViewItemClicked(EventBase evt)
        {
            if (((VisualElement)evt.target).GetFirstAncestorOfType<TreeViewItem>() is not { } treeViewItem)
                return;
            if (treeViewItem.userData is not TreeViewItemModel item)
                return;
            if (item.Children.Count > 0)
                treeViewItem.expanded = true;
            if (m_NavHost.navController.currentDestination.name == item.Name)
                return;
            m_NavHost.navController.Navigate(VisualDocActions.GoTo(item.Name));
            m_NavDrawer.Close();
        }

        void OnTreeViewItemCaretClicked(EventBase evt)
        {
            if (((VisualElement)evt.target).GetFirstAncestorOfType<TreeViewItem>() is not { } treeViewItem)
                return;
            treeViewItem.ToggleExpand();
        }

        void OnDrawerButtonClicked()
        {
            m_NavDrawer.Toggle();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(m_PreviousWidth, evt.newRect.width))
                return;

            var newWidth = evt.newRect.width;
            m_PreviousWidth = newWidth;
            var root = evt.target as VisualElement;
            if (root == null)
                return;

            root.EnableInClassList("media-query--small-mobile", newWidth < 768);
            root.EnableInClassList("media-query--mobile", newWidth is < 960 and >= 768);
            root.EnableInClassList("media-query--tablet", newWidth is < 1280 and >= 960);
            root.EnableInClassList("media-query--desktop", newWidth >= 1280);

            RefreshAppBar(m_NavHost.navController.currentDestination);
            if (!isMobile)
                m_NavDrawer.isOpen = false;
            m_NavDrawer.SetEnabled(isMobile);
        }

        bool isMobile => m_PreviousWidth < 960;

        void OnBackButtonClicked()
        {
            m_NavHost.navController.PopBackStack();
        }

        void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            RefreshAppBar(destination);
            RefreshSelectedItem(destination);
        }

        void OnExit(NavController controller, NavDestination destination, Argument[] args)
        {
            // nothing to do
        }

        void RefreshAppBar(NavDestination destination)
        {
            var controller = m_NavHost.navController;

            m_AppBar.showDrawerButton = !controller.canGoBack && isMobile;
            m_AppBar.showBackButton = controller.canGoBack && isMobile;
        }

        void RefreshSelectedItem(NavDestination destination)
        {
            var destinationPath = destination ? destination.name : string.Empty;
            this.Query<TreeViewItem>()
                .ForEach(item =>
                {
                    if (item.userData is TreeViewItemModel treeViewItem)
                        item.selected = !string.IsNullOrEmpty(destinationPath) && treeViewItem.Name == destinationPath;
                });
        }
    }
}
