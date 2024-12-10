using Unity.AppUI.Redux.DevTools;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// Utility class to inject the default enhancer into a Redux Store.
    /// The default enhancer includes the thunk middleware and the dev tools enhancer.
    /// </summary>
    public static class DefaultEnhancer
    {
        /// <summary>
        /// Configuration for the default enhancer.
        /// </summary>
        /// <seealso cref="GetDefaultEnhancer{TStore,TStoreState}"/>
        public struct Configuration
        {
            /// <summary>
            /// The configuration for the dev tools.
            /// </summary>
            public DevToolsConfiguration devTools;
        }

        /// <summary>
        /// Get the default enhancer for the store.
        /// </summary>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <param name="cfg"> The configuration for the default enhancer. </param>
        /// <returns> The default enhancer. </returns>
        public static StoreEnhancer<Store<TStoreState>.CreationContext,TStoreState>
            GetDefaultEnhancer<TStore,TStoreState>(Configuration cfg = default)
            where TStore : Store<TStoreState>
        {
            var middlewares = Store.ApplyMiddleware(
                Thunk.ThunkMiddleware<TStore,TStoreState>());
            var enhancer = middlewares;
            if (cfg.devTools.enabled)
                enhancer = Store.ComposeEnhancers(enhancer, DevToolsService.Enhancer<TStoreState>(cfg.devTools));
            return enhancer;
        }
    }
}
