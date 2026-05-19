#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using NUnit.Framework;
using Unity.AppUI.TextMateLib;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(Grammar))]
    class GrammarTests
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

        // ScopeName Tests

        [Test]
        public void ScopeName_ReturnsCorrectValue()
        {
            Assert.That(m_Grammar.ScopeName, Is.EqualTo("source.simple"));
        }

        // TokenizeLine Tests

        [Test]
        public void TokenizeLine_WithEmptyString_ReturnsValidResult()
        {
            var result = m_Grammar.TokenizeLine(string.Empty, IntPtr.Zero);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Tokens, Is.Not.Null);
        }

        [Test]
        public void TokenizeLine_WithKeyword_ReturnsTokenWithKeywordScope()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.Tokens, Has.Count.GreaterThan(0));

            var hasKeywordScope = false;
            foreach (var token in result.Tokens)
            {
                foreach (var scope in token.Scopes)
                {
                    if (scope.Contains("keyword"))
                    {
                        hasKeywordScope = true;
                        break;
                    }
                }
                if (hasKeywordScope) break;
            }
            Assert.That(hasKeywordScope, Is.True);
        }

        [Test]
        public void TokenizeLine_WithNumber_ReturnsTokenWithNumericScope()
        {
            var result = m_Grammar.TokenizeLine("42", IntPtr.Zero);

            Assert.That(result.Tokens, Has.Count.GreaterThan(0));

            var hasNumericScope = false;
            foreach (var token in result.Tokens)
            {
                foreach (var scope in token.Scopes)
                {
                    if (scope.Contains("constant.numeric"))
                    {
                        hasNumericScope = true;
                        break;
                    }
                }
                if (hasNumericScope) break;
            }
            Assert.That(hasNumericScope, Is.True);
        }

        [Test]
        public void TokenizeLine_WithString_ReturnsTokenWithStringScope()
        {
            var result = m_Grammar.TokenizeLine("\"hello\"", IntPtr.Zero);

            Assert.That(result.Tokens, Has.Count.GreaterThan(0));

            var hasStringScope = false;
            foreach (var token in result.Tokens)
            {
                foreach (var scope in token.Scopes)
                {
                    if (scope.Contains("string"))
                    {
                        hasStringScope = true;
                        break;
                    }
                }
                if (hasStringScope) break;
            }
            Assert.That(hasStringScope, Is.True);
        }

        [Test]
        public void TokenizeLine_TokensCoverEntireLine()
        {
            var line = "if x == 42";
            var result = m_Grammar.TokenizeLine(line, IntPtr.Zero);

            Assert.That(result.Tokens, Has.Count.GreaterThan(0));

            // First token should start at 0
            Assert.That(result.Tokens[0].StartIndex, Is.EqualTo(0));

            // Last token should end at line length
            var lastToken = result.Tokens[result.Tokens.Count - 1];
            Assert.That(lastToken.EndIndex, Is.EqualTo(line.Length));

            // Tokens should be contiguous (no gaps)
            for (int i = 1; i < result.Tokens.Count; i++)
            {
                Assert.That(result.Tokens[i].StartIndex, Is.EqualTo(result.Tokens[i - 1].EndIndex));
            }
        }

        [Test]
        public void TokenizeLine_ReturnsNonZeroStateStack()
        {
            var result = m_Grammar.TokenizeLine("if", IntPtr.Zero);

            Assert.That(result.StateStack, Is.Not.EqualTo(IntPtr.Zero));
        }

        [Test]
        public void TokenizeLine_WithPreviousState_UsesStateForContinuation()
        {
            var line1Result = m_Grammar.TokenizeLine("if", IntPtr.Zero);
            var line2Result = m_Grammar.TokenizeLine("else", line1Result.StateStack);

            Assert.That(line2Result, Is.Not.Null);
            Assert.That(line2Result.Tokens, Has.Count.GreaterThan(0));
        }

        [Test]
        public void TokenizeLine_WithMultipleKeywords_ReturnsMultipleTokens()
        {
            var result = m_Grammar.TokenizeLine("if else while", IntPtr.Zero);

            Assert.That(result.Tokens.Count, Is.GreaterThan(1));
        }

        // TokenizeLines Tests

        [Test]
        public void TokenizeLines_WithNullArray_ReturnsEmptyArray()
        {
            var results = m_Grammar.TokenizeLines(null);

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void TokenizeLines_WithEmptyArray_ReturnsEmptyArray()
        {
            var results = m_Grammar.TokenizeLines(Array.Empty<string>());

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void TokenizeLines_WithMultipleLines_ReturnsResultForEachLine()
        {
            var lines = new[] { "if", "else", "while" };
            var results = m_Grammar.TokenizeLines(lines);

            Assert.That(results.Length, Is.EqualTo(3));
            foreach (var result in results)
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Tokens, Is.Not.Null);
            }
        }

        [Test]
        public void TokenizeLines_PropagatesStateBetweenLines()
        {
            // Setup multi-line grammar for block comment testing
            m_Grammar.Dispose();
            m_Registry.Dispose();

            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(TextMateTestData.MultiLineGrammarJson);
            m_Grammar = m_Registry.LoadGrammar("source.multiline");

            var lines = new[] { "/* start", "middle", "end */" };
            var results = m_Grammar.TokenizeLines(lines);

            // All lines should be tokenized
            Assert.That(results.Length, Is.EqualTo(3));

            // State should propagate (each result should have a valid state)
            foreach (var result in results)
            {
                Assert.That(result.StateStack, Is.Not.EqualTo(IntPtr.Zero));
            }
        }

        // Disposal Tests

        [Test]
        public void Dispose_WhenCalled_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_Grammar.Dispose());
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            m_Grammar.Dispose();
            Assert.DoesNotThrow(() => m_Grammar.Dispose());
            Assert.DoesNotThrow(() => m_Grammar.Dispose());
        }

        [Test]
        public void ScopeName_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Grammar.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                _ = m_Grammar.ScopeName;
            });
        }

        [Test]
        public void TokenizeLine_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Grammar.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Grammar.TokenizeLine("test", IntPtr.Zero);
            });
        }

        [Test]
        public void TokenizeLines_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Grammar.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Grammar.TokenizeLines(new[] { "test" });
            });
        }
    }
}
#endif
