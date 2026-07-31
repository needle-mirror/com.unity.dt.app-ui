using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A slider component for selecting an integer value from a range.
    /// </summary>
    /// <remarks>
    /// The SliderInt component lets users select a value by moving a thumb control along a horizontal or vertical
    /// track. It's ideal for adjusting settings that have a fixed numerical range and where users benefit from
    /// visual feedback.
    ///
    /// The component supports various features like:
    /// - Customizable range with minimum and maximum values
    /// - Optional step values for incremental changes
    /// - Value display modes (always visible, on interaction, or hidden)
    /// - Customizable track appearance
    /// - Optional marks and labels along the track
    /// - Keyboard navigation and accessibility support
    /// - RTL (Right-to-Left) layout support
    ///
    /// The SliderInt only works with integer values. For decimal values, use SliderFloat instead.
    /// </remarks>
    /// <example>
    /// <para>Basic slider with default settings: Creates a basic horizontal slider with range 0-100 and initial value 50</para>
    /// <code lang="xml"><![CDATA[
    /// <Slider value="50" />
    /// ]]></code>
    /// <para>Slider with custom range and step: Creates a slider with range -50 to 50, step size 5, and visible marks</para>
    /// <code lang="xml"><![CDATA[
    /// <Slider low-value="-50" high-value="50" step="5" value="0" show-marks="true" />
    /// ]]></code>
    /// <para>Vertical slider with value label: Creates a vertical slider with always visible value label and track</para>
    /// <code lang="xml"><![CDATA[
    /// <Slider orientation="Vertical" display-value-label="On" track="On" />
    /// ]]></code>
    /// <para>Advanced slider with custom formatting: Creates a slider for selecting memory sizes with custom value
    /// formatting</para>
    /// <code lang="csharp"><![CDATA[
    /// var slider = new SliderInt {
    ///     value = 1024,
    ///     lowValue = 0,
    ///     highValue = 8192,
    ///     step = 1024,
    ///     showMarks = true,
    ///     displayValueLabel = ValueDisplayMode.On,
    ///     formatFunction = (v) => $"{v / 1024}MB"
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class SliderInt : Slider<int,int,IntField>
    {


        const int k_DefaultStep = 1;

        const int k_DefaultShiftStep = 10;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SliderInt()
        {
            formatStringOverride = UINumericFieldsUtils.k_IntFieldFormatString;
            stepOverride = k_DefaultStep;
            shiftStepOverride = k_DefaultShiftStep;
            lowValueOverride = 0;
            highValueOverride = 100;
            valueOverride = 0;
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

        [UxmlAttribute("format-string")]
        string formatStringOverride
        {
            get => formatString;
            set => formatString = value;
        }

        /// <inheritdoc />
        protected override int thumbCount => 1;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out int v)
        {
            var ret = int.TryParse(strValue, out var val);
            v = val;
            return ret;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(int val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            if (UINumericFieldsUtils.IsPercentFormatString(formatString))
                Debug.LogWarning("Percent format string is not supported for integer values.\n" +
                    "Please use a SliderFloat instead.");

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(int val) => ParseValueToString(val);

        /// <inheritdoc />
        protected override int SliderLerpUnclamped(int a, int b, float interpolant)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
        }

        /// <inheritdoc />
        protected override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
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
        protected override int GetValueFromScalarValues(Span<int> values)
        {
            return values[0];
        }

        /// <inheritdoc />
        protected override int ClampThumb(int x, int min, int max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc />
        protected override void GetScalarValuesFromValue(int v, Span<int> values)
        {
            values[0] = v;
        }

    }
}
