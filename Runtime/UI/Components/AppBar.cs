using System;
using System.Collections;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A customizable top app bar that provides content and actions related to the current screen.
    /// </summary>
    /// <remarks>
    /// The AppBar is a flexible navigation component that displays information and actions relating to the current
    /// screen. It provides consistent navigation and helps users identify where they are in the app.
    ///
    /// The AppBar supports both fixed and collapsible (stretching) behaviors, making it suitable for various
    /// navigation patterns. It can display a title, navigation buttons (back and drawer), and custom actions.
    ///
    /// Key features include:
    /// - Dynamic height adjustment with stretching behavior
    /// - Support for back and drawer navigation buttons
    /// - Customizable action buttons
    /// - Compact and regular size modes
    /// - Flexible space for custom content
    /// - Configurable elevation levels
    ///
    /// The AppBar is commonly used in conjunction with other navigation components like Drawer and NavigationRail
    /// to create a cohesive navigation experience.
    /// </remarks>
    /// <example>
    /// <para>Basic AppBar with title and actions. Creating a basic AppBar with a title and action buttons.</para>
    /// <code lang="xml"><![CDATA[
    /// <AppBar title="My App">
    ///     <ActionButton icon="settings" />
    ///     <ActionButton icon="account" />
    /// </AppBar>
    /// ]]></code>
    /// <para>Collapsible AppBar with navigation. Setting up a collapsible AppBar with navigation and actions.</para>
    /// <code lang="xml"><![CDATA[
    /// <AppBar
    ///     stretch="true"
    ///     expanded-height="150"
    ///     show-back-button="true"
    ///     elevation="1"
    ///     title="Details Page">
    ///     <ActionButton icon="share" />
    ///     <ActionButton icon="more" />
    /// </AppBar>
    /// ]]></code>
    /// <para>Compact AppBar with drawer. Creating a compact AppBar with drawer button and search action programmatically.</para>
    /// <code lang="csharp"><![CDATA[
    /// var appBar = new AppBar();
    /// appBar.compact = true;
    /// appBar.showDrawerButton = true;
    /// appBar.title = "Dashboard";
    ///
    /// // Add a search action
    /// var searchButton = new ActionButton { icon = "search" };
    /// appBar.AddAction(searchButton);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("nav-components")]
    public partial class AppBar : ExVisualElement
    {
        internal static readonly BindingId stretchProperty = nameof(stretch);

        internal static readonly BindingId compactProperty = nameof(compact);

        internal static readonly BindingId expandedHeightProperty = nameof(expandedHeight);

        internal static readonly BindingId elevationProperty = nameof(elevation);

        internal static readonly BindingId showBackButtonProperty = nameof(showBackButton);

        internal static readonly BindingId showDrawerButtonProperty = nameof(showDrawerButton);
        /// <summary>
        /// Main USS class name of the AppBar.
        /// </summary>
        public const string ussClassName = "appui-appbar";

        /// <summary>
        /// USS class name of the AppBar's elevation.
        /// </summary>
        public const string elevationUssClassName = Styles.elevationUssClassName;

        /// <summary>
        /// USS class name of the AppBar's bar.
        /// </summary>
        public const string barUssClassName = ussClassName + "__bar";

        /// <summary>
        /// USS class name of the AppBar's bottom element.
        /// </summary>
        public const string bottomUssClassName = ussClassName + "__bottom";

        /// <summary>
        /// USS class name of the AppBar's bottom border element.
        /// </summary>
        public const string bottomBorderUssClassName = ussClassName + "__bottom-border";

        /// <summary>
        /// USS class name of the AppBar's compact title.
        /// </summary>
        public const string compactTitleUssClassName = ussClassName + "__compact-title";

        /// <summary>
        /// USS class name of the AppBar's large title.
        /// </summary>
        public const string largeTitleUssClassName = ussClassName + "__large-title";

        /// <summary>
        /// USS class name of the AppBar's action container.
        /// </summary>
        public const string actionContainerUssClassName = ussClassName + "__action-container";

        /// <summary>
        /// USS class name of the AppBar's back button.
        /// </summary>
        public const string backButtonUssClassName = ussClassName + "__back-button";

        /// <summary>
        /// USS class name of the AppBar's drawer button.
        /// </summary>
        public const string drawerButtonUssClassName = ussClassName + "__drawer-button";

        /// <summary>
        /// USS class name of the AppBar's bar spacer.
        /// </summary>
        public const string spacerUssClassName = ussClassName + "__spacer";

        /// <summary>
        /// USS class name of the AppBar's flexible space.
        /// </summary>
        public const string flexibleSpaceUssClassName = ussClassName + "__flexible-space";

        /// <summary>
        /// USS class name of the AppBar's stretch variant.
        /// </summary>
        public const string stretchUssClassName = ussClassName + "--stretch";

        /// <summary>
        /// USS class name of the AppBar's compact variant.
        /// </summary>
        public const string compactUssClassName = ussClassName + "--compact";

        /// <summary>
        /// Event triggered when the AppBar is being stretched. The float parameter is the stretch ratio (0.0 to 1.0).
        /// </summary>
        public event Action<float> stretchTriggered;

        /// <summary>
        /// Event triggered when the AppBar's back button is pressed.
        /// </summary>
        public event Action backButtonTriggered;

        /// <summary>
        /// Event triggered when the AppBar's drawer button is pressed.
        /// </summary>
        public event Action drawerButtonTriggered;

        readonly ActionButton m_BackButton;

        readonly ActionButton m_DrawerButton;

        readonly LocalizedTextElement m_CompactTitle;

        readonly LocalizedTextElement m_LargeTitle;

        readonly VisualElement m_ActionContainer;

        float m_ExpandedHeight;

        VisualElement m_BottomElement;

        VisualElement m_BottomBorderElement;

        int m_Elevation;

        readonly VisualElement m_FlexibleSpace;

        float m_CurrentScrollHeight;

        /// <summary>
        /// Add a new action to the AppBar.
        /// </summary>
        /// <param name="actionVisualElement">The action to add.</param>
        public void AddAction(VisualElement actionVisualElement)
        {
            if (actionVisualElement is ActionButton button)
                button.size = compact ? Size.S : Size.M;
            m_ActionContainer.Add(actionVisualElement);
        }

        /// <summary>
        /// Container element for the AppBar's bottom element.
        /// This element is located below the flexible space.
        /// </summary>
        public VisualElement bottom => m_BottomElement;

        /// <summary>
        /// Display or hide the AppBar's back button.
        /// </summary>
        public bool showBackButton
        {
            get => !m_BackButton.ClassListContains(Styles.hiddenUssClassName);
            set => m_BackButton.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// Display or hide the AppBar's drawer button.
        /// </summary>
        public bool showDrawerButton
        {
            get => !m_DrawerButton.ClassListContains(Styles.hiddenUssClassName);
            set => m_DrawerButton.EnableInClassList(Styles.hiddenUssClassName, !value);
        }

        /// <summary>
        /// The AppBar's title.
        /// </summary>
        public string title
        {
            get => m_CompactTitle.text;
            set
            {
                m_CompactTitle.text = value;
                m_LargeTitle.text = value;
            }
        }

        /// <summary>
        /// The stretch of the AppBar.
        /// Set to true to make it stretchable. Set to false to make it fixed.
        /// </summary>
        public bool stretch
        {
            get => ClassListContains(stretchUssClassName);
            set
            {
                EnableInClassList(stretchUssClassName, value);
                RefreshFromScrollChanges();
            }
        }

        /// <summary>
        /// The compactness of the AppBar.
        /// Set to true to make it compact. Set to false to use the default size.
        /// </summary>
        public bool compact
        {
            get => ClassListContains(compactUssClassName);
            set
            {
                EnableInClassList(compactUssClassName, value);
                m_BackButton.size = value ? Size.S : Size.M;
                m_DrawerButton.size = value ? Size.S : Size.M;
                RefreshActionsStyle();
            }
        }

        void RefreshActionsStyle()
        {
            foreach (var child in m_ActionContainer.Children())
            {
                if (child is ActionButton button)
                    button.size = compact ? Size.S : Size.M;
            }
        }

        /// <summary>
        /// The elevation of the AppBar.
        /// </summary>
        public int elevation
        {
            get => m_Elevation;
            set
            {
                RemoveFromClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));
                m_Elevation = value;
                AddToClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));
            }
        }

        /// <summary>
        /// The height of the AppBar when it is expanded.
        /// See <see cref="stretch"/>.
        /// </summary>
        public float expandedHeight
        {
            get => m_ExpandedHeight;
            set
            {
                m_ExpandedHeight = value;
                RefreshFromScrollChanges();
            }
        }

        /// <summary>
        /// The flexible space element.
        /// </summary>
        public VisualElement flexibleSpace => m_FlexibleSpace;

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => m_ActionContainer;

        /// <summary>
        /// Creates a new AppBar.
        /// </summary>
        public AppBar()
        {
            pickingMode = PickingMode.Ignore;
            passMask = Passes.Clear | Passes.OutsetShadows;
            AddToClassList(ussClassName);

            var bar = new VisualElement { name = barUssClassName, pickingMode = PickingMode.Ignore };
            bar.AddToClassList(barUssClassName);
            hierarchy.Add(bar);

            m_CompactTitle = new LocalizedTextElement
            {
                name = compactTitleUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_CompactTitle.AddToClassList(compactTitleUssClassName);
            bar.Add(m_CompactTitle);

            m_BackButton = new ActionButton
            {
                name = backButtonUssClassName,
                quiet = true,
                icon = "caret-left",
            };
            m_BackButton.AddToClassList(backButtonUssClassName);
            m_BackButton.clickable.clicked += () => backButtonTriggered?.Invoke();
            bar.Add(m_BackButton);

            m_DrawerButton = new ActionButton
            {
                name = drawerButtonUssClassName,
                quiet = true,
                icon = "menu",
            };
            m_DrawerButton.AddToClassList(drawerButtonUssClassName);
            m_DrawerButton.clickable.clicked += () => drawerButtonTriggered?.Invoke();
            bar.Add(m_DrawerButton);

            var spacer = new Spacer {spacing = SpacerSpacing.Expand};
            spacer.AddToClassList(spacerUssClassName);
            bar.Add(spacer);

            m_ActionContainer = new VisualElement
            {
                name = actionContainerUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_ActionContainer.AddToClassList(actionContainerUssClassName);
            bar.Add(m_ActionContainer);

            m_FlexibleSpace = new VisualElement
            {
                name = flexibleSpaceUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_FlexibleSpace.AddToClassList(flexibleSpaceUssClassName);
            hierarchy.Add(m_FlexibleSpace);

            m_LargeTitle = new LocalizedTextElement
            {
                name = largeTitleUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_LargeTitle.AddToClassList(largeTitleUssClassName);
            m_FlexibleSpace.Add(m_LargeTitle);

            m_BottomElement = new VisualElement
            {
                name = bottomUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_BottomElement.AddToClassList(bottomUssClassName);
            hierarchy.Add(m_BottomElement);

            m_BottomBorderElement = new VisualElement
            {
                name = bottomBorderUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_BottomBorderElement.AddToClassList(bottomBorderUssClassName);
            hierarchy.Add(m_BottomBorderElement);

            showBackButton = false;
            showDrawerButton = false;
            scrollOffset = 0;
            compact = false;
            stretch = false;
            elevation = 0;
            expandedHeight = 128;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
                RefreshFromScrollChanges();
            }
            else
            {
                UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            RefreshFromScrollChanges();
        }

        /// <summary>
        /// Set this value based on the scrollview's scrollOffset in order to stretch the AppBar accordingly.
        /// </summary>
        public float scrollOffset
        {
            get => m_CurrentScrollHeight;
            set
            {
                m_CurrentScrollHeight = value;
                RefreshFromScrollChanges();
            }
        }

        /// <summary>
        /// Refreshes the AppBar from the current scroll height.
        /// </summary>
        void RefreshFromScrollChanges()
        {
            if (!stretch)
            {
                m_FlexibleSpace.style.height = 0;
                m_BottomBorderElement.style.opacity = 1;
                m_CompactTitle.style.opacity = 1;
                return;
            }

            var delta = 1f - Mathf.Clamp01(m_CurrentScrollHeight / expandedHeight);
            var height = delta * expandedHeight;
            var newStretch = height < expandedHeight;

            m_FlexibleSpace.style.height = height;
            m_BottomBorderElement.style.opacity = 1 - delta;
            m_CompactTitle.style.opacity = Mathf.Clamp01(Mathf.InverseLerp(0.65f, 0, delta));
            m_LargeTitle.style.opacity = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 1f, delta));

            if (newStretch)
                stretchTriggered?.Invoke(delta);
        }
    }
}
