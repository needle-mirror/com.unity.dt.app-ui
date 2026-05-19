#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System.Reflection;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(TextMateGrammarAsset))]
    class TextMateGrammarAssetTests
    {
        TextMateGrammarAsset m_Asset;

        [TearDown]
        public void TearDown()
        {
            if (m_Asset != null)
            {
                Object.DestroyImmediate(m_Asset);
                m_Asset = null;
            }
        }

        [Test]
        public void CreateInstance_ReturnsValidScriptableObject()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            Assert.That(m_Asset, Is.Not.Null);
            Assert.That(m_Asset, Is.InstanceOf<ScriptableObject>());
        }

        [Test]
        public void JsonContent_ByDefault_ReturnsNull()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            Assert.That(m_Asset.jsonContent, Is.Null);
        }

        [Test]
        public void ScopeName_ByDefault_ReturnsNull()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            Assert.That(m_Asset.scopeName, Is.Null);
        }

        [Test]
        public void DisplayName_ByDefault_ReturnsNull()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            Assert.That(m_Asset.displayName, Is.Null);
        }

        [Test]
        public void SetData_WithValidData_SetsAllProperties()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            var json = TextMateTestData.SimpleGrammarJson;
            var scopeName = "source.simple";
            var displayName = "Simple";

            SetGrammarData(m_Asset, json, scopeName, displayName);

            Assert.That(m_Asset.jsonContent, Is.EqualTo(json));
            Assert.That(m_Asset.scopeName, Is.EqualTo(scopeName));
            Assert.That(m_Asset.displayName, Is.EqualTo(displayName));
        }

        [Test]
        public void SetData_WithNullJson_SetsNullJsonContent()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            SetGrammarData(m_Asset, null, "source.test", "Test");

            Assert.That(m_Asset.jsonContent, Is.Null);
            Assert.That(m_Asset.scopeName, Is.EqualTo("source.test"));
            Assert.That(m_Asset.displayName, Is.EqualTo("Test"));
        }

        [Test]
        public void SetData_WithNullScopeName_SetsNullScopeName()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            SetGrammarData(m_Asset, TextMateTestData.SimpleGrammarJson, null, "Test");

            Assert.That(m_Asset.jsonContent, Is.Not.Null);
            Assert.That(m_Asset.scopeName, Is.Null);
        }

        [Test]
        public void SetData_CalledMultipleTimes_OverwritesPreviousData()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            SetGrammarData(m_Asset, "first", "scope.first", "First");
            SetGrammarData(m_Asset, "second", "scope.second", "Second");

            Assert.That(m_Asset.jsonContent, Is.EqualTo("second"));
            Assert.That(m_Asset.scopeName, Is.EqualTo("scope.second"));
            Assert.That(m_Asset.displayName, Is.EqualTo("Second"));
        }

        static void SetGrammarData(TextMateGrammarAsset asset, string json, string scopeName, string displayName)
        {
            var method = typeof(TextMateGrammarAsset).GetMethod("SetData",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(asset, new object[] { json, scopeName, displayName });
        }
    }
}
#endif
