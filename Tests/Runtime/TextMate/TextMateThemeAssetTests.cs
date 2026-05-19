#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System.Reflection;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.TextMate
{
    [TestFixture]
    [TestOf(typeof(TextMateThemeAsset))]
    class TextMateThemeAssetTests
    {
        TextMateThemeAsset m_Asset;

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
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            Assert.That(m_Asset, Is.Not.Null);
            Assert.That(m_Asset, Is.InstanceOf<ScriptableObject>());
        }

        [Test]
        public void JsonContent_ByDefault_ReturnsNull()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            Assert.That(m_Asset.jsonContent, Is.Null);
        }

        [Test]
        public void DisplayName_ByDefault_ReturnsNull()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            Assert.That(m_Asset.displayName, Is.Null);
        }

        [Test]
        public void SetData_WithValidData_SetsAllProperties()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            var json = TextMateTestData.MinimalThemeJson;
            var displayName = "Minimal Theme";

            SetThemeData(m_Asset, json, displayName);

            Assert.That(m_Asset.jsonContent, Is.EqualTo(json));
            Assert.That(m_Asset.displayName, Is.EqualTo(displayName));
        }

        [Test]
        public void SetData_WithNullJson_SetsNullJsonContent()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            SetThemeData(m_Asset, null, "Test Theme");

            Assert.That(m_Asset.jsonContent, Is.Null);
            Assert.That(m_Asset.displayName, Is.EqualTo("Test Theme"));
        }

        [Test]
        public void SetData_WithNullDisplayName_SetsNullDisplayName()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            SetThemeData(m_Asset, TextMateTestData.MinimalThemeJson, null);

            Assert.That(m_Asset.jsonContent, Is.Not.Null);
            Assert.That(m_Asset.displayName, Is.Null);
        }

        [Test]
        public void SetData_CalledMultipleTimes_OverwritesPreviousData()
        {
            m_Asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            SetThemeData(m_Asset, "first", "First Theme");
            SetThemeData(m_Asset, "second", "Second Theme");

            Assert.That(m_Asset.jsonContent, Is.EqualTo("second"));
            Assert.That(m_Asset.displayName, Is.EqualTo("Second Theme"));
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
