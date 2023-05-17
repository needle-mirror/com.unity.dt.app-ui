using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SplitView))]
    class SplitViewTests : VisualElementTests<SplitView>
    {
        protected override string mainUssClassName => SplitView.ussClassName;
    }
}
