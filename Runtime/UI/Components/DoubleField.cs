using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Double Field UI element.
    /// </summary>
    public class DoubleField : NumericalField<double>
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
            // If formatString is percent, we need to divide by 100
            if (UINumericFieldsUtils.IsPercentFormatString(formatString))
            {
                val /= 100f;
                if (highValue == 1f)
                    Debug.LogWarning("High value is set to 1, but format string is a percent format string.\n" +
                        "Please set high value to 100 instead.");
            }
            
            return val.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
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
            return 0.001f * (AreEqual(baseValue, 0) ? 1f : (float)baseValue);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="DoubleField"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<DoubleField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="DoubleField"/>.
        /// </summary>
        public new class UxmlTraits : NumericalField<double>.UxmlTraits { }
    }
}
