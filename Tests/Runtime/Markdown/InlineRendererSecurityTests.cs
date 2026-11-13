#if APPUI_ENABLE_MARKDOWN
using System;
using System.Linq;
using Markdig;
using NUnit.Framework;
using Unity.AppUI.Markdown;
using Unity.AppUI.Markdown.Renderers.Inlines;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.Markdown
{
    /// <summary>
    /// Markdown reaching these renderers can be untrusted (chat, assistant or mention payloads,
    /// fetched documentation), so inline tags must never become live formatting and only ordinary
    /// web links may become clickable.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(HrefUtils))]
    class InlineRendererSecurityTests
    {
        static readonly MarkdownPipeline s_Pipeline =
            new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoLinks().Build();

        VisualElement m_Root;
        UITKMarkdownRenderer m_Renderer;

        [SetUp]
        public void SetUp()
        {
            m_Root = new VisualElement();
            m_Renderer = new UITKMarkdownRenderer(m_Root);
        }

        string RenderToText(string markdown)
        {
            var doc = Markdig.Markdown.Parse(markdown, s_Pipeline);
            m_Renderer.Render(doc);
            return string.Join("\n", m_Root.Query<TextElement>().ToList().Select(t => t.text));
        }

        [Test]
        [TestCase("http://example.com")]
        [TestCase("https://example.com/page?a=1")]
        [TestCase("HTTPS://EXAMPLE.COM")]
        [TestCase("mailto:jane@example.com")]
        [TestCase("/relative/path")]
        [TestCase("example.com/page")]
        [TestCase("#anchor")]
        public void IsSafeHref_AllowsOrdinaryWebLinks(string url)
        {
            Assert.IsTrue(HrefUtils.IsSafeHref(url));
        }

        [Test]
        [TestCase("javascript:alert(1)")]
        [TestCase("JavaScript:alert(1)")]
        [TestCase("data:text/html;base64,PHN2Zz4=")]
        [TestCase("vbscript:msgbox(1)")]
        [TestCase("file:///etc/passwd")]
        [TestCase("ftp://example.com/f")]
        [TestCase("unityhub://install")]
        [TestCase("//evil.example")]
        [TestCase("/\\evil.example")]
        [TestCase("/\\\\evil.example")]
        [TestCase("\\\\evil.example")]
        [TestCase("\\evil.example")]
        [TestCase("\\/evil.example")]
        [TestCase("path\\to\\thing")]
        [TestCase("")]
        [TestCase(null)]
        public void IsSafeHref_RejectsEverythingElse(string url)
        {
            Assert.IsFalse(HrefUtils.IsSafeHref(url));
        }

        [Test]
        public void IsSafeHref_RejectsControlCharacterSmuggling()
        {
            Assert.IsFalse(HrefUtils.IsSafeHref("java\nscript:alert(1)"));
            Assert.IsFalse(HrefUtils.IsSafeHref("java\tscript:alert(1)"));
        }

        [Test]
        public void IsSafeHref_RejectsOverlongUrls()
        {
            Assert.IsFalse(HrefUtils.IsSafeHref("https://example.com/" + new string('a', HrefUtils.maxHrefLength)));
        }

        [Test]
        public void Link_WithDisallowedScheme_IsNotClickable()
        {
            var text = RenderToText("[Click here](file:///etc/passwd)");

            StringAssert.DoesNotContain("<a href=", text);
            StringAssert.Contains("Click here", text);
        }

        [Test]
        public void Link_WithHttpsScheme_IsClickable()
        {
            var text = RenderToText("[Click here](https://example.com)");

            StringAssert.Contains("<a href=\"https://example.com\">", text);
        }

        [Test]
        public void InlineHtml_IsRenderedAsLiteralText()
        {
            var text = RenderToText("Normal <color=#ff0000><size=40><b>SPOOFED</b></size></color> more");

            // Every injected tag survives as text inside a noparse span rather than being applied.
            StringAssert.Contains("<noparse>", text);
            foreach (var tag in new[] { "<color=#ff0000>", "<size=40>", "<b>" })
            {
                var idx = text.IndexOf(tag, StringComparison.Ordinal);
                Assert.Greater(idx, -1, $"expected {tag} to survive as text");
                var openIdx = text.LastIndexOf("<noparse>", idx, StringComparison.Ordinal);
                var closeIdx = text.LastIndexOf("</noparse>", idx, StringComparison.Ordinal);
                Assert.Greater(openIdx, closeIdx, $"{tag} escaped its noparse span");
            }
        }

        [Test]
        public void CodeSpan_ClosingNoParse_CannotEscapeItsSpan()
        {
            // A code span is ordinary markdown, so its content is untrusted too. Only the
            // surrounding color/bold markup the renderer adds is trusted.
            var text = RenderToText("`</noparse><size=200%>spoof</size>`");

            Assert.IsTrue(text.Contains("​"), "the embedded closing marker should be broken");
            var idx = text.IndexOf("<size=200%>", StringComparison.Ordinal);
            Assert.Greater(idx, -1, "expected the injected tag to survive as text");
            var openIdx = text.LastIndexOf("<noparse>", idx, StringComparison.Ordinal);
            var closeIdx = text.LastIndexOf("</noparse>", idx, StringComparison.Ordinal);
            Assert.Greater(openIdx, closeIdx, "the tag escaped its noparse span");
        }

        [Test]
        public void Text_ClosingNoParse_CannotEscapeItsSpan()
        {
            var text = RenderToText("a</noparse><b>bold</b>");

            // The payload's own closing marker is broken with a zero-width space, so it can never
            // terminate the span the renderer opened.
            Assert.IsTrue(text.Contains("​"), "the embedded closing marker should be broken");
        }
    }
}
#endif
