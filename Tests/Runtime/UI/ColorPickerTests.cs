using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorPicker))]
    class ColorPickerTests : VisualElementTests<ColorPicker>
    {
        protected override string mainUssClassName => ColorPicker.ussClassName;
    }
}
