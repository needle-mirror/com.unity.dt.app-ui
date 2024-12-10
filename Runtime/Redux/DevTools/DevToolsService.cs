using System;
using UnityEngine;

namespace Unity.AppUI.Redux.DevTools
{
    /// <summary>
    /// The configuration for the DevTools.
    /// </summary>
    public struct DevToolsConfiguration
    {
        /// <summary>
        /// Whether the DevTools are enabled.
        /// </summary>
        public bool enabled;
    }

    /// <summary>
    /// A class that provides a set of tools for debugging and inspecting the state of the store.
    /// </summary>
    public class DevToolsService
    {
        const string initActionType = "@@INIT";

        const string pausedActionType = "@@PAUSED";

        const string resumedActionType = "@@RESUMED";

        const string lockedActionType = "@@LOCKED";

        const string unlockedActionType = "@@UNLOCKED";

        internal static DevToolsService Instance { get; } = new DevToolsService();

        /// <summary>
        /// A reducer for DevTools actions.
        /// </summary>
        /// <param name="baseReducer"> The base reducer. </param>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> The new reducer. </returns>
        public static Reducer<TStoreState> Reducer<TStoreState>(Reducer<TStoreState> baseReducer)
        {
            return (state, action) =>
            {
                if (action == null)
                    return state;

                return action.type switch
                {
                    initActionType => state,
                    pausedActionType => state,
                    resumedActionType => state,
                    lockedActionType => state,
                    unlockedActionType => state,
                    _ => baseReducer(state, action)
                };
            };
        }

        /// <summary>
        /// Enhancer for the store creation that adds the DevTools to the store.
        /// </summary>
        /// <param name="config"> The configuration for the DevTools. </param>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> The store enhancer. </returns>
        public static StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState>
            Enhancer<TStoreState>(DevToolsConfiguration config = default)
        {
            return createStore => (reducer, initialState) =>
            {
                var storeCreator = createStore(reducer, initialState);
                if (!config.enabled)
                    return storeCreator;
                var originalDispatcher = storeCreator.dispatcher;
                storeCreator.reducer = Reducer(storeCreator.reducer);
                storeCreator.dispatcher = action =>
                {
                    var previousState = storeCreator.store.GetState();
                    originalDispatcher(action);
                    var nextState = storeCreator.store.GetState();
                    Instance.Record(storeCreator.store, config, action, previousState, nextState);
                };

                return storeCreator;
            };
        }

        internal void Record<TState>(
            Store<TState> store,
            DevToolsConfiguration config,
            IAction action,
            TState previousState,
            TState nextState)
        {
            Debug.Log($"Action: {action.type}");
        }
    }
}
