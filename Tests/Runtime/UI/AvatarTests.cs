using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Avatar))]
    class AvatarTests : VisualElementTests<Avatar>
    {
        protected override string mainUssClassName => Avatar.ussClassName;

        // Handed over to AvatarSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Avatar />",
            @"<appui:Avatar size=""Lg"" variant=""Rounded"" background-color=""#FF0000"" outline-width=""3"" outline-color=""#00FF00"" />",
            @"<appui:Avatar label=""MB"" background-color=""#4D70EE"" auto-label-color=""true"" />"
        };
    }
}
