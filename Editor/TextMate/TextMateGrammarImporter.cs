#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System.IO;
using System.Text.RegularExpressions;
using Unity.AppUI.UI;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// Scripted importer for TextMate grammar files (.tmLanguage.json).
    /// Creates a TextMateGrammarAsset from the JSON content.
    /// </summary>

    [ScriptedImporter(1, new[] { "tmLanguage.json" }, new[] { "json" })]
    class TextMateGrammarImporter : ScriptedImporter
    {
        /// <summary>
        /// Imports a TextMate grammar file and creates a TextMateGrammarAsset.
        /// </summary>
        /// <param name="ctx">The asset import context.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var jsonContent = File.ReadAllText(ctx.assetPath);

            var (scopeName, displayName) = ExtractGrammarMetadata(jsonContent);

            var asset = ScriptableObject.CreateInstance<TextMateGrammarAsset>();
            asset.SetData(jsonContent, scopeName, displayName);
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath)
                .Replace(".tmLanguage", "");

            ctx.AddObjectToAsset("grammar", asset);
            ctx.SetMainObject(asset);
        }

        static (string scopeName, string displayName) ExtractGrammarMetadata(string json)
        {
            var scopeName = ExtractJsonStringField(json, "scopeName")
                ?? ExtractJsonStringField(json, "name")
                ?? "unknown";

            var displayName = ExtractJsonStringField(json, "displayName")
                ?? scopeName;

            return (scopeName, displayName);
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
