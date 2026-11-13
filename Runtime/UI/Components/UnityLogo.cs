using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Fill states for a <see cref="UnityLogo"/>.
    /// </summary>
    public enum UnityLogoVariant
    {
        /// <summary>
        /// The solid logo. This is the default.
        /// </summary>
        Filled = 1,

        /// <summary>
        /// A dimmed, unfilled logo.
        /// </summary>
        Empty,

        /// <summary>
        /// The logo fills in from empty, then restarts.
        /// </summary>
        Animated
    }

    /// <summary>
    /// The Unity brand mark.
    /// </summary>
    /// <remarks>
    /// UnityLogo draws the mark through an <see cref="Icon"/>, so it takes its size from the same
    /// <c>--appui-alias-control-sizing-icon-*</c> ramp as every other glyph and rescales with density.
    /// The image is the brand asset the package already ships, tinted with the primary icon token so it
    /// follows the current theme.
    ///
    /// <see cref="variant"/> controls the fill. <see cref="UnityLogoVariant.Filled"/> is the solid mark,
    /// <see cref="UnityLogoVariant.Empty"/> dims it evenly, and <see cref="UnityLogoVariant.Animated"/>
    /// fills it in from empty over two seconds and restarts. USS has no keyframe animations, so the
    /// animated variant is driven from C# by a scheduled item that writes the icon's opacity every
    /// frame. The driver only runs while the element is attached to a panel, and it clears the opacity
    /// it wrote when it stops.
    ///
    /// The logo ignores pointer events and is not focusable.
    /// </remarks>
    /// <example>
    /// <para>The solid mark, at the default medium size.</para>
    /// <code lang="csharp">
    /// var logo = new UnityLogo();
    /// parentElement.Add(logo);
    /// </code>
    /// <para>A large logo that fills itself in, for a splash or a loading state.</para>
    /// <code lang="csharp">
    /// var logo = new UnityLogo
    /// {
    ///     variant = UnityLogoVariant.Animated,
    ///     size = IconSize.Lg
    /// };
    /// parentElement.Add(logo);
    /// </code>
    /// <para>The three variants in UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:UnityLogo /&gt;
    /// &lt;appui:UnityLogo variant="Empty" /&gt;
    /// &lt;appui:UnityLogo variant="Animated" size="Lg" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("iconography")]
    public partial class UnityLogo : BaseVisualElement
    {
        internal static readonly BindingId variantProperty = new BindingId(nameof(variant));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        /// <summary>
        /// The UnityLogo main styling class.
        /// </summary>
        public const string ussClassName = "appui-unity-logo";

        /// <summary>
        /// The UnityLogo variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(UnityLogoVariant))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The styling class of the Icon the logo is drawn with.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The icon name the logo asks for. The image itself is bound in UnityLogo.uss.
        /// </summary>
        internal const string logoIconName = "unity";

        /// <summary>
        /// The opacity of the dimmed, unfilled logo. Matches the web implementation, and is also where
        /// the animated variant starts each cycle.
        /// </summary>
        internal const float emptyOpacity = 0.3f;

        /// <summary>
        /// The duration of one full fill cycle, in milliseconds. Matches the web implementation's two
        /// second sweep.
        /// </summary>
        internal const float sweepPeriodMs = 2000f;

        readonly Icon m_Icon;

        UnityLogoVariant m_Variant;

        IVisualElementScheduledItem m_SweepItem;

        /// <summary>
        /// The <see cref="Icon"/> the logo is drawn with.
        /// </summary>
        public Icon icon => m_Icon;

        /// <summary>
        /// Whether the fill driver is currently running.
        /// </summary>
        internal bool isAnimating => m_SweepItem != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnityLogo()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;

            m_Icon = new Icon
            {
                primary = true,
                iconName = logoIconName,
                name = iconUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);

            variant = UnityLogoVariant.Filled;
            size = IconSize.Md;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// The fill state of the logo. <see cref="UnityLogoVariant.Filled"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public UnityLogoVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));
                UpdateDriver();

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// The size of the logo, on the shared icon sizing ramp. <see cref="IconSize.Md"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconSize size
        {
            get => m_Icon.size;
            set
            {
                var changed = m_Icon.size != value;
                m_Icon.size = value;

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The opacity of the fill at a given point in time.
        /// </summary>
        /// <remarks>
        /// A linear ramp from <see cref="emptyOpacity"/> to full over <see cref="sweepPeriodMs"/>, then
        /// back to empty. UITK cannot mask an image with a conic gradient, so the web implementation's
        /// radial sweep is approximated by fading the whole mark in over the same period.
        /// </remarks>
        /// <param name="timeSeconds"> The current time, in seconds. </param>
        /// <returns> The opacity to apply, between <see cref="emptyOpacity"/> and 1. </returns>
        internal static float ComputeSweepOpacity(float timeSeconds)
        {
            var phase = Mathf.Repeat(timeSeconds * 1000f, sweepPeriodMs) / sweepPeriodMs;
            return Mathf.Lerp(emptyOpacity, 1f, phase);
        }

        /// <summary>
        /// Advances the fill by one frame.
        /// </summary>
        internal void TickSweep()
        {
            m_Icon.style.opacity = ComputeSweepOpacity(TimeUtils.GetCurrentTime());
        }

        void OnAttachToPanel(AttachToPanelEvent _)
        {
            UpdateDriver();
        }

        void OnDetachFromPanel(DetachFromPanelEvent _)
        {
            StopDriver();
        }

        void UpdateDriver()
        {
            if (m_Variant == UnityLogoVariant.Animated && panel != null)
                m_SweepItem ??= schedule.Execute(TickSweep).Every(Styles.animationRefreshDelayMs);
            else
                StopDriver();
        }

        void StopDriver()
        {
            if (m_SweepItem == null)
                return;

            m_SweepItem.Pause();
            m_SweepItem = null;
            m_Icon.style.opacity = StyleKeyword.Null;
        }
    }
}
