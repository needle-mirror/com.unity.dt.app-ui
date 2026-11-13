using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AvatarGroup))]
    class AvatarGroupTests : VisualElementTests<AvatarGroup>
    {

        // An empty AvatarGroup lays out to nothing. The stack is the component.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var group = new AvatarGroup { max = 3, size = Size.Md };
                    for (var i = 0; i < 5; i++)
                        group.Add(new Unity.AppUI.UI.Avatar { backgroundColor = Color.gray });
                    return group;
                });
            }
        }

        protected override string mainUssClassName => AvatarGroup.ussClassName;

        // Handed over to AvatarGroupSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:AvatarGroup />",
            @"<appui:AvatarGroup size=""Lg"" spacing=""L"" variant=""Rounded"" total=""42"" max=""40"" />"
        };
    }
}
