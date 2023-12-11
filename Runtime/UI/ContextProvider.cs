using System;
using UnityEngine.Assertions;
using Unity.AppUI.Core;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// This element can be used in the visual tree to wrap a part of the user-interface where the context
    /// of the application needs to be overriden.
    /// </summary>
    public class ContextProvider : VisualElement
    {
        /// <summary>
        /// Main Uss Class Name.
        /// </summary>
        public static readonly string ussClassName = "appui-context-provider";

        /// <summary>
        /// Prefix used in Context USS classes.
        /// </summary>
        public static readonly string contextPrefix = "appui--";
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContextProvider()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// The current context, computed using the root context of the <see cref="Panel"/> and cascading context
        /// providers overrides until this <see cref="ContextProvider"/> instance.
        /// </summary>
        public virtual ApplicationContext context
        {
            get
            {
                var parentContextProvider = GetFirstAncestorOfType<ContextProvider>();
                Assert.IsNotNull(parentContextProvider, "You should have at least the main Application element as parent context provider.");
                return new ApplicationContext(parentContextProvider.context, this);
            }
        }

        /// <summary>
        /// The current language override.
        /// <remarks> This property is useful only if you use the Unity Localization package inside your project.</remarks>
        /// </summary>
        public string lang
        {
            get => this.GetSelfContext<LangContext>()?.lang;
            set
            {
                var currentLang = lang;
                if (!string.IsNullOrEmpty(currentLang))
                    RemoveFromClassList($"{contextPrefix}{currentLang}");
                if (!string.IsNullOrEmpty(value))
                    AddToClassList($"{contextPrefix}{value}");
                this.ProvideContext(string.IsNullOrEmpty(value) ? null : new LangContext(value));
            }
        }

        /// <summary>
        /// The preferred <see cref="Tooltip"/> placement in this context.
        /// </summary>
        public PopoverPlacement? preferredTooltipPlacement
        {
            get => this.GetSelfContext<TooltipPlacementContext>()?.placement;
            set => this.ProvideContext(value.HasValue ? new TooltipPlacementContext(value.Value) : null);
        }

        /// <summary>
        /// The current UI scale override.
        /// </summary>
        public string scale
        {
            get => this.GetSelfContext<ScaleContext>()?.scale;
            set
            {
                var currentScale = scale;
                if (!string.IsNullOrEmpty(currentScale))
                    RemoveFromClassList($"{contextPrefix}{currentScale}");
                if (!string.IsNullOrEmpty(value))
                    AddToClassList($"{contextPrefix}{value}");
                this.ProvideContext(string.IsNullOrEmpty(value) ? null : new ScaleContext(value));
            }
        }

        /// <summary>
        /// The current UI theme override.
        /// </summary>
        public string theme
        {
            get => this.GetSelfContext<ThemeContext>()?.theme;
            set
            {
                var currentTheme = theme;
                if (!string.IsNullOrEmpty(currentTheme))
                    RemoveFromClassList($"{contextPrefix}{currentTheme}");
                if (!string.IsNullOrEmpty(value))
                    AddToClassList($"{contextPrefix}{value}");
                this.ProvideContext(string.IsNullOrEmpty(value) ? null : new ThemeContext(value));
            }
        }

        /// <summary>
        /// The delay in milliseconds before a tooltip is shown.
        /// </summary>
        public int? tooltipDelayMs
        {
            get => this.GetSelfContext<TooltipDelayContext>()?.tooltipDelayMs;
            set => this.ProvideContext(value.HasValue ? new TooltipDelayContext(value.Value) : null);
        }

        /// <summary>
        /// The current layout direction override.
        /// </summary>
        public Dir? dir
        {
            get => this.GetSelfContext<DirContext>()?.dir;
            set
            {
                var currentDir = dir;
                if (currentDir.HasValue)
                    RemoveFromClassList($"{contextPrefix}{currentDir.Value.ToString().ToLower()}");
                if (value != null)
                    AddToClassList($"{contextPrefix}{value.ToString().ToLower()}");
                this.ProvideContext(value == null ? null : new DirContext(value.Value));
            }
        }

        /// <summary>
        /// A class responsible for creating a <see cref="ContextProvider"/> from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ContextProvider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ContextProvider"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Lang = new UxmlStringAttributeDescription
            {
                defaultValue = null,
                name = "lang",
                use = UxmlAttributeDescription.Use.Optional
            };

            readonly UxmlStringAttributeDescription m_Scale = new UxmlStringAttributeDescription
            {
                defaultValue = "medium",
                name = "scale",
                use = UxmlAttributeDescription.Use.Optional
            };

            readonly UxmlStringAttributeDescription m_Theme = new UxmlStringAttributeDescription
            {
                defaultValue = "dark",
                name = "theme",
                use = UxmlAttributeDescription.Use.Optional
            };
            
            readonly UxmlIntAttributeDescription m_TooltipDelayMs = new UxmlIntAttributeDescription
            {
                defaultValue = TooltipManipulator.defaultDelayMs,
                name = "tooltip-delay-ms",
                use = UxmlAttributeDescription.Use.Optional
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);
                var el = (ContextProvider)ve;
                string val = null;
                if (m_Lang.TryGetValueFromBag(bag, cc, ref val))
                    el.lang = val;
                if (m_Theme.TryGetValueFromBag(bag, cc, ref val))
                    el.theme = val;
                if (m_Scale.TryGetValueFromBag(bag, cc, ref val))
                    el.scale = val;
                var tooltipDelayMs = TooltipManipulator.defaultDelayMs;
                if (m_TooltipDelayMs.TryGetValueFromBag(bag, cc, ref tooltipDelayMs))
                    el.tooltipDelayMs = tooltipDelayMs;
            }
        }
    }
}
