using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A numerical input field that accepts and validates long integer values.
    /// </summary>
    /// <remarks>
    /// LongField is a specialized input component designed for entering and manipulating long integer values. It
    /// provides built-in validation, formatting options, and support for value constraints.
    ///
    /// The component extends NumericalField&lt;long&gt; and includes features such as:
    /// - Value constraints through lowValue and highValue properties
    /// - Custom value formatting using formatString or formatFunction
    /// - Unit display support
    /// - Keyboard navigation and increment/decrement functionality
    /// - Invalid state handling with custom validation
    ///
    /// **Tip:** Use LongField when you need to collect or display long integer values in forms, settings panels,
    /// or any interface requiring numerical input within the long integer range.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class LongField : NumericalField<long>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LongField()
        {
            formatString = UINumericFieldsUtils.k_IntFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out long val)
        {
            var ret = UINumericFieldsUtils.StringToLong(strValue, out var v);
            val = ret ? v : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(long val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.ParseRawValueToString"/>
        protected override string ParseRawValueToString(long val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(long a, long b)
        {
            return a == b;
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override long Min(long a, long b)
        {
            return Math.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override long Max(long a, long b)
        {
            return Math.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override long Increment(long originalValue, float delta)
        {
            return originalValue + (Mathf.Approximately(0, delta) ? 0 : Math.Sign(delta));
        }

        /// <inheritdoc/>
        protected override float GetIncrementFactor(long baseValue)
        {
            return Math.Abs(baseValue) > 100 ? (float)Math.Ceiling(baseValue * 0.1f) : 1;
        }

        /// <inheritdoc/>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, long startValue)
        {
            var previousValue = m_Value;
            var sensitivity = UINumericFieldsUtils.CalculateIntDragSensitivity(startValue);
            var acceleration = UINumericFieldsUtils.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
            var v = previousValue;
            v += (long)Math.Round(UINumericFieldsUtils.NiceDelta(delta, acceleration) * sensitivity);
            var newValue = v;
            SetValueWithoutNotify(newValue);
            TrySendChangingEvent(previousValue, newValue);
        }

    }
}
