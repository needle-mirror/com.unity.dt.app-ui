using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Attribute to specify the path to the UXML file for a VisualElement
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UxmlFilePathAttribute : Attribute
    {
        /// <summary>
        /// The path to the UXML file
        /// </summary>
        public string filePath { get; }

        /// <summary>
        /// The type of path used for loading (AssetDatabase or Resources)
        /// </summary>
        public UxmlFilePathType pathType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Defaults to UxmlFilePathType.Resources
        /// </remarks>
        /// <param name="filePath"> The path to the UXML file </param>
        public UxmlFilePathAttribute(string filePath)
        {
            this.filePath = filePath;
            this.pathType = UxmlFilePathType.Resources;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath"> The path to the UXML file </param>
        /// <param name="pathType"> The type of path used for loading</param>
        public UxmlFilePathAttribute(string filePath, UxmlFilePathType pathType)
        {
            this.filePath = filePath;
            this.pathType = pathType;
        }
    }
}
