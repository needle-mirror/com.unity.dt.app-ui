using System;
using System.Collections.Generic;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// Represents a subscription to a store.
    /// </summary>
    public interface IDisposableSubscription : IDisposable
    {
        /// <summary>
        /// Whether the subscription is still valid.
        /// A Subscription is no longer valid when it has been unsubscribed
        /// or of the store it was subscribed to has been disposed.
        /// </summary>
        /// <returns> True if the subscription is still valid, false otherwise. </returns>
        bool IsValid();
    }

    /// <summary>
    /// Options for subscribing to a store.
    /// </summary>
    /// <typeparam name="TResult"> The type of the result of the subscription. </typeparam>
    public struct SubscribeOptions<TResult>
    {
        /// <summary>
        /// The comparer to use to compare the result of the subscription.
        /// If not provided, the default comparer for the type will be used.
        /// </summary>
        /// <seealso cref="EqualityComparer{T}.Default"/>
        public EqualityComparer<TResult> comparer;

        /// <summary>
        /// Whether to invoke the listener immediately when subscribing.
        /// </summary>
        public bool invokeOnSubscribe;
    }

    public partial class Store<TStoreState>
    {
        /// <summary>
        /// A Subscription to a store. This abstraction is used by the store to manage subscriptions.
        /// </summary>
        protected interface ISubscription : IDisposableSubscription
        {
            /// <summary>
            /// Unsubscribe from the store.
            /// </summary>
            /// <returns> True if the subscription was removed, false otherwise. </returns>
            bool Unsubscribe();

            /// <summary>
            /// Notify the listener of a new state.
            /// </summary>
            /// <param name="state"> The new state of the store. </param>
            void Notify(TStoreState state);
        }

        /// <summary>
        /// A subscription to a store for the entire state (without selection).
        /// </summary>
        protected class Subscription : ISubscription
        {
            WeakReference<Store<TStoreState>> m_Store;

            Listener<TStoreState> m_Listener;

            readonly SubscribeOptions<TStoreState> m_Options;

            /// <summary>
            /// Create a new subscription to a store.
            /// </summary>
            /// <param name="listener"> The listener to notify of state changes. </param>
            /// <param name="store"> The store to subscribe to. </param>
            /// <param name="options"> The options for the subscription. </param>
            public Subscription(Listener<TStoreState> listener, Store<TStoreState> store, SubscribeOptions<TStoreState> options)
            {
                m_Store = new WeakReference<Store<TStoreState>>(store);
                m_Listener = listener;
                m_Options = options;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Unsubscribe();
            }

            /// <inheritdoc/>
            public bool IsValid()
            {
                return m_Store.TryGetTarget(out _) && m_Listener != null;
            }

            /// <inheritdoc/>
            public bool Unsubscribe()
            {
                var removed = false;
                if (m_Store.TryGetTarget(out var store))
                {
                    lock (store.m_SubscriptionsLock)
                    {
                        removed = store.m_Subscriptions.Remove(this);
                    }
                }
                m_Store = null;
                m_Listener = null;
                return removed;
            }

            /// <inheritdoc/>
            public void Notify(TStoreState state)
            {
                if (m_Listener != null)
                {
                    // TODO: Should we compare the state?
                    m_Listener(state);
                }
                else
                {
                    Unsubscribe();
                }
            }
        }

        /// <summary>
        /// A subscription to a store for a selected state.
        /// </summary>
        /// <typeparam name="TSelected"> The type of the selected state. </typeparam>
        protected class SelectorSubscription<TSelected> : ISubscription
        {
            WeakReference<Store<TStoreState>> m_Store;

            Listener<TSelected> m_Listener;

            Selector<TStoreState,TSelected> m_Selector;

            TSelected m_LastSelected;

            readonly SubscribeOptions<TSelected> m_Options;

            /// <summary>
            /// Create a new subscription to a store for a selected state.
            /// </summary>
            /// <param name="selector"> The selector to use to select the state. </param>
            /// <param name="listener"> The listener to notify of state changes. </param>
            /// <param name="store"> The store to subscribe to. </param>
            /// <param name="options"> The options for the subscription. </param>
            public SelectorSubscription(
                Selector<TStoreState,TSelected> selector,
                Listener<TSelected> listener,
                Store<TStoreState> store,
                SubscribeOptions<TSelected> options)
            {
                m_Selector = selector;
                m_Store = new WeakReference<Store<TStoreState>>(store);
                m_Listener = listener;
                m_Options = options;
                m_LastSelected = m_Selector(store.m_State);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                Unsubscribe();
            }

            /// <inheritdoc/>
            public bool IsValid()
            {
                return m_Store.TryGetTarget(out _) && m_Listener != null && m_Selector != null;
            }

            /// <inheritdoc/>
            public bool Unsubscribe()
            {
                var removed = false;
                if (m_Store.TryGetTarget(out var store))
                {
                    lock (store.m_SubscriptionsLock)
                    {
                        removed = store.m_Subscriptions.Remove(this);
                    }
                }
                m_Store = null;
                m_Listener = null;
                m_Selector = null;
                return removed;
            }

            /// <inheritdoc/>
            public void Notify(TStoreState state)
            {
                if (m_Listener == null || m_Selector == null)
                {
                    Unsubscribe();
                    return;
                }

                var selected = m_Selector(state);
                var comparer = m_Options.comparer ?? EqualityComparer<TSelected>.Default;
                if (!comparer.Equals(selected, m_LastSelected))
                {
                    m_LastSelected = selected;
                    m_Listener(selected);
                }
            }
        }
    }
}
