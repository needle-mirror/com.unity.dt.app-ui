using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// A store that holds the complete state tree of your app using slices.
    /// </summary>
    [Preserve]
    public class Store : Store<PartitionedState>
    {
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
        public Store(Reducer<PartitionedState> rootReducer, PartitionedState initialState, Dispatcher dispatcher)
            : base(rootReducer, initialState, dispatcher) { }

        /// <summary>
        /// Create a new Store instance.
        /// This method should be used by enhancers to create a new store instance.
        /// </summary>
        /// <param name="reducer"> The root reducer of the store. </param>
        /// <param name="initialState"> The initial state of the store. </param>
        /// <param name="dispatcher"> The dispatcher of the store. </param>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> A new store. </returns>
        /// <seealso cref="CreateStore{TStore,TStoreState}(Unity.AppUI.Redux.Reducer{TStoreState},TStoreState,Unity.AppUI.Redux.StoreEnhancer{Store{TStoreState}.CreationContext,TStoreState})"/>
        internal static TStore CreateStoreInstance<TStore,TStoreState>(
            Reducer<TStoreState> reducer,
            TStoreState initialState,
            Dispatcher dispatcher)
            where TStore : Store<TStoreState>
        {
            return (TStore)Activator.CreateInstance(typeof(TStore), reducer, initialState, dispatcher);
        }

        /// <summary>
        /// Creates a Redux store that holds the complete state tree of your app.
        /// </summary>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <param name="rootReducer"> The root reducer of the store. </param>
        /// <param name="initialState"> The initial state of the store. </param>
        /// <param name="enhancer"> The enhancer for the store. </param>
        /// <returns> A new store. </returns>
        public static TStore CreateStore<TStore,TStoreState>(
            Reducer<TStoreState> rootReducer,
            TStoreState initialState = default,
            StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState> enhancer = null)
            where TStore : Store<TStoreState>
            where TStoreState : new()
        {
            rootReducer = rootReducer ?? throw new ArgumentNullException(nameof(rootReducer));
            initialState ??= new TStoreState();
            enhancer ??= DefaultEnhancer.GetDefaultEnhancer<TStore,TStoreState>();

            var createStore = enhancer(CreateBaseStore);
            var storeCreator = createStore(rootReducer, initialState);
            return storeCreator.store as TStore;

            Store<TStoreState>.CreationContext CreateBaseStore(Reducer<TStoreState> r, TStoreState s)
            {
                var store = CreateStoreInstance<TStore, TStoreState>(r, s, null);
                var context = new Store<TStoreState>.CreationContext(store);
                return context;
            }
        }

        /// <summary>
        /// Creates a Redux store that holds the complete state tree of your app.
        /// </summary>
        /// <param name="rootReducer"> The root reducer of the store. </param>
        /// <param name="initialState"> The initial state of the store. </param>
        /// <param name="enhancer"> The enhancer for the store. </param>
        /// <returns> A new store. </returns>
        /// <remarks>
        /// This is a specialization of
        /// <see cref="CreateStore{TStore,TStoreState}(Reducer{TStoreState},TStoreState,StoreEnhancer{Store{TStoreState}.CreationContext,TStoreState})"/>
        /// for a <see cref="Store"/> that uses <see cref="PartitionedState"/>.
        /// </remarks>
        public static Store CreateStore(
            Reducer<PartitionedState> rootReducer,
            PartitionedState initialState = null,
            StoreEnhancer<CreationContext,PartitionedState> enhancer = null)
        {
            return CreateStore<Store,PartitionedState>(rootReducer, initialState, enhancer);
        }

        /// <summary>
        /// Creates a Redux store composed of multiple slices.
        /// </summary>
        /// <param name="enhancer"> The enhancer for the store. </param>
        /// <param name="slices"> The slices that compose the store. </param>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> The new store. </returns>
        /// <exception cref="ArgumentNullException"> Thrown if the slices are null. </exception>
        public static TStore CreateStore<TStore,TStoreState>(
            IReadOnlyCollection<ISlice<TStoreState>> slices,
            StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState> enhancer = null)
            where TStore : Store<TStoreState>
            where TStoreState : IPartionableState<TStoreState>, new()
        {
            if (slices == null)
                throw new ArgumentNullException(nameof(slices));

            var rootReducer = CombineReducers(slices);
            var initialState = new TStoreState();
            foreach (var slice in slices)
            {
                initialState = slice.InitializeState(initialState);
            }

            return CreateStore<TStore,TStoreState>(rootReducer, initialState, enhancer);
        }

        /// <summary>
        /// Creates a Redux store composed of multiple slices.
        /// </summary>
        /// <param name="enhancer"> The enhancer for the store. </param>
        /// <param name="slices"> The slices that compose the store. </param>
        /// <returns> The new store. </returns>
        /// <remarks>
        /// This is a specialization of
        /// <see cref="CreateStore{TStore,TStoreState}(IReadOnlyCollection{ISlice{TStoreState}},StoreEnhancer{Store{TStoreState}.CreationContext,TStoreState})"/>
        /// for a <see cref="Store"/> that uses <see cref="PartitionedState"/>.
        /// </remarks>
        public static Store CreateStore(
            IReadOnlyCollection<ISlice<PartitionedState>> slices,
            StoreEnhancer<CreationContext,PartitionedState> enhancer = null)
        {
            return CreateStore<Store,PartitionedState>(slices, enhancer);
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
        public static IActionCreator CreateAction(string type, Type actionType)
        {
            return (IActionCreator)Activator.CreateInstance(actionType, type);
        }

        /// <summary>
        /// Create a new state slice. A state slice is a part of the state tree.
        /// You can provide reducers that will "own" the state slice at the same time.
        /// </summary>
        /// <remarks>
        /// You can also provide extra reducers that will be called if the action type does not match any of
        /// the main reducers.
        /// </remarks>
        /// <param name="name"> The name of the state slice. </param>
        /// <param name="initialState"> The initial state of the state slice. </param>
        /// <param name="reducers"> The reducers that will "own" the state slice. </param>
        /// <param name="extraReducers"> The reducers that will be called if the action type does not match any of the
        /// main reducers. </param>
        /// <typeparam name="TState"> The type of the slice state. </typeparam>
        /// <returns> A slice object that can be used to access the state slice. </returns>
        /// <exception cref="ArgumentNullException"> Thrown if the name is null or empty. </exception>
        public static Slice<TState,PartitionedState> CreateSlice<TState>(
            string name,
            TState initialState,
            System.Action<SliceReducerSwitchBuilder<TState>> reducers,
            System.Action<ReducerSwitchBuilder<TState>> extraReducers = null)
        {
            return new Slice<TState,PartitionedState>(name, initialState, reducers, extraReducers);
        }

        /// <summary>
        /// Create a reducer that combines multiple reducers into one.
        /// </summary>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <param name="reducers"> The reducers to combine. </param>
        /// <returns> A reducer that combines the given reducers. </returns>
        public static Reducer<TState> CombineReducers<TState>(IEnumerable<Reducer<TState>> reducers)
        {
            return CombinedReducers;

            TState CombinedReducers(TState state, IAction action)
            {
                var newState = state;

                foreach (var reducer in reducers)
                {
                    newState = reducer(newState, action);
                }

                return newState;
            }
        }

        /// <summary>
        /// Create a reducer that combines multiple reducers into one.
        /// </summary>
        /// <typeparam name="TState"> The type of the state. </typeparam>
        /// <param name="reducers"> The reducers to combine. </param>
        /// <returns> A reducer that combines the given reducers. </returns>
        public static Reducer<TState> CombineReducers<TState>(params Reducer<TState>[] reducers)
        {
            return CombinedReducers;

            TState CombinedReducers(TState state, IAction action)
            {
                var newState = state;

                foreach (var reducer in reducers)
                {
                    newState = reducer(newState, action);
                }

                return newState;
            }
        }

        /// <summary>
        /// Create a reducer that combines multiple slices into one.
        /// </summary>
        /// <param name="slices"> The slices to combine. </param>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> A reducer that combines the given slices. </returns>
        public static Reducer<TStoreState> CombineReducers<TStoreState>(IReadOnlyCollection<ISlice<TStoreState>> slices)
        {
            return CombinedReducers;

            TStoreState CombinedReducers(TStoreState state, IAction action)
            {
                var newState = state;
                foreach (var slice in slices)
                {
                    newState = slice.Reduce(newState, action);
                }
                return newState;
            }
        }

        /// <summary>
        /// Apply middlewares to the store.
        /// </summary>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <param name="middlewares"> The middlewares to apply. </param>
        /// <returns> A new store enhancer. </returns>
        public static StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState>
            ApplyMiddleware<TStore,TStoreState>(params Middleware<TStore,TStoreState>[] middlewares)
            where TStore : Store<TStoreState>
        {
            return next => (reducer, initialState) =>
            {
                var storeCtx = next(reducer, initialState);
                var store = storeCtx.store as TStore;
                var middlewareChain = middlewares.Select(middleware => middleware(store));
                storeCtx.dispatcher = middlewareChain
                    .Reverse()
                    .Aggregate(storeCtx.dispatcher, (nextDispatch, middleware) => middleware(nextDispatch));
                return storeCtx;
            };
        }

        /// <summary>
        /// Compose enhancers into a single enhancer.
        /// </summary>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <param name="enhancers"> The enhancers to compose. </param>
        /// <returns> A new store enhancer. </returns>
        public static StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState>
            ComposeEnhancers<TStoreState>(params StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState>[] enhancers)
        {
            return enhancers
                .Aggregate((prev, next) =>
                    createStore => next(prev(createStore)));
        }
    }
}
