using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// A ScriptableObject that holds a flat list of emoji data entries.
    /// </summary>
    /// <remarks>
    /// Unity does not support serialized dictionaries natively, so the data is stored
    /// as an array of <see cref="EmojiData"/> structs keyed by the emoji character.
    /// </remarks>
    [CreateAssetMenu(menuName = "App UI/Emoji Database", fileName = "EmojiDatabase")]
    public class EmojiDatabase : ScriptableObject
    {
        [SerializeField]
        EmojiData[] m_Emojis = Array.Empty<EmojiData>();

        /// <summary>
        /// Returns an enumerable of all emoji entries in the database.
        /// </summary>
        /// <param name="query">An optional search string used to filter emojis by name. When <c>null</c> or empty, all emojis are returned.</param>
        /// <returns>An enumerable of <see cref="EmojiData"/> entries matching the query, or all entries if no query is provided.</returns>
        public IEnumerable<EmojiData> GetEmojis(string query = null)
        {
            return string.IsNullOrEmpty(query) ?
                m_Emojis :
                m_Emojis.Where(e => e.name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the number of emoji entries in the database.
        /// </summary>
        /// <returns>The total number of emoji entries in the database.</returns>
        public int GetEmojiCount()
        {
            return m_Emojis.Length;
        }
    }

    /// <summary>
    /// Data for a single emoji entry.
    /// </summary>
    [Serializable]
    public struct EmojiData
    {
        /// <summary>
        /// The emoji character (e.g. "😀").
        /// </summary>
        public string emoji => m_Emoji;

        /// <summary>
        /// The human-readable name of the emoji (e.g. "grinning face").
        /// </summary>
        public string name => m_Name;

        /// <summary>
        /// The URL-friendly slug for the emoji (e.g. "grinning_face").
        /// </summary>
        public string slug => m_Slug;

        /// <summary>
        /// The emoji group / category (e.g. "Smileys &amp; Emotion").
        /// </summary>
        public string group => m_Group;

        /// <summary>
        /// The emoji specification version in which this emoji was introduced.
        /// </summary>
        public string emojiVersion => m_EmojiVersion;

        /// <summary>
        /// The Unicode standard version in which this character was introduced.
        /// </summary>
        public string unicodeVersion => m_UnicodeVersion;

        /// <summary>
        /// Whether this emoji supports skin-tone modifier variants.
        /// </summary>
        public bool skinToneSupport => m_SkinToneSupport;

        [SerializeField]
        string m_Emoji;

        [SerializeField]
        string m_Name;

        [SerializeField]
        string m_Slug;

        [SerializeField]
        string m_Group;

        [SerializeField]
        string m_EmojiVersion;

        [SerializeField]
        string m_UnicodeVersion;

        [SerializeField]
        bool m_SkinToneSupport;
    }
}
