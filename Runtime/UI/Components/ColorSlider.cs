using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A slider component for selecting a color value from a gradient range.
    /// </summary>
    /// <remarks>
    /// The ColorSlider component enables users to select a color value by dragging a thumb along a track that
    /// displays a gradient. It extends the SliderFloat component and provides a visual way to pick colors from a
    /// defined color range.
    ///
    /// The slider displays a gradient track that can be customized to show different color transitions. Users
    /// can interact with the thumb to select any color value within the defined gradient range.
    ///
    /// Typical use cases include:
    /// - Color opacity/alpha selection
    /// - Color temperature adjustment
    /// - Color intensity control
    /// - Gradient-based value selection
    ///
    /// The component supports both mouse/touch input and keyboard navigation for precise color selection.
    /// </remarks>
    /// <example>
    /// <para>Basic color slider with default settings:
    /// Creating a basic color slider</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ColorSlider name="opacity-slider" />
    /// </UXML>
    ///
    /// <C#>
    /// var slider = new ColorSlider();
    /// container.Add(slider);
    /// ]]></code>
    /// <para>Color slider with custom gradient and value display:
    /// Creating a color slider with custom appearance and behavior</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ColorSlider
    ///     display-value-label="On"
    ///     track="On"
    ///     show-marks="true"
    ///     show-marks-label="true"
    ///     color-range="Fixed:[(0,#FF0000FF),(1,#00FF00FF)]+[(0,1),(1,1)]"
    /// />
    /// </UXML>
    /// ]]></code>
    /// <para>Color slider for alpha selection:
    /// Setting up a slider for opacity/alpha selection</para>
    /// <code lang="csharp"><![CDATA[
    /// var slider = new ColorSlider();
    /// var gradient = new Gradient();
    /// gradient.SetKeys(
    ///     new GradientColorKey[] {
    ///         new(Color.white, 0),
    ///         new(Color.white, 1)
    ///     },
    ///     new GradientAlphaKey[] {
    ///         new(0, 0),
    ///         new(1, 1)
    ///     }
    /// );
    /// slider.colorRange = gradient;
    /// slider.displayValueLabel = ValueDisplayMode.Auto;
    /// slider.showMarks = true;
    /// container.Add(slider);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public sealed partial class ColorSlider : SliderFloat
    {

        internal static readonly BindingId colorValueProperty = nameof(colorValue);

        internal static readonly BindingId colorRangeProperty = nameof(colorRange);


        /// <summary>
        /// The ColorSlider main styling class.
        /// </summary>
        public new const string ussClassName = "appui-color-slider";

        readonly ColorSwatch m_TrackSwatch;

        float m_IncrementFactor;

        /// <summary>
        /// The currently selected color value.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public Color colorValue => colorRange.Evaluate(m_Value);

        /// <summary>
        /// The current color range in the track.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Gradient colorRange
        {
            get => m_TrackSwatch.value;
            set
            {
                var changed = !m_TrackSwatch.value?.Equals(value) ?? value != null;
                m_TrackSwatch.value = value;
                SetValueWithoutNotify(this.value);

                if (changed)
                    NotifyPropertyChanged(in colorRangeProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorSlider()
        {
            AddToClassList(ussClassName);

            m_TrackSwatch = new ColorSwatch { pickingMode = PickingMode.Ignore };
            var g = new Gradient();
            g.SetKeys(new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0),
                new GradientColorKey(Color.red, 1),
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(0, 0),
                new GradientAlphaKey(1, 1),
            });
            m_TrackSwatch.SetValueWithoutNotify(g);

            m_TrackElement.Add(m_TrackSwatch);
            m_TrackSwatch.StretchToParentSize();

            lowValue = 0;
            highValue = 1f;
            step = 0.01f;
            shiftStep = 0.1f;
            value = 0;
        }

        /// <inheritdoc />
        protected override void InvokeValueChangedCallbacks()
        {
            base.InvokeValueChangedCallbacks();
            NotifyPropertyChanged(in colorValueProperty);
        }

        /// <inheritdoc />
        protected override void RefreshUI()
        {
            base.RefreshUI();
            if (m_ThumbsContainer is not {childCount: >0})
                return;

            m_TrackSwatch.orientation = orientation;
            var thumb = (Thumb)m_ThumbsContainer[0];
            thumb.fill = colorValue;
        }

    }
}
