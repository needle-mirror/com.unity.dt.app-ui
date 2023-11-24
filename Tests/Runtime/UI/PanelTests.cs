using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Panel))]
    class PanelTests : VisualElementTests<Panel>
    {
        protected override string mainUssClassName => ContextProvider.ussClassName;
    }
}
