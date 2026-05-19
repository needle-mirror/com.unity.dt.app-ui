namespace Unity.AppUI.Core
{
    /// <summary>
    /// A context that provides access to an <see cref="EmojiDatabase"/> for emoji-related features.
    /// </summary>
    /// <param name="database">The <see cref="EmojiDatabase"/> instance to use.</param>
    public record EmojisContext(EmojiDatabase database) : IContext
    {
        /// <summary>
        /// The emoji database containing the available emojis.
        /// </summary>
        public EmojiDatabase database { get; } = database;

        /// <summary>
        /// Whether the context is valid, which requires a non-null database with at least one emoji.
        /// </summary>
        public bool IsValid => this is { database: { } db } && db.GetEmojiCount() > 0;
    }
}
