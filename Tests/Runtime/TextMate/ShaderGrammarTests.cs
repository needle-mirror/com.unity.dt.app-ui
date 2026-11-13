#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.AppUI.TextMateLib;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.TextMate
{
    /// <summary>
    /// The shipped hlsl and wgsl grammars, the two shader languages the registry's highlighter also
    /// loads. Each snippet is tokenised with the real grammar file and every listed slice must carry
    /// the scope the theme colours.
    /// </summary>
    [TestFixture]
    class ShaderGrammarTests
    {
        const string k_GrammarsDir = "Packages/com.unity.dt.app-ui/PackageResources/SyntaxHighlighting/Grammars/";

        Registry m_Registry;
        Grammar m_Grammar;

        [TearDown]
        public void TearDown()
        {
            m_Grammar?.Dispose();
            m_Grammar = null;
            m_Registry?.Dispose();
            m_Registry = null;
        }

        Grammar Load(string language, string scopeName)
        {
            var json = File.ReadAllText(Path.GetFullPath(k_GrammarsDir + language + ".tmLanguage.json"));
            m_Registry = new Registry();
            m_Registry.AddGrammarFromJson(json);
            m_Grammar = m_Registry.LoadGrammar(scopeName);
            Assert.That(m_Grammar, Is.Not.Null, $"{language} grammar did not load as {scopeName}.");
            return m_Grammar;
        }

        static void AssertScope(string line, IReadOnlyList<Token> tokens, string text, string scope)
        {
            var start = line.IndexOf(text, System.StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), $"'{text}' is not in the snippet.");
            foreach (var token in tokens)
            {
                if (token.StartIndex > start || token.EndIndex <= start)
                    continue;
                Assert.That(token.EndIndex, Is.GreaterThanOrEqualTo(start + text.Length),
                    $"'{text}' was split across tokens.");
                Assert.That(token.Scopes, Has.Member(scope), $"'{text}' scopes: {string.Join(" ", token.Scopes)}");
                return;
            }
            Assert.Fail($"No token covers '{text}'.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The grammar source is not shipped with a player build")]
        public void Hlsl_TokenisesTypesSemanticsKeywordsNumbersAndComments()
        {
            const string line = "float4 frag(float2 uv : TEXCOORD0) : SV_Target { return 1.0; } // tint";
            var tokens = Load("hlsl", "source.hlsl").TokenizeLine(line, System.IntPtr.Zero).Tokens;

            AssertScope(line, tokens, "float4", "storage.type.basic.hlsl");
            AssertScope(line, tokens, "TEXCOORD0", "support.variable.semantic.hlsl");
            AssertScope(line, tokens, "SV_Target", "support.variable.semantic.sm4.hlsl");
            AssertScope(line, tokens, "return", "keyword.control.hlsl");
            AssertScope(line, tokens, "1.0", "constant.numeric.decimal.hlsl");
            AssertScope(line, tokens, "tint", "comment.line.double-slash.hlsl");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The grammar source is not shipped with a player build")]
        public void Wgsl_TokenisesAttributesFunctionsTypesKeywordsAndNumbers()
        {
            const string line = "@fragment fn main(@location(0) uv: vec2<f32>) -> vec4<f32> { let x = 1.0; return vec4<f32>(x); }";
            var tokens = Load("wgsl", "source.wgsl").TokenizeLine(line, System.IntPtr.Zero).Tokens;

            AssertScope(line, tokens, "fragment", "entity.name.attribute.wgsl");
            AssertScope(line, tokens, "fn", "keyword.other.fn.wgsl");
            AssertScope(line, tokens, "main", "entity.name.function.wgsl");
            AssertScope(line, tokens, "vec2", "storage.type.wgsl");
            AssertScope(line, tokens, "f32", "storage.type.wgsl");
            AssertScope(line, tokens, "let", "keyword.other.wgsl");
            AssertScope(line, tokens, "1.0", "constant.numeric.float.wgsl");
            AssertScope(line, tokens, "return", "keyword.control.wgsl");
        }

        /// <summary>
        /// A language resolves its grammar only through the stylesheet rule that maps its class to
        /// the file, so both components must map each shader language onto its own grammar.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void CodeBlockAndCodeEditorStylesheets_MapShaderLanguagesToTheirGrammars(
            [Values("CodeBlock.uss|appui-code-block", "CodeEditor.uss|appui-code-editor")] string sheet,
            [Values("hlsl", "wgsl")] string language)
        {
            var parts = sheet.Split('|');
            var uss = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/" + parts[0]));
            var selector = "." + parts[1] + "--lang-" + language + " {";
            var at = uss.IndexOf(selector, System.StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), $"{parts[0]} has no '{selector}' rule.");
            var body = uss.Substring(at, uss.IndexOf('}', at) - at);
            StringAssert.Contains("SyntaxHighlighting/Grammars/" + language + ".tmLanguage.json", body);
        }
    }
}
#endif
