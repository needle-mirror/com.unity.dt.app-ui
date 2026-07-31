using System;
using System.Collections.Generic;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A documentation page for a UI component, extracted at compile time from its XML documentation.
    /// </summary>
    class VisualDocPageInfo
    {
        /// <summary>
        /// The unique page identifier (slug).
        /// </summary>
        public string id { get; }

        /// <summary>
        /// The full name of the documented type.
        /// </summary>
        public string typeFullName { get; }

        /// <summary>
        /// The page display name.
        /// </summary>
        public string displayName { get; }

        /// <summary>
        /// The documentation category slug.
        /// </summary>
        public string category { get; }

        /// <summary>
        /// Whether the page should carry a "New" badge.
        /// </summary>
        public bool isNew { get; }

        /// <summary>
        /// The one-sentence component description (page subtitle).
        /// </summary>
        public string summary { get; }

        /// <summary>
        /// Whether the component can be instantiated from UXML.
        /// </summary>
        public bool uxmlSupported { get; }

        /// <summary>
        /// The ordered page sections (Overview first).
        /// </summary>
        public IReadOnlyList<VisualDocSection> sections { get; }

        /// <summary>
        /// The usage examples.
        /// </summary>
        public IReadOnlyList<VisualDocExample> examples { get; }

        /// <summary>
        /// The documented properties and events declared by the type.
        /// </summary>
        public IReadOnlyList<VisualDocProperty> properties { get; }

        /// <summary>
        /// Creates a new page description.
        /// </summary>
        /// <param name="id"> The unique page identifier (slug). </param>
        /// <param name="typeFullName"> The full name of the documented type. </param>
        /// <param name="displayName"> The page display name. </param>
        /// <param name="category"> The documentation category slug. </param>
        /// <param name="isNew"> Whether the page should carry a "New" badge. </param>
        /// <param name="summary"> The one-sentence component description. </param>
        /// <param name="uxmlSupported"> Whether the component can be instantiated from UXML. </param>
        /// <param name="sections"> The ordered page sections. </param>
        /// <param name="examples"> The usage examples. </param>
        /// <param name="properties"> The documented properties and events. </param>
        public VisualDocPageInfo(
            string id,
            string typeFullName,
            string displayName,
            string category,
            bool isNew,
            string summary,
            bool uxmlSupported,
            IReadOnlyList<VisualDocSection> sections,
            IReadOnlyList<VisualDocExample> examples,
            IReadOnlyList<VisualDocProperty> properties)
        {
            this.id = id;
            this.typeFullName = typeFullName;
            this.displayName = displayName;
            this.category = category;
            this.isNew = isNew;
            this.summary = summary;
            this.uxmlSupported = uxmlSupported;
            this.sections = sections ?? Array.Empty<VisualDocSection>();
            this.examples = examples ?? Array.Empty<VisualDocExample>();
            this.properties = properties ?? Array.Empty<VisualDocProperty>();
        }
    }

    /// <summary>
    /// A titled markdown section of a documentation page.
    /// </summary>
    class VisualDocSection
    {
        /// <summary>
        /// The section title (Table of Contents entry).
        /// </summary>
        public string title { get; }

        /// <summary>
        /// The section content, as markdown.
        /// </summary>
        public string markdown { get; }

        /// <summary>
        /// Creates a new section.
        /// </summary>
        /// <param name="title"> The section title. </param>
        /// <param name="markdown"> The section content, as markdown. </param>
        public VisualDocSection(string title, string markdown)
        {
            this.title = title;
            this.markdown = markdown;
        }
    }

    /// <summary>
    /// A code example attached to a documentation page or property.
    /// </summary>
    class VisualDocExample
    {
        /// <summary>
        /// The scenario description displayed above the code.
        /// </summary>
        public string description { get; }

        /// <summary>
        /// The example source code.
        /// </summary>
        public string code { get; }

        /// <summary>
        /// The language of the source code (e.g. "csharp", "xml").
        /// </summary>
        public string language { get; }

        /// <summary>
        /// Creates a new example.
        /// </summary>
        /// <param name="description"> The scenario description displayed above the code. </param>
        /// <param name="code"> The example source code. </param>
        /// <param name="language"> The language of the source code. </param>
        public VisualDocExample(string description, string code, string language)
        {
            this.description = description;
            this.code = code;
            this.language = language;
        }
    }

    /// <summary>
    /// A documented property or event of a UI component.
    /// </summary>
    class VisualDocProperty
    {
        /// <summary>
        /// The member name.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// The member type, as displayed in the API table.
        /// </summary>
        public string type { get; }

        /// <summary>
        /// The default value, as displayed in the API table (null when unknown).
        /// </summary>
        public string defaultValue { get; }

        /// <summary>
        /// The member description.
        /// </summary>
        public string summary { get; }

        /// <summary>
        /// Extended notes, as markdown (null when absent).
        /// </summary>
        public string remarksMarkdown { get; }

        /// <summary>
        /// The examples attached to the member.
        /// </summary>
        public IReadOnlyList<VisualDocExample> examples { get; }

        /// <summary>
        /// Whether the member is settable from UXML.
        /// </summary>
        public bool isUxmlAttribute { get; }

        /// <summary>
        /// Whether the member is an event.
        /// </summary>
        public bool isEvent { get; }

        /// <summary>
        /// Creates a new property description.
        /// </summary>
        /// <param name="name"> The member name. </param>
        /// <param name="type"> The member type. </param>
        /// <param name="defaultValue"> The default value (null when unknown). </param>
        /// <param name="summary"> The member description. </param>
        /// <param name="remarksMarkdown"> Extended notes, as markdown. </param>
        /// <param name="examples"> The examples attached to the member. </param>
        /// <param name="isUxmlAttribute"> Whether the member is settable from UXML. </param>
        /// <param name="isEvent"> Whether the member is an event. </param>
        public VisualDocProperty(
            string name,
            string type,
            string defaultValue,
            string summary,
            string remarksMarkdown,
            IReadOnlyList<VisualDocExample> examples,
            bool isUxmlAttribute,
            bool isEvent)
        {
            this.name = name;
            this.type = type;
            this.defaultValue = defaultValue;
            this.summary = summary;
            this.remarksMarkdown = remarksMarkdown;
            this.examples = examples ?? Array.Empty<VisualDocExample>();
            this.isUxmlAttribute = isUxmlAttribute;
            this.isEvent = isEvent;
        }
    }
}
