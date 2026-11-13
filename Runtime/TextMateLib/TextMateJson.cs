// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

#nullable enable

using System;
using System.IO;

namespace Unity.AppUI.TextMateLib
{
    /// <summary>
    /// Managed-side validation for the grammar and theme payloads handed to the native TextMate
    /// tokenizer.
    /// </summary>
    /// <remarks>
    /// The native parser is reached through P/Invoke, so a malformed payload that makes it crash
    /// takes the whole process down rather than surfacing as a catchable exception — the return
    /// code these entry points check is only produced by a parser that survived. Rejecting
    /// structurally invalid input here means the failure is a managed exception instead.
    /// </remarks>
    static class TextMateJson
    {
        /// <summary>
        /// Returns whether <paramref name="json"/> is a well-formed JSON document whose root is an
        /// object, which is the shape both grammars and themes use.
        /// </summary>
        /// <param name="json">The candidate JSON document.</param>
        /// <returns><see langword="true"/> when the document parses.</returns>
        public static bool IsWellFormedObject(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            var i = 0;
            SkipWhitespace(json, ref i);
            if (i >= json.Length || json[i] != '{')
                return false;

            if (!TryReadValue(json, ref i, 0))
                return false;

            SkipWhitespace(json, ref i);
            return i == json.Length;
        }

        /// <summary>
        /// Validates a grammar/theme payload and throws when it cannot be parsed.
        /// </summary>
        /// <remarks>
        /// Malformed content raises <see cref="InvalidOperationException"/> rather than
        /// <see cref="ArgumentException"/> on purpose: that is what these entry points have always
        /// thrown when content failed to load, and callers catch it. This validation moves the
        /// failure earlier — before the native parser ever sees the bytes — without changing the
        /// exception type that reaches the caller.
        /// </remarks>
        /// <param name="json">The candidate JSON document.</param>
        /// <param name="paramName">The name of the caller's parameter, for the exception message.</param>
        /// <param name="what">Describes the payload, for the exception message.</param>
        /// <exception cref="ArgumentNullException">Thrown when the payload is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the payload is not a well-formed JSON object.</exception>
        public static void Validate(string? json, string paramName, string what)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(paramName);

            if (!IsWellFormedObject(json))
                throw new InvalidOperationException($"Failed to load {what}: the content is not a well-formed JSON object.");
        }

        /// <summary>
        /// Resolves a grammar/theme file path, reads it, and validates its content, surfacing every
        /// failure as the load exception this API documents.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="File.Exists"/> only reports that a name resolves to a file, not that it can be
        /// opened: a locked file, or one on a volume the process cannot read, still throws from the
        /// read itself. Those are load failures like any other, so they are reported as
        /// <see cref="InvalidOperationException"/> rather than making callers add filesystem-specific
        /// handlers that this API never previously required.
        /// </para>
        /// <para>
        /// The content is returned so the caller can hand the validated bytes to the native loader.
        /// Passing the path instead would have the native side re-read the file, and anything that
        /// changed it between the two reads would reach the parser unvalidated.
        /// </para>
        /// </remarks>
        /// <param name="path">The path supplied by the caller.</param>
        /// <param name="paramName">The name of the caller's parameter, for the exception message.</param>
        /// <param name="what">Describes the payload, for the exception message.</param>
        /// <returns>The validated file content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file cannot be resolved, read, or parsed.</exception>
        public static string ReadAndValidate(string? path, string paramName, string what)
        {
            var resolvedPath = ResolveExistingFile(path, paramName, what);

            string content;
            try
            {
                content = File.ReadAllText(resolvedPath);
            }
            catch (Exception e) when (!(e is OutOfMemoryException))
            {
                throw new InvalidOperationException(
                    $"Failed to load {what} from file: '{resolvedPath}' could not be read.", e);
            }

            Validate(content, paramName, what);
            return content;
        }

