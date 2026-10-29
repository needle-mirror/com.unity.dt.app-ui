using System;
using NUnit.Framework;
using Unity.AppUI.Redux;

namespace Unity.AppUI.Tests.Redux
{
    [TestFixture]
    [TestOf(typeof(Store<>))]
    class StoreTests
    {
        [Test]
        public void CanCreateStore()
        {
            var store = new Store<PartitionedState>((state, _) => state, new PartitionedState());
            Assert.IsNotNull(store);
            Assert.IsNotNull(store.GetState());
            Assert.Throws<ArgumentNullException>(() => store.Dispatch((IAction)null));
        }

        [Test]
        public void CanDispatch()
        {
            var store = new Store<PartitionedState>((state, _) => state, new PartitionedState());
            Assert.Throws<ArgumentNullException>(() => store.Dispatch((IAction)null));
            Assert.DoesNotThrow(() => store.Dispatch(new ActionCreator("test").Invoke()));
        }

        [Test]
        public void CanSubscribe()
        {
            var store = new Store<PartitionedState>((state, _) => state, new PartitionedState());
            IDisposableSubscription subscription = null;
            Assert.Throws<ArgumentNullException>(() => store.Subscribe(null));
            Assert.DoesNotThrow(() => subscription = store.Subscribe(_ => { }, new SubscribeOptions<PartitionedState> { fireImmediately = true }));
            Assert.IsNotNull(subscription);
            Assert.IsTrue(subscription.IsValid());
            Assert.DoesNotThrow(() => subscription.Dispose());
        }

        [Test]
        public void CanSubscribeWithSelector()
        {
            var store = new Store<PartitionedState>((state, _) => state, new PartitionedState());
            IDisposableSubscription subscription = null;
            Assert.Throws<ArgumentNullException>(() => store.Subscribe<PartitionedState>(null,
                _ => { }));
            Assert.Throws<ArgumentNullException>(() => store.Subscribe(state => state, null));
            Assert.DoesNotThrow(() => subscription = store.Subscribe(state => state, _ => { }, new SubscribeOptions<PartitionedState> { fireImmediately = true }));
            Assert.IsNotNull(subscription);
            Assert.IsTrue(subscription.IsValid());
            Assert.DoesNotThrow(() => subscription.Dispose());
        }

        [Test]
        public void Dispatch_AllowsListenersToUnsubscribeAndSubscribeWhileNotified()
        {
            var store = new Store<int>((state, a) => a.type == "inc" ? state + 1 : state, 0);
            IDisposableSubscription first = null;
            var received = -1;
            var extra = 0;
            first = store.Subscribe(s => s, _ =>
            {
                first.Dispose();
                store.Subscribe(s => s, __ => extra++);
            });
            store.Subscribe(s => s, v => received = v);
            Assert.DoesNotThrow(() => store.Dispatch(new ActionCreator("inc").Invoke()));
            Assert.AreEqual(1, received);
        }

        [Test]
        public void IsValid_ReturnsFalse_AfterDispose()
        {
            var store = new Store<int>((state, a) => a.type == "inc" ? state + 1 : state, 0);
            var subscription = store.Subscribe(s => s, _ => { });
            Assert.IsTrue(subscription.IsValid());
            subscription.Dispose();
            Assert.IsFalse(subscription.IsValid());

            var other = store.Subscribe(s => s, _ => { });
            store.Dispose();
            Assert.IsFalse(other.IsValid());
        }

        [Test]
        public void Notify_DoesNotThrow_WhenUnsubscribedWhileNotifying()
        {
            // Unsubscribing from the selector clears the subscription between the null check and the
            // listener call, the same window another thread's Unsubscribe can hit.
            var store = new Store<int>((state, a) => a.type == "inc" ? state + 1 : state, 0);
            IDisposableSubscription subscription = null;
            var received = -1;
            subscription = store.Subscribe(s =>
            {
                subscription?.Dispose();
                return s;
            }, v => received = v);

            Assert.DoesNotThrow(() => store.Dispatch(new ActionCreator("inc").Invoke()));
            Assert.AreEqual(1, received);
            Assert.IsFalse(subscription.IsValid());
        }
    }
}
