using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonTests : VisualElementTests<ActionButton>
    {
        protected override string mainUssClassName => ActionButton.ussClassName;

        protected override IEnumerable<string> uxmlTestCases
        {
            get
            {
                yield return @"<appui:ActionButton/>";
                yield return @"<appui:ActionButton label=""Test""/>";
                yield return @"<appui:ActionButton icon=""info""/>";
                yield return @"<appui:ActionButton label=""Test"" icon=""info"" icon-variant=""Regular"" size=""M"" accent=""true"" selected=""false"" quiet=""true""/>";
            }
        }
    }
}
