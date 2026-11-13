using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Styles))]
    public class StylesTests
    {
        [Test]
        [TestCase(Size.Sm, ExpectedResult = IconSize.Sm)]
        [TestCase(Size.Md, ExpectedResult = IconSize.Md)]
        [TestCase(Size.Lg, ExpectedResult = IconSize.Lg)]
        public IconSize ToIconSize_ValidSize_ReturnsIconSize(Size size)
        {
            return size.ToIconSize();
        }

        [Test]
        public void ToIconSize_InvalidSize_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ((Size) 100).ToIconSize());
        }

        [Test]
        [TestCase(IconSize.Sm, ExpectedResult = Size.Sm)]
        [TestCase(IconSize.Md, ExpectedResult = Size.Md)]
        [TestCase(IconSize.Lg, ExpectedResult = Size.Lg)]
        public Size ToSize_ValidIconSize_ReturnsSize(IconSize size)
        {
            return size.ToSize();
        }

        /// <summary>
        /// <see cref="IconSize.Xxs"/> and <see cref="IconSize.Xs"/> are glyph-only steps with no
        /// control-size counterpart, so the conversion must refuse rather than round to Sm.
        /// </summary>
        [Test]
        [TestCase(IconSize.Xxs)]
        [TestCase(IconSize.Xs)]
        public void ToSize_GlyphOnlyIconSize_ThrowsException(IconSize size)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => size.ToSize());
        }

        [Test]
        public void ToSize_InvalidIconSize_ThrowsException()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ((IconSize) 100).ToSize());
        }
    }
}
