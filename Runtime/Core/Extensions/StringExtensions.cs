using System;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Capitalizes the first letter of a string.
        /// </summary>
        /// <param name="arg"> The string to capitalize. </param>
        /// <returns> The capitalized string. </returns>
        public static string Capitalize(this string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return arg;

            return arg[0].ToString().ToUpper() + arg[1..];
        }

        /// <summary>
        /// Gets the initials from a name string.
        /// </summary>
        /// <param name="name"> The name to extract initials from. </param>
        /// <returns>
        /// The uppercase initials: first letter of the first two words for multi-word names,
        /// first two characters for single-word names with 2+ characters,
        /// the single character for one-character names, or "?" if the string is null or empty.
        /// </returns>
        public static string GetInitials(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";
            var parts = name.Split(' ');
            if (parts.Length >= 2)
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
            return name.Length >= 2
                ? $"{char.ToUpper(name[0])}{char.ToUpper(name[1])}"
                : $"{char.ToUpper(name[0])}";
        }
    }
}
