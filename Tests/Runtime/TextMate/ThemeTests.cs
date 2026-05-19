#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using NUnit.Framework;
using Unity.AppUI.TextMateLib;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(Theme))]
    class ThemeTests
    {
        Theme m_Theme;

        [TearDown]
        public void TearDown()
        {
            m_Theme?.Dispose();
            m_Theme = null;
        }

        // LoadFromJson Tests

        [Test]
        public void LoadFromJson_WithValidJson_ReturnsTheme()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            Assert.That(m_Theme, Is.Not.Null);
        }

        [Test]
        public void LoadFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Theme = Theme.LoadFromJson(null);
            });
        }

        [Test]
        public void LoadFromJson_WithEmptyJson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Theme = Theme.LoadFromJson(string.Empty);
            });
        }

        [Test]
        public void LoadFromJson_WithInvalidJson_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Theme = Theme.LoadFromJson(TextMateTestData.InvalidJson);
            });
        }

        // GetDefaultForeground Tests

        [Test]
        public void GetDefaultForeground_ReturnsNonZeroValue()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var color = m_Theme.GetDefaultForeground();

            // Should return the theme's default foreground (D4D4D4 in the minimal theme)
            Assert.That(color, Is.Not.EqualTo(0u));
        }

        // GetDefaultBackground Tests

        [Test]
        public void GetDefaultBackground_ReturnsValue()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            // Just verify it doesn't throw - background could legitimately be 0
            Assert.DoesNotThrow(() =>
            {
                m_Theme.GetDefaultBackground();
            });
        }

        // GetForeground Tests

        [Test]
        public void GetForeground_WithKnownScope_ReturnsColor()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var color = m_Theme.GetForeground("keyword", 0u);

            // Keyword should have a defined color in the minimal theme
            Assert.That(color, Is.Not.EqualTo(0u));
        }

        [Test]
        public void GetForeground_WithUnknownScope_ReturnsThemeDefaultForeground()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var color = m_Theme.GetForeground("nonexistent.scope.that.does.not.exist", 0u);
            var themeDefault = m_Theme.GetDefaultForeground();

            // Unknown scopes fall back to the theme's default foreground color
            Assert.That(color, Is.EqualTo(themeDefault));
        }

        [Test]
        public void GetForeground_WithNullScope_DoesNotThrow()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            const uint defaultColor = 0xFFFFFFFFu;

            Assert.DoesNotThrow(() =>
            {
                m_Theme.GetForeground(null, defaultColor);
            });
        }

        [Test]
        public void GetForeground_WithEmptyScope_ReturnsThemeDefaultForeground()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var color = m_Theme.GetForeground(string.Empty, 0u);
            var themeDefault = m_Theme.GetDefaultForeground();

            // Empty scope falls back to the theme's default foreground color
            Assert.That(color, Is.EqualTo(themeDefault));
        }

        // GetBackground Tests

        [Test]
        public void GetBackground_WithUnknownScope_ReturnsThemeDefaultBackground()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var color = m_Theme.GetBackground("nonexistent.scope", 0u);
            var themeDefault = m_Theme.GetDefaultBackground();

            // Unknown scopes fall back to the theme's default background color
            Assert.That(color, Is.EqualTo(themeDefault));
        }

        // GetFontStyle Tests

        [Test]
        public void GetFontStyle_WithKeywordScope_ReturnsBold()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var style = m_Theme.GetFontStyle("keyword");

            Assert.That(style & FontStyle.Bold, Is.EqualTo(FontStyle.Bold));
        }

        [Test]
        public void GetFontStyle_WithCommentScope_ReturnsItalic()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var style = m_Theme.GetFontStyle("comment");

            Assert.That(style & FontStyle.Italic, Is.EqualTo(FontStyle.Italic));
        }

        [Test]
        public void GetFontStyle_WithUnknownScope_ReturnsThemeDefaultStyle()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            var style = m_Theme.GetFontStyle("nonexistent.scope", FontStyle.Underline);

            // Unknown scopes fall back to the theme's default font style (None)
            Assert.That(style, Is.EqualTo(FontStyle.None));
        }

        [Test]
        public void GetFontStyle_WithNullScope_DoesNotThrow()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            Assert.DoesNotThrow(() =>
            {
                m_Theme.GetFontStyle(null);
            });
        }

        // Disposal Tests

        [Test]
        public void Dispose_WhenCalled_DoesNotThrow()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            Assert.DoesNotThrow(() => m_Theme.Dispose());
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);

            m_Theme.Dispose();
            Assert.DoesNotThrow(() => m_Theme.Dispose());
            Assert.DoesNotThrow(() => m_Theme.Dispose());
        }

        [Test]
        public void GetForeground_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            m_Theme.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Theme.GetForeground("keyword");
            });
        }

        [Test]
        public void GetBackground_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            m_Theme.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Theme.GetBackground("keyword");
            });
        }

        [Test]
        public void GetFontStyle_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            m_Theme.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Theme.GetFontStyle("keyword");
            });
        }

        [Test]
        public void GetDefaultForeground_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            m_Theme.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Theme.GetDefaultForeground();
            });
        }

        [Test]
        public void GetDefaultBackground_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Theme = Theme.LoadFromJson(TextMateTestData.MinimalThemeJson);
            m_Theme.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Theme.GetDefaultBackground();
            });
        }
    }
}
#endif
