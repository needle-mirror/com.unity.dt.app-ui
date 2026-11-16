using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A floating control that switches the host <see cref="Panel"/>'s theme and density.
    /// </summary>
    /// <remarks>
    /// TweakMenu is a developer and design-review affordance: a gear button pinned to a corner of the
    /// screen that opens a menu of every App UI theme and every density step. Drop one into a sample,
    /// a test scene or a debug overlay and the whole panel restyles live, which is the fastest way to
    /// check a component under all eight themes and all three densities without rebuilding.
    ///
    /// It writes straight through to the two context providers that matter, <see cref="Panel.theme"/>
    /// and <see cref="Panel.density"/>, so every descendant of the panel follows. On attach it reads
    /// the panel back, so the menu opens showing the state that is actually in effect rather than its
    /// own defaults.
    ///
    /// The density axis is <c>normal</c> / <c>compact</c> / <c>comfort</c>. It is a design axis and is
    /// unrelated to <c>Platform.scaleFactor</c>, which is display DPI.
    ///
    /// This is not a user-facing preferences UI. It has no persistence of its own; if you want the
    /// choice to survive a session, listen to <see cref="themeChanged"/> and
    /// <see cref="densityChanged"/> and store it yourself.
    /// </remarks>
    /// <example>
    /// <para>Adding a tweak menu to a panel.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Panel theme="dark" density="normal">
    ///     <ui:TweakMenu />
    /// </ui:Panel>
    /// ]]></code>
    /// <para>Persisting the choice.</para>
    /// <code lang="csharp"><![CDATA[
    /// var tweaks = new TweakMenu
    /// {
    ///     theme = PlayerPrefs.GetString("theme", "dark"),
    ///     density = PlayerPrefs.GetString("density", "normal"),
    /// };
    /// tweaks.themeChanged += t => PlayerPrefs.SetString("theme", t);
    /// tweaks.densityChanged += d => PlayerPrefs.SetString("density", d);
    /// panel.Add(tweaks);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("context-components", id = "tweak-menu", displayName = "Tweak Menu")]
    public partial class TweakMenu : BaseVisualElement
    {
        internal static readonly BindingId themeProperty = new BindingId(nameof(theme));

        internal static readonly BindingId densityProperty = new BindingId(nameof(density));

        const string k_TriggerIconName = "gear";

        /// <summary>
        /// The TweakMenu main styling class.
        /// </summary>
        public const string ussClassName = "appui-tweakmenu";

        /// <summary>
        /// The TweakMenu trigger button styling class.
        /// </summary>
        public const string triggerUssClassName = ussClassName + "__trigger";

        /// <summary>
        /// The TweakMenu menu styling class.
        /// </summary>
        public const string menuUssClassName = ussClassName + "__menu";

        /// <summary>
        /// Every theme the menu offers, in display order.
        /// </summary>
        public static readonly string[] themes =
        {
            "dark",
            "light",
            "editor-dark",
            "editor-light",
            "editor-7-dark",
            "editor-7-light",
            "cloud-dark",
            "cloud-light",
        };

        /// <summary>
        /// Every density step the menu offers, in display order.
        /// </summary>
        public static readonly string[] densities =
        {
            "normal",
            "compact",
            "comfort",
        };

        static readonly string[] k_ThemeLabels =
        {
            "Dark",
            "Light",
            "Editor Dark",
            "Editor Light",
            "Editor 7 Dark",
            "Editor 7 Light",
            "Cloud Dark",
            "Cloud Light",
        };

        static readonly string[] k_DensityLabels =
        {
            "Normal",
            "Compact",
            "Comfort",
        };

        readonly IconButton m_Trigger;

        readonly Menu m_Menu;

        readonly MenuRadioGroup m_ThemeGroup;

        readonly MenuRadioGroup m_DensityGroup;

        string m_Theme = Panel.defaultTheme;

        string m_Density = Panel.defaultDensity;

        /// <summary>
        /// Raised when the theme has changed.
        /// </summary>
        public event Action<string> themeChanged;

        /// <summary>
        /// Raised when the density has changed.
        /// </summary>
        public event Action<string> densityChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TweakMenu()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;

            m_Trigger = new IconButton(k_TriggerIconName) { name = triggerUssClassName };
            m_Trigger.AddToClassList(triggerUssClassName);
            m_Trigger.clickable.clicked += OpenMenu;
            hierarchy.Add(m_Trigger);

            m_Menu = new Menu { name = menuUssClassName };
            m_Menu.AddToClassList(menuUssClassName);

            m_ThemeGroup = new MenuRadioGroup { value = m_Theme };
            for (var i = 0; i < themes.Length; i++)
                m_ThemeGroup.Add(new MenuItem { key = themes[i], label = k_ThemeLabels[i] });
            m_ThemeGroup.RegisterValueChangedCallback(evt => theme = evt.newValue);

            m_DensityGroup = new MenuRadioGroup { value = m_Density };
            for (var i = 0; i < densities.Length; i++)
                m_DensityGroup.Add(new MenuItem { key = densities[i], label = k_DensityLabels[i] });
            m_DensityGroup.RegisterValueChangedCallback(evt => density = evt.newValue);

            var themeSection = new MenuSection { title = "Theme" };
            themeSection.Add(m_ThemeGroup);
            var densitySection = new MenuSection { title = "Density" };
            densitySection.Add(m_DensityGroup);

            m_Menu.Add(themeSection);
            m_Menu.Add(new MenuDivider());
            m_Menu.Add(densitySection);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        /// <summary>
        /// The gear button that opens the menu.
        /// </summary>
        public IconButton trigger => m_Trigger;

        /// <summary>
        /// The menu shown by the trigger.
        /// </summary>
        public Menu menu => m_Menu;

        /// <summary>
        /// The theme radio group.
        /// </summary>
        public MenuRadioGroup themeRadioGroup => m_ThemeGroup;

        /// <summary>
        /// The density radio group.
        /// </summary>
        public MenuRadioGroup densityRadioGroup => m_DensityGroup;

        /// <summary>
        /// The active theme. One of <see cref="themes"/>. Applied to the host <see cref="Panel"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string theme
        {
            get => m_Theme;
            set
            {
                if (string.IsNullOrEmpty(value) || m_Theme == value)
                    return;
                m_Theme = value;
                m_ThemeGroup.SetValueWithoutNotify(value);
                var hostPanel = GetFirstAncestorOfType<Panel>();
                if (hostPanel != null)
                    hostPanel.theme = value;
                themeChanged?.Invoke(value);

                NotifyPropertyChanged(in themeProperty);
            }
        }

        /// <summary>
        /// The active density. One of <see cref="densities"/>. Applied to the host <see cref="Panel"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string density
        {
            get => m_Density;
            set
            {
                if (string.IsNullOrEmpty(value) || m_Density == value)
                    return;
                m_Density = value;
                m_DensityGroup.SetValueWithoutNotify(value);
                var hostPanel = GetFirstAncestorOfType<Panel>();
                if (hostPanel != null)
                    hostPanel.density = value;
                densityChanged?.Invoke(value);

                NotifyPropertyChanged(in densityProperty);
            }
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            var hostPanel = GetFirstAncestorOfType<Panel>();
            if (hostPanel == null)
                return;

            // Adopt what is actually in effect, so the menu never opens on a lie.
            theme = hostPanel.theme;
            density = hostPanel.density;
        }

        void OpenMenu()
        {
            MenuBuilder.Build(m_Trigger, m_Menu)
                .SetCloseOnSelection(false)
                .Show();
        }
    }
}
