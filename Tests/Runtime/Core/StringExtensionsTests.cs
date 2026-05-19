using NUnit.Framework;
using Unity.AppUI.Core;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(StringExtensions))]
    class StringExtensionsTests
    {
        [Test]
        [TestCase(null, "?")]
        [TestCase("", "?")]
        public void GetInitials_ShouldReturnQuestionMark_WhenNullOrEmpty(string input, string expected)
        {
            Assert.AreEqual(expected, input.GetInitials());
        }

        [Test]
        [TestCase("A", "A")]
        [TestCase("z", "Z")]
        public void GetInitials_ShouldReturnSingleUpperChar_WhenOneCharacter(string input, string expected)
        {
            Assert.AreEqual(expected, input.GetInitials());
        }

        [Test]
        [TestCase("ab", "AB")]
        [TestCase("john", "JO")]
        [TestCase("Ab", "AB")]
        public void GetInitials_ShouldReturnFirstTwoCharsUppercased_WhenSingleWord(string input, string expected)
        {
            Assert.AreEqual(expected, input.GetInitials());
        }

        [Test]
        [TestCase("John Doe", "JD")]
        [TestCase("jane smith", "JS")]
        [TestCase("Alice Bob Charlie", "AB")]
        public void GetInitials_ShouldReturnFirstLetterOfFirstTwoWords_WhenMultipleWords(string input, string expected)
        {
            Assert.AreEqual(expected, input.GetInitials());
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        public void Capitalize_ShouldReturnSameValue_WhenNullOrEmpty(string input, string expected)
        {
            Assert.AreEqual(expected, input.Capitalize());
        }

        [Test]
        [TestCase("a", "A")]
        [TestCase("z", "Z")]
        [TestCase("A", "A")]
        public void Capitalize_ShouldUppercaseFirstChar_WhenSingleCharacter(string input, string expected)
        {
            Assert.AreEqual(expected, input.Capitalize());
        }

        [Test]
        [TestCase("hello", "Hello")]
        [TestCase("Hello", "Hello")]
        [TestCase("hELLO", "HELLO")]
        [TestCase("already Capitalized", "Already Capitalized")]
        public void Capitalize_ShouldUppercaseOnlyFirstChar_WhenMultipleCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, input.Capitalize());
        }
    }
}
