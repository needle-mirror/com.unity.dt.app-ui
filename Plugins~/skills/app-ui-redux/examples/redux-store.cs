// Complete Redux Store Setup Example
// This example demonstrates a complete Redux setup with multiple slices,
// async operations, and middleware configuration.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.Redux;
using UnityEngine;

// ============================================================================
// 1. STATE DEFINITIONS - Using immutable records with init
// ============================================================================

/// <summary>
/// Counter slice state - simple counter with status
/// </summary>
public record CounterState
{
    public int Count { get; init; } = 0;
    public string LastAction { get; init; } = "none";
    public DateTime LastActionTime { get; init; } = DateTime.MinValue;
}

/// <summary>
/// User slice state - represents authenticated user
/// </summary>
public record UserState
{
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public int Id { get; init; } = -1;
    public bool IsLoading { get; init; } = false;
    public string Error { get; init; } = null;
    public bool HasError => !string.IsNullOrEmpty(Error);
}

/// <summary>
/// Todos slice state - todo list management
/// </summary>
public record TodosState
{
    public TodoItem[] Items { get; init; } = Array.Empty<TodoItem>();
    public bool IsLoading { get; init; } = false;
    public string Error { get; init; } = null;
    public int TotalCount { get; init; } = 0;
}

public record TodoItem
{
    public int Id { get; init; }
    public string Title { get; init; }
    public bool Completed { get; init; } = false;
    public string Description { get; init; } = "";
}

// ============================================================================
// 2. ACTION CREATORS - Define all actions for the store
// ============================================================================

public static class CounterActions
{
    // Simple actions without payload
    public static readonly ActionCreator Increment = "counter/Increment";
    public static readonly ActionCreator Decrement = "counter/Decrement";
    public static readonly ActionCreator Reset = "counter/Reset";

    // Actions with payload
    public static readonly ActionCreator<int> AddAmount = "counter/AddAmount";
    public static readonly ActionCreator<int> SetCount = "counter/SetCount";
}

public static class UserActions
{
    // Simple user actions
    public static readonly ActionCreator Logout = "user/Logout";
    public static readonly ActionCreator ClearError = "user/ClearError";

    // User actions with payload
    public static readonly ActionCreator<string> SetName = "user/SetName";
    public static readonly ActionCreator<string> SetEmail = "user/SetEmail";

    // Async thunk for login operation
    public static readonly AsyncThunkCreator<(string email, string password), int> LoginUser =
        new("user/login", async (credentials, api, token) =>
        {
            // Simulate API call delay
            await Task.Delay(1500, token);

            // Simulate validation
            if (string.IsNullOrEmpty(credentials.email) || string.IsNullOrEmpty(credentials.password))
                throw new InvalidOperationException("Invalid credentials");

            // Simulate user ID response
            return credentials.email.GetHashCode();
        });

    // Async thunk for fetching user profile
    public static readonly AsyncThunkCreator<int, (string name, string email)> FetchUserProfile =
        new("user/fetchProfile", async (userId, api, token) =>
        {
            await Task.Delay(1000, token);
            return ($"User_{userId}", $"user_{userId}@example.com");
        });
}

public static class TodoActions
{
    // Simple todo actions
    public static readonly ActionCreator ClearError = "todos/ClearError";
    public static readonly ActionCreator ClearAll = "todos/ClearAll";

    // Todo actions with payload
    public static readonly ActionCreator<TodoItem> AddTodo = "todos/AddTodo";
    public static readonly ActionCreator<int> RemoveTodo = "todos/RemoveTodo";
    public static readonly ActionCreator<int> ToggleTodo = "todos/ToggleTodo";
    public static readonly ActionCreator<(int id, string title)> UpdateTodoTitle = "todos/UpdateTodoTitle";

    // Async thunk for fetching todos
    public static readonly AsyncThunkCreator<Unit, TodoItem[]> FetchTodos =
        new("todos/fetch", async (_, api, token) =>
        {
            await Task.Delay(1200, token);

            return new[]
            {
                new TodoItem { Id = 1, Title = "Learn Redux", Completed = true },
                new TodoItem { Id = 2, Title = "Build App", Completed = false },
                new TodoItem { Id = 3, Title = "Deploy", Completed = false }
            };
        });

