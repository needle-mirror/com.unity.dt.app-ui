using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A field component that allows users to input and edit double precision floating-point numbers.
    /// </summary>
    /// <remarks>
    /// The Double Field component is a specialized input field designed for handling double precision floating-point
    /// numbers. It provides a clean, user-friendly interface for entering and manipulating numerical values with
    /// decimal points.
    ///
    /// This component extends the NumericalField base class and includes features such as value validation,
    /// formatting options, and unit display. It's particularly useful in scenarios where precise decimal input is
    /// required, such as scientific applications, financial calculations, or detailed parameter configurations.
    ///
    /// Double Fields should be used when decimal precision is crucial. For simpler whole number inputs, consider
    /// using IntegerField instead.
    ///
    /// - Decimal number input with high precision
    /// - Optional minimum and maximum value constraints
    /// - Customizable value formatting
    /// - Unit display support
    /// - Keyboard navigation and input
    /// - Value validation
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class DoubleField : NumericalField<double>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DoubleField()
        {
            formatString = UINumericFieldsUtils.k_DoubleFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out double val)
        {
            var ret = UINumericFieldsUtils.StringToDouble(strValue, out var d);
            val = ret ? d : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(double val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.ParseRawValueToString"/>
        protected override string ParseRawValueToString(double val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override double Min(double a, double b)
        {
            return Math.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override double Max(double a, double b)
        {
            return Math.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override double Increment(double originalValue, float delta)
        {
            return originalValue + delta;
        }

        /// <inheritdoc/>
        protected override float GetIncrementFactor(double baseValue)
        {
            // Use log scale for better increment across different magnitudes
            var absValue = Math.Abs(baseValue);
            if (absValue < 0.001 || AreEqual(absValue, 0))
                return 0.001f;

            // Calculate the order of magnitude
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(absValue)));
            return (float)(magnitude * 0.01);
        }

        /// <inheritdoc/>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, double startValue)
        {
            var previousValue = m_Value;
            var sensitivity = UINumericFieldsUtils.CalculateFloatDragSensitivity(startValue);
            var acceleration = UINumericFieldsUtils.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
            var v = (double)previousValue;
            v += UINumericFieldsUtils.NiceDelta(delta, acceleration) * sensitivity;
            v = UINumericFieldsUtils.RoundBasedOnMinimumDifference(v, sensitivity);
            var newValue = v;
            SetValueWithoutNotify(newValue);
            TrySendChangingEvent(previousValue, newValue);
        }

    }
}
