using Unity.AppUI.MVVM;
using Unity.AppUI.Navigation;
using UnityEngine;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    public class MVVMNavigationAppBuilder : UIToolkitAppBuilder<MVVMNavigationApp>
    {
        [SerializeField]
        NavGraphViewAsset m_NavGraphAsset;

        public NavGraphViewAsset navGraphAsset => m_NavGraphAsset;

        internal static MVVMNavigationAppBuilder instance { get; private set; }

        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);

            instance = this;

            // Services
            builder.services.AddSingleton<IItemService, ItemService>();

            // ViewModels
            builder.services.AddTransient<HomeScreenViewModel>();
            builder.services.AddTransient<DetailScreenViewModel>();

            // NavigationScreens — registered as Transient so the NavHost gets
            // a fresh, fully-injected instance every time a destination is reached.
            builder.services.AddTransient<HomeScreen>();
            builder.services.AddTransient<DetailScreen>();
        }
    }
}
