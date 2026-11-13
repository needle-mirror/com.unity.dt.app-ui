using System.ComponentModel;
using System.Linq;
using Unity.AppUI.MVVM;
using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// Detail screen that displays a single item.
    /// <para>
    /// Demonstrates <c>[Service]</c> property injection combined with navigation arguments.
    /// The <see cref="ViewModel"/> is injected by the DI container, then
    /// <see cref="OnEnter"/> passes the navigation argument (<c>itemId</c>) to the ViewModel.
    /// </para>
    /// </summary>
    [Preserve]
    public class DetailScreen : NavigationScreen, IDependencyInjectionListener
    {
        // Property injection: set by the DI container after construction.
        [Service]
        public DetailScreenViewModel ViewModel { get; private set; }

        readonly Text m_TitleLabel;
        readonly Text m_DescriptionLabel;

        public DetailScreen()
        {
            // Build the UI skeleton.  ViewModel is NOT available yet.
            var content = new VisualElement();
            content.AddToClassList("mvvm-nav-detail-content");

            m_TitleLabel = new Text { size = TextSize.XL };
            m_TitleLabel.AddToClassList("mvvm-nav-detail-title");

            m_DescriptionLabel = new Text { size = TextSize.M };
            m_DescriptionLabel.AddToClassList("mvvm-nav-detail-description");

            content.Add(m_TitleLabel);
            content.Add(m_DescriptionLabel);
            Add(content);
        }

        /// <summary>
        /// Called by the DI container once all <c>[Service]</c> properties have been injected.
        /// </summary>
        public void OnDependenciesInjected()
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        /// <summary>
        /// Called when the screen is navigated to. Receives the <c>itemId</c> argument
        /// and forwards it to the ViewModel.
        /// </summary>
        public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
        {
            base.OnEnter(controller, destination, args);

            var itemIdArg = args?.FirstOrDefault(a => a.name == "itemId");
            if (itemIdArg != null)
            {
                ViewModel.LoadItem(itemIdArg.value);
            }

            RefreshUI();
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshUI();
        }

        void RefreshUI()
        {
            m_TitleLabel.text = ViewModel.Title ?? string.Empty;
            m_DescriptionLabel.text = ViewModel.Description ?? string.Empty;
        }

        protected override void SetupAppBar(AppBar appBar, NavController navController)
        {
            appBar.title = ViewModel?.Title ?? "Detail";
        }
    }
}
