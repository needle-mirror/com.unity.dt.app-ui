using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(GridView))]
    class GridViewTests : VisualElementTests<GridView>
    {
        protected override string mainUssClassName => GridView.ussClassName;

        protected override bool uxmlConstructable => true;
    }
}
