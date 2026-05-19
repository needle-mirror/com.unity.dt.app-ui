#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A ScriptableObject that stores a TextMate theme definition.
    /// The JSON content can be passed directly to the TextMateLib bindings.
    /// </summary>
    [Serializable]
    public class TextMateThemeAsset : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        string m_JsonContent;

        [SerializeField]
        string m_DisplayName;

        /// <summary>
        /// The raw JSON content of the theme file.
        /// </summary>
        public string jsonContent => m_JsonContent;

        /// <summary>
        /// The human-readable display name of the theme (e.g., "Dark Plus").
        /// </summary>
        public string displayName => m_DisplayName;

        /// <summary>
        /// Sets the theme data. Used internally by the importer.
        /// </summary>
        /// <param name="json">The raw JSON content.</param>
        /// <param name="displayName">The human-readable name.</param>
        internal void SetData(string json, string displayName)
        {
            m_JsonContent = json;
            m_DisplayName = displayName;
        }
    }
}
#endif
