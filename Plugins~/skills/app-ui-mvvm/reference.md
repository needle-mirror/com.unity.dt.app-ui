# App UI MVVM & Dependency Injection - Reference

## Table of Contents
1. [Observable Properties](#observable-properties)
2. [RelayCommand](#relaycommand)
3. [Async Commands](#async-commands)
4. [AppBuilder Setup](#appbuilder-setup)
5. [Service Registration](#service-registration)
6. [Service Injection](#service-injection)
7. [Dependency Injection Listener](#dependency-injection-listener)
8. [Data Binding](#data-binding)
9. [ViewModel Lifecycle](#viewmodel-lifecycle)
10. [Code Examples](#code-examples)

## Observable Properties

### ObservableObject Class
Base class for objects that notify on property changes.

```csharp
using Unity.AppUI.MVVM;

[ObservableObject]
public partial class MyViewModel
{
    // Properties are auto-generated
}
```

**Key Features:**
- Implements `INotifyPropertyChanged` and `INotifyPropertyChanging`
- Provides `SetProperty()` methods for safe property updates
- Exposes `PropertyChanged` and `PropertyChanging` events

### ObservableProperty Attribute
Auto-generates observable properties from private fields.

**Syntax:**
```csharp
[ObservableObject]
public partial class MyViewModel
{
    [ObservableProperty]
    private string _name;
    // Generates: public string Name { get; set; }
}
```

**Requirements:**
- Class must be marked `[ObservableObject]`
- Class must be `partial`
- Field must be private
- Field name must start with underscore (`_`)

**Example:**
```csharp
[ObservableObject]
public partial class UserViewModel
{
    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private bool _isActive;

    // Generated properties:
    // public string Username { get; set; }
    // public int Age { get; set; }
    // public bool IsActive { get; set; }
}
```

### AlsoNotifyChangeFor Attribute
Notifies other properties when a property changes.

**Syntax:**
```csharp
[ObservableProperty]
[AlsoNotifyChangeFor(nameof(PropertyName1))]
[AlsoNotifyChangeFor(nameof(PropertyName2))]
private T _fieldName;
```

**Use Cases:**
- Dependent properties that derive from other properties
- Computed properties that change when source properties change
- Validation flags that depend on multiple properties

**Example:**
```csharp
[ObservableObject]
public partial class ValidationViewModel
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsValid))]
    [AlsoNotifyChangeFor(nameof(ErrorMessage))]
    private string _email;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsValid))]
    private string _password;

    [CreateProperty(ReadOnly = true)]
    public bool IsValid => ValidateInput();

    [CreateProperty(ReadOnly = true)]
    public string ErrorMessage => GetErrorMessage();

    private bool ValidateInput() => !string.IsNullOrEmpty(Email) && Email.Contains("@")
        && !string.IsNullOrEmpty(Password) && Password.Length >= 8;

    private string GetErrorMessage()
    {
        if (string.IsNullOrEmpty(Email)) return "Email is required";
        if (!Email.Contains("@")) return "Invalid email format";
        if (string.IsNullOrEmpty(Password)) return "Password is required";
        if (Password.Length < 8) return "Password must be at least 8 characters";
        return "";
    }
}
```

### CreateProperty Attribute (from Unity.Properties)
Creates bindable properties for data binding in UI Toolkit.

```csharp
using Unity.Properties;

[CreateProperty]
public string Name { get; set; }

[CreateProperty(ReadOnly = true)]
public bool IsValid => CheckValidity();
```

## RelayCommand

### Basic RelayCommand
Wraps a method as an executable command.

**Syntax:**
```csharp
[RelayCommand]
private void MethodName()
{
    // Command execution logic
}
// Generates: public RelayCommand MethodNameCommand { get; }
```

**Example:**
```csharp
[ObservableObject]
public partial class ButtonViewModel
{
    [RelayCommand]
    private void SaveData()
    {
        Debug.Log("Saving data...");
    }

    [RelayCommand]
    private void DeleteItem()
    {
        Debug.Log("Deleting item...");
    }
}

// Usage:
// var vm = new ButtonViewModel();
// vm.SaveDataCommand.Execute();
// vm.DeleteItemCommand.Execute();
```

### RelayCommand with Parameter
Pass data when executing the command.

**Syntax:**
```csharp
[RelayCommand]
private void MethodName(ParameterType parameter)
{
    // Use parameter
}
// Generates: public RelayCommand<ParameterType> MethodNameCommand { get; }
```

**Example:**
```csharp
[ObservableObject]
public partial class ItemViewModel
{
    [RelayCommand]
    private void DeleteItem(int itemId)
    {
        Debug.Log($"Deleting item {itemId}");
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        Debug.Log($"Selected category: {category}");
    }
}

// Usage:
// var vm = new ItemViewModel();
// vm.DeleteItemCommand.Execute(42);
// vm.SelectCategoryCommand.Execute("Electronics");
```

### RelayCommand with CanExecute
Control whether the command can be executed.

**Syntax:**
```csharp
[RelayCommand(CanExecute = nameof(CanExecuteMethod))]
private void MethodName()
{
    // Execution logic
}

private bool CanExecuteMethod() => /* condition */;
```

**Example:**
```csharp
[ObservableObject]
public partial class FormViewModel
{
    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        Debug.Log($"Logging in as {Email}");
    }

    private bool CanLogin =>
        !string.IsNullOrEmpty(Email) &&
        !string.IsNullOrEmpty(Password) &&
        Email.Contains("@");
}
```

### Async RelayCommand
Execute async operations as commands.

**Syntax:**
```csharp
[RelayCommand]
private async Task MethodNameAsync()
{
    // Async operation
}
// Generates: public AsyncRelayCommand MethodNameAsyncCommand { get; }
```

**Supports:**
- `async Task` methods
- `async Task<T>` methods (result is returned)
- Parameters: `async Task MethodName(ParameterType param)`
- CanExecute conditions

**Example:**
```csharp
[ObservableObject]
public partial class DataLoadViewModel
{
    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage;

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        ErrorMessage = "";

        try
        {
            // Simulate async data loading
            await Task.Delay(1000);
            var items = await FetchItemsFromServer();

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteItem))]
    private async Task DeleteItemAsync(Item item)
    {
        IsLoading = true;
        try
        {
            await RemoveItemFromServer(item.Id);
            Items.Remove(item);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDeleteItem => Items?.Count > 0;

    private async Task<List<Item>> FetchItemsFromServer() => new();
    private async Task RemoveItemFromServer(int id) => await Task.Delay(100);
}
```

## Async Commands

### AsyncRelayCommand Options
Control concurrent execution behavior.

```csharp
using Unity.AppUI.MVVM;

public enum AsyncRelayCommandOptions
{
    None = 0,
    AllowConcurrentExecutions = 1
}
```

**Default Behavior (None):**
- Command blocks new executions while already executing
- Subsequent calls while executing are ignored
- Safe for operations that shouldn't run concurrently

**AllowConcurrentExecutions:**
- Multiple invocations can execute simultaneously
- Useful for independent async operations
- Requires careful state management

**Example:**
```csharp
[ObservableObject]
public partial class DownloadViewModel
{
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private async Task DownloadAsync(string url)
    {
        // Only one download at a time (default behavior)
        Debug.Log($"Downloading {url}");
        await Task.Delay(2000);
    }

    private bool CanDownload => true;
}
```

## AppBuilder Setup

### UIToolkitAppBuilder
Configures the application and dependency injection.

**Inheritance:**
```csharp
using Unity.AppUI.MVVM;

public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder appBuilder)
    {
        base.OnConfiguringApp(appBuilder);
        // Register services here
    }

    protected override void OnAppInitialized(MyApp app)
    {
        base.OnAppInitialized(app);
        // App initialized
    }

    protected override void OnAppShuttingDown(MyApp app)
    {
        base.OnAppShuttingDown(app);
        // App shutting down
    }
}
```

**Lifecycle:**
1. `OnConfiguringApp()` - Configure services and app settings
2. `OnAppInitialized()` - App ready to use
3. `OnAppShuttingDown()` - Cleanup before shutdown

### App Class
Entry point for the application.

```csharp
using Unity.AppUI.MVVM;

public class MyApp : App
{
    public new static MyApp current => (MyApp)App.current;

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        // Initialize app UI
        rootVisualElement.Add(services.GetRequiredService<MainPage>());
    }

    public override void Shutdown()
    {
        // Cleanup
    }
}
```

**Complete AppBuilder Example:**
```csharp
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder appBuilder)
    {
        base.OnConfiguringApp(appBuilder);

        // Register services
        appBuilder.services.AddSingleton<ILogger, ConsoleLogger>();
        appBuilder.services.AddSingleton<IDataRepository, DataRepository>();

        // Register ViewModels
        appBuilder.services.AddTransient<MainViewModel>();
        appBuilder.services.AddTransient<DetailViewModel>();

        // Register Pages
        appBuilder.services.AddTransient<MainPage>();
        appBuilder.services.AddTransient<DetailPage>();
    }

    protected override void OnAppInitialized(MyApp app)
    {
        base.OnAppInitialized(app);
        Debug.Log("App initialized successfully!");
    }

    protected override void OnAppShuttingDown(MyApp app)
    {
        base.OnAppShuttingDown(app);
        Debug.Log("App shutting down");
    }
}
```

## Service Registration

### AddTransient
Create new instance every time requested.

```csharp
services.AddTransient<MyService>();
services.AddTransient<IMyService, MyService>();

// Usage:
var service1 = serviceProvider.GetRequiredService<MyService>();
var service2 = serviceProvider.GetRequiredService<MyService>();
// service1 != service2
```

### AddSingleton
Create once, reuse forever.

```csharp
services.AddSingleton<MyService>();
services.AddSingleton<IMyService, MyService>();

// Usage:
var service1 = serviceProvider.GetRequiredService<MyService>();
var service2 = serviceProvider.GetRequiredService<MyService>();
// service1 == service2
```

### AddScoped
Create once per scope.

```csharp
services.AddScoped<MyService>();
services.AddScoped<IMyService, MyService>();

// Usage:
using (var scope = serviceProvider.CreateScope())
{
    var service1 = scope.ServiceProvider.GetRequiredService<MyService>();
    var service2 = scope.ServiceProvider.GetRequiredService<MyService>();
    // service1 == service2 (same scope)
}

using (var scope2 = serviceProvider.CreateScope())
{
    var service3 = scope2.ServiceProvider.GetRequiredService<MyService>();
    // service3 != service1 (different scope)
}
```

### Conditional Registration (When)
Register implementations based on conditions.

```csharp
// AddTransientWhen, AddSingletonWhen, AddScopedWhen
services.AddSingletonWhen(
    typeof(ILogger),
    typeof(FileLogger),
    ctx => ctx.RequestingType == typeof(DatabaseService));

services.AddSingletonWhen(
    typeof(ILogger),
    typeof(ConsoleLogger),
    ctx => ctx.RequestingType == typeof(UIViewModel));
```

**ResolutionContext Properties:**
- `ServiceType` - The interface/type being requested
- `RequestingType` - The class requesting the service
- `IsScoped` - Whether resolution is in a scoped context
- `ServiceProvider` - The current service provider

## Service Injection

### Constructor Injection
Inject dependencies through constructor parameters.

```csharp
[ObservableObject]
public partial class MyViewModel
{
    private readonly IMyService _myService;
    private readonly ILogger _logger;

    public MyViewModel(IMyService myService, ILogger logger)
    {
        _myService = myService;
        _logger = logger;
    }
}
```

**Best For:**
- Required dependencies
- Immutable services
- Clear dependency declaration

### Service Attribute (Property Injection)
Inject dependencies into properties or fields.

```csharp
using Unity.AppUI.MVVM;

[ObservableObject]
public partial class MyViewModel
{
    [Service]
    public IMyService MyService { get; set; }

    [Service]
    private ILogger _logger;
}
```

**Requirements:**
- Properties must have a setter (can be private)
- Fields will be set after constructor
- Marked with `[Service]` attribute

**Best For:**
- Optional dependencies
- Mutable services
- Properties that change after initialization

### Service Attribute Example
```csharp
[ObservableObject]
public partial class DataViewModel : IDependencyInjectionListener
{
    [Service]
    public IDataRepository Repository { get; set; }

    [Service]
    private ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<Data> _items = new();

    public void OnDependenciesInjected()
    {
        _logger.Log("Dependencies injected, loading data...");
        LoadData();
    }

    private void LoadData()
    {
        var items = Repository.GetAll();
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }
}
```

## Dependency Injection Listener

### IDependencyInjectionListener Interface
Listen for property injection completion.

```csharp
using Unity.AppUI.MVVM;

public interface IDependencyInjectionListener
{
    void OnDependenciesInjected();
}
```

**When Called:**
- After all `[Service]` properties are injected
- Before the object is used elsewhere
- Perfect for initialization code that depends on injected services

**Example:**
```csharp
[ObservableObject]
public partial class ComplexViewModel : IDependencyInjectionListener
{
    [Service]
    public IDataService DataService { get; set; }

    [Service]
    public IEventAggregator EventAggregator { get; set; }

    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

    [ObservableProperty]
    private bool _isInitialized;

    public void OnDependenciesInjected()
    {
        Debug.Log("All dependencies injected, initializing...");

        // Subscribe to events
        EventAggregator.Subscribe<ItemAddedEvent>(OnItemAdded);
        EventAggregator.Subscribe<ItemRemovedEvent>(OnItemRemoved);

        // Load initial data
        LoadItems();

        IsInitialized = true;
    }

    private void LoadItems()
    {
        var items = DataService.GetItems();
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }

    private void OnItemAdded(ItemAddedEvent @event)
    {
        Items.Add(@event.Item);
    }

    private void OnItemRemoved(ItemRemovedEvent @event)
    {
        Items.Remove(@event.Item);
    }
}
```

## Data Binding

### UXML Data Binding
Bind ViewModel properties to UI elements in UXML.

**Syntax:**
```xml
<ui:DataBinding
    property="targetProperty"
    binding-mode="ToTarget|ToSource|TwoWay"
    data-source-path="viewModelProperty"/>
```

**Binding Modes:**
- `ToTarget` - ViewModel → UI (read-only for UI)
- `ToSource` - UI → ViewModel (user input)
- `TwoWay` - Bidirectional synchronization

**Example UXML:**
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    <appui:Panel data-source-type="MyNamespace.UserViewModel, Assembly-CSharp">
        <!-- Text binding -->
        <appui:TextField label="Name">
            <Bindings>
                <ui:DataBinding property="value" binding-mode="TwoWay" data-source-path="Username"/>
            </Bindings>
        </appui:TextField>

        <!-- Read-only binding -->
        <appui:Label>
            <Bindings>
                <ui:DataBinding property="text" binding-mode="ToTarget" data-source-path="FullName"/>
            </Bindings>
        </appui:Label>

        <!-- Boolean binding -->
        <appui:Toggle label="Active">
            <Bindings>
                <ui:DataBinding property="value" binding-mode="TwoWay" data-source-path="IsActive"/>
            </Bindings>
        </appui:Toggle>

        <!-- Command binding -->
        <appui:Button title="Save">
            <Bindings>
                <ui:DataBinding property="clickable.command" binding-mode="ToTarget" data-source-path="SaveCommand"/>
                <ui:DataBinding property="enabledSelf" binding-mode="ToTarget" data-source-path="CanSave"/>
            </Bindings>
        </appui:Button>
    </appui:Panel>
</ui:UXML>
```

### Programmatic Binding
Bind in C# code.

```csharp
using UnityEngine.UIElements;

var label = new Label();
var binding = new DataBinding()
{
    sourceObjectType = typeof(MyViewModel),
    sourcePath = "PropertyName",
    targetProperty = "text"
};
label.SetBinding("text", binding);
```

## ViewModel Lifecycle

### Typical Lifecycle Flow

```
1. Register in AppBuilder
   ↓
2. Request via ServiceProvider
   ↓
3. Constructor Injection (IServiceProvider, interfaces)
   ↓
4. Property [Service] Injection
   ↓
5. IDependencyInjectionListener.OnDependenciesInjected()
   ↓
6. ViewModel Ready - View Binds to Properties/Commands
   ↓
7. User Interactions trigger Commands
   ↓
8. Observable Properties notify View of changes
   ↓
9. View cleanup triggers object disposal
```

### Complete Lifecycle Example

```csharp
[ObservableObject]
public partial class LifecycleViewModel : IDependencyInjectionListener, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    [Service]
    public IDataService DataService { get; set; }

    [Service]
    public IEventAggregator EventAggregator { get; set; }

    [ObservableProperty]
    private ObservableCollection<Item> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    // Step 3: Constructor injection
    public LifecycleViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Debug.Log("ViewModel created");
    }

    // Step 5: Called after property injection
    public void OnDependenciesInjected()
    {
        Debug.Log("Dependencies injected");

        // Subscribe to events
        EventAggregator.Subscribe<DataChangedEvent>(OnDataChanged);

        // Load initial data
        LoadItems();
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        try
        {
            var items = await DataService.GetItemsAsync();
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync(string name)
    {
        var item = new Item { Name = name };
        await DataService.AddItemAsync(item);
        Items.Add(item);
        EventAggregator.Publish(new ItemAddedEvent(item));
    }

    private void LoadItems() => LoadItemsAsyncCommand.Execute(null);

    private void OnDataChanged(DataChangedEvent @event)
    {
        Debug.Log("Data changed, reloading...");
        LoadItems();
    }

    public void Dispose()
    {
        Debug.Log("ViewModel disposed");
        EventAggregator?.Unsubscribe<DataChangedEvent>(OnDataChanged);
    }
}
```

## Code Examples

### Simple Counter ViewModel
```csharp
[ObservableObject]
public partial class CounterViewModel
{
    [ObservableProperty]
    private int _count;

    [RelayCommand]
    private void Increment()
    {
        Count++;
    }

    [RelayCommand]
    private void Decrement()
    {
        Count--;
    }

    [RelayCommand]
    private void Reset()
    {
        Count = 0;
    }
}
```

### Form Validation ViewModel
```csharp
[ObservableObject]
public partial class FormViewModel
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsFormValid))]
    private string _email;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsFormValid))]
    private string _password;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsFormValid))]
    private string _confirmPassword;

    [CreateProperty(ReadOnly = true)]
    public bool IsFormValid =>
        ValidateEmail() &&
        ValidatePassword() &&
        ValidatePasswordMatch();

    [RelayCommand(CanExecute = nameof(IsFormValid))]
    private async Task SubmitAsync()
    {
        Debug.Log("Form submitted!");
        await Task.Delay(1000); // Simulate submission
    }

    private bool ValidateEmail() =>
        !string.IsNullOrEmpty(Email) && Email.Contains("@");

    private bool ValidatePassword() =>
        !string.IsNullOrEmpty(Password) && Password.Length >= 8;

    private bool ValidatePasswordMatch() =>
        Password == ConfirmPassword;
}
```

### List with CRUD Operations
```csharp
[ObservableObject]
public partial class ItemListViewModel : IDependencyInjectionListener
{
    [Service]
    public IItemRepository Repository { get; set; }

    [ObservableProperty]
    private ObservableCollection<ItemViewModel> _items = new();

    [ObservableProperty]
    private ItemViewModel _selectedItem;

    [ObservableProperty]
    private bool _isLoading;

    public void OnDependenciesInjected()
    {
        LoadItemsAsyncCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        IsLoading = true;
        try
        {
            Items.Clear();
            var dbItems = await Repository.GetAllAsync();
            foreach (var item in dbItems)
                Items.Add(new ItemViewModel(item, Repository));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        Items.Add(new ItemViewModel(new Item(), Repository));
    }

    [RelayCommand(CanExecute = nameof(CanDeleteItem))]
    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null) return;
        await Repository.DeleteAsync(SelectedItem.Id);
        Items.Remove(SelectedItem);
        SelectedItem = null;
    }

    private bool CanAddItem => !IsLoading;
    private bool CanDeleteItem => SelectedItem != null && !IsLoading;
}
```
