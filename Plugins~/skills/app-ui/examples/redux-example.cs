// Redux State Management Example

using Unity.AppUI.Redux;
using UnityEngine;

// ============================================================================
// 1. STATE DEFINITION
// ============================================================================

public record AppState
{
    public CounterState Counter { get; init; } = new();
    public UserState User { get; init; } = new();
}

public record CounterState
{
    public int Count { get; init; } = 0;
}

public record UserState
{
    public string Name { get; init; } = "";
    public bool IsLoading { get; init; } = false;
    public string Error { get; init; } = null;
}

// ============================================================================
// 2. ACTION CREATORS
// ============================================================================

public static class Actions
{
    // Counter actions
    public static readonly ActionCreator Increment = "counter/Increment";
    public static readonly ActionCreator Decrement = "counter/Decrement";
    public static readonly ActionCreator<int> AddAmount = "counter/AddAmount";
    public static readonly ActionCreator Reset = "counter/Reset";

    // User actions
    public static readonly ActionCreator<string> SetName = "user/SetName";
    public static readonly ActionCreator ClearUser = "user/ClearUser";

    // Async thunk for fetching user
    public static readonly AsyncThunkCreator<int, string> FetchUserName =
        new("user/fetchName", async (userId, api) =>
        {
            // Simulate API call
            await System.Threading.Tasks.Task.Delay(1000);
            return $"User_{userId}";
        });
}

// ============================================================================
// 3. REDUCERS
// ============================================================================

public static class Reducers
{
    // Counter reducers
    public static CounterState IncrementReducer(CounterState state, IAction action)
    {
        return state with { Count = state.Count + 1 };
    }

    public static CounterState DecrementReducer(CounterState state, IAction action)
    {
        return state with { Count = state.Count - 1 };
    }

    public static CounterState AddAmountReducer(CounterState state, IAction<int> action)
    {
        return state with { Count = state.Count + action.payload };
    }

    public static CounterState ResetReducer(CounterState state, IAction action)
    {
        return state with { Count = 0 };
    }

    // User reducers
    public static UserState SetNameReducer(UserState state, IAction<string> action)
    {
        return state with { Name = action.payload };
    }

    public static UserState ClearUserReducer(UserState state, IAction action)
    {
        return state with { Name = "", IsLoading = false, Error = null };
    }

    // Async thunk reducers
    public static UserState FetchPendingReducer(UserState state, IAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    public static UserState FetchFulfilledReducer(UserState state, IAction<string> action)
    {
        return state with { IsLoading = false, Name = action.payload };
    }

    public static UserState FetchRejectedReducer(UserState state, IAction action)
    {
        return state with { IsLoading = false, Error = "Failed to fetch user" };
    }
}

// ============================================================================
// 4. STORE SETUP
// ============================================================================

public static class StoreConfig
{
    public static Store<AppState> CreateStore()
    {
        return StoreFactory.CreateStore(new[]
        {
            // Counter slice
            StoreFactory.CreateSlice("counter", new CounterState(), builder =>
            {
                builder.AddCase(Actions.Increment, Reducers.IncrementReducer);
                builder.AddCase(Actions.Decrement, Reducers.DecrementReducer);
                builder.AddCase(Actions.AddAmount, Reducers.AddAmountReducer);
                builder.AddCase(Actions.Reset, Reducers.ResetReducer);
            }),

            // User slice with async thunk support
            StoreFactory.CreateSlice("user", new UserState(), builder =>
            {
                builder.AddCase(Actions.SetName, Reducers.SetNameReducer);
                builder.AddCase(Actions.ClearUser, Reducers.ClearUserReducer);
            },
            extraReducers: builder =>
            {
                // Handle async thunk states
                builder.AddCase(Actions.FetchUserName.pending, Reducers.FetchPendingReducer);
                builder.AddCase(Actions.FetchUserName.fulfilled, Reducers.FetchFulfilledReducer);
                builder.AddCase(Actions.FetchUserName.rejected, Reducers.FetchRejectedReducer);
            })
        });
    }
}

// ============================================================================
// 5. USAGE EXAMPLE
// ============================================================================

public class ReduxExample : MonoBehaviour
{
    private Store<AppState> m_Store;
    private IDisposableSubscription m_CounterSubscription;
    private IDisposableSubscription m_UserSubscription;

    void Start()
    {
        // Create store
        m_Store = StoreConfig.CreateStore();

        // Subscribe to counter state changes
        m_CounterSubscription = m_Store.Subscribe<CounterState>("counter", state =>
        {
            Debug.Log($"Counter: {state.Count}");
        });

        // Subscribe to user state changes
        m_UserSubscription = m_Store.Subscribe<UserState>("user", state =>
        {
            if (state.IsLoading)
                Debug.Log("Loading user...");
            else if (!string.IsNullOrEmpty(state.Error))
                Debug.Log($"Error: {state.Error}");
            else
                Debug.Log($"User: {state.Name}");
        });

        // Dispatch actions
        m_Store.Dispatch(Actions.Increment.Invoke());
        m_Store.Dispatch(Actions.Increment.Invoke());
        m_Store.Dispatch(Actions.AddAmount.Invoke(5));

        // Dispatch async thunk
        m_Store.Dispatch(Actions.FetchUserName.Invoke(123));
    }

    void OnDestroy()
    {
        // Clean up subscriptions
        m_CounterSubscription?.Dispose();
        m_UserSubscription?.Dispose();
    }
}

// ============================================================================
// 6. MIDDLEWARE EXAMPLE
// ============================================================================

public static class Middleware
{
    public static Middleware<TStore, TStoreState> LoggerMiddleware<TStore, TStoreState>()
        where TStore : IStore
    {
        return (store) => (next) => (action) =>
        {
            Debug.Log($"[Redux] Dispatching: {action.type}");
            var result = next(action);
            Debug.Log($"[Redux] State after: {store.GetState()}");
            return result;
        };
    }
}
