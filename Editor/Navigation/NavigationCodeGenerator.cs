using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.AppUI.Navigation.Editor
{
    /// <summary>
    /// Code generator for navigation graphs.
    /// </summary>
    class NavigationCodeGenerator
    {
        /// <summary>
        /// Generates code for the given <see cref="NavGraphViewAsset"/>.
        /// </summary>
        /// <param name="asset"> The asset to generate code for. </param>
        public static void GenerateCode(NavGraphViewAsset asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var codePath = path.Replace(".asset", "");

            var output = EditorUtility.SaveFilePanel("Save generated code", Path.GetDirectoryName(codePath), Path.GetFileName(codePath), "cs");

            if (string.IsNullOrEmpty(output))
                return;

            using (var streamWriter = new StreamWriter(output, false))
            {
                streamWriter.WriteLine("// This file is auto-generated. Do not edit it directly.");
                streamWriter.WriteLine("// Date: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                streamWriter.WriteLine("using System;");
                streamWriter.WriteLine("using System.Collections.Generic;");
                streamWriter.WriteLine("using UnityEngine;");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("namespace Unity.AppUI.Navigation.Generated");
                streamWriter.WriteLine("{");

                WriteConstantsClass(streamWriter, "Actions",
                    asset.nodes.Where(n => n is NavAction).Select(n => n.name));

                WriteConstantsClass(streamWriter, "Destinations",
                    asset.nodes.Where(n => n is NavDestination).Select(n => n.name));

                WriteConstantsClass(streamWriter, "Graphs",
                    asset.nodes.Where(n => n is NavGraph).Select(n => n.name));

                streamWriter.WriteLine("}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Generated code for navigation graph " + asset.name + " at " + output);
        }

        /// <summary>
        /// Writes a `public static partial class` of string constants, one per node name.
        /// </summary>
        static void WriteConstantsClass(TextWriter writer, string className, IEnumerable<string> names)
        {
            writer.WriteLine("    public static partial class " + className);
            writer.WriteLine("    {");

            // Node names are authored data and need not be unique, but the identifiers
            // derived from them must be, or the generated file will not compile.
            var used = new HashSet<string>();
            foreach (var name in names)
            {
                var identifier = GetVariableName(name);
                var candidate = identifier;
                for (var suffix = 2; !used.Add(candidate); suffix++)
                    candidate = identifier + "_" + suffix.ToString(CultureInfo.InvariantCulture);

                writer.WriteLine("        public const string " + candidate + " = " + ToLiteral(name) + ";");
            }

            writer.WriteLine("    }");
        }

        /// <summary>
        /// Converts an arbitrary node name into a valid, unambiguous C# identifier.
        /// </summary>
        /// <remarks>
        /// Node names are arbitrary user input from the graph editor. Emitting them
        /// unescaped both broke compilation (for names containing `-`, `.` or a leading
        /// digit) and allowed a crafted name to inject arbitrary members into the
        /// generated file, which the Editor then compiles.
        /// </remarks>
        internal static string GetVariableName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "_";

            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                // Only letters, digits and underscore can appear in a C# identifier.
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                var isIdentifierPart =
                    category == UnicodeCategory.UppercaseLetter ||
                    category == UnicodeCategory.LowercaseLetter ||
                    category == UnicodeCategory.TitlecaseLetter ||
                    category == UnicodeCategory.ModifierLetter ||
                    category == UnicodeCategory.OtherLetter ||
                    category == UnicodeCategory.LetterNumber ||
                    category == UnicodeCategory.DecimalDigitNumber ||
                    category == UnicodeCategory.ConnectorPunctuation ||
                    category == UnicodeCategory.NonSpacingMark ||
                    category == UnicodeCategory.SpacingCombiningMark ||
                    category == UnicodeCategory.Format;

                builder.Append(isIdentifierPart ? c : '_');
            }

            var identifier = builder.ToString().ToLowerInvariant();

            // C# allows a wider set of characters in the body of an identifier than at
            // the start: digits, combining marks, formatting characters and connector
            // punctuation are all valid later but not first. Prefix (rather than
            // replace) whenever the first character is not a valid start, so that "1st"
            // and "2nd" stay distinct instead of collapsing to the same name.
            if (!IsIdentifierStart(identifier[0]))
                identifier = "_" + identifier;

            // Escape C# keywords rather than mangling them, so `new` stays readable.
            if (s_Keywords.Contains(identifier))
                identifier = "@" + identifier;

            return identifier;
        }

        /// <summary>
        /// Whether <paramref name="c"/> is one of the characters C# treats as a new line
        /// but that <see cref="char.IsControl(char)"/> does not report as a control
        /// character: U+2028 LINE SEPARATOR and U+2029 PARAGRAPH SEPARATOR.
        /// </summary>
        static bool IsLineBreak(char c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            return category == UnicodeCategory.LineSeparator ||
                category == UnicodeCategory.ParagraphSeparator;
        }

        /// <summary>
        /// Whether <paramref name="c"/> may be the first character of a C# identifier:
        /// an underscore, or a letter (including letter-numbers such as Roman numerals).
        /// </summary>
        static bool IsIdentifierStart(char c)
        {
            if (c == '_')
                return true;

            switch (CharUnicodeInfo.GetUnicodeCategory(c))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Renders a string as a C# double-quoted literal, escaping the contents.
        /// </summary>
        internal static string ToLiteral(string value)
        {
            if (value == null)
                return "null";

            var builder = new StringBuilder(value.Length + 2);
            builder.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '"': builder.Append("\\\""); break;
                    case '\0': builder.Append("\\0"); break;
                    case '\a': builder.Append("\\a"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    case '\v': builder.Append("\\v"); break;
                    default:
                        // Escape anything else non-printable so the emitted file stays
                        // valid regardless of what the name contains.
                        //
                        // char.IsControl is not sufficient on its own: U+2028 and U+2029
                        // are LineSeparator/ParagraphSeparator rather than Control, but
                        // C# counts them as new-line characters, so emitting them raw
                        // would terminate the string literal.
                        if (char.IsControl(c) || IsLineBreak(c))
                            builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
            return builder.ToString();
        }

        static readonly HashSet<string> s_Keywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate",
            "do", "double", "else", "enum", "event", "explicit", "extern", "false",
            "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
            "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
            "new", "null", "object", "operator", "out", "override", "params", "private",
            "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
            "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };
    }
}
