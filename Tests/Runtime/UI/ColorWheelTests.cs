using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorWheel))]
    class ColorWheelTests : VisualElementTests<ColorWheel>
    {
        protected override string mainUssClassName => ColorWheel.ussClassName;
    }
}
