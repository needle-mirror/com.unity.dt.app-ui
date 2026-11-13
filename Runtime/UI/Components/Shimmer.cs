using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A text element that brightens and dims on a slow loop to show that work is still in progress.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use Shimmer for a surface that has no determinate progress to report and where a spinner would
    /// overstate it, for instance the verb of a streaming chat response. It is a
    /// <see cref="LocalizedTextElement"/>, so it drops in wherever a label already sits and keeps its
    /// localization and its inherited font.
    /// </para>
    /// <para>
    /// The web implementation sweeps a gradient highlight through the glyphs with
    /// <c>background-clip: text</c>. USS has neither gradient backgrounds on text nor keyframe
    /// animations, so the UITK port keeps the intent rather than the mechanism: the text color travels
    /// between its resting value and <c>--shimmer-highlight-color</c> on the same two second loop. The
    /// driver is a scheduled item, the way <see cref="Skeleton"/> drives its pulse.
    /// </para>
    /// <para>
    /// Both ends of the loop come from the stylesheet: <c>--shimmer-base-color</c> is the resting
    /// colour and also what the element paints when it is not looping, and
    /// <c>--shimmer-highlight-color</c> is the peak. Restyle a Shimmer by overriding those, not
    /// <c>color</c>. Stopping the loop restores the resting colour rather than leaving it wherever
    /// the last frame put it. Set <see cref="active"/> to false for static text, for instance when
    /// honouring a reduced motion preference.
    /// </para>
    /// <para>
    /// The two second period is deliberately off the motion token scale, whose durations top out at
    /// 500ms because they size discrete transitions. A continuous ambient loop at that speed reads as
    /// a flicker.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>A label that shimmers while a response streams in.</para>
    /// <code lang="csharp">
    /// var status = new Shimmer { text = "Thinking" };
    /// parentElement.Add(status);
    ///
    /// // Once the work is done, settle it into static text.
    /// status.active = false;
    /// </code>
    /// <para>Static from the start.</para>
    /// <code lang="xml">
    /// &lt;appui:Shimmer text="Thinking" active="false" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class Shimmer : LocalizedTextElement
    {
        internal static readonly BindingId activeProperty = new BindingId(nameof(active));

        /// <summary>
        /// The Shimmer main styling class.
        /// </summary>
        public new const string ussClassName = "appui-shimmer";

        /// <summary>
        /// The styling class applied while the Shimmer is looping.
        /// </summary>
        public const string activeUssClassName = ussClassName + "--active";

        /// <summary>
        /// The duration of one full brighten-and-dim cycle, in milliseconds. Matches the web
        /// implementation's two second sweep.
        /// </summary>
        internal const float sweepPeriodMs = 2000f;

        static readonly CustomStyleProperty<Color> k_BaseColor =
            new CustomStyleProperty<Color>("--shimmer-base-color");

        static readonly CustomStyleProperty<Color> k_HighlightColor =
            new CustomStyleProperty<Color>("--shimmer-highlight-color");

        IVisualElementScheduledItem m_SweepItem;

        Color m_HighlightColor = Color.white;

        bool m_HasHighlightColor;

        /// <summary>
        /// The resting end of the loop, read from <c>--shimmer-base-color</c>.
        /// </summary>
        /// <remarks>
        /// Both ends come from the stylesheet rather than from <c>resolvedStyle.color</c>. The
        /// driver writes <c>style.color</c> every frame, and an inline colour participates in
        /// style resolution, so reading the resolved colour back would return the driver's own
        /// last write and the loop would chase its own tail and settle on the highlight. Taking
        /// both ends from custom properties is also how <see cref="AudioWaveform"/> and
        /// <c>CanvasEdge</c> get the colours they paint with: a colour C# can only read if the
        /// stylesheet hands it over.
        /// </remarks>
        Color m_BaseColor;

        bool m_HasBaseColor;

        /// <summary>
        /// Whether the sweep driver is currently running.
        /// </summary>
        internal bool isSweeping => m_SweepItem != null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Shimmer()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
            focusable = false;

            active = true;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>
        /// Construct a Shimmer with the given text.
        /// </summary>
        /// <param name="text"> The text to display. </param>
        public Shimmer(string text)
            : this()
        {
            this.text = text;
        }

        /// <summary>
        /// Whether the text loops between its resting color and the highlight. True by default.
        /// </summary>
        /// <remarks>
        /// Setting this to false stops the driver and restores the resting color.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool active
        {
            get => ClassListContains(activeUssClassName);
            set
            {
                var changed = ClassListContains(activeUssClassName) != value;
                EnableInClassList(activeUssClassName, value);
                UpdateDriver();

                if (changed)
                    NotifyPropertyChanged(in activeProperty);
            }
        }

        /// <summary>
        /// How far along the loop is at a given point in time.
        /// </summary>
        /// <remarks>
        /// A raised cosine over <see cref="sweepPeriodMs"/>: resting at the start of the cycle, fully
        /// highlighted at the middle, resting again at the end. This is the envelope of the web
        /// implementation's travelling gradient, which USS cannot express.
        /// </remarks>
        /// <param name="timeSeconds"> The current time, in seconds. </param>
        /// <returns> The blend factor, between 0 (resting) and 1 (highlighted). </returns>
        internal static float ComputeSweepBlend(float timeSeconds)
        {
            var phase = Mathf.Repeat(timeSeconds * 1000f, sweepPeriodMs) / sweepPeriodMs;
            return (1f - Mathf.Cos(phase * 2f * Mathf.PI)) * 0.5f;
        }

        /// <summary>
        /// Advances the loop by one frame.
        /// </summary>
        internal void TickSweep()
        {
            if (!m_HasBaseColor)
                return;

            var highlight = m_HasHighlightColor ? m_HighlightColor : m_BaseColor;
            style.color = Color.Lerp(m_BaseColor, highlight, ComputeSweepBlend(TimeUtils.GetCurrentTime()));
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            m_HasBaseColor = evt.customStyle.TryGetValue(k_BaseColor, out m_BaseColor);
            m_HasHighlightColor = evt.customStyle.TryGetValue(k_HighlightColor, out m_HighlightColor);
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
            if (active && panel != null)
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
            style.color = StyleKeyword.Null;
        }
    }
}
