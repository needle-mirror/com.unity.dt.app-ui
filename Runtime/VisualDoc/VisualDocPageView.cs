using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    [UxmlElement]
    partial class VisualDocPageView : ScrollView, INavigationScreen
    {
        public new const string ussClassName = "visual-doc-page-view";

        public const string contentScrollViewUssClassName = ussClassName + "__content-scroll-view";

        public const string tocUssClassName = ussClassName + "__toc";

        public const string breadcrumbsUssClassName = ussClassName + "__breadcrumbs";

        public const string contentContainerUssClassName = ussClassName + "__content-container";

        readonly ScrollView m_ContentScrollView;

        readonly Breadcrumbs m_Breadcrumbs;

        internal readonly VisualElement m_ContentContainer;

        readonly TableOfContents m_ToC;

        public VisualDocPageView()
            : base(ScrollViewMode.Vertical)
        {
            AddToClassList(ussClassName);
            horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            m_ContentScrollView = new ScrollView
            {
                name = contentScrollViewUssClassName,
                nestedInteractionKind = NestedInteractionKind.ForwardScrolling,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_ContentScrollView.AddToClassList(contentScrollViewUssClassName);
            Add(m_ContentScrollView);

            m_ToC = new TableOfContents
            {
                name = tocUssClassName,
            };
            m_ToC.AddToClassList(tocUssClassName);
            Add(m_ToC);

            m_Breadcrumbs = new Breadcrumbs
            {
                name = breadcrumbsUssClassName,
            };
            m_Breadcrumbs.AddToClassList(breadcrumbsUssClassName);
            m_ContentScrollView.Add(m_Breadcrumbs);

            m_ContentContainer = new VisualElement
            {
                name = contentContainerUssClassName,
            };
            m_ContentContainer.AddToClassList(contentContainerUssClassName);
            m_ContentScrollView.Add(m_ContentContainer);
        }

        public void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            UpdateBreadcrumbs(destination);
            var kindArgIndex = destination.arguments.FindIndex(a => a.name == "pageKind");
            var refArgIndex = destination.arguments.FindIndex(a => a.name == "pageRef");
            if (kindArgIndex < 0 || refArgIndex < 0)
                return;
            var pageKind = destination.arguments[kindArgIndex].value;
            var pageRef = destination.arguments[refArgIndex].value;

            m_ContentContainer.Clear();
            switch (pageKind)
            {
                case nameof(PageKind.Component):
                {
                    var page = FindPage(pageRef);
                    if (page != null)
                        VisualDocPageBuilder.BuildComponentPage(m_ContentContainer, page);
                    else
                        Debug.LogError($"No documentation page found in the registry for '{pageRef}'.");
                    break;
                }
                case nameof(PageKind.Guide):
                    VisualDocPageBuilder.BuildGuidePage(m_ContentContainer, pageRef);
                    break;
                default:
                    Debug.LogError($"Unknown documentation page kind '{pageKind}'.");
                    break;
            }

            m_ToC.RemoveFromHierarchy();
            m_ToC.Clear();
            m_ContentContainer
                .Query<VisualElement>(className: "section")
                .Where(s => !string.IsNullOrWhiteSpace(s.name))
                .ForEach(s =>
                {
                    m_ToC.Add(new TreeViewItem
                    {
                        label = s.name,
                        userData = s,
                        depth = 1,
                        expanded = false,
                        clickable = new Pressable(evt =>
                        {
                            if (((VisualElement)evt.target).hierarchy.parent is { userData: VisualElement element })
                                element.GetFirstAncestorOfType<ScrollView>()?.ScrollTo(element);
                        })
                    });
                    if (m_ToC.parent == null)
                        Add(m_ToC);
                });
        }

        static VisualDocPageInfo FindPage(string id)
        {
            foreach (var page in VisualDocRegistry.pages)
            {
                if (page.id == id)
                    return page;
            }

            return null;
        }

        void UpdateBreadcrumbs(NavDestination destination)
        {
            var itemIndex = destination.arguments.FindIndex(a => a.name == "treeViewItem");
            if (itemIndex < 0)
                return;

            var item = ((VisualDocNavArgument)destination.arguments[itemIndex]).treeViewItem;
            var current = true;
            while (item != null)
            {
                var crumb = new BreadcrumbItem
                {
                    text = item.DisplayName,
                    userData = item,
                    clickable = new Pressable(OnCrumbClicked)
                };
                crumb.SetEnabled(item.IsBrowsable);
                crumb.isCurrent = current;
                m_Breadcrumbs.Insert(0, crumb);
                current = false;
                item = item.Parent;
                if (item != null)
                {
                    if (item.IsCategory)
                        break;
                    m_Breadcrumbs.Insert(0, new BreadcrumbSeparator {text = ">"});
                }
            }

            m_Breadcrumbs.Insert(0, new BreadcrumbSeparator {text = ">"});
            var home = new BreadcrumbItem
            {
                text = "Docs",
                userData = null,
                clickable = null
            };
            home.SetEnabled(false);
            m_Breadcrumbs.Insert(0, home);
        }

        void OnCrumbClicked(EventBase evt)
        {
            if (evt.target is BreadcrumbItem { userData: TreeViewItemModel item })
            {
                this.FindNavController().Navigate(VisualDocActions.GoTo(item.Name));
            }
        }

        public void OnExit(NavController controller, NavDestination destination, Argument[] args)
        {

        }
    }
}
