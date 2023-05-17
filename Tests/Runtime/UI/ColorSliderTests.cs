using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorSlider))]
    class ColorSliderTests : VisualElementTests<ColorSlider>
    {
        protected override string mainUssClassName => ColorSlider.ussClassName;
    }
}
