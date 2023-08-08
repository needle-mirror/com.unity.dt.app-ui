using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Canvas))]
    class CanvasTests : VisualElementTests<Canvas>
    {
        protected override string mainUssClassName => Canvas.ussClassName;

        protected override bool uxmlConstructable => true;
    }
}
