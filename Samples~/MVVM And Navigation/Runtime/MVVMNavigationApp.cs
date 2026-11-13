using Unity.AppUI.MVVM;
using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    public class MVVMNavigationApp : App
    {
        public new static MVVMNavigationApp current => (MVVMNavigationApp)App.current;

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            var navGraphAsset = MVVMNavigationAppBuilder.instance.navGraphAsset;

            var navHost = new NavHost();
            navHost.visualController = new SampleVisualController();
            navHost.navController.SetGraph(navGraphAsset);

            rootVisualElement.Add(navHost);
            navHost.StretchToParentSize();
        }
    }
}
