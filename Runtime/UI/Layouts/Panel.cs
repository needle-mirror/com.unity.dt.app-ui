using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The root UI container that provides layered architecture for the main interface, popups, notifications,
    /// and tooltips.
    /// </summary>
    /// <remarks>
    /// The <see cref="Panel"/> component is the foundational element of any App UI runtime application. It
    /// establishes a hierarchical layer system that organizes different UI contexts, ensuring proper z-ordering
    /// and isolation between the main interface, popup elements, notifications, and tooltips.
    ///
    /// Key features:
    /// - Four distinct UI layers: main container, popup container, notification container, and tooltip container
    /// - Global context providers for theme, density, language, and layout direction
    /// - Automatic DPI scaling support for different displays
    /// - Integrated tooltip management system
    /// - RTL (Right-to-Left) layout support
    /// - Localization integration with Unity Localization package
    ///
    /// The Panel automatically manages contexts that are inherited by all child elements, making it easy to
    /// apply consistent theming, scaling, and localization throughout your application.
    ///
    /// NOTE: Each application should have one root Panel element. Additional Panel elements can be nested but
    /// will not act as root panels.
    /// </remarks>
    /// <example>
    /// <para>Basic panel setup: Creating a basic application with a Panel root.</para>
    /// <code lang="xml"><![CDATA[
    /// <Panel>
    ///     <Box>
    ///         <Heading>Welcome to App UI</Heading>
    ///         <Text text="Main content goes here" />
    ///     </Box>
    /// </Panel>
    /// ]]></code>
    /// <para>Panel with custom theme and density: Configuring panel appearance and density.</para>
    /// <code lang="xml"><![CDATA[
    /// <Panel theme="light" density="comfort" lang="en">
    ///     <Box>
    ///         <Text text="This panel uses a light theme with comfort density" />
    ///     </Box>
    /// </Panel>
    /// ]]></code>
    /// <para>Accessing panel layers programmatically: Using panel utility methods to access UI layers.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Get the panel from any element in the hierarchy
    /// var notificationLayer = Panel.FindNotificationLayer(myElement);
    /// var popupLayer = Panel.FindPopupLayer(myElement);
    /// var tooltipLayer = Panel.FindTooltipLayer(myElement);
    ///
    /// // Add a notification to the notification layer
    /// var snackbar = new Snackbar("Operation completed");
    /// notificationLayer.Add(snackbar);
    /// snackbar.Show();
    /// ]]></code>
    /// <para>RTL layout support: Setting up a panel for right-to-left languages.</para>
    /// <code lang="xml"><![CDATA[
    /// <Panel layout-direction="Rtl" lang="ar" theme="dark">
    ///     <Box>
    ///         <Text text="مرحبا بكم في واجهة المستخدم" />
    ///     </Box>
    /// </Panel>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Panel : VisualElement
    {
        internal static readonly BindingId densityProperty = nameof(density);

        internal static readonly BindingId themeProperty = nameof(theme);

        internal static readonly BindingId layoutDirectionProperty = nameof(layoutDirection);

        internal static readonly BindingId langProperty = nameof(lang);

        internal static readonly BindingId tooltipPlacementProperty = nameof(preferredTooltipPlacement);

        internal static readonly BindingId tooltipDelayMsProperty = nameof(tooltipDelayMs);

        internal static readonly BindingId forceUseTooltipSystemProperty = nameof(forceUseTooltipSystem);

        /// <summary>
        /// Main Uss Class Name.
        /// </summary>
        public const string ussClassName = "appui";

        /// <summary>
        /// Prefix used in App UI context USS classes.
        /// </summary>
        [EnumName("GetLayoutDirectionUssClassName", typeof(Dir))]
        public const string contextPrefix = "appui--";

        /// <summary>
        /// USS class applied alongside <see cref="contextPrefix"/> whenever the provided theme is a
        /// dark one, so a stylesheet can target any dark theme with a single selector.
        /// </summary>
        /// <remarks>
        /// Unlike <c>appui--dark</c>, which names one specific theme, this class marks the *kind* of
        /// surface and therefore also matches <c>editor-dark</c>, <c>cloud-dark</c> and any custom
        /// theme whose name ends in <c>dark</c>. It is mutually exclusive with
        /// <see cref="lightContextUssClassName"/>.
        /// </remarks>
        public const string darkContextUssClassName = "dark";

        /// <summary>
        /// USS class applied alongside <see cref="contextPrefix"/> whenever the provided theme is a
        /// light one. The light counterpart of <see cref="darkContextUssClassName"/>.
        /// </summary>
        public const string lightContextUssClassName = "light";

        /// <summary>
        /// The name of the main UI layer.
        /// </summary>
        public const string mainContainerName = "main-container";

        /// <summary>
        /// The name of the Popups layer.
        /// </summary>
        public const string popupContainerName = "popup-container";

        /// <summary>
        /// The name of the Notifications layer.
        /// </summary>
        public const string notificationContainerName = "notification-container";

        /// <summary>
        /// The name of the Tooltip layer.
        /// </summary>
        public const string tooltipContainerName = "tooltip-container";

        /// <summary>
        /// The default language for this panel.
        /// </summary>
        internal const string defaultLang = "en";

        /// <summary>
        /// The default density for this panel.
        /// </summary>
        internal const string defaultDensity = "normal";

        /// <summary>
        /// The default theme for this panel.
        /// </summary>
        internal const string defaultTheme = "dark";

        /// <summary>
        /// The default layout direction for this panel.
        /// </summary>
        internal const Dir defaultDir = Dir.Ltr;

        string m_PreviousTheme;

        string m_PreviousDensity;

        Dir m_PreviousDir;

        string m_PreviousLang;

        readonly VisualElement m_MainContainer;

        readonly VisualElement m_NotificationContainer;

        readonly VisualElement m_PopupContainer;

        readonly VisualElement m_TooltipContainer;

        TooltipManipulator m_TooltipManipulator;

        bool m_ForceUseTooltipSystem;

        float m_PreviousDpi = 96f;

        Vector2 m_PrimaryPointerPosition = Vector2.negativeInfinity;

        static readonly ObjectPool<Event> k_EventPool = new ObjectPool<Event>(() => new Event());

        static readonly EventCallback<UpdateEvent> k_UpdateCallback = new EventCallback<UpdateEvent>(OnUpdate);

#if UNITY_LOCALIZATION_PRESENT
        SelectedLocaleListener m_SelectedLocaleListener;
#endif

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Panel()
        {
            AddToClassList(ussClassName);

            // Add a layer for the main UI
            m_MainContainer = new VisualElement { name = mainContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_MainContainer);
            hierarchy.Add(m_MainContainer);

            // Add a layer for popups stack (popovers, modals, trays)
            m_PopupContainer = new VisualElement { name = popupContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_PopupContainer);
            hierarchy.Add(m_PopupContainer);

            // Add a layer for notifications (snackbars, toasts)
            m_NotificationContainer = new VisualElement { name = notificationContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_NotificationContainer);
            m_NotificationContainer.style.flexDirection = FlexDirection.Column;
            m_NotificationContainer.style.alignItems = Align.Center;
            m_NotificationContainer.style.justifyContent = Justify.Center;
            hierarchy.Add(m_NotificationContainer);

            // Add a layer for tooltips
            m_TooltipContainer = new VisualElement { name = tooltipContainerName, pickingMode = PickingMode.Ignore };
            SetFixedFullScreen(m_TooltipContainer);
            hierarchy.Add(m_TooltipContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<PointerMoveEvent>(OnPointerMoved);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeft);

            this.RegisterContextChangedCallback<ThemeContext>(OnThemeContextChanged);
            this.RegisterContextChangedCallback<DensityContext>(OnDensityContextChanged);
            this.RegisterContextChangedCallback<DirContext>(OnDirContextChanged);
            this.RegisterContextChangedCallback<LangContext>(OnLangContextChanged);

            lang = defaultLang;
            density = defaultDensity;
            theme = defaultTheme;
            layoutDirection = defaultDir;
            preferredTooltipPlacement = Tooltip.defaultPlacement;
            tooltipDelayMs = TooltipManipulator.defaultDelayMs;
        }

        /// <summary>
        /// Whether <paramref name="theme"/> names a dark theme.
        /// </summary>
        /// <remarks>
        /// Theme names are free-form, so darkness is read off the name's suffix rather than a fixed
        /// list. This matches the default <c>dark</c> theme as well as <c>editor-dark</c>,
        /// <c>cloud-dark</c> and a consumer's own <c>acme-dark</c>.
        /// </remarks>
        /// <param name="theme"> The theme name. </param>
        /// <returns> True if the theme is a dark one, false otherwise. </returns>
        internal static bool IsDarkTheme(string theme)
        {
            return !string.IsNullOrEmpty(theme) && theme.EndsWith("dark", StringComparison.Ordinal);
        }

        /// <summary>
        /// Swap the theme USS classes on <paramref name="element"/>, replacing those of
        /// <paramref name="previousTheme"/> with those of <paramref name="newTheme"/>.
        /// </summary>
        /// <remarks>
        /// Every element that provides a <see cref="ThemeContext"/> carries two classes: the
        /// theme-specific <c>appui--&lt;theme&gt;</c>, and one of
        /// <see cref="darkContextUssClassName"/> / <see cref="lightContextUssClassName"/> marking the
        /// kind of surface. The two markers are mutually exclusive - stylesheets rely on never
        /// matching both on one element, since that would make the outcome depend on USS source
        /// order.
        /// </remarks>
        /// <param name="element"> The element providing the theme context. </param>
        /// <param name="previousTheme"> The theme being replaced, if any. </param>
        /// <param name="newTheme"> The theme being applied, if any. </param>
        internal static void SwapThemeClasses(VisualElement element, string previousTheme, string newTheme)
        {
            if (!string.IsNullOrEmpty(previousTheme))
                element.RemoveFromClassList(MemoryUtils.Concatenate(contextPrefix, previousTheme));
            if (!string.IsNullOrEmpty(newTheme))
                element.AddToClassList(MemoryUtils.Concatenate(contextPrefix, newTheme));

            // Keep the markers exclusive: drop both, then re-add the single one that applies.
            element.RemoveFromClassList(darkContextUssClassName);
            element.RemoveFromClassList(lightContextUssClassName);
            if (!string.IsNullOrEmpty(newTheme))
                element.AddToClassList(IsDarkTheme(newTheme) ? darkContextUssClassName : lightContextUssClassName);
        }

        void OnThemeContextChanged(ContextChangedEvent<ThemeContext> evt)
        {
            // only handle the event if it comes from this panel
            if (this.GetContextProvider<ThemeContext>() != this)
                return;

            var newTheme = evt.context?.theme;
            if (m_PreviousTheme != newTheme)
            {
                SwapThemeClasses(this, m_PreviousTheme, newTheme);
                m_PreviousTheme = newTheme;
            }
        }

        void OnDensityContextChanged(ContextChangedEvent<DensityContext> evt)
        {
            // only handle the event if it comes from this panel
            if (this.GetContextProvider<DensityContext>() != this)
                return;

            var newDensity = evt.context?.density;
            if (m_PreviousDensity != newDensity)
            {
                if (m_PreviousDensity != null)
                    RemoveFromClassList(MemoryUtils.Concatenate(contextPrefix, m_PreviousDensity));
                if (newDensity != null)
                    AddToClassList(MemoryUtils.Concatenate(contextPrefix, newDensity));

                m_PreviousDensity = newDensity;
            }
        }

        void OnDirContextChanged(ContextChangedEvent<DirContext> evt)
        {
            // only handle the event if it comes from this panel
            if (this.GetContextProvider<DirContext>() != this)
                return;

            var newDir = evt.context?.dir ?? defaultDir;
            // cannot check if previous value is different than the new one here because its an enum
            // but we don't need to check if class list contains the new/old values because it is already
            // checked by UIElements API
            AddToClassList(GetLayoutDirectionUssClassName(newDir));
            if (m_PreviousDir != newDir)
                RemoveFromClassList(GetLayoutDirectionUssClassName(m_PreviousDir));

            m_PreviousDir = newDir;
        }

        void OnLangContextChanged(ContextChangedEvent<LangContext> evt)
        {
            // only handle the event if it comes from this panel
            if (this.GetContextProvider<LangContext>() != this)
                return;

            var newLang = evt.context?.lang;
            if (m_PreviousLang != newLang)
            {
                if (m_PreviousLang != null)
                    RemoveFromClassList(MemoryUtils.Concatenate(contextPrefix, m_PreviousLang));
                if (newLang != null)
                    AddToClassList(MemoryUtils.Concatenate(contextPrefix, newLang));

                m_PreviousLang = newLang;
            }
        }

        /// <summary>
        /// The default language for this panel.
        /// </summary>
        [Tooltip("The default language for this panel.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Panel")]
        public string lang
        {
            get => this.GetSelfContext<LangContext>()?.lang ?? defaultLang;
            set
            {
                var previous = this.GetSelfContext<LangContext>();
                if (previous == null || previous.lang != value)
                {
                    this.ProvideContext(string.IsNullOrEmpty(value) ? null : new LangContext(value));
                    NotifyPropertyChanged(in langProperty);
                }
            }
        }

        /// <summary>
        /// The default density for this panel.
        /// </summary>
        [Tooltip("The default density for this panel.")]
        [CreateProperty]
        [UxmlAttribute]
        public string density
        {
            get => this.GetSelfContext<DensityContext>()?.density ?? defaultDensity;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Density cannot be null or empty on a Panel element.");
                var previous = this.GetSelfContext<DensityContext>();
                if (previous?.density != value)
                {
                    this.ProvideContext(new DensityContext(value));
                    NotifyPropertyChanged(in densityProperty);
                }
            }
        }

        /// <summary>
        /// The default scale for this panel. Obsolete: the sizing axis is now <see cref="density"/>.
        /// </summary>
        /// <remarks>
        /// Maps the old scale names onto densities: <c>small</c> to <c>compact</c>, <c>medium</c> to
        /// <c>normal</c> and <c>large</c> to <c>comfort</c>. Any other value is passed through unchanged.
        /// </remarks>
        [Obsolete("Use density instead (small → compact, medium → normal, large → comfort).")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public string scale
        {
            get => density switch
            {
                "compact" => "small",
                "normal" => "medium",
                "comfort" => "large",
                var other => other
            };
            set => density = value switch
            {
                "small" => "compact",
                "medium" => "normal",
                "large" => "comfort",
                var other => other
            };
        }

        /// <summary>
        /// The default theme for this panel.
        /// </summary>
        [Tooltip("The default theme for this panel.")]
        [CreateProperty]
        [UxmlAttribute]
        public string theme
        {
            get => this.GetSelfContext<ThemeContext>()?.theme ?? defaultTheme;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Theme cannot be null or empty on a Panel element.");
                var previous = this.GetSelfContext<ThemeContext>();
                if (previous?.theme != value)
                {
                    this.ProvideContext(new ThemeContext(value));
                    NotifyPropertyChanged(in themeProperty);
                }
            }
        }

        /// <summary>
        /// The default layout direction for this panel.
        /// </summary>
        [Tooltip("The default layout direction for this panel.")]
        [CreateProperty]
        [UxmlAttribute]
        public Dir layoutDirection
        {
            get => this.GetSelfContext<DirContext>()?.dir ?? Dir.Ltr;
            set
            {
                var previous = this.GetSelfContext<DirContext>();
                if (previous == null || previous.dir != value)
                {
                    this.ProvideContext(new DirContext(value));
                    NotifyPropertyChanged(in layoutDirectionProperty);
                }
            }
        }

        /// <summary>
        /// The default preferred tooltip placement for this panel.
        /// </summary>
        /// <remarks>
        /// Note that this is just the ideal placement, the tooltip will be placed on the opposite side if there is not enough space.
        /// </remarks>
        [Tooltip("The default preferred tooltip placement for this panel.\n" +
            "Note that this is just the ideal placement, the tooltip will be placed on the opposite side if there is not enough space.")]
        [CreateProperty]
        [UxmlAttribute]
        public PopoverPlacement preferredTooltipPlacement
        {
            get => this.GetSelfContext<TooltipPlacementContext>()?.placement ?? Tooltip.defaultPlacement;
            set
            {
                var previous = this.GetSelfContext<TooltipPlacementContext>();
                if (previous == null || previous.placement != value)
                {
                    this.ProvideContext(new TooltipPlacementContext(value));
                    NotifyPropertyChanged(in tooltipPlacementProperty);
                }
            }
        }

        /// <summary>
        /// The default tooltip delay in milliseconds for this panel.
        /// </summary>
        [Tooltip("The default tooltip delay in milliseconds for this panel.")]
        [CreateProperty]
        [UxmlAttribute]
        public int tooltipDelayMs
        {
            get => this.GetSelfContext<TooltipDelayContext>()?.tooltipDelayMs ?? TooltipManipulator.defaultDelayMs;
            set
            {
                var previous = this.GetSelfContext<TooltipDelayContext>();
                if (previous == null || previous.tooltipDelayMs != value)
                {
                    this.ProvideContext(new TooltipDelayContext(value));
                    NotifyPropertyChanged(in tooltipDelayMsProperty);
                }
            }
        }

        /// <summary>
        /// If true, the panel will use the tooltip system, even if the default UI-Toolkit tooltips are enabled.
        /// </summary>
        [Tooltip("Force the use of the tooltip system, even if the default UI-Toolkit tooltips are enabled.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool forceUseTooltipSystem
        {
            get => m_ForceUseTooltipSystem;
            set
            {
                m_ForceUseTooltipSystem = value;
                if (m_TooltipManipulator != null)
                    m_TooltipManipulator.force = value;

                NotifyPropertyChanged(in forceUseTooltipSystemProperty);
            }
        }

        /// <summary>
        /// If true, this panel is the root panel of the application.
        /// </summary>
        internal bool isRootPanel { get; private set; }

        /// <summary>
        /// The main UI layer container.
        /// </summary>
        public override VisualElement contentContainer => m_MainContainer;

        /// <summary>
        /// The Popups layer container.
        /// </summary>
        public VisualElement popupContainer => m_PopupContainer;

        /// <summary>
        /// The Notifications layer container.
        /// </summary>
        public VisualElement notificationContainer => m_NotificationContainer;

        /// <summary>
        /// The Tooltip layer container.
        /// </summary>
        public VisualElement tooltipContainer => m_TooltipContainer;

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallback<UpdateEvent>(OnUpdate);
            Platform.scaleFactorChanged -= OnScaleFactorChanged;
            DismissAllPopups();
            isRootPanel = false;
            if (m_TooltipManipulator != null)
                this.RemoveManipulator(m_TooltipManipulator);
#if UNITY_LOCALIZATION_PRESENT
            if (m_SelectedLocaleListener != null)
                this.RemoveManipulator(m_SelectedLocaleListener);
#endif
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            if (!isRootPanel)
                return;

            // we receive focus out event even when calling Focus() on a child element,
            // we need to filter out those cases
            if (evt.relatedTarget is VisualElement target && target.FindCommonAncestor(this) == this)
                return;

            DismissAllPopups(DismissType.OutOfBounds);
        }

        internal void DismissAllPopups(DismissType reason = DismissType.Manual)
        {
            for (var i = m_PopupContainer.childCount - 1; i >= 0; i--)
            {
                var popupElement = m_PopupContainer[i];
                if (popupElement.userData is Popup popup)
                {
                    var shouldDismiss = reason == DismissType.Manual ||
                        (reason == DismissType.OutOfBounds && popup.focusOutDismissable);
                    if (shouldDismiss)
                        popup.Dismiss(reason);
                }
            }
        }

        void OnPointerMoved(PointerMoveEvent evt)
        {
            if (evt.pointerId == PointerId.mousePointerId || evt.isPrimary)
                m_PrimaryPointerPosition = evt.position;
        }

        void OnPointerLeft(PointerLeaveEvent evt)
        {
            if (evt.pointerId == PointerId.mousePointerId || evt.isPrimary)
                m_PrimaryPointerPosition = Vector2.negativeInfinity;
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                m_TooltipManipulator ??= new TooltipManipulator();
                this.AddManipulator(m_TooltipManipulator);
                m_TooltipManipulator.force = forceUseTooltipSystem;

                isRootPanel = !this.HasAncestorsOfType<Panel>();
                if (isRootPanel)
                {
                    if (global::Unity.AppUI.Core.AppUI.settings.autoCorrectUiScale &&
                        evt.destinationPanel.contextType == ContextType.Player)
                    {
                        m_PreviousDpi = panel.GetPanelSettings().referenceDpi;
                        // we wait to let a chance fo others panels to set their previous DPI correctly
                        schedule.Execute(() => OnScaleFactorChanged(Platform.scaleFactor));
                    }
                    Platform.scaleFactorChanged -= OnScaleFactorChanged;
                    Platform.scaleFactorChanged += OnScaleFactorChanged;
                    this.RegisterUpdateCallback(k_UpdateCallback);
#if UNITY_LOCALIZATION_PRESENT
                    m_SelectedLocaleListener ??= new SelectedLocaleListener();
                    this.AddManipulator(m_SelectedLocaleListener);
#endif
                }
            }
        }

        static void OnUpdate(UpdateEvent e)
        {
            if (e.target is not Panel panelElement)
                return;

            // skip sending event to unattached panels
            var iPanel = panelElement.panel;
            if (iPanel == null)
                return;

            var shouldPickElement = AppUIInput.pinchGestureChangedThisFrame;
            var pickedElement = shouldPickElement ? iPanel.Pick(panelElement.m_PrimaryPointerPosition) : null;
            var shouldHandleGestures = pickedElement != null;

            if (AppUIInput.pinchGestureChangedThisFrame)
            {
                if (shouldHandleGestures)
                {
                    using var evt = PinchGestureEvent.GetPooled();
                    evt.gesture = AppUIInput.pinchGesture;
                    evt.target = pickedElement;
                    panelElement.SendEvent(evt);

                    var systemEvent = k_EventPool.Get();
                    systemEvent.type = EventType.ScrollWheel;
                    systemEvent.pointerType = UnityEngine.PointerType.Mouse;
                    systemEvent.modifiers = EventModifiers.Control;
                    systemEvent.mousePosition = panelElement.m_PrimaryPointerPosition;
                    systemEvent.delta = evt.gesture.scrollDelta;
                    systemEvent.button = global::Unity.AppUI.Core.AppUI.touchPadId;
                    systemEvent.clickCount = 0;

                    using var evt2 = WheelEvent.GetPooled(systemEvent);
                    evt2.target = pickedElement;
                    panelElement.SendEvent(evt2);

                    k_EventPool.Release(systemEvent);
                }
            }
        }

        void OnScaleFactorChanged(float _)
        {
            if (!isRootPanel ||
                panel is not {contextType: ContextType.Player} ||
                !global::Unity.AppUI.Core.AppUI.settings.autoCorrectUiScale)
                return;

            try
            {
                var dpi = Platform.referenceDpi;
                var panelSettings = panel?.GetPanelSettings();
                if (panelSettings)
                {
                    var isValidDpi = dpi > 0;
                    var dpiChanged = !Mathf.Approximately(m_PreviousDpi, dpi);

                    if (dpiChanged && isValidDpi)
                    {
                        panelSettings.referenceDpi = dpi;
                        // send event
                        using var evt = DpiChangedEvent.GetPooled();
                        evt.previousValue = m_PreviousDpi;
                        evt.newValue = dpi;
                        evt.target = this;
                        SendEvent(evt);
                        m_PreviousDpi = dpi;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Utility method to quickly find the current application's Notification layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Notification layer container.</returns>
        public static VisualElement FindNotificationLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.notificationContainer;
            return element.GetFirstAncestorOfType<Panel>()?.notificationContainer;
        }

        /// <summary>
        /// Utility method to quickly find the current application's Popup layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Popup layer container.</returns>
        public static VisualElement FindPopupLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.popupContainer;
            return element.GetFirstAncestorOfType<Panel>()?.popupContainer;
        }

        /// <summary>
        /// Utility method to quickly find the current application's Tooltip layer.
        /// </summary>
        /// <param name="element">An element present in the application visual tree.</param>
        /// <returns>The Tooltip layer container.</returns>
        public static VisualElement FindTooltipLayer(VisualElement element)
        {
            if (element is Panel app)
                return app.tooltipContainer;
            return element.GetFirstAncestorOfType<Panel>()?.tooltipContainer;
        }

        static void SetFixedFullScreen(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.top = 0;
            element.style.bottom = 0;
            element.style.left = 0;
            element.style.right = 0;
        }

    }
}
