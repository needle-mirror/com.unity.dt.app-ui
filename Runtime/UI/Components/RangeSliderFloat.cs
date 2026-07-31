using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A simple comparer for Vector2 that compares the x and y values lexicographically.
    /// </summary>
    public class Vector2LexicographicalComparer : IComparer<Vector2>
    {
        /// <summary>
        /// Compares two Vector2 values lexicographically.
        /// </summary>
        /// <param name="v1"> The first Vector2 value to compare.</param>
        /// <param name="v2"> The second Vector2 value to compare.</param>
        /// <returns> A value indicating the relative order of the two Vector2 values.</returns>
        public int Compare(Vector2 v1, Vector2 v2)
        {
            var xComparison = v1.x.CompareTo(v2.x);
            return xComparison != 0 ? xComparison : v1.y.CompareTo(v2.y);
        }
    }

    /// <summary>
    /// A dual-thumb slider component for selecting a range of floating point values.
    /// </summary>
    /// <remarks>
    /// The RangeSliderFloat is an input component that allows users to select a range between two numeric
    /// values by dragging two thumbs along a track. It's particularly useful when users need to define minimum
    /// and maximum bounds within a larger range of values.
    ///
    /// The slider supports both horizontal and vertical orientations, customizable step increments, and
    /// various visual feedback options like marks, labels, and track highlighting.
    ///
    /// The slider automatically handles RTL (Right-to-Left) layouts and will reverse its direction accordingly
    /// when in RTL context.
    /// </remarks>
    /// <example>
    /// <para>Basic range slider setup with UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    ///   <ui:RangeSliderFloat
    ///     low-value="0"
    ///     high-value="100"
    ///     min-value="20"
    ///     max-value="80"
    ///     step="5"
    ///     show-marks="true"
    ///     display-value-label="Auto"
    ///     track="On" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating and configuring a range slider in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var rangeSlider = new RangeSliderFloat
    /// {
    ///     lowValue = 0f,
    ///     highValue = 100f,
    ///     value = new Vector2(25f, 75f),
    ///     step = 5f,
    ///     showMarks = true,
    ///     displayValueLabel = ValueDisplayMode.Auto,
    ///     track = TrackDisplayType.On
    /// };
    ///
    /// rangeSlider.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Range changed: {evt.newValue.x} - {evt.newValue.y}");
    /// });
    /// ]]></code>
    /// <para>Custom formatting of values.</para>
    /// <code lang="csharp"><![CDATA[
    /// rangeSlider.formatString = "F1"; // Display one decimal place
    /// rangeSlider.formatFunction = (value) => $"{value:F1}°C"; // Add units
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class RangeSliderFloat : Slider<Vector2, float, Vector2Field>
    {
        const float k_DefaultStep = 0.1f;

        const float k_DefaultShiftStep = 1f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RangeSliderFloat()
        {
            comparer = new Vector2LexicographicalComparer();
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
            stepOverride = k_DefaultStep;
            shiftStepOverride = k_DefaultShiftStep;
            lowValueOverride = 0;
            highValueOverride = 100;
            minValueOverride = 0;
            maxValueOverride = 100;
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

        [UxmlAttribute("min-value")]
        float minValueOverride
        {
            get => minValue;
            set => minValue = value;
        }

        [UxmlAttribute("max-value")]
        float maxValueOverride
        {
            get => maxValue;
            set => maxValue = value;
        }

        /// <summary>
        /// The low part of the range value.
        /// </summary>
        public float minValue
        {
            get => m_Value.x;
            set => this.value = new Vector2(value, m_Value.y);
        }

        /// <summary>
        /// The high part of the range value.
        /// </summary>
        public float maxValue
        {
            get => m_Value.y;
            set => this.value = new Vector2(m_Value.x, value);
        }

        /// <inheritdoc />
        protected override int thumbCount => 2;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out Vector2 val)
        {
            var strValues = strValue.Split(" - ");
            var xStr = strValues[0];
            var yStr = strValues[1];
            var xRet = float.TryParse(xStr, out var val1);
            var yRet = float.TryParse(yStr, out var val2);
            val = new Vector2(val1, val2);
            return xRet && yRet;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(Vector2 val)
        {
            return $"[{ParseSubValueToString(val.x)} - {ParseSubValueToString(val.y)}]";
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(float val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
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

        /// <inheritdoc cref="BaseSlider{TValue,TScalar}.GetValueFromScalarValues"/>
        protected override Vector2 GetValueFromScalarValues(Span<float> values)
        {
            return new Vector2(values[0], values[1]);
        }

        /// <inheritdoc cref="BaseSlider{TValue,TScalar}.GetScalarValuesFromValue"/>
        protected override void GetScalarValuesFromValue(Vector2 v, Span<float> values)
        {
            values[0] = v.x;
            values[1] = v.y;
        }

    }
}
