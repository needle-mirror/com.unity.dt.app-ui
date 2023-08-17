namespace Unity.AppUI.Core
{
    /// <summary>
    /// The Lang context of the application.
    /// </summary>
    public record LangContext(string lang) : IContext
    {
        /// <summary>
        /// The current language.
        /// </summary>
        public string lang { get; } = lang;
    }
}