    // Async thunk for adding todo to server
    public static readonly AsyncThunkCreator<TodoItem, TodoItem> SaveTodoToServer =
        new("todos/saveToServer", async (todo, api, token) =>
        {
            // Simulate server save
            await Task.Delay(800, token);
            // Return todo with assigned ID from server
            return todo with { Id = DateTimeOffset.UtcNow.ToUnixTimeSeconds().GetHashCode() };
        });
}

// ============================================================================
// 3. REDUCER FUNCTIONS - Pure functions that transform state
// ============================================================================

public static class CounterReducers
{
    // IMPORTANT: Always use AddCase in the builder, not Add!

    public static CounterState IncrementReducer(CounterState state, IAction action)
    {
        return state with
        {
            Count = state.Count + 1,
            LastAction = "increment",
            LastActionTime = DateTime.Now
        };
    }

    public static CounterState DecrementReducer(CounterState state, IAction action)
    {
        return state with
        {
            Count = state.Count - 1,
            LastAction = "decrement",
            LastActionTime = DateTime.Now
        };
    }

    public static CounterState AddAmountReducer(CounterState state, IAction<int> action)
    {
        return state with
        {
            Count = state.Count + action.payload,
            LastAction = $"addAmount({action.payload})",
            LastActionTime = DateTime.Now
        };
    }

    public static CounterState SetCountReducer(CounterState state, IAction<int> action)
    {
        return state with
        {
            Count = action.payload,
            LastAction = $"setCount({action.payload})",
            LastActionTime = DateTime.Now
        };
    }

    public static CounterState ResetReducer(CounterState state, IAction action)
    {
        return new CounterState { LastAction = "reset", LastActionTime = DateTime.Now };
    }
}

public static class UserReducers
{
    public static UserState SetNameReducer(UserState state, IAction<string> action)
    {
        return state with { Name = action.payload };
    }

    public static UserState SetEmailReducer(UserState state, IAction<string> action)
    {
        return state with { Email = action.payload };
    }

    public static UserState LogoutReducer(UserState state, IAction action)
    {
        return new UserState();
    }

    public static UserState ClearErrorReducer(UserState state, IAction action)
    {
        return state with { Error = null };
    }

    // Async thunk reducers - handle pending/fulfilled/rejected states

