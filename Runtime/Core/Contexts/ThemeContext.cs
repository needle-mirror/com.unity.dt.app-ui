using System;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The Theme context of the application.
    /// </summary>
    public record ThemeContext(string theme) : IContext
    {
        /// <summary>
        /// The current theme.
        /// </summary>
        public string theme { get; } = theme;
    }
}
