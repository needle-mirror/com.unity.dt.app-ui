#if UNITY_EDITOR && APPUI_ENABLE_MARKDOWN
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.VisualDoc;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.VisualDoc
{
    /// <summary>
    /// <see cref="VisualDocPageBuilder.BuildGuidePage"/> combines its file-name argument with the
    /// package documentation root and reads the result, so the resolved path must be confirmed to
    /// still be inside that root.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(VisualDocPageBuilder))]
    class VisualDocPageBuilderTests
    {
        [Test]
        [TestCase("../../../../../../etc/passwd")]
        [TestCase("../package.json")]
        [TestCase("subdir/../../package.json")]
        [TestCase("/etc/passwd")]
        [TestCase(@"C:\Windows\win.ini")]
        public void BuildGuidePage_RefusesToEscapeTheDocumentationRoot(string fileName)
        {
            var container = new VisualElement();

            VisualDocPageBuilder.BuildGuidePage(container, fileName);

            var labels = container.Children().OfType<Label>().ToList();
            Assert.AreEqual(1, labels.Count, "expected only the not-found message");
            StringAssert.StartsWith("Documentation file not found", labels[0].text);
        }
    }
}
#endif
