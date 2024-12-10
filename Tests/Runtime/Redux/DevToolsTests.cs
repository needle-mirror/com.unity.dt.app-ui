using NUnit.Framework;
using Unity.AppUI.Redux;
using Unity.AppUI.Redux.DevTools;

namespace Unity.AppUI.Tests.Redux
{
    [TestFixture]
    [TestOf(typeof(DevToolsService))]
    class DevToolsTests
    {
        [Test]
        public void CanCreateStoreWithDevTools()
        {
            var defaultEnhancer =
                DefaultEnhancer.GetDefaultEnhancer<Store, PartitionedState>(
                    new DefaultEnhancer.Configuration
                    {
                        devTools = { enabled = true}
                    });
            var store = Store.CreateStore((state, action) => state, new PartitionedState(), defaultEnhancer);
            Assert.IsNotNull(store);
            Assert.DoesNotThrow(() => store.Dispatch(Store.CreateAction("test")));
            Assert.DoesNotThrow(() => store.Dispatch(Store.CreateAction<int>("testWithPayload"), 0));
        }
    }
}