    public static UserState LoginPendingReducer(UserState state, IAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    public static UserState LoginFulfilledReducer(UserState state, IAction<int> action)
    {
        return state with { IsLoading = false, Id = action.payload };
    }

    public static UserState LoginRejectedReducer(UserState state, IAction action)
    {
        return state with { IsLoading = false, Error = "Login failed" };
    }

    public static UserState FetchProfilePendingReducer(UserState state, IAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    public static UserState FetchProfileFulfilledReducer(UserState state, IAction<(string name, string email)> action)
    {
        return state with
        {
            IsLoading = false,
            Name = action.payload.name,
            Email = action.payload.email
        };
    }

    public static UserState FetchProfileRejectedReducer(UserState state, IAction action)
    {
        return state with { IsLoading = false, Error = "Failed to fetch profile" };
    }
}

public static class TodoReducers
{
    public static TodosState AddTodoReducer(TodosState state, IAction<TodoItem> action)
    {
        var newItems = state.Items.Append(action.payload).ToArray();
        return state with { Items = newItems, TotalCount = newItems.Length };
    }

    public static TodosState RemoveTodoReducer(TodosState state, IAction<int> action)
    {
        var newItems = state.Items.Where(t => t.Id != action.payload).ToArray();
        return state with { Items = newItems, TotalCount = newItems.Length };
    }

    public static TodosState ToggleTodoReducer(TodosState state, IAction<int> action)
    {
        var newItems = state.Items.Select(t =>
            t.Id == action.payload ? t with { Completed = !t.Completed } : t
        ).ToArray();
        return state with { Items = newItems };
    }

    public static TodosState UpdateTodoTitleReducer(TodosState state, IAction<(int id, string title)> action)
    {
        var (id, title) = action.payload;
        var newItems = state.Items.Select(t =>
            t.Id == id ? t with { Title = title } : t
        ).ToArray();
        return state with { Items = newItems };
    }

    public static TodosState ClearAllReducer(TodosState state, IAction action)
    {
        return new TodosState();
    }

    public static TodosState ClearErrorReducer(TodosState state, IAction action)
    {
        return state with { Error = null };
    }

    // Async thunk reducers

    public static TodosState FetchTodosPendingReducer(TodosState state, IAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    public static TodosState FetchTodosFulfilledReducer(TodosState state, IAction<TodoItem[]> action)
    {
        return state with
        {
            IsLoading = false,
            Items = action.payload,
            TotalCount = action.payload.Length
        };
    }

    public static TodosState FetchTodosRejectedReducer(TodosState state, IAction action)
    {
        return state with { IsLoading = false, Error = "Failed to fetch todos" };
    }

    public static TodosState SaveTodoToServerPendingReducer(TodosState state, IAction action)
    {
        return state with { IsLoading = true };
    }

    public static TodosState SaveTodoToServerFulfilledReducer(TodosState state, IAction<TodoItem> action)
    {
        return state with { IsLoading = false };
    }

    public static TodosState SaveTodoToServerRejectedReducer(TodosState state, IAction action)
    {
        return state with { IsLoading = false, Error = "Failed to save todo" };
    }
}

// ============================================================================
// 4. STATE SELECTORS - Derived/computed state
// ============================================================================

public static class CounterSelectors
{
    public static int SelectCount(CounterState state) => state.Count;

    public static bool SelectIsPositive(CounterState state) => state.Count > 0;

    public static bool SelectIsNegative(CounterState state) => state.Count < 0;

    public static string SelectDisplay(CounterState state) =>
        $"Count: {state.Count} (Last: {state.LastAction})";
}

public static class UserSelectors
{
    public static bool SelectIsAuthenticated(UserState state) => state.Id >= 0;

    public static string SelectDisplayName(UserState state) =>
        string.IsNullOrEmpty(state.Name) ? "Anonymous" : state.Name;

    public static string SelectFullInfo(UserState state) =>
        $"{state.Name} ({state.Email})";
}

public static class TodoSelectors
{
    public static int SelectCompletedCount(TodosState state) =>
        state.Items.Count(t => t.Completed);

    public static int SelectPendingCount(TodosState state) =>
        state.Items.Count(t => !t.Completed);

    public static decimal SelectCompletionPercentage(TodosState state) =>
        state.Items.Length == 0 ? 0 : (SelectCompletedCount(state) / (decimal)state.Items.Length) * 100;

    public static TodoItem[] SelectCompletedTodos(TodosState state) =>
        state.Items.Where(t => t.Completed).ToArray();

    public static TodoItem[] SelectPendingTodos(TodosState state) =>
        state.Items.Where(t => !t.Completed).ToArray();
}

// ============================================================================
// 5. MIDDLEWARE - Processing actions before store
// ============================================================================

public static class ReduxMiddleware
{
    /// <summary>
    /// Logger middleware that logs all actions
    /// </summary>
    public static Middleware<Store<PartitionedState>, PartitionedState> LoggerMiddleware()
    {
        return (store) => (next) => (action) =>
        {
            Debug.Log($"[Redux] Dispatching: {action.type}");
            var result = next(action);
            Debug.Log($"[Redux] Action completed: {action.type}");
            return result;
        };
    }

    /// <summary>
    /// Timing middleware that measures action duration
    /// </summary>
    public static Middleware<Store<PartitionedState>, PartitionedState> TimingMiddleware()
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

    /// <summary>
    /// Selective logger that only logs specific actions
    /// </summary>
    public static Middleware<Store<PartitionedState>, PartitionedState> SelectiveLoggerMiddleware(
        params string[] actionTypes)
    {
        return (store) => (next) => (action) =>
        {
            if (actionTypes.Contains(action.type))
                Debug.Log($"[Redux] Action: {action.type}");
            return next(action);
        };
    }
}

// ============================================================================
// 6. STORE CONFIGURATION - Complete store setup
// ============================================================================

public static class StoreConfiguration
{
    /// <summary>
    /// Creates the complete Redux store with all slices and middleware
    /// </summary>
    public static Store<PartitionedState> CreateReduxStore()
    {
        return StoreFactory.CreateStore(
            // Array of slices
            new[]
            {
                // Counter slice with simple reducers
                StoreFactory.CreateSlice(
                    name: "counter",
                    initialState: new CounterState(),
                    reducer: builder =>
                    {
                        // IMPORTANT: Use AddCase, NOT Add
                        builder.AddCase(CounterActions.Increment, CounterReducers.IncrementReducer);
                        builder.AddCase(CounterActions.Decrement, CounterReducers.DecrementReducer);
                        builder.AddCase(CounterActions.AddAmount, CounterReducers.AddAmountReducer);
                        builder.AddCase(CounterActions.SetCount, CounterReducers.SetCountReducer);
                        builder.AddCase(CounterActions.Reset, CounterReducers.ResetReducer);
                    }
                ),

                // User slice with async thunk support
                StoreFactory.CreateSlice(
                    name: "user",
                    initialState: new UserState(),
                    reducer: builder =>
                    {
                        builder.AddCase(UserActions.SetName, UserReducers.SetNameReducer);
                        builder.AddCase(UserActions.SetEmail, UserReducers.SetEmailReducer);
                        builder.AddCase(UserActions.Logout, UserReducers.LogoutReducer);
                        builder.AddCase(UserActions.ClearError, UserReducers.ClearErrorReducer);
                    },
                    extraReducers: builder =>
                    {
                        // Handle login async thunk (pending/fulfilled/rejected)
                        builder.AddCase(UserActions.LoginUser.pending, UserReducers.LoginPendingReducer);
                        builder.AddCase(UserActions.LoginUser.fulfilled, UserReducers.LoginFulfilledReducer);
                        builder.AddCase(UserActions.LoginUser.rejected, UserReducers.LoginRejectedReducer);

                        // Handle fetch profile async thunk
                        builder.AddCase(UserActions.FetchUserProfile.pending, UserReducers.FetchProfilePendingReducer);
                        builder.AddCase(UserActions.FetchUserProfile.fulfilled, UserReducers.FetchProfileFulfilledReducer);
                        builder.AddCase(UserActions.FetchUserProfile.rejected, UserReducers.FetchProfileRejectedReducer);
                    }
                ),

                // Todos slice with async thunk support
                StoreFactory.CreateSlice(
                    name: "todos",
                    initialState: new TodosState(),
                    reducer: builder =>
                    {
                        builder.AddCase(TodoActions.AddTodo, TodoReducers.AddTodoReducer);
                        builder.AddCase(TodoActions.RemoveTodo, TodoReducers.RemoveTodoReducer);
                        builder.AddCase(TodoActions.ToggleTodo, TodoReducers.ToggleTodoReducer);
                        builder.AddCase(TodoActions.UpdateTodoTitle, TodoReducers.UpdateTodoTitleReducer);
                        builder.AddCase(TodoActions.ClearAll, TodoReducers.ClearAllReducer);
                        builder.AddCase(TodoActions.ClearError, TodoReducers.ClearErrorReducer);
                    },
                    extraReducers: builder =>
                    {
                        // Handle fetch todos async thunk
                        builder.AddCase(TodoActions.FetchTodos.pending, TodoReducers.FetchTodosPendingReducer);
                        builder.AddCase(TodoActions.FetchTodos.fulfilled, TodoReducers.FetchTodosFulfilledReducer);
                        builder.AddCase(TodoActions.FetchTodos.rejected, TodoReducers.FetchTodosRejectedReducer);

                        // Handle save todo async thunk
                        builder.AddCase(TodoActions.SaveTodoToServer.pending, TodoReducers.SaveTodoToServerPendingReducer);
                        builder.AddCase(TodoActions.SaveTodoToServer.fulfilled, TodoReducers.SaveTodoToServerFulfilledReducer);
                        builder.AddCase(TodoActions.SaveTodoToServer.rejected, TodoReducers.SaveTodoToServerRejectedReducer);
                    }
                ),
            },
            // Enhancer with middleware
            enhancer: Store.ApplyMiddleware(
                ReduxMiddleware.LoggerMiddleware(),
                ReduxMiddleware.TimingMiddleware()
            )
        );
    }
}

// ============================================================================
// 7. USAGE EXAMPLE - Demonstrates store operations
// ============================================================================

public class ReduxStoreExample : MonoBehaviour
{
    private Store<PartitionedState> m_Store;
    private IDisposableSubscription m_CounterSubscription;
    private IDisposableSubscription m_UserSubscription;
    private IDisposableSubscription m_TodosSubscription;

    void Start()
    {
        // Create the Redux store
        m_Store = StoreConfiguration.CreateReduxStore();

        // Subscribe to counter changes
        m_CounterSubscription = m_Store.Subscribe<CounterState>("counter", state =>
        {
            Debug.Log($"Counter State: {CounterSelectors.SelectDisplay(state)}");
        });

        // Subscribe to user changes
        m_UserSubscription = m_Store.Subscribe<UserState>("user", state =>
        {
            Debug.Log($"User State: Authenticated={UserSelectors.SelectIsAuthenticated(state)}, " +
                $"Name={UserSelectors.SelectDisplayName(state)}");
        });

        // Subscribe to todos changes
        m_TodosSubscription = m_Store.Subscribe<TodosState>("todos", state =>
        {
            Debug.Log($"Todos: Total={state.TotalCount}, " +
                $"Completed={TodoSelectors.SelectCompletedCount(state)}, " +
                $"Progress={TodoSelectors.SelectCompletionPercentage(state):F1}%");
        });

        // Example operations
        ExampleCounterOperations();
        ExampleUserOperations();
        ExampleTodoOperations();
    }

    private void ExampleCounterOperations()
    {
        // Dispatch simple actions
        m_Store.Dispatch(CounterActions.Increment.Invoke());
        m_Store.Dispatch(CounterActions.Increment.Invoke());
        m_Store.Dispatch(CounterActions.AddAmount.Invoke(10));
        m_Store.Dispatch(CounterActions.Decrement.Invoke());

        // Get state
        var counterState = m_Store.GetState<CounterState>("counter");
        Debug.Log($"Current count: {counterState.Count}");
    }

    private void ExampleUserOperations()
    {
        // Set user info
        m_Store.Dispatch(UserActions.SetName.Invoke("John Doe"));
        m_Store.Dispatch(UserActions.SetEmail.Invoke("john@example.com"));

        // Async operation (fire and forget)
        var loginAction = UserActions.LoginUser.Invoke(("john@example.com", "password123"));
        m_Store.Dispatch(loginAction);
    }

    private async void ExampleTodoOperations()
    {
        // Fetch todos from server
        var fetchAction = TodoActions.FetchTodos.Invoke();
        await m_Store.DispatchAsyncThunk(fetchAction);

        // Add a new todo
        m_Store.Dispatch(TodoActions.AddTodo.Invoke(
            new TodoItem { Id = 999, Title = "New Task", Description = "Test todo" }
        ));

        // Toggle a todo
        m_Store.Dispatch(TodoActions.ToggleTodo.Invoke(1));

        // Get todos state
        var todosState = m_Store.GetState<TodosState>("todos");
        var pendingCount = TodoSelectors.SelectPendingCount(todosState);
        Debug.Log($"Pending todos: {pendingCount}");
    }

    void OnDestroy()
    {
        // Clean up subscriptions
        m_CounterSubscription?.Dispose();
        m_UserSubscription?.Dispose();
        m_TodosSubscription?.Dispose();
    }
}
