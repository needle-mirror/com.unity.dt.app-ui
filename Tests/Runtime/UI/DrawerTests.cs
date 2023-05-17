using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Drawer))]
    class DrawerTests : VisualElementTests<Drawer>
    {
        protected override string mainUssClassName => Drawer.ussClassName;
    }
}
