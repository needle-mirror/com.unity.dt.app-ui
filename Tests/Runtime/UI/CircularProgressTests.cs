using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CircularProgress))]
    class CircularProgressTests : VisualElementTests<CircularProgress>
    {
        protected override string mainUssClassName => CircularProgress.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CircularProgress />",
            @"<appui:CircularProgress size=""M"" buffer-opacity=""0.5"" variant=""Determinate"" value=""0.5"" buffer-value=""0.75"" color-override=""#FF0000"" />",
        };
    }
}
