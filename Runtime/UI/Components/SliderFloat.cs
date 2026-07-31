using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A control that allows users to select a value from a continuous range.
    /// </summary>
    /// <remarks>
    /// The SliderFloat component is a user interface control that allows users to select a floating-point value
    /// from a continuous range by dragging a thumb along a track. It provides visual feedback through its track
    /// and thumb elements, making it intuitive for users to adjust values.
    ///
    /// The slider supports both horizontal and vertical orientations, and can be customized with step values,
    /// marks, value labels, and input field integration. It's particularly useful for scenarios where users need
    /// to adjust values like volume, opacity, size, or any other continuous numeric parameter.
    ///
    /// ### Features
    /// - Continuous value selection with floating-point precision
    /// - Customizable value range with minimum and maximum bounds
    /// - Optional step values for discrete increments
    /// - Support for custom marks and labels
    /// - Value display modes (always on, auto, or off)
    /// - Track display customization
    /// - Optional input field integration
    ///
    /// ### Best Practices
    /// - Use appropriate step values that make sense for your use case
    /// - Provide meaningful labels when using marks
    /// - Consider using value labels for better user feedback
    ///
    /// **Note:** The slider supports keyboard navigation using arrow keys, and the shift key can be used for
    /// larger step increments.
    /// </remarks>
    /// <example>
    /// <para>Basic slider with default settings — creates a basic horizontal slider with default range (0-100).</para>
    /// <code lang="xml"><![CDATA[
    /// <SliderFloat />
    /// ]]></code>
    /// <para>Customized slider with marks and labels — creates a slider for decimal values (0-1) with marks, labels, and
    /// visible track.</para>
    /// <code lang="xml"><![CDATA[
    /// <SliderFloat
    ///     low-value="0"
    ///     high-value="1"
    ///     step="0.1"
    ///     show-marks="true"
    ///     show-marks-label="true"
    ///     display-value-label="On"
    ///     track="On"
    /// />
    /// ]]></code>
    /// <para>Slider with custom scaling and formatting — creates a slider that uses exponential scaling (2^n) and
    /// formats values with KB units.</para>
    /// <code lang="csharp"><![CDATA[
    /// var slider = new SliderFloat {
    ///     scale = v => Mathf.Pow(2, v),
    ///     formatFunction = v => $"{v:F0} KB",
    ///     lowValue = 0,
    ///     highValue = 10,
    ///     step = 1
    /// };
    /// ]]></code>
    /// <para>Vertical slider with custom marks — creates a vertical slider with custom marks and labels at specific
    /// positions.</para>
    /// <code lang="csharp"><![CDATA[
    /// var slider = new SliderFloat {
    ///     orientation = Direction.Vertical,
    ///     customMarks = new List<SliderMark<float>> {
    ///         new() { value = 0, label = "Min" },
    ///         new() { value = 50, label = "Mid" },
    ///         new() { value = 100, label = "Max" }
    ///     },
    ///     showMarks = true,
    ///     showMarksLabel = true
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class SliderFloat : Slider<float,float,FloatField>
    {
        const float k_DefaultStep = 0.1f;

        const float k_DefaultShiftStep = 1f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SliderFloat()
        {
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
            step = k_DefaultStep;
            shiftStep = k_DefaultShiftStep;
            lowValueOverride = 0;
            highValueOverride = 100f;
            valueOverride = 0;
        }

        [UxmlAttribute("step")]
        float stepOverride
        {
            get => step;
            set => step = value;
        }

        [UxmlAttribute("shift-step")]
        float shiftStepOverride
        {
            get => shiftStep;
            set => shiftStep = value;
        }

        [UxmlAttribute("low-value")]
        float lowValueOverride
        {
            get => lowValue;
            set => lowValue = value;
        }

        [UxmlAttribute("high-value")]
        float highValueOverride
        {
            get => highValue;
            set => highValue = value;
        }

        [UxmlAttribute("value")]
        float valueOverride
        {
            get => value;
            set => this.value = value;
        }

        /// <inheritdoc />
        protected override int thumbCount => 1;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out float v)
        {
            var ret = float.TryParse(strValue, out var val);
            v = val;
            return ret;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(float val)
        {
            return formatFunction != null
                ? formatFunction(val)
                : val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(float val) => ParseValueToString(val);

        /// <inheritdoc />
        protected override float SliderLerpUnclamped(float a, float b, float interpolant)
        {
            return Mathf.LerpUnclamped(a, b, interpolant);
        }

        /// <inheritdoc />
        protected override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc />
        protected override float Mad(int m, float a, float b)
        {
            return m * a + b;
        }

        /// <inheritdoc />
        protected override int GetStepCount(float stepValue)
        {
            return Mathf.FloorToInt((highValue - lowValue) / stepValue) + 1;
        }

        /// <inheritdoc />
        protected override float ClampThumb(float x, float min, float max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc />
        protected override float GetValueFromScalarValues(Span<float> values)
        {
            return values[0];
        }

        /// <inheritdoc />
        protected override void GetScalarValuesFromValue(float v, Span<float> values)
        {
            values[0] = v;
        }

    }
}
