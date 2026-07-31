using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Breadcrumbs documentation page.</summary>
    static class BreadcrumbsDemos
    {
        [VisualDocDemo("breadcrumbs")]
        static VisualElement Basic()
        {
            var breadcrumbs = new Breadcrumbs();

            var homeItem = new BreadcrumbItem { text = "Home", url = "/" };
            var projectsItem = new BreadcrumbItem { text = "Projects", url = "/projects" };
            var currentItem = new BreadcrumbItem { text = "Project Alpha", isCurrent = true };

            breadcrumbs.Add(homeItem);
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(projectsItem);
            breadcrumbs.Add(new BreadcrumbSeparator());
            breadcrumbs.Add(currentItem);

            return DemoUtils.Row(breadcrumbs);
        }
    }
}
