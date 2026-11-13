using NUnit.Framework;
using Unity.AppUI.Navigation.Editor;

namespace Unity.AppUI.Tests.Editor
{
    [TestFixture]
    [TestOf(typeof(NavigationCodeGenerator))]
    class NavigationCodeGeneratorTests
    {
        // Node names come from the graph editor and are arbitrary user input. They are
        // emitted both as C# identifiers and as C# string literals, so both paths have
        // to survive hostile input without changing the shape of the generated file.

        [TestCase("Home Screen", ExpectedResult = "home_screen")]
        [TestCase("Details", ExpectedResult = "details")]
        [TestCase("my-node.v2", ExpectedResult = "my_node_v2")]
        [TestCase("has spaces", ExpectedResult = "has_spaces")]
        [TestCase("", ExpectedResult = "_")]
        public string GetVariableName_produces_valid_identifiers(string name)
        {
            return NavigationCodeGenerator.GetVariableName(name);
        }

        [Test]
        public void GetVariableName_prefixes_leading_digits()
        {
            // A bare "1st" is not a legal identifier; prefixing (rather than replacing)
            // keeps "1st" and "2nd" distinct.
            Assert.AreEqual("_1st", NavigationCodeGenerator.GetVariableName("1st"));
            Assert.AreEqual("_2nd", NavigationCodeGenerator.GetVariableName("2nd"));
        }

        [Test]
        public void GetVariableName_prefixes_non_start_characters()
        {
            // Combining marks, formatting characters and connector punctuation are legal
            // inside an identifier but not as its first character, so filtering for
            // "identifier part" alone still let through names that would not compile.
            Assert.AreEqual("_́home", NavigationCodeGenerator.GetVariableName("́Home"));
            Assert.AreEqual("_‍home", NavigationCodeGenerator.GetVariableName("‍Home"));

            // An underscore or a letter is a valid start and must not gain a prefix.
            Assert.AreEqual("_home", NavigationCodeGenerator.GetVariableName("_Home"));
            Assert.AreEqual("home", NavigationCodeGenerator.GetVariableName("Home"));
        }

        [Test]
        public void GetVariableName_deduplicates_colliding_names()
        {
            // "A B" and "A-B" both reduce to "a_b"; the generated file would not compile
            // if the second declaration reused the identifier.
            var names = new[] { "A B", "A-B", "A.B" };
            var seen = new System.Collections.Generic.List<string>();
            var used = new System.Collections.Generic.HashSet<string>();

            foreach (var name in names)
            {
                var id = NavigationCodeGenerator.GetVariableName(name);
                var candidate = id;
                for (var suffix = 2; !used.Add(candidate); suffix++)
                    candidate = id + "_" + suffix;
                seen.Add(candidate);
            }

            Assert.AreEqual(new[] { "a_b", "a_b_2", "a_b_3" }, seen.ToArray());
        }

        [Test]
        public void GetVariableName_escapes_keywords()
        {
            Assert.AreEqual("@new", NavigationCodeGenerator.GetVariableName("new"));
            Assert.AreEqual("@class", NavigationCodeGenerator.GetVariableName("class"));
            Assert.AreEqual("@return", NavigationCodeGenerator.GetVariableName("Return"));
        }

        [Test]
        public void GetVariableName_neutralises_injection_payload()
        {
            // Regression guard: this name previously produced an identifier that closed
            // the declaration and injected new members into the generated file.
            const string payload = "x\"; static void Boom(){} const string y=\"";
            var identifier = NavigationCodeGenerator.GetVariableName(payload);

            Assert.AreEqual("x___static_void_boom_____const_string_y__", identifier);
            Assert.That(identifier, Does.Not.Contain("\""));
            Assert.That(identifier, Does.Not.Contain(";"));
            Assert.That(identifier, Does.Not.Contain("{"));
            Assert.That(identifier, Does.Not.Contain("}"));
        }

        [Test]
        public void ToLiteral_quotes_and_escapes()
        {
            Assert.AreEqual("\"plain\"", NavigationCodeGenerator.ToLiteral("plain"));
            Assert.AreEqual("\"a\\\\b\"", NavigationCodeGenerator.ToLiteral("a\\b"));
            Assert.AreEqual("\"a\\\"b\"", NavigationCodeGenerator.ToLiteral("a\"b"));
            Assert.AreEqual("\"a\\nb\"", NavigationCodeGenerator.ToLiteral("a\nb"));
            Assert.AreEqual("\"a\\tb\"", NavigationCodeGenerator.ToLiteral("a\tb"));
            Assert.AreEqual("\"a\\u0001b\"", NavigationCodeGenerator.ToLiteral("a\u0001b"));
            Assert.AreEqual("null", NavigationCodeGenerator.ToLiteral(null));
        }

        [Test]
        public void ToLiteral_escapes_unicode_line_separators()
        {
            // Regression guard: U+2028 and U+2029 are LineSeparator/ParagraphSeparator,
            // so char.IsControl reports false for both — but C# counts them as new-line
            // characters, so emitting them raw terminated the string literal and the
            // generated file failed to compile.
            Assert.AreEqual("\"a\\u2028b\"", NavigationCodeGenerator.ToLiteral("a\u2028b"));
            Assert.AreEqual("\"a\\u2029b\"", NavigationCodeGenerator.ToLiteral("a\u2029b"));

            // A non-breaking space is a SpaceSeparator, not a line break, and is legal
            // inside a literal — it must not be escaped.
            Assert.AreEqual("\"a b\"", NavigationCodeGenerator.ToLiteral("a b"));
        }

        [Test]
        public void ToLiteral_keeps_injection_payload_inside_the_literal()
        {
            // Regression guard: the payload must remain a single string literal. Every
            // embedded quote is escaped, so it cannot terminate the literal early.
            const string payload = "x\"; static void Boom(){} const string y=\"";
            var literal = NavigationCodeGenerator.ToLiteral(payload);

            Assert.AreEqual("\"x\\\"; static void Boom(){} const string y=\\\"\"", literal);

            // Exactly two unescaped quotes: the opening and closing delimiters.
            var unescaped = 0;
            for (var i = 0; i < literal.Length; i++)
            {
                if (literal[i] != '"')
                    continue;
                var backslashes = 0;
                for (var j = i - 1; j >= 0 && literal[j] == '\\'; j--)
                    backslashes++;
                if (backslashes % 2 == 0)
                    unescaped++;
            }
            Assert.AreEqual(2, unescaped, "payload escaped the string literal");
        }
    }
}
