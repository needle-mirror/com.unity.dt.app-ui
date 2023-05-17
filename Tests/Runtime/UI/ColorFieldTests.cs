using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldTests : VisualElementTests<ColorField>
    {
        protected override string mainUssClassName => ColorField.ussClassName;
    }
}
