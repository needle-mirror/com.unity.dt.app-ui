---
uid: mvvm-di
---

# Dependency Injection

Dependency injection is a software design pattern that implements inversion of control for resolving dependencies. 
It is a technique in which an object receives other objects that it depends on. 
These other objects are called dependencies. 

In the typical "using" relationship, the receiving object would create or find the dependency. 
Instead, in dependency injection, the class itself receives the dependency from an external source.

While Dependency Injection is not tied to the MVVM pattern, it is a very useful pattern to use when building
applications with this kind of architecture.

> [!NOTE]
> The dependency injection pattern provided by the App UI framework is a subset of what you can find in
> .NET Runtime Extensions. For more information, see the 
> [.NET Dependency Injection Extension](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) documentation.

## Types of Dependencies

There are two types of dependencies that you can register in the App UI framework.

### Transient Dependencies

Transient dependencies are created each time they are requested. This is the default behavior when registering a dependency.

You can use [AddTransient](xref:Unity.AppUI.MVVM.ServicesCollectionExtensions.AddTransient(Unity.AppUI.MVVM.IServiceCollection,System.Type)) to register a transient dependency.

### Singleton Dependencies

Singleton dependencies are created the first time they are requested, and then reused for all subsequent requests.

You can use [AddSingleton](xref:Unity.AppUI.MVVM.ServicesCollectionExtensions.AddSingleton(Unity.AppUI.MVVM.IServiceCollection,System.Type)) to register a singleton dependency.

### Scoped Dependencies

Scoped dependencies are created once per scope. A scope is an object that handles the lifetime of dependencies.

> [!NOTE]
> The App UI framework does not provide a built-in implementation of scopes right now.

## Registering Dependencies

There are several ways to register your classes as dependencies in the App UI framework. The registration process is 
done through the [IServiceCollection](xref:Unity.AppUI.MVVM.IServiceCollection) interface.

When building your own [AppBuilder](xref:Unity.AppUI.MVVM.UIToolkitAppBuilder`1) implementation, you can access the service collection 
during the [OnConfiguringApp](xref:Unity.AppUI.MVVM.UIToolkitAppBuilder`1.OnConfiguringApp(Unity.AppUI.MVVM.AppBuilder)) event.

```cs
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder appBuilder)
    {
        // Register dependencies here
        // ex: appBuilder.services.AddSingleton<IMyService, MyService>();
    }
}
```

### Register the class itself

The simplest way to register a dependency is to register the class itself. This is useful when you only need to
register a single implementation of a class.

```cs
public class MyService
{
    public void DoSomething()
    {
        // Do something
    }
}
// Register the service inside the app builder
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder appBuilder)
    {
        appBuilder.services.AddSingleton<MyService>();
    }
}
```

### Register as an Interface

The most common way to register a dependency is to register it as an interface.
This gives you the ability to register multiple implementations of the same interface, and instantiate them 
depending on the context.

```cs
public interface IMyService
{
    void DoSomething();
}
public class MyDebugService : IMyService
{
    public void DoSomething()
    {
        // Do something
    }
}
public class MyProductionService : IMyService
{
    public void DoSomething()
    {
        // Do something
    }
}
// Register the services inside the app builder
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder appBuilder)
    {
        if (IsDebugMode())
            appBuilder.services.AddSingleton<IMyService, MyDebugService>();
        else
            appBuilder.services.AddSingleton<IMyService, MyProductionService>();
    }
}
```

## Use Dependencies

For now App UI framework only supports constructor injection for dependencies.

### Constructor Injection

Constructor injection is the most common way to use dependency injection. 
It is used when the dependency is required by the class. 
The dependency is passed to the class through its constructor.

```cs
public class MyViewModel : ObservableObject
{
    public MyViewModel(MyModel model)
    {
        Model = model;
    }
    public MyModel Model { get; }
}
```