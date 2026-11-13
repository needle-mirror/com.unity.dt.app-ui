using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorWheel))]
    class ColorWheelTests : VisualElementTests<ColorWheel>
    {

        protected override string mainUssClassName => ColorWheel.ussClassName;

        // Handed over to ColorWheelSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        // The wheel is drawn by a shader into whatever box it is given, and it has no intrinsic
        // size, so an unsized one captures nothing at all.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => Sized());
                yield return new Story("Disabled", (ctx) =>
                {
                    var wheel = Sized();
                    wheel.SetEnabled(false);
                    return wheel;
                });
            }
        }

        static ColorWheel Sized() => new ColorWheel { style = { width = 160, height = 160 } };

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ColorWheel />",
            @"<appui:ColorWheel value=""0"" opacity=""1"" brightness=""1"" saturation=""1"" inner-radius=""0.4"" checker-size=""4"" checker-color-1=""#FFF"" checker-color-2=""#000"" increment-factor=""0.1""  />",
            @"<appui:ColorWheel enabled=""false"" />",
        };
    }
}
