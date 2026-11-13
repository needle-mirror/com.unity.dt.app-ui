using System;
using Unity.AppUI.MVVM;
using UnityEngine;

namespace Unity.AppUI.Navigation
{
    /// <summary>
    /// A destination template that resolves <see cref="NavigationScreen"/> instances through
    /// the App UI dependency injection container, enabling both constructor injection and
    /// <c>[Service]</c> property/field injection on screen classes.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="DefaultDestinationTemplate"/> which uses <c>Activator.CreateInstance()</c>,
    /// this template calls <c>App.current.services.GetService()</c> so that the full DI pipeline
    /// (constructor parameters, <c>[Service]</c> attributes, <c>IDependencyInjectionListener</c>)
    /// is executed when a screen is created.
    ///
    /// Screen types must be registered in the service collection via the <c>AppBuilder</c>.
    /// </remarks>
    [Serializable]
    public class DIDestinationTemplate : NavDestinationTemplate
    {
        [SerializeField]
        [Tooltip("Assembly Qualified Name of the NavigationScreen type to instantiate.")]
        string m_ScreenType;

        [SerializeField]
        bool m_ShowBottomNavBar = true;

        [SerializeField]
        bool m_ShowAppBar = true;

        [SerializeField]
        bool m_ShowBackButton = true;

        [SerializeField]
        bool m_ShowDrawer = true;

        [SerializeField]
        bool m_ShowNavigationRail;

        /// <summary>
        /// The Assembly Qualified Name of the <see cref="NavigationScreen"/> type.
        /// </summary>
        public string screenType
        {
            get => m_ScreenType;
            set => m_ScreenType = value;
        }

        /// <summary>
        /// Whether the bottom navigation bar should be visible for this destination.
        /// </summary>
        public bool showBottomNavBar
        {
            get => m_ShowBottomNavBar;
            set => m_ShowBottomNavBar = value;
        }

        /// <summary>
        /// Whether the app bar should be visible for this destination.
        /// </summary>
        public bool showAppBar
        {
            get => m_ShowAppBar;
            set => m_ShowAppBar = value;
        }

        /// <summary>
        /// Whether the back button should be visible for this destination.
        /// </summary>
        public bool showBackButton
        {
            get => m_ShowBackButton;
            set => m_ShowBackButton = value;
        }

        /// <summary>
        /// Whether the drawer should be visible for this destination.
        /// </summary>
        public bool showDrawer
        {
            get => m_ShowDrawer;
            set => m_ShowDrawer = value;
        }

        /// <summary>
        /// Whether the navigation rail should be visible for this destination.
        /// </summary>
        public bool showNavigationRail
        {
            get => m_ShowNavigationRail;
            set => m_ShowNavigationRail = value;
        }

        /// <inheritdoc/>
        public override INavigationScreen CreateScreen(NavHost host)
        {
            var type = Type.GetType(m_ScreenType);
            if (type == null)
            {
                Debug.LogError($"[DIDestinationTemplate] Could not resolve type '{m_ScreenType}'. " +
                    "Verify the Assembly Qualified Name and that the type is registered in your AppBuilder.");
                return new NavigationScreen();
            }

            // Resolve through the DI container.
            // The ServiceProvider will:
            //  1. Pick the best constructor and resolve its parameters
            //  2. Set any properties/fields annotated with [Service]
            //  3. Call IDependencyInjectionListener.OnDependenciesInjected() if implemented
            var screen = App.current.services.GetService(type) as NavigationScreen;

            if (screen == null)
            {
                Debug.LogError($"[DIDestinationTemplate] ServiceProvider returned null for '{type.FullName}'. " +
                    "Make sure the type is registered in your AppBuilder (e.g. services.AddTransient<T>()).");
                return new NavigationScreen();
            }

            screen.showBottomNavBar = m_ShowBottomNavBar;
            screen.showAppBar = m_ShowAppBar;
            screen.showBackButton = m_ShowBackButton;
            screen.showDrawer = m_ShowDrawer;
            screen.showNavigationRail = m_ShowNavigationRail;

            return screen;
        }
    }
}
