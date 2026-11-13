using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// Imports a <c>.appuitheme.json</c> file, as exported by the App UI Theme Builder, and
    /// compiles it into stylesheets a panel can use.
    /// </summary>
    /// <remarks>
    /// Produces three objects under fixed identifiers — <c>theme</c>, <c>stylesheet</c> and
    /// <c>themeStyleSheet</c>. The identifiers are deliberately not derived from the theme's name:
    /// renaming a theme would otherwise break every scene and prefab referencing its stylesheet.
    /// </remarks>
    [ScriptedImporter(k_Version, new[] { "appuitheme", "appuitheme.json" }, new[] { "json" })]
    class AppUIThemeImporter : ScriptedImporter
    {
        // 2: consumer custom properties and non-colour values (lengths, numbers, var() references).
        const int k_Version = 2;

        /// <summary>
        /// Reimport user themes when App UI's own stylesheets change, since the generated theme
        /// file imports them.
        /// </summary>
        const string k_AppUIThemePath =
            "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<AppUIThemeAsset>();
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath).Replace(".appuitheme", "");

            // The asset is registered before anything can go wrong, so that a file with bad content
            // shows its import errors in the inspector instead of turning into a broken asset with
            // nowhere to attach them.
            ctx.AddObjectToAsset("theme", asset);
            ctx.SetMainObject(asset);
            ctx.DependsOnSourceAsset(k_AppUIThemePath);

            var parsed = AppUIThemeJsonReader.Parse(File.ReadAllText(ctx.assetPath), out var error);
            if (parsed == null)
            {
                ctx.LogImportError($"[App UI] {error}", asset);
                return;
            }

            if (parsed.schemaVersion > AppUIThemeJson.supportedSchemaVersion)
            {
                ctx.LogImportError(
                    $"[App UI] This theme uses schema version {parsed.schemaVersion}, but this " +
                    $"version of App UI understands up to {AppUIThemeJson.supportedSchemaVersion}. " +
                    "Update the package.", asset);
                return;
            }

            var themeName = string.IsNullOrEmpty(parsed.name) ? asset.name : parsed.name;
            if (!AppUIThemeUssWriter.IsValidThemeName(themeName))
            {
                ctx.LogImportError(
                    $"[App UI] \"{themeName}\" cannot be used as a theme name: it becomes the " +
                    "USS selector .appui--" + themeName + ", so it must start with a letter or " +
                    "underscore and contain only letters, digits, hyphens and underscores.", asset);
                return;
            }

            if (AppUIThemeUssWriter.IsBuiltInTheme(themeName))
            {
                // Not fatal: shadowing a built-in theme is occasionally what someone wants, but it
                // is far more often a name they did not realise was taken.
                ctx.LogImportWarning(
                    $"[App UI] \"{themeName}\" is one of App UI's own themes. This theme will " +
                    "override it wherever both stylesheets are loaded.", asset);
            }

            var tokens = ValidateTokens(ctx, asset, parsed.tokens);

            asset.SetMetadata(themeName, parsed.displayName, parsed.baseTheme, parsed.colorScheme);
            asset.SetTokens(tokens);

            if (!StyleSheetCompiler.available)
            {
                // The internal USS parser moved. Rather than fail, ship the generated USS as text so
                // the theme can still be used by hand, and say so.
                var fallback = new TextAsset(
                    AppUIThemeUssWriter.WriteStyleSheet(themeName, tokens));
                fallback.name = $"{themeName}.uss";
                ctx.AddObjectToAsset("uss", fallback);
                ctx.LogImportWarning(
                    "[App UI] This Unity version does not expose the USS compiler this importer " +
                    "uses, so the theme could not be compiled. The generated USS is included as a " +
                    "text asset: save it as a .uss file and reference it from a .tss instead.",
                    asset);
                return;
            }

            var tokensUss = AppUIThemeUssWriter.WriteStyleSheet(themeName, tokens);
            var styleSheet = StyleSheetCompiler.Compile<StyleSheet>(ctx, tokensUss, "StyleSheet");
            var themeStyleSheet = StyleSheetCompiler.Compile<ThemeStyleSheet>(
                ctx,
                AppUIThemeUssWriter.WriteThemeStyleSheet(themeName, tokens),
                "ThemeStyleSheet");

            if (styleSheet != null)
                ctx.AddObjectToAsset("stylesheet", styleSheet);
            if (themeStyleSheet != null)
                ctx.AddObjectToAsset("themeStyleSheet", themeStyleSheet);

            asset.SetStyleSheets(styleSheet, themeStyleSheet);
        }

        /// <summary>
        /// Drops tokens that would not compile, reporting each one.
        /// </summary>
        /// <remarks>
        /// A bad colour is reported and skipped rather than substituted. Emitting a placeholder
        /// would compile, but it would also put a colour on screen that nobody chose; leaving the
        /// token out lets the base theme underneath show through, which is both closer to correct
        /// and easier to spot alongside the import error.
        ///
        /// Names are checked as well as values. A token name is written into the generated
        /// stylesheet as the suffix of a custom property, so a name carrying USS punctuation could
        /// close the theme's rule and open another — turning an imported theme into one that
        /// restyles the whole application from outside its own selector.
        /// </remarks>
        static Dictionary<string, string> ValidateTokens(
            AssetImportContext ctx,
            Object asset,
            IReadOnlyDictionary<string, string> tokens)
        {
            var valid = new Dictionary<string, string>(tokens.Count);
            foreach (var pair in tokens)
            {
                if (!AppUIThemeUssWriter.IsValidTokenName(pair.Key))
                {
                    ctx.LogImportError(
                        $"[App UI] \"{pair.Key}\" is not a valid token name. An App UI token is " +
                        "written without its --appui- prefix, in lower-case letters, digits and " +
                        "single hyphens; any other custom property keeps its full --name. It has " +
                        "been left out of the generated theme.", asset);
                    continue;
                }

                if (AppUIThemeUssWriter.IsValidValue(pair.Value))
                {
                    valid[pair.Key] = pair.Value.Trim();
                    continue;
                }

                ctx.LogImportError(
                    $"[App UI] Token \"{pair.Key}\" has the value \"{pair.Value}\", which is not a " +
                    "colour, a number or length, or a var() reference. It has been left out of the " +
                    "generated theme.", asset);
            }

            return valid;
        }
    }
}
