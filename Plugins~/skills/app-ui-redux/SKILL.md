---
name: app-ui-redux
description: Expert for App UI Redux state management - Store, Slices, Reducers, Actions, AsyncThunks, middleware, and Redux DevTools.
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# App UI Redux Expert

Expert assistant for implementing Redux state management in Unity using the App UI Redux framework.

## Overview

Redux is a predictable state management pattern for Unity applications. It provides a centralized store to manage application state, making it easier to debug, test, and maintain complex state logic. App UI implements Redux using C# with support for slices, async thunks, middleware, and Redux DevTools.

## Key Concepts

### Why Use Redux?

Use Redux when you need to:
- Manage complex, interconnected state
- Share state across multiple components or systems
- Track state changes for debugging (Redux DevTools)
- Implement predictable state mutations
- Decouple UI from business logic
- Build scalable applications

### Core Principles

1. **Single Source of Truth** - All application state is stored in one centralized store
2. **State is Immutable** - State is never modified directly; new state objects are created
3. **Pure Reducers** - Reducers are pure functions with no side effects
4. **Actions Describe Changes** - Actions are dispatched to describe what happened
5. **Predictability** - Same state + same action = same new state

## Key Namespace

```csharp
using Unity.AppUI.Redux;
```

## Store Creation Patterns

### Basic Store with StoreFactory

```csharp
var store = StoreFactory.CreateStore(new[]
{
    StoreFactory.CreateSlice(
        "sliceName",
        new MyState(),
        builder => { /* reducers */ })
});
```

### Store with Multiple Slices

```csharp
var store = StoreFactory.CreateStore(new[]
{
    StoreFactory.CreateSlice("counter", new CounterState(), counterBuilder => { }),
    StoreFactory.CreateSlice("user", new UserState(), userBuilder => { }),
    StoreFactory.CreateSlice("todos", new TodosState(), todosBuilder => { })
});
```

### Store with Middleware and Enhancers

```csharp
var store = StoreFactory.CreateStore(
    slices: new[] { /* slices */ },
    enhancer: Store.ApplyMiddleware(
        LoggerMiddleware(),
        ThunkMiddleware()
    )
);
```

## Essential Patterns

### State Definition using Records

Always use immutable records with `init` properties:

```csharp
public record CounterState
{
    public int Count { get; init; } = 0;
    public string Status { get; init; } = "idle";
}
```

### Action Creators

**Without Payload:**
```csharp
public static readonly ActionCreator Increment = "counter/Increment";
```

**With Payload:**
```csharp
public static readonly ActionCreator<int> AddAmount = "counter/AddAmount";
public static readonly ActionCreator<string> SetName = "user/SetName";
```

### Reducers - IMPORTANT: Use AddCase, NOT Add

```csharp
builder.AddCase(Actions.Increment, (state, action) =>
    state with { Count = state.Count + 1 });

builder.AddCase(Actions.AddAmount, (state, action) =>
    state with { Count = state.Count + action.payload });
```

**Wrong pattern (do not use):**
```csharp
// DO NOT USE Add() - use AddCase() instead
builder.Add(Actions.Increment, reducer); // INCORRECT
```

### AsyncThunk Operations

AsyncThunk creates pending/fulfilled/rejected actions automatically:

```csharp
public static readonly AsyncThunkCreator<int, string> FetchUserName =
    new("user/fetchName", async (userId, api) =>
    {
        await Task.Delay(1000);
        return $"User_{userId}";
    });

// In extra reducers:
builder.AddCase(FetchUserName.pending, (state, action) =>
    state with { IsLoading = true, Error = null });
builder.AddCase(FetchUserName.fulfilled, (state, action) =>
    state with { IsLoading = false, Name = action.payload });
builder.AddCase(FetchUserName.rejected, (state, action) =>
    state with { IsLoading = false, Error = "Failed to fetch" });
```

### Slice Configuration

```csharp
StoreFactory.CreateSlice(
    name: "counter",
    initialState: new CounterState(),
    reducer: builder =>
    {
        builder.AddCase(Actions.Increment, IncrementReducer);
        builder.AddCase(Actions.Decrement, DecrementReducer);
    },
    extraReducers: builder =>
    {
        // Handle async thunk actions here
        builder.AddCase(MyAsyncThunk.pending, PendingReducer);
        builder.AddCase(MyAsyncThunk.fulfilled, FulfilledReducer);
    }
);
```

### Store Subscription

```csharp
var subscription = store.Subscribe<CounterState>("counter", state =>
{
    Debug.Log($"Counter: {state.Count}");
});

// Unsubscribe when done
subscription.Dispose();
```

### Dispatching Actions

**Simple Actions:**
```csharp
store.Dispatch(Actions.Increment.Invoke());
store.Dispatch(Actions.AddAmount.Invoke(5));
```

**Async Thunks:**
```csharp
var action = MyAsyncThunk.Invoke(123);
store.Dispatch(action);

// Or wait for completion
await store.DispatchAsyncThunk(action);
```

