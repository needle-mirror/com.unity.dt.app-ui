using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Attribute to bind a property or field to a UXML element by its name.
    /// When used in conjunction with <see cref="UxmlFilePathAttribute"/>, the source generator
    /// will automatically query the element from the visual tree and assign it to the decorated member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UxmlElementNameAttribute : Attribute
    {
        /// <summary>
        /// The name of the element in the UXML file to bind to.
        /// </summary>
        public string elementName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elementName"> The name of the element in the UXML file </param>
        public UxmlElementNameAttribute(string elementName)
        {
            this.elementName = elementName;
        }
    }
}
