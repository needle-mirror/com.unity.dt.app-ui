using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A loading placeholder that stands in for content while it is being fetched.
    /// </summary>
    /// <remarks>
    /// Skeleton is a plain, muted block that occupies the shape of the content it replaces. Give it the
    /// width and height of the thing that is loading, and stack several of them to sketch a paragraph,
    /// a card or a list row.
    ///
    /// By default it pulses, fading between full and half opacity over a two second cycle. USS has no
    /// keyframe animations, so the pulse is driven from C# by a scheduled item that writes the element's
    /// opacity every frame. The driver only runs while the Skeleton is attached to a panel and
    /// <see cref="animated"/> is true, and it restores full opacity when it stops. Set
    /// <see cref="animated"/> to false for a static placeholder, for instance when honouring a reduced
    /// motion preference.
    ///
    /// The Skeleton ignores pointer events, so it never steals input from the content around it.
    /// </remarks>
    /// <example>
    /// <para>A single line placeholder. The Skeleton takes the shape you give it.</para>
    /// <code lang="csharp">
    /// var skeleton = new Skeleton();
    /// skeleton.style.width = 200;
    /// skeleton.style.height = 12;
    /// parentElement.Add(skeleton);
    /// </code>
    /// <para>Sketching a paragraph out of several placeholders.</para>
    /// <code lang="csharp">
    /// var paragraph = new VisualElement();
    /// foreach (var width in new[] { 100f, 80f, 60f })
    /// {
    ///     var line = new Skeleton();
    ///     line.style.height = 12;
    ///     line.style.width = Length.Percent(width);
    ///     line.style.marginBottom = 6;
    ///     paragraph.Add(line);
    /// }
    /// </code>
    /// <para>A static placeholder, with no pulse.</para>
    /// <code lang="xml">
    /// &lt;appui:Skeleton animated="false" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class Skeleton : BaseVisualElement
    {
        internal static readonly BindingId animatedProperty = new BindingId(nameof(animated));

        /// <summary>
        /// The Skeleton main styling class.
        /// </summary>
        public const string ussClassName = "appui-skeleton";

        /// <summary>
        /// The styling class applied while the Skeleton pulses.
        /// </summary>
        public const string animatedUssClassName = ussClassName + "--animated";

        /// <summary>
        /// The duration of one full pulse cycle, in milliseconds. Matches the web implementation's
        /// two second <c>animate-pulse</c> cycle.
        /// </summary>
        internal const float pulsePeriodMs = 2000f;

        /// <summary>
        /// The opacity the pulse dips to at the middle of its cycle.
        /// </summary>
        internal const float minPulseOpacity = 0.5f;

        IVisualElementScheduledItem m_PulseItem;

        /// <summary>
        /// Whether the pulse driver is currently running.
        /// </summary>
        internal bool isPulsing => m_PulseItem != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Skeleton()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;

            animated = true;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Whether the Skeleton pulses. True by default.
        /// </summary>
        /// <remarks>
        /// Set this to false for a static placeholder. Stopping the pulse restores the Skeleton to full
        /// opacity rather than leaving it wherever the last frame put it.
        /// </remarks>
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
        /// The opacity of the pulse at a given point in time.
        /// </summary>
        /// <remarks>
        /// A raised cosine over <see cref="pulsePeriodMs"/>: full opacity at the start of the cycle,
        /// <see cref="minPulseOpacity"/> at the middle, full again at the end. This reproduces the
        /// envelope of the web implementation's <c>animate-pulse</c> keyframes, which USS cannot express.
        /// </remarks>
        /// <param name="timeSeconds"> The current time, in seconds. </param>
        /// <returns> The opacity to apply, between <see cref="minPulseOpacity"/> and 1. </returns>
        internal static float ComputePulseOpacity(float timeSeconds)
        {
            var phase = Mathf.Repeat(timeSeconds * 1000f, pulsePeriodMs) / pulsePeriodMs;
            var envelope = (Mathf.Cos(phase * 2f * Mathf.PI) + 1f) * 0.5f;
            return Mathf.Lerp(minPulseOpacity, 1f, envelope);
        }

        /// <summary>
        /// Advances the pulse by one frame.
        /// </summary>
        internal void TickPulse()
        {
            style.opacity = ComputePulseOpacity(TimeUtils.GetCurrentTime());
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
            style.opacity = StyleKeyword.Null;
        }
    }
}
