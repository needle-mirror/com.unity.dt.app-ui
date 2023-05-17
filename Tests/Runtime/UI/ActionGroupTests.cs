using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionGroup))]
    class ActionGroupTests : VisualElementTests<ActionGroup>
    {
        protected override string mainUssClassName => ActionGroup.ussClassName;
    }
}
