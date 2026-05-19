#if APPUI_ENABLE_MARKDOWN
using System.Linq;
using Markdig;
using NUnit.Framework;
using Unity.AppUI.Markdown;
using Unity.AppUI.Markdown.Renderers.Blocks;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.Markdown
{
    [TestFixture]
    [TestOf(typeof(UITKMarkdownRenderer))]
    class UITKMarkdownRendererTests
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

        void Render(string markdown)
        {
            var doc = Markdig.Markdown.Parse(markdown, s_Pipeline);
            m_Renderer.Render(doc);
        }

        [Test]
        public void Constructor_SetsRoot()
        {
            Assert.AreEqual(m_Root, m_Renderer.Root);
        }

        [Test]
        public void Render_Paragraph_CreatesTextElement()
        {
            Render("Hello world");

            var paragraph = m_Root.Children().FirstOrDefault();
            Assert.IsNotNull(paragraph);
            Assert.IsTrue(paragraph.ClassListContains(ParagraphRenderer.ussClassName));
        }

        [Test]
        public void Render_Heading_CreatesHeadingElement()
        {
            Render("# Title");

            var heading = m_Root.Children().OfType<Heading>().FirstOrDefault();
            Assert.IsNotNull(heading);
            Assert.IsTrue(heading.ClassListContains(HeadingRenderer.ussClassName));
            Assert.AreEqual(HeadingSize.XXL, heading.size);
        }

        [Test]
        public void Render_HeadingLevels([Values(1, 2, 3, 4, 5, 6)] int level)
        {
            var prefix = new string('#', level);
            Render($"{prefix} H{level}");

            var heading = m_Root.Children().OfType<Heading>().FirstOrDefault();
            Assert.IsNotNull(heading);

            var expected = level switch
            {
                1 => HeadingSize.XXL,
                2 => HeadingSize.XL,
                3 => HeadingSize.L,
                4 => HeadingSize.M,
                5 => HeadingSize.S,
                _ => HeadingSize.XS,
            };
            Assert.AreEqual(expected, heading.size);
        }

        [Test]
        public void Render_FencedCodeBlock_CreatesCodeBlock()
        {
            Render("```csharp\nvar x = 1;\n```");

            var codeBlock = m_Root.Children().OfType<CodeBlock>().FirstOrDefault();
            Assert.IsNotNull(codeBlock);
            Assert.AreEqual("csharp", codeBlock.language);
        }

        [Test]
        public void Render_Quote_CreatesQuoteElement()
        {
            Render("> Some quote");

            var quote = m_Root.Children().OfType<Quote>().FirstOrDefault();
            Assert.IsNotNull(quote);
            Assert.IsTrue(quote.ClassListContains(QuoteBlockRenderer.ussClassName));
        }

        [Test]
        public void Render_UnorderedList_CreatesListContainer()
        {
            Render("- item one\n- item two");

            var list = m_Root.Children().FirstOrDefault(
                e => e.ClassListContains(ListBlockRenderer.ussClassName));
            Assert.IsNotNull(list);
            Assert.IsTrue(list.ClassListContains(ListBlockRenderer.unorderedUssClassName));
        }

        [Test]
        public void Render_OrderedList_CreatesOrderedListContainer()
        {
            Render("1. first\n2. second");

            var list = m_Root.Children().FirstOrDefault(
                e => e.ClassListContains(ListBlockRenderer.ussClassName));
            Assert.IsNotNull(list);
            Assert.IsTrue(list.ClassListContains(ListBlockRenderer.orderedUssClassName));
        }

        [Test]
        public void Render_ThematicBreak_CreatesDivider()
        {
            Render("---");

            var divider = m_Root.Children().OfType<Divider>().FirstOrDefault();
            Assert.IsNotNull(divider);
            Assert.IsTrue(divider.ClassListContains(ThematicBreakRenderer.ussClassName));
        }

        [Test]
        public void Render_EmptyString_AddsNoChildren()
        {
            Render("");

            Assert.AreEqual(0, m_Root.childCount);
        }

        [Test]
        public void Render_MultipleBlocks_PreservesOrder()
        {
            Render("# Title\n\nParagraph\n\n> Quote");

            var children = m_Root.Children().ToList();
            Assert.AreEqual(3, children.Count);
            Assert.IsInstanceOf<Heading>(children[0]);
            Assert.IsTrue(children[1].ClassListContains(ParagraphRenderer.ussClassName));
            Assert.IsInstanceOf<Quote>(children[2]);
        }
    }
}
#endif
