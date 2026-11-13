#if APPUI_ENABLE_MARKDOWN
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MarkdownView))]
    class MarkdownViewTests : VisualElementTests<MarkdownView>
    {

        // An empty MarkdownView lays out to nothing. The rendered document is the component.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new MarkdownView
                {
                    style = { width = 360 },
                    content = "# Heading\n\nSome **bold** text with a `code` span.\n\n- First\n- Second\n",
                });
            }
        }

        protected override string mainUssClassName => MarkdownView.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            "<appui:MarkdownView/>",
        };
    }
}
#endif
