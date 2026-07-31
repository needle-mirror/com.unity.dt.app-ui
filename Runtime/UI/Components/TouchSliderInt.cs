using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A touch-optimized slider component for selecting integer values within a specified range.
    /// </summary>
    /// <remarks>
    /// TouchSliderInt is a user interface control that allows users to select an integer value from a
    /// continuous range by dragging a thumb along a track. It is specifically designed for touch interactions
    /// while maintaining full keyboard and mouse support.
    ///
    /// The slider provides visual feedback through a progress bar that fills according to the current value.
    /// It supports both horizontal and vertical orientations, and automatically adapts to RTL (Right-to-Left)
    /// layouts.
    ///
    /// The component includes a label that can display units or descriptions, and a value display that shows
    /// the current selection. Users can directly edit the value through a text input field that appears when
    /// clicking on the value display.
    ///
    /// For performance reasons, it's recommended to set appropriate step values to limit the number of possible
    /// values in very large ranges.
    /// </remarks>
    /// <example>
    /// <para>Basic usage of TouchSliderInt in UXML — creating an age selector slider in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <TouchSliderInt name="ageSlider"
    ///              low-value="0"
    ///              high-value="100"
    ///              value="25"
    ///              label="years"
    ///              step="1"
    ///              shift-step="5"
    ///              size="M" />
    /// ]]></code>
    /// <para>Creating and configuring TouchSliderInt in C# — creating a volume control slider programmatically.</para>
    /// <code lang="csharp"><![CDATA[
    /// var volumeSlider = new TouchSliderInt
    /// {
    ///     lowValue = 0,
    ///     highValue = 100,
    ///     value = 50,
    ///     label = "%",
    ///     step = 5,
    ///     shiftStep = 10,
    ///     size = Size.M
    /// };
    ///
    /// // Add value change listener
    /// volumeSlider.RegisterValueChangedCallback(evt =>
    /// {
    ///     Debug.Log($"Volume changed from {evt.previousValue} to {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>Creating a vertical slider with custom styling — creating a height measurement slider.</para>
    /// <code lang="csharp"><![CDATA[
    /// var heightSlider = new TouchSliderInt
    /// {
    ///     orientation = Direction.Vertical,
    ///     lowValue = 0,
    ///     highValue = 200,
    ///     value = 170,
    ///     label = "cm",
    ///     step = 1,
    ///     shiftStep = 10
    /// };
    ///
    /// // Add custom styling
    /// heightSlider.AddToClassList("custom-slider");
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TouchSliderInt : TouchSlider<int>
    {
        const int k_DefaultStep = 1;

        const int k_DefaultShiftStep = 10;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TouchSliderInt()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            step = k_DefaultStep;
            shiftStep = k_DefaultShiftStep;
            lowValue = 0;
            highValue = 1;
            value = 0;
        }

        [UxmlAttribute("step")]
        int stepOverride
        {
            get => step;
            set => step = value;
        }

        [UxmlAttribute("shift-step")]
        int shiftStepOverride
        {
            get => shiftStep;
            set => shiftStep = value;
        }

        [UxmlAttribute("low-value")]
        int lowValueOverride
        {
            get => lowValue;
            set => lowValue = value;
        }

        [UxmlAttribute("high-value")]
        int highValueOverride
        {
            get => highValue;
            set => highValue = value;
        }

        [UxmlAttribute("value")]
        int valueOverride
        {
            get => value;
            set => this.value = value;
        }

        /// <inheritdoc />
        protected override int thumbCount => 1;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out int val)
        {
            var ret = UINumericFieldsUtils.StringToLong(strValue, out var v);
            val = ret ? UINumericFieldsUtils.ClampToInt(v) : value;
            return ret;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(int val)
        {
            return formatFunction != null
                ? formatFunction(val)
                : val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(int val) => ParseValueToString(val);

        /// <inheritdoc />
        protected override string ParseRawValueToString(int val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override int SliderLerpUnclamped(int a, int b, float interpolant)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
        }

        /// <inheritdoc />
        protected override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.InverseLerp(lowerValue,higherValue, currentValue);
        }

        /// <inheritdoc />
        protected override int Mad(int m, int a, int b)
        {
            return m * a + b;
        }

        /// <inheritdoc />
        protected override int GetStepCount(int stepValue)
        {
            return (highValue - lowValue) / stepValue + 1;
        }

        /// <inheritdoc />
        protected override int ClampThumb(int x, int min, int max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc />
        protected override int GetValueFromScalarValues(Span<int> values)
        {
            return values[0];
        }

        /// <inheritdoc />
        protected override void GetScalarValuesFromValue(int v, Span<int> values)
        {
            values[0] = v;
        }

    }

}
