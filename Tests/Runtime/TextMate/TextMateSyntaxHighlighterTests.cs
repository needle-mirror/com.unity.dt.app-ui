#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using System.Reflection;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(TextMateSyntaxHighlighter))]
    class TextMateSyntaxHighlighterTests
    {
        TextMateGrammarAsset m_GrammarAsset;
        TextMateThemeAsset m_ThemeAsset;
        TextMateSyntaxHighlighter m_Highlighter;

        [SetUp]
        public void SetUp()
        {
            m_GrammarAsset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            m_ThemeAsset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            m_Highlighter?.Dispose();
            m_Highlighter = null;

            if (m_GrammarAsset != null)
            {
                UnityEngine.Object.DestroyImmediate(m_GrammarAsset);
                m_GrammarAsset = null;
            }

            if (m_ThemeAsset != null)
            {
                UnityEngine.Object.DestroyImmediate(m_ThemeAsset);
                m_ThemeAsset = null;
            }
        }

        void SetupValidAssets()
        {
            SetGrammarData(m_GrammarAsset, TextMateTestData.SimpleGrammarJson, "source.simple", "Simple");
            SetThemeData(m_ThemeAsset, TextMateTestData.MinimalThemeJson, "Minimal Theme");
        }

        void SetupMultiLineAssets()
        {
            SetGrammarData(m_GrammarAsset, TextMateTestData.MultiLineGrammarJson, "source.multiline", "MultiLine");
            SetThemeData(m_ThemeAsset, TextMateTestData.MinimalThemeJson, "Minimal Theme");
        }

        // Constructor Tests

        [Test]
        public void Constructor_WithNullGrammarAsset_ThrowsArgumentNullException()
        {
            SetThemeData(m_ThemeAsset, TextMateTestData.MinimalThemeJson, "Theme");

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Highlighter = new TextMateSyntaxHighlighter(null, m_ThemeAsset);
            });
        }

        [Test]
        public void Constructor_WithNullThemeAsset_ThrowsArgumentNullException()
        {
            SetGrammarData(m_GrammarAsset, TextMateTestData.SimpleGrammarJson, "source.simple", "Simple");

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, null);
            });
        }

        [Test]
        public void Constructor_WithValidAssets_CreatesInstance()
        {
            SetupValidAssets();

            Assert.DoesNotThrow(() =>
            {
                m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            });
            Assert.That(m_Highlighter, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithInvalidGrammarJson_ThrowsInvalidOperationException()
        {
            SetGrammarData(m_GrammarAsset, TextMateTestData.InvalidJson, "source.invalid", "Invalid");
            SetThemeData(m_ThemeAsset, TextMateTestData.MinimalThemeJson, "Theme");

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            });
        }

        [Test]
        public void Constructor_WithInvalidThemeJson_ThrowsInvalidOperationException()
        {
            SetGrammarData(m_GrammarAsset, TextMateTestData.SimpleGrammarJson, "source.simple", "Simple");
            SetThemeData(m_ThemeAsset, TextMateTestData.InvalidJson, "Invalid Theme");

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            });
        }

        // Highlight Tests

        [Test]
        public void Highlight_WithNullInput_ReturnsEmptyString()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Highlight_WithEmptyInput_ReturnsEmptyString()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight(string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Highlight_WithKeyword_ReturnsRichTextWithBoldTag()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("if");

            Assert.That(result, Does.Contain("<b>"));
            Assert.That(result, Does.Contain("</b>"));
            Assert.That(result, Does.Contain("<color="));
            Assert.That(result, Does.Contain("if"));
        }

        [Test]
        public void Highlight_WithString_ContainsColorTag()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("\"hello\"");

            Assert.That(result, Does.Contain("<color="));
            Assert.That(result, Does.Contain("</color>"));
        }

        [Test]
        public void Highlight_WithNumber_ContainsColorTag()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("42");

            Assert.That(result, Does.Contain("<color="));
            Assert.That(result, Does.Contain("42"));
        }

        [Test]
        public void Highlight_WithMultipleLines_PreservesNewlines()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            var input = "if\nelse\nwhile";

            var result = m_Highlighter.Highlight(input);

            var newlineCount = result.Split('\n').Length - 1;
            Assert.That(newlineCount, Is.EqualTo(2));
        }

        [Test]
        public void Highlight_WithAngleBrackets_EscapesBrackets()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("<test>");

            // The text should be escaped using zero-width space
            Assert.That(result, Does.Contain("<\u200B"));
            Assert.That(result, Does.Contain(">\u200B"));
        }

        [Test]
        public void Highlight_WithMixedContent_HighlightsCorrectly()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("if (x == 42) return \"done\";");

            Assert.That(result, Does.Contain("<b>")); // keyword styling
            Assert.That(result, Does.Contain("<color="));
        }

        [Test]
        public void Highlight_WithComment_ContainsItalicTag()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.Highlight("// this is a comment");

            Assert.That(result, Does.Contain("<i>"));
            Assert.That(result, Does.Contain("</i>"));
        }

        // HighlightLine Tests

        [Test]
        public void HighlightLine_WithEmptyInput_ReturnsEmptyStringAndPreservesState()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            var prevState = IntPtr.Zero;

            var result = m_Highlighter.HighlightLine(string.Empty, prevState, out var newState);

            Assert.That(result, Is.EqualTo(string.Empty));
            Assert.That(newState, Is.EqualTo(prevState));
        }

        [Test]
        public void HighlightLine_WithNullInput_ReturnsEmptyString()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.HighlightLine(null, IntPtr.Zero, out _);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void HighlightLine_WithInitialZeroState_ReturnsTokenizedOutput()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            var result = m_Highlighter.HighlightLine("return 42", IntPtr.Zero, out var newState);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("<color="));
            Assert.That(newState, Is.Not.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void HighlightLine_OutputsNewStateForNextLine()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            m_Highlighter.HighlightLine("if", IntPtr.Zero, out var state1);
            m_Highlighter.HighlightLine("else", state1, out var state2);

            // States should be valid (non-zero)
            Assert.That(state1, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(state2, Is.Not.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void HighlightLine_WithMultiLineBlockComment_MaintainsStateAcrossLines()
        {
            SetupMultiLineAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            // Start a block comment
            var line1 = m_Highlighter.HighlightLine("/* start comment", IntPtr.Zero, out var state1);
            // Continue in the block comment
            var line2 = m_Highlighter.HighlightLine("still in comment", state1, out var state2);
            // End the block comment
            var line3 = m_Highlighter.HighlightLine("end */", state2, out var state3);

            // All lines should have some output
            Assert.That(line1, Is.Not.Empty);
            Assert.That(line2, Is.Not.Empty);
            Assert.That(line3, Is.Not.Empty);

            // State should propagate
            Assert.That(state1, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(state2, Is.Not.EqualTo(IntPtr.Zero));
            Assert.That(state3, Is.Not.EqualTo(IntPtr.Zero));
        }

        // Unicode / Non-ASCII Tests

        [Test]
        public void Highlight_WithNonAsciiCharacters_PreservesTextAndDoesNotThrow()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            // Emoji characters are multi-byte in UTF-8 (4 bytes each) and
            // may be surrogate pairs in UTF-16 (2 chars each).
            // If the native library returns UTF-8 byte offsets while C# uses
            // UTF-16 char indices, Token.GetValue() will misalign or throw.
            var input = "if (x == \"\U0001F600\U0001F680\") return 42";

            string result = null;
            Assert.DoesNotThrow(() => result = m_Highlighter.Highlight(input));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            // The emojis should survive round-trip through the highlighter
            Assert.That(result, Does.Contain("\U0001F600"));
            Assert.That(result, Does.Contain("\U0001F680"));

            // Keywords and numbers should still be highlighted
            Assert.That(result, Does.Contain("<b>")); // keyword styling
            Assert.That(result, Does.Contain("42"));
        }

        [Test]
        public void Highlight_WithAccentedCharacters_PreservesText()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            // Accented characters are multi-byte in UTF-8 (2-3 bytes) but
            // single char in UTF-16. Tests a different offset mismatch scenario.
            var input = "if (naïveté == \"café\") return 0";

            string result = null;
            Assert.DoesNotThrow(() => result = m_Highlighter.Highlight(input));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            Assert.That(result, Does.Contain("naïveté"));
            Assert.That(result, Does.Contain("café"));
        }

        [Test]
        public void Highlight_WithCJKCharacters_PreservesText()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            // CJK characters are 3 bytes in UTF-8 but 1 char in UTF-16
            var input = "if (名前 == \"テスト\") return 1";

            string result = null;
            Assert.DoesNotThrow(() => result = m_Highlighter.Highlight(input));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            Assert.That(result, Does.Contain("名前"));
            Assert.That(result, Does.Contain("テスト"));
        }

        [Test]
        public void HighlightLine_WithEmojisAfterKeyword_TokenIndicesAlignWithString()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            // The keyword "return" appears before the emoji.
            // If indices are UTF-8 byte offsets, the token for "return" may be
            // correct but subsequent tokens (after the emoji) will have wrong
            // start/end indices when used with C# string.Substring().
            var input = "\U0001F600 return 42";

            string result = null;
            Assert.DoesNotThrow(() => result = m_Highlighter.HighlightLine(input, IntPtr.Zero, out _));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);

            // "return" should still be found and highlighted as a keyword
            Assert.That(result, Does.Contain("return"));
            Assert.That(result, Does.Contain("42"));
        }

        // Disposal Tests

        [Test]
        public void Dispose_WhenCalled_DoesNotThrow()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            Assert.DoesNotThrow(() => m_Highlighter.Dispose());
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);

            m_Highlighter.Dispose();
            Assert.DoesNotThrow(() => m_Highlighter.Dispose());
            Assert.DoesNotThrow(() => m_Highlighter.Dispose());
        }

        [Test]
        public void Highlight_AfterDispose_ThrowsObjectDisposedException()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            m_Highlighter.Dispose();

            Assert.Throws<ObjectDisposedException>(() => m_Highlighter.Highlight("test"));
        }

        [Test]
        public void HighlightLine_AfterDispose_ThrowsObjectDisposedException()
        {
            SetupValidAssets();
            m_Highlighter = new TextMateSyntaxHighlighter(m_GrammarAsset, m_ThemeAsset);
            m_Highlighter.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
                m_Highlighter.HighlightLine("test", IntPtr.Zero, out _));
        }

        // Helper methods

        static void SetGrammarData(TextMateGrammarAsset asset, string json, string scopeName, string displayName)
        {
            var method = typeof(TextMateGrammarAsset).GetMethod("SetData",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(asset, new object[] { json, scopeName, displayName });
        }

        static void SetThemeData(TextMateThemeAsset asset, string json, string displayName)
        {
            var method = typeof(TextMateThemeAsset).GetMethod("SetData",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(asset, new object[] { json, displayName });
        }
    }
}
#endif
