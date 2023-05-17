using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dialog))]
    class DialogTests : VisualElementTests<Dialog>
    {
        protected override string mainUssClassName => BaseDialog.ussClassName;
    }
}
