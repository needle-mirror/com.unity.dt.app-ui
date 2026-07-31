using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Base class for all textual App UI components.
    /// </summary>
    [UxmlElement]
    public abstract partial class BaseTextElement : TextElement, IContextOverrideElement, IAdditionalDataHolder
    {
        internal static readonly BindingId preferredTooltipPlacementOverrideProperty = nameof(preferredTooltipPlacementOverride);

        internal static readonly BindingId tooltipDelayMsOverrideProperty = nameof(tooltipDelayMsOverride);

        internal static readonly BindingId scaleOverrideProperty = nameof(scaleOverride);

        internal static readonly BindingId themeOverrideProperty = nameof(themeOverride);

        internal static readonly BindingId langOverrideProperty = nameof(langOverride);

        internal static readonly BindingId layoutDirectionOverrideProperty = nameof(layoutDirectionOverride);

        /// <summary>
        /// The context prefix used as USS selector.
        /// </summary>
        [EnumName("GetLayoutDirectionUssClassName", typeof(Dir))]
        public const string contextPrefix = Panel.contextPrefix;

        VisualElementExtensions.AdditionalData IAdditionalDataHolder.additionalData { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected BaseTextElement()
        {
            preferredTooltipPlacementOverride = OptionalEnum<PopoverPlacement>.none;
            tooltipDelayMsOverride = Optional<int>.none;
            scaleOverride = Optional<string>.none;
            themeOverride = Optional<string>.none;
            langOverride = Optional<string>.none;
            layoutDirectionOverride = OptionalEnum<Dir>.none;
        }

        /// <summary>
        /// The scale to use in this part of the UI.
        /// </summary>
        [Tooltip("The scale to use in this part of the UI.")]
        [CreateProperty]
        [UxmlAttribute("scale")]
        [OptionalScaleDrawer]
        public Optional<string> scaleOverride
        {
            get => this.GetSelfContext<ScaleContext>() is {} ctx ?
                ctx.scale : Optional<string>.none;
            set
            {
                var previous = this.GetSelfContext<ScaleContext>();
                var newCtx = value.IsSet ? new ScaleContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                if (!string.IsNullOrEmpty(previous?.scale))
                    RemoveFromClassList(MemoryUtils.Concatenate(Panel.contextPrefix, previous.scale));
                if (!string.IsNullOrEmpty(newCtx?.scale))
                    AddToClassList(MemoryUtils.Concatenate(Panel.contextPrefix, newCtx.scale));
                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in scaleOverrideProperty);
            }
        }

        /// <summary>
        /// The theme to use in this part of the UI.
        /// </summary>
        [Tooltip("The theme to use in this part of the UI.")]
        [CreateProperty]
        [UxmlAttribute("theme")]
        [OptionalThemeDrawer]
        public Optional<string> themeOverride
        {
            get => this.GetSelfContext<ThemeContext>() is {} ctx ?
                ctx.theme : Optional<string>.none;
            set
            {
                var previous = this.GetSelfContext<ThemeContext>();
                var newCtx = value.IsSet ? new ThemeContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                if (!string.IsNullOrEmpty(previous?.theme))
                    RemoveFromClassList(MemoryUtils.Concatenate(Panel.contextPrefix, previous.theme));
                if (!string.IsNullOrEmpty(newCtx?.theme))
                    AddToClassList(MemoryUtils.Concatenate(Panel.contextPrefix, newCtx.theme));
                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in themeOverrideProperty);
            }
        }

        /// <summary>
        /// The language to use in this part of the UI.
        /// </summary>
        [Tooltip("The language to use in this part of the UI.")]
        [CreateProperty]
        [UxmlAttribute("lang")]
        public Optional<string> langOverride
        {
            get => this.GetSelfContext<LangContext>() is {} ctx ?
                ctx.lang : Optional<string>.none;
            set
            {
                var previous = this.GetSelfContext<LangContext>();
                var newCtx = value.IsSet ? new LangContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                if (!string.IsNullOrEmpty(previous?.lang))
                    RemoveFromClassList(MemoryUtils.Concatenate(Panel.contextPrefix, previous.lang));
                if (!string.IsNullOrEmpty(newCtx?.lang))
                    AddToClassList(MemoryUtils.Concatenate(Panel.contextPrefix, newCtx.lang));
                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in langOverrideProperty);
            }
        }

        /// <summary>
        /// The layout direction to use in this part of the UI.
        /// </summary>
        [Tooltip("The layout direction to use in this part of the UI.")]
        [CreateProperty]
        [UxmlAttribute("dir")]
        public OptionalEnum<Dir> layoutDirectionOverride
        {
            get => this.GetSelfContext<DirContext>() is {} ctx ?
                ctx.dir : OptionalEnum<Dir>.none;
            set
            {
                var previous = this.GetSelfContext<DirContext>();
                var newCtx = value.IsSet ? new DirContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                if (previous != null)
                    RemoveFromClassList(GetLayoutDirectionUssClassName(previous.dir));
                if (newCtx != null)
                    AddToClassList(GetLayoutDirectionUssClassName(newCtx.dir));
                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in layoutDirectionOverrideProperty);
            }
        }

        /// <summary>
        /// Preferred placement for tooltips.
        /// </summary>
        [Tooltip("Preferred placement for tooltips.\n" +
            "Note that this is only a hint and the tooltip may be placed differently if there is not enough space.")]
        [CreateProperty]
        [UxmlAttribute("preferred-tooltip-placement")]
        public OptionalEnum<PopoverPlacement> preferredTooltipPlacementOverride
        {
            get => this.GetSelfContext<TooltipPlacementContext>() is {} ctx ?
                ctx.placement : OptionalEnum<PopoverPlacement>.none;
            set
            {
                var previous = this.GetSelfContext<TooltipPlacementContext>();
                var newCtx = value.IsSet ? new TooltipPlacementContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in preferredTooltipPlacementOverrideProperty);
            }
        }

        /// <summary>
        /// Delay in milliseconds before showing a tooltip.
        /// </summary>
        [Tooltip("Delay in milliseconds before showing a tooltip.")]
        [CreateProperty]
        [UxmlAttribute("tooltip-delay-ms")]
        public Optional<int> tooltipDelayMsOverride
        {
            get => this.GetSelfContext<TooltipDelayContext>() is {} ctx ?
                ctx.tooltipDelayMs : Optional<int>.none;
            set
            {
                var previous = this.GetSelfContext<TooltipDelayContext>();
                var newCtx = value.IsSet ? new TooltipDelayContext(value.Value) : null;

                if (previous == newCtx)
                    return;

                this.ProvideContext(newCtx);
                NotifyPropertyChanged(in tooltipDelayMsOverrideProperty);
            }
        }


    }
}
