using Unity.AppUI.MVVM;
using Unity.Properties;

namespace Unity.AppUI.Samples.MVVMRedux
{
    [ObservableObject]
    public partial class TodoItemViewModel
    {
        public event System.Action<TodoItemViewModel> deleteRequested;

        public event System.Action<(TodoItemViewModel, string)> renamed;

        // When creating properties yourself, you must
        // use CreateProperty attribute for UITK data binding to work.
        [CreateProperty]
        public bool completed
        {
            get => todo.completed;
            set => SetProperty(ref todo.completed, value);
        }

        public Todo todo { get; }

        public TodoItemViewModel(Todo todo)
        {
            this.todo = todo;
        }

        [ICommand]
        void Delete()
        {
            deleteRequested?.Invoke(this);
        }

        [ICommand]
        void Edit(string newName)
        {
             renamed?.Invoke((this, newName));
        }
    }
}
