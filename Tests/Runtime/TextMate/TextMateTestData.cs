#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
namespace Unity.AppUI.Tests.TextMate
{
    /// <summary>
    /// Embedded JSON test data for TextMate tests.
    /// Avoids Unity asset import dependencies by using raw JSON strings.
    /// </summary>
    static class TextMateTestData
    {
        /// <summary>
        /// A simple grammar that matches keywords, numbers, and strings.
        /// Scope: source.simple
        /// </summary>
        public const string SimpleGrammarJson = @"
{
    ""name"": ""Simple"",
    ""scopeName"": ""source.simple"",
    ""patterns"": [
        {
            ""name"": ""keyword.control.simple"",
            ""match"": ""\\b(if|else|while|for|return)\\b""
        },
        {
            ""name"": ""constant.numeric.simple"",
            ""match"": ""\\b[0-9]+\\b""
        },
        {
            ""name"": ""string.quoted.double.simple"",
            ""begin"": ""\"""",
            ""end"": ""\"""",
            ""patterns"": [
                {
                    ""name"": ""constant.character.escape.simple"",
                    ""match"": ""\\\\.""
                }
            ]
        },
        {
            ""name"": ""comment.line.double-slash.simple"",
            ""match"": ""//.*$""
        }
    ]
}";

        /// <summary>
        /// A grammar that includes multi-line block comment support for testing state management.
        /// Scope: source.multiline
        /// </summary>
        public const string MultiLineGrammarJson = @"
{
    ""name"": ""MultiLine"",
    ""scopeName"": ""source.multiline"",
    ""patterns"": [
        {
            ""name"": ""keyword.control.multiline"",
            ""match"": ""\\b(if|else|while|for|return)\\b""
        },
        {
            ""name"": ""constant.numeric.multiline"",
            ""match"": ""\\b[0-9]+\\b""
        },
        {
            ""name"": ""comment.block.multiline"",
            ""begin"": ""/\\*"",
            ""end"": ""\\*/"",
            ""patterns"": []
        }
    ]
}";

        /// <summary>
        /// A minimal theme that provides styling for keywords (bold), strings, numbers, and comments (italic).
        /// </summary>
        public const string MinimalThemeJson = @"
{
    ""name"": ""Minimal Theme"",
    ""colors"": {
        ""editor.foreground"": ""#D4D4D4"",
        ""editor.background"": ""#1E1E1E""
    },
    ""tokenColors"": [
        {
            ""scope"": ""keyword"",
            ""settings"": {
                ""foreground"": ""#569CD6"",
                ""fontStyle"": ""bold""
            }
        },
        {
            ""scope"": ""string"",
            ""settings"": {
                ""foreground"": ""#CE9178""
            }
        },
        {
            ""scope"": ""constant.numeric"",
            ""settings"": {
                ""foreground"": ""#B5CEA8""
            }
        },
        {
            ""scope"": ""comment"",
            ""settings"": {
                ""foreground"": ""#6A9955"",
                ""fontStyle"": ""italic""
            }
        }
    ]
}";

        /// <summary>
        /// Invalid JSON for testing error cases.
        /// </summary>
        public const string InvalidJson = @"{ this is not valid json }";

        /// <summary>
        /// Empty JSON object.
        /// </summary>
        public const string EmptyJson = @"{}";
    }
}
#endif
