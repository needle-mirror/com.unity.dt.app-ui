// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Unity.AppUI.TextMateLib
{
    /// <summary>
    /// Native method declarations for TextMateLib C API
    /// </summary>
    static class NativeMethods
    {
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LibraryName = "__Internal";
#else
        const string LibraryName = "AppUITextMateLib";
#endif

        // ============================================================================
        // Opaque handle types
        // ============================================================================

        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateRegistry { public IntPtr Handle; }
        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateGrammar { public IntPtr Handle; }
        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateStateStack { public IntPtr Handle; }
        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateOnigLib { public IntPtr Handle; }
        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateTheme { public IntPtr Handle; }

        // ============================================================================
        // Token structures
        // ============================================================================

        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateToken
        {
            public int StartIndex;
            public int EndIndex;
            public int ScopeDepth;
            public IntPtr Scopes; // char**
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateTokenizeResult
        {
            public IntPtr Tokens; // TextMateToken*
            public int TokenCount;
            public IntPtr RuleStack;
            public int StoppedEarly;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TextMateTokenizeResult2
        {
            public IntPtr Tokens; // uint32_t*
            public int TokenCount;
            public IntPtr RuleStack;
            public int StoppedEarly;
        }

        // ============================================================================
        // Theme API
        // ============================================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr textmate_theme_load_from_file(
            string themePath);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_theme_load_from_json(
            byte[] jsonContentUtf8);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern uint textmate_theme_get_foreground(
            IntPtr theme,
            string scopePath,
            uint defaultColor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern uint textmate_theme_get_background(
            IntPtr theme,
            string scopePath,
            uint defaultColor);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int textmate_theme_get_font_style(
            IntPtr theme,
            string scopePath,
            int defaultStyle);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint textmate_theme_get_default_foreground(IntPtr theme);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint textmate_theme_get_default_background(IntPtr theme);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_theme_dispose(IntPtr theme);

        // ============================================================================
        // Registry and Grammar API
        // ============================================================================

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_oniglib_create();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_registry_create(IntPtr onigLib);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_registry_dispose(IntPtr registry);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int textmate_registry_add_grammar_from_file(
            IntPtr registry,
            string grammarPath);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int textmate_registry_add_grammar_from_json(
            IntPtr registry,
            byte[] jsonContentUtf8);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr textmate_registry_load_grammar(
            IntPtr registry,
            string scopeName);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_get_initial_state();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr textmate_tokenize_line(
            IntPtr grammar,
            string lineText,
            IntPtr prevState);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr textmate_tokenize_line2(
            IntPtr grammar,
            string lineText,
            IntPtr prevState);

        // UTF-16 variants: accept byte[] (caller provides UTF-8 with null terminator)
        // and return token indices as UTF-16 code unit offsets.

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_tokenize_line_utf16(
            IntPtr grammar,
            byte[] lineTextUtf8,
            IntPtr prevState);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_tokenize_line2_utf16(
            IntPtr grammar,
            byte[] lineTextUtf8,
            IntPtr prevState);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr textmate_tokenize_lines_utf16(
            IntPtr grammar,
            IntPtr lines,
            int lineCount,
            IntPtr initialState);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_free_tokenize_result(IntPtr result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_free_tokenize_result2(IntPtr result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_free_tokenize_lines_result(IntPtr result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr textmate_grammar_get_scope_name(IntPtr grammar);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void textmate_oniglib_dispose(IntPtr onigLib);

        // ============================================================================
        // Helpers
        // ============================================================================

        internal static byte[] ToUtf8NullTerminated(string text)
        {
            text ??= string.Empty;
            var byteCount = Encoding.UTF8.GetByteCount(text);
            var bytes = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
            return bytes;
        }
    }
}
