using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.AppUI.Navigation
{
    /// <summary>
    /// The NavHost is the UI element that manages the navigation stack.
    /// It will manage the display of <see cref="NavigationScreen"/> objects through its <see cref="NavController"/>.
    /// </summary>
    public class NavHost : VisualElement
    {
        /// <summary>
        /// The NavHost main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-navhost";
        
        /// <summary>
        /// The NavHost container styling class.
        /// </summary>
        public static readonly string containerUssClassName = ussClassName + "__container";
        
        /// <summary>
        /// The NavHost item styling class.
        /// </summary>
        public static readonly string itemUssClassName = ussClassName + "__item";

        /// <summary>
        /// The controller that manages the navigation stack.
        /// </summary>
        public NavController navController { get; }

        /// <summary>
        /// The visual controller that will be used to handle modification of Navigation visual elements, such as BottomNavBar.
        /// </summary>
        public INavVisualController visualController { get; set; }

        readonly VisualElement m_Container;
        
        ValueAnimation<float> m_RemoveAnim;
        
        ValueAnimation<float> m_AddAnim;
        
        /// <summary>
        /// The container that will hold the current <see cref="NavigationScreen"/>.
        /// </summary>
        public override VisualElement contentContainer => m_Container.contentContainer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NavHost()
        {
            AddToClassList(ussClassName);
            
            focusable = true;
            pickingMode = PickingMode.Position;
            navController = new NavController(this);
            m_Container = new StackView();
            m_Container.AddToClassList(containerUssClassName);
            m_Container.StretchToParentSize();
            hierarchy.Add(m_Container);
            
            RegisterCallback<NavigationCancelEvent>(OnCancelNavigation);
        }

        void OnCancelNavigation(NavigationCancelEvent evt)
        {
            if (navController.canGoBack)
            {
                evt.StopPropagation();
                navController.PopBackStack();
            }
        }

        /// <summary>
        /// Switch to a new <see cref="NavDestination"/> using the provided <see cref="NavigationAnimation"/>.
        /// </summary>
        /// <param name="destination"> The destination to switch to. </param>
        /// <param name="exitAnim"> The animation to use when exiting the current destination. </param>
        /// <param name="enterAnim"> The animation to use when entering the new destination. </param>
        /// <param name="args"> The arguments to pass to the new destination. </param>
        /// <param name="isPop"> Whether the navigation is a pop operation. </param>
        /// <param name="callback"> A callback that will be called when the navigation is complete. </param>
        internal void SwitchTo(
            NavDestination destination, 
            NavigationAnimation exitAnim, 
            NavigationAnimation enterAnim, 
            Argument[] args, 
            bool isPop,
            Action<bool> callback = null)
        {
            var content = destination.template;
            if (content != null)
            {
                m_RemoveAnim?.Recycle();
                m_AddAnim?.Recycle();

                var item = CreateItem(destination, content);
                if (m_Container.childCount == 0)
                {
                    m_Container.Add(item);
                }
                else
                {
                    var exitAnimationFunc = GetAnimationFunc(exitAnim);
                    var enterAnimationFunc = GetAnimationFunc(enterAnim);
                    if (enterAnimationFunc.durationMs > 0 && exitAnimationFunc.durationMs == 0)
                        exitAnimationFunc.durationMs = enterAnimationFunc.durationMs;
                    var previousItem = m_Container[0];
                    (previousItem.ElementAt(0) as NavigationScreen)!.InvokeOnExit(navController, destination, args);
                    m_RemoveAnim = previousItem.experimental.animation.Start(0, 1, 
                            exitAnimationFunc.durationMs, 
                            exitAnimationFunc.callback)
                        .Ease(exitAnimationFunc.easing)
                        .OnCompleted(() => m_Container.Remove(previousItem))
                        .KeepAlive();
                    if (isPop)
                        m_Container.Insert(0, item);
                    else
                        m_Container.Add(item);
                    m_AddAnim = item.experimental.animation.Start(0, 1, 
                            enterAnimationFunc.durationMs, 
                            enterAnimationFunc.callback)
                        .Ease(enterAnimationFunc.easing)
                        .KeepAlive();
                }
                if (item.Q<NavigationScreen>() is { } screen)
                    screen.InvokeOnEnter(navController, destination, args);
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"The template for navigation " +
                    $"destination {destination.name} is not valid.");
                callback?.Invoke(false);
            }
        }

        VisualElement CreateItem(NavDestination destination, string template)
        {
            var item = new VisualElement { name = itemUssClassName, pickingMode = PickingMode.Ignore };
            item.AddToClassList(itemUssClassName);

            var screen = (NavigationScreen) Activator.CreateInstance(Type.GetType(template)!);
            item.Add(screen);

            if (destination.showBottomNavBar)
            {
                var bottomNavBar = new BottomNavBar();
                visualController?.SetupBottomNavBar(bottomNavBar, destination, navController);
                item.Add(bottomNavBar);
            }
            
            AppBar appBar = null;
            if (destination.showAppBar)
            {
                appBar = new AppBar();
                visualController?.SetupAppBar(appBar, destination, navController);
                item.Add(appBar);

                if (appBar.stretch)
                {
                    screen.scrollView.verticalScroller.valueChanged += (f) =>
                    {
                        appBar.scrollOffset = f;
                    };
                    appBar.RegisterCallback<GeometryChangedEvent>(evt =>
                    {
                        var h = evt.newRect.height;
                        screen.style.marginTop = h;
                    });
                }
                appBar.scrollOffset = screen.scrollView.verticalScroller.value;
                screen.AddToClassList(NavigationScreen.withAppBarUssClassName);
                screen.EnableInClassList(NavigationScreen.withCompactAppBarUssClassName, appBar.compact);

                if (destination.showBackButton && navController.canGoBack)
                {
                    appBar.backButtonTriggered += () => navController.PopBackStack();
                    appBar.showBackButton = true;
                }
            }

            if (destination.showDrawer)
            {
                var drawer = new Drawer();
                visualController?.SetupDrawer(drawer, destination, navController);
                item.Add(drawer);
                
                if (destination.showAppBar && !navController.canGoBack)
                {
                    appBar.showDrawerButton = true;
                    appBar.drawerButtonTriggered += () => drawer.Toggle();
                }
            }
            
            return item;
        }

        /// <summary>
        /// No animation.
        /// </summary>
        internal static readonly AnimationDescription noneAnimation = new AnimationDescription
        {
            easing = Easing.Linear,
            durationMs = 0,
            callback = null
        };
        
        /// <summary>
        /// Scale down and fade in animation.
        /// </summary>
        internal static readonly AnimationDescription scaleDownFadeInAnimation = new AnimationDescription
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                // Scale from 1.2 to 1.0
                var delta = 1.2f - (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = f;
            }
        };
        
        /// <summary>
        /// Scale up and fade out animation.
        /// </summary>
        internal static readonly AnimationDescription scaleUpFadeOutAnimation = new AnimationDescription
        {
            easing = Easing.OutCubic,
            durationMs = 150,
            callback = (v, f) =>
            {
                // Scale from 1.0 to 1.2
                var delta = 1.0f + (f * 0.2f);
                v.style.scale = new Scale(new Vector3(delta, delta, 1));
                v.style.opacity = 1.0f - f;
            }
        };
        
        /// <summary>
        /// Fade in animation.
        /// </summary>
        internal static readonly AnimationDescription fadeInAnimation = new AnimationDescription
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) =>
            {
                // Opacity from 0.0 to 1.0
                v.style.opacity = f;
            }
        };
        
        /// <summary>
        /// Fade out animation.
        /// </summary>
        internal static readonly AnimationDescription fadeOutAnimation = new AnimationDescription
        {
            easing = Easing.OutCubic,
            durationMs = 500,
            callback = (v, f) =>
            {
                // Opacity from 1.0 to 0.0
                v.style.opacity = 1.0f - f;
            }
        };

        /// <summary>
        /// Get the <see cref="AnimationDescription"/> for the provided <see cref="NavigationAnimation"/>.
        /// </summary>
        /// <param name="anim"> The <see cref="NavigationAnimation"/> to get the <see cref="AnimationDescription"/> for. </param>
        /// <returns> The <see cref="AnimationDescription"/> for the provided <see cref="NavigationAnimation"/>. </returns>
        internal static AnimationDescription GetAnimationFunc(NavigationAnimation anim)
        {
            switch (anim)
            {
                case NavigationAnimation.ScaleDownFadeIn:
                    return scaleDownFadeInAnimation;
                case NavigationAnimation.ScaleUpFadeOut:
                    return scaleUpFadeOutAnimation;
                case NavigationAnimation.FadeIn:
                    return fadeInAnimation;
                case NavigationAnimation.FadeOut:
                    return fadeOutAnimation;
                case NavigationAnimation.None:
                default:
                    return noneAnimation;
            }
        }

        /// <summary>
        /// The UXML Factory for the <see cref="NavHost"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<NavHost, UxmlTraits> { }

        /// <summary>
        /// Class containing the UXML traits for the <see cref="NavHost"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            
        }
    }
}
