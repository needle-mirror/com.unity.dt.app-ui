// =============================================================================
// This file is derived from TextMateLib
// Repository: https://github.com/jbltx/TextMateLib
// Author: Mickael Bonfill aka jbltx <jbltx@protonmail.com>
// Year: 2026
// License: MIT
// =============================================================================

using System;

namespace Unity.AppUI.TextMateLib
{
    /// <summary>
    /// Font style flags for tokens
    /// </summary>
    [Flags]
    public enum FontStyle
    {
        /// <summary>No special style</summary>
        None = 0,
        /// <summary>Italic text</summary>
        Italic = 1,
        /// <summary>Bold text</summary>
        Bold = 2,
        /// <summary>Underlined text</summary>
        Underline = 4
    }

    /// <summary>
    /// Represents a TextMate theme for applying colors and styles to tokens
    /// </summary>
    public class Theme : IDisposable
    {
        IntPtr m_Handle;

        bool m_Disposed;

        internal Theme(IntPtr handle)
        {
            m_Handle = handle;
        }

        /// <summary>
        /// Loads a theme from a JSON file
        /// </summary>
        /// <param name="themePath">Path to the theme JSON file</param>
        /// <returns>A new Theme instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when themePath is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the theme fails to load</exception>
        public static Theme LoadFromFile(string themePath)
        {
            if (string.IsNullOrEmpty(themePath))
                throw new ArgumentNullException(nameof(themePath));

            var handle = NativeMethods.textmate_theme_load_from_file(themePath);
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to load theme from file: {themePath}");

            return new Theme(handle);
        }

        /// <summary>
        /// Loads a theme from a JSON string
        /// </summary>
        /// <param name="jsonContent">JSON content of the theme</param>
        /// <returns>A new Theme instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when jsonContent is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the theme fails to load</exception>
        public static Theme LoadFromJson(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
                throw new ArgumentNullException(nameof(jsonContent));

            var handle = NativeMethods.textmate_theme_load_from_json(NativeMethods.ToUtf8NullTerminated(jsonContent));
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to load theme from JSON");

            return new Theme(handle);
        }

        /// <summary>
        /// Gets the foreground color for a scope path
        /// </summary>
        /// <param name="scopePath">The scope path (e.g., "source.js string.quoted")</param>
        /// <param name="defaultColor">Default color to return if scope not found (RGBA format: 0xRRGGBBAA)</param>
        /// <returns>Color in RGBA format (0xRRGGBBAA)</returns>
        public uint GetForeground(string scopePath, uint defaultColor = 0xFFFFFFFF)
        {
            ThrowIfDisposed();
            return NativeMethods.textmate_theme_get_foreground(m_Handle, scopePath ?? string.Empty, defaultColor);
        }

        /// <summary>
        /// Gets the background color for a scope path
        /// </summary>
        /// <param name="scopePath">The scope path</param>
        /// <param name="defaultColor">Default color to return if scope not found (RGBA format: 0xRRGGBBAA)</param>
        /// <returns>Color in RGBA format (0xRRGGBBAA)</returns>
        public uint GetBackground(string scopePath, uint defaultColor = 0x00000000)
        {
            ThrowIfDisposed();
            return NativeMethods.textmate_theme_get_background(m_Handle, scopePath ?? string.Empty, defaultColor);
        }

        /// <summary>
        /// Gets the font style for a scope path
        /// </summary>
        /// <param name="scopePath">The scope path</param>
        /// <param name="defaultStyle">Default style to return if scope not found</param>
        /// <returns>Font style flags</returns>
        public FontStyle GetFontStyle(string scopePath, FontStyle defaultStyle = FontStyle.None)
        {
            ThrowIfDisposed();
            int style = NativeMethods.textmate_theme_get_font_style(m_Handle, scopePath ?? string.Empty, (int)defaultStyle);
            return (FontStyle)style;
        }

        /// <summary>
        /// Gets the default foreground color for the theme
        /// </summary>
        /// <returns>Color in RGBA format (0xRRGGBBAA)</returns>
        public uint GetDefaultForeground()
        {
            ThrowIfDisposed();
            return NativeMethods.textmate_theme_get_default_foreground(m_Handle);
        }

        /// <summary>
        /// Gets the default background color for the theme
        /// </summary>
        /// <returns>Color in RGBA format (0xRRGGBBAA)</returns>
        public uint GetDefaultBackground()
        {
            ThrowIfDisposed();
            return NativeMethods.textmate_theme_get_default_background(m_Handle);
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
                throw new ObjectDisposedException(nameof(Theme));
        }

        /// <summary>
        /// Releases the native resources
        /// </summary>
        public void Dispose()
        {
            if (!m_Disposed)
            {
                if (m_Handle != IntPtr.Zero)
                {
                    NativeMethods.textmate_theme_dispose(m_Handle);
                    m_Handle = IntPtr.Zero;
                }
                m_Disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~Theme()
        {
            Dispose();
        }
    }
}
