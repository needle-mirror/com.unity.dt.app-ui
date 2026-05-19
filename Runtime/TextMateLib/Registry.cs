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
    /// Registry for managing grammars and themes
    /// </summary>
    public class Registry : IDisposable
    {
        IntPtr m_Handle;

        IntPtr m_OnigLib;

        bool m_Disposed;

        readonly List<Grammar> m_LoadedGrammars = new ();

        /// <summary>
        /// Creates a new grammar registry
        /// </summary>
        public Registry()
        {
            m_OnigLib = NativeMethods.textmate_oniglib_create();
            if (m_OnigLib == IntPtr.Zero)
                throw new InvalidOperationException("Failed to initialize Oniguruma library");

            m_Handle = NativeMethods.textmate_registry_create(m_OnigLib);
            if (m_Handle == IntPtr.Zero)
            {
                NativeMethods.textmate_oniglib_dispose(m_OnigLib);
                throw new InvalidOperationException("Failed to create registry");
            }
        }

        /// <summary>
        /// Adds a grammar to the registry from a JSON file
        /// </summary>
        /// <param name="grammarPath">Path to the grammar JSON file</param>
        /// <exception cref="ArgumentNullException">Thrown when grammarPath is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the grammar fails to load</exception>
        public void AddGrammarFromFile(string grammarPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(grammarPath))
                throw new ArgumentNullException(nameof(grammarPath));

            int result = NativeMethods.textmate_registry_add_grammar_from_file(m_Handle, grammarPath);
            if (result == 0)
                throw new InvalidOperationException($"Failed to add grammar from file: {grammarPath}");
        }

        /// <summary>
        /// Adds a grammar to the registry from a JSON string
        /// </summary>
        /// <param name="jsonContent">JSON content of the grammar</param>
        /// <exception cref="ArgumentNullException">Thrown when jsonContent is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the grammar fails to load</exception>
        public void AddGrammarFromJson(string jsonContent)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(jsonContent))
                throw new ArgumentNullException(nameof(jsonContent));

            int result = NativeMethods.textmate_registry_add_grammar_from_json(m_Handle, NativeMethods.ToUtf8NullTerminated(jsonContent));
            if (result == 0)
                throw new InvalidOperationException("Failed to add grammar from JSON");
        }

        /// <summary>
        /// Loads a grammar by scope name
        /// </summary>
        /// <param name="scopeName">The scope name (e.g., "source.js")</param>
        /// <returns>A Grammar instance for the requested scope</returns>
        /// <exception cref="ArgumentNullException">Thrown when scopeName is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the grammar fails to load</exception>
        public Grammar LoadGrammar(string scopeName)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName));

            var handle = NativeMethods.textmate_registry_load_grammar(m_Handle, scopeName);
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to load grammar for scope: {scopeName}");

            var grammar = new Grammar(handle);
            var grammarScope = grammar.ScopeName;
            if (grammarScope != scopeName)
            {
                grammar.Dispose();
                throw new InvalidOperationException($"Loaded grammar scope mismatch: expected {scopeName}, got {grammarScope}");
            }

            m_LoadedGrammars.Add(grammar);
            return grammar;
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(Registry));
        }

        /// <summary>
        /// Releases the native resources
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed)
            {
                // Dispose all tracked grammars first
                foreach (var grammar in m_LoadedGrammars)
                {
                    grammar.Dispose();
                }
                m_LoadedGrammars.Clear();

                if (m_Handle != IntPtr.Zero)
                {
                    NativeMethods.textmate_registry_dispose(m_Handle);
                    m_Handle = IntPtr.Zero;
                }

                if (m_OnigLib != IntPtr.Zero)
                {
                    NativeMethods.textmate_oniglib_dispose(m_OnigLib);
                    m_OnigLib = IntPtr.Zero;
                }

                m_Disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~Registry()
        {
            Dispose();
        }
    }
}
