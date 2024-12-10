using System;

namespace Unity.AppUI.Redux
{
    /// <summary>
    /// Utility class for creating thunk middleware.
    /// </summary>
    public static class Thunk
    {
        /// <summary>
        /// The default thunk middleware.
        /// </summary>
        /// <typeparam name="TStore"> The type of the store. </typeparam>
        /// <typeparam name="TStoreState"> The type of the store state. </typeparam>
        /// <returns> The thunk middleware. </returns>
        public static Middleware<TStore,TStoreState> ThunkMiddleware<TStore,TStoreState>()
            where TStore : Store<TStoreState>
            => store => next => action =>
        {
            if (action is IAsyncThunkAction asyncThunkAction)
            {
                _ = asyncThunkAction.ExecuteAsync(store);
            }
            else
            {
                next(action);
            }
        };
    }
}
