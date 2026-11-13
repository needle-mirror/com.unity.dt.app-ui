using NUnit.Framework;
using Unity.AppUI.MVVM;

namespace Unity.AppUI.Tests.MVVM
{
    [TestFixture]
    [TestOf(typeof(App))]
    class AppCurrentTests
    {
        [Test]
        public void DisposingAnotherApp_DoesNotClearCurrent()
        {
            var current = new App();
            var other = new App();
            current.Initialize(new ServiceCollection().BuildServiceProvider());
            try
            {
                Assert.AreSame(current, App.current);
                other.Dispose();
                Assert.AreSame(current, App.current);
            }
            finally
            {
                current.Dispose();
            }
            Assert.IsNull(App.current);
        }
    }
}
