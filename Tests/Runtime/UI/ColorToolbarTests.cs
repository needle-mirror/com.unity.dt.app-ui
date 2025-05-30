using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorToolbar))]
    class ColorToolbarTests : VisualElementTests<ColorToolbar>
    {
        protected override string mainUssClassName => ColorToolbar.ussClassName;

        protected override bool uxmlConstructable => false;
    }
}
