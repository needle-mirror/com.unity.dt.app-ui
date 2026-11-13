using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Checkbox))]
    class CheckboxTests : VisualElementTests<Checkbox>
    {
        protected override string mainUssClassName => Checkbox.ussClassName;

        // Handed over to CheckboxSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Checkbox />",
            @"<appui:Checkbox label=""Checkbox"" />",
            @"<appui:Checkbox label=""Checkbox"" size=""Md"" />",
            @"<appui:Checkbox label=""Checkbox"" size=""Md"" value=""Checked"" emphasized=""true"" invalid=""false"" />",
        };

    }
}
