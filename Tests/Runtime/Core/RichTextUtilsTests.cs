using System;
using NUnit.Framework;
using Unity.AppUI.Core;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(RichTextUtils))]
    class RichTextUtilsTests
    {
        const string k_ZeroWidthSpace = "​";

        [Test]
        [TestCase("</noparse>")]
        [TestCase("</NOPARSE>")]
        [TestCase("</ noparse >")]
        [TestCase("< / NoParse >")]
        public void NeutralizeNoParse_BreaksEveryClosingMarkerVariant(string marker)
        {
            var result = RichTextUtils.NeutralizeNoParse($"before{marker}after");

            Assert.IsTrue(result.StartsWith("before"));
            Assert.IsTrue(result.EndsWith("after"));
            Assert.IsTrue(result.Contains("<" + k_ZeroWidthSpace));

            // Ordinal on purpose. StringAssert.DoesNotContain compares culture-sensitively, and a
            // zero-width space is an ignorable character to ICU -- so on Unity 7 the marker still
            // "matches" straight through the one this method inserts, and the assertion fails even
            // though the output is correct. The consumer that matters, the rich text parser, is
            // ordinal, so this is what the guarantee actually is.
            Assert.IsFalse(result.Contains(marker, StringComparison.Ordinal),
                $"Expected the neutralized output not to contain \"{marker}\" ordinally, but was \"{result}\".");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void NeutralizeNoParse_PassesThroughEmptyInput(string input)
        {
            Assert.AreEqual(input, RichTextUtils.NeutralizeNoParse(input));
        }

        [Test]
        public void NeutralizeNoParse_LeavesOpeningMarkerAlone()
        {
            Assert.AreEqual("<noparse>x", RichTextUtils.NeutralizeNoParse("<noparse>x"));
        }

        [Test]
        public void Escape_WrapsAndNeutralizes()
        {
            var result = RichTextUtils.Escape("a</noparse><b>bold</b>");

            Assert.IsTrue(result.StartsWith("<noparse>"));
            Assert.IsTrue(result.EndsWith("</noparse>"));
            // The only closing marker left is the one this method appended.
            Assert.AreEqual(1, CountOccurrences(result, "</noparse>"));
            StringAssert.Contains("<b>bold</b>", result);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void Escape_PassesThroughEmptyInput(string input)
        {
            Assert.AreEqual(input, RichTextUtils.Escape(input));
        }

        static int CountOccurrences(string haystack, string needle)
        {
            var count = 0;
            var i = haystack.IndexOf(needle, System.StringComparison.Ordinal);
            while (i >= 0)
            {
                count++;
                i = haystack.IndexOf(needle, i + needle.Length, System.StringComparison.Ordinal);
            }
            return count;
        }
    }
}
