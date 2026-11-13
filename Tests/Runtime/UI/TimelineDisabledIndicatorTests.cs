using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards the disabled Timeline against indicators that paint the same colour as the lane
    /// behind them.
    /// </summary>
    /// <remarks>
    /// The disabled keyframes, range and playhead once borrowed the button disabled fill,
    /// <c>--appui-alias-actions-background-color-disabled</c>. In App UI Dark and Light that token
    /// resolves to exactly the track background, so every keyframe, selected ones included,
    /// vanished. Nothing errors and the stylesheet still looks token-correct, so the test resolves
    /// both tokens through every theme and compares the colours they land on.
    /// </remarks>
    [TestFixture]
    class TimelineDisabledIndicatorTests
    {
        const string k_Disabled = ".appui-timeline--disabled ";

        /// <summary>
        /// The disabled indicator rules whose fill must stay visible over a lane.
        /// </summary>
        static readonly string[] k_IndicatorSelectors =
        {
            k_Disabled + ".appui-timeline__keyframe-glyph",
            k_Disabled + ".appui-timeline__keyframe.is-selected > .appui-timeline__keyframe-glyph",
            k_Disabled + ".appui-timeline__range-band",
            k_Disabled + ".appui-timeline__range-handle",
            k_Disabled + ".appui-timeline__playhead-line",
            k_Disabled + ".appui-timeline__playhead-handle",
        };

        /// <summary>
        /// What sits behind an indicator: a track's lane, the bare strip where no track is, and
        /// the ruler and group headers that the playhead head and the range band also cross.
        /// </summary>
        static readonly string[] k_BackdropSelectors =
        {
            ".appui-timeline__track-background",
            ".appui-timeline",
            ".appui-timeline__ruler",
            ".appui-timeline__group-header",
        };

        // The theme stylesheets resolve their aliases against these shared ones, in the order the
        // generated .tss imports them.
        static readonly string[] k_SharedThemeFiles =
        {
            "App UI - Globals.uss",
            "App UI - Alias.uss",
            "App UI - Components.uss",
        };

        static readonly Regex k_Comment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
        static readonly Regex k_Rule = new Regex(@"([^{}]+)\{([^}]*)\}");
        static readonly Regex k_Declaration = new Regex(@"(--appui-[A-Za-z0-9_-]+)\s*:\s*([^;]+);");
        static readonly Regex k_VarOnly = new Regex(@"^var\(\s*(--appui-[A-Za-z0-9_-]+)\s*\)$");

        static string StripComments(string source) => k_Comment.Replace(source, "");

        /// <summary>
        /// The last <c>background-color</c> any rule declares for <paramref name="selector"/>,
        /// matching the whole selector inside a comma list so a descendant rule never stands in
        /// for it. Same-specificity rules cascade by source order, so the last one wins.
        /// </summary>
        static string BackgroundToken(string uss, string selector)
        {
            string token = null;
            foreach (Match rule in k_Rule.Matches(StripComments(uss)))
            {
                var selectors = rule.Groups[1].Value.Split(',')
                    .Select(s => Regex.Replace(s.Trim(), @"\s+", " "));
                if (!selectors.Contains(selector))
                    continue;
                var decl = Regex.Match(rule.Groups[2].Value,
                    @"(?:^|;|\s)background-color\s*:\s*var\(\s*(--appui-[A-Za-z0-9_-]+)\s*\)");
                if (decl.Success)
                    token = decl.Groups[1].Value;
            }
            return token;
        }

        static Dictionary<string, string> LoadVars(string themesDir, string themeFile)
        {
            var vars = new Dictionary<string, string>();
            foreach (var file in k_SharedThemeFiles.Append(themeFile))
            {
                var source = StripComments(File.ReadAllText(Path.Combine(themesDir, file)));
                foreach (Match m in k_Declaration.Matches(source))
                    vars[m.Groups[1].Value] = m.Groups[2].Value.Trim();
            }
            return vars;
        }

        static string Resolve(Dictionary<string, string> vars, string token)
        {
            var value = $"var({token})";
            for (var depth = 0; depth < 32; depth++)
            {
                var m = k_VarOnly.Match(value);
                if (!m.Success)
                    return Regex.Replace(value, @"\s+", "").ToLowerInvariant();
                if (!vars.TryGetValue(m.Groups[1].Value, out value))
                    return null;
            }
            return null;
        }

        /// <summary>
        /// Every theme the package ships, in both its <c>:root</c> and its class-scoped form.
        /// A theme is whatever has a generated <c>.tss</c>.
        /// </summary>
        static List<string> ThemeFiles(string themesDir)
        {
            var themes = new List<string>();
            foreach (var tss in Directory.EnumerateFiles(themesDir, "App UI - *.tss").OrderBy(p => p))
            {
                var name = Path.GetFileNameWithoutExtension(tss);
                foreach (var file in new[] { name + ".uss", name + ".class.uss" })
                {
                    if (File.Exists(Path.Combine(themesDir, file)))
                        themes.Add(file);
                }
            }
            return themes;
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void DisabledIndicators_ResolveToAColourOtherThanTheTrackBackground_InEveryTheme()
        {
            var uss = UssSource.Read("Timeline.uss");
            var themesDir = Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Themes");

            var indicators = k_IndicatorSelectors.ToDictionary(s => s, s => BackgroundToken(uss, s));
            foreach (var kv in indicators)
                Assert.IsNotNull(kv.Value, $"Timeline.uss declares no token background-color for {kv.Key}.");

            var backdrops = k_BackdropSelectors.ToDictionary(s => s, s => BackgroundToken(uss, s));
            foreach (var kv in backdrops)
                Assert.IsNotNull(kv.Value, $"Timeline.uss declares no token background-color for {kv.Key}.");

            var themes = ThemeFiles(themesDir);
            Assert.GreaterOrEqual(themes.Count, 6, "Expected the Dark, Light, Cloud and Editor theme stylesheets.");

            var failures = new List<string>();
            foreach (var theme in themes)
            {
                var vars = LoadVars(themesDir, theme);
                foreach (var backdrop in backdrops)
                {
                    var backdropColour = Resolve(vars, backdrop.Value);
                    Assert.IsNotNull(backdropColour, $"{backdrop.Value} does not resolve in {theme}.");

                    foreach (var indicator in indicators)
                    {
                        var colour = Resolve(vars, indicator.Value);
                        Assert.IsNotNull(colour, $"{indicator.Value} does not resolve in {theme}.");
                        if (colour == backdropColour)
                        {
                            failures.Add($"{theme}: '{indicator.Key}' paints {indicator.Value} = {colour}, "
                                + $"the same as '{backdrop.Key}' ({backdrop.Value})");
                        }
                    }
                }
            }

            Assert.IsEmpty(failures,
                "A disabled Timeline indicator resolves to the colour of the lane behind it, so it "
                + "disappears. Use a disabled token that contrasts with the track, such as "
                + "--appui-alias-text-disabled:\n" + string.Join("\n", failures));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void DisabledPlayheadLabel_TextDiffersFromItsFill_InEveryTheme()
        {
            var uss = StripComments(UssSource.Read("Timeline.uss"));
            const string selector = k_Disabled + ".appui-timeline__playhead-label";
            var fill = BackgroundToken(uss, selector);
            Assert.IsNotNull(fill, $"Timeline.uss declares no token background-color for {selector}.");

            string text = null;
            foreach (Match rule in k_Rule.Matches(uss))
            {
                var selectors = rule.Groups[1].Value.Split(',').Select(s => Regex.Replace(s.Trim(), @"\s+", " "));
                if (!selectors.Contains(selector))
                    continue;
                var decl = Regex.Match(rule.Groups[2].Value,
                    @"(?:^|;|\s)color\s*:\s*var\(\s*(--appui-[A-Za-z0-9_-]+)\s*\)");
                if (decl.Success)
                    text = decl.Groups[1].Value;
            }
            Assert.IsNotNull(text, $"Timeline.uss declares no token color for {selector}.");

            var themesDir = Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Themes");
            foreach (var theme in ThemeFiles(themesDir))
            {
                var vars = LoadVars(themesDir, theme);
                Assert.AreNotEqual(Resolve(vars, fill), Resolve(vars, text),
                    $"{theme}: the disabled playhead label paints its text ({text}) in its own fill ({fill}).");
            }
        }
    }
}
