using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsIntField))]
    class BoundsIntFieldTests : VisualElementTests<BoundsIntField>
    {
        protected override string mainUssClassName => BoundsIntField.ussClassName;
    }
}
