using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RangeSliderFloat))]
    class RangeSliderFloatTests : SliderTests<RangeSliderFloat,Vector2,float,Vector2Field>
    {
        protected override string mainUssClassName => RangeSliderFloat.ussClassName;

        protected override List<SliderMark<float>> marks => new List<SliderMark<float>>
        {
            new() {label = "Low", value = 0f},
            new() {label = "Middle", value = 5f},
            new() {label = "High", value = 10f},
        };

        protected override float highValue => 10f;

        protected override float lowValue => 0f;

        protected override Vector2 value => new Vector2(3f, 7f);

        protected override float step => 1f;

        protected override float shiftStep => 2f;

        protected override string highValueToUxml => $"high-value=\"{highValue}\"";

        protected override string valueToUxml => $"min-value=\"{value.x}\" max-value=\"{value.y}\"";
    }

    [TestFixture]
    [TestOf(typeof(RangeSliderInt))]
    class RangeSliderIntTests : SliderTests<RangeSliderInt,Vector2Int,int,Vector2IntField>
    {
        protected override string mainUssClassName => RangeSliderInt.ussClassName;

        protected override List<SliderMark<int>> marks => new List<SliderMark<int>>
        {
            new() {label = "Low", value = 0},
            new() {label = "Middle", value = 5},
            new() {label = "High", value = 10},
        };

        protected override int highValue => 10;

        protected override int lowValue => 0;

        protected override Vector2Int value => new Vector2Int(3, 7);

        protected override int step => 1;

        protected override int shiftStep => 2;

        protected override string highValueToUxml => $"high-value=\"{highValue}\"";

        protected override string valueToUxml => $"min-value=\"{value.x}\" max-value=\"{value.y}\"";
    }

    [TestFixture]
    [TestOf(typeof(SliderFloat))]
    class SliderFloatParseTests
    {
        class ParseSliderFloat : SliderFloat
        {
            public bool Parse(string text, out float v) => ParseStringToValue(text, out v);
        }

        class ParseRangeSliderFloat : RangeSliderFloat
        {
            public bool Parse(string text, out Vector2 v) => ParseStringToValue(text, out v);
        }

        [TestCase("de-DE", "0,5", 0.5f)]
        [TestCase("de-DE", "0.5", 0.5f)]
        [TestCase("en-US", "1000.5", 1000.5f)]
        public void ParseStringToValue_ReadsInvariantThenCurrentCulture(string cultureName, string text, float expected)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                Assert.IsTrue(new ParseSliderFloat().Parse(text, out var value));
                Assert.AreEqual(expected, value, 1e-4f);

                Assert.IsTrue(new ParseRangeSliderFloat().Parse(text + " - " + text, out var range));
                Assert.AreEqual(new Vector2(expected, expected), range);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
