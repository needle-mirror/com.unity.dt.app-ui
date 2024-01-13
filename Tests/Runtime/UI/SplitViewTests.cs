using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SplitView))]
    class SplitViewTests : VisualElementTests<SplitView>
    {
        protected override string mainUssClassName => SplitView.ussClassName;
        
        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:SplitView>
                <ui:VisualElement />
                <ui:VisualElement />
              </appui:SplitView>",
        };
    }
}
