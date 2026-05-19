#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.TextMateLib;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(Token))]
    class TokenTests
    {
        Registry m_Registry;
        Grammar m_Grammar;

        [SetUp]
        public void SetUp()
        {
            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);
            m_Grammar = m_Registry.LoadGrammar("source.simple");
        }

        [TearDown]
        public void TearDown()
        {
            m_Grammar?.Dispose();
            m_Grammar = null;
            m_Registry?.Dispose();
            m_Registry = null;
        }

        // Token Property Tests

        [Test]
        public void Token_StartIndex_ReturnsCorrectValue()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens[0].StartIndex, Is.EqualTo(0));
        }

        [Test]
        public void Token_EndIndex_ReturnsCorrectValue()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens[0].EndIndex, Is.EqualTo(2));
        }

        [Test]
        public void Token_Length_ReturnsCorrectValue()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens[0].Length, Is.EqualTo(2));
        }

        [Test]
        public void Token_Scopes_ReturnsNonEmptyList()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens[0].Scopes, Is.Not.Null);
            Assert.That(result.Tokens[0].Scopes, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Token_Scopes_ContainsRootScope()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens[0].Scopes, Does.Contain("source.simple"));
        }

        // Token.GetValue Tests

        [Test]
        public void GetValue_WithValidLine_ReturnsCorrectSubstring()
        {
            var line = "if else while";
            var result = m_Grammar.TokenizeLine(line, IntPtr.Zero);

            var firstToken = result.Tokens[0];
            var value = firstToken.GetValue(line);

            Assert.That(value, Is.EqualTo("if"));
        }

        [Test]
        public void GetValue_WithNullLine_ReturnsEmptyString()
        {
            var result = m_Grammar.TokenizeLine("test", IntPtr.Zero);
            var token = result.Tokens[0];

            var value = token.GetValue(null);

            Assert.That(value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetValue_WithEmptyLine_ReturnsEmptyString()
        {
            var result = m_Grammar.TokenizeLine("test", IntPtr.Zero);
            var token = result.Tokens[0];

            var value = token.GetValue(string.Empty);

            Assert.That(value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetValue_ExtractsMultipleTokensCorrectly()
        {
            var line = "if 42";
            var result = m_Grammar.TokenizeLine(line, IntPtr.Zero);

            var extractedValues = new List<string>();
            foreach (var token in result.Tokens)
            {
                extractedValues.Add(token.GetValue(line));
            }

            var combined = string.Join("", extractedValues);
            Assert.That(combined, Is.EqualTo(line));
        }

        // Token.ToString Tests

        [Test]
        public void ToString_ReturnsNonEmptyString()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);
            var token = result.Tokens[0];

            var str = token.ToString();

            Assert.That(str, Is.Not.Null.And.Not.Empty);
            Assert.That(str, Does.Contain("Token"));
        }

        [Test]
        public void ToString_ContainsStartAndEndIndex()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);
            var token = result.Tokens[0];

            var str = token.ToString();

            Assert.That(str, Does.Contain("0"));
            Assert.That(str, Does.Contain("2"));
        }
    }

    [TestFixture]
    [TestOf(typeof(TokenizeResult))]
    class TokenizeResultTests
    {
        Registry m_Registry;
        Grammar m_Grammar;

        [SetUp]
        public void SetUp()
        {
            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);
            m_Grammar = m_Registry.LoadGrammar("source.simple");
        }

        [TearDown]
        public void TearDown()
        {
            m_Grammar?.Dispose();
            m_Grammar = null;
            m_Registry?.Dispose();
            m_Registry = null;
        }

        // TokenizeResult Property Tests

        [Test]
        public void Tokens_ReturnsNonNullList()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens, Is.Not.Null);
        }

        [Test]
        public void Tokens_WithNonEmptyLine_ReturnsNonEmptyList()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens, Has.Count.GreaterThan(0));
        }

        [Test]
        public void StateStack_ReturnsNonZeroPointer()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.StateStack, Is.Not.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void StateStack_CanBeUsedForNextLine()
        {
            var result1 = m_Grammar.TokenizeLine("if", IntPtr.Zero);
            var result2 = m_Grammar.TokenizeLine("else", result1.StateStack);

            Assert.That(result2, Is.Not.Null);
            Assert.That(result2.Tokens, Is.Not.Null);
        }

        [Test]
        public void StoppedEarly_WithShortLine_ReturnsFalse()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.StoppedEarly, Is.False);
        }
    }

    [TestFixture]
    [TestOf(typeof(FontStyle))]
    class FontStyleTests
    {
        [Test]
        public void None_HasValueZero()
        {
            Assert.That((int)FontStyle.None, Is.EqualTo(0));
        }

        [Test]
        public void Italic_HasValueOne()
        {
            Assert.That((int)FontStyle.Italic, Is.EqualTo(1));
        }

        [Test]
        public void Bold_HasValueTwo()
        {
            Assert.That((int)FontStyle.Bold, Is.EqualTo(2));
        }

        [Test]
        public void Underline_HasValueFour()
        {
            Assert.That((int)FontStyle.Underline, Is.EqualTo(4));
        }

        [Test]
        public void CanCombineFlags_BoldAndItalic()
        {
            var combined = FontStyle.Bold | FontStyle.Italic;

            Assert.That((int)combined, Is.EqualTo(3));
            Assert.That(combined.HasFlag(FontStyle.Bold), Is.True);
            Assert.That(combined.HasFlag(FontStyle.Italic), Is.True);
            Assert.That(combined.HasFlag(FontStyle.Underline), Is.False);
        }

        [Test]
        public void CanCombineFlags_AllStyles()
        {
            var combined = FontStyle.Bold | FontStyle.Italic | FontStyle.Underline;

            Assert.That((int)combined, Is.EqualTo(7));
            Assert.That(combined.HasFlag(FontStyle.Bold), Is.True);
            Assert.That(combined.HasFlag(FontStyle.Italic), Is.True);
            Assert.That(combined.HasFlag(FontStyle.Underline), Is.True);
        }

        [Test]
        public void CanCheckIndividualFlag()
        {
            var style = FontStyle.Bold | FontStyle.Underline;

            Assert.That((style & FontStyle.Bold) != 0, Is.True);
            Assert.That((style & FontStyle.Italic) != 0, Is.False);
            Assert.That((style & FontStyle.Underline) != 0, Is.True);
        }
    }
}
#endif
