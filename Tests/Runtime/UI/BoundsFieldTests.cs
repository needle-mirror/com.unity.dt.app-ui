using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsField))]
    class BoundsFieldTests : VisualElementTests<BoundsField>
    {
        protected override string mainUssClassName => BoundsField.ussClassName;
    }
}
