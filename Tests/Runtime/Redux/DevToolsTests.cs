using System.Threading.Tasks;
using NUnit.Framework;
using Unity.AppUI.Redux;
using Unity.AppUI.Redux.DevTools;

namespace Unity.AppUI.Tests.Redux
{
    [TestFixture]
    [TestOf(typeof(DevToolsService))]
    class DevToolsTests
    {
        record DummyState {}

        record ThunkState
        {
            public string status { get; set; }
        }

        [Test]
        public void CanCreateStoreWithDevTools()
        {
            var defaultEnhancer =
                StoreFactory.DefaultEnhancer<DummyState>(
                    new DefaultEnhancerConfiguration()
                    {
                        devTools = { enabled = true}
                    });
            var store = StoreFactory.CreateStore((state, _) => state with {}, new DummyState(), defaultEnhancer);
            Assert.IsNotNull(store);
            Assert.DoesNotThrow(() => store.Dispatch(new ActionCreator("test").Invoke()));
            Assert.DoesNotThrow(() => store.Dispatch( new ActionCreator<int>("testWithPayload"), 0));
        }

        static IStore<DummyState> CreateInstrumentedStore(int maxAge)
        {
            var enhancer = Instrument<DummyState>.Enhancer(new DevToolsConfiguration { enabled = true, maxAge = maxAge });
            return StoreFactory.CreateStore((state, _) => state with {}, new DummyState(), enhancer);
        }

        [Test]
        public void MaxAge_CommitsExcessActions_AndForgetsThem()
        {
            var store = CreateInstrumentedStore(3);
            for (var i = 0; i < 5; i++)
                store.Dispatch(new ActionCreator("test").Invoke());

            var liftedState = store.AsInstrumentedStore().GetLiftedState();
            Assert.AreEqual(3, liftedState.stagedActionIds.Count);
            Assert.AreEqual(3, liftedState.actionsById.Count);
            CollectionAssert.AreEquivalent(liftedState.stagedActionIds, liftedState.actionsById.Keys);
            Assert.AreEqual(3, liftedState.computedStates.Count);
            store.Dispose();
        }

        [TestCase(0)]
        [TestCase(1)]
        public void MaxAge_WhenLowerThanTwo_DispatchDoesNotThrow(int maxAge)
        {
            var store = CreateInstrumentedStore(maxAge);
            for (var i = 0; i < 3; i++)
                Assert.DoesNotThrow(() => store.Dispatch(new ActionCreator("test").Invoke()));
            store.Dispose();
        }

        [Test]
        public void DefaultEnhancer_WithDevTools_RunsAsyncThunks()
        {
            var ran = false;
            var thunk = new AsyncThunkCreator<string,string>("thunk", _ =>
            {
                ran = true;
                return Task.FromResult("done");
            });
            var enhancer = StoreFactory.DefaultEnhancer<ThunkState>(new DefaultEnhancerConfiguration
            {
                devTools = { enabled = true }
            });
            var store = StoreFactory.CreateStore((state, action) => action.type switch
            {
                "thunk/pending" => state with { status = "pending" },
                "thunk/fulfilled" => state with { status = "fulfilled" },
                _ => state
            }, new ThunkState(), enhancer);

            store.Dispatch(thunk.Invoke("arg"));

            Assert.IsTrue(ran);
            Assert.AreEqual("fulfilled", store.GetState().status);
            store.Dispose();
        }
    }
}
