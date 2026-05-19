#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System.IO;
using System.Text.RegularExpressions;
using Unity.AppUI.UI;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// Scripted importer for TextMate theme files (.tmTheme.json).
    /// Creates a TextMateThemeAsset from the JSON content.
    /// </summary>
    [ScriptedImporter(1, new[] { "tmTheme.json" }, new[] { "json" })]
    class TextMateThemeImporter : ScriptedImporter
    {
        /// <summary>
        /// Imports a TextMate theme file and creates a TextMateThemeAsset.
        /// </summary>
        /// <param name="ctx">The asset import context.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var jsonContent = File.ReadAllText(ctx.assetPath);

            var displayName = ExtractThemeDisplayName(jsonContent);

            var asset = ScriptableObject.CreateInstance<TextMateThemeAsset>();
            asset.SetData(jsonContent, displayName);
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath)
                .Replace(".tmTheme", "");

            ctx.AddObjectToAsset("theme", asset);
            ctx.SetMainObject(asset);
        }

        static string ExtractThemeDisplayName(string json)
        {
            return ExtractJsonStringField(json, "displayName")
                ?? ExtractJsonStringField(json, "name")
                ?? "Unknown Theme";
        }

        static string ExtractJsonStringField(string json, string fieldName)
        {
            // Simple regex to extract a top-level string field from JSON
            // Matches: "fieldName": "value" or "fieldName":"value"
            var pattern = $"\"{fieldName}\"\\s*:\\s*\"([^\"]+)\"";
            var match = Regex.Match(json, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
#endif
