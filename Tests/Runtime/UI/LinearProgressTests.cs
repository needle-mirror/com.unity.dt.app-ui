using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(LinearProgress))]
    class LinearProgressTests : VisualElementTests<LinearProgress>
    {
        protected override string mainUssClassName => LinearProgress.ussClassName;
    }
}