        /// <summary>
        /// Resolves a grammar/theme file path and validates that it points at an existing file.
        /// </summary>
        /// <param name="path">The path supplied by the caller.</param>
        /// <param name="paramName">The name of the caller's parameter, for the exception message.</param>
        /// <param name="what">Describes the payload, for the exception message.</param>
        /// <returns>The resolved absolute path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the path is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the path cannot be resolved or no file exists there.</exception>
        public static string ResolveExistingFile(string? path, string paramName, string what)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(paramName);

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to load {what} from file: the path '{path}' could not be resolved.", e);
            }

            if (!File.Exists(fullPath))
                throw new InvalidOperationException($"Failed to load {what} from file: no file found at '{fullPath}'.");

            return fullPath;
        }

        // Nesting is bounded so a deeply nested document cannot exhaust the stack while being
        // validated — which would be the very crash this validation exists to prevent.
        const int k_MaxDepth = 128;

        static bool TryReadValue(string s, ref int i, int depth)
        {
            if (depth > k_MaxDepth)
                return false;

            SkipWhitespace(s, ref i);
            if (i >= s.Length)
                return false;

            switch (s[i])
            {
                case '{': return TryReadObject(s, ref i, depth);
                case '[': return TryReadArray(s, ref i, depth);
                case '"': return TryReadString(s, ref i);
                case 't': return TryReadKeyword(s, ref i, "true");
                case 'f': return TryReadKeyword(s, ref i, "false");
                case 'n': return TryReadKeyword(s, ref i, "null");
                default: return TryReadNumber(s, ref i);
            }
        }

        static bool TryReadObject(string s, ref int i, int depth)
        {
            i++; // '{'
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == '}')
            {
                i++;
                return true;
            }

            while (true)
            {
                SkipWhitespace(s, ref i);
                if (i >= s.Length || s[i] != '"' || !TryReadString(s, ref i))
                    return false;

                SkipWhitespace(s, ref i);
                if (i >= s.Length || s[i] != ':')
                    return false;
                i++;

                if (!TryReadValue(s, ref i, depth + 1))
                    return false;

                SkipWhitespace(s, ref i);
                if (i >= s.Length)
                    return false;
                if (s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (s[i] == '}')
                {
                    i++;
                    return true;
                }
                return false;
            }
        }

        static bool TryReadArray(string s, ref int i, int depth)
        {
            i++; // '['
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == ']')
            {
                i++;
                return true;
            }

            while (true)
            {
                if (!TryReadValue(s, ref i, depth + 1))
                    return false;

                SkipWhitespace(s, ref i);
                if (i >= s.Length)
                    return false;
                if (s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (s[i] == ']')
                {
                    i++;
                    return true;
                }
                return false;
            }
        }

        static bool TryReadString(string s, ref int i)
        {
            i++; // opening quote
            while (i < s.Length)
            {
                var c = s[i];
                if (c == '"')
                {
                    i++;
                    return true;
                }
                if (c == '\\')
                {
                    i++;
                    if (i >= s.Length)
                        return false;
                    var esc = s[i];
                    if (esc == 'u')
                    {
                        if (i + 4 >= s.Length)
                            return false;
                        for (var k = 1; k <= 4; k++)
                        {
                            if (!IsHexDigit(s[i + k]))
                                return false;
                        }
                        i += 4;
                    }
                    else if ("\"\\/bfnrt".IndexOf(esc) < 0)
                    {
                        return false;
                    }
                    i++;
                    continue;
                }
                if (c < 0x20)
                    return false;
                i++;
            }
            return false;
        }

        static bool TryReadNumber(string s, ref int i)
        {
            var start = i;
            if (i < s.Length && s[i] == '-')
                i++;

            if (i >= s.Length || !IsDigit(s[i]))
                return false;

            if (s[i] == '0')
                i++;
            else
                while (i < s.Length && IsDigit(s[i])) i++;

            if (i < s.Length && s[i] == '.')
            {
                i++;
                if (i >= s.Length || !IsDigit(s[i]))
                    return false;
                while (i < s.Length && IsDigit(s[i])) i++;
            }

            if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                    i++;
                if (i >= s.Length || !IsDigit(s[i]))
                    return false;
                while (i < s.Length && IsDigit(s[i])) i++;
            }

            return i > start;
        }

        static bool TryReadKeyword(string s, ref int i, string keyword)
        {
            if (i + keyword.Length > s.Length || string.CompareOrdinal(s, i, keyword, 0, keyword.Length) != 0)
                return false;
            i += keyword.Length;
            return true;
        }

        static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r'))
                i++;
        }

        static bool IsDigit(char c) => c >= '0' && c <= '9';

        static bool IsHexDigit(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
}
