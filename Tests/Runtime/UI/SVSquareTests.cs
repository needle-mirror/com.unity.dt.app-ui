using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SVSquare))]
    class SVSquareTests : VisualElementTests<SVSquare>
    {

        protected override string mainUssClassName => SVSquare.ussClassName;

        // Handed over to SVSquareSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // The square is drawn by a shader into whatever box it is given, and it has no intrinsic
        // size, so an unsized one captures nothing at all.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => Sized());
                yield return new Story("Disabled", (ctx) =>
                {
                    var square = Sized();
                    square.SetEnabled(false);
                    return square;
                });
            }
        }

        static SVSquare Sized() => new SVSquare { style = { width = 160, height = 160 } };

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:SVSquare />",
            @"<appui:SVSquare enabled=""false"" />",
        };
    }
}
