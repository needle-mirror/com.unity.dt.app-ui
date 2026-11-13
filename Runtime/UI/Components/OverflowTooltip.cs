using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A truncating label that surfaces its full text in a <see cref="Tooltip"/>, but only while
    /// the text actually overflows.
    /// </summary>
    /// <remarks>
    /// A label that already fits gets no tooltip: repeating text the user can plainly read is
    /// noise, and it makes every genuinely truncated label harder to notice. The element measures
    /// itself against its own content box and only then publishes the tooltip.
    ///
    /// Overflow is re-measured whenever the box changes — a
    /// <see cref="GeometryChangedEvent"/> covers the container narrowing or the density changing —
    /// and whenever the text changes, since a rename can overflow the same box without moving it.
    /// The tooltip therefore appears and disappears live, and never goes stale.
    ///
    /// Use it for names in tight columns: grid view item labels, asset rows, breadcrumb segments.
    /// The truncation itself comes from USS (<c>text-overflow: ellipsis</c> over
    /// <c>white-space: nowrap</c>), so the element's width is whatever its parent gives it.
    /// </remarks>
    /// <example>
    /// <para>A label inside a fixed-width tile.</para>
    /// <code lang="csharp"><![CDATA[
    /// var tile = new VisualElement { style = { width = 120 } };
    /// tile.Add(new OverflowTooltip { text = asset.name });
    /// // Renaming re-measures on its own.
    /// label.text = asset.name;
    /// ]]></code>
    /// <para>Declaring one in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <appui:OverflowTooltip text="A very long asset name" placement="Top" />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups")]
    public partial class OverflowTooltip : LocalizedTextElement
    {
        internal static readonly BindingId placementProperty = nameof(placement);

        internal static readonly BindingId isOverflowingProperty = nameof(isOverflowing);

        /// <summary>
        /// The main styling class of the OverflowTooltip.
        /// </summary>
        public new const string ussClassName = "appui-overflow-tooltip";

        /// <summary>
        /// The styling class applied while the text is truncated.
        /// </summary>
        public const string overflowingUssClassName = ussClassName + "--overflowing";

        /// <summary>
        /// The default tooltip placement, matching the registry's <c>side</c> default.
        /// </summary>
        public const PopoverPlacement defaultPlacement = PopoverPlacement.Top;

        // A one-pixel slack: the measured text width and the content box are computed by different
        // paths and can disagree by a rounding step on a label that exactly fits.
        const float k_Slack = 1f;

        PopoverPlacement m_Placement = defaultPlacement;

        bool m_IsOverflowing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OverflowTooltip()
            : this(null) { }

        /// <summary>
        /// Constructor with a reference text.
        /// </summary>
        /// <param name="text">The reference text, or a Localization dictionary key.</param>
        public OverflowTooltip(string text)
            : base(text)
        {
            AddToClassList(ussClassName);
            AddToClassList(Text.ussClassName);
            this.SetPreferredTooltipPlacement(m_Placement);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// The reference text, or a Localization dictionary key.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="LocalizedTextElement.text"/> only to re-measure. A rename can
        /// overflow the same box without moving it, so <see cref="GeometryChangedEvent"/> alone
        /// would never fire. The measure is deferred one frame because the localized string
        /// resolves after the reference text is assigned.
        /// </remarks>
        public new string text
        {
            get => base.text;
            set
            {
                base.text = value;
                schedule.Execute(RefreshOverflow);
            }
        }

        /// <summary>
        /// Where the tooltip opens relative to the label. Defaults to
        /// <see cref="defaultPlacement"/>.
        /// </summary>
        [Tooltip("Where the tooltip opens relative to the label.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Overflow Tooltip")]
        public PopoverPlacement placement
        {
            get => m_Placement;
            set
            {
                var changed = m_Placement != value;
                m_Placement = value;
                this.SetPreferredTooltipPlacement(m_Placement);
                if (changed)
                    NotifyPropertyChanged(in placementProperty);
            }
        }

        /// <summary>
        /// `true` while the text does not fit in the element's content box, which is exactly when
        /// the tooltip is published.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public bool isOverflowing => m_IsOverflowing;

        /// <summary>
        /// Raised when <see cref="isOverflowing"/> flips, and only then. Lets a container react to
        /// a label becoming truncated without polling.
        /// </summary>
        public event Action<bool> overflowChanged;

        /// <summary>
        /// Re-measure the text against the content box and publish or withdraw the tooltip.
        /// Called automatically on layout and text changes; call it by hand only after changing
        /// something the element cannot observe, such as a font asset swapped underneath it.
        /// </summary>
        public void RefreshOverflow()
        {
            var box = contentRect.width;
            if (float.IsNaN(box) || box <= 0f)
                return;

            var full = localizedText;
            var overflowing = false;
            if (!string.IsNullOrEmpty(full))
            {
                var measured = MeasureTextSize(
                    full,
                    0f, MeasureMode.Undefined,
                    0f, MeasureMode.Undefined);
                overflowing = measured.x > box + k_Slack;
            }

            if (overflowing == m_IsOverflowing)
                return;

            m_IsOverflowing = overflowing;
            EnableInClassList(overflowingUssClassName, overflowing);
            tooltip = overflowing ? full : null;
            NotifyPropertyChanged(in isOverflowingProperty);
            overflowChanged?.Invoke(overflowing);
        }

        void OnGeometryChanged(GeometryChangedEvent evt) => RefreshOverflow();
    }
}
