using Unity.AppUI.Redux.DevTools.States;

namespace Unity.AppUI.Redux.DevTools
{
    static class Reducers
    {
        internal static AppState SetFoundStores(AppState state, IAction<FoundStore[]> action)
        {
            var s = state with
            {
                stores = action.payload
            };
            var currentSelectedStoreFound = false;
            if (s.stores != null)
            {
                foreach (var store in s.stores)
                {
                    if (store == s.selectedStore)
                    {
                        currentSelectedStoreFound = true;
                        break;
                    }
                }
            }

            if (!currentSelectedStoreFound)
            {
                s = s with
                {
                    selectedStore = s.stores is {Length: > 0} ? s.stores[0] : null
                };
            }

            return s;
        }

        internal static AppState SetSelectedStore(AppState state, IAction<FoundStore> action)
        {
            return state with
            {
                selectedStore = action.payload
            };
        }

        internal static AppState SetIsRecording(AppState state, IAction<bool> action)
        {
            return state with
            {
                recording = action.payload
            };
        }
    }
}
