using System;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Three staggered dots standing in for a value that is not known yet.
    /// </summary>
    /// <remarks>
    /// A compact inline "still loading" affordance, used by <see cref="CreditsPill"/> while the balance
    /// is being fetched and by conversation titles while a suggestion is being generated.
    ///
    /// USS has no keyframe animations, so the pulse is driven from C# the same way
    /// <see cref="Skeleton"/> drives its own: a scheduled item writes each dot's opacity every frame,
    /// runs only while the element is attached to a panel and <see cref="animated"/> is true, and
    /// restores full opacity when it stops. The three dots share one cycle, offset by
    /// <see cref="dotDelayMs"/> each, which is what makes them read as a wave rather than a blink.
    /// </remarks>
    /// <example>
    /// <para>Standing in for a number.</para>
    /// <code lang="csharp">
    /// var dots = new LoadingDots();
    /// parentElement.Add(dots);
    /// </code>
    /// <para>A static version, for a reduced motion preference.</para>
    /// <code lang="xml">
    /// &lt;appui:LoadingDots animated="false" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class LoadingDots : BaseVisualElement
    {
        internal static readonly BindingId animatedProperty = nameof(animated);

        /// <summary>
        /// The LoadingDots main styling class.
        /// </summary>
        public const string ussClassName = "appui-loading-dots";

        /// <summary>
        /// The styling class applied while the dots pulse.
        /// </summary>
        public const string animatedUssClassName = ussClassName + "--animated";

        /// <summary>
        /// The styling class of one dot.
        /// </summary>
        public const string dotUssClassName = ussClassName + "__dot";

        /// <summary>
        /// How many dots there are.
        /// </summary>
        public const int dotCount = 3;

        /// <summary>
        /// The duration of one full pulse cycle, in milliseconds. Matches the web implementation's two
        /// second <c>animate-pulse</c> cycle.
        /// </summary>
        internal const float pulsePeriodMs = 2000f;

        /// <summary>
        /// How far each dot lags the one before it, in milliseconds. Matches the web implementation's
        /// 0 / 200 / 400 animation delays.
        /// </summary>
        internal const float dotDelayMs = 200f;

        /// <summary>
        /// The opacity the pulse dips to at the middle of its cycle.
        /// </summary>
        internal const float minPulseOpacity = 0.5f;

        readonly VisualElement[] m_Dots = new VisualElement[dotCount];

        IVisualElementScheduledItem m_PulseItem;

        /// <summary>
        /// Whether the pulse driver is currently running.
        /// </summary>
        internal bool isPulsing => m_PulseItem != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LoadingDots()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;

            for (var i = 0; i < dotCount; i++)
            {
                var dot = new VisualElement { name = dotUssClassName, pickingMode = PickingMode.Ignore };
                dot.AddToClassList(dotUssClassName);
                // USS has no :nth-child, so the leading dot is marked explicitly.
                if (i == 0)
                    dot.AddToClassList(Styles.firstChildUssClassName);
                m_Dots[i] = dot;
                hierarchy.Add(dot);
            }

            animated = true;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Whether the dots pulse. True by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool animated
        {
            get => ClassListContains(animatedUssClassName);
            set
            {
                var changed = ClassListContains(animatedUssClassName) != value;
                EnableInClassList(animatedUssClassName, value);
                UpdateDriver();

                if (changed)
                    NotifyPropertyChanged(in animatedProperty);
            }
        }

        /// <summary>
        /// The opacity of one dot at a given point in time.
        /// </summary>
        /// <remarks>
        /// The same raised cosine <see cref="Skeleton"/> uses, shifted back by the dot's own delay so
        /// the three dots trail one another.
        /// </remarks>
        /// <param name="timeSeconds"> The current time, in seconds. </param>
        /// <param name="dotIndex"> Which dot, from 0 to <see cref="dotCount"/> - 1. </param>
        /// <returns> The opacity to apply, between <see cref="minPulseOpacity"/> and 1. </returns>
        internal static float ComputeDotOpacity(float timeSeconds, int dotIndex)
        {
            var shiftedMs = timeSeconds * 1000f - dotIndex * dotDelayMs;
            var phase = Mathf.Repeat(shiftedMs, pulsePeriodMs) / pulsePeriodMs;
            var envelope = (Mathf.Cos(phase * 2f * Mathf.PI) + 1f) * 0.5f;
            return Mathf.Lerp(minPulseOpacity, 1f, envelope);
        }

        /// <summary>
        /// Advances the pulse by one frame.
        /// </summary>
        internal void TickPulse()
        {
            var time = TimeUtils.GetCurrentTime();
            for (var i = 0; i < dotCount; i++)
                m_Dots[i].style.opacity = ComputeDotOpacity(time, i);
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
            if (animated && panel != null)
                m_PulseItem ??= schedule.Execute(TickPulse).Every(Styles.animationRefreshDelayMs);
            else
                StopDriver();
        }

        void StopDriver()
        {
            if (m_PulseItem == null)
                return;

            m_PulseItem.Pause();
            m_PulseItem = null;
            for (var i = 0; i < dotCount; i++)
                m_Dots[i].style.opacity = StyleKeyword.Null;
        }
    }

    /// <summary>
    /// How much of a spend budget a conversation has used, as a labelled bar.
    /// </summary>
    /// <remarks>
    /// A header line pairs the "Budget" label with the raw "spent / total unit" reading, and a
    /// <see cref="LinearProgress"/> underneath fills to the same ratio. Once <see cref="spent"/>
    /// reaches <see cref="total"/> the whole thing turns destructive, so an exhausted budget cannot be
    /// mistaken for a full one.
    ///
    /// The bar is the package's own progress component rather than a bespoke fill, so it inherits the
    /// rounded-corner and density behaviour of every other progress in App UI.
    /// </remarks>
    /// <example>
    /// <para>A budget just over halfway through.</para>
    /// <code lang="csharp">
    /// var budget = new ChatBudget { spent = 120, total = 200 };
    /// parentElement.Add(budget);
    /// </code>
    /// <para>In UXML, with a custom unit.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatBudget spent="200" total="200" unit="credits" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatBudget : BaseVisualElement
    {
        internal static readonly BindingId spentProperty = nameof(spent);

        internal static readonly BindingId totalProperty = nameof(total);

        internal static readonly BindingId unitProperty = nameof(unit);

        /// <summary>
        /// The ChatBudget main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-budget";

        /// <summary>
        /// The styling class applied once the budget is used up.
        /// </summary>
        public const string exhaustedUssClassName = ussClassName + "--exhausted";

        /// <summary>
        /// The styling class of the header line.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the header's leading glyph.
        /// </summary>
        public const string headerIconUssClassName = ussClassName + "__header-icon";

        /// <summary>
        /// The styling class of the "Budget" label.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The styling class of the "spent / total unit" reading.
        /// </summary>
        public const string readingUssClassName = ussClassName + "__reading";

        /// <summary>
        /// The styling class of the bar.
        /// </summary>
        public const string barUssClassName = ussClassName + "__bar";

        /// <summary>
        /// The default unit suffix.
        /// </summary>
        public const string defaultUnit = "pts";

        /// <summary>
        /// The header label.
        /// </summary>
        public const string budgetLabel = "Budget";

        readonly LocalizedTextElement m_Reading;

        readonly LinearProgress m_Bar;

        float m_Spent;

        float m_Total = 1f;

        string m_Unit = defaultUnit;

        /// <summary>
        /// The bar the budget fills.
        /// </summary>
        public LinearProgress bar => m_Bar;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatBudget()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            var header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore };
            header.AddToClassList(headerUssClassName);

            var icon = new Icon
            {
                name = headerIconUssClassName,
                iconName = "sparkle",
                size = IconSize.Xs,
                pickingMode = PickingMode.Ignore,
            };
            icon.AddToClassList(headerIconUssClassName);

            var label = new LocalizedTextElement
            {
                name = labelUssClassName,
                text = budgetLabel,
                pickingMode = PickingMode.Ignore,
            };
            label.AddToClassList(labelUssClassName);

            m_Reading = new LocalizedTextElement { name = readingUssClassName, pickingMode = PickingMode.Ignore };
            m_Reading.AddToClassList(readingUssClassName);

            header.hierarchy.Add(icon);
            header.hierarchy.Add(label);
            header.hierarchy.Add(m_Reading);

            m_Bar = new LinearProgress
            {
                name = barUssClassName,
                variant = Progress.Variant.Determinate,
                roundedProgressCorners = true,
                pickingMode = PickingMode.Ignore,
            };
            m_Bar.AddToClassList(barUssClassName);

            hierarchy.Add(header);
            hierarchy.Add(m_Bar);

            Refresh();
        }

        /// <summary>
        /// How much of the budget has been spent.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float spent
        {
            get => m_Spent;
            set
            {
                var changed = !Mathf.Approximately(m_Spent, value);
                m_Spent = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in spentProperty);
            }
        }

        /// <summary>
        /// The size of the budget.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float total
        {
            get => m_Total;
            set
            {
                var changed = !Mathf.Approximately(m_Total, value);
                m_Total = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in totalProperty);
            }
        }

        /// <summary>
        /// The unit suffix on the reading. "pts" by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string unit
        {
            get => m_Unit;
            set
            {
                var changed = m_Unit != value;
                m_Unit = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in unitProperty);
            }
        }

        /// <summary>
        /// How full the budget is, from 0 to 1. A non-positive <see cref="total"/> reads as empty
        /// rather than dividing by zero.
        /// </summary>
        public float progress => m_Total > 0f ? Mathf.Clamp01(m_Spent / m_Total) : 0f;

        /// <summary>
        /// Whether the budget is used up.
        /// </summary>
        public bool exhausted => m_Total > 0f && m_Spent >= m_Total;

        /// <summary>
        /// The reading the header currently shows.
        /// </summary>
        public string reading => m_Reading.text;

        void Refresh()
        {
            m_Bar.value = progress;
            m_Reading.text = ChatFormat.Plain(m_Spent) + " / " + ChatFormat.Plain(m_Total) +
                (string.IsNullOrEmpty(m_Unit) ? string.Empty : " " + m_Unit);
            EnableInClassList(exhaustedUssClassName, exhausted);
        }
    }

    /// <summary>
    /// A titlebar badge showing an AI credit balance.
    /// </summary>
    /// <remarks>
    /// The number is shown compactly, for instance "51k Credits", with the exact figure in the tooltip.
    /// <see cref="loading"/> swaps it for a <see cref="LoadingDots"/> while the balance is being
    /// fetched, and <see cref="error"/> swaps it for a warning glyph with
    /// <see cref="errorMessage"/> as the tooltip. Loading wins over error: there is nothing to report
    /// about a request that has not finished.
    ///
    /// <see cref="tooltipLabel"/> replaces the exact-balance tooltip, as the registry's <c>tooltip</c>
    /// prop does, and a balance update leaves it alone.
    /// </remarks>
    /// <example>
    /// <para>A resolved balance.</para>
    /// <code lang="csharp">
    /// var pill = new CreditsPill { credits = 51200 };
    /// parentElement.Add(pill);
    /// </code>
    /// <para>The two unresolved states, in UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:CreditsPill loading="true" /&gt;
    /// &lt;appui:CreditsPill error="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class CreditsPill : BaseVisualElement
    {
        internal static readonly BindingId creditsProperty = nameof(credits);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId prefixProperty = nameof(prefix);

        internal static readonly BindingId loadingProperty = nameof(loading);

        internal static readonly BindingId errorProperty = nameof(error);

        internal static readonly BindingId errorMessageProperty = nameof(errorMessage);

        internal static readonly BindingId tooltipLabelProperty = nameof(tooltipLabel);

        /// <summary>
        /// The CreditsPill main styling class.
        /// </summary>
        public const string ussClassName = "appui-credits-pill";

        /// <summary>
        /// The styling class of the leading glyph.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The styling class of the balance text.
        /// </summary>
        public const string valueUssClassName = ussClassName + "__value";

        /// <summary>
        /// The styling class of the loading dots.
        /// </summary>
        public const string dotsUssClassName = ussClassName + "__dots";

        /// <summary>
        /// The styling class of the warning glyph.
        /// </summary>
        public const string warningUssClassName = ussClassName + "__warning";

        /// <summary>
        /// The default trailing label.
        /// </summary>
        public const string defaultLabel = "Credits";

        /// <summary>
        /// The default explanation shown when the balance could not be fetched.
        /// </summary>
        public const string defaultErrorMessage = "Couldn't load your credit balance. Check your connection.";

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_Value;

        readonly LoadingDots m_Dots;

        readonly Icon m_Warning;

        float m_Credits;

        string m_Label = defaultLabel;

        string m_Prefix;

        bool m_Loading;

        bool m_Error;

        string m_ErrorMessage = defaultErrorMessage;

        string m_TooltipLabel;

        string m_WrittenTooltip;

        /// <summary>
        /// The leading glyph. Retarget its <see cref="Icon.iconName"/> to front the pill with something
        /// other than the sparkle.
        /// </summary>
        public Icon icon => m_Icon;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CreditsPill()
        {
            AddToClassList(ussClassName);

            m_Icon = new Icon
            {
                name = iconUssClassName,
                iconName = "sparkle",
                size = IconSize.Xs,
                pickingMode = PickingMode.Ignore,
            };
            m_Icon.AddToClassList(iconUssClassName);

            m_Value = new LocalizedTextElement { name = valueUssClassName, pickingMode = PickingMode.Ignore };
            m_Value.AddToClassList(valueUssClassName);

            m_Dots = new LoadingDots { name = dotsUssClassName };
            m_Dots.AddToClassList(dotsUssClassName);

            m_Warning = new Icon
            {
                name = warningUssClassName,
                iconName = "warning",
                size = IconSize.Xs,
                pickingMode = PickingMode.Ignore,
            };
            m_Warning.AddToClassList(warningUssClassName);

            hierarchy.Add(m_Icon);
            hierarchy.Add(m_Value);
            hierarchy.Add(m_Dots);
            hierarchy.Add(m_Warning);

            Refresh();
        }

        /// <summary>
        /// The credit balance.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float credits
        {
            get => m_Credits;
            set
            {
                var changed = !Mathf.Approximately(m_Credits, value);
                m_Credits = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in creditsProperty);
            }
        }

        /// <summary>
        /// The trailing label after the number. "Credits" by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label;
            set
            {
                var changed = m_Label != value;
                m_Label = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// An optional prefix glued to the number, for instance "+".
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string prefix
        {
            get => m_Prefix;
            set
            {
                var changed = m_Prefix != value;
                m_Prefix = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in prefixProperty);
            }
        }

        /// <summary>
        /// Whether the balance is still being fetched. Takes precedence over <see cref="error"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool loading
        {
            get => m_Loading;
            set
            {
                var changed = m_Loading != value;
                m_Loading = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in loadingProperty);
            }
        }

        /// <summary>
        /// Whether the balance could not be fetched.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool error
        {
            get => m_Error;
            set
            {
                var changed = m_Error != value;
                m_Error = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in errorProperty);
            }
        }

        /// <summary>
        /// Why the balance could not be fetched. Shown as the tooltip while <see cref="error"/> is set.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string errorMessage
        {
            get => m_ErrorMessage;
            set
            {
                var changed = m_ErrorMessage != value;
                m_ErrorMessage = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in errorMessageProperty);
            }
        }

        /// <summary>
        /// Replaces the exact-balance tooltip while the balance is shown. Null or empty, the default,
        /// shows the exact balance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The registry's <c>tooltip</c> prop. It cannot be named <c>tooltip</c> here, since that is
        /// <see cref="VisualElement.tooltip"/>, which the pill writes on every refresh; it is named
        /// after <see cref="BudgetIndicator.tooltipLabel"/>, its sibling's override.
        /// </para>
        /// <para>
        /// The registry's precedence holds: the error message still takes its place while
        /// <see cref="error"/> is set, and loading shows no tooltip at all. A value written straight to
        /// <see cref="VisualElement.tooltip"/> is not lost either: the next refresh adopts it here
        /// rather than writing over it.
        /// </para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string tooltipLabel
        {
            get => m_TooltipLabel;
            set
            {
                var next = string.IsNullOrEmpty(value) ? null : value;
                var changed = m_TooltipLabel != next;
                m_TooltipLabel = next;
                // The latest assignment wins: a direct tooltip write still pending from before this one
                // is superseded, not adopted over it.
                m_WrittenTooltip = tooltip;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in tooltipLabelProperty);
            }
        }

        void Refresh()
        {
            var showDots = m_Loading;
            var showWarning = !m_Loading && m_Error;
            var showValue = !showDots && !showWarning;

            m_Dots.EnableInClassList(Styles.hiddenUssClassName, !showDots);
            m_Warning.EnableInClassList(Styles.hiddenUssClassName, !showWarning);
            m_Value.EnableInClassList(Styles.hiddenUssClassName, !showValue);

            m_Value.text = showValue
                ? m_Prefix + ChatFormat.Compact(m_Credits) +
                    (string.IsNullOrEmpty(m_Label) ? string.Empty : " " + m_Label)
                : null;

            // A write straight to VisualElement.tooltip cannot be observed as it happens (the pill is no
            // INotifyBindablePropertyChanged), so it is only seen here, as a value that differs from
            // what the pill last wrote. Adopt it as the override instead of writing over it; clearing
            // it brings the exact balance back.
            if (!string.Equals(tooltip, m_WrittenTooltip, StringComparison.Ordinal))
            {
                var adopted = string.IsNullOrEmpty(tooltip) ? null : tooltip;
                if (m_TooltipLabel != adopted)
                {
                    m_TooltipLabel = adopted;
                    NotifyPropertyChanged(in tooltipLabelProperty);
                }
            }

            // Nothing exact to reveal while the request is still out, and the error explains itself
            // even over a consumer's tooltip, as in the registry.
            tooltip = showDots
                ? null
                : showWarning
                    ? m_ErrorMessage
                    : m_TooltipLabel ?? m_Prefix + ChatFormat.Grouped(m_Credits) +
                        (string.IsNullOrEmpty(m_Label) ? string.Empty : " " + m_Label);
            m_WrittenTooltip = tooltip;
        }
    }

    /// <summary>
    /// The same budget as <see cref="ChatBudget"/>, compressed into a titlebar-sized readout.
    /// </summary>
    /// <remarks>
    /// Shows the amount spent, the percentage, and a ring that fills to match, with the full
    /// "spent / total unit" reading in the tooltip. A non-positive <see cref="total"/> hides the whole
    /// indicator: there is no budget to report on.
    ///
    /// The ring is a determinate <see cref="CircularProgress"/>, so it is the package's own ring rather
    /// than a second one drawn by hand.
    /// </remarks>
    /// <example>
    /// <para>A titlebar readout.</para>
    /// <code lang="csharp">
    /// var indicator = new BudgetIndicator { spent = 40, total = 200 };
    /// parentElement.Add(indicator);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:BudgetIndicator spent="200" total="200" unit="pts" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class BudgetIndicator : BaseVisualElement
    {
        internal static readonly BindingId spentProperty = nameof(spent);

        internal static readonly BindingId totalProperty = nameof(total);

        internal static readonly BindingId unitProperty = nameof(unit);

        internal static readonly BindingId tooltipLabelProperty = nameof(tooltipLabel);

        /// <summary>
        /// The BudgetIndicator main styling class.
        /// </summary>
        public const string ussClassName = "appui-budget-indicator";

        /// <summary>
        /// The styling class applied once the budget is used up.
        /// </summary>
        public const string exhaustedUssClassName = ussClassName + "--exhausted";

        /// <summary>
        /// The styling class of the spent amount.
        /// </summary>
        public const string amountUssClassName = ussClassName + "__amount";

        /// <summary>
        /// The styling class of the percentage.
        /// </summary>
        public const string percentUssClassName = ussClassName + "__percent";

        /// <summary>
        /// The styling class of the ring.
        /// </summary>
        public const string ringUssClassName = ussClassName + "__ring";

        readonly LocalizedTextElement m_Amount;

        readonly LocalizedTextElement m_Percent;

        readonly CircularProgress m_Ring;

        float m_Spent;

        float m_Total = 1f;

        string m_Unit = ChatBudget.defaultUnit;

        string m_TooltipLabel;

        /// <summary>
        /// The ring the budget fills.
        /// </summary>
        public CircularProgress ring => m_Ring;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BudgetIndicator()
        {
            AddToClassList(ussClassName);

            m_Amount = new LocalizedTextElement { name = amountUssClassName, pickingMode = PickingMode.Ignore };
            m_Amount.AddToClassList(amountUssClassName);

            m_Percent = new LocalizedTextElement { name = percentUssClassName, pickingMode = PickingMode.Ignore };
            m_Percent.AddToClassList(percentUssClassName);

            m_Ring = new CircularProgress
            {
                name = ringUssClassName,
                variant = Progress.Variant.Determinate,
                size = Size.Sm,
                pickingMode = PickingMode.Ignore,
            };
            m_Ring.AddToClassList(ringUssClassName);

            hierarchy.Add(m_Amount);
            hierarchy.Add(m_Percent);
            hierarchy.Add(m_Ring);

            Refresh();
        }

        /// <summary>
        /// How much of the budget has been spent.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float spent
        {
            get => m_Spent;
            set
            {
                var changed = !Mathf.Approximately(m_Spent, value);
                m_Spent = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in spentProperty);
            }
        }

        /// <summary>
        /// The size of the budget. Non-positive hides the indicator.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float total
        {
            get => m_Total;
            set
            {
                var changed = !Mathf.Approximately(m_Total, value);
                m_Total = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in totalProperty);
            }
        }

        /// <summary>
        /// The unit suffix. "pts" by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string unit
        {
            get => m_Unit;
            set
            {
                var changed = m_Unit != value;
                m_Unit = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in unitProperty);
            }
        }

        /// <summary>
        /// Replaces the default "spent / total unit" tooltip.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string tooltipLabel
        {
            get => m_TooltipLabel;
            set
            {
                var changed = m_TooltipLabel != value;
                m_TooltipLabel = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in tooltipLabelProperty);
            }
        }

        /// <summary>
        /// How full the budget is, from 0 to 1.
        /// </summary>
        public float progress => m_Total > 0f ? Mathf.Clamp01(m_Spent / m_Total) : 0f;

        /// <summary>
        /// How full the budget is, as a whole percentage.
        /// </summary>
        /// <remarks>
        /// Rounded half up, like the registry's <c>Math.round</c>. <c>Mathf.RoundToInt</c> rounds a
        /// midpoint to even, so 20.5% would report 20 there and 21 on the web.
        /// </remarks>
        public int percent => Mathf.FloorToInt(progress * 100f + 0.5f);

        /// <summary>
        /// Whether the budget is used up.
        /// </summary>
        public bool exhausted => m_Total > 0f && m_Spent >= m_Total;

        void Refresh()
        {
            // No budget, nothing to report.
            EnableInClassList(Styles.hiddenUssClassName, m_Total <= 0f);

            m_Ring.value = progress;
            m_Amount.text = ChatFormat.Plain(m_Spent) + (string.IsNullOrEmpty(m_Unit) ? string.Empty : " " + m_Unit);
            m_Percent.text = percent.ToString(CultureInfo.InvariantCulture) + "%";
            EnableInClassList(exhaustedUssClassName, exhausted);

            tooltip = string.IsNullOrEmpty(m_TooltipLabel)
                ? ChatFormat.Plain(m_Spent) + " / " + ChatFormat.Plain(m_Total) +
                    (string.IsNullOrEmpty(m_Unit) ? string.Empty : " " + m_Unit)
                : m_TooltipLabel;
        }
    }
}
