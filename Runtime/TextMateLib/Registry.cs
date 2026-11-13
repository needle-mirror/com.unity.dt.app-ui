// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

            // Read and validate in managed code, then hand the validated content to the native
            // loader. Passing the path instead would have the native side re-read the file, so
            // anything that changed it in between would reach the parser unvalidated.
            var content = TextMateJson.ReadAndValidate(grammarPath, nameof(grammarPath), "grammar");

            // The native file loader parses only files named '*.json' (or with no extension at
            // all); preserved here so routing through the JSON entry point does not quietly start
            // accepting grammars this API used to reject.
            RequireJsonExtension(grammarPath, "grammar");

            int result = NativeMethods.textmate_registry_add_grammar_from_json(m_Handle, NativeMethods.ToUtf8NullTerminated(content));
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

            TextMateJson.Validate(jsonContent, nameof(jsonContent), "grammar");

            int result = NativeMethods.textmate_registry_add_grammar_from_json(m_Handle, NativeMethods.ToUtf8NullTerminated(jsonContent));
            if (result == 0)
                throw new InvalidOperationException("Failed to add grammar from JSON");
        }

        /// <summary>
        /// Sets the color theme on the registry from a JSON string.
        /// Must be called before using TokenizeLine2 for themed output.
        /// </summary>
        /// <param name="jsonContent">The theme JSON content.</param>
        /// <exception cref="ArgumentNullException">Thrown when jsonContent is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the theme fails to load</exception>
        public void SetThemeFromJson(string jsonContent)
        {
            ThrowIfDisposed();

            TextMateJson.Validate(jsonContent, nameof(jsonContent), "theme");

            int result = NativeMethods.textmate_registry_set_theme(m_Handle, NativeMethods.ToUtf8NullTerminated(jsonContent));
            if (result == 0)
                throw new InvalidOperationException("Failed to set theme from JSON");
        }

        /// <summary>
        /// Gets the color map from the registry after a theme has been set.
        /// Color IDs in encoded tokens are indices into this array.
        /// </summary>
        /// <returns>The theme color palette as hex strings, indexed by color ID.</returns>
        public string[] GetColorMap()
        {
            ThrowIfDisposed();

            var mapPtr = NativeMethods.textmate_registry_get_color_map(m_Handle);
            if (mapPtr == IntPtr.Zero)
                return Array.Empty<string>();

            try
            {
                var map = Marshal.PtrToStructure<NativeMethods.TextMateColorMap>(mapPtr);

                // Guard against a malformed native color map (ColorCount > 0 but a null
                // pointer) to avoid a hard native crash when dereferencing map.Colors below.
                if (map.ColorCount > 0 && map.Colors == IntPtr.Zero)
                    return Array.Empty<string>();

                var colors = new string[map.ColorCount];

                for (int i = 0; i < map.ColorCount; i++)
                {
                    var colorPtr = Marshal.ReadIntPtr(map.Colors, i * IntPtr.Size);
                    colors[i] = colorPtr != IntPtr.Zero
                        ? Marshal.PtrToStringAnsi(colorPtr) ?? string.Empty
                        : string.Empty;
                }

                return colors;
            }
            finally
            {
                NativeMethods.textmate_free_color_map(mapPtr);
            }
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

        static void RequireJsonExtension(string path, string what)
        {
            var dot = path.LastIndexOf('.');
            if (dot >= 0 && !path.Substring(dot).Equals(".json", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Failed to load {what} from file: only the JSON format is supported, but '{path}' does not end in '.json'.");
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
