using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Checkbox))]
    class CheckboxTests : VisualElementTests<Checkbox>
    {
        protected override string mainUssClassName => Checkbox.ussClassName;
    }
}
