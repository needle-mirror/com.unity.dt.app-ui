using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Double Field UI element.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
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

        /// <inheritdoc cref="NumericalField{T}.GetIncrementFactor"/>
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

#if ENABLE_VALUEFIELD_INTERFACE
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
#endif

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="DoubleField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<DoubleField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="DoubleField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<double>.UxmlTraits { }
#endif
    }
}
