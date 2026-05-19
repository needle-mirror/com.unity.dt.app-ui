using Unity.AppUI.UI;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Context propagated from <see cref="Thread"/> to child components.
    /// </summary>
    /// <param name="enableReactions">Whether reactions are enabled.</param>
    /// <param name="enableLikes">Whether likes/dislikes are enabled.</param>
    /// <param name="enableDislikes">Whether dislikes are enabled.</param>
    /// <param name="enableResolution">Whether resolution is enabled.</param>
    /// <param name="mentionProvider">The mention provider for resolving mention markup.</param>
    public record ThreadContext(
        bool enableReactions,
        bool enableLikes,
        bool enableDislikes,
        bool enableResolution,
        IMentionProvider mentionProvider
    ) : IContext
    {
        /// <summary>Whether reactions are enabled.</summary>
        public bool enableReactions { get; } = enableReactions;

        /// <summary>Whether likes/dislikes are enabled.</summary>
        public bool enableLikes { get; } = enableLikes;

        /// <summary>Whether dislikes are enabled.</summary>
        public bool enableDislikes { get; } = enableDislikes;

        /// <summary>Whether resolution is enabled.</summary>
        public bool enableResolution { get; } = enableResolution;

        /// <summary>The mention provider for resolving mention markup.</summary>
        public IMentionProvider mentionProvider { get; } = mentionProvider;
    }
}
