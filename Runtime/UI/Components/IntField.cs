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
    /// A <see cref="VisualElement"/> that displays a numeric value and allows the user to edit it.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
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

        /// <inheritdoc cref="NumericalField{T}.GetIncrementFactor"/>
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

#if ENABLE_VALUEFIELD_INTERFACE
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
#endif

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="IntField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<IntField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="IntField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<int>.UxmlTraits { }

#endif
    }
}
