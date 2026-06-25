// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

using System;
using System.Collections.Generic;

namespace Unity.AppUI.TextMateLib
{
    /// <summary>
    /// Represents a syntax token with position and scope information
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets the starting position of the token in the line
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Gets the ending position of the token in the line
        /// </summary>
        public int EndIndex { get; }

        /// <summary>
        /// Gets the scope stack for this token
        /// </summary>
        public IReadOnlyList<string> Scopes { get; }

        internal Token(int startIndex, int endIndex, IReadOnlyList<string> scopes)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            Scopes = scopes;
        }

        /// <summary>
        /// Gets the length of the token
        /// </summary>
        public int Length => EndIndex - StartIndex;

        /// <summary>
        /// Gets the value of the token from the source line
        /// </summary>
        /// <param name="line">The source line</param>
        /// <returns>The substring representing the token</returns>
        public string GetValue(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            if (StartIndex >= line.Length)
                return string.Empty;

            int length = Math.Min(EndIndex, line.Length) - StartIndex;
            return line.Substring(StartIndex, length);
        }

        /// <summary>
        /// Returns a string representation of the token
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Token [{StartIndex}..{EndIndex}] Scopes: {string.Join(", ", Scopes)}";
        }
    }

    /// <summary>
    /// Represents the result of tokenizing a line
    /// </summary>
    public class TokenizeResult
    {
        /// <summary>
        /// Gets the tokens for the line
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// Gets the state stack at the end of the line
        /// </summary>
        public IntPtr StateStack { get; }

        /// <summary>
        /// Gets a value indicating whether tokenization stopped early
        /// </summary>
        public bool StoppedEarly { get; }

        internal TokenizeResult(IReadOnlyList<Token> tokens, IntPtr stateStack, bool stoppedEarly)
        {
            Tokens = tokens;
            StateStack = stateStack;
            StoppedEarly = stoppedEarly;
        }
    }

    /// <summary>
    /// Helpers for decoding encoded token metadata (bit-packed uint32)
    /// </summary>
    public static class EncodedTokenAttributes
    {
        const int FontStyleOffset = 11;
        const int ForegroundOffset = 15;
        const int BackgroundOffset = 24;

        const uint FontStyleMask = 0b00000000000000000111100000000000;
        const uint ForegroundMask = 0b00000000111111111000000000000000;
        const uint BackgroundMask = 0b11111111000000000000000000000000;

        /// <summary>
        /// Gets the font style flags from encoded metadata
        /// </summary>
        /// <param name="metadata">The bit-packed token metadata.</param>
        /// <returns>The font style flag bits.</returns>
        public static int GetFontStyle(uint metadata) =>
            (int)((metadata & FontStyleMask) >> FontStyleOffset);

        /// <summary>
        /// Gets the foreground color ID (index into the color map)
        /// </summary>
        /// <param name="metadata">The bit-packed token metadata.</param>
        /// <returns>The foreground color ID (index into the color map).</returns>
        public static int GetForeground(uint metadata) =>
            (int)((metadata & ForegroundMask) >> ForegroundOffset);

        /// <summary>
        /// Gets the background color ID (index into the color map)
        /// </summary>
        /// <param name="metadata">The bit-packed token metadata.</param>
        /// <returns>The background color ID (index into the color map).</returns>
        public static int GetBackground(uint metadata) =>
            (int)((metadata & BackgroundMask) >> BackgroundOffset);
    }

    /// <summary>
    /// Represents a single encoded token (start index + packed metadata)
    /// </summary>
    public readonly struct EncodedToken
    {
        /// <summary>
        /// Start offset in UTF-16 code units
        /// </summary>
        public readonly int StartIndex;

        /// <summary>
        /// Bit-packed metadata (font style, foreground/background color IDs)
        /// </summary>
        public readonly uint Metadata;

        internal EncodedToken(int startIndex, uint metadata)
        {
            StartIndex = startIndex;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the font style flags
        /// </summary>
        public int FontStyle => EncodedTokenAttributes.GetFontStyle(Metadata);

        /// <summary>
        /// Gets the foreground color ID (index into the color map)
        /// </summary>
        public int ForegroundId => EncodedTokenAttributes.GetForeground(Metadata);

        /// <summary>
        /// Gets the background color ID (index into the color map)
        /// </summary>
        public int BackgroundId => EncodedTokenAttributes.GetBackground(Metadata);
    }

    /// <summary>
    /// Represents the result of themed tokenization (binary/encoded tokens)
    /// </summary>
    public class TokenizeResult2
    {
        /// <summary>
        /// Gets the encoded tokens (start index + packed metadata pairs)
        /// </summary>
        public IReadOnlyList<EncodedToken> Tokens { get; }

        /// <summary>
        /// Gets the number of tokens
        /// </summary>
        public int TokenCount => Tokens.Count;

        /// <summary>
        /// Gets the state stack at the end of the line
        /// </summary>
        public IntPtr StateStack { get; }

        /// <summary>
        /// Gets a value indicating whether tokenization stopped early
        /// </summary>
        public bool StoppedEarly { get; }

        internal TokenizeResult2(IReadOnlyList<EncodedToken> tokens, IntPtr stateStack, bool stoppedEarly)
        {
            Tokens = tokens;
            StateStack = stateStack;
            StoppedEarly = stoppedEarly;
        }
    }
}
