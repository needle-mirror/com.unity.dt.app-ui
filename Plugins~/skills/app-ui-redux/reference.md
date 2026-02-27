# App UI Redux Reference

Complete reference guide for Redux state management patterns in Unity.

## Table of Contents

1. [Store Creation](#store-creation)
2. [Slices](#slices)
3. [Actions and ActionCreators](#actions-and-actioncreators)
4. [Reducers](#reducers)
5. [AsyncThunk](#asyncthunk)
6. [Middleware](#middleware)
7. [Store Subscriptions](#store-subscriptions)
8. [State Selectors](#state-selectors)
9. [Redux DevTools](#redux-devtools)
10. [Common Patterns](#common-patterns)

## Store Creation

### StoreFactory.CreateStore

Creates a Redux store with the specified slices.

**Signature:**
```csharp
public static Store<TState> CreateStore<TState>(
    Slice<TState>[] slices,
    StoreEnhancer<Store<TState>, TState> enhancer = null)
    where TState : IPartionableState<TState>
```

**Basic Example:**
```csharp
var store = StoreFactory.CreateStore(new[]
{
    StoreFactory.CreateSlice("counter", new CounterState(), builder =>
    {
        builder.AddCase(Actions.Increment, IncrementReducer);
    })
});
```

**With Multiple Slices:**
```csharp
var store = StoreFactory.CreateStore(new[]
{
    StoreFactory.CreateSlice("counter", new CounterState(), counterBuilder =>
    {
        counterBuilder.AddCase(Actions.Increment, (state, _) =>
            state with { Count = state.Count + 1 });
    }),
    StoreFactory.CreateSlice("user", new UserState(), userBuilder =>
    {
        userBuilder.AddCase(Actions.SetName, (state, action) =>
            state with { Name = action.payload });
    })
});
```

**With Enhancer (Middleware):**
```csharp
var store = StoreFactory.CreateStore(
    new[] { /* slices */ },
    enhancer: Store.ApplyMiddleware(LoggerMiddleware(), DebugMiddleware())
);
```

### Store Class Methods

**Dispatch an action:**
```csharp
store.Dispatch(Actions.Increment.Invoke());
store.Dispatch(Actions.AddAmount.Invoke(10));
```

**Get current state:**
```csharp
var counterState = store.GetState<CounterState>("counter");
var allState = store.GetState(); // Gets entire PartitionedState
```

**Subscribe to state changes:**
```csharp
var subscription = store.Subscribe<CounterState>("counter", state =>
{
    Debug.Log($"Count changed: {state.Count}");
});
```

**Dispatch async thunk:**
```csharp
var asyncAction = MyAsyncThunk.Invoke(arg);
await store.DispatchAsyncThunk(asyncAction);
```

## Slices

A slice is a portion of the Redux state tree managed by a specific reducer.

### StoreFactory.CreateSlice

Creates a slice with reducers and optional extra reducers.

**Signature:**
```csharp
public static Slice<TState> CreateSlice<TState>(
    string name,
    TState initialState,
    Action<SliceReducerSwitchBuilder<TState>> reducer = null,
    Action<ReducerSwitchBuilder<TState>> extraReducers = null)
```

**Simple Slice:**
```csharp
StoreFactory.CreateSlice(
    name: "counter",
    initialState: new CounterState { Count = 0 },
    reducer: builder =>
    {
        builder.AddCase(Actions.Increment, (state, _) =>
            state with { Count = state.Count + 1 });
        builder.AddCase(Actions.Decrement, (state, _) =>
            state with { Count = state.Count - 1 });
    }
)
```

**Slice with Payload Actions:**
```csharp
StoreFactory.CreateSlice(
    name: "user",
    initialState: new UserState(),
    reducer: builder =>
    {
        builder.AddCase(Actions.SetName, (state, action) =>
            state with { Name = action.payload });
        builder.AddCase(Actions.SetEmail, (state, action) =>
            state with { Email = action.payload });
    }
)
```

**Slice with AsyncThunk (Extra Reducers):**
```csharp
StoreFactory.CreateSlice(
    name: "users",
    initialState: new UsersState(),
    reducer: builder =>
    {
        builder.AddCase(Actions.AddUser, (state, action) =>
            state with { Users = state.Users.Append(action.payload).ToArray() });
    },
    extraReducers: builder =>
    {
        builder.AddCase(Actions.FetchUsers.pending, (state, _) =>
            state with { IsLoading = true, Error = null });
        builder.AddCase(Actions.FetchUsers.fulfilled, (state, action) =>
            state with { IsLoading = false, Users = action.payload });
        builder.AddCase(Actions.FetchUsers.rejected, (state, action) =>
            state with { IsLoading = false, Error = action.payload });
    }
)
```

## Actions and ActionCreators

Actions are plain objects that describe what happened. ActionCreators are functions that create actions.

### ActionCreator (No Payload)

**Declaration:**
```csharp
public static readonly ActionCreator Increment = "counter/Increment";
public static readonly ActionCreator Decrement = "counter/Decrement";
public static readonly ActionCreator Reset = "counter/Reset";
```

**Usage:**
```csharp
var action = Increment.Invoke();
store.Dispatch(Increment.Invoke());
```

### ActionCreator<TPayload> (With Payload)

**Declaration:**
```csharp
public static readonly ActionCreator<int> AddAmount = "counter/AddAmount";
public static readonly ActionCreator<string> SetName = "user/SetName";
public static readonly ActionCreator<User> AddUser = "users/AddUser";
```

**Usage:**
```csharp
var action = AddAmount.Invoke(5);
store.Dispatch(AddAmount.Invoke(10));

var setNameAction = SetName.Invoke("John");
store.Dispatch(setNameAction);
```

### Accessing Payload in Reducer

**Without Payload:**
```csharp
builder.AddCase(Actions.Increment, (state, action) =>
{
    // action has no payload
    return state with { Count = state.Count + 1 };
});
```

**With Payload:**
```csharp
builder.AddCase(Actions.AddAmount, (state, action) =>
{
    // access payload via action.payload
    return state with { Count = state.Count + action.payload };
});
```

## Reducers

Reducers are pure functions that transform state based on actions. They must be immutable.

### Reducer Rules

1. Must be pure (no side effects, no API calls, no random values)
2. Must be deterministic (same input = same output)
3. Must not mutate the input state
4. Must use `state with { ... }` for immutable updates
5. Must be registered using `AddCase`, not `Add`

### Basic Reducer Pattern

```csharp
public static CounterState IncrementReducer(CounterState state, IAction action)
{
    return state with { Count = state.Count + 1 };
}

// In slice builder:
builder.AddCase(Actions.Increment, IncrementReducer);
```

### Reducer with Payload

```csharp
public static CounterState AddAmountReducer(CounterState state, IAction<int> action)
{
    return state with { Count = state.Count + action.payload };
}

// In slice builder:
builder.AddCase(Actions.AddAmount, AddAmountReducer);
```

### Inline Reducer

```csharp
builder.AddCase(Actions.Increment, (state, action) =>
    state with { Count = state.Count + 1 });
```

### Complex State Updates

```csharp
builder.AddCase(Actions.UpdateUser, (state, action) =>
    state with
    {
        Name = action.payload.Name,
        Email = action.payload.Email,
        UpdatedAt = System.DateTime.Now
    });
```

### Reducer with Conditional Logic

```csharp
builder.AddCase(Actions.AddItem, (state, action) =>
{
    if (state.Items.Count >= state.MaxItems)
        return state; // Don't add if at max

    var newItems = state.Items.Append(action.payload).ToArray();
    return state with { Items = newItems };
});
```

### IMPORTANT: Use AddCase, Not Add

```csharp
// CORRECT - Use AddCase
builder.AddCase(Actions.Increment, (state, action) =>
    state with { Count = state.Count + 1 });

// WRONG - Do not use Add
builder.Add(Actions.Increment, (state, action) =>
    state with { Count = state.Count + 1 }); // INCORRECT!
```

## AsyncThunk

AsyncThunk is used for asynchronous operations like API calls. It automatically generates pending, fulfilled, and rejected actions.

### AsyncThunkCreator Declaration

**Signature:**
```csharp
public AsyncThunkCreator<TArg, TResult>(
    string type,
    Func<TArg, ThunkAPI<TArg, TResult>, CancellationToken, Task<TResult>> payloadCreator)
```

**Simple AsyncThunk (No Arguments):**
```csharp
public static readonly AsyncThunkCreator<Unit, string[]> FetchUsers =
    new("users/fetchUsers", async (_, api, token) =>
    {
        var response = await ApiClient.GetUsersAsync(token);
        return response.Users;
    });
```

**AsyncThunk with Arguments:**
```csharp
public static readonly AsyncThunkCreator<int, User> FetchUser =
    new("users/fetchUser", async (userId, api, token) =>
    {
        var response = await ApiClient.GetUserAsync(userId, token);
        return response.User;
    });
```

### AsyncThunk States (pending/fulfilled/rejected)

Each AsyncThunk automatically creates three action creators:

```csharp
var thunk = new AsyncThunkCreator<int, string>("myOp", ...);

thunk.pending;   // IAction - triggered when operation starts
thunk.fulfilled; // IAction<TResult> - triggered on success
thunk.rejected;  // IAction - triggered on error
```

### Handling AsyncThunk in Extra Reducers

```csharp
extraReducers: builder =>
{
    // Pending state - operation starting
    builder.AddCase(FetchUser.pending, (state, action) =>
        state with
        {
            IsLoading = true,
            Error = null,
            CurrentUser = null
        });

    // Fulfilled state - operation succeeded
    builder.AddCase(FetchUser.fulfilled, (state, action) =>
        state with
        {
            IsLoading = false,
            CurrentUser = action.payload, // payload is User
            Error = null
        });

    // Rejected state - operation failed
    builder.AddCase(FetchUser.rejected, (state, action) =>
        state with
        {
            IsLoading = false,
            CurrentUser = null,
            Error = "Failed to fetch user"
        });
}
```

### Dispatching AsyncThunk

**Basic Dispatch (fire and forget):**
```csharp
var action = FetchUser.Invoke(userId);
store.Dispatch(action);
```

**Await Completion:**
```csharp
var action = FetchUser.Invoke(userId);
await store.DispatchAsyncThunk(action);
var state = store.GetState<UserState>("users");
Debug.Log(state.CurrentUser.Name);
```

### Dispatching Other Actions from Async Operation

```csharp
public static readonly AsyncThunkCreator<int, string[]> FetchUsersAndNotify =
    new("users/fetchAndNotify", async (_, api, token) =>
    {
        api.Dispatch(Actions.SetLoading.Invoke(true));
        try
        {
            var users = await ApiClient.GetUsersAsync(token);
            api.Dispatch(Actions.ShowNotification.Invoke("Users loaded"));
            return users;
        }
        catch
        {
            api.Dispatch(Actions.ShowError.Invoke("Failed to load users"));
            throw;
        }
    });
```

### Complete AsyncThunk Example

```csharp
public record UserState
{
    public User CurrentUser { get; init; } = null;
    public bool IsLoading { get; init; } = false;
    public string Error { get; init; } = null;
}

public static readonly AsyncThunkCreator<int, User> FetchUser =
    new("user/fetch", async (userId, api, token) =>
    {
        await Task.Delay(1000, token);
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID");
        return new User { Id = userId, Name = $"User_{userId}" };
    });

// In slice:
StoreFactory.CreateSlice(
    "user",
    new UserState(),
    extraReducers: builder =>
    {
        builder.AddCase(FetchUser.pending, (state, _) =>
            state with { IsLoading = true, Error = null });
        builder.AddCase(FetchUser.fulfilled, (state, action) =>
            state with { IsLoading = false, CurrentUser = action.payload });
        builder.AddCase(FetchUser.rejected, (state, _) =>
            state with { IsLoading = false, Error = "Failed to fetch" });
    }
)
```

## Middleware

Middleware intercepts actions before they're dispatched to the store.

### Middleware Signature

```csharp
public delegate Middleware<TStore, TStoreState> (
    TStore store) => (
    Middleware next) => (
    IAction action) => object;
```

### Logger Middleware Example

```csharp
public static Middleware<Store<TState>, TState> LoggerMiddleware<TState>()
    where TState : IPartionableState<TState>
{
    return (store) => (next) => (action) =>
    {
        Debug.Log($"[Redux] Action: {action.type}");
        var result = next(action);
        Debug.Log($"[Redux] State updated");
        return result;
    };
}

// Usage:
var store = StoreFactory.CreateStore(
    slices,
    enhancer: Store.ApplyMiddleware(LoggerMiddleware<AppState>())
);
```

### Filter Middleware (Only log specific actions)

```csharp
public static Middleware<Store<TState>, TState> SelectiveLoggerMiddleware<TState>(
    params string[] actionTypes)
    where TState : IPartionableState<TState>
{
    return (store) => (next) => (action) =>
    {
        if (actionTypes.Contains(action.type))
            Debug.Log($"[Redux] Action: {action.type}");
        return next(action);
    };
}

// Usage:
Store.ApplyMiddleware(
    SelectiveLoggerMiddleware<AppState>("user/SetName", "counter/Increment")
)
```

### Timing Middleware (Measure action duration)

```csharp
public static Middleware<Store<TState>, TState> TimingMiddleware<TState>()
    where TState : IPartionableState<TState>
{
    return (store) => (next) => (action) =>
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = next(action);
        stopwatch.Stop();
        Debug.Log($"[Redux] {action.type} took {stopwatch.ElapsedMilliseconds}ms");
        return result;
    };
}
```

### Composing Multiple Middleware

```csharp
var store = StoreFactory.CreateStore(
    slices,
    enhancer: Store.ApplyMiddleware(
        LoggerMiddleware<AppState>(),
        TimingMiddleware<AppState>(),
        ErrorHandlingMiddleware<AppState>()
    )
);
```

## Store Subscriptions

Subscriptions allow listening to state changes.

### Subscribe Method

**Signature:**
```csharp
public IDisposableSubscription Subscribe<TSliceState>(
    string sliceName,
    Listener<TSliceState> listener,
    SubscribeOptions<TSliceState> options = null)
    where TSliceState : class
```

### Basic Subscription

```csharp
var subscription = store.Subscribe<CounterState>("counter", state =>
{
    Debug.Log($"Counter updated: {state.Count}");
});
```

### Multiple Subscriptions

```csharp
var counterSub = store.Subscribe<CounterState>("counter", state =>
    Debug.Log($"Count: {state.Count}"));

var userSub = store.Subscribe<UserState>("user", state =>
    Debug.Log($"User: {state.Name}"));
```

### Unsubscribe

```csharp
subscription.Dispose();

// Or using try/finally:
try
{
    // Use store
}
finally
{
    subscription.Dispose();
}
```

### Subscription with Filter

```csharp
var subscription = store.Subscribe<CounterState>(
    "counter",
    state => Debug.Log($"Count: {state.Count}"),
    new SubscribeOptions<CounterState>
    {
        Selector = state => state.Count // Only subscribe to Count changes
    });
```

### Component Lifecycle Integration

```csharp
public class MyComponent : MonoBehaviour
{
    private IDisposableSubscription m_Subscription;

    void Start()
    {
        m_Subscription = m_Store.Subscribe<MyState>("mySlice", OnStateChanged);
    }

    void OnStateChanged(MyState state)
    {
        // Handle state change
    }

    void OnDestroy()
    {
        m_Subscription?.Dispose();
    }
}
```

## State Selectors

Selectors are functions that extract and compute derived state.

### Basic Selector

```csharp
public static class CounterSelectors
{
    public static int SelectCount(CounterState state) => state.Count;

    public static bool SelectIsPositive(CounterState state) => state.Count > 0;

    public static string SelectCounterDisplay(CounterState state) =>
        $"Count: {state.Count}";
}

// Usage:
var count = CounterSelectors.SelectCount(state);
```

### Memoized Selector Pattern

```csharp
public static class UserSelectors
{
    public static User SelectCurrentUser(UserState state) => state.CurrentUser;

    public static string SelectUserDisplayName(UserState state) =>
        state.CurrentUser?.Name ?? "Anonymous";

    public static bool SelectIsUserLoaded(UserState state) =>
        state.CurrentUser != null && !state.IsLoading;
}
```

### Selector from Store

```csharp
var state = store.GetState<CounterState>("counter");
var isPositive = CounterSelectors.SelectIsPositive(state);
```

## Redux DevTools

Redux DevTools is a browser extension for inspecting state and actions.

### Enabling DevTools

In the Unity Editor:
1. Go to **Window > App UI > Redux DevTools**
2. DevTools window opens as a dockable panel

### Using DevTools

**View Actions:**
- See all dispatched actions in chronological order
- Click action to see action object and payload

**Inspect State:**
- View current state after each action
- Compare state before and after

**Time Travel:**
- Click on any action to jump to that point
- Step through actions one by one
- Jump back to previous states

**Dispatch Custom Actions:**
- Test how reducers handle new actions
- Verify state transitions

**Export/Import State:**
- Save application state for later testing
- Load saved states for debugging

## Common Patterns

### Counter State Pattern

```csharp
public record CounterState
{
    public int Count { get; init; } = 0;
}

public static readonly ActionCreator Increment = "counter/Increment";
public static readonly ActionCreator Decrement = "counter/Decrement";
public static readonly ActionCreator<int> AddAmount = "counter/AddAmount";

StoreFactory.CreateSlice("counter", new CounterState(), builder =>
{
    builder.AddCase(Increment, (state, _) =>
        state with { Count = state.Count + 1 });
    builder.AddCase(Decrement, (state, _) =>
        state with { Count = state.Count - 1 });
    builder.AddCase(AddAmount, (state, action) =>
        state with { Count = state.Count + action.payload });
});
```

### Loading State Pattern

```csharp
public record LoadingState
{
    public bool IsLoading { get; init; } = false;
    public string Error { get; init; } = null;
    public bool HasError => !string.IsNullOrEmpty(Error);
}

public record UsersState : LoadingState
{
    public User[] Users { get; init; } = Array.Empty<User>();
}

extraReducers: builder =>
{
    builder.AddCase(FetchUsers.pending, (state, _) =>
        state with { IsLoading = true, Error = null });
    builder.AddCase(FetchUsers.fulfilled, (state, action) =>
        state with { IsLoading = false, Users = action.payload });
    builder.AddCase(FetchUsers.rejected, (state, _) =>
        state with { IsLoading = false, Error = "Failed to load" });
}
```

### MVVM Service Integration

```csharp
public interface IReduxService
{
    Store Store { get; }
    void Subscribe<T>(string sliceName, Action<T> listener)
        where T : class;
}

public class ReduxService : IReduxService
{
    public Store Store { get; }

    public ReduxService()
    {
        Store = StoreFactory.CreateStore(/* slices */);
    }

    public void Subscribe<T>(string sliceName, Action<T> listener)
        where T : class
    {
        Store.Subscribe(sliceName, listener);
    }
}
```

### Observable Pattern for MVVM

```csharp
public class CounterViewModel : ObservableObject
{
    readonly IReduxService m_ReduxService;
    private int m_Count;

    public int Count
    {
        get => m_Count;
        private set => SetProperty(ref m_Count, value);
    }

    public CounterViewModel(IReduxService reduxService)
    {
        m_ReduxService = reduxService;
        m_ReduxService.Subscribe<CounterState>("counter", state =>
        {
            Count = state.Count;
        });
    }

    public void Increment()
    {
        m_ReduxService.Store.Dispatch(Actions.Increment.Invoke());
    }
}
```

### Nested State Pattern

```csharp
public record AppState
{
    public CounterState Counter { get; init; } = new();
    public UserState User { get; init; } = new();
    public TodosState Todos { get; init; } = new();
}

// Or using IPartionableState for cleaner API
public class AppStatePartitioned : IPartionableState<AppStatePartitioned>
{
    readonly Dictionary<string, object> m_Slices = new();

    public TSliceState Get<TSliceState>(string sliceName) =>
        m_Slices.TryGetValue(sliceName, out var slice) ? (TSliceState)slice : default;

    public AppStatePartitioned Set<TSliceState>(string sliceName, TSliceState sliceState)
    {
        var newState = new AppStatePartitioned();
        foreach (var slice in m_Slices)
            newState.m_Slices[slice.Key] = slice.Value;
        newState.m_Slices[sliceName] = sliceState;
        return newState;
    }
}
```

### Error Handling Pattern

```csharp
public record ApiState
{
    public string Error { get; init; } = null;
    public bool HasError => !string.IsNullOrEmpty(Error);
    public void ThrowIfError() { if (HasError) throw new InvalidOperationException(Error); }
}

// In reducer:
builder.AddCase(MyAsyncThunk.rejected, (state, action) =>
    state with { Error = "Operation failed" });
```

### Reset/Clear State Pattern

```csharp
public static readonly ActionCreator ResetCounter = "counter/Reset";
public static readonly ActionCreator ClearUser = "user/Clear";

builder.AddCase(ResetCounter, (state, _) => new CounterState());
builder.AddCase(ClearUser, (state, _) => new UserState());
```
