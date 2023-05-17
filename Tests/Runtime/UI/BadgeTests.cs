using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Badge))]
    class BadgeTests : VisualElementTests<Badge>
    {
        protected override string mainUssClassName => Badge.ussClassName;
    }
}
