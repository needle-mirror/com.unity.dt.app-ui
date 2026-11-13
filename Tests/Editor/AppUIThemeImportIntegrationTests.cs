using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.Editor.Theming;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.Editor
{
    /// <summary>
    /// End-to-end checks for the <c>.appuitheme.json</c> importer: a real file goes through the
    /// AssetDatabase and has to come out as usable stylesheets.
    /// </summary>
    /// <remarks>
    /// The fixture is written into <c>Assets/</c> at run time rather than committed, because a
    /// committed one would be imported by every project that pulls in the package's tests, and
    /// would then sit in the theme registry for the rest of the session.
    /// </remarks>
    [TestFixture]
    [TestOf(typeof(AppUIThemeAsset))]
    class AppUIThemeImportIntegrationTests
    {
        const string k_Folder = "Assets/__AppUIThemeImportTests";
        const string k_Path = k_Folder + "/test-theme.appuitheme.json";
        const string k_ProbePath = k_Folder + "/probe.uss";

        /// <summary>
        /// A stylesheet that consumes one of the theme's variables, standing in for App UI's alias
        /// layer. Imported by Unity's own importer, so what it proves is that the theme sheet this
        /// package generates is readable by ordinary USS.
        /// </summary>
        const string k_ProbeUss = @".theme-probe {
    background-color: var(--appui-accent-100);
}

.theme-probe-literal {
    background-color: #ff0000;
}";

        const string k_Json = @"{
  ""schemaVersion"": 1,
  ""name"": ""test-theme"",
  ""displayName"": ""Test Theme"",
  ""baseTheme"": ""dark"",
  ""colorScheme"": ""dark"",
  ""seeds"": { ""accent"": ""#3e63dd"", ""neutral"": { ""tint"": 0 } },
  ""tokens"": {
    ""accent-100"": ""#3e63dd"",
    ""neutral-900"": ""#aaaaaa"",
    ""backgrounds-100"": ""#1d1d1d"",
    ""alias-surface-card"": ""#282828""
  }
}";

        [OneTimeSetUp]
        public void CreateFixture()
        {
            Directory.CreateDirectory(k_Folder);
            File.WriteAllText(k_Path, k_Json);
            File.WriteAllText(k_ProbePath, k_ProbeUss);
            AssetDatabase.ImportAsset(k_Path, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(k_ProbePath, ImportAssetOptions.ForceSynchronousImport);
        }

        [OneTimeTearDown]
        public void DeleteFixture()
        {
            AssetDatabase.DeleteAsset(k_Folder);
            AssetDatabase.Refresh();
        }

        [Test]
        public void TheInternalUssCompilerIsReachable()
        {
            // The canary for the whole feature. StyleSheetCompiler drives an internal Unity type;
            // if a Unity upgrade moves it this fails loudly here rather than quietly producing
            // themes that no longer paint anything.
            Assert.That(StyleSheetCompiler.available, Is.True,
                "Unity's internal USS importer could not be located. Custom themes will fall back " +
                "to being emitted as plain text instead of compiled stylesheets.");
        }

        [Test]
        public void ImportsAsAThemeAssetWithBothStyleSheets()
        {
            var theme = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(k_Path);
            Assert.That(theme, Is.Not.Null, "the main object should be an AppUIThemeAsset");

            Assert.That(theme.themeName, Is.EqualTo("test-theme"));
            Assert.That(theme.displayName, Is.EqualTo("Test Theme"));
            Assert.That(theme.baseTheme, Is.EqualTo("dark"));
            Assert.That(theme.colorScheme, Is.EqualTo("dark"));
            Assert.That(theme.tokenCount, Is.EqualTo(4));
            Assert.That(theme.tokens["accent-100"], Is.EqualTo("#3e63dd"));

            Assert.That(theme.styleSheet, Is.Not.Null, "a StyleSheet should have been compiled");
            Assert.That(theme.themeStyleSheet, Is.Not.Null, "a ThemeStyleSheet should have been compiled");

            var all = AssetDatabase.LoadAllAssetsAtPath(k_Path);
            Assert.That(all.OfType<StyleSheet>().Any(), Is.True, "the StyleSheet should be a sub-asset");
            Assert.That(all.OfType<ThemeStyleSheet>().Any(), Is.True,
                "the ThemeStyleSheet should be a sub-asset");
        }

        [Test]
        public void SubAssetReferencesSurviveAReimport()
        {
            var before = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(k_Path).styleSheet;
            AssetDatabase.ImportAsset(k_Path, ImportAssetOptions.ForceSynchronousImport);
            var after = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(k_Path).styleSheet;

            Assert.That(after, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(after), Is.EqualTo(AssetDatabase.GetAssetPath(before)),
                "a reimport must not move the stylesheet out from under existing references");
        }

        [Test]
        public void TheCompiledSheetCarriesTheThemeRule()
        {
            // The failure mode worth guarding is a sheet that compiles to nothing: the import
            // succeeds, the asset exists, and the theme silently paints nothing. Comparing the
            // theme's sheet against one Unity imported from ordinary USS shows it came out of the
            // parser with the same shape.
            //
            // Asserted by reflection because StyleSheet exposes neither its rules nor its selectors
            // publicly. Resolving the variables on a live element would be the fuller check, but
            // EditMode runs no style pass on a detached panel — a literal colour in an ordinary
            // stylesheet does not reach resolvedStyle here either, so that route asserts nothing.
            var theme = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(k_Path);
            var reference = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_ProbePath);
            Assume.That(theme.styleSheet, Is.Not.Null);
            Assume.That(reference, Is.Not.Null, "the reference stylesheet should have imported");

            Assert.That(CountRules(reference), Is.GreaterThan(0),
                "the reference stylesheet should have parsed into rules");
            Assert.That(CountRules(theme.styleSheet), Is.EqualTo(1),
                "the theme should compile to exactly one rule");
            Assert.That(CountRules(theme.themeStyleSheet), Is.GreaterThan(0),
                "the theme stylesheet should have parsed into rules");
        }

        [Test]
        public void TheThemeStyleSheetResolvesItsAppUIImports()
        {
            // The generated .tss pulls in App UI's own stylesheets by absolute package path
            // ("/Packages/com.unity.dt.app-ui/..."). If that path form were wrong the theme would
            // still compile and still carry its own rule, but a panel using it would get the
            // variables and none of the component styles — so the imports are asserted directly.
            var theme = AssetDatabase.LoadAssetAtPath<AppUIThemeAsset>(k_Path);
            Assume.That(theme.themeStyleSheet, Is.Not.Null);

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public;

            var field = typeof(StyleSheet)
                .GetFields(flags)
                .FirstOrDefault(f => f.Name.IndexOf("import", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                                     typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) &&
                                     f.FieldType != typeof(string));

            Assume.That(field, Is.Not.Null, "could not locate the imported-sheet collection");

            var imports = ((System.Collections.IEnumerable)field.GetValue(theme.themeStyleSheet))
                ?.Cast<object>()
                .Where(o => o != null)
                .ToList();

            Assert.That(imports, Is.Not.Null.And.Not.Empty,
                $"the generated .tss resolved no imports through '{field.Name}', so its " +
                "\"/Packages/com.unity.dt.app-ui/...\" paths did not resolve");
        }

        /// <summary>
        /// How many style rules a compiled sheet holds. <see cref="StyleSheet"/> keeps these in an
        /// internal array with no public accessor, hence the reflection.
        /// </summary>
        static int CountRules(StyleSheet sheet)
        {
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public;

            var field = typeof(StyleSheet)
                .GetFields(flags)
                .FirstOrDefault(f => f.FieldType.IsArray &&
                                     f.FieldType.GetElementType()?.Name == "StyleRule");

            Assume.That(field, Is.Not.Null, "could not locate StyleSheet's rule array");
            return ((System.Array)field.GetValue(sheet))?.Length ?? 0;
        }

        [Test]
        public void ImportedThemesJoinTheBuiltInsInTheRegistry()
        {
            ThemeRegistry.Invalidate();
            var themes = ThemeRegistry.GetThemes();

            foreach (var builtIn in AppUIThemeUssWriter.builtInThemes)
                Assert.That(themes, Does.Contain(builtIn), $"{builtIn} should be listed");

            Assert.That(themes, Does.Contain("test-theme"), "the imported theme should be listed");
            Assert.That(ThemeRegistry.GetDisplayName("editor-dark"), Is.EqualTo("Editor Dark"));
            Assert.That(ThemeRegistry.GetDisplayName("test-theme"), Is.EqualTo("Test Theme"));
        }
    }
}
