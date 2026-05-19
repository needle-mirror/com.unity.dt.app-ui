#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using System;
using UnityEngine;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A ScriptableObject that stores a TextMate grammar definition.
    /// The JSON content can be passed directly to the TextMateLib bindings.
    /// </summary>
    [Serializable]
    public class TextMateGrammarAsset : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        string m_JsonContent;

        [SerializeField]
        string m_ScopeName;

        [SerializeField]
        string m_DisplayName;

        /// <summary>
        /// The raw JSON content of the grammar file.
        /// </summary>
        public string jsonContent => m_JsonContent;

        /// <summary>
        /// The scope name of the grammar (e.g., "source.csharp").
        /// This is the unique identifier used to reference the grammar.
        /// </summary>
        public string scopeName => m_ScopeName;

        /// <summary>
        /// The human-readable display name of the grammar (e.g., "C#").
        /// </summary>
        public string displayName => m_DisplayName;

        /// <summary>
        /// Sets the grammar data. Used internally by the importer.
        /// </summary>
        /// <param name="json">The raw JSON content.</param>
        /// <param name="scopeName">The scope name identifier.</param>
        /// <param name="displayName">The human-readable name.</param>
        internal void SetData(string json, string scopeName, string displayName)
        {
            m_JsonContent = json;
            m_ScopeName = scopeName;
            m_DisplayName = displayName;
        }
    }
}
#endif
