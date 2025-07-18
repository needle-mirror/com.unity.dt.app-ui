using System;
using System.Collections.Generic;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// A function that takes the current state and an action, and returns a new state.
    /// </summary>
    /// <typeparam name="TState"> The type of the state. </typeparam>
    /// <param name="state"> The current state. </param>
    /// <param name="action"> The action. </param>
    /// <returns> The new state. </returns>
    public delegate TState CaseReducer<TState>(TState state, Action action);

    /// <summary>
    /// A function that takes the current state and an action, and returns a new state.
    /// </summary>
    /// <typeparam name="T"> The type of the action payload. </typeparam>
    /// <typeparam name="TState"> The type of the state. </typeparam>
    /// <param name="state"> The current state. </param>
    /// <param name="action"> The action. </param>
    /// <returns> The new state. </returns>
    public delegate TState CaseReducer<T, TState>(TState state, Action<T> action);

    /// <summary>
    /// A function that takes the current state and an action, and returns a new state.
    /// </summary>
    /// <param name="state"> The current state. </param>
    /// <param name="action"> The action. </param>
    /// <returns> The new state. </returns>
    public delegate object Reducer(object state, Action action);

    /// <summary>
    /// A function obtained from <see cref="Store.Subscribe{TState}"/> that can be called to unsubscribe the listener.
    /// </summary>
    /// <returns> True if the listener was removed, false otherwise. </returns>
    public delegate bool Unsubscriber();

    /// <summary>
    /// <para>
    /// A store holds the whole state tree of your application. The only way to change the state inside it is to dispatch an action on it.
    /// Your application should only have a single store in a Redux app. As your app grows, instead of adding stores,
    /// you split the root reducer into smaller reducers independently operating on the different parts of the state tree.
    /// </para>
    /// <para>
    /// The store has the following responsibilities:<br/>
    ///  - Holds application state <br/>
    ///  - Allows access to state via <see cref="GetState{TState}"/><br/>
    ///  - Allows state to be updated via <see cref="Dispatch(Action)"/><br/>
    ///  - Registers listeners via <see cref="Subscribe{TState}"/><br/>
    ///  - Handles unregistering of listeners via the function returned by <see cref="Subscribe{TState}"/><br/>
    /// </para>
    /// <para>
    /// Here are some important principles you should understand about Reducers:<br/>
    ///  - Reducers are the only way to update the state.<br/>
    ///  - Reducers are pure functions that take the previous state and an action, and return the next state.<br/>
    ///  - Reducers must be pure functions. They should not mutate the state, perform side effects like API calls or routing transitions, or call non-pure functions.<br/>
    ///  - Reducers must not do asynchronous logic.<br/>
    ///  - Reducers must not call other <see cref="Reducer"/>.<br/>
    ///  - Reducers must not call <see cref="Subscribe{TState}"/>.<br/>
    ///  - Reducers must not call <see cref="GetState{TState}"/><br/>
    ///  - Reducers must not call <see cref="Dispatch(Action)"/><br/>
    /// </para>
    /// </summary>
    [Obsolete("Use Store<TStoreState> instead. This class will be removed in a future major version.")]
    public class Store
    {
        readonly Dictionary<string, object> m_State;

        readonly Dictionary<string, Reducer> m_Reducers = new Dictionary<string, Reducer>();

        readonly Dictionary<string, List<System.Action<object>>> m_ListenerWrappers = new Dictionary<string, List<System.Action<object>>>();

        /// <summary>
        /// Creates a Redux store that holds the complete state tree of your app.
        /// </summary>
        public Store()
        {
            m_State = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns the current state tree of your application for a specific slice.
        /// It is equal to the last value returned by the store's reducer.
        /// </summary>
        /// <param name="name"> The name of the state slice. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <returns> The current state tree of your application. </returns>
        /// <exception cref="ArgumentException"> Thrown if the state slice does not exist. </exception>
        public TState GetState<TState>(string name)
        {
            if (!m_State.ContainsKey(name))
            {
                throw new ArgumentException($"State slice '{name}' does not exist.");
            }

            return (TState)m_State[name];
        }

        /// <summary>
        /// Returns the current state tree of your application. It is equal to the last value returned by the store's reducer.
        /// </summary>
        /// <returns> The current state tree of your application. </returns>
        public Dictionary<string, object> GetState()
        {
            return m_State;
        }

        /// <summary>
        /// Dispatches an action. This is the only way to trigger a state change.
        /// </summary>
        /// <param name="action"> An object describing the change that makes up the action. </param>
        /// <exception cref="ArgumentException"> Thrown if the reducer for the action type does not exist. </exception>
        public void Dispatch(Action action)
        {
            if (action.type.Contains('/'))
            {
                DispatchSlice(action);
                return;
            }

            foreach (var slice in m_Reducers.Keys)
            {
                m_State[slice] = m_Reducers[slice](m_State[slice], action);

                if (m_ListenerWrappers.ContainsKey(slice))
                {
                    foreach (var listener in m_ListenerWrappers[slice])
                    {
                        listener.Invoke(m_State[slice]);
                    }
                }
            }
        }

        /// <summary>
        /// Dispatches an action. This is the only way to trigger a state change.
        /// </summary>
        /// <param name="actionType"> The type of the action. </param>
        public void Dispatch(string actionType)
        {
            Dispatch(CreateAction(actionType).Invoke());
        }

        /// <summary>
        /// Dispatches an action. This is the only way to trigger a state change.
        /// </summary>
        /// <param name="actionType"> The type of the action. </param>
        /// <param name="payload"> The payload of the action. </param>
        /// <typeparam name="T"> The type of the payload. </typeparam>
        public void Dispatch<T>(string actionType, T payload)
        {
            Dispatch(CreateAction<T>(actionType).Invoke(payload));
        }

        void DispatchSlice(Action action)
        {
            var slice = action.type[..action.type.IndexOf('/')];

            if (!m_Reducers.ContainsKey(slice))
            {
                throw new ArgumentException($"Reducer for action type '{action.type}' does not exist.");
            }

            m_State[slice] = m_Reducers[slice](m_State[slice], action);

            if (m_ListenerWrappers.ContainsKey(slice))
            {
                foreach (var listener in m_ListenerWrappers[slice])
                {
                    listener.Invoke(m_State[slice]);
                }
            }
        }

        /// <summary>
        /// Adds a change listener.
        /// It will be called any time an action is dispatched, and some part of the state tree may potentially have changed.
        /// </summary>
        /// <remarks>
        /// This method doesn't check for duplicate listeners,
        /// so calling it multiple times with the same listener will result in the listener being called multiple times.
        /// </remarks>
        /// <param name="name"> The name of the state slice. </param>
        /// <param name="listener"> A callback to be invoked on every dispatch. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <returns> A function to remove this change listener. </returns>
        public Unsubscriber Subscribe<TState>(string name, System.Action<TState> listener)
        {
            if (!m_ListenerWrappers.ContainsKey(name))
                m_ListenerWrappers[name] = new List<System.Action<object>>();

            var wrapper = new System.Action<object>(state => listener.Invoke((TState)state));
            m_ListenerWrappers[name].Add(wrapper);

            return () => Unsubscribe(name, wrapper);
        }

        /// <summary>
        /// Removes a change listener.
        /// </summary>
        /// <remarks>
        /// This method won't throw if the listener is not found.
        /// </remarks>
        /// <param name="name"> The name of the state slice. </param>
        /// <param name="listenerWrapper"> A callback to be invoked on every dispatch. </param>
        /// <returns> True if the listener was removed, false otherwise. </returns>
        internal bool Unsubscribe(string name, System.Action<object> listenerWrapper)
        {
            return m_ListenerWrappers.ContainsKey(name) && m_ListenerWrappers[name].Remove(listenerWrapper);
        }

        /// <summary>
        /// Create a new state slice. A state slice is a part of the state tree.
        /// You can provide reducers that will "own" the state slice at the same time.
        /// </summary>
        /// <remarks>
        /// You can also provide extra reducers that will be called if the action type does not match any of the main reducers.
        /// </remarks>
        /// <param name="name"> The name of the state slice. </param>
        /// <param name="initialState"> The initial state of the state slice. </param>
        /// <param name="reducers"> The reducers that will "own" the state slice. </param>
        /// <param name="extraReducers"> The reducers that will be called if the action type does not match any of the main reducers. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <returns> A slice object that can be used to access the state slice. </returns>
        /// <exception cref="ArgumentException"> Thrown if the state slice already exists. </exception>
        public Slice<TState> CreateSlice<TState>(
            string name,
            TState initialState,
            System.Action<SliceReducerSwitchBuilder<TState>> reducers,
            System.Action<ReducerSwitchBuilder<TState>> extraReducers = null)
        {
            if (m_State.ContainsKey(name))
            {
                throw new ArgumentException($"State slice '{name}' already exists.");
            }

            // add the reducers
            var builder = new SliceReducerSwitchBuilder<TState>(name);
            reducers(builder);
            var actionCreators = builder.BuildActionCreators();
            var reducer = CreateReducer(initialState, builder.BuildReducers(actionCreators.Values));

            // add the extra reducers
            if (extraReducers != null)
            {
                var extraReducer = CreateReducer(initialState, extraReducers);
                m_Reducers[name] = CombineReducers(reducer, extraReducer);
            }
            else
            {
                m_Reducers[name] = reducer;
            }

            // add the initial state
            m_State[name] = initialState;

            // return the slice
            return new Slice<TState>(name, actionCreators, initialState);
        }

        /// <summary>
        /// Create a new Action. See <see cref="Action"/> for more information.
        /// </summary>
        /// <param name="type"> The type of the action. </param>
        /// <returns> A new Action. </returns>
        public static ActionCreator CreateAction(string type)
        {
            return new ActionCreator(type);
        }

        /// <summary>
        /// Create a new Action. See <see cref="Action{T}"/> for more information.
        /// </summary>
        /// <param name="type"> The type of the action. </param>
        /// <typeparam name="TPayload"> The type of the payload. </typeparam>
        /// <returns> A new Action. </returns>
        public static ActionCreator<TPayload> CreateAction<TPayload>(string type)
        {
            return new ActionCreator<TPayload>(type);
        }

        /// <summary>
        /// Create a new Action. See <see cref="Action{T}"/> for more information.
        /// </summary>
        /// <param name="type"> The type of the action. </param>
        /// <param name="actionType"> The type of the action to instantiate. </param>
        /// <returns> A new Action. </returns>
        public static ActionCreator CreateAction(string type, Type actionType)
        {
            return (ActionCreator)Activator.CreateInstance(actionType, type);
        }

        /// <summary>
        /// Create reducers for a state slice. See <see cref="SliceReducerSwitchBuilder{TState}"/> for more information.
        /// </summary>
        /// <param name="initialState"> The initial state of the state slice. </param>
        /// <param name="builderCallback"> The builder that will be used to create the reducers. </param>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <returns> A reducer record that can be used to create a state slice. </returns>
        public static Reducer CreateReducer<TState>(
            TState initialState,
            System.Action<ReducerSwitchBuilder<TState>> builderCallback)
        {
            var builder = new ReducerSwitchBuilder<TState>();
            builderCallback(builder);
            return builder.BuildReducer(initialState);
        }

        /// <summary>
        /// Create a reducer that combines multiple reducers into one.
        /// </summary>
        /// <param name="reducers"> The reducers to combine. </param>
        /// <returns> A reducer that combines the given reducers. </returns>
        public static Reducer CombineReducers(params Reducer[] reducers)
        {
            return (state, action) =>
            {
                var newState = state;

                foreach (var reducer in reducers)
                {
                    newState = reducer(newState, action);
                }

                return newState;
            };
        }

        /// <summary>
        /// Force notify all listeners of a state slice.
        /// </summary>
        /// <param name="name"> The name of the state slice. </param>
        public void NotifyStateChanged(string name)
        {
            if (m_State.TryGetValue(name, out var state) && m_ListenerWrappers.TryGetValue(name, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.Invoke(state);
                }
            }
        }
    }
}
