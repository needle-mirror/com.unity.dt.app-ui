using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionBar))]
    class ActionBarTests : VisualElementTests<ActionBar>
    {
        protected override string mainUssClassName => ActionBar.ussClassName;
    }
}
