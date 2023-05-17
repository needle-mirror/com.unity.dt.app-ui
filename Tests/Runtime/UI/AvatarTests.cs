using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Avatar))]
    class AvatarTests : VisualElementTests<Avatar>
    {
        protected override string mainUssClassName => Avatar.ussClassName;
    }
}
