using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A touch-optimized slider component for selecting floating-point values with support for step increments and
    /// custom formatting.
    /// </summary>
    /// <remarks>
    /// TouchSliderFloat is a touch-optimized slider component that allows users to select a floating-point value
    /// from a defined range. It supports both touch and keyboard interactions, making it suitable for touch devices
    /// while maintaining accessibility.
    ///
    /// The component features a visual track with a progress indicator, customizable step increments, shift-key
    /// modifier for larger steps, and support for value formatting. It can be oriented horizontally or vertically
    /// and adapts to RTL (Right-to-Left) layouts automatically.
    ///
    /// For optimal touch interaction, the slider defaults to a medium size that provides an adequately sized touch
    /// target. You can adjust the size using the size property to match your UI requirements.
    ///
    /// The slider supports both UXML definition and runtime instantiation, making it flexible for various UI
    /// development workflows.
    /// </remarks>
    /// <example>
    /// <para>Basic slider with default settings — creates a basic horizontal slider with default range (0-1) and initial
    /// value of 0.5.</para>
    /// <code lang="xml"><![CDATA[
    /// <TouchSliderFloat value="0.5" />
    /// ]]></code>
    /// <para>Customized slider with specific range and step values — creates a large slider with a range from -10 to 10,
    /// 0.5 step increments, 2.0 shift-step, and one decimal place formatting.</para>
    /// <code lang="xml"><![CDATA[
    /// <TouchSliderFloat
    ///     low-value="-10"
    ///     high-value="10"
    ///     value="0"
    ///     step="0.5"
    ///     shift-step="2.0"
    ///     format-string="F1"
    ///     size="L"
    /// />
    /// ]]></code>
    /// <para>Vertical slider with custom styling — creates a vertical slider with a range from 0 to 100 and custom
    /// styling class.</para>
    /// <code lang="xml"><![CDATA[
    /// <TouchSliderFloat
    ///     orientation="Vertical"
    ///     low-value="0"
    ///     high-value="100"
    ///     value="50"
    ///     class="custom-slider"
    /// />
    /// ]]></code>
    /// <para>Runtime instantiation and event handling — creates a slider in code, configures it to display values as
    /// percentages, and handles value changes.</para>
    /// <code lang="csharp"><![CDATA[
    /// var slider = new TouchSliderFloat();
    /// slider.lowValue = 0f;
    /// slider.highValue = 1f;
    /// slider.value = 0.5f;
    /// slider.formatString = "P0";
    /// slider.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"New value: {evt.newValue}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TouchSliderFloat : TouchSlider<float>
    {
        const float k_DefaultStep = 0.1f;

        const float k_DefaultShiftStep = 1f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TouchSliderFloat()
        {
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
            step = k_DefaultStep;
            shiftStep = k_DefaultShiftStep;
            lowValue = 0f;
            highValue = 1f;
            value = 0;
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
        protected override bool ParseStringToValue(string strValue, out float val)
        {
            var ret = UINumericFieldsUtils.StringToDouble(strValue, out var d);
            var f  = ret ? UINumericFieldsUtils.ClampToFloat(d) : value;
            val = f;
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
        protected override string ParseRawValueToString(float val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

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
