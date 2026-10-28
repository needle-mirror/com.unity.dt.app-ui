#if ENABLE_UXML_SERIALIZED_DATA
using System.Globalization;
using NUnit.Framework;
using Unity.AppUI.Core;

namespace Unity.AppUI.Editor.Tests
{
    [TestFixture]
    [TestOf(typeof(DateConverter))]
    class DateConverterTests
    {
        [TestCase("en-GB")]
        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        [TestCase("th-TH")]
        public void DateConverter_RoundTrips_UnderAnyCulture(string cultureName)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                var converter = new DateConverter();
                foreach (var date in new[] { new Date(2024, 3, 5), new Date(2024, 3, 25) })
                {
                    Assert.AreEqual(date, converter.FromString(converter.ToString(date)));
                }

                var rangeConverter = new DateRangeConverter();
                var range = new DateRange(new Date(2024, 3, 5), new Date(2024, 3, 25));
                Assert.AreEqual(range, rangeConverter.FromString(rangeConverter.ToString(range)));
                Assert.IsTrue(DateRange.TryParse("2024-03-05,2024-03-25", out var parsed));
                Assert.AreEqual(range, parsed);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}

#endif
