using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AvatarGroup))]
    class AvatarGroupTests : VisualElementTests<AvatarGroup>
    {
        protected override string mainUssClassName => AvatarGroup.ussClassName;
    }
}
