using System;
using Unity.AppUI.MVVM;
using Unity.Properties;

namespace Unity.AppUI.Samples.MVVM
{
    [ObservableObject]
    public partial class MainViewModel
    {
        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(ClickCountMessage))]
        int m_Counter;

        public MainViewModel()
        {
            Counter = 0;
        }

        [ICommand]
        void IncrementCounter()
        {
            Counter++;
        }

        // When creating properties yourself, you must
        // use CreateProperty attribute for UITK data binding to work.
        [CreateProperty(ReadOnly = true)]
        public string ClickCountMessage => $"Click count: {Counter}";
    }
}
