using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A text field component that accepts and validates integer number input.
    /// </summary>
    /// <remarks>
    /// IntField is an input component that allows users to enter and modify integer values. It provides
    /// validation, formatting, and range constraints to ensure proper numeric input.
    ///
    /// The field supports keyboard input, arrow key increments/decrements, and optional unit display. It can
    /// be customized with minimum and maximum value constraints.
    ///
    /// When setting minimum (lowValue) and maximum (highValue) constraints, ensure that the minimum value is
    /// less than the maximum value to avoid unexpected behavior.
    ///
    /// The component supports different sizes and can be integrated into forms or used standalone. It
    /// automatically validates input to ensure only valid integer values are accepted.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class IntField : NumericalField<int>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public IntField()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out int val)
        {
            var ret = UINumericFieldsUtils.StringToLong(strValue, out var v);
            val = ret ? UINumericFieldsUtils.ClampToInt(v) : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(int val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.ParseRawValueToString"/>
        protected override string ParseRawValueToString(int val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(int a, int b)
        {
            return a == b;
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override int Max(int a, int b)
        {
            return Math.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override int Increment(int originalValue, float delta)
        {
            return originalValue + (Mathf.Approximately(0, delta) ? 0 : Mathf.RoundToInt(delta));
        }

        /// <inheritdoc/>
        protected override float GetIncrementFactor(int baseValue)
        {
            var absValue = Math.Abs(baseValue);
            if (absValue == 0 || absValue <= 10)
                return 1;
            if (absValue <= 100)
                return 5;

            // Use log scale for larger values
            var magnitude = (int)Math.Pow(10, Math.Floor(Math.Log10(absValue)));
            return Math.Max(1, magnitude / 10);
        }

        /// <inheritdoc/>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
        {
            var previousValue = m_Value;
            var sensitivity = UINumericFieldsUtils.CalculateIntDragSensitivity(startValue);
            var acceleration = UINumericFieldsUtils.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
            var v = (long)previousValue;
            v += (long)Math.Round(UINumericFieldsUtils.NiceDelta(delta, acceleration) * sensitivity);
            var newValue = UINumericFieldsUtils.ClampToInt(v);
            SetValueWithoutNotify(newValue);
            TrySendChangingEvent(previousValue, newValue);
        }

    }
}
