using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A text field that accepts floating-point numeric values.
    /// </summary>
    /// <remarks>
    /// FloatField is an input control that allows users to enter and edit floating-point numbers. It provides
    /// validation, formatting, and range constraints for numeric input.
    ///
    /// The component supports both keyboard input and increment/decrement functionality. Users can directly type
    /// values or use up/down arrow keys to adjust the value incrementally.
    ///
    /// **Note:** The field can be customized with various formatting options and can display optional units
    /// alongside the numeric value.
    /// </remarks>
    /// <example>
    /// <para>Basic FloatField with validation.</para>
    /// <code lang="csharp"><![CDATA[
    /// var field = new FloatField
    /// {
    ///     value = 50.0f,
    ///     lowValue = 0.0f,
    ///     highValue = 100.0f,
    ///     formatString = "F1",
    ///     unit = "%"
    /// };
    ///
    /// // Add validation for even numbers
    /// field.validateValue = (value) => value % 2 == 0;
    /// ]]></code>
    /// <para>FloatField with custom formatting and event handling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var field = new FloatField();
    ///
    /// // Custom format function to display with units
    /// field.formatFunction = (value) => $"{value:0.##} sec";
    ///
    /// // Listen for value changes
    /// field.RegisterValueChangedCallback(evt =>
    /// {
    ///     Debug.Log($"Value changed from {evt.previousValue} to {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>UXML Definition Example.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:FloatField
    ///     name="opacity-field"
    ///     value="0.5"
    ///     low-value="0"
    ///     high-value="1"
    ///     format-string="P0"
    ///     unit="opacity"
    ///     size="M" />
    /// ]]></code>
    /// </example>
    [VisualDocPage("inputs")]
    [UxmlElement]
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

        /// <inheritdoc/>
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

    }
}
