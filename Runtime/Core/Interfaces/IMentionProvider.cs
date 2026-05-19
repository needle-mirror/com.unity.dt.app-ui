using System.Collections;
using UnityEngine.UIElements;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Interface for providing mention suggestions and resolving mention markup in thread messages.
    /// </summary>
    /// <remarks>
    /// Mentions in raw message content use the format <c>:entityType[displayName]{#entityId}</c>.
    /// For example: <c>:user[Jane Doe]{#9876543211011}</c>.
    /// The <see cref="ConvertToRichText"/> method converts these patterns into rich text for display.
    /// </remarks>
    public interface IMentionProvider
    {
        /// <summary>
        /// Returns mention suggestions matching the given query.
        /// </summary>
        /// <param name="query">The search query typed after the @ character.</param>
        /// <returns>An list of mention suggestions matching the query. The type of the objects in the list is defined by the implementation.</returns>
        IList GetSuggestions(string query);

        /// <summary>
        /// Resolves mention markup in raw message content into rich text for display.
        /// </summary>
        /// <remarks>
        /// The raw content may contain mention patterns like <c>:user[Jane Doe]{#9876543211011}</c>.
        /// Implementations should replace these patterns with appropriate rich text markup
        /// (e.g. <c>&lt;b&gt;@Jane Doe&lt;/b&gt;</c>).
        /// Because the output is rendered with <c>enableRichText = true</c>, implementations must
        /// sanitize user-provided text segments (e.g. by wrapping them in <c>&lt;noparse&gt;</c> tags)
        /// to prevent rich text injection.
        /// </remarks>
        /// <param name="rawContent">The raw message content potentially containing mention markup.</param>
        /// <returns>The content with mentions resolved to rich text.</returns>
        string ConvertToRichText(string rawContent);

        /// <summary>
        /// Use a mention to the display text for that mention without rich text formatting.
        /// </summary>
        /// <param name="mention"> The mention </param>
        /// <returns> The display name </returns>
        string GetDisplayName(object mention);

        /// <summary>
        /// Creates a visual element to represent a mention suggestion in the UI.
        /// </summary>
        /// <returns> A new instance of a VisualElement configured to display a mention suggestion.</returns>
        VisualElement MakeSuggestionItem();

        /// <summary>
        /// Binds a mention suggestion data object to a visual element for display in the suggestions list.
        /// </summary>
        /// <param name="item">The visual element created by <see cref="MakeSuggestionItem"/> to bind data to.</param>
        /// <param name="mention">The mention suggestion data object to display.</param>
        void BindItem(VisualElement item, object mention);

        /// <summary>
        /// Formats a selected mention into the markup string to be inserted into the message content.
        /// </summary>
        /// <remarks>
        /// Implementations define the serialization format. For example, a user mention might
        /// produce <c>:user[Jane Doe]{#9876543211011}</c>.
        /// </remarks>
        /// <param name="mention">The mention to format.</param>
        /// <returns>The formatted mention markup string.</returns>
        string Encode(object mention);
    }
}
