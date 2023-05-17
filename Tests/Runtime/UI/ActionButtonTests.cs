using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonTests : VisualElementTests<ActionButton>
    {
        protected override string mainUssClassName => ActionButton.ussClassName;
    }
}
