using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards the 10px text floor in the font-size steps the density stylesheets declare.
    /// </summary>
    /// <remarks>
    /// 10px is the smallest text that stays readable on a 96dpi screen, so steps 25 to 100 never
    /// resolve below it in any density. Compact keeps step 100 at 12px, level with step 200.
    /// The values come from <c>@unity/app-ui-tokens</c>; the web side has the same check in
    /// <c>tests/unit/font-size-floor.test.ts</c>.
    /// </remarks>
    [TestFixture]
    class FontSizeFloorTests
    {
        const float k_Floor = 10f;
        static readonly int[] k_SmallSteps = { 25, 50, 75, 100 };

        static readonly string[] k_DensityFiles =
        {
            "Themes/App UI - Globals.uss",
            "Themes/App UI - Compact.uss",
            "Themes/App UI - Compact.class.uss",
            "Themes/App UI - Comfort.uss",
            "Themes/App UI - Comfort.class.uss",
        };

        static float? Step(string uss, int step)
        {
            var m = Regex.Match(uss, $@"--appui-font-sizes-{step}:\s*([\d.]+)px");
            return m.Success ? float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : null;
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void SmallFontSizeSteps_NeverGoBelowTenPx_InAnyDensity()
        {
            var globals = UssSource.ReadStylesheet("Themes/App UI - Globals.uss");
            foreach (var file in k_DensityFiles)
            {
                var uss = UssSource.ReadStylesheet(file);
                foreach (var step in k_SmallSteps)
                {
                    // A density file that does not redeclare a step inherits the global value.
                    var px = Step(uss, step) ?? Step(globals, step);
                    Assert.IsTrue(px.HasValue, $"--appui-font-sizes-{step} is not declared.");
                    Assert.GreaterOrEqual(px.Value, k_Floor,
                        $"In {file}, --appui-font-sizes-{step} is {px}px, below the 10px text floor.");
                }
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void CompactStep100_IsTwelvePx()
        {
            foreach (var file in new[] { "Themes/App UI - Compact.uss", "Themes/App UI - Compact.class.uss" })
                Assert.AreEqual(12f, Step(UssSource.ReadStylesheet(file), 100),
                    $"In {file}, --appui-font-sizes-100 must stay 12px.");
        }
    }
}
