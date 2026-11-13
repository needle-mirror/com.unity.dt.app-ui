using System.ComponentModel;
using Unity.AppUI.MVVM;
using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// Home screen that displays a list of items.
    /// <para>
    /// Demonstrates <c>[Service]</c> property injection on a <see cref="NavigationScreen"/>.
    /// The <see cref="ViewModel"/> property is set by the DI container after construction,
    /// so all binding logic lives in <see cref="OnDependenciesInjected"/>.
    /// </para>
    /// </summary>
    [Preserve]
    public class HomeScreen : NavigationScreen, IDependencyInjectionListener
    {
        // Property injection: the DI container sets this AFTER construction.
        [Service]
        public HomeScreenViewModel ViewModel { get; private set; }

        readonly VisualElement m_ListContainer;

        public HomeScreen()
        {
            // Build the UI skeleton.  ViewModel is NOT available yet.
            m_ListContainer = new VisualElement();
            m_ListContainer.AddToClassList("mvvm-nav-list");
            Add(m_ListContainer);
        }

        /// <summary>
        /// Called by the DI container once all <c>[Service]</c> properties have been injected.
        /// This is the earliest point where <see cref="ViewModel"/> is usable.
        /// </summary>
        public void OnDependenciesInjected()
        {
            BuildItemList();

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HomeScreenViewModel.Items))
                BuildItemList();
        }

        void BuildItemList()
        {
            m_ListContainer.Clear();

            if (ViewModel.Items == null)
                return;

            foreach (var item in ViewModel.Items)
            {
                var row = new VisualElement();
                row.AddToClassList("mvvm-nav-item-row");

                var title = new Text(item.title) { size = TextSize.L };
                title.AddToClassList("mvvm-nav-item-title");

                var description = new Text(item.description) { size = TextSize.S };
                description.AddToClassList("mvvm-nav-item-description");

                row.Add(title);
                row.Add(description);

                // Capture for the closure.
                var capturedId = item.id;
                row.AddManipulator(new Pressable(() =>
                {
                    this.FindNavController().Navigate(
                        Actions.go_to_detail,
                        new Argument("itemId", capturedId));
                }));

                m_ListContainer.Add(row);
            }
        }

        protected override void SetupAppBar(AppBar appBar, NavController navController)
        {
            appBar.title = "Home";
        }
    }
}
