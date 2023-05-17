using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toast))]
    class ToastTests : BottomNotificationTests<Toast>
    {
        protected override Toast CreatePopup()
        {
            return Toast.Build(GetReferenceElement(), "Hello", NotificationDuration.Indefinite);
        }
        
        protected override bool shouldContainView => false;
    }
}
