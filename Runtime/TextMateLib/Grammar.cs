// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

using System;
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
