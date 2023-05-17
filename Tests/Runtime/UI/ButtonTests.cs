using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Button))]
    class ButtonTests : VisualElementTests<Button>
    {
        protected override string mainUssClassName => Button.ussClassName;
    }
}
