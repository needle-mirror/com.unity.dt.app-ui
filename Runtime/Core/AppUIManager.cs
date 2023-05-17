using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_INPUTSYSTEM_PRESENT
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
#endif

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The main manager for the AppUI system.
    /// This class is responsible for managing the main looper, the notification manager and the settings.
    /// It also provides access to them.
    /// </summary>
    public class AppUIManager
    {
        // ReSharper disable once InconsistentNaming
        internal AppUISettings m_Settings;

        Looper m_MainLooper;

        readonly Dictionary<PanelSettings, HashSet<Panel>> m_PanelSettings = new Dictionary<PanelSettings, HashSet<Panel>>();

        NotificationManager m_NotificationManager;

        internal AppUISettings defaultSettings { get; private set; }

        /// <summary>
        /// The current settings.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the settings are not ready.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the provided settings object is null.</exception>
        internal AppUISettings settings
        {
            get
            {
                if (m_Settings)
                    return m_Settings;
                
                if (!defaultSettings)
                    defaultSettings = ScriptableObject.CreateInstance<AppUISettings>();
                
                return defaultSettings;
            }
            set
            {
                if (!value)
                    throw new ArgumentNullException(nameof(value));

                if (m_Settings == value)
                    return;

                m_Settings = value;
                ApplySettings();
            }
        }

        /// <summary>
        /// The main looper.
        /// </summary>
        internal Looper mainLooper => m_MainLooper;

        /// <summary>
        /// The notification manager.
        /// </summary>
        internal NotificationManager notificationManager => m_NotificationManager;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal AppUIManager()
        {

        }

        /// <summary>
        /// Initializes the AppUIManager.
        /// </summary>
        /// <param name="newSettings">The settings to use.</param>
        internal void Initialize(AppUISettings newSettings)
        {
            Debug.Assert(newSettings, "The provided settings object can't be null");

            m_Settings = newSettings;
            m_MainLooper = new Looper();
            m_NotificationManager = new NotificationManager(this);

            m_MainLooper.Loop();

            ApplySettings();
            
#if UNITY_INPUTSYSTEM_PRESENT && ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            InputSystem.onActionChange -= OnActionChange;
            InputSystem.onActionChange += OnActionChange;
#endif
        }

#if UNITY_INPUTSYSTEM_PRESENT && ENABLE_INPUT_SYSTEM

        Vector2 m_PointerPosition = Vector2.zero;

        void OnActionChange(object arg1, InputActionChange arg2)
        {
            var module = EventSystem.current ? EventSystem.current.currentInputModule as InputSystemUIInputModule : null;
            if (arg2 == InputActionChange.ActionPerformed && arg1 is InputAction action && module)
            {
                if (action.name == module.leftClick.action.name)
                    HandlePointerDown(m_PointerPosition);
                else if (action.name == module.point.action.name)
                    m_PointerPosition = action.ReadValue<Vector2>();
            }
        }
#endif

        /// <summary>
        /// Shutdown the AppUIManager.
        /// </summary>
        internal void Shutdown()
        {
            m_MainLooper.Quit();
        }

        /// <summary>
        /// Applies the current settings.
        /// </summary>
        internal void ApplySettings()
        {

        }

        /// <summary>
        /// Update method that should be called every frame.
        /// </summary>
        internal void Update()
        {
            Platform.PollSystemTheme();

            if (m_PanelSettings is { Count: > 0 } && m_Settings.autoCorrectUiScale)
            {
                var dpi = Platform.referenceDpi;
                foreach (var panelSettings in m_PanelSettings.Keys)
                {
                    if (panelSettings)
                    {
                        if (!Mathf.Approximately(panelSettings.referenceDpi, dpi))
                        {
                            var previousValue = panelSettings.referenceDpi;
                            panelSettings.referenceDpi = dpi;
                            foreach (var panel in m_PanelSettings[panelSettings])
                            {
                                using var evt = DpiChangedEvent.GetPooled();
                                evt.previousValue = previousValue;
                                evt.newValue = dpi;
                                evt.target = panel;
                                panel.SendEvent(evt);
                            }
                        }
                    }
                }
            }
            
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
                HandlePointerDown(Input.mousePosition);
#endif

            m_MainLooper.LoopOnce();
        }
        
        void HandlePointerDown(Vector2 position)
        {
            // Reverse Y axis for UIElements
            position = new Vector2(position.x, Screen.height - position.y);
            foreach (var panelSettings in m_PanelSettings.Keys)
            {
                foreach (var panel in m_PanelSettings[panelSettings])
                {
                    if (panel is {panel: {} iPanel})
                    {
                        var coord = RuntimePanelUtils.ScreenToPanel(iPanel, position);
                        var picked = iPanel.Pick(coord);
                        if (picked == null)
                            panel.DismissAnyPopups(DismissType.OutOfBounds);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a panel.
        /// </summary>
        /// <param name="element">The panel to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided panel is null.</exception>
        internal void RegisterPanel(Panel element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var panelSettings = element.panel?.GetPanelSettings();

            if (panelSettings == null)
                return;

            if (m_PanelSettings.ContainsKey(panelSettings))
                m_PanelSettings[panelSettings].Add(element);
            else
                m_PanelSettings.Add(panelSettings, new HashSet<Panel> { element });
        }

        /// <summary>
        /// Unregisters a panel.
        /// </summary>
        /// <param name="element">The panel to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided panel is null.</exception>
        internal void UnregisterPanel(Panel element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var panelSettings = element.panel?.GetPanelSettings();

            if (panelSettings == null)
                return;

            if (m_PanelSettings.ContainsKey(panelSettings))
            {
                m_PanelSettings[panelSettings].Remove(element);
                if (m_PanelSettings[panelSettings].Count == 0)
                    m_PanelSettings.Remove(panelSettings);
            }
        }
    }
}