### State Access

```csharp
var state = store.GetState<CounterState>("counter");
Debug.Log($"Current count: {state.Count}");
```

### Selectors (Computed State)

Create selector functions for derived state:

```csharp
public static class Selectors
{
    public static bool IsCounterPositive(CounterState state) =>
        state.Count > 0;

    public static string FormatCount(CounterState state) =>
        $"Count: {state.Count}";
}

// Usage
var isPositive = Selectors.IsCounterPositive(state);
```

## Middleware Configuration

### Logger Middleware

```csharp
public static Middleware<TStore, TStoreState> LoggerMiddleware<TStore, TStoreState>()
    where TStore : IStore
{
    return (store) => (next) => (action) =>
    {
        Debug.Log($"[Redux] Dispatching: {action.type}");
        var result = next(action);
        Debug.Log($"[Redux] Action completed");
        return result;
    };
}
```

### Custom Middleware

```csharp
public static Middleware<TStore, TStoreState> CustomMiddleware<TStore, TStoreState>()
    where TStore : IStore
{
    return (store) => (next) => (action) =>
    {
        // Before action
        Debug.Log($"Before: {action.type}");

        var result = next(action);

        // After action
        Debug.Log($"After: {action.type}");
        return result;
    };
}
```

## Redux DevTools Integration

The Redux DevTools is available in the Unity Editor:

1. Open **Window > App UI > Redux DevTools**
2. Inspect action history and state changes
3. Time-travel debug by stepping through actions
4. Dispatch custom actions for testing

## MVVM Integration

### Using Redux with MVVM ViewModels

```csharp
public interface IStoreService
{
    Store Store { get; }
}

public class StoreService : IStoreService
{
    public Store Store { get; }

    public StoreService()
    {
        Store = StoreFactory.CreateStore(/* slices */);
    }
}

// Register in app builder
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);
        builder.services.AddSingleton<IStoreService, StoreService>();
    }
}

// Use in ViewModel
public class MyViewModel : ObservableObject
{
    readonly IStoreService m_StoreService;

    public MyViewModel(IStoreService storeService)
    {
        m_StoreService = storeService;
        m_StoreService.Store.Subscribe<MyState>("mySlice", state =>
        {
            UpdateUI(state);
        });
    }
}
```

## Common Patterns

### Immutable State Updates

Always use the `with` keyword for immutable updates:

```csharp
// Good
return state with { Count = state.Count + 1 };

// Bad - mutates state
state.Count++;
return state;
```

### Error Handling in AsyncThunk

```csharp
public static readonly AsyncThunkCreator<int, string> FetchData =
    new("app/fetchData", async (id, api) =>
    {
        try
        {
            var result = await SomeApiCall(id);
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed: {ex.Message}");
        }
    });
```

### Dispatching from Async Operations

```csharp
var thunk = new AsyncThunkCreator("operation", async (arg, api) =>
{
    api.Dispatch(MyAction.Invoke("starting"));
    // Do work
    api.Dispatch(MyOtherAction.Invoke("done"));
    return result;
});
```

## Best Practices

1. **Use Records for State** - Immutable by design
2. **Always Use AddCase** - Not Add, for consistency
3. **Keep Reducers Pure** - No side effects, no async operations
4. **Use AsyncThunk for Async Operations** - Not in reducers
5. **Use Selectors** - For derived/computed state
6. **Subscribe Carefully** - Unsubscribe when component/viewmodel is destroyed
7. **Use Redux DevTools** - For debugging complex state flows
8. **Organize Slices** - Group related state together
9. **Name Actions Descriptively** - Use "slice/Action" format
10. **Test Reducers** - Pure functions are easy to unit test

## File Organization

```
YourProject/
├── Redux/
│   ├── Store/
│   │   └── StoreConfiguration.cs
│   ├── Slices/
│   │   ├── CounterSlice.cs
│   │   ├── UserSlice.cs
│   │   └── ...
│   ├── Actions/
│   │   ├── CounterActions.cs
│   │   └── ...
│   ├── Reducers/
│   │   ├── CounterReducers.cs
│   │   └── ...
│   ├── Selectors/
│   │   ├── CounterSelectors.cs
│   │   └── ...
│   └── Middleware/
│       └── CustomMiddleware.cs
```

## Common Issues

**Issue: Reducer mutations not reflecting in UI**
- Solution: Always use `state with { ... }` for immutable updates

**Issue: AsyncThunk not updating state**
- Solution: Handle pending/fulfilled/rejected cases in extra reducers

**Issue: Middleware not being applied**
- Solution: Pass enhancer to StoreFactory.CreateStore second parameter

**Issue: Subscriptions memory leaks**
- Solution: Always dispose subscriptions in OnDestroy/OnDisable

## See Also

- [reference.md](reference.md) - Complete API reference
- [examples/redux-store.cs](examples/redux-store.cs) - Full working example
- State Management Documentation
- Redux DevTools Guide
