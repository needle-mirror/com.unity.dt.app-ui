namespace Unity.AppUI.UI
{
    /// <summary>
    /// Specifies the type of path used for loading UXML files
    /// </summary>
    public enum UxmlFilePathType
    {
        /// <summary>
        /// Use AssetDatabase.LoadAssetAtPath for editor and direct asset paths
        /// </summary>
        AssetDatabase,

        /// <summary>
        /// Use Resources.Load for files in the Resources folder
        /// </summary>
        Resources
    }
}
