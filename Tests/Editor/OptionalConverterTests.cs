#if ENABLE_UXML_SERIALIZED_DATA
using System.Globalization;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Editor.Tests
{
    [TestFixture]
    [TestOf(typeof(OptionalConverter<>))]
    class OptionalConverterTests
    {
        class OptionalIntConverter : OptionalConverter<int>
        {
            protected override bool TryParse(string value, out int v)
            {
                return int.TryParse(value, out v);
            }
        }

        [Test]
        public void OptionalConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalIntConverter();
            var expected = Optional<int>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalIntConverter();
            var expected = Optional<int>.none;
            var actual = converter.FromString("not an int");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalIntConverter();
            var expected = new Optional<int>(42);
            var actual = converter.FromString("42");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalIntConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<int>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalIntConverter();
            var expected = "42";
            var actual = converter.ToString(new Optional<int>(42));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalEnumConverter<>))]
    class OptionalEnumConverterTests
    {
        class OptionalDirConverter : OptionalEnumConverter<Dir> { }

        [Test]
        public void OptionalEnumConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalDirConverter();
            var expected = OptionalEnum<Dir>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalEnumConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalDirConverter();
            var expected = OptionalEnum<Dir>.none;
            var actual = converter.FromString("not a dir");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalEnumConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalDirConverter();
            var expected = new OptionalEnum<Dir>(Dir.Ltr);
            var actual = converter.FromString("Ltr");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalEnumConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalDirConverter();
            var expected = string.Empty;
            var actual = converter.ToString(OptionalEnum<Dir>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalEnumConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalDirConverter();
            var expected = "Ltr";
            var actual = converter.ToString(new OptionalEnum<Dir>(Dir.Ltr));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalPopoverPlacementConverter))]
    class OptionalPopoverPlacementConverterTests
    {
        [Test]
        public void OptionalPopoverPlacementConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalPopoverPlacementConverter();
            var expected = OptionalEnum<PopoverPlacement>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalPopoverPlacementConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalPopoverPlacementConverter();
            var expected = OptionalEnum<PopoverPlacement>.none;
            var actual = converter.FromString("not a placement");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalPopoverPlacementConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalPopoverPlacementConverter();
            var expected = new OptionalEnum<PopoverPlacement>(PopoverPlacement.Top);
            var actual = converter.FromString("Top");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalPopoverPlacementConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalPopoverPlacementConverter();
            var expected = string.Empty;
            var actual = converter.ToString(OptionalEnum<PopoverPlacement>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalPopoverPlacementConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalPopoverPlacementConverter();
            var expected = "Top";
            var actual = converter.ToString(new OptionalEnum<PopoverPlacement>(PopoverPlacement.Top));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalDirConverter))]
    class OptionalDirConverterTests
    {
        [Test]
        public void OptionalDirConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalDirConverter();
            var expected = OptionalEnum<Dir>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDirConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalDirConverter();
            var expected = OptionalEnum<Dir>.none;
            var actual = converter.FromString("not a dir");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDirConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalDirConverter();
            var expected = new OptionalEnum<Dir>(Dir.Ltr);
            var actual = converter.FromString("Ltr");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDirConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalDirConverter();
            var expected = string.Empty;
            var actual = converter.ToString(OptionalEnum<Dir>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDirConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalDirConverter();
            var expected = "Ltr";
            var actual = converter.ToString(new OptionalEnum<Dir>(Dir.Ltr));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalStringConverter))]
    class OptionalStringConverterTests
    {
        [Test]
        public void OptionalStringConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalStringConverter();
            var expected = Optional<string>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalStringConverter_FromString_ShouldReturnSome_WhenValueIsNotNullOrEmpty()
        {
            var converter = new OptionalStringConverter();
            var expected = new Optional<string>("not empty");
            var actual = converter.FromString("not empty");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalStringConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalStringConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<string>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalStringConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalStringConverter();
            var expected = "not empty";
            var actual = converter.ToString(new Optional<string>("not empty"));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalIntConverter))]
    class OptionalIntConverterTests
    {
        [Test]
        public void OptionalIntConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalIntConverter();
            var expected = Optional<int>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalIntConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalIntConverter();
            var expected = Optional<int>.none;
            var actual = converter.FromString("not an int");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalIntConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalIntConverter();
            var expected = new Optional<int>(42);
            var actual = converter.FromString("42");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalIntConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalIntConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<int>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalIntConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalIntConverter();
            var expected = "42";
            var actual = converter.ToString(new Optional<int>(42));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalFloatConverter))]
    class OptionalFloatConverterTests
    {
        [Test]
        public void OptionalFloatConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalFloatConverter();
            var expected = Optional<float>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalFloatConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalFloatConverter();
            var expected = Optional<float>.none;
            var actual = converter.FromString("not a float");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalFloatConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalFloatConverter();
            var expected = new Optional<float>(42.0f);
            var actual = converter.FromString("42.0");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalFloatConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalFloatConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<float>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalFloatConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalFloatConverter();
            var expected = "42";
            var actual = converter.ToString(new Optional<float>(42.0f));
            Assert.AreEqual(expected, actual);

            expected = "42.5";
            actual = converter.ToString(new Optional<float>(42.5f));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalDoubleConverter))]
    class OptionalDoubleConverterTests
    {
        [Test]
        public void OptionalDoubleConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalDoubleConverter();
            var expected = Optional<double>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDoubleConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalDoubleConverter();
            var expected = Optional<double>.none;
            var actual = converter.FromString("not a double");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDoubleConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalDoubleConverter();
            var expected = new Optional<double>(42.0);
            var actual = converter.FromString("42.0");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDoubleConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalDoubleConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<double>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalDoubleConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalDoubleConverter();
            var expected = "42";
            var actual = converter.ToString(new Optional<double>(42.0));
            Assert.AreEqual(expected, actual);

            expected = "42.5";
            actual = converter.ToString(new Optional<double>(42.5));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalLongConverter))]
    class OptionalLongConverterTests
    {
        [Test]
        public void OptionalLongConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalLongConverter();
            var expected = Optional<long>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalLongConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalLongConverter();
            var expected = Optional<long>.none;
            var actual = converter.FromString("not a long");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalLongConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalLongConverter();
            var expected = new Optional<long>(42);
            var actual = converter.FromString("42");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalLongConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalLongConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<long>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalLongConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalLongConverter();
            var expected = "42";
            var actual = converter.ToString(new Optional<long>(42));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalColorConverter))]
    class OptionalColorConverterTests
    {
        [Test]
        public void OptionalColorConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalColorConverter();
            var expected = Optional<Color>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalColorConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalColorConverter();
            var expected = Optional<Color>.none;
            var actual = converter.FromString("not a color");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalColorConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalColorConverter();
            var expected = new Optional<Color>(Color.red);
            var actual = converter.FromString("red");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalColorConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalColorConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<Color>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalColorConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalColorConverter();
            var expected = "RGBA(1.000, 0.000, 0.000, 1.000)";
            var actual = converter.ToString(new Optional<Color>(Color.red));
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    [TestOf(typeof(OptionalRectConverter))]
    class OptionalRectConverterTests
    {
        [Test]
        public void OptionalRectConverter_FromString_ShouldReturnNone_WhenValueIsNullOrEmpty()
        {
            var converter = new OptionalRectConverter();
            var expected = Optional<Rect>.none;
            var actual = converter.FromString(null);
            Assert.AreEqual(expected, actual);

            actual = converter.FromString(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalRectConverter_FromString_ShouldReturnNone_WhenValueIsNotParsable()
        {
            var converter = new OptionalRectConverter();
            var expected = Optional<Rect>.none;
            var actual = converter.FromString("not a rect");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalRectConverter_FromString_ShouldReturnSome_WhenValueIsParsable()
        {
            var converter = new OptionalRectConverter();
            var expected = new Optional<Rect>(new Rect(0, 0, 100, 100));
            var actual = converter.FromString("0,0,100,100");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalRectConverter_ToString_ShouldReturnEmptyString_WhenValueIsNone()
        {
            var converter = new OptionalRectConverter();
            var expected = string.Empty;
            var actual = converter.ToString(Optional<Rect>.none);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void OptionalRectConverter_ToString_ShouldReturnStringRepresentationOfValue_WhenValueIsSome()
        {
            var converter = new OptionalRectConverter();
            var expected = "0,0,100,100";
            var actual = converter.ToString(new Optional<Rect>(new Rect(0, 0, 100, 100)));
            Assert.AreEqual(expected, actual);
        }

        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        public void NumberConverters_UseInvariantCulture(string cultureName)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                var floatConverter = new Unity.AppUI.Editor.OptionalFloatConverter();
                Assert.AreEqual(new Optional<float>(0.5f), floatConverter.FromString("0.5"));
                Assert.AreEqual("0.5", floatConverter.ToString(new Optional<float>(0.5f)));

                var doubleConverter = new Unity.AppUI.Editor.OptionalDoubleConverter();
                Assert.AreEqual(new Optional<double>(1.25), doubleConverter.FromString("1.25"));

                var rectConverter = new Unity.AppUI.Editor.OptionalRectConverter();
                var rect = new Rect(0.5f, 1.5f, 10, 20);
                var text = rectConverter.ToString(new Optional<Rect>(rect));
                Assert.AreEqual("0.5,1.5,10,20", text);
                Assert.AreEqual(new Optional<Rect>(rect), rectConverter.FromString(text));

                Assert.IsTrue(RectExtensions.TryParse("0.5,1.5,10,20", out var parsed));
                Assert.AreEqual(rect, parsed);

                Assert.IsTrue(GradientExtensions.TryParse("Blend:(0.25,#FF0000);(1,#0000FF)+(0,1);(1,1)", out var gradient));
                Assert.AreEqual(0.25f, gradient.colorKeys[0].time, 1e-4f);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [TestCase("de-DE", "0,5", 0.5)]
        [TestCase("de-DE", "0.5", 0.5)]
        [TestCase("de-DE", "-1,25", -1.25)]
        [TestCase("en-US", "1000.5", 1000.5)]
        [TestCase("en-US", "0.5", 0.5)]
        public void NumberConverters_ReadLegacyCultureValues(string cultureName, string text, double expected)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                Assert.AreEqual(new Optional<float>((float)expected), new Unity.AppUI.Editor.OptionalFloatConverter().FromString(text));
                Assert.AreEqual(new Optional<double>(expected), new Unity.AppUI.Editor.OptionalDoubleConverter().FromString(text));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}

#endif