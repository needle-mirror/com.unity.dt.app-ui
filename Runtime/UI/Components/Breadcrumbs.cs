using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A navigation component that helps users track their location within an application's hierarchy.
    /// </summary>
    /// <remarks>
    /// Breadcrumbs are a navigation pattern that shows users their current location in an application's hierarchy.
    /// They provide a trail of links that allows users to quickly move up to a parent level or previously viewed
    /// location.
    ///
    /// The component consists of three main parts:
    ///
    /// - BreadcrumbItem: Clickable links representing each level in the hierarchy
    /// - BreadcrumbSeparator: Visual separator (default '/') between items
    /// - The current page/location is indicated by a BreadcrumbItem with isCurrent=true
    ///
    /// Breadcrumbs are particularly useful in:
    ///
    /// - Applications with hierarchical navigation
    /// - Complex folder structures
    /// - Multi-step processes
    /// - Any interface where users need to understand and navigate their current location
    /// </remarks>
    /// <example>
    /// <para>Basic Breadcrumbs Implementation</para>
    ///
    /// <para>A basic breadcrumbs navigation showing three levels deep with the current location.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:Breadcrumbs>
    ///         <ui:BreadcrumbItem text="Home" url="/" />
    ///         <ui:BreadcrumbSeparator />
    ///         <ui:BreadcrumbItem text="Projects" url="/projects" />
    ///         <ui:BreadcrumbSeparator />
    ///         <ui:BreadcrumbItem text="Project Alpha" is-current="true" />
    ///     </ui:Breadcrumbs>
    /// </UXML>
    /// ]]></code>
    /// <para>Creating Breadcrumbs in C#</para>
    ///
    /// <para>Programmatically creating a breadcrumbs navigation with three levels.</para>
    /// <code lang="csharp"><![CDATA[
    /// var breadcrumbs = new Breadcrumbs();
    ///
    /// var homeItem = new BreadcrumbItem { text = "Home", url = "/" };
    /// var projectsItem = new BreadcrumbItem { text = "Projects", url = "/projects" };
    /// var currentItem = new BreadcrumbItem { text = "Project Alpha", isCurrent = true };
    ///
    /// breadcrumbs.Add(homeItem);
    /// breadcrumbs.Add(new BreadcrumbSeparator());
    /// breadcrumbs.Add(projectsItem);
    /// breadcrumbs.Add(new BreadcrumbSeparator());
    /// breadcrumbs.Add(currentItem);
    /// ]]></code>
    /// <para>Custom Separator</para>
    ///
    /// <para>Using a custom separator character instead of the default '/'.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:Breadcrumbs>
    ///         <ui:BreadcrumbItem text="Home" />
    ///         <ui:BreadcrumbSeparator text=">" />
    ///         <ui:BreadcrumbItem text="Settings" is-current="true" />
    ///     </ui:Breadcrumbs>
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("nav-components")]
    public partial class Breadcrumbs : BaseVisualElement
    {
        /// <summary>
        /// The Breadcrumbs' USS class name.
        /// </summary>
        public const string ussClassName = "appui-breadcrumbs";

        /// <summary>
        /// Constructor.
        /// </summary>
        public Breadcrumbs()
        {
            AddToClassList(ussClassName);
        }

    }

    /// <summary>
    /// BreadcrumbItem visual element.
    /// </summary>
    [UxmlElement]
    public partial class BreadcrumbItem : Link
    {
        internal static readonly BindingId isCurrentProperty = nameof(isCurrent);
        /// <summary>
        /// The BreadcrumbItem's USS class name.
        /// </summary>
        public new const string ussClassName = "appui-breadcrumb-item";

        /// <summary>
        /// The BreadcrumbItem's active USS class name.
        /// </summary>
        public const string currentUssClassName = ussClassName + "--current";

        /// <summary>
        /// Whether the BreadcrumbItem is the current item.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Breadcrumb Item")]
        public bool isCurrent
        {
            get => ClassListContains(currentUssClassName);
            set
            {
                var changed = isCurrent != value;
                EnableInClassList(currentUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in isCurrentProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BreadcrumbItem()
        {
            AddToClassList(ussClassName);
        }

    }

    /// <summary>
    /// BreadcrumbSeparator visual element.
    /// </summary>
    [UxmlElement]
    public partial class BreadcrumbSeparator : BaseTextElement
    {
        /// <summary>
        /// The BreadcrumbSeparator's USS class name.
        /// </summary>
        public new const string ussClassName = "appui-breadcrumb-separator";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BreadcrumbSeparator()
        {
            AddToClassList(ussClassName);

            text = "/";
        }

    }
}
