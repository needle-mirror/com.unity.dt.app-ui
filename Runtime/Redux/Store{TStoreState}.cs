using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// <para>
    /// A store holds the whole state tree of your application. The only way to change the state inside it is to dispatch an action on it.
    /// Your application should only have a single store in a Redux app. As your app grows, instead of adding stores,
    /// you split the root reducer into smaller reducers independently operating on the different parts of the state tree.
    /// </para>
    /// <para>
    /// The store has the following responsibilities:<br/>
    ///  - Holds application state <br/>
    ///  - Allows access to state via <see cref="GetState"/><br/>
    ///  - Allows state to be updated via <see cref="Dispatch(IAction)"/><br/>
    ///  - Registers listeners via <see cref="Subscribe{TState}"/><br/>
    ///  - Handles unregistering of listeners via the function returned by <see cref="Subscribe{TState}"/><br/>
    /// </para>
    /// <para>
    /// Here are some important principles you should understand about Reducers:<br/>
    ///  - Reducers are the only way to update the state.<br/>
    ///  - Reducers are pure functions that take the previous state and an action, and return the next state.<br/>
    ///  - Reducers must be pure functions. They should not mutate the state, perform side effects like API calls or routing transitions, or call non-pure functions.<br/>
    ///  - Reducers must not do asynchronous logic.<br/>
    ///  - Reducers must not call other <see cref="Reducer{TState}"/>.<br/>
    ///  - Reducers must not call <see cref="Subscribe{TState}"/>.<br/>
    ///  - Reducers must not call <see cref="GetState"/><br/>
    ///  - Reducers must not call <see cref="Dispatch(IAction)"/><br/>
    /// </para>
    /// </summary>
    /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
    [Preserve]
    public partial class Store<TStoreState> : INotifiable<TStoreState>, IStateProvider<TStoreState>, IDispatchable
    {
        /// <summary>
        /// The state of the store.
        /// </summary>
        protected TStoreState m_State;

        /// <summary>
        /// The subscriptions of the store.
        /// </summary>
        protected readonly List<ISubscription> m_Subscriptions = new();

        /// <summary>
        /// The lock for the subscriptions.
        /// </summary>
        protected readonly object m_SubscriptionsLock = new();

        /// <summary>
        /// The root reducer of the store. You can override this to provide custom state handling logic inside enhancers.
        /// </summary>
        Reducer<TStoreState> m_Reducer;

        /// <summary>
        /// The dispatcher of the store. You can override this to provide custom dispatching logic inside enhancers.
        /// </summary>
        /// <seealso cref="StoreEnhancer{TStore,TStoreState}"/>
        Dispatcher m_Dispatcher;

        /// <summary>
        /// Creates a Redux store that holds the complete state tree of your app.
        /// </summary>
        /// <param name="rootReducer"> The root reducer of the store. </param>
        /// <param name="initialState"> The initial state of the store. </param>
        /// <param name="dispatcher"> The dispatcher of the store. </param>
        /// <remarks>
        /// You should not use directly this constructor to create a store.
        /// Instead, use <c>StoreFactory.CreateStore{TStoreState}</c> to create a store with optional enhancers.
        /// This constructor should be called only by enhancers to return an enhanced store.
        /// </remarks>
        [Preserve]
        internal Store(Reducer<TStoreState> rootReducer, TStoreState initialState, Dispatcher dispatcher)
        {
            m_State = initialState;
            m_Reducer = rootReducer;
            m_Dispatcher = dispatcher ?? DefaultDispatch;
        }

        /// <summary>
        /// Returns the current state tree of your application. It is equal to the last value returned by the store's reducer.
        /// </summary>
        /// <returns> The current state tree of your application. </returns>
        public TStoreState GetState()
        {
            return m_State;
        }

        /// <summary>
        /// Dispatches an action. This is the only way to trigger a state change.
        /// </summary>
        /// <param name="action"> An object describing the change that makes up the action. </param>
        /// <exception cref="ArgumentException"> Thrown if the reducer for the action type does not exist. </exception>
        /// <exception cref="ArgumentNullException"> Thrown if the action is null. </exception>
        public void Dispatch(IAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            m_Dispatcher(action);
            NotifySubscribers();
        }

        /// <summary>
        /// Adds a change listener.
        /// It will be called any time an action is dispatched, and some part of the state tree may potentially have changed.
        /// </summary>
        /// <remarks>
        /// This method doesn't check for duplicate listeners,
        /// so calling it multiple times with the same listener will result in the listener being called multiple times.
        /// </remarks>
        /// <param name="listener"> A callback to be invoked on every dispatch. </param>
        /// <param name="options"> The options for the subscription. </param>
        /// <returns> A Subscription object that can be disposed. </returns>
        /// <exception cref="ArgumentNullException"> Thrown if the listener is null. </exception>
        public IDisposableSubscription Subscribe(
            Listener<TStoreState> listener,
            SubscribeOptions<TStoreState> options = default)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var subscription = new Subscription(listener, this, options);
            lock (m_SubscriptionsLock)
            {
                m_Subscriptions.Add(subscription);
            }

            if (options.invokeOnSubscribe)
                listener(m_State);

            return subscription;
        }

        /// <summary>
        /// Subscribe to a state change and listen to a specific part of the state.
        /// </summary>
        /// <param name="selector"> The selector to get the selected part of the state. </param>
        /// <param name="listener"> The listener to be invoked on every dispatch. </param>
        /// <param name="options"> The options for the subscription. </param>
        /// <typeparam name="TSelected"> The type of the selected part of the state. </typeparam>
        /// <returns> A Subscription object that can be disposed. </returns>
        /// <exception cref="ArgumentNullException"> Thrown if the selector or listener is null. </exception>
        public IDisposableSubscription Subscribe<TSelected>(
            Selector<TStoreState,TSelected> selector,
            Listener<TSelected> listener,
            SubscribeOptions<TSelected> options = default)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var subscription = new SelectorSubscription<TSelected>(selector, listener, this, options);
            lock (m_SubscriptionsLock)
            {
                m_Subscriptions.Add(subscription);
            }

            if (options.invokeOnSubscribe)
                listener(selector(m_State));

            return subscription;
        }

        internal void DefaultDispatch(IAction action)
        {
            m_State = m_Reducer(m_State, action);
        }

        void NotifySubscribers()
        {
            lock (m_SubscriptionsLock)
            {
                foreach (var subscription in m_Subscriptions)
                {
                    subscription.Notify(m_State);
                }
            }
        }
    }
}
