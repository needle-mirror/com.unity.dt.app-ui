using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dropdown))]
    class DropdownTests : VisualElementTests<Dropdown>
    {
        protected override string mainUssClassName => Dropdown.ussClassName;
    }
}
