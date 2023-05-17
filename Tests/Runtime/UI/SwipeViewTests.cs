using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SwipeView))]
    class SwipeViewTests : VisualElementTests<SwipeView>
    {
        protected override string mainUssClassName => SwipeView.ussClassName;
    }
}
