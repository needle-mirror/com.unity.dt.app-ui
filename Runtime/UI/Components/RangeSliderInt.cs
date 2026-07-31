using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A simple comparer for Vector2Int that compares the x and y values lexicographically.
    /// </summary>
    public class Vector2IntLexicographicalComparer : IComparer<Vector2Int>
    {
        /// <summary>
        /// Compares two Vector2Int values lexicographically.
        /// </summary>
        /// <param name="v1"> The first Vector2Int value to compare.</param>
        /// <param name="v2"> The second Vector2Int value to compare.</param>
        /// <returns> A value indicating the relative order of the two Vector2Int values.</returns>
        public int Compare(Vector2Int v1, Vector2Int v2)
        {
            var xComparison = v1.x.CompareTo(v2.x);
            return xComparison != 0 ? xComparison : v1.y.CompareTo(v2.y);
        }
    }

    /// <summary>
    /// A UI component that lets users select a range of integer values by dragging handles along a track.
    /// </summary>
    /// <remarks>
    /// The RangeSliderInt is a UI component that allows users to select a range of integer values by dragging two
    /// handles along a track. It's particularly useful when you need to let users define minimum and maximum
    /// bounds within a specific range.
    ///
    /// The component supports both horizontal and vertical orientations, customizable step values, and various
    /// display options for the track and value labels. Users can interact with the slider using mouse/touch input
    /// or keyboard navigation.
    ///
    /// Unlike a regular slider that has a single value, a range slider operates with two values (minValue and
    /// maxValue) that define the selected range. These values can be accessed and modified independently.
    /// </remarks>
    /// <example>
    /// <para>Basic range slider with default settings: creates a horizontal range slider with a range of 0-100 and
    /// initial selection of 25-75.</para>
    /// <code lang="xml"><![CDATA[
    /// <RangeSliderInt low-value="0" high-value="100" min-value="25" max-value="75" />
    /// ]]></code>
    /// <para>Advanced range slider with custom configuration: creates a vertical range slider with custom range, step
    /// value, and visual feedback options.</para>
    /// <code lang="xml"><![CDATA[
    /// <RangeSliderInt
    ///     orientation="Vertical"
    ///     low-value="-50"
    ///     high-value="50"
    ///     step="5"
    ///     display-value-label="Auto"
    ///     show-marks="true"
    ///     track="On"
    /// />
    /// ]]></code>
    /// <para>Range slider with C# event handling: demonstrates how to create a range slider and handle value changes in
    /// code.</para>
    /// <code lang="csharp"><![CDATA[
    /// var rangeSlider = new RangeSliderInt();
    /// rangeSlider.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Range changed: {evt.newValue.x} - {evt.newValue.y}");
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class RangeSliderInt : Slider<Vector2Int, int, Vector2IntField>
    {
        const int k_DefaultStep = 1;

        const int k_DefaultShiftStep = 10;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RangeSliderInt()
        {
            comparer = new Vector2IntLexicographicalComparer();
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            stepOverride = k_DefaultStep;
            shiftStepOverride = k_DefaultShiftStep;
            lowValueOverride = 0;
            highValueOverride = 100;
            minValueOverride = 0;
            maxValueOverride = 100;
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

        [UxmlAttribute("min-value")]
        int minValueOverride
        {
            get => minValue;
            set => minValue = value;
        }

        [UxmlAttribute("max-value")]
        int maxValueOverride
        {
            get => maxValue;
            set => maxValue = value;
        }

        /// <summary>
        /// The low part of the range value.
        /// </summary>
        public int minValue
        {
            get => m_Value.x;
            set => this.value = new Vector2Int(value, m_Value.y);
        }

        /// <summary>
        /// The high part of the range value.
        /// </summary>
        public int maxValue
        {
            get => m_Value.y;
            set => this.value = new Vector2Int(m_Value.x, value);
        }

        /// <inheritdoc />
        protected override int thumbCount => 2;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out Vector2Int val)
        {
            var strValues = strValue.Split(" - ");
            var xStr = strValues[0];
            var yStr = strValues[1];
            var xRet = int.TryParse(xStr, out var val1);
            var yRet = int.TryParse(yStr, out var val2);
            val = new Vector2Int(val1, val2);
            return xRet && yRet;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(Vector2Int val)
        {
            if (UINumericFieldsUtils.IsPercentFormatString(formatString))
                Debug.LogWarning("Percent format string is not supported for integer values.\n" +
                    "Please use a RangeSliderFloat instead.");

            return $"[{ParseSubValueToString(val.x)} - {ParseSubValueToString(val.y)}]";
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(int val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

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
        protected override int ClampThumb(int x, int min, int max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc cref="BaseSlider{TValue,TScalar}.GetValueFromScalarValues"/>
        protected override Vector2Int GetValueFromScalarValues(Span<int> values)
        {
            return new Vector2Int(values[0], values[1]);
        }

        /// <inheritdoc cref="BaseSlider{TValue,TScalar}.GetScalarValuesFromValue"/>
        protected override void GetScalarValuesFromValue(Vector2Int v, Span<int> values)
        {
            values[0] = v.x;
            values[1] = v.y;
        }

        /// <inheritdoc />
        protected override int GetStepCount(int stepValue)
        {
            return (highValue - lowValue) / stepValue + 1;
        }

    }
}
