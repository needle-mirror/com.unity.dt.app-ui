using Unity.AppUI.Core;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The state of a thread message.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum ThreadMessageState
    {
        /// <summary>
        /// The default state.
        /// </summary>
        Default,

        /// <summary>
        /// The message is being sent.
        /// </summary>
        Sending,

        /// <summary>
        /// The message is a draft.
        /// </summary>
        Draft
    }

    /// <summary>
    /// Represents a reaction on a thread message.
    /// </summary>
    public struct ReactionInfo
    {
        /// <summary>
        /// The emoji character for this reaction.
        /// </summary>
        public string emoji;

        /// <summary>
        /// The number of users who reacted with this emoji.
        /// </summary>
        public int count;

        /// <summary>
        /// Whether the current user has reacted with this emoji.
        /// </summary>
        public bool isOwnReaction;
    }
}
