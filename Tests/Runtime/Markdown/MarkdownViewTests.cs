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
        protected override string mainUssClassName => MarkdownView.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            "<appui:MarkdownView/>",
        };
    }
}
#endif
