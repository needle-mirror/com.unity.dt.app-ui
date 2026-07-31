#if UNITY_EDITOR && APPUI_ENABLE_MARKDOWN
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using Unity.AppUI.VisualDoc;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VisualDocDemoRegistry))]
    class VisualDocDemoTests
    {
        [Test]
        public void Demos_TargetExistingPages()
        {
            var pageIds = new HashSet<string>(VisualDocRegistry.pages.Select(p => p.id));
            foreach (var pageId in VisualDocDemoRegistry.pageIds)
                Assert.That(pageIds, Does.Contain(pageId), $"Demo registered for unknown page '{pageId}'.");
        }

        [Test]
        public void DemoFactories_ProduceElements()
        {
            foreach (var pageId in VisualDocDemoRegistry.pageIds)
            {
                foreach (var demo in VisualDocDemoRegistry.GetDemos(pageId))
                {
                    Assert.That(demo.section, Is.Not.Empty, $"Demo for page '{pageId}' has no section.");
                    Assert.That(demo.factory(), Is.Not.Null, $"Demo for page '{pageId}' returned null.");
                }
            }
        }

        [Test]
        public void PreprocessGuideMarkdown_StripsHtmlImageBlocks()
        {
            const string markdown = "Intro text.\n" +
                "<p align=\"center\">\n" +
                "  <img src=\"images/foo.png\" alt=\"Foo\">\n" +
                "</p>\n" +
                "After image.";

            var processed = VisualDocPageBuilder.PreprocessGuideMarkdown(markdown);

            Assert.That(processed, Does.Not.Contain("<p"));
            Assert.That(processed, Does.Not.Contain("<img"));
            Assert.That(processed, Does.Contain("Intro text."));
            Assert.That(processed, Does.Contain("After image."));
        }

        [Test]
        public void PreprocessGuideMarkdown_StripsMarkdownImages()
        {
            var processed = VisualDocPageBuilder.PreprocessGuideMarkdown(
                "Some text.\n![screenshot](images/foo.png)\nMore text with ![inline](x.png) image.");

            Assert.That(processed, Does.Not.Contain("!["));
            Assert.That(processed, Does.Contain("Some text."));
            Assert.That(processed, Does.Contain("More text with  image."));
        }

        [Test]
        public void PreprocessGuideMarkdown_LeavesFencedCodeUntouched()
        {
            const string markdown = "Text.\n```xml\n<p align=\"center\">\n<img src=\"x\">\n</p>\n```\nEnd.";

            var processed = VisualDocPageBuilder.PreprocessGuideMarkdown(markdown);

            Assert.That(processed, Does.Contain("<p align=\"center\">"));
            Assert.That(processed, Does.Contain("<img src=\"x\">"));
        }
    }
}
#endif
