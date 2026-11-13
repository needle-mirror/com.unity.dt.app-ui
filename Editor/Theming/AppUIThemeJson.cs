using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Unity.AppUI.Editor.Theming
{
    /// <summary>
    /// The parsed contents of a <c>.appuitheme.json</c> file.
    /// </summary>
    /// <remarks>
    /// Only the fields Unity actually needs are read. The file also carries the seeds, overrides
    /// and algorithm settings the web Theme Builder used, so that re-opening it there restores the
    /// editing session, but nothing here looks at them: the <see cref="tokens"/> map is already
    /// fully resolved, which keeps the importer deterministic and stops the C# side from having to
    /// reimplement any colour science.
    /// </remarks>
    class AppUIThemeJson
    {
        public int schemaVersion;
        public string name;
        public string displayName;
        public string baseTheme;
        public string colorScheme;

        /// <summary>Token name without the <c>--appui-</c> prefix, mapped to a colour literal.</summary>
        public readonly Dictionary<string, string> tokens = new Dictionary<string, string>();

        /// <summary>The highest schema version this importer understands.</summary>
        public const int supportedSchemaVersion = 1;
    }

    /// <summary>
    /// A small reader for the <c>.appuitheme.json</c> shape.
    /// </summary>
    /// <remarks>
    /// Hand-written rather than delegating to a library. <c>JsonUtility</c> cannot express the
    /// <c>tokens</c> map, which is an object with arbitrary keys, and pulling in Newtonsoft would
    /// mean setting <c>overrideReferences</c> on <c>Unity.AppUI.Editor.asmdef</c> and then
    /// enumerating every precompiled reference it needs — a real risk of breaking unrelated
    /// compilation for one file's worth of parsing. The format here is shallow: a handful of
    /// top-level strings and one flat object of string values.
    /// </remarks>
    static class AppUIThemeJsonReader
    {
        /// <summary>
        /// Parses theme JSON.
        /// </summary>
        /// <param name="json">The file contents.</param>
        /// <param name="error">A human-readable reason when parsing fails.</param>
        /// <returns>The parsed theme, or null when <paramref name="json"/> could not be read.</returns>
        public static AppUIThemeJson Parse(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "The file is empty.";
                return null;
            }

            var result = new AppUIThemeJson();
            try
            {
                result.schemaVersion = (int)(ReadNumber(json, "schemaVersion") ?? 0);
                result.name = ReadString(json, "name");
                result.displayName = ReadString(json, "displayName");
                result.baseTheme = ReadString(json, "baseTheme");
                result.colorScheme = ReadString(json, "colorScheme");

                if (!TryReadObject(json, "tokens", out var tokensBody))
                {
                    error = "No \"tokens\" object found.";
                    return null;
                }

                foreach (var pair in ReadStringPairs(tokensBody))
                    result.tokens[pair.Key] = pair.Value;
            }
            catch (Exception e)
            {
                error = $"Could not parse the file: {e.Message}";
                return null;
            }

            if (result.tokens.Count == 0)
            {
                error = "The \"tokens\" object is empty.";
                return null;
            }

            return result;
        }

        /// <summary>Reads a top-level string field. Returns null when absent.</summary>
        static string ReadString(string json, string field)
        {
            if (!TryFindValue(json, field, out var cursor))
                return null;

            return cursor < json.Length && json[cursor] == '"' ? ReadQuoted(json, ref cursor) : null;
        }

        /// <summary>Reads a top-level numeric field. Returns null when absent.</summary>
        static double? ReadNumber(string json, string field)
        {
            if (!TryFindValue(json, field, out var cursor))
                return null;

            var start = cursor;
            while (cursor < json.Length && (char.IsDigit(json[cursor]) || json[cursor] == '-' ||
                                            json[cursor] == '+' || json[cursor] == '.' ||
                                            json[cursor] == 'e' || json[cursor] == 'E'))
                cursor++;

            var text = json.Substring(start, cursor - start);
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : (double?)null;
        }

        /// <summary>Extracts the body of a top-level object field, without its braces.</summary>
        static bool TryReadObject(string json, string field, out string body)
        {
            body = null;
            if (!TryFindValue(json, field, out var cursor))
                return false;

            if (cursor >= json.Length || json[cursor] != '{')
                return false;

            var depth = 0;
            var start = cursor;
            for (; cursor < json.Length; cursor++)
            {
                var c = json[cursor];
                if (c == '"')
                {
                    ReadQuoted(json, ref cursor);
                    cursor--; // the loop will step past the closing quote
                    continue;
                }

                if (c == '{')
                    depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        body = json.Substring(start + 1, cursor - start - 1);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds where the value of a top-level field starts.
        /// </summary>
        /// <remarks>
        /// Walks the document rather than searching the text for <c>"field"</c>. A substring search
        /// cannot tell a key from a value that happens to read the same, and a theme may legitimately
        /// be called "tokens" — at which point <c>"name": "tokens"</c> appears before the real token
        /// map and the search lands on it, leaving a perfectly valid exported theme unimportable.
        ///
        /// Strings are consumed whole so that braces and quotes inside them do not disturb the
        /// depth count, and a match only counts at depth 1 and in key position, immediately before
        /// a colon.
        /// </remarks>
        static bool TryFindValue(string json, string field, out int valueStart)
        {
            valueStart = -1;
            var depth = 0;
            var i = 0;

            while (i < json.Length)
            {
                var c = json[i];
                if (c == '"')
                {
                    var cursor = i;
                    var text = ReadQuoted(json, ref cursor);

                    var after = cursor;
                    while (after < json.Length && char.IsWhiteSpace(json[after]))
                        after++;

                    if (depth == 1 && after < json.Length && json[after] == ':' && text == field)
                    {
                        valueStart = SkipToValue(json, after);
                        return true;
                    }

                    i = cursor;
                    continue;
                }

                if (c == '{' || c == '[')
                    depth++;
                else if (c == '}' || c == ']')
                    depth--;

                i++;
            }

            return false;
        }

        /// <summary>Reads every <c>"key": "value"</c> pair from an object body.</summary>
        static IEnumerable<KeyValuePair<string, string>> ReadStringPairs(string body)
        {
            var cursor = 0;
            while (cursor < body.Length)
            {
                while (cursor < body.Length && body[cursor] != '"')
                    cursor++;
                if (cursor >= body.Length)
                    yield break;

                var key = ReadQuoted(body, ref cursor);

                while (cursor < body.Length && body[cursor] != ':')
                    cursor++;
                cursor++;

                while (cursor < body.Length && char.IsWhiteSpace(body[cursor]))
                    cursor++;
                if (cursor >= body.Length)
                    yield break;

                if (body[cursor] != '"')
                {
                    // Not a string value; skip to the next entry rather than misreading it.
                    while (cursor < body.Length && body[cursor] != ',')
                        cursor++;
                    continue;
                }

                yield return new KeyValuePair<string, string>(key, ReadQuoted(body, ref cursor));
            }
        }

        static int SkipToValue(string json, int cursor)
        {
            while (cursor < json.Length && (json[cursor] == ':' || char.IsWhiteSpace(json[cursor])))
                cursor++;
            return cursor;
        }

        /// <summary>
        /// Reads a JSON string starting at the opening quote, leaving the cursor past the closing
        /// one and unescaping as it goes.
        /// </summary>
        static string ReadQuoted(string text, ref int cursor)
        {
            cursor++; // opening quote
            var builder = new StringBuilder();
            while (cursor < text.Length)
            {
                var c = text[cursor];
                if (c == '\\' && cursor + 1 < text.Length)
                {
                    cursor++;
                    var escape = text[cursor];
                    builder.Append(escape switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        'r' => '\r',
                        'b' => '\b',
                        'f' => '\f',
                        _ => escape,
                    });
                    cursor++;
                    continue;
                }

                if (c == '"')
                {
                    cursor++;
                    return builder.ToString();
                }

                builder.Append(c);
                cursor++;
            }

            return builder.ToString();
        }
    }
}
