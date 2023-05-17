using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ContextProvider))]
    class ContextProviderTests : VisualElementTests<ContextProvider>
    {
        protected override string mainUssClassName => ContextProvider.ussClassName;
    }
}
