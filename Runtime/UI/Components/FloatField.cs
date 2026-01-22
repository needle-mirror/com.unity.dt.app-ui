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
    /// Float Field UI element.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class FloatField : NumericalField<float>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FloatField()
        {
            formatString = UINumericFieldsUtils.k_FloatFieldFormatString;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseStringToValue"/>
        protected override bool ParseStringToValue(string strValue, out float val)
        {
            var ret = UINumericFieldsUtils.StringToDouble(strValue, out var d);
            val = ret ? UINumericFieldsUtils.ClampToFloat(d) : value;
            return ret;
        }

        /// <inheritdoc cref="NumericalField{T}.ParseValueToString"/>
        protected override string ParseValueToString(float val)
        {
            if (formatFunction != null)
                return formatFunction(val);

            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.ParseRawValueToString"/>
        protected override string ParseRawValueToString(float val)
        {
            return val.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc cref="NumericalField{T}.AreEqual"/>
        protected override bool AreEqual(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Min(T,T)"/>
        protected override float Min(float a, float b)
        {
            return Mathf.Min(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Max(T,T)"/>
        protected override float Max(float a, float b)
        {
            return Mathf.Max(a, b);
        }

        /// <inheritdoc cref="NumericalField{T}.Increment"/>
        protected override float Increment(float originalValue, float delta)
        {
            return originalValue + delta;
        }

        /// <inheritdoc cref="NumericalField{T}.GetIncrementFactor"/>
        protected override float GetIncrementFactor(float baseValue)
        {
            // Use log scale for better increment across different magnitudes
            var absValue = Mathf.Abs(baseValue);
            if (absValue < 0.001f || Mathf.Approximately(absValue, 0f))
                return 0.001f;

            // Calculate the order of magnitude
            var magnitude = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(absValue)));
            return magnitude * 0.01f;
        }

#if ENABLE_VALUEFIELD_INTERFACE
        /// <inheritdoc/>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue)
        {
            var previousValue = m_Value;
            var sensitivity = UINumericFieldsUtils.CalculateFloatDragSensitivity(startValue);
            var acceleration = UINumericFieldsUtils.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
            var v = (double)previousValue;
            v += UINumericFieldsUtils.NiceDelta(delta, acceleration) * sensitivity;
            v = UINumericFieldsUtils.RoundBasedOnMinimumDifference(v, sensitivity);
            var newValue = UINumericFieldsUtils.ClampToFloat(v);
            SetValueWithoutNotify(newValue);
            TrySendChangingEvent(previousValue, newValue);
        }
#endif

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="FloatField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<FloatField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="FloatField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<float>.UxmlTraits { }

#endif
    }
}
