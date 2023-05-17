using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Chip))]
    class ChipTests : VisualElementTests<Chip>
    {
        protected override string mainUssClassName => Chip.ussClassName;
    }
}
