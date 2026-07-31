using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Place this attribute on a UI component class to register it as a page in the Visual Documentation browser.
    /// <para/>
    /// The page content is extracted at compile time from the XML documentation of the class and its members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class VisualDocPageAttribute : Attribute
    {
        /// <summary>
        /// The documentation category slug the page belongs to (e.g. "actions", "inputs", "navigation").
        /// </summary>
        public string category { get; }

        /// <summary>
        /// Optional page identifier override. Defaults to the type name in lower case.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Optional display name override. Defaults to the type name.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// Whether the page should carry a "New" badge in the navigation tree.
        /// </summary>
        public bool isNew { get; set; }

        /// <summary>
        /// Place this attribute on a UI component class to register it as a page in the Visual Documentation browser.
        /// </summary>
        /// <param name="category"> The documentation category slug the page belongs to. </param>
        public VisualDocPageAttribute(string category)
        {
            this.category = category;
        }
    }
}
