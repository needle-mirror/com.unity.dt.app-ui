#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using NUnit.Framework;
using Unity.AppUI.TextMateLib;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(Registry))]
    class RegistryTests
    {
        Registry m_Registry;

        [TearDown]
        public void TearDown()
        {
            m_Registry?.Dispose();
            m_Registry = null;
        }

        // Constructor Tests

        [Test]
        public void Constructor_CreatesValidInstance()
        {
            Assert.DoesNotThrow(() =>
            {
                m_Registry = new Registry();
            });
            Assert.That(m_Registry, Is.Not.Null);
        }

        // AddGrammarFromJson Tests

        [Test]
        public void AddGrammarFromJson_WithValidJson_DoesNotThrow()
        {
            m_Registry = new Registry();

            Assert.DoesNotThrow(() =>
            {
                m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);
            });
        }

        [Test]
        public void AddGrammarFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            m_Registry = new Registry();

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Registry.AddGrammarFromJson(null);
            });
        }

        [Test]
        public void AddGrammarFromJson_WithEmptyJson_ThrowsArgumentNullException()
        {
            m_Registry = new Registry();

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Registry.AddGrammarFromJson(string.Empty);
            });
        }

        [Test]
        public void AddGrammarFromJson_WithInvalidJson_ThrowsInvalidOperationException()
        {
            m_Registry = new Registry();

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Registry.AddGrammarFromJson(TextMateTestData.InvalidJson);
            });
        }

        [Test]
        public void AddGrammarFromJson_CalledMultipleTimes_DoesNotThrow()
        {
            m_Registry = new Registry();

            Assert.DoesNotThrow(() =>
            {
                m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);
                m_Registry.AddGrammarFromJson(TextMateTestData.MultiLineGrammarJson);
            });
        }

        // LoadGrammar Tests

        [Test]
        public void LoadGrammar_WithRegisteredScope_ReturnsGrammar()
        {
            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);

            var grammar = m_Registry.LoadGrammar("source.simple");

            Assert.That(grammar, Is.Not.Null);
            Assert.That(grammar.ScopeName, Is.EqualTo("source.simple"));
        }

        [Test]
        public void LoadGrammar_WithNullScopeName_ThrowsArgumentNullException()
        {
            m_Registry = new Registry();

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Registry.LoadGrammar(null);
            });
        }

        [Test]
        public void LoadGrammar_WithEmptyScopeName_ThrowsArgumentNullException()
        {
            m_Registry = new Registry();

            Assert.Throws<ArgumentNullException>(() =>
            {
                m_Registry.LoadGrammar(string.Empty);
            });
        }

        [Test]
        public void LoadGrammar_WithUnregisteredScope_ThrowsInvalidOperationException()
        {
            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);

            Assert.Throws<InvalidOperationException>(() =>
            {
                m_Registry.LoadGrammar("source.nonexistent");
            });
        }

        // Disposal Tests

        [Test]
        public void Dispose_WhenCalled_DoesNotThrow()
        {
            m_Registry = new Registry();

            Assert.DoesNotThrow(() => m_Registry.Dispose());
        }

        [Test]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            m_Registry = new Registry();

            m_Registry.Dispose();
            Assert.DoesNotThrow(() => m_Registry.Dispose());
            Assert.DoesNotThrow(() => m_Registry.Dispose());
        }

        [Test]
        public void AddGrammarFromJson_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Registry = new Registry();
            m_Registry.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Registry.AddGrammarFromJson(TextMateTestData.SimpleGrammarJson);
            });
        }

        [Test]
        public void LoadGrammar_AfterDispose_ThrowsObjectDisposedException()
        {
            m_Registry = new Registry();
            m_Registry.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                m_Registry.LoadGrammar("source.simple");
            });
        }
    }
}
#endif
