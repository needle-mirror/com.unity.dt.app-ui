using Unity.AppUI.MVVM;

namespace Unity.AppUI.Samples.MVVMRedux
{
    public class MVVMReduxAppBuilder : UIToolkitAppBuilder<MVVMReduxApp>
    {
        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);
            
            // Services
            builder.services.AddSingleton<ILocalStorageService, LocalStorageService>();
            builder.services.AddSingleton<IStoreService, StoreService>();
            
            // ViewModels
            builder.services.AddTransient<MainViewModel>();
            
            // Views
            builder.services.AddTransient<MainPage>();
        }
    }
}
