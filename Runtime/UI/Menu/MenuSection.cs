using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A labeled section within a menu that groups related menu items.
    /// </summary>
    /// <remarks>
    /// MenuSection is a container element designed to organize menu items into logical groups with optional section
    /// headings. This helps users understand the relationship between different menu items and improves menu
    /// scanability.
    ///
    /// Each section can display a title at the top, followed by any number of menu items. Sections are commonly
    /// used in larger menus to create visual hierarchy and group related functionality.
    ///
    /// Sections have picking mode set to ignore by default, making them non-interactive containers. The title is
    /// automatically hidden when not set or empty.
    ///
    /// Tip: Combine MenuSection with MenuDivider to create clear visual separation between different functional
    /// areas of your menu.
    /// </remarks>
    /// <example>
    /// <para>Basic Menu Section: Organizing menu items into logical sections</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuSection title="Edit">
    ///         <ui:MenuItem label="Undo" icon="undo" shortcut="Ctrl+Z" />
    ///         <ui:MenuItem label="Redo" icon="redo" shortcut="Ctrl+Y" />
    ///     </ui:MenuSection>
    ///     <ui:MenuDivider />
    ///     <ui:MenuSection title="Clipboard">
    ///         <ui:MenuItem label="Cut" icon="scissors" shortcut="Ctrl+X" />
    ///         <ui:MenuItem label="Copy" icon="copy" shortcut="Ctrl+C" />
    ///         <ui:MenuItem label="Paste" icon="clipboard" shortcut="Ctrl+V" />
    ///     </ui:MenuSection>
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Section Without Title: Using sections for structure without always showing titles</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuSection>
    ///         <ui:MenuItem label="New Window" />
    ///         <ui:MenuItem label="New Tab" />
    ///     </ui:MenuSection>
    ///     <ui:MenuDivider />
    ///     <ui:MenuSection title="Tools">
    ///         <ui:MenuItem label="Settings" icon="settings" />
    ///         <ui:MenuItem label="Extensions" icon="puzzle" />
    ///     </ui:MenuSection>
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Creating Sections Programmatically: Building a structured menu with sections in code</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Create first section
    /// var editSection = new MenuSection { title = "Edit" };
    /// editSection.Add(new MenuItem { label = "Undo", shortcut = "Ctrl+Z" });
    /// editSection.Add(new MenuItem { label = "Redo", shortcut = "Ctrl+Y" });
    /// menu.Add(editSection);
    ///
    /// // Add separator
    /// menu.Add(new MenuDivider());
    ///
    /// // Create second section
    /// var viewSection = new MenuSection { title = "View" };
    /// viewSection.Add(new MenuItem { label = "Zoom In", shortcut = "Ctrl++" });
    /// viewSection.Add(new MenuItem { label = "Zoom Out", shortcut = "Ctrl+-" });
    /// viewSection.Add(new MenuItem { label = "Full Screen", shortcut = "F11" });
    /// menu.Add(viewSection);
    /// ]]></code>
    /// <para>Complex Menu Structure: Building a menu with dynamic content and multiple section types</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Recent files section
    /// var recentSection = new MenuSection { title = "Recent Files" };
    /// for (int i = 0; i < 5; i++)
    /// {
    ///     recentSection.Add(new MenuItem {
    ///         label = $"Document_{i}.txt",
    ///         icon = "file"
    ///     });
    /// }
    /// menu.Add(recentSection);
    ///
    /// menu.Add(new MenuDivider());
    ///
    /// // Settings section with toggles
    /// var settingsSection = new MenuSection { title = "Settings" };
    /// settingsSection.Add(new MenuItem {
    ///     label = "Auto-Save",
    ///     selectable = true,
    ///     value = true
    /// });
    /// settingsSection.Add(new MenuItem {
    ///     label = "Show Tooltips",
    ///     selectable = true,
    ///     value = true
    /// });
    /// menu.Add(settingsSection);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups", id = "menu-section", displayName = "Menu Section")]
    public partial class MenuSection : BaseVisualElement
    {

        internal static readonly BindingId titleProperty = new BindingId(nameof(title));


        /// <summary>
        /// The MenuSection main styling class.
        /// </summary>
        public const string ussClassName = "appui-menusection";

        /// <summary>
        /// The MenuSection title styling class.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The MenuSection container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        readonly VisualElement m_Container;

        readonly LocalizedTextElement m_Title;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuSection()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);

            hierarchy.Add(m_Title);
            hierarchy.Add(m_Container);

            title = null;
        }

        /// <summary>
        /// The MenuSection container.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The text to display in the section heading.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title.text;
            set
            {
                var changed = m_Title.text != value;
                m_Title.text = value;
                m_Title.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

    }
}
