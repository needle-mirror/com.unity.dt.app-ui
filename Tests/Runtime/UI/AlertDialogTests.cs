using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AlertDialog))]
    class AlertDialogTests : VisualElementTests<AlertDialog>
    {
        protected override string mainUssClassName => BaseDialog.ussClassName;
    }
}
