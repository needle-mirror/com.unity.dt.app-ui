---
uid: state-management
---

# State Management

App UI provides an additional assembly called `Unity.AppUI.Redux` that contains a set of classes that implement 
the [Redux](https://redux.js.org/) pattern. 

The Redux pattern is a way to manage the state of your application. 
It is a pattern that is used in many different frameworks and libraries, especially in the JavaScript ecosystem. 

The Redux pattern is based on the following principles:

* The state of your application is stored in a single object called the `store`.
* The only way to change the state is to dispatch an `action` to the `store`.
* To specify how the state tree is transformed by actions, you write pure `reducers`.
* The `store` is created by combining the `reducers` into a single reducer function.
* The `store` has a single `dispatch` method that accepts an `action` and returns a `state` object.
* The `store` has a single `getState` method that returns the current `state` object.
* The `store` has a single `subscribe` method that accepts a callback that is called every time the `state` changes.

For more extensibility, App UI includes the concept of **Slice**. A slice is a part of the state tree that is managed by a
specific reducer. A slice can be used to manage a specific part of the application state, such as the state of a specific
screen. It is also useful to monitor changes on a specific part of the state tree. This approach is similar to what 
offers [Redux Toolkit](https://redux-toolkit.js.org/).

## Components

### Store

The [Store](xref:Unity.AppUI.Redux.Store) class is the main entry point for the Redux pattern. 
It is responsible for creating the `store` and dispatching actions to it.

### Reducer

A reducer is a pure function that takes the current state and an action as parameters and returns the new state.
The user has to create Reducers as pure functions inside the application code. You can check the [Simple Counter](#simple-counter)
example to see how to create a reducer.

### Slice

A [Slice](xref:Unity.AppUI.Redux.Slice) is a part of the state tree that is managed by a specific reducer.
You can add multiple slices to the store. Each slice has a unique name that is used to identify it.

### Action Creator And Action

An action creator is a method that is used to create an action. An action is an object that is dispatched to the store.
The action creator method can be created using the [CreateAction](xref:Unity.AppUI.Redux.Store.CreateAction(System.String)) utility method.

To get an action instance from the Actioncreator, you can call the `Invoke` method on the action creator, and eventually
pass the action parameters to it (if any).

You can then dispatch the action to the store using the [Dispatch](xref:Unity.AppUI.Redux.Store.Dispatch(System.String)) method.

### Middleware

Middlewares are not yet supported in App UI, but they will be added in the future.

## Examples

### Simple Counter

First, create the state object that will be used by the store. This object will contain the current value of the counter.
You can take advantage of the new C# 9.0 feature called [Records](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9#record-types).
The `record` type is a reference type that is immutable by default. It is a good fit for the state object.

```cs
public record CounterState
{
    public int Count { get; init; } = 0;
}
```

Then, create a reducer method that will be used to update the state. The reducer method is a pure function that takes
the current state and an action as parameters and returns the new state. The reducer method is responsible for updating
the state based on the action type. In this example, the reducer method will only handle the `Increment` action.

Since when building the [Store](xref:Unity.AppUI.Redux.Store) you can tie a reducer to a specific action type,
you don't have to to check the action type inside the reducer method. The reducer method will only be called when the
action type matches the one that is tied to the reducer.

```cs
public static CounterState IncrementReducer(CounterState state, Increment action)
{
    return state with { Count = state.Count + 1 };
}
```

Now, create an action creator method that will be used to create the `Increment` action. The action creator method can be created 
using [CreateAction](xref:Unity.AppUI.Redux.Store.CreateAction(System.String)) utility method. 
The action creator method will be used to create the `Increment` action that will be dispatched to the store.

```cs
public static Actions
{
    public const string Increment = "counter/Increment";
}

public static readonly ActionCreator Increment = Store.CreateAction(Actions.Increment);
```

Finally, create the store and subscribe to the state changes. The store is created by passing the reducer method to the
[Store](xref:Unity.AppUI.Redux.Store.CreateSlice``1(System.String,``0,System.Action{Unity.AppUI.Redux.SliceReducerSwitchBuilder{``0}},System.Action{Unity.AppUI.Redux.ReducerSwitchBuilder{``0}})) method. 
The store is responsible for calling the reducer method when an action is dispatched to it. The store also provides a 
`Subscribe` method that accepts a callback that is called every time the state changes. 
The callback is called with the new state as a parameter.

```cs
var store = new Store();
store.CreateSlice<CounterState>("counter", new CounterState(), builder => {
    builder.Add(Actions.Increment, IncrementReducer);
});

var unsubscriber = store.Subscribe<CounterState>("counter", state => {
    Debug.Log($"Counter value: {state.Count}");
});
```

Now, you can dispatch the `Increment` action to the store. The store will call the reducer method and update the state.

```cs
store.Dispatch(Increment.Invoke());
```

To unsubscribe from the state changes, call the `unsubscriber` method that was returned by the 
[Subscribe](xref:Unity.AppUI.Redux.Store.Subscribe``1(System.String,System.Action{``0})) method.

```cs
unsubscriber();
```

### Using the Store inside the MVVM Pattern

> [!NOTE]
> This example is using the **MVVM** pattern. If you are not familiar with the MVVM pattern, you can read the
> [MVVM](xref:mvvm-intro) documentation first.

The [Store](xref:Unity.AppUI.Redux.Store) class can be used inside the MVVM pattern, as a service.

First, create a service that will be used to access the store. The service will be responsible for dispatching actions
to the store and subscribing to the state changes.

```cs
public interface IStoreService
{
    Store Store { get; }
}

public class StoreService 
{
    public Store Store { get; }

    public StoreService()
    {
        Store = new Store();
    }
}
```

Then, register the service inside your custom [UIToolkitAppBuilder](xref:Unity.AppUI.MVVM.UIToolkitAppBuilder`1) 
implementation.

```cs
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);
        
        builder.services.AddSingleton<IStoreService, StoreService>();
        
        // Add others services/viewmodels/views here...
    }
}
```

Now, you can access the store inside your viewmodels. The store can be injected via constructor injection.

```cs
public class MyViewModel : ObservableObject
{
    readonly IStoreService m_StoreService;

    public MyViewModel(IStoreService storeService)
    {
        m_StoreService = storeService;
        
        // Subscribe to the state changes etc...
    }
}
```
