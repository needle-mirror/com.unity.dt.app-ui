using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorSwatch))]
    class ColorSwatchTests : VisualElementTests<ColorSwatch>
    {
        protected override string mainUssClassName => ColorSwatch.ussClassName;
    }
}
