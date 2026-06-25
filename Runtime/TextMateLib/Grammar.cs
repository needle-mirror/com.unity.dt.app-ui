// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Unity.AppUI.TextMateLib
{
    /// <summary>
    /// Represents a TextMate grammar for tokenizing source code
    /// </summary>
    public class Grammar : IDisposable
    {
        IntPtr m_Handle;

        bool m_Disposed;

        internal Grammar(IntPtr handle)
        {
            m_Handle = handle;
        }

        /// <summary>
        /// Gets the scope name of the grammar (e.g., "source.js")
        /// </summary>
        public string ScopeName
        {
            get
            {
                ThrowIfDisposed();
                IntPtr ptr = NativeMethods.textmate_grammar_get_scope_name(m_Handle);
                return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            }
        }

        /// <summary>
        /// Tokenizes a line of text
        /// </summary>
        /// <param name="lineText">The line to tokenize</param>
        /// <param name="prevState">Previous state stack (use IntPtr.Zero for initial state)</param>
        /// <returns>Tokenization result with tokens and state</returns>
        public TokenizeResult TokenizeLine(string lineText, IntPtr prevState)
        {
            ThrowIfDisposed();

            if (prevState == IntPtr.Zero)
            {
                prevState = NativeMethods.textmate_get_initial_state();
            }

            var text = lineText ?? string.Empty;

            // Manually encode to UTF-8 with null terminator.
            // This avoids CharSet.Ansi which uses the system ANSI code page on Windows,
            // corrupting non-ASCII characters.
            var utf8ByteCount = Encoding.UTF8.GetByteCount(text);
            var utf8Bytes = new byte[utf8ByteCount + 1]; // +1 for null terminator
            Encoding.UTF8.GetBytes(text, 0, text.Length, utf8Bytes, 0);

            // Call the _utf16 variant which returns indices as UTF-16 code unit offsets,
            // matching C# string indexing directly.
            var resultPtr = NativeMethods.textmate_tokenize_line_utf16(m_Handle, utf8Bytes, prevState);

            if (resultPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to tokenize line");

            try
            {
                var result = Marshal.PtrToStructure<NativeMethods.TextMateTokenizeResult>(resultPtr);
                var tokens = new List<Token>();

                // Convert native tokens to managed tokens
                for (int i = 0; i < result.TokenCount; i++)
                {
                    IntPtr tokenPtr = result.Tokens + i * Marshal.SizeOf<NativeMethods.TextMateToken>();
                    var nativeToken = Marshal.PtrToStructure<NativeMethods.TextMateToken>(tokenPtr);

                    // Extract scopes
                    var scopes = new List<string>();
                    for (int j = 0; j < nativeToken.ScopeDepth; j++)
                    {
                        IntPtr scopePtr = Marshal.ReadIntPtr(nativeToken.Scopes, j * IntPtr.Size);
                        if (scopePtr != IntPtr.Zero)
                        {
                            var scope = Marshal.PtrToStringAnsi(scopePtr);
                            if (!string.IsNullOrEmpty(scope))
                                scopes.Add(scope);
                        }
                    }

                    tokens.Add(new Token(nativeToken.StartIndex, nativeToken.EndIndex, scopes));
                }

                return new TokenizeResult(tokens, result.RuleStack, result.StoppedEarly != 0);
            }
            finally
            {
                NativeMethods.textmate_free_tokenize_result(resultPtr);
            }
        }

        /// <summary>
        /// Tokenizes a line of text with binary/themed output (encoded tokens).
        /// Requires a theme to be set on the registry via SetThemeFromJson.
        /// </summary>
        /// <param name="lineText">The line to tokenize.</param>
        /// <param name="prevState">Previous state stack (use IntPtr.Zero for the initial state).</param>
        /// <returns>The themed tokenization result with encoded tokens and end-of-line state.</returns>
        public TokenizeResult2 TokenizeLine2(string lineText, IntPtr prevState)
        {
            ThrowIfDisposed();

            if (prevState == IntPtr.Zero)
                prevState = NativeMethods.textmate_get_initial_state();

            var text = lineText ?? string.Empty;
            var utf8ByteCount = Encoding.UTF8.GetByteCount(text);
            var utf8Bytes = new byte[utf8ByteCount + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, utf8Bytes, 0);

            var resultPtr = NativeMethods.textmate_tokenize_line2_utf16(m_Handle, utf8Bytes, prevState);

            if (resultPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to tokenize line (themed)");

            try
            {
                var result = Marshal.PtrToStructure<NativeMethods.TextMateTokenizeResult2>(resultPtr);
                var tokens = MarshalEncodedTokens(result.Tokens, result.TokenCount);
                return new TokenizeResult2(tokens, result.RuleStack, result.StoppedEarly != 0);
            }
            finally
            {
                NativeMethods.textmate_free_tokenize_result2(resultPtr);
            }
        }

        /// <summary>
        /// Tokenizes multiple lines with encoded/themed output in a single batch call.
        /// Requires a theme to be set on the registry via SetThemeFromJson.
        /// </summary>
        /// <param name="lines">The lines to tokenize.</param>
        /// <returns>An array of themed tokenization results, one per input line.</returns>
        public TokenizeResult2[] TokenizeLines2(string[] lines)
        {
            ThrowIfDisposed();

            if (lines == null || lines.Length == 0)
                return Array.Empty<TokenizeResult2>();

            var utf8Lines = new byte[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                utf8Lines[i] = NativeMethods.ToUtf8NullTerminated(lines[i]);
            }

            var pinnedHandles = new GCHandle[lines.Length];
            var linePointers = new IntPtr[lines.Length];
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    pinnedHandles[i] = GCHandle.Alloc(utf8Lines[i], GCHandleType.Pinned);
                    linePointers[i] = pinnedHandles[i].AddrOfPinnedObject();
                }

                var pinnedArray = GCHandle.Alloc(linePointers, GCHandleType.Pinned);
                try
                {
                    var batchPtr = NativeMethods.textmate_tokenize_lines2_utf16(
                        m_Handle, pinnedArray.AddrOfPinnedObject(),
                        lines.Length, NativeMethods.textmate_get_initial_state());

                    if (batchPtr == IntPtr.Zero)
                        throw new InvalidOperationException("Failed to batch tokenize lines (themed)");

                    try
                    {
                        var batch = Marshal.PtrToStructure<NativeMethods.TextMateTokenizeMultiLinesResult2>(batchPtr);

                        // Guard against a malformed native batch (LineCount > 0 but a null
                        // results pointer) so we surface a managed exception instead of a hard
                        // native crash when dereferencing batch.LineResults below.
                        if (batch.LineCount > 0 && batch.LineResults == IntPtr.Zero)
                            throw new InvalidOperationException("Batch tokenization returned a null line results pointer");

                        var results = new TokenizeResult2[batch.LineCount];

                        for (int i = 0; i < batch.LineCount; i++)
                        {
                            var lineResultPtr = Marshal.ReadIntPtr(batch.LineResults, i * IntPtr.Size);
                            var lineResult = Marshal.PtrToStructure<NativeMethods.TextMateTokenizeResult2>(lineResultPtr);
                            var tokens = MarshalEncodedTokens(lineResult.Tokens, lineResult.TokenCount);
                            results[i] = new TokenizeResult2(tokens, lineResult.RuleStack, lineResult.StoppedEarly != 0);
                        }

                        return results;
                    }
                    finally
                    {
                        NativeMethods.textmate_free_tokenize_lines_result2(batchPtr);
                    }
                }
                finally
                {
                    pinnedArray.Free();
                }
            }
            finally
            {
                for (int i = 0; i < pinnedHandles.Length; i++)
                {
                    if (pinnedHandles[i].IsAllocated)
                        pinnedHandles[i].Free();
                }
            }
        }

        static EncodedToken[] MarshalEncodedTokens(IntPtr tokensPtr, int rawCount)
        {
            if (rawCount <= 0 || tokensPtr == IntPtr.Zero)
                return Array.Empty<EncodedToken>();

            int tokenCount = rawCount / 2;
            var tokens = new EncodedToken[tokenCount];

            // Rent the temporary copy buffer from the shared pool to avoid a per-line
            // managed allocation (and the GC pressure it creates on large files).
            var raw = ArrayPool<int>.Shared.Rent(rawCount);
            try
            {
                Marshal.Copy(tokensPtr, raw, 0, rawCount);

                for (int i = 0; i < tokenCount; i++)
                {
                    tokens[i] = new EncodedToken(raw[i * 2], unchecked((uint)raw[i * 2 + 1]));
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(raw);
            }

            return tokens;
        }

        /// <summary>
        /// Tokenizes multiple lines sequentially
        /// </summary>
        /// <param name="lines">Array of lines to tokenize</param>
        /// <returns>Array of tokenization results, one per line</returns>
        public TokenizeResult[] TokenizeLines(string[] lines)
        {
            ThrowIfDisposed();

            if (lines == null || lines.Length == 0)
                return Array.Empty<TokenizeResult>();

            var results = new TokenizeResult[lines.Length];
            IntPtr state = NativeMethods.textmate_get_initial_state();

            for (int i = 0; i < lines.Length; i++)
            {
                results[i] = TokenizeLine(lines[i], state);
                state = results[i].StateStack;
            }

            return results;
        }

        internal IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return m_Handle;
            }
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(Grammar));
        }

        /// <summary>
        /// Releases the managed handle reference. The underlying native Grammar is owned and disposed by the Registry.
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed)
            {
                if (m_Handle != IntPtr.Zero)
                {
                    m_Handle = IntPtr.Zero;
                }
                m_Disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~Grammar()
        {
            Dispose();
        }
    }
}
