using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Breadcrumbs))]
    class BreadcrumbsTests : VisualElementTests<Breadcrumbs>
    {
        protected override string mainUssClassName => Breadcrumbs.ussClassName;
    }
    
    [TestFixture]
    [TestOf(typeof(BreadcrumbItem))]
    class BreadcrumbItemTests : VisualElementTests<BreadcrumbItem>
    {
        protected override string mainUssClassName => BreadcrumbItem.ussClassName;
    }
}
