using System;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Utility class for the numeric fields.
    /// </summary>
    static class UINumericFieldsUtils
    {
        public const string k_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";
        public const string k_AllowedCharactersForInt = "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";
        public const string k_DoubleFieldFormatString = "R";
        public const string k_FloatFieldFormatString = "g7";
        public const string k_IntFieldFormatString = "#######0";
        internal const float k_DragSensitivity = .03f;
        static bool s_UseYSign = false;

        /// <summary>
        /// Convert a string to a double by evaluating it as an expression.
        /// </summary>
        /// <param name="str"> The string to convert.</param>
        /// <param name="value"> The converted value.</param>
        /// <returns> True if the conversion was successful, False otherwise.</returns>
        public static bool StringToDouble(string str, out double value)
        {
            var lowered = str.ToLower();
            if (lowered == "inf" || lowered == "infinity")
                value = double.PositiveInfinity;
            else if (lowered == "-inf" || lowered == "-infinity")
                value = double.NegativeInfinity;
            else if (lowered == "nan")
                value = double.NaN;
            else
                return ExpressionEvaluator.Evaluate(str, out value);

            return true;
        }

        /// <summary>
        /// Convert a string to a float by evaluating it as an expression.
        /// </summary>
        /// <param name="str"> The string to convert.</param>
        /// <param name="value"> The converted value.</param>
        /// <returns> True if the conversion was successful, False otherwise.</returns>
        public static bool StringToLong(string str, out long value)
        {
            return ExpressionEvaluator.Evaluate(str, out value);
        }

        internal static int ClampToInt(long value)
        {
            if (value < int.MinValue)
                return int.MinValue;

            if (value > int.MaxValue)
                return int.MaxValue;

            return (int)value;
        }

        /// <summary>
        /// Clamp a double to a float.
        /// </summary>
        /// <param name="value"> The double value to clamp.</param>
        /// <returns> The clamped float value.</returns>
        public static float ClampToFloat(double value)
        {
            if (value < float.MinValue)
                return float.MinValue;

            if (value > float.MaxValue)
                return float.MaxValue;

            return (float)value;
        }

        /// <summary>
        /// Check if the string formatting code is a percent format.
        /// </summary>
        /// <param name="formatString"> The string formatting code.</param>
        /// <returns> True if the string formatting code is a percent format, False otherwise.</returns>
        public static bool IsPercentFormatString(string formatString)
        {
            if (string.IsNullOrEmpty(formatString))
                return false;

            if (formatString.ToUpperInvariant().StartsWith("P"))
                return !formatString.Contains(".");

            var pCount = 0;
            var dCount = 0;
            foreach (var c in formatString)
            {
                switch (c)
                {
                    case '%':
                        pCount++;
                        break;
                    case '#' or '0':
                        dCount++;
                        break;
                }
            }

            return pCount == 1 && dCount > 0 && (formatString.StartsWith("%") || formatString.EndsWith("%"));
        }

        /// <summary>
        /// Calculate the drag sensitivity for a float value.
        /// </summary>
        /// <param name="value"> The float value.</param>
        /// <returns> The drag sensitivity.</returns>
        public static double CalculateFloatDragSensitivity(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return 0.0;
            }
            return Math.Max(1, Math.Pow(Math.Abs(value), 0.5)) * k_DragSensitivity;
        }

        /// <summary>
        /// Calculate the drag sensitivity for an int value.
        /// </summary>
        /// <param name="value"> The int value as long.</param>
        /// <returns> The drag sensitivity.</returns>
        public static long CalculateIntDragSensitivity(long value)
        {
            return (long)CalculateIntDragSensitivity((double)value);
        }

        /// <summary>
        /// Calculate the drag sensitivity for a double value.
        /// </summary>
        /// <param name="value"> The double value.</param>
        /// <returns> The drag sensitivity.</returns>
        public static double CalculateIntDragSensitivity(double value)
        {
            return Math.Max(1, Math.Pow(Math.Abs(value), 0.5) * k_DragSensitivity);
        }

        /// <summary>
        /// Calculate the acceleration factor based on the shift and alt keys.
        /// </summary>
        /// <param name="shiftPressed"> Whether the shift key is pressed.</param>
        /// <param name="altPressed"> Whether the alt key is pressed.</param>
        /// <returns> The acceleration factor.</returns>
        public static float Acceleration(bool shiftPressed, bool altPressed)
        {
            return (shiftPressed ? 4 : 1) * (altPressed ? .25f : 1);
        }

        /// <summary>
        /// Calculate a "nice" delta value based on the device delta and acceleration.
        /// </summary>
        /// <param name="deviceDelta"> The device delta.</param>
        /// <param name="acceleration"> The acceleration factor.</param>
        /// <returns> The "nice" delta value.</returns>
        public static double NiceDelta(Vector2 deviceDelta, float acceleration)
        {
            deviceDelta.y = -deviceDelta.y;

            if (Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > .1f)
                s_UseYSign = !(Mathf.Abs(deviceDelta.x) > Mathf.Abs(deviceDelta.y));

            if (s_UseYSign)
                return Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration;

            return Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
        }

        /// <summary>
        /// Round a value based on a minimum difference.
        /// </summary>
        /// <param name="valueToRound"> The value to round.</param>
        /// <param name="minDifference"> The minimum difference.</param>
        /// <returns> The rounded value.</returns>
        public static double RoundBasedOnMinimumDifference(double valueToRound, double minDifference)
        {
            if (minDifference == 0)
                return DiscardLeastSignificantDecimal(valueToRound);
            return Math.Round(valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference),
                MidpointRounding.AwayFromZero);
        }

        static double DiscardLeastSignificantDecimal(double v)
        {
            var decimals = Math.Max(0, (int)(5 - Math.Log10(Math.Abs(v))));
            try
            {
                return Math.Round(v, decimals);
            }
            catch (ArgumentOutOfRangeException)
            {
                // This can happen for very small numbers.
                return 0;
            }
        }

        static int GetNumberOfDecimalsForMinimumDifference(double minDifference) =>
            (int)Math.Max(0.0, -Math.Floor(Math.Log10(Math.Abs(minDifference))));
    }
}
