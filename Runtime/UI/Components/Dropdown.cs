using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Dropdown item UI element.
    /// </summary>
    public class DropdownItem : BaseVisualElement
    {
        /// <summary>
        /// The Dropdown item main styling class.
        /// </summary>
        public const string ussClassName = "appui-dropdown-item";

        /// <summary>
        /// The Dropdown item label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Dropdown item icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Dropdown item checkmark styling class.
        /// </summary>
        public const string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Dropdown item label.
        /// </summary>
        public LocalizedTextElement labelElement { get; }

        readonly Icon m_IconElement;

        readonly Icon m_CheckmarkElement;

        /// <summary>
        /// The Dropdown label text.
        /// </summary>
        public string label
        {
            get => labelElement.text;
            set => labelElement.text = value;
        }

        /// <summary>
        /// The Dropdown item icon.
        /// </summary>
        public string icon
        {
            get => m_IconElement.iconName;
            set
            {
                m_IconElement.iconName = value;
                m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DropdownItem()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_CheckmarkElement = new Icon
            {
                pickingMode = PickingMode.Ignore,
                iconName = MenuItem.checkmarkIconName
            };
            m_CheckmarkElement.AddToClassList(checkmarkUssClassName);
            Add(m_CheckmarkElement);

            m_IconElement = new Icon
            {
                pickingMode = PickingMode.Ignore
            };
            m_IconElement.AddToClassList(iconUssClassName);
            Add(m_IconElement);

            labelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            labelElement.AddToClassList(labelUssClassName);
            Add(labelElement);

            icon = null;
            labelElement.text = null;
        }
    }

    /// <summary>
    /// A form control that lets users select a value from a list of options.
    /// </summary>
    /// <remarks>
    /// The Dropdown component presents a list of options that users can choose from. It appears as a button that,
    /// when clicked, shows a list of selectable options in a popup menu.
    ///
    /// Dropdowns are useful when you need to provide users with a set of predefined options while conserving screen
    /// space. They're commonly used in forms, settings panels, and configuration interfaces.
    ///
    /// The component supports both single and multiple selection modes, making it versatile for different use cases.
    ///
    /// For the best user experience, consider the following guidelines:
    /// - Use Dropdown when you have 3-10 options. For fewer options, consider using Radio Buttons or Toggle
    ///   Buttons. For more options, consider using a searchable ComboBox.
    /// - Order the options in a logical way (e.g., alphabetically, numerically, or by frequency of use)
    /// - Use clear, concise labels for options
    /// </remarks>
    /// <example>
    /// <para>Basic Dropdown: Creating a simple dropdown with string options</para>
    /// <code lang="csharp"><![CDATA[
    /// // Create a basic dropdown with string items
    /// var dropdown = new Dropdown();
    /// var items = new List<string> { "Option 1", "Option 2", "Option 3" };
    /// dropdown.sourceItems = items;
    ///
    /// // Add it to your UI
    /// root.Add(dropdown);
    /// ]]></code>
    /// <para>Multiple Selection Dropdown: Creating a multiple selection dropdown with custom handling of selection changes</para>
    /// <code lang="csharp"><![CDATA[
    /// var dropdown = new Dropdown {
    ///     selectionType = PickerSelectionType.Multiple,
    ///     closeOnSelection = false,
    ///     defaultMessage = "Select options"
    /// };
    ///
    /// var items = new List<string> { "Red", "Green", "Blue" };
    /// dropdown.sourceItems = items;
    ///
    /// // Handle selection changes
    /// dropdown.RegisterValueChangedCallback(evt => {
    ///     var selectedIndices = evt.newValue;
    ///     Debug.Log($"Selected {selectedIndices.Count()} items");
    /// });
    /// ]]></code>
    /// <para>Custom Item Binding: Creating a dropdown with custom item binding and display</para>
    /// <code lang="csharp"><![CDATA[
    /// var dropdown = new Dropdown();
    ///
    /// // Custom class for items
    /// class ColorOption {
    ///     public string Name { get; set; }
    ///     public Color Color { get; set; }
    /// }
    ///
    /// var items = new List<ColorOption> {
    ///     new ColorOption { Name = "Red", Color = Color.red },
    ///     new ColorOption { Name = "Green", Color = Color.green }
    /// };
    ///
    /// dropdown.sourceItems = items;
    /// dropdown.bindItem = (item, index) => {
    ///     var colorOption = items[index] as ColorOption;
    ///     item.label = colorOption.Name;
    /// };
    ///
    /// dropdown.bindTitle = (item, indices) => {
    ///     if (indices.Count() == 0)
    ///         item.label = "Select a color";
    ///     else
    ///         item.label = (items[indices.First()] as ColorOption).Name;
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Dropdown : Picker<DropdownItem, DropdownItem>
    {

        internal static readonly new BindingId bindTitleProperty = new BindingId(nameof(bindTitle));


        /// <summary>
        /// The Dropdown main styling class.
        /// </summary>
        public new const string ussClassName = "appui-dropdown";

        BindTitleFunc m_CustomBindTitle;

        /// <summary>
        /// A method that will be called to bind the title.
        /// </summary>
        [CreateProperty]
        public new BindTitleFunc bindTitle
        {
            get => m_CustomBindTitle;
            set
            {
                m_CustomBindTitle = value;
                base.bindTitle = BindTitle;

                NotifyPropertyChanged(in Picker<DropdownItem,DropdownItem>.bindTitleProperty);
                NotifyPropertyChanged(in bindTitleProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dropdown()
            : this(null) { }

        /// <summary>
        /// Construct a Dropdown UI element with a provided dynamic collection of items.
        /// </summary>
        /// <param name="items">An items collection.</param>
        /// <param name="bindItemFunc">The binding function used to populate display data for each item.</param>
        /// <param name="bindTitleFunc">The binding function used to populate display data for the title.</param>
        /// <param name="defaultIndices">The selected index by default.</param>
        public Dropdown(
            IList items,
            BindItemFunc bindItemFunc = null,
            BindTitleFunc bindTitleFunc = null,
            int[] defaultIndices = null)
            : base(items, null, null, null, null, null, defaultIndices)
        {
            AddToClassList(ussClassName);

            makeItem = MakeItem;
            makeTitle = MakeTitle;
            bindItem = bindItemFunc;
            bindTitle = bindTitleFunc;
        }

        void BindTitle(DropdownItem item, IEnumerable<int> indices)
        {
            if (bindTitle != null)
            {
                bindTitle.Invoke(item, indices);
            }
            else if (bindItem != null && m_Value is {Count: 1})
            {
                bindItem.Invoke(item, m_Value[0]);
            }
            else
            {
                item.icon = null;
                if (m_Value == null || m_Value.Count == 0)
                    item.labelElement.text = defaultMessage;
                else
                    ApplyMultiSelectionMessage(item.labelElement, m_Value.Count);
            }
        }

        /// <summary>
        /// Apply the Picker localized multi selection message.
        /// </summary>
        /// <param name="element"> The <see cref="LocalizedTextElement"/> to apply the message to. </param>
        /// <param name="selectionCount"> The number of selected items. </param>
        protected virtual void ApplyMultiSelectionMessage(LocalizedTextElement element, int selectionCount)
        {
            element.variables = new object[]
            {
                new Dictionary<string, object>
                {
                    { "itemCount", selectionCount }
                }
            };
#if UNITY_LOCALIZATION_PRESENT
            element.text = "@AppUI:selectedItemsMessage";
#else
            element.text = $"{selectionCount} item{(selectionCount > 1 ? "s" : "")} selected";
#endif
        }

        /// <summary>
        /// Default delegate to create a new DropdownItem for the title.
        /// </summary>
        /// <returns> A new DropdownItem. </returns>
        protected static DropdownItem MakeTitle()
        {
            return new DropdownItem();
        }

        /// <summary>
        /// Default delegate to create a new DropdownItem for any item.
        /// </summary>
        /// <returns> A new DropdownItem. </returns>
        protected static DropdownItem MakeItem()
        {
            return new DropdownItem();
        }

    }
}
