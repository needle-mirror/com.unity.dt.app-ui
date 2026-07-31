using Unity.AppUI.Navigation;

namespace Unity.AppUI.VisualDoc
{
    class VisualDocPageDestinationTemplate : NavDestinationTemplate
    {
        public override INavigationScreen CreateScreen(NavHost host)
        {
            return new VisualDocPageView();
        }
    }
}
