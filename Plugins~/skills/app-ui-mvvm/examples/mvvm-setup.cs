using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.MVVM;
using Unity.AppUI.UI;
using Unity.Properties;

namespace MyApp.MVVM.Example
{
    // ============================================================================
    // MODELS - Simple data structures
    // ============================================================================

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsComplete { get; set; }
    }

    // ============================================================================
    // SERVICES - Business logic and data access
    // ============================================================================

    public interface IUserService
    {
        Task<User> GetCurrentUserAsync();
        Task UpdateUserAsync(User user);
    }

    public class UserService : IUserService
    {
        private User _currentUser;

        public async Task<User> GetCurrentUserAsync()
        {
            await Task.Delay(500); // Simulate network call
            return _currentUser ?? new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
        }

        public async Task UpdateUserAsync(User user)
        {
            await Task.Delay(300); // Simulate network call
            _currentUser = user;
            Debug.Log($"User updated: {user.Name}");
        }
    }

    public interface ITaskService
    {
        Task<System.Collections.Generic.List<TodoItem>> GetTasksAsync();
        Task AddTaskAsync(TodoItem task);
        Task CompleteTaskAsync(int taskId);
    }

    public class TaskService : ITaskService
    {
        private readonly System.Collections.Generic.List<TodoItem> _tasks = new();

        public TaskService()
        {
            _tasks.Add(new TodoItem { Id = 1, Title = "Learn MVVM", IsComplete = false });
            _tasks.Add(new TodoItem { Id = 2, Title = "Build App UI", IsComplete = false });
        }

        public async Task<System.Collections.Generic.List<TodoItem>> GetTasksAsync()
        {
            await Task.Delay(300); // Simulate network call
            return _tasks;
        }

        public async Task AddTaskAsync(TodoItem task)
        {
            await Task.Delay(200);
            task.Id = _tasks.Count + 1;
            _tasks.Add(task);
            Debug.Log($"Task added: {task.Title}");
        }

        public async Task CompleteTaskAsync(int taskId)
        {
            await Task.Delay(200);
            var task = _tasks.Find(t => t.Id == taskId);
            if (task != null)
                task.IsComplete = true;
            Debug.Log($"Task {taskId} completed");
        }
    }

    // ============================================================================
    // VIEWMODELS - Application logic and state management
    // ============================================================================

    /// <summary>
    /// Profile ViewModel - Manages user profile data and operations
    /// </summary>
    [ObservableObject]
    public partial class ProfileViewModel : IDependencyInjectionListener
    {
        // Constructor injection for required services
        private readonly IUserService _userService;

        // Property injection for optional services
        [Service]
        public ITaskService TaskService { get; set; }

        // Observable properties - automatically notify when changed
        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userEmail;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(IsProfileComplete))]
        private string _bio;

        [ObservableProperty]
        private bool _isSaving;

        [CreateProperty(ReadOnly = true)]
        public bool IsProfileComplete => !string.IsNullOrEmpty(Bio) && Bio.Length > 10;

        public ProfileViewModel(IUserService userService)
        {
            _userService = userService;
            Debug.Log("ProfileViewModel created");
        }

        // Called after all [Service] properties are injected
        public void OnDependenciesInjected()
        {
            Debug.Log("ProfileViewModel dependencies injected");
            LoadProfileCommand.Execute(null);
        }

        // Load user profile data
        [RelayCommand]
        private async Task LoadProfileAsync()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                UserName = user.Name;
                UserEmail = user.Email;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load profile: {ex.Message}");
            }
        }

        // Save user profile with condition check
        [RelayCommand(CanExecute = nameof(CanSaveProfile))]
        private async Task SaveProfileAsync()
        {
            IsSaving = true;
            try
            {
                var user = new User { Name = UserName, Email = UserEmail };
                await _userService.UpdateUserAsync(user);
                Debug.Log("Profile saved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save profile: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        // Condition for save button
        private bool CanSaveProfile => !IsSaving && !string.IsNullOrEmpty(UserName);
    }

    /// <summary>
    /// Tasks ViewModel - Manages task list with CRUD operations
    /// </summary>
    [ObservableObject]
    public partial class TasksViewModel : IDependencyInjectionListener
    {
        [Service]
        public ITaskService TaskService { get; set; }

        [ObservableProperty]
        private UnityEngine.Collections.NativeArray<TaskItemViewModel> _tasks;

        [ObservableProperty]
        private TaskItemViewModel _selectedTask;

        [ObservableProperty]
        private string _newTaskTitle;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(CompletedTaskCount))]
        [AlsoNotifyChangeFor(nameof(RemainingTaskCount))]
        private int _totalTasks;

        [CreateProperty(ReadOnly = true)]
        public int CompletedTaskCount => _tasks.Length;

        [CreateProperty(ReadOnly = true)]
        public int RemainingTaskCount => TotalTasks - CompletedTaskCount;

        public void OnDependenciesInjected()
        {
            Debug.Log("TasksViewModel dependencies injected");
            LoadTasksCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadTasksAsync()
        {
            IsLoading = true;
            try
            {
                var taskList = await TaskService.GetTasksAsync();
                var viewModels = new TaskItemViewModel[taskList.Count];
                for (int i = 0; i < taskList.Count; i++)
                    viewModels[i] = new TaskItemViewModel(taskList[i], TaskService);

                // Note: NativeArray requires careful handling in real scenarios
                TotalTasks = taskList.Count;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load tasks: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanAddTask))]
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            IsLoading = true;
            try
            {
                var task = new TodoItem { Title = NewTaskTitle };
                await TaskService.AddTaskAsync(task);
                NewTaskTitle = "";
                await LoadTasksAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCompleteTask))]
        private async Task CompleteTaskAsync()
        {
            if (SelectedTask == null)
                return;

            await TaskService.CompleteTaskAsync(SelectedTask.Model.Id);
            SelectedTask.IsComplete = true;
        }

        private bool CanAddTask => !IsLoading && !string.IsNullOrWhiteSpace(NewTaskTitle);
        private bool CanCompleteTask => SelectedTask != null && !SelectedTask.IsComplete && !IsLoading;
    }

    /// <summary>
    /// Individual task item wrapper - Wraps a task with ViewModel functionality
    /// </summary>
    public class TaskItemViewModel
    {
        public TodoItem Model { get; }
        public ITaskService TaskService { get; }

        [ObservableProperty]
        private bool _isComplete;

        public TaskItemViewModel(TodoItem task, ITaskService taskService)
        {
            Model = task;
            TaskService = taskService;
            _isComplete = task.IsComplete;
        }
    }

    // ============================================================================
    // VIEWS - UI Layer (minimal code-behind)
    // ============================================================================

    /// <summary>
    /// Profile view - Displays and edits user profile
    /// </summary>
    public class ProfileView : VisualElement
    {
        public ProfileView(ProfileViewModel viewModel)
        {
            var container = new Box();
            Add(container);

            // User Name Input
            var nameField = new TextField("Name:");
            nameField.value = viewModel.UserName;
            nameField.RegisterValueChangedCallback(evt =>
            {
                viewModel.UserName = evt.newValue;
            });
            container.Add(nameField);

            // User Email Display
            var emailLabel = new Label($"Email: {viewModel.UserEmail}");
            container.Add(emailLabel);

            // Bio Input
            var bioField = new TextField("Bio:");
            bioField.multiline = true;
            bioField.value = viewModel.Bio ?? "";
            bioField.RegisterValueChangedCallback(evt =>
            {
                viewModel.Bio = evt.newValue;
            });
            container.Add(bioField);

            // Save Button
            var saveButton = new Button { text = "Save Profile" };
            saveButton.clicked += () => viewModel.SaveProfileCommand.Execute(null);
            container.Add(saveButton);

            // Update UI when ViewModel properties change
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(viewModel.UserName))
                    emailLabel.text = $"Email: {viewModel.UserEmail}";
            };
        }
    }

    /// <summary>
    /// Tasks view - Displays and manages task list
    /// </summary>
    public class TasksView : VisualElement
    {
        public TasksView(TasksViewModel viewModel)
        {
            var container = new Box();
            Add(container);

            // New task input
            var newTaskField = new TextField("New Task:");
            newTaskField.RegisterValueChangedCallback(evt =>
            {
                viewModel.NewTaskTitle = evt.newValue;
            });
            container.Add(newTaskField);

            // Add task button
            var addButton = new Button { text = "Add Task" };
            addButton.clicked += () => viewModel.AddTaskCommand.Execute(null);
            container.Add(addButton);

            // Task list
            var taskList = new ListView();
            container.Add(taskList);

            // Complete button
            var completeButton = new Button { text = "Complete Task" };
            completeButton.clicked += () => viewModel.CompleteTaskCommand.Execute(null);
            container.Add(completeButton);

            // Stats
            var statsLabel = new Label();
            container.Add(statsLabel);

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(viewModel.CompletedTaskCount))
                    statsLabel.text = $"Tasks: {viewModel.CompletedTaskCount} completed, " +
                                     $"{viewModel.RemainingTaskCount} remaining";
            };
        }
    }

    // ============================================================================
    // APP - Application entry point
    // ============================================================================

    /// <summary>
    /// Main application class
    /// </summary>
    public class MainApp : App
    {
        public new static MainApp current => (MainApp)App.current;

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            // Get services from DI container
            var profileViewModel = services.GetRequiredService<ProfileViewModel>();
            var tasksViewModel = services.GetRequiredService<TasksViewModel>();

            // Create and add views
            rootVisualElement.Add(new ProfileView(profileViewModel));
            rootVisualElement.Add(new TasksView(tasksViewModel));

            Debug.Log("MainApp initialized");
        }

        public override void Shutdown()
        {
            Debug.Log("MainApp shutting down");
        }
    }

    // ============================================================================
    // APP BUILDER - Dependency Injection Configuration
    // ============================================================================

    /// <summary>
    /// Configures the application and registers services
    /// </summary>
    public class MainAppBuilder : UIToolkitAppBuilder<MainApp>
    {
        protected override void OnConfiguringApp(AppBuilder appBuilder)
        {
            base.OnConfiguringApp(appBuilder);
            Debug.Log("Configuring application services");

            // Register singleton services (one instance for the app lifetime)
            appBuilder.services.AddSingleton<IUserService, UserService>();
            appBuilder.services.AddSingleton<ITaskService, TaskService>();

            // Register transient ViewModels (new instance each time)
            appBuilder.services.AddTransient<ProfileViewModel>();
            appBuilder.services.AddTransient<TasksViewModel>();

            // Register transient Views (new instance each time)
            appBuilder.services.AddTransient<ProfileView>();
            appBuilder.services.AddTransient<TasksView>();
        }

        protected override void OnAppInitialized(MainApp app)
        {
            base.OnAppInitialized(app);
            Debug.Log("Application initialized successfully!");
        }

        protected override void OnAppShuttingDown(MainApp app)
        {
            base.OnAppShuttingDown(app);
            Debug.Log("Application shutting down");
        }
    }

    // ============================================================================
    // USAGE EXAMPLE
    // ============================================================================

    /*
    To use this MVVM setup:

    1. Attach MainAppBuilder to a UIDocument GameObject
    2. MainAppBuilder will:
       - Configure all services in OnConfiguringApp
       - Create MainApp and initialize it
       - Call InitializeComponent to build the UI

    3. The application flow:
       - Services are created and registered
       - ViewModels are instantiated with their dependencies
       - Views are created and bound to ViewModels
       - User interactions trigger Commands on ViewModels
       - Observable properties notify Views to update

    4. Key patterns:
       - Constructor injection for required dependencies
       - [Service] attribute for optional properties
       - [ObservableProperty] for automatic change notifications
       - [RelayCommand] for user action handling
       - IDependencyInjectionListener for initialization after injection
    */
}
