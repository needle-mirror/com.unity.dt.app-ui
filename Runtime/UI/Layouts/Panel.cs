using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// This is the main UI element of any Runtime App. The <see cref="Panel"/> class will create different
    /// UI layers for the main user-interface, popups, notifications and tooltips.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class Panel : VisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId scaleProperty = nameof(scale);
        
        internal static readonly BindingId themeProperty = nameof(theme);
        
        internal static readonly BindingId layoutDirectionProperty = nameof(layoutDirection);
        
        internal static readonly BindingId langProperty = nameof(lang);
        
        internal static readonly BindingId tooltipPlacementProperty = nameof(preferredTooltipPlacement);
        
        internal static readonly BindingId tooltipDelayMsProperty = nameof(tooltipDelayMs);
#endif
        
        /// <summary>
        /// Main Uss Class Name.
        /// </summary>
        public static readonly string ussClassName = "appui";
        
        /// <summary>
        /// Prefix used in App UI context USS classes.
        /// </summary>
        public static readonly string contextPrefix = "appui--";
        
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
        
        const string k_DefaultLang = "en";
        
        const string k_DefaultScale = "medium";
        
        const string k_DefaultTheme = "dark";
        
        const Dir k_DefaultDir = Dir.Ltr;
        
        readonly VisualElement m_MainContainer;

        readonly VisualElement m_NotificationContainer;

        readonly VisualElement m_PopupContainer;

        readonly VisualElement m_TooltipContainer;

        readonly List<Popup> m_DismissablePopups = new List<Popup>();

        TooltipManipulator m_TooltipManipulator;

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

            lang = k_DefaultLang;
            scale = k_DefaultScale;
            theme = k_DefaultTheme;
            layoutDirection = k_DefaultDir;
            preferredTooltipPlacement = Tooltip.defaultPlacement;
            tooltipDelayMs = TooltipManipulator.defaultDelayMs;
        }
        
        /// <summary>
        /// The default language for this panel.
        /// </summary>
        [Tooltip("The default language for this panel.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
        [Header("Panel")]
#endif
        public string lang
        {
            get => this.GetSelfContext<LangContext>()?.lang ?? k_DefaultLang;
            set
            {
                var previous = this.GetSelfContext<LangContext>();
                if (previous == null || previous.lang != value)
                {
                    if (!string.IsNullOrEmpty(previous?.lang))
                        RemoveFromClassList(contextPrefix + previous.lang);
                    if (!string.IsNullOrEmpty(value))
                        AddToClassList(contextPrefix + value);
                    this.ProvideContext(string.IsNullOrEmpty(value) ? null : new LangContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in langProperty);
#endif
                }
            }
        }

        /// <summary>
        /// The default scale for this panel.
        /// </summary>
        [Tooltip("The default scale for this panel.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string scale
        {
            get => this.GetSelfContext<ScaleContext>()?.scale ?? k_DefaultScale;
            set
            {
                if (string.IsNullOrEmpty(value))
                    Debug.LogError("Scale cannot be null or empty on a Panel element.");
                var previous = this.GetSelfContext<ScaleContext>();
                if (previous == null || previous.scale != value)
                {
                    if (!string.IsNullOrEmpty(previous?.scale))
                        RemoveFromClassList(contextPrefix + previous.scale);
                    if (!string.IsNullOrEmpty(value))
                        AddToClassList(contextPrefix + value);
                    this.ProvideContext(new ScaleContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in scaleProperty);
#endif
                }
            }
        }
        
        /// <summary>
        /// The default theme for this panel.
        /// </summary>
        [Tooltip("The default theme for this panel.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string theme
        {
            get => this.GetSelfContext<ThemeContext>()?.theme ?? k_DefaultTheme;
            set
            {
                if (string.IsNullOrEmpty(value))
                    Debug.LogError("Theme cannot be null or empty on a Panel element.");
                var previous = this.GetSelfContext<ThemeContext>();
                if (previous == null || previous.theme != value)
                {
                    if (!string.IsNullOrEmpty(previous?.theme))
                        RemoveFromClassList(contextPrefix + previous.theme);
                    if (!string.IsNullOrEmpty(value))
                        AddToClassList(contextPrefix + value);
                    this.ProvideContext(new ThemeContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in themeProperty);
#endif
                }
            }
        }
        
        /// <summary>
        /// The default layout direction for this panel.
        /// </summary>
        [Tooltip("The default layout direction for this panel.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Dir layoutDirection
        {
            get => this.GetSelfContext<DirContext>()?.dir ?? Dir.Ltr;
            set
            {
                var previous = this.GetSelfContext<DirContext>();
                if (previous == null || previous.dir != value)
                {
                    if (previous != null)
                        RemoveFromClassList(contextPrefix + previous.dir.ToString().ToLower());
                    AddToClassList(contextPrefix + value.ToString().ToLower());
                    this.ProvideContext(new DirContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in layoutDirectionProperty);
#endif
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
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public PopoverPlacement preferredTooltipPlacement
        {
            get => this.GetSelfContext<TooltipPlacementContext>()?.placement ?? Tooltip.defaultPlacement;
            set
            {
                var previous = this.GetSelfContext<TooltipPlacementContext>();
                if (previous == null || previous.placement != value)
                {
                    this.ProvideContext(new TooltipPlacementContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in tooltipPlacementProperty);
#endif
                }
            }
        }
        
        /// <summary>
        /// The default tooltip delay in milliseconds for this panel.
        /// </summary>
        [Tooltip("The default tooltip delay in milliseconds for this panel.")]
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int tooltipDelayMs
        {
            get => this.GetSelfContext<TooltipDelayContext>()?.tooltipDelayMs ?? TooltipManipulator.defaultDelayMs;
            set
            {
                var previous = this.GetSelfContext<TooltipDelayContext>();
                if (previous == null || previous.tooltipDelayMs != value)
                {
                    this.ProvideContext(new TooltipDelayContext(value));
#if ENABLE_RUNTIME_DATA_BINDINGS
                    NotifyPropertyChanged(in tooltipDelayMsProperty);
#endif
                }
            }
        }

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
            if (evt.originPanel != null)
            {
                if (m_TooltipManipulator != null)
                    this.RemoveManipulator(m_TooltipManipulator);
                Core.AppUI.UnregisterPanel(this);
            }
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                if (m_TooltipManipulator == null)
                {
                    m_TooltipManipulator = new TooltipManipulator();
                    this.AddManipulator(m_TooltipManipulator);
                }

                Core.AppUI.RegisterPanel(this);
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

        /// <summary>
        /// Dismiss any open <see cref="Popup"/> that are actually open.
        /// </summary>
        internal void DismissAnyPopups(DismissType reason)
        {
            foreach (var popover in m_DismissablePopups)
            {
                popover?.Dismiss(reason);
            }
            m_DismissablePopups.Clear();
        }

        /// <summary>
        /// Register a <see cref="Popup"/> to the list of dismissable popups.
        /// </summary>
        /// <param name="popup"> The <see cref="Popup"/> to register.</param>
        internal void RegisterPopup(Popup popup)
        {
            m_DismissablePopups.Add(popup);
        }

        /// <summary>
        /// Unregister a <see cref="Popup"/> from the list of dismissable popups.
        /// </summary>
        /// <param name="anchorPopup"></param>
        public void UnregisterPopup(Popup anchorPopup)
        {
            m_DismissablePopups.Remove(anchorPopup);
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Class used to create instances of <see cref="Panel"/> from UXML.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<Panel, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="Panel"/>.
        /// </summary>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Lang = new UxmlStringAttributeDescription
            {
                name = "lang",
                defaultValue = k_DefaultLang
            };
            
            readonly UxmlStringAttributeDescription m_Scale = new UxmlStringAttributeDescription
            {
                name = "scale", 
                defaultValue = k_DefaultScale,
                restriction = new UxmlEnumeration
                {
                    values = new[] { "small", "medium", "large" }
                }
            };
            
            readonly UxmlStringAttributeDescription m_Theme = new UxmlStringAttributeDescription
            {
                name = "theme", 
                defaultValue = k_DefaultTheme,
                restriction = new UxmlEnumeration
                {
                    values = new[] { "light", "dark", "editor-dark", "editor-light" }
                }
            };
            
            readonly UxmlEnumAttributeDescription<Dir> m_Dir = new UxmlEnumAttributeDescription<Dir>
            {
                name = "dir", 
                defaultValue = k_DefaultDir
            };
            
            readonly UxmlEnumAttributeDescription<PopoverPlacement> m_PreferredTooltipPlacement = new UxmlEnumAttributeDescription<PopoverPlacement>
            {
                name = "preferred-tooltip-placement", 
                defaultValue = Tooltip.defaultPlacement
            };
            
            readonly UxmlIntAttributeDescription m_TooltipDelayMs = new UxmlIntAttributeDescription
            {
                name = "tooltip-delay-ms", 
                defaultValue = TooltipManipulator.defaultDelayMs
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var panel = (Panel)ve;
                
                panel.lang = m_Lang.GetValueFromBag(bag, cc);
                panel.scale = m_Scale.GetValueFromBag(bag, cc);
                panel.theme = m_Theme.GetValueFromBag(bag, cc);
                panel.layoutDirection = m_Dir.GetValueFromBag(bag, cc);
                panel.preferredTooltipPlacement = m_PreferredTooltipPlacement.GetValueFromBag(bag, cc);
                panel.tooltipDelayMs = m_TooltipDelayMs.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
