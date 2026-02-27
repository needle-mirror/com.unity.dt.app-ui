// MVVM Example - Complete App UI Application Structure

using Unity.AppUI.MVVM;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

// ============================================================================
// 1. APP BUILDER - Entry point and dependency injection configuration
// ============================================================================

public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);

        // Register pages (Transient = new instance each time)
        builder.services.AddTransient<MainPage>();
        builder.services.AddTransient<SettingsPage>();

        // Register ViewModels
        builder.services.AddTransient<MainViewModel>();
        builder.services.AddTransient<SettingsViewModel>();

        // Register services (Singleton = one instance for app lifetime)
        builder.services.AddSingleton<IUserService, UserService>();
    }

    protected override void OnAppInitialized(MyApp app)
    {
        // Called after app is initialized
    }

    protected override void OnAppShuttingDown(MyApp app)
    {
        // Called before app shuts down
    }
}

// ============================================================================
// 2. APP CLASS - Main application class
// ============================================================================

public class MyApp : App
{
    public new static MyApp current => (MyApp)App.current;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        // Add the main page to the root
        rootVisualElement.Add(services.GetRequiredService<MainPage>());
    }

    public override void Shutdown()
    {
        // Cleanup logic
    }
}

// ============================================================================
// 3. VIEWMODEL - Business logic with observable properties
// ============================================================================

[ObservableObject]
public partial class MainViewModel
{
    private readonly IServiceProvider m_ServiceProvider;
    private readonly IUserService m_UserService;

    // Observable properties - UI automatically updates when these change
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasName))]
    private string _userName;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage;

    // Computed property
    public bool HasName => !string.IsNullOrEmpty(UserName);

    public MainViewModel(IServiceProvider serviceProvider, IUserService userService)
    {
        m_ServiceProvider = serviceProvider;
        m_UserService = userService;
    }

    // Commands - auto-generated as SaveCommand
    [RelayCommand]
    async Task SaveAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await m_UserService.SaveUserAsync(UserName);
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

    // Command with CanExecute - auto-checks if command can run
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    void Submit()
    {
        // Submit logic
    }

    private bool CanSubmit() => HasName && !IsLoading;

    // Navigation command
    [RelayCommand]
    void NavigateToSettings()
    {
        var settingsPage = m_ServiceProvider.GetRequiredService<SettingsPage>();
        var app = MyApp.current;
        app.rootVisualElement.Clear();
        app.rootVisualElement.Add(settingsPage);
    }
}

// ============================================================================
// 4. VIEW (PAGE) - Visual representation
// ============================================================================

public class MainPage : VisualElement
{
    private readonly MainViewModel m_ViewModel;

    public MainPage(MainViewModel viewModel)
    {
        m_ViewModel = viewModel;

        // Create UI
        var container = new VisualElement();
        container.style.padding = new StyleLength(16);

        // Text field bound to UserName
        var nameField = new TextField { label = "Name" };
        nameField.RegisterValueChangedCallback(evt => m_ViewModel.UserName = evt.newValue);
        container.Add(nameField);

        // Save button bound to SaveCommand
        var saveButton = new Button { title = "Save" };
        saveButton.clickable.clicked += async () => await m_ViewModel.SaveAsyncCommand.ExecuteAsync(null);
        container.Add(saveButton);

        // Settings button
        var settingsButton = new Button { title = "Settings" };
        settingsButton.clickable.clicked += () => m_ViewModel.NavigateToSettingsCommand.Execute(null);
        container.Add(settingsButton);

        // Error message
        var errorLabel = new Text();
        errorLabel.style.color = new StyleColor(UnityEngine.Color.red);
        container.Add(errorLabel);

        // Bind to ViewModel changes
        m_ViewModel.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(MainViewModel.UserName):
                    if (nameField.value != m_ViewModel.UserName)
                        nameField.SetValueWithoutNotify(m_ViewModel.UserName);
                    break;
                case nameof(MainViewModel.IsLoading):
                    saveButton.SetEnabled(!m_ViewModel.IsLoading);
                    break;
                case nameof(MainViewModel.ErrorMessage):
                    errorLabel.text = m_ViewModel.ErrorMessage ?? "";
                    break;
            }
        };

        Add(container);
    }
}

// ============================================================================
// 5. SERVICE INTERFACE AND IMPLEMENTATION
// ============================================================================

public interface IUserService
{
    Task SaveUserAsync(string userName);
}

public class UserService : IUserService
{
    public async Task SaveUserAsync(string userName)
    {
        // Simulate async operation
        await Task.Delay(1000);
        UnityEngine.Debug.Log($"User saved: {userName}");
    }
}
