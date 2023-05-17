using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CircularProgress))]
    class CircularProgressTests : VisualElementTests<CircularProgress>
    {
        protected override string mainUssClassName => CircularProgress.ussClassName;
    }
}
