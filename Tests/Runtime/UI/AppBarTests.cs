using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AppBar))]
    class AppBarTests : VisualElementTests<AppBar>
    {
        protected override string mainUssClassName => AppBar.ussClassName;
    }
}
