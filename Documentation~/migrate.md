---
uid: migrate
---

# Migration Guide

## Redux

The Redux API has been refactored for more flexibility and better performance.
It should become more intuitive to use.

Please read the [State Management](xref:state-management) guide for more information.

### Create a store and slices

When the [Store&lt;TState&gt;](xref:Unity.AppUI.Redux.Store`1) instance is constructed and returned, it is ready to be used.
You cannot add slices to the store after it has been created.

Before:
```csharp
var store = new Store();
store.AddSlice("sliceName", new MySliceState(), builder => { /* ... */ });
store.AddSlice("slice2Name", /* ... */ );
```

After:
```csharp
var store = StoreFactory.CreateStore(new [] {
    StoreFactory.CreateSlice("sliceName", new MySliceState(), builder => { /* ... */ }),
    StoreFactory.CreateSlice("slice2Name", /* ... */ )
    // ...
});
```

### Build switch cases for reducers

When creating slice, a builder callback is passed as argument for both the
primary reducers and the extra reducers of this slice.
For the ease of use, both builders inherit from the same base class. Instead of calling `Add`
method to add a reducer in the primary reducers builder, you can use the `AddCase` method.

The `AddDefault` and `AddMatcher` methods are still only available in the extra reducers builder.

Before:
```csharp
store.AddSlice("sliceName", new MySliceState(), builder => {
    builder.Add("actionType", (state, action) => { /* ... */ });
});
```

After:
```csharp
StoreFactory.CreateSlice("sliceName", new MySliceState(), builder => {
    builder.AddCase("actionType", (state, action) => { /* ... */ });
});
```

Passing a `string` value as actionType will automatically create an action creator for this action type, but
you will have to explicitly cast the string to either a `ActionCreator` or `ActionCreator<T>`.
You can also directly pass an `ActionCreator` or `ActionCreator<T>` instance.

Before:
```csharp
store.AddSlice("sliceName", new MySliceState(), builder => {
    builder.Add("actionType", (state, action) => { /* ... */ });
});
```

After:
```csharp
static readonly ActionCreator actionType0 = "actionType0"; // no payload
static readonly ActionCreator<int> actionType1 = nameof(actionType1); // with int payload

StoreFactory.CreateSlice("sliceName", new MySliceState(), builder => {
    builder.AddCase((ActionCreator<int>)"actionType2", (state, action) => { /* ... */ }); // cast to ActionCreator<int>
});
```

### Reducer declaration

Reducers now takes an `IAction` interface type as the second parameter instead of `Action`.

Before:
```csharp
// with no payload
static MyAppState MyReducer1(MyAppState state, Action action) { /* ... */ }
// with int payload
static MyAppState MyReducer2(MyAppState state, Action<int> action) { /* ... */ }
```

After:
```csharp
// with no payload
static MyAppState MyReducer1(MyAppState state, IAction action) { /* ... */ }
// with int payload
static MyAppState MyReducer2(MyAppState state, IAction<int> action) { /* ... */ }
```

### Subscribe to store changes

When subscribing to store changes, instead of returning a function to unsubscribe,
an [IDisposableSubscription](xref:Unity.AppUI.Redux.IDisposableSubscription) instance is returned.
You can check its validity and dispose of it.

Before:
```csharp
// Subscribe to store changes
var unsub = store.Subscribe("sliceName", state => { /* ... */ });
// Unsubscribe
unsub();
```

After:
```csharp
// Subscribe to store changes
var subscription = store.Subscribe("sliceName", state => { /* ... */ });
// Unsubscribe
subscription.Dispose();
// Check if subscription is valid
Assert.IsFalse(subscription.IsValid());
```
