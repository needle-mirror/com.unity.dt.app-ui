using System;
using System.Globalization;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(UINumericFieldsUtils))]
    class UINumericFieldsUtilsTests
    {
        #region IsPercentFormatString Tests
        [Test]
        [TestCase("0", false)]
        [TestCase("0.0", false)]
        [TestCase("0.00", false)]
        [TestCase("0.000", false)]
        [TestCase("0P", false)]
        [TestCase("0.0P", false)]
        [TestCase("0.00P", false)]
        [TestCase("P", true)]
        [TestCase("P0", true)]
        [TestCase("P0.0", false)]
        [TestCase("P0.00", false)]
        [TestCase("P0.000", false)]
        [TestCase("P00", true)]
        [TestCase("P000", true)]
        [TestCase("P0000", true)]
        [TestCase("P1", true)]
        [TestCase("P2", true)]
        [TestCase("P3", true)]
        [TestCase("P4", true)]
        [TestCase("0%", true)]
        [TestCase("0.0%", true)]
        [TestCase("0.00%", true)]
        [TestCase("%", false)]
        [TestCase("%0", true)]
        [TestCase("%0.0", true)]
        [TestCase("%0.00", true)]
        [TestCase(" %", false)]
        [TestCase("0 %", true)]
        [TestCase("0.0 %", true)]
        [TestCase("0.00 %", true)]
        [TestCase("% ", false)]
        [TestCase("% 0", true)]
        [TestCase("% 0.0", true)]
        [TestCase("% 0.00", true)]
        [TestCase("% 000", true)]
        [TestCase("%%% 000", false)]
        [TestCase("0%0", false)]
        [TestCase("## %", true)]
        public void IsPercentFormatString_Succeed(string formatString, bool isPercent)
        {
            Assert.AreEqual(isPercent, UINumericFieldsUtils.IsPercentFormatString(formatString));
        }
        #endregion

        #region StringToDouble Tests
        [Test]
        [TestCase("0", 0.0)]
        [TestCase("42", 42.0)]
        [TestCase("-42", -42.0)]
        [TestCase("3.14", 3.14)]
        [TestCase("-3.14", -3.14)]
        [TestCase("0.001", 0.001)]
        [TestCase("-0.001", -0.001)]
        [TestCase("1e5", 100000.0)]
        [TestCase("1e-5", 0.00001)]
        public void StringToDouble_ValidNumbers_ReturnTrue(string input, double expected)
        {
            var result = UINumericFieldsUtils.StringToDouble(input, out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [Test]
        public void StringToDouble_Infinity_ReturnTrue()
        {
            var result = UINumericFieldsUtils.StringToDouble("inf", out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(double.PositiveInfinity, value);
        }

        [Test]
        public void StringToDouble_InfinityLong_ReturnTrue()
        {
            var result = UINumericFieldsUtils.StringToDouble("infinity", out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(double.PositiveInfinity, value);
        }

        [Test]
        public void StringToDouble_NegativeInfinity_ReturnTrue()
        {
            var result = UINumericFieldsUtils.StringToDouble("-inf", out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(double.NegativeInfinity, value);
        }

        [Test]
        public void StringToDouble_NegativeInfinityLong_ReturnTrue()
        {
            var result = UINumericFieldsUtils.StringToDouble("-infinity", out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(double.NegativeInfinity, value);
        }

        [Test]
        public void StringToDouble_NaN_ReturnTrue()
        {
            var result = UINumericFieldsUtils.StringToDouble("nan", out var value);
            Assert.IsTrue(result);
            Assert.IsTrue(double.IsNaN(value));
        }

        [Test]
        [TestCase("inf")]
        [TestCase("INF")]
        [TestCase("Inf")]
        public void StringToDouble_InfinityCase_ReturnTrue(string input)
        {
            var result = UINumericFieldsUtils.StringToDouble(input, out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(double.PositiveInfinity, value);
        }

        [Test]
        [TestCase("nan")]
        [TestCase("NAN")]
        [TestCase("NaN")]
        public void StringToDouble_NaNCase_ReturnTrue(string input)
        {
            var result = UINumericFieldsUtils.StringToDouble(input, out var value);
            Assert.IsTrue(result);
            Assert.IsTrue(double.IsNaN(value));
        }
        #endregion

        #region StringToLong Tests
        [Test]
        [TestCase("0", 0L)]
        [TestCase("42", 42L)]
        [TestCase("-42", -42L)]
        [TestCase("1000", 1000L)]
        [TestCase("-1000", -1000L)]
        [TestCase("9223372036854775807", long.MaxValue)]
        [TestCase("-9223372036854775808", long.MinValue)]
        public void StringToLong_ValidNumbers_ReturnTrue(string input, long expected)
        {
            var result = UINumericFieldsUtils.StringToLong(input, out var value);
            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [Test]
        [TestCase("2+3")]
        [TestCase("10-5")]
        [TestCase("4*5")]
        [TestCase("20/4")]
        public void StringToLong_ValidExpressions_ReturnTrue(string input)
        {
            var result = UINumericFieldsUtils.StringToLong(input, out var value);
            Assert.IsTrue(result);
            Assert.IsTrue(value > 0);
        }
        #endregion

        #region ClampToInt Tests
        [Test]
        [TestCase(0L, 0)]
        [TestCase(42L, 42)]
        [TestCase(-42L, -42)]
        [TestCase(2147483647L, 2147483647)]
        [TestCase(-2147483648L, -2147483648)]
        public void ClampToInt_ValueWithinRange_ReturnValue(long input, int expected)
        {
            var result = UINumericFieldsUtils.ClampToInt(input);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ClampToInt_ValueAboveMaxValue_ReturnMaxValue()
        {
            var result = UINumericFieldsUtils.ClampToInt(long.MaxValue);
            Assert.AreEqual(int.MaxValue, result);
        }

        [Test]
        public void ClampToInt_ValueBelowMinValue_ReturnMinValue()
        {
            var result = UINumericFieldsUtils.ClampToInt(long.MinValue);
            Assert.AreEqual(int.MinValue, result);
        }

        [Test]
        public void ClampToInt_LargePositiveValue_ReturnMaxValue()
        {
            var result = UINumericFieldsUtils.ClampToInt(9223372036854775807L);
            Assert.AreEqual(int.MaxValue, result);
        }

        [Test]
        public void ClampToInt_LargeNegativeValue_ReturnMinValue()
        {
            var result = UINumericFieldsUtils.ClampToInt(-9223372036854775808L);
            Assert.AreEqual(int.MinValue, result);
        }
        #endregion

        #region ClampToFloat Tests
        [Test]
        [TestCase(0.0, 0.0f)]
        [TestCase(42.5, 42.5f)]
        [TestCase(-42.5, -42.5f)]
        public void ClampToFloat_ValueWithinRange_ReturnValue(double input, float expected)
        {
            var result = UINumericFieldsUtils.ClampToFloat(input);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ClampToFloat_ValueAboveMaxValue_ReturnMaxValue()
        {
            var result = UINumericFieldsUtils.ClampToFloat(double.MaxValue);
            Assert.AreEqual(float.MaxValue, result);
        }

        [Test]
        public void ClampToFloat_ValueBelowMinValue_ReturnMinValue()
        {
            var result = UINumericFieldsUtils.ClampToFloat(double.MinValue);
            Assert.AreEqual(float.MinValue, result);
        }

        [Test]
        public void ClampToFloat_PositiveInfinity_ReturnMaxValue()
        {
            var result = UINumericFieldsUtils.ClampToFloat(double.PositiveInfinity);
            Assert.AreEqual(float.MaxValue, result);
        }

        [Test]
        public void ClampToFloat_NegativeInfinity_ReturnMinValue()
        {
            var result = UINumericFieldsUtils.ClampToFloat(double.NegativeInfinity);
            Assert.AreEqual(float.MinValue, result);
        }
        #endregion

        #region CalculateFloatDragSensitivity Tests
        [Test]
        [TestCase(0.0)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(100.0)]
        [TestCase(-1.0)]
        [TestCase(-10.0)]
        public void CalculateFloatDragSensitivity_ValidValues_ReturnPositiveSensitivity(double value)
        {
            var result = UINumericFieldsUtils.CalculateFloatDragSensitivity(value);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void CalculateFloatDragSensitivity_PositiveInfinity_ReturnZero()
        {
            var result = UINumericFieldsUtils.CalculateFloatDragSensitivity(double.PositiveInfinity);
            Assert.AreEqual(0.0, result);
        }

        [Test]
        public void CalculateFloatDragSensitivity_NegativeInfinity_ReturnZero()
        {
            var result = UINumericFieldsUtils.CalculateFloatDragSensitivity(double.NegativeInfinity);
            Assert.AreEqual(0.0, result);
        }

        [Test]
        public void CalculateFloatDragSensitivity_NaN_ReturnZero()
        {
            var result = UINumericFieldsUtils.CalculateFloatDragSensitivity(double.NaN);
            Assert.AreEqual(0.0, result);
        }

        [Test]
        public void CalculateFloatDragSensitivity_Zero_ReturnMinimumSensitivity()
        {
            var result = UINumericFieldsUtils.CalculateFloatDragSensitivity(0.0);
            Assert.IsTrue(Mathf.Approximately((float)result, UINumericFieldsUtils.k_DragSensitivity));
        }

        [Test]
        public void CalculateFloatDragSensitivity_LargerValueHasHigherSensitivity()
        {
            var sensitivity1 = UINumericFieldsUtils.CalculateFloatDragSensitivity(1.0);
            var sensitivity100 = UINumericFieldsUtils.CalculateFloatDragSensitivity(100.0);
            Assert.IsTrue(sensitivity100 > sensitivity1);
        }
        #endregion

        #region CalculateIntDragSensitivity Tests
        [Test]
        [TestCase(0L)]
        [TestCase(1L)]
        [TestCase(10L)]
        [TestCase(100L)]
        [TestCase(-1L)]
        [TestCase(-10L)]
        public void CalculateIntDragSensitivity_Long_ValidValues_ReturnPositiveSensitivity(long value)
        {
            var result = UINumericFieldsUtils.CalculateIntDragSensitivity(value);
            Assert.IsTrue(result > 0);
        }

        [Test]
        [TestCase(0.0)]
        [TestCase(1.0)]
        [TestCase(10.0)]
        [TestCase(100.0)]
        [TestCase(-1.0)]
        [TestCase(-10.0)]
        public void CalculateIntDragSensitivity_Double_ValidValues_ReturnPositiveSensitivity(double value)
        {
            var result = UINumericFieldsUtils.CalculateIntDragSensitivity(value);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void CalculateIntDragSensitivity_Double_Zero_ReturnMinimumSensitivity()
        {
            var result = UINumericFieldsUtils.CalculateIntDragSensitivity(0.0);
            Assert.IsTrue(result >= 0.03);
        }

        [Test]
        public void CalculateIntDragSensitivity_Double_LargerValueHasSameSensitivity()
        {
            var sensitivity1 = UINumericFieldsUtils.CalculateIntDragSensitivity(1.0);
            var sensitivity2 = UINumericFieldsUtils.CalculateIntDragSensitivity(1000.0);
            Assert.IsTrue(Math.Abs(sensitivity2 - sensitivity1) < Mathf.Epsilon);
        }
        #endregion

        #region Acceleration Tests
        [Test]
        [TestCase(false, false, 1.0f)]
        [TestCase(true, false, 4.0f)]
        [TestCase(false, true, 0.25f)]
        [TestCase(true, true, 1.0f)]
        public void Acceleration_WithShiftAndAlt_ReturnCorrectFactor(bool shift, bool alt, float expected)
        {
            var result = UINumericFieldsUtils.Acceleration(shift, alt);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Acceleration_NoKeysPressed_ReturnOne()
        {
            var result = UINumericFieldsUtils.Acceleration(false, false);
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void Acceleration_OnlyShiftPressed_ReturnFour()
        {
            var result = UINumericFieldsUtils.Acceleration(true, false);
            Assert.AreEqual(4.0f, result);
        }

        [Test]
        public void Acceleration_OnlyAltPressed_ReturnQuarterSpeed()
        {
            var result = UINumericFieldsUtils.Acceleration(false, true);
            Assert.AreEqual(0.25f, result);
        }

        [Test]
        public void Acceleration_ShiftAndAltPressed_ReturnOne()
        {
            var result = UINumericFieldsUtils.Acceleration(true, true);
            Assert.AreEqual(1.0f, result);
        }
        #endregion

        #region NiceDelta Tests
        [Test]
        public void NiceDelta_ZeroDelta_ReturnZero()
        {
            var delta = new Vector2(0, 0);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.AreEqual(0.0, result);
        }

        [Test]
        public void NiceDelta_PositiveXDelta_ReturnPositiveValue()
        {
            var delta = new Vector2(10, 0);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void NiceDelta_NegativeXDelta_ReturnNegativeValue()
        {
            var delta = new Vector2(-10, 0);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result < 0);
        }

        [Test]
        public void NiceDelta_NegativeYDelta_ReturnPositiveValue()
        {
            var delta = new Vector2(0, -10);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void NiceDelta_PositiveYDelta_ReturnNegativeValue()
        {
            var delta = new Vector2(0, 10);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result < 0);
        }

        [Test]
        public void NiceDelta_WithAcceleration_ReturnMultipliedValue()
        {
            var delta = new Vector2(10, 0);
            var result1 = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            var result2 = UINumericFieldsUtils.NiceDelta(delta, 2.0f);
            Assert.IsTrue(Mathf.Approximately((float)(result2 / result1), 2.0f));
        }

        [Test]
        public void NiceDelta_DiagonalDeltaPreferX_ReturnXDominant()
        {
            var delta = new Vector2(100, 10);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void NiceDelta_DiagonalDeltaPreferY_ReturnYDominant()
        {
            var delta = new Vector2(10, -100f);
            var result = UINumericFieldsUtils.NiceDelta(delta, 1.0f);
            Assert.IsTrue(result > 0);
        }
        #endregion

        #region RoundBasedOnMinimumDifference Tests
        [Test]
        [TestCase(3.14159, 0.01, 3.14)]
        [TestCase(3.14159, 0.001, 3.142)]
        [TestCase(3.14159, 0.1, 3.1)]
        [TestCase(3.14159, 1.0, 3.0)]
        public void RoundBasedOnMinimumDifference_ValidInputs_ReturnRoundedValue(double value, double minDifference, double expected)
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(value, minDifference);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RoundBasedOnMinimumDifference_NegativeValue_ReturnRoundedValue()
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(-3.14159, 0.01);
            Assert.AreEqual(-3.14, result);
        }

        [Test]
        public void RoundBasedOnMinimumDifference_ZeroMinDifference_ReturnDiscardedValue()
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(3.14159, 0.0);
            Assert.IsTrue(result > 0);
        }

        [Test]
        public void RoundBasedOnMinimumDifference_VerySmallMinDifference_ReturnPreciseValue()
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(1.23456789, 0.0001);
            Assert.IsTrue(Mathf.Approximately((float)result, 1.2346f));
        }

        [Test]
        public void RoundBasedOnMinimumDifference_LargeMinDifference_ReturnCoarseValue()
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(3.14159, 10.0);
            Assert.AreEqual(3, result);
        }

        [Test]
        [TestCase(0.5, 0.1)]
        [TestCase(1.5, 0.1)]
        [TestCase(2.5, 0.1)]
        public void RoundBasedOnMinimumDifference_MidpointRounding_ReturnAwayFromZero(double value, double minDifference)
        {
            var result = UINumericFieldsUtils.RoundBasedOnMinimumDifference(value, minDifference);
            Assert.IsTrue(result > value || Mathf.Approximately((float)result, (float)value));
        }
        #endregion
    }
}
