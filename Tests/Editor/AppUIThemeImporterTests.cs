using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Editor.Theming;

namespace Unity.AppUI.Tests.Editor
{
    [TestFixture]
    [TestOf(typeof(AppUIThemeUssWriter))]
    class AppUIThemeUssWriterTests
    {
        [TestCase("darkBlue", ExpectedResult = true)]
        [TestCase("dark-blue", ExpectedResult = true)]
        [TestCase("_private", ExpectedResult = true)]
        [TestCase("a", ExpectedResult = true)]
        [TestCase("theme2", ExpectedResult = true)]
        [TestCase("2cool", ExpectedResult = false)]
        [TestCase("-2cool", ExpectedResult = false)]
        [TestCase("my theme", ExpectedResult = false)]
        [TestCase("my.theme", ExpectedResult = false)]
        [TestCase("my/theme", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool ValidatesThemeNamesAsUssIdentifiers(string name)
        {
            return AppUIThemeUssWriter.IsValidThemeName(name);
        }

        [TestCase("#fff", ExpectedResult = true)]
        [TestCase("#ffffff", ExpectedResult = true)]
        [TestCase("#ffffffff", ExpectedResult = true)]
        [TestCase("#0892FF", ExpectedResult = true)]
        [TestCase("rgb(8, 146, 255)", ExpectedResult = true)]
        [TestCase("rgba(8, 146, 255, 0.5)", ExpectedResult = true)]
        [TestCase("not-a-color", ExpectedResult = false)]
        [TestCase("#12345", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool ValidatesColorLiterals(string value)
        {
            return AppUIThemeUssWriter.IsValidColor(value);
        }

        [Test]
        public void AllShippedThemeNamesAreRecognisedAsBuiltIn()
        {
            // Guards the regression this replaced: the theme dropdown used to omit cloud-dark and
            // cloud-light even though both ship and are wired into App UI.tss.
            Assert.That(AppUIThemeUssWriter.builtInThemes, Is.EquivalentTo(new[]
            {
                "dark", "light", "editor-dark", "editor-light", "cloud-dark", "cloud-light",
            }));
        }

        [Test]
        public void WritesTokensUnderTheThemeClass()
        {
            var uss = AppUIThemeUssWriter.WriteStyleSheet("darkBlue", new Dictionary<string, string>
            {
                { "accent-100", "#0892ff" },
                { "neutral-100", "#1d1d1d" },
            });

            Assert.That(uss, Does.Contain(".appui--darkBlue {"));
            Assert.That(uss, Does.Contain("  --appui-accent-100: #0892ff;"));
            Assert.That(uss, Does.Contain("  --appui-neutral-100: #1d1d1d;"));
            Assert.That(uss, Does.Contain("Do not edit directly"));
        }

        [Test]
        public void WritesTokensUnderRootWhenRootScoped()
        {
            var uss = AppUIThemeUssWriter.WriteStyleSheet(
                "darkBlue",
                new Dictionary<string, string> { { "accent-100", "#0892ff" } },
                rootScoped: true);

            Assert.That(uss, Does.Contain(":root {"));
            Assert.That(uss, Does.Not.Contain(".appui--darkBlue"));
        }

        [Test]
        public void TokenOrderIsStableRegardlessOfInputOrder()
        {
            // The token map comes out of a JSON object, so its order is not something a generated
            // file should inherit — otherwise the asset churns on every reimport.
            var forwards = AppUIThemeUssWriter.WriteStyleSheet("t", new Dictionary<string, string>
            {
                { "accent-100", "#111111" },
                { "neutral-100", "#222222" },
            });
            var backwards = AppUIThemeUssWriter.WriteStyleSheet("t", new Dictionary<string, string>
            {
                { "neutral-100", "#222222" },
                { "accent-100", "#111111" },
            });

            Assert.That(forwards, Is.EqualTo(backwards));
        }

        [Test]
        public void ThemeStyleSheetImportsAppUIThenTheThemeItself()
        {
            var tss = AppUIThemeUssWriter.WriteThemeStyleSheet(
                "darkBlue", new Dictionary<string, string> { { "accent-100", "#0892ff" } });

            var appUI = tss.IndexOf("App UI.uss", System.StringComparison.Ordinal);
            var theme = tss.IndexOf(".appui--darkBlue", System.StringComparison.Ordinal);

            Assert.That(tss, Does.Contain("unity-theme://default"));
            Assert.That(appUI, Is.GreaterThan(-1), "the component styles must be imported");
            Assert.That(theme, Is.GreaterThan(appUI), "the theme's tokens must come last so they win");
            // Unity's theme importer needs a rule to emit a valid ThemeStyleSheet.
            Assert.That(tss, Does.Contain("VisualElement {}"));
        }

        [Test]
        public void ThemeStyleSheetImportsTheDensitySheets()
        {
            // Density is orthogonal to colour and lives in its own sheets, which is why the
            // package's own App UI.tss imports all three. Omitting them left a theme applied
            // through PanelSettings with nothing behind .appui--compact or .appui--comfort, so
            // density switching silently stopped working for that panel.
            var tss = AppUIThemeUssWriter.WriteThemeStyleSheet(
                "darkBlue", new Dictionary<string, string> { { "accent-100", "#0892ff" } });

            foreach (var sheet in new[] { "Normal", "Compact", "Comfort" })
            {
                Assert.That(tss, Does.Contain($"App UI - {sheet}.class.uss"),
                    $"the {sheet} density sheet must be imported");
            }

            var components = tss.IndexOf("App UI.uss", System.StringComparison.Ordinal);
            var density = tss.IndexOf("App UI - Compact.class.uss", System.StringComparison.Ordinal);
            Assert.That(density, Is.GreaterThan(components),
                "density overrides sizing, so it must come after the component styles");
        }

        [Test]
        public void ThemeStyleSheetDeclaresTokensAtRootSoItWorksUnaided()
        {
            // A freshly constructed Panel sits on the `appui--dark` class, and this file
            // deliberately does not import App UI's dark theme. With only a class-scoped rule, a
            // project that assigned this .tss to its PanelSettings and nothing else would get the
            // alias layer with no ramp underneath it and render unstyled.
            var tss = AppUIThemeUssWriter.WriteThemeStyleSheet(
                "darkBlue", new Dictionary<string, string> { { "accent-100", "#0892ff" } });

            Assert.That(tss, Does.Match(@"(?m)^:root \{$"),
                "the theme must also be declared at :root to apply without setting Panel.theme");
            Assert.That(tss, Does.Match(@"(?m)^\.appui--darkBlue \{$"),
                "the class rule must remain so Panel.theme can select this theme");

            // Both rules carry the token, so either route resolves it.
            Assert.That(
                System.Text.RegularExpressions.Regex.Matches(tss, "--appui-accent-100:").Count,
                Is.EqualTo(2));
        }

        [Test]
        public void GeneratedSheetsUseLfLineEndingsOnEveryPlatform()
        {
            // The same .appuitheme.json must generate the same bytes on Windows as elsewhere.
            var tokens = new Dictionary<string, string> { { "accent-100", "#0892ff" } };
            Assert.That(AppUIThemeUssWriter.WriteStyleSheet("darkBlue", tokens), Does.Not.Contain("\r"));
            Assert.That(AppUIThemeUssWriter.WriteThemeStyleSheet("darkBlue", tokens), Does.Not.Contain("\r"));
        }

        [TestCase("#0892ff", ExpectedResult = true)]
        [TestCase("rgba(206, 206, 206, 0.08)", ExpectedResult = true)]
        [TestCase("24px", ExpectedResult = true)]
        [TestCase("7", ExpectedResult = true)]
        [TestCase("-0.5", ExpectedResult = true)]
        [TestCase("50%", ExpectedResult = true)]
        [TestCase("var(--appui-sizing-500)", ExpectedResult = true)]
        [TestCase("var(--grain-row-h)", ExpectedResult = true)]
        [TestCase("calc(var(--appui-sizing-75) * 2)", ExpectedResult = false)]
        [TestCase("oklch(0.2 0 250)", ExpectedResult = false)]
        [TestCase("var(--a, 1px)", ExpectedResult = false)]
        [TestCase("24px; } VisualElement { color: red", ExpectedResult = false)]
        [TestCase("none", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool ValidatesValuesUssCanRead(string value)
        {
            return AppUIThemeUssWriter.IsValidValue(value);
        }

        [Test]
        public void WritesConsumerPropertiesUnderTheirFullName()
        {
            // A theme family carries its consumer's own tokens next to App UI's. They keep their
            // name, and a density-relative metric arrives as a bare reference USS can read.
            var uss = AppUIThemeUssWriter.WriteStyleSheet("grain-dark", new Dictionary<string, string>
            {
                { "accent-100", "#0892ff" },
                { "alias-inspector-row-height", "var(--appui-sizing-500)" },
                { "--grain-row-h", "24px" },
                { "--canvas", "#1f1f1f" },
            });

            Assert.That(uss, Does.Contain(".appui--grain-dark {"));
            Assert.That(uss, Does.Contain("  --appui-alias-inspector-row-height: var(--appui-sizing-500);"));
            Assert.That(uss, Does.Contain("  --grain-row-h: 24px;"));
            Assert.That(uss, Does.Contain("  --canvas: #1f1f1f;"));
            Assert.That(uss, Does.Not.Contain("--appui---"));
        }

        [TestCase("accent-100", ExpectedResult = "--appui-accent-100")]
        [TestCase("--grain-row-h", ExpectedResult = "--grain-row-h")]
        public string MapsTokenNamesToTheirProperty(string name)
        {
            return AppUIThemeUssWriter.PropertyNameOf(name);
        }

        [TestCase("accent-100", ExpectedResult = true)]
        [TestCase("color-dark-gray-1100", ExpectedResult = true)]
        [TestCase("alias-surface-card", ExpectedResult = true)]
        [TestCase("a", ExpectedResult = true)]
        [TestCase("--grain-row-h", ExpectedResult = true)]
        [TestCase("--Tab_Case", ExpectedResult = true)]
        [TestCase("--", ExpectedResult = false)]
        [TestCase("--row h", ExpectedResult = false)]
        [TestCase("--row:h", ExpectedResult = false)]
        [TestCase("Accent-100", ExpectedResult = false)]
        [TestCase("accent_100", ExpectedResult = false)]
        [TestCase("accent--100", ExpectedResult = false)]
        [TestCase("-accent", ExpectedResult = false)]
        [TestCase("accent-", ExpectedResult = false)]
        [TestCase("accent 100", ExpectedResult = false)]
        [TestCase("accent:100", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool ValidatesTokenNames(string name)
        {
            return AppUIThemeUssWriter.IsValidTokenName(name);
        }

        [Test]
        public void ATokenNameCannotBreakOutOfTheThemeRule()
        {
            // A name is written into the stylesheet as the suffix of a custom property. One
            // carrying USS punctuation would close the theme's rule and open another, so an
            // imported theme could restyle the whole application from outside its own selector.
            var uss = AppUIThemeUssWriter.WriteStyleSheet("evil", new Dictionary<string, string>
            {
                { "accent-100", "#0892ff" },
                { "accent-100: #000; }\nVisualElement { background-color", "#ff0000" },
            });

            Assert.That(uss, Does.Not.Contain("VisualElement {"),
                "the generated sheet must not contain a rule the theme did not ask for");
            Assert.That(
                System.Text.RegularExpressions.Regex.Matches(uss, @"\{").Count, Is.EqualTo(1),
                "exactly one rule should have been written");
            Assert.That(uss, Does.Contain("--appui-accent-100: #0892ff;"),
                "the legitimate token should survive");
        }

        [Test]
        public void SkipsTokensWithNoValue()
        {
            var uss = AppUIThemeUssWriter.WriteStyleSheet("t", new Dictionary<string, string>
            {
                { "accent-100", "#0892ff" },
                { "neutral-100", "" },
            });

            Assert.That(uss, Does.Contain("--appui-accent-100"));
            Assert.That(uss, Does.Not.Contain("--appui-neutral-100"));
        }
    }

    [TestFixture]
    [TestOf(typeof(AppUIThemeJsonReader))]
    class AppUIThemeJsonReaderTests
    {
        const string k_Sample = @"{
  ""schemaVersion"": 1,
  ""$generator"": ""app-ui-theme-builder"",
  ""name"": ""dark-blue"",
  ""displayName"": ""Dark Blue"",
  ""baseTheme"": ""dark"",
  ""colorScheme"": ""dark"",
  ""seeds"": { ""accent"": ""#0892ff"", ""neutral"": { ""tint"": 0 } },
  ""overrides"": {},
  ""tokens"": {
    ""neutral-15"": ""#000000"",
    ""accent-100"": ""#0892ff"",
    ""alias-surface-card"": ""#282828""
  }
}";

        [Test]
        public void ReadsMetadataAndTokens()
        {
            var theme = AppUIThemeJsonReader.Parse(k_Sample, out var error);

            Assert.That(error, Is.Null);
            Assert.That(theme, Is.Not.Null);
            Assert.That(theme.schemaVersion, Is.EqualTo(1));
            Assert.That(theme.name, Is.EqualTo("dark-blue"));
            Assert.That(theme.displayName, Is.EqualTo("Dark Blue"));
            Assert.That(theme.baseTheme, Is.EqualTo("dark"));
            Assert.That(theme.colorScheme, Is.EqualTo("dark"));
            Assert.That(theme.tokens.Count, Is.EqualTo(3));
            Assert.That(theme.tokens["accent-100"], Is.EqualTo("#0892ff"));
            Assert.That(theme.tokens["alias-surface-card"], Is.EqualTo("#282828"));
        }

        [Test]
        public void IgnoresTheNestedSeedsObjectWhenReadingTokens()
        {
            // `seeds` also contains a nested object and appears before `tokens`; the reader must
            // not bleed one into the other.
            var theme = AppUIThemeJsonReader.Parse(k_Sample, out _);
            Assert.That(theme.tokens.ContainsKey("accent"), Is.False);
            Assert.That(theme.tokens.ContainsKey("tint"), Is.False);
        }

        [Test]
        public void ReportsMissingTokens()
        {
            var theme = AppUIThemeJsonReader.Parse(@"{ ""name"": ""x"" }", out var error);
            Assert.That(theme, Is.Null);
            Assert.That(error, Does.Contain("tokens"));
        }

        [Test]
        public void ReportsEmptyInput()
        {
            Assert.That(AppUIThemeJsonReader.Parse("", out var error), Is.Null);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void ReportsAnEmptyTokenMap()
        {
            var theme = AppUIThemeJsonReader.Parse(@"{ ""tokens"": {} }", out var error);
            Assert.That(theme, Is.Null);
            Assert.That(error, Does.Contain("empty"));
        }

        [Test]
        public void ReadsAThemeWhoseNameCollidesWithAFieldName()
        {
            // "tokens" is a perfectly good theme name, and the builder writes "name" before the
            // token map. A reader that searches the text for "tokens" finds the name's value first
            // and concludes the file has no token map at all.
            var json = @"{
  ""schemaVersion"": 1,
  ""name"": ""tokens"",
  ""displayName"": ""baseTheme"",
  ""baseTheme"": ""dark"",
  ""tokens"": { ""accent-100"": ""#0892ff"" }
}";

            var theme = AppUIThemeJsonReader.Parse(json, out var error);

            Assert.That(error, Is.Null, "a theme named after one of our own fields must still load");
            Assert.That(theme.name, Is.EqualTo("tokens"));
            Assert.That(theme.displayName, Is.EqualTo("baseTheme"));
            Assert.That(theme.baseTheme, Is.EqualTo("dark"), "the real field must win over the value");
            Assert.That(theme.tokens.Count, Is.EqualTo(1));
            Assert.That(theme.tokens["accent-100"], Is.EqualTo("#0892ff"));
        }

        [Test]
        public void IgnoresFieldNamesAppearingBelowTheTopLevel()
        {
            // The round-trip payload nests objects that reuse these names; only the top-level
            // fields describe the theme.
            var json = @"{
  ""schemaVersion"": 1,
  ""seeds"": { ""name"": ""not-the-theme-name"", ""tokens"": { ""nested"": ""#000000"" } },
  ""name"": ""real-name"",
  ""tokens"": { ""accent-100"": ""#0892ff"" }
}";

            var theme = AppUIThemeJsonReader.Parse(json, out var error);

            Assert.That(error, Is.Null);
            Assert.That(theme.name, Is.EqualTo("real-name"));
            Assert.That(theme.tokens.Count, Is.EqualTo(1), "the nested tokens object must be ignored");
            Assert.That(theme.tokens.ContainsKey("nested"), Is.False);
        }

        [Test]
        public void IsNotConfusedByBracesInsideStrings()
        {
            var json = @"{
  ""schemaVersion"": 1,
  ""displayName"": ""a } brace { in a string"",
  ""tokens"": { ""accent-100"": ""#0892ff"" }
}";

            var theme = AppUIThemeJsonReader.Parse(json, out var error);

            Assert.That(error, Is.Null);
            Assert.That(theme.displayName, Is.EqualTo("a } brace { in a string"));
            Assert.That(theme.tokens["accent-100"], Is.EqualTo("#0892ff"));
        }

        [Test]
        public void HandlesMinifiedJson()
        {
            var theme = AppUIThemeJsonReader.Parse(
                @"{""schemaVersion"":1,""name"":""t"",""tokens"":{""accent-100"":""#0892ff""}}",
                out var error);

            Assert.That(error, Is.Null);
            Assert.That(theme.name, Is.EqualTo("t"));
            Assert.That(theme.tokens["accent-100"], Is.EqualTo("#0892ff"));
        }
    }
}
