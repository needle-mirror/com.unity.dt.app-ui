using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A search input field with a magnifying glass icon that allows users to enter search queries.
    /// </summary>
    /// <remarks>
    /// The SearchBar component is a specialized TextField that provides a dedicated interface for search
    /// functionality. It comes with a built-in magnifying glass icon and placeholder text, making it instantly
    /// recognizable as a search input.
    ///
    /// By default, the SearchBar displays a magnifying glass icon on the left side and includes a 'Search...'
    /// placeholder text, providing clear affordance for its purpose.
    ///
    /// The SearchBar inherits from TextField, which means it includes all TextField functionality including
    /// validation, size variants, and read-only states.
    ///
    /// SearchBars are commonly used in:
    ///
    /// - Navigation menus to filter items
    /// - Data tables to search through records
    /// - Document libraries to find specific files
    /// - Any interface where users need to search through content
    /// </remarks>
    /// <example>
    /// <para>Basic Usage</para>
    /// <code lang="csharp"><![CDATA[
    /// // Creating a basic search bar
    /// var searchBar = new SearchBar();
    /// searchBar.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Search query: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>Search with Validation</para>
    /// <code lang="csharp"><![CDATA[
    /// var searchBar = new SearchBar();
    /// // Add validation to ensure minimum search length
    /// searchBar.validateValue = (value) => value.Length >= 3;
    /// searchBar.placeholder = "Enter at least 3 characters...";
    /// ]]></code>
    /// <para>UXML Usage</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:SearchBar size="M"
    ///                      placeholder="Search documents..."
    ///                      leading-icon-name="magnifying-glass" />
    /// </ui:UXML>
    /// ]]></code>
    /// <para>Complete Search Implementation</para>
    /// <code lang="csharp"><![CDATA[
    /// public class SearchableList : VisualElement
    /// {
    ///     private SearchBar searchBar;
    ///     private ListView listView;
    ///     private List<string> allItems;
    ///
    ///     public SearchableList()
    ///     {
    ///         // Create and configure search bar
    ///         searchBar = new SearchBar();
    ///         searchBar.placeholder = "Search items...";
    ///         searchBar.size = Size.M;
    ///
    ///         // Handle search input
    ///         searchBar.RegisterValueChangedCallback(OnSearchQueryChanged);
    ///
    ///         // Add to visual hierarchy
    ///         Add(searchBar);
    ///
    ///         // Initialize list view...
    ///     }
    ///
    ///     private void OnSearchQueryChanged(ChangeEvent<string> evt)
    ///     {
    ///         var query = evt.newValue.ToLower();
    ///         var filteredItems = allItems
    ///             .Where(item => item.ToLower().Contains(query))
    ///             .ToList();
    ///         listView.itemsSource = filteredItems;
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class SearchBar : TextField
    {
        /// <summary>
        /// The SearchBar main styling class.
        /// </summary>
        public new const string ussClassName = "appui-searchbar";

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SearchBar()
        {
            AddToClassList(ussClassName);
            leadingIconName = "magnifying-glass";
            placeholder = "Search...";
        }

        [UxmlAttribute("leading-icon-name")]
        [HideInInspector]
        string leadingIconNameOverride
        {
            get => leadingIconName;
            set => leadingIconName = value;
        }

    }
}
